namespace CongCraft.Engine.Rendering;

/// <summary>
/// All GLSL shaders embedded as string constants. No external files needed.
/// Upgraded for mid-2000s RPG quality: triplanar texturing, specular, shadows,
/// atmospheric fog, Fresnel water, procedural clouds.
/// </summary>
public static class ShaderSources
{
    // ─── Shadow map generation ─────────────────────────────────────────
    public const string ShadowVertex = @"
#version 330 core
layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec3 aNormal;
layout (location = 2) in vec2 aTexCoord;

uniform mat4 uLightSpaceMatrix;
uniform mat4 uModel;

void main()
{
    gl_Position = uLightSpaceMatrix * uModel * vec4(aPosition, 1.0);
}
";

    public const string ShadowFragment = @"
#version 330 core
void main() { }
";

    // Shadow vertex for entities with color attribute (different layout)
    public const string ShadowVertexEntity = @"
#version 330 core
layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec3 aNormal;
layout (location = 2) in vec3 aColor;

uniform mat4 uLightSpaceMatrix;
uniform mat4 uModel;

void main()
{
    gl_Position = uLightSpaceMatrix * uModel * vec4(aPosition, 1.0);
}
";

    // ─── Terrain (triplanar + multi-texture + shadow + specular) ────────
    public const string TerrainVertex = @"
#version 330 core
layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec3 aNormal;
layout (location = 2) in vec2 aTexCoord;

uniform mat4 uModel;
uniform mat4 uView;
uniform mat4 uProjection;
uniform mat4 uLightSpaceMatrix;

out vec3 FragPos;
out vec3 Normal;
out vec2 TexCoord;
out float Height;
out vec4 FragPosLightSpace;

void main()
{
    vec4 worldPos = uModel * vec4(aPosition, 1.0);
    FragPos = worldPos.xyz;
    Normal = mat3(transpose(inverse(uModel))) * aNormal;
    TexCoord = aTexCoord;
    Height = aPosition.y;
    FragPosLightSpace = uLightSpaceMatrix * worldPos;
    gl_Position = uProjection * uView * worldPos;
}
";

    public const string TerrainFragment = @"
#version 330 core
in vec3 FragPos;
in vec3 Normal;
in vec2 TexCoord;
in float Height;
in vec4 FragPosLightSpace;

uniform vec3 uSunDirection;
uniform vec3 uSunColor;
uniform vec3 uAmbientColor;
uniform float uSunIntensity;
uniform float uFogDensity;
uniform vec3 uFogColor;
uniform vec3 uCameraPos;
uniform sampler2D uGrassTex;
uniform sampler2D uStoneTex;
uniform sampler2D uDirtTex;
uniform sampler2D uSnowTex;
uniform sampler2D uPathTex;
uniform sampler2D uShadowMap;

out vec4 FragColor;

vec3 triplanar(sampler2D tex, vec3 pos, vec3 bl, float scale)
{
    return texture(tex, pos.yz * scale).rgb * bl.x
         + texture(tex, pos.xz * scale).rgb * bl.y
         + texture(tex, pos.xy * scale).rgb * bl.z;
}

float ShadowCalculation(vec4 fragPosLightSpace, vec3 normal, vec3 lightDir)
{
    vec3 projCoords = fragPosLightSpace.xyz / fragPosLightSpace.w;
    projCoords = projCoords * 0.5 + 0.5;
    if (projCoords.z > 1.0 || projCoords.x < 0.0 || projCoords.x > 1.0 ||
        projCoords.y < 0.0 || projCoords.y > 1.0)
        return 0.0;

    float currentDepth = projCoords.z;
    float bias = max(0.005 * (1.0 - dot(normal, lightDir)), 0.002);

    // PCF soft shadows (3x3 kernel)
    float shadow = 0.0;
    vec2 texelSize = 1.0 / textureSize(uShadowMap, 0);
    for (int x = -1; x <= 1; ++x)
    {
        for (int y = -1; y <= 1; ++y)
        {
            float pcfDepth = texture(uShadowMap, projCoords.xy + vec2(x, y) * texelSize).r;
            shadow += currentDepth - bias > pcfDepth ? 1.0 : 0.0;
        }
    }
    shadow /= 9.0;
    return shadow;
}

void main()
{
    vec3 norm = normalize(Normal);
    float slope = 1.0 - abs(dot(norm, vec3(0.0, 1.0, 0.0)));

    // Triplanar blending weights
    vec3 blending = abs(norm);
    blending = normalize(max(blending, 0.00001));
    float btotal = blending.x + blending.y + blending.z;
    blending /= btotal;

    // Sample all terrain textures via triplanar
    float texScale = 0.15;
    vec3 grassSample = triplanar(uGrassTex, FragPos, blending, texScale);
    vec3 stoneSample = triplanar(uStoneTex, FragPos, blending, texScale * 1.3);
    vec3 dirtSample = triplanar(uDirtTex, FragPos, blending, texScale * 0.9);
    vec3 snowSample = triplanar(uSnowTex, FragPos, blending, texScale * 1.1);
    vec3 pathSample = triplanar(uPathTex, FragPos, blending, texScale * 0.85);

    // Multi-scale texture blending (adds close-up detail)
    float detailScale = texScale * 4.0;
    vec3 grassDetail = triplanar(uGrassTex, FragPos, blending, detailScale);
    vec3 stoneDetail = triplanar(uStoneTex, FragPos, blending, detailScale * 1.3);

    // Blend detail textures at close range
    float camDist = length(FragPos.xz - uCameraPos.xz);
    float detailBlend = smoothstep(0.0, 35.0, camDist);
    grassSample = mix(grassSample * (0.6 + grassDetail.g * 0.8), grassSample, detailBlend);
    stoneSample = mix(stoneSample * (0.6 + stoneDetail.r * 0.8), stoneSample, detailBlend);

    // Height-based blending with smooth transitions (5 layers)
    // Terrain amplitude is 20 so heights range from ~-20 to ~+20
    vec3 baseColor;
    if (Height < -8.0)
        baseColor = mix(pathSample, dirtSample, smoothstep(-14.0, -8.0, Height));
    else if (Height < -2.0)
        baseColor = mix(dirtSample, grassSample, smoothstep(-8.0, -2.0, Height));
    else if (Height < 10.0)
        baseColor = grassSample;
    else if (Height < 16.0)
        baseColor = mix(grassSample, stoneSample, smoothstep(10.0, 16.0, Height));
    else
        baseColor = mix(stoneSample, snowSample, smoothstep(16.0, 22.0, Height));

    // Steep slopes get stone texture
    baseColor = mix(baseColor, stoneSample, smoothstep(0.35, 0.65, slope));

    // Snow accumulation on flat surfaces at high altitude
    float flatness = max(dot(norm, vec3(0.0, 1.0, 0.0)), 0.0);
    float snowAccum = smoothstep(16.0, 20.0, Height) * smoothstep(0.2, 0.8, flatness);
    baseColor = mix(baseColor, snowSample, snowAccum);

    // Path blending near low flat areas (natural paths in valleys)
    float pathWeight = smoothstep(-4.0, -8.0, Height) * smoothstep(0.1, 0.0, slope);
    baseColor = mix(baseColor, pathSample, pathWeight * 0.6);

    // Half-Lambert diffuse for softer terrain shading (Gothic 3 style)
    vec3 lightDir = normalize(-uSunDirection);
    float NdotL = dot(norm, lightDir);
    float diff = NdotL * 0.5 + 0.5;
    diff = diff * diff;

    // Specular for wet stone/snow (higher on steep/high areas)
    vec3 viewDir = normalize(uCameraPos - FragPos);
    vec3 halfDir = normalize(lightDir + viewDir);
    float specRoughness = mix(64.0, 16.0, slope);
    float spec = pow(max(dot(norm, halfDir), 0.0), specRoughness);
    float specIntensity = mix(0.08, 0.3, smoothstep(10.0, 22.0, Height)); // Snow/ice = shinier
    specIntensity += snowAccum * 0.15; // Extra glint on snow
    vec3 specular = spec * uSunColor * specIntensity * uSunIntensity;

    // Hemisphere ambient (sky above, ground below)
    float hemiBlend = dot(norm, vec3(0.0, 1.0, 0.0)) * 0.5 + 0.5;
    vec3 ambient = mix(uAmbientColor * 0.45, uAmbientColor * 1.15, hemiBlend);

    // Fake ambient occlusion from terrain crevices
    float terrainAO = smoothstep(-0.2, 0.5, norm.y) * 0.25 + 0.75;
    ambient *= terrainAO;

    // Shadow
    float shadow = ShadowCalculation(FragPosLightSpace, norm, lightDir);
    vec3 diffuse = diff * uSunColor * uSunIntensity * (1.0 - shadow * 0.7);
    vec3 lighting = (ambient + diffuse) * baseColor + specular * (1.0 - shadow);

    // Atmospheric fog (height-adjusted, denser in valleys)
    float dist = length(FragPos - uCameraPos);
    float valleyFog = max(0.0, 1.0 - FragPos.y * 0.08);
    float heightFactor = max(0.0, 1.0 - (FragPos.y - uCameraPos.y) * 0.02);
    float fog = 1.0 - exp(-dist * uFogDensity * (1.0 + heightFactor * 0.5 + valleyFog * 0.3));
    lighting = mix(lighting, uFogColor, clamp(fog, 0.0, 1.0));

    FragColor = vec4(lighting, 1.0);
}
";

    // ─── Basic object shader (enemies, player, NPCs) with shadows ─────
    public const string BasicVertex = @"
#version 330 core
layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec3 aNormal;
layout (location = 2) in vec3 aColor;

uniform mat4 uModel;
uniform mat4 uView;
uniform mat4 uProjection;
uniform mat4 uLightSpaceMatrix;

out vec3 FragPos;
out vec3 LocalPos;
out vec3 Normal;
out vec3 VertexColor;
out vec4 FragPosLightSpace;

void main()
{
    vec4 worldPos = uModel * vec4(aPosition, 1.0);
    FragPos = worldPos.xyz;
    LocalPos = aPosition;
    Normal = mat3(transpose(inverse(uModel))) * aNormal;
    VertexColor = aColor;
    FragPosLightSpace = uLightSpaceMatrix * worldPos;
    gl_Position = uProjection * uView * worldPos;
}
";

    public const string BasicFragment = @"
#version 330 core
in vec3 FragPos;
in vec3 LocalPos;
in vec3 Normal;
in vec3 VertexColor;
in vec4 FragPosLightSpace;

uniform vec3 uSunDirection;
uniform vec3 uSunColor;
uniform vec3 uAmbientColor;
uniform float uSunIntensity;
uniform float uFogDensity;
uniform vec3 uFogColor;
uniform vec3 uCameraPos;
uniform sampler2D uShadowMap;
uniform sampler2D uMetalTex;
uniform sampler2D uLeatherTex;
uniform sampler2D uSkinTex;
uniform sampler2D uWoodTex;
uniform sampler2D uFabricTex;

out vec4 FragColor;

vec3 triplanarSample(sampler2D tex, vec3 pos, vec3 n, float scale)
{
    vec3 bl = abs(n);
    bl = normalize(max(bl, 0.00001));
    float btotal = bl.x + bl.y + bl.z;
    bl /= btotal;
    return texture(tex, pos.yz * scale).rgb * bl.x
         + texture(tex, pos.xz * scale).rgb * bl.y
         + texture(tex, pos.xy * scale).rgb * bl.z;
}

float ShadowCalculation(vec4 fragPosLightSpace, vec3 normal, vec3 lightDir)
{
    vec3 projCoords = fragPosLightSpace.xyz / fragPosLightSpace.w;
    projCoords = projCoords * 0.5 + 0.5;
    if (projCoords.z > 1.0 || projCoords.x < 0.0 || projCoords.x > 1.0 ||
        projCoords.y < 0.0 || projCoords.y > 1.0)
        return 0.0;

    float currentDepth = projCoords.z;
    float bias = max(0.005 * (1.0 - dot(normal, lightDir)), 0.002);

    float shadow = 0.0;
    vec2 texelSize = 1.0 / textureSize(uShadowMap, 0);
    for (int x = -1; x <= 1; ++x)
    {
        for (int y = -1; y <= 1; ++y)
        {
            float pcfDepth = texture(uShadowMap, projCoords.xy + vec2(x, y) * texelSize).r;
            shadow += currentDepth - bias > pcfDepth ? 1.0 : 0.0;
        }
    }
    shadow /= 9.0;
    return shadow;
}

void main()
{
    vec3 norm = normalize(Normal);
    vec3 lightDir = normalize(-uSunDirection);
    vec3 viewDir = normalize(uCameraPos - FragPos);

    // Material properties from vertex color brightness
    float luminance = dot(VertexColor, vec3(0.299, 0.587, 0.114));
    float metallic = smoothstep(0.32, 0.52, luminance) * 0.7;
    float roughness = mix(0.20, 0.90, 1.0 - metallic);

    // --- Material texture sampling via triplanar in object space ---
    float texScale = 2.5;
    vec3 localNorm = normalize(Normal);
    vec3 metalSample = triplanarSample(uMetalTex, LocalPos, localNorm, texScale);
    vec3 leatherSample = triplanarSample(uLeatherTex, LocalPos, localNorm, texScale * 1.1);
    vec3 skinSample = triplanarSample(uSkinTex, LocalPos, localNorm, texScale * 1.3);
    vec3 woodSample = triplanarSample(uWoodTex, LocalPos, localNorm, texScale * 0.7);
    vec3 fabricSample = triplanarSample(uFabricTex, LocalPos, localNorm, texScale * 1.4);

    // Multi-scale detail layers for close-up richness (Two Worlds style micro-detail)
    float detailScale = texScale * 3.5;
    float microScale = texScale * 7.0;
    vec3 metalDetail = triplanarSample(uMetalTex, LocalPos, localNorm, detailScale);
    vec3 leatherDetail = triplanarSample(uLeatherTex, LocalPos, localNorm, detailScale);
    vec3 skinDetail = triplanarSample(uSkinTex, LocalPos, localNorm, detailScale);
    vec3 woodDetail = triplanarSample(uWoodTex, LocalPos, localNorm, detailScale * 0.8);
    vec3 microNoise = triplanarSample(uFabricTex, LocalPos, localNorm, microScale);
    float camDist = length(FragPos - uCameraPos);
    float detailMix = smoothstep(2.0, 25.0, camDist);
    metalSample = mix(metalSample * (0.50 + metalDetail.r * 1.0), metalSample, detailMix);
    leatherSample = mix(leatherSample * (0.50 + leatherDetail.r * 1.0), leatherSample, detailMix);
    skinSample = mix(skinSample * (0.55 + skinDetail.r * 0.9), skinSample, detailMix);
    woodSample = mix(woodSample * (0.50 + woodDetail.r * 1.0), woodSample, detailMix);

    // Classify material from vertex color hue/brightness
    float warmth = VertexColor.r - VertexColor.b;
    float greenness = VertexColor.g - (VertexColor.r + VertexColor.b) * 0.5;
    float coldness = VertexColor.b - VertexColor.r;

    // Material classification with separation
    float metalWeight = smoothstep(0.36, 0.55, luminance) * smoothstep(-0.02, 0.08, -warmth) * 1.2;
    float skinWeight = smoothstep(0.12, 0.28, warmth) * smoothstep(0.40, 0.55, luminance);
    float woodWeight = max(smoothstep(0.04, 0.20, greenness),
                           smoothstep(0.10, 0.30, warmth) * step(luminance, 0.28));
    float leatherWeight = smoothstep(0.05, 0.22, warmth) * step(luminance, 0.38) * (1.0 - skinWeight * 0.8);
    float fabricWeight = max(0.0, 1.0 - metalWeight - skinWeight - woodWeight - leatherWeight);

    // Gold/ornate detection
    float goldWeight = smoothstep(0.35, 0.55, warmth) * smoothstep(0.45, 0.60, luminance);
    metalWeight += goldWeight * 0.6;
    skinWeight *= (1.0 - goldWeight);

    // Normalize
    float totalWeight = max(metalWeight + leatherWeight + skinWeight + woodWeight + fabricWeight, 0.001);
    metalWeight /= totalWeight;
    leatherWeight /= totalWeight;
    skinWeight /= totalWeight;
    woodWeight /= totalWeight;
    fabricWeight /= totalWeight;

    // Blend material textures
    vec3 materialTex = metalSample * metalWeight + leatherSample * leatherWeight
                     + skinSample * skinWeight + woodSample * woodWeight
                     + fabricSample * fabricWeight;

    // Texture detail with micro-noise for surface grain (Two Worlds style)
    vec3 texDetail = materialTex / vec3(0.5);
    float microMix = smoothstep(1.0, 15.0, camDist);
    float microGrain = mix(microNoise.r * 0.15 + 0.925, 1.0, microMix);
    texDetail = clamp(texDetail * microGrain, 0.45, 1.60);

    vec3 texturedColor = VertexColor * texDetail;

    // Distance-based fade
    float textureFade = smoothstep(3.0, 55.0, camDist);
    vec3 baseColor = mix(texturedColor, VertexColor, textureFade);

    // Two Worlds style: warm color grading shift
    baseColor *= vec3(1.02, 1.00, 0.96);

    // Diffuse with wrapped lighting for softer, more atmospheric look
    float NdotL = dot(norm, lightDir);
    float diff = NdotL * 0.5 + 0.5;
    diff = diff * diff;
    // Soften shadows on skin surfaces
    diff = mix(diff, diff * 0.7 + 0.3, skinWeight);

    // Blinn-Phong specular with material-dependent shininess
    vec3 halfDir = normalize(lightDir + viewDir);
    float shininess = mix(12.0, 160.0, 1.0 - roughness);
    float spec = pow(max(dot(norm, halfDir), 0.0), shininess);
    float specIntensity = mix(0.10, 0.60, metallic);
    // Gold gets warmer specular highlights
    vec3 specColor = mix(vec3(1.0), baseColor, metallic);
    specColor = mix(specColor, vec3(1.0, 0.85, 0.5), goldWeight * 0.4);
    vec3 specular = spec * specColor * specIntensity * uSunIntensity;

    // Hemisphere ambient with fake AO (Two Worlds had deep ambient in crevices)
    float hemiBlend = dot(norm, vec3(0.0, 1.0, 0.0)) * 0.5 + 0.5;
    vec3 ambient = mix(uAmbientColor * 0.40, uAmbientColor * 1.3, hemiBlend);
    // Cavity AO — deeper darks in crevices for more sculpted look
    float cavityAO = smoothstep(-0.4, 0.5, norm.y) * 0.25 + 0.75;
    // Micro-AO from texture detail for surface depth
    float microAO = mix(microGrain, 1.0, 0.5);
    ambient *= cavityAO * microAO;

    // Bounce light — warm fill from below (ground reflection, Two Worlds style)
    vec3 bounceLight = vec3(0.08, 0.06, 0.03) * max(0.0, -norm.y) * uSunIntensity;

    // Shadow with deeper darkening
    float shadow = ShadowCalculation(FragPosLightSpace, norm, lightDir);
    vec3 diffuse = diff * uSunColor * uSunIntensity * (1.0 - shadow * 0.75);
    vec3 lighting = (ambient + diffuse + bounceLight) * baseColor + specular * (1.0 - shadow);

    // Rim lighting (atmospheric backlight, Two Worlds golden rim)
    float rim = 1.0 - max(dot(viewDir, norm), 0.0);
    rim = smoothstep(0.35, 1.0, rim);
    vec3 rimColor = mix(uSunColor * 0.10, baseColor * 0.15, 1.0 - metallic);
    rimColor = mix(rimColor, vec3(0.9, 0.7, 0.3) * 0.12, goldWeight);
    lighting += rim * rimColor * uSunIntensity;

    // Fresnel edge highlighting (stronger on metallic)
    float fresnel = pow(1.0 - max(dot(viewDir, norm), 0.0), 3.0);
    lighting += fresnel * uSunColor * 0.07 * metallic * uSunIntensity;

    // Fog with height-based density and warm atmospheric scattering
    float fogDist = length(FragPos - uCameraPos);
    float heightFog = max(0.0, 1.0 - (FragPos.y - uCameraPos.y) * 0.012);
    float fog = 1.0 - exp(-fogDist * uFogDensity * (1.0 + heightFog * 0.5));
    // Slightly warm the fog color for atmospheric scatter
    vec3 warmFog = uFogColor + vec3(0.02, 0.01, -0.01);
    lighting = mix(lighting, warmFog, clamp(fog, 0.0, 1.0));

    FragColor = vec4(lighting, 1.0);
}
";

    // ─── Water (Fresnel, dynamic normals, fake reflection) ─────────────
    public const string WaterVertex = @"
#version 330 core
layout (location = 0) in vec3 aPosition;

uniform mat4 uModel;
uniform mat4 uView;
uniform mat4 uProjection;
uniform float uTime;

out vec3 FragPos;
out vec3 WaveNormal;

void main()
{
    vec3 pos = aPosition;

    // Multi-octave waves
    float wave1 = sin(pos.x * 0.4 + uTime * 1.2) * 0.12;
    float wave2 = cos(pos.z * 0.3 + uTime * 0.8) * 0.09;
    float wave3 = sin((pos.x + pos.z) * 0.2 + uTime * 0.5) * 0.06;
    float wave4 = cos(pos.x * 0.8 - uTime * 1.5) * 0.04;
    pos.y += wave1 + wave2 + wave3 + wave4;

    // Wave normal from partial derivatives
    float dx = 0.4 * 0.12 * cos(pos.x * 0.4 + uTime * 1.2)
             + 0.2 * 0.06 * cos((pos.x + pos.z) * 0.2 + uTime * 0.5)
             + 0.8 * 0.04 * (-sin(pos.x * 0.8 - uTime * 1.5));
    float dz = 0.3 * 0.09 * (-sin(pos.z * 0.3 + uTime * 0.8))
             + 0.2 * 0.06 * cos((pos.x + pos.z) * 0.2 + uTime * 0.5);
    WaveNormal = normalize(vec3(-dx, 1.0, -dz));

    vec4 worldPos = uModel * vec4(pos, 1.0);
    FragPos = worldPos.xyz;
    gl_Position = uProjection * uView * worldPos;
}
";

    public const string WaterFragment = @"
#version 330 core
in vec3 FragPos;
in vec3 WaveNormal;

uniform vec3 uSunDirection;
uniform vec3 uSunColor;
uniform float uSunIntensity;
uniform vec3 uAmbientColor;
uniform float uFogDensity;
uniform vec3 uFogColor;
uniform vec3 uCameraPos;
uniform float uTime;
uniform vec3 uZenithColor;
uniform vec3 uHorizonColor;

out vec4 FragColor;

void main()
{
    vec3 deepColor = vec3(0.02, 0.08, 0.18);
    vec3 shallowColor = vec3(0.05, 0.25, 0.35);
    vec3 norm = normalize(WaveNormal);
    vec3 viewDir = normalize(uCameraPos - FragPos);

    // Fresnel
    float fresnel = pow(1.0 - max(dot(viewDir, norm), 0.0), 4.0);
    fresnel = mix(0.04, 0.9, fresnel);

    // Fake sky reflection
    vec3 reflectDir = reflect(-viewDir, norm);
    float skyT = clamp(reflectDir.y * 0.5 + 0.5, 0.0, 1.0);
    vec3 reflectionColor = mix(uHorizonColor * 1.2, uZenithColor, pow(skyT, 1.5));

    // Sun specular
    vec3 lightDir = normalize(-uSunDirection);
    vec3 halfDir = normalize(lightDir + viewDir);
    float spec = pow(max(dot(norm, halfDir), 0.0), 256.0);
    vec3 sunSpec = spec * uSunColor * 2.0 * uSunIntensity;
    float spec2 = pow(max(dot(norm, halfDir), 0.0), 32.0);
    vec3 broadSpec = spec2 * uSunColor * 0.3 * uSunIntensity;

    // Water color
    vec3 waterColor = mix(shallowColor, deepColor, clamp(fresnel, 0.0, 1.0));

    // Diffuse
    float diff = max(dot(norm, lightDir), 0.0);
    vec3 lighting = (uAmbientColor * 0.6 + diff * uSunColor * uSunIntensity * 0.4) * waterColor;

    // Blend with reflection
    vec3 finalColor = mix(lighting, reflectionColor, fresnel * 0.6) + sunSpec + broadSpec;

    // Caustic shimmer
    float caustic = sin(FragPos.x * 2.0 + uTime * 3.0) * sin(FragPos.z * 2.0 + uTime * 2.0);
    caustic = max(0.0, caustic) * 0.03 * uSunIntensity;
    finalColor += vec3(caustic);

    // Fog
    float dist = length(FragPos - uCameraPos);
    float fog = 1.0 - exp(-dist * uFogDensity);
    finalColor = mix(finalColor, uFogColor, clamp(fog, 0.0, 1.0));

    float alpha = mix(0.55, 0.92, fresnel);
    FragColor = vec4(finalColor, alpha);
}
";

    // ─── Sky (atmospheric + clouds + stars) ────────────────────────────
    public const string SkyVertex = @"
#version 330 core
layout (location = 0) in vec2 aPosition;
out vec2 TexCoord;
void main()
{
    TexCoord = aPosition * 0.5 + 0.5;
    gl_Position = vec4(aPosition, 0.999, 1.0);
}
";

    public const string SkyFragment = @"
#version 330 core
in vec2 TexCoord;

uniform vec3 uZenithColor;
uniform vec3 uHorizonColor;
uniform vec3 uSunDirection;
uniform float uTime;

out vec4 FragColor;

float hash(vec2 p)
{
    p = fract(p * vec2(123.34, 456.21));
    p += dot(p, p + 45.32);
    return fract(p.x * p.y);
}

float noise(vec2 p)
{
    vec2 i = floor(p);
    vec2 f = fract(p);
    f = f * f * (3.0 - 2.0 * f);
    float a = hash(i);
    float b = hash(i + vec2(1.0, 0.0));
    float c = hash(i + vec2(0.0, 1.0));
    float d = hash(i + vec2(1.0, 1.0));
    return mix(mix(a, b, f.x), mix(c, d, f.x), f.y);
}

float fbm(vec2 p)
{
    float val = 0.0;
    float amp = 0.5;
    for (int i = 0; i < 5; i++)
    {
        val += amp * noise(p);
        p *= 2.0;
        amp *= 0.5;
    }
    return val;
}

void main()
{
    float t = TexCoord.y;
    vec3 sky = mix(uHorizonColor, uZenithColor, pow(t, 1.2));

    vec3 sunDir = normalize(-uSunDirection);
    vec3 viewDir = normalize(vec3(TexCoord.x * 2.0 - 1.0, t * 1.5 - 0.2, 0.6));

    // Multi-layer sun glow
    float sunDot = max(dot(viewDir, sunDir), 0.0);
    sky += pow(sunDot, 128.0) * vec3(1.0, 0.95, 0.8) * 1.5;
    sky += pow(sunDot, 16.0) * vec3(1.0, 0.7, 0.3) * 0.4;
    sky += pow(sunDot, 4.0) * vec3(0.8, 0.5, 0.2) * 0.15;

    // Procedural clouds
    if (t > 0.15)
    {
        float cloudHeight = (t - 0.15) / 0.85;
        vec2 cloudUV = vec2(TexCoord.x * 3.0 + uTime * 0.01, cloudHeight * 2.0 + uTime * 0.005);
        float cloud = fbm(cloudUV * 4.0);
        cloud = smoothstep(0.35, 0.65, cloud);

        float cloudLight = max(dot(sunDir, vec3(0.0, 1.0, 0.0)), 0.2);
        vec3 cloudColor = mix(vec3(0.6, 0.6, 0.65), vec3(1.0, 0.98, 0.92), cloudLight);

        float sunHeight = sunDir.y;
        if (sunHeight < 0.1)
        {
            float duskFactor = smoothstep(-0.1, 0.1, sunHeight);
            cloudColor = mix(cloudColor * vec3(0.8, 0.4, 0.25), cloudColor, duskFactor);
        }

        float horizonFade = smoothstep(0.15, 0.35, t);
        cloud *= horizonFade * 0.6;
        sky = mix(sky, cloudColor, cloud);
    }

    // Stars at night
    float nightFactor = smoothstep(0.0, -0.3, sunDir.y);
    if (nightFactor > 0.0 && t > 0.2)
    {
        vec2 starUV = TexCoord * 80.0;
        float starBrightness = hash(floor(starUV));
        starBrightness = pow(starBrightness, 20.0) * nightFactor;
        float twinkle = sin(uTime * 2.0 + starBrightness * 100.0) * 0.3 + 0.7;
        sky += vec3(starBrightness * twinkle);
    }

    FragColor = vec4(sky, 1.0);
}
";

    // ─── HUD shaders ───────────────────────────────────────────────────
    public const string HudVertex = @"
#version 330 core
layout (location = 0) in vec2 aPosition;
uniform mat4 uProjection;
uniform vec4 uRect;
void main()
{
    vec2 pos = uRect.xy + aPosition * uRect.zw;
    gl_Position = uProjection * vec4(pos, 0.0, 1.0);
}
";

    public const string HudFragment = @"
#version 330 core
uniform vec4 uColor;
out vec4 FragColor;
void main()
{
    FragColor = uColor;
}
";

    public const string ParticleVertex = @"
#version 330 core
layout (location = 0) in vec2 aQuad;
uniform mat4 uView;
uniform mat4 uProjection;
uniform vec3 uPosition;
uniform float uSize;
uniform vec3 uCameraRight;
uniform vec3 uCameraUp;
out vec2 vQuadCoord;
void main()
{
    vQuadCoord = aQuad + vec2(0.5);
    vec3 worldPos = uPosition
        + uCameraRight * aQuad.x * uSize
        + uCameraUp * aQuad.y * uSize;
    gl_Position = uProjection * uView * vec4(worldPos, 1.0);
}
";

    public const string ParticleFragment = @"
#version 330 core
in vec2 vQuadCoord;
uniform vec4 uColor;
out vec4 FragColor;
void main()
{
    vec2 center = vQuadCoord - vec2(0.5);
    float dist = length(center) * 2.0;
    float alpha = uColor.a * smoothstep(1.0, 0.3, dist);
    FragColor = vec4(uColor.rgb, alpha);
}
";

    public const string TextVertex = @"
#version 330 core
layout (location = 0) in vec2 aPosition;
layout (location = 1) in vec2 aTexCoord;
uniform mat4 uProjection;
out vec2 TexCoord;
void main()
{
    TexCoord = aTexCoord;
    gl_Position = uProjection * vec4(aPosition, 0.0, 1.0);
}
";

    public const string TextFragment = @"
#version 330 core
in vec2 TexCoord;
uniform sampler2D uTexture;
uniform vec4 uColor;
out vec4 FragColor;
void main()
{
    float alpha = texture(uTexture, TexCoord).a;

    // Drop shadow for readability (sample offset by 1 texel)
    vec2 texSize = vec2(textureSize(uTexture, 0));
    vec2 shadowOffset = vec2(1.0, 1.0) / texSize;
    float shadowAlpha = texture(uTexture, TexCoord + shadowOffset).a;
    shadowAlpha = max(shadowAlpha, texture(uTexture, TexCoord + vec2(shadowOffset.x, 0.0)).a);
    shadowAlpha = max(shadowAlpha, texture(uTexture, TexCoord + vec2(0.0, shadowOffset.y)).a);
    vec3 shadowColor = vec3(0.0);
    float shadowStrength = shadowAlpha * uColor.a * 0.5 * (1.0 - alpha);

    vec3 finalColor = mix(shadowColor, uColor.rgb, step(0.01, alpha));
    float finalAlpha = max(uColor.a * alpha, shadowStrength);
    FragColor = vec4(finalColor, finalAlpha);
}
";

    public const string HudTexturedVertex = @"
#version 330 core
layout (location = 0) in vec2 aPosition;
uniform mat4 uProjection;
uniform vec4 uRect;
out vec2 TexCoord;
void main()
{
    vec2 pos = uRect.xy + aPosition * uRect.zw;
    TexCoord = aPosition;
    gl_Position = uProjection * vec4(pos, 0.0, 1.0);
}
";

    public const string HudTexturedFragment = @"
#version 330 core
in vec2 TexCoord;
uniform sampler2D uTexture;
uniform vec4 uColor;
out vec4 FragColor;
void main()
{
    vec4 texColor = texture(uTexture, TexCoord);
    FragColor = texColor * uColor;
}
";

    // ─── Entity shader with point lights + shadows ────────────────────
    public const string BasicVertexPointLights = @"
#version 330 core
layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec3 aNormal;
layout (location = 2) in vec3 aColor;

uniform mat4 uModel;
uniform mat4 uView;
uniform mat4 uProjection;
uniform mat4 uLightSpaceMatrix;

out vec3 FragPos;
out vec3 LocalPos;
out vec3 Normal;
out vec3 VertexColor;
out vec4 FragPosLightSpace;

void main()
{
    vec4 worldPos = uModel * vec4(aPosition, 1.0);
    FragPos = worldPos.xyz;
    LocalPos = aPosition;
    Normal = mat3(transpose(inverse(uModel))) * aNormal;
    VertexColor = aColor;
    FragPosLightSpace = uLightSpaceMatrix * worldPos;
    gl_Position = uProjection * uView * worldPos;
}
";

    public const string BasicFragmentPointLights = @"
#version 330 core
in vec3 FragPos;
in vec3 LocalPos;
in vec3 Normal;
in vec3 VertexColor;
in vec4 FragPosLightSpace;

uniform vec3 uSunDirection;
uniform vec3 uSunColor;
uniform vec3 uAmbientColor;
uniform float uSunIntensity;
uniform float uFogDensity;
uniform vec3 uFogColor;
uniform vec3 uCameraPos;
uniform sampler2D uShadowMap;
uniform sampler2D uMetalTex;
uniform sampler2D uLeatherTex;
uniform sampler2D uSkinTex;
uniform sampler2D uWoodTex;
uniform sampler2D uFabricTex;

uniform int uPointLightCount;
uniform vec3 uPointLightPos[4];
uniform vec3 uPointLightColor[4];
uniform float uPointLightIntensity[4];
uniform float uPointLightRadius[4];

out vec4 FragColor;

vec3 triplanarSample(sampler2D tex, vec3 pos, vec3 n, float scale)
{
    vec3 bl = abs(n);
    bl = normalize(max(bl, 0.00001));
    float btotal = bl.x + bl.y + bl.z;
    bl /= btotal;
    return texture(tex, pos.yz * scale).rgb * bl.x
         + texture(tex, pos.xz * scale).rgb * bl.y
         + texture(tex, pos.xy * scale).rgb * bl.z;
}

float ShadowCalculation(vec4 fragPosLightSpace, vec3 normal, vec3 lightDir)
{
    vec3 projCoords = fragPosLightSpace.xyz / fragPosLightSpace.w;
    projCoords = projCoords * 0.5 + 0.5;
    if (projCoords.z > 1.0 || projCoords.x < 0.0 || projCoords.x > 1.0 ||
        projCoords.y < 0.0 || projCoords.y > 1.0)
        return 0.0;
    float currentDepth = projCoords.z;
    float bias = max(0.005 * (1.0 - dot(normal, lightDir)), 0.002);
    float shadow = 0.0;
    vec2 texelSize = 1.0 / textureSize(uShadowMap, 0);
    for (int x = -1; x <= 1; ++x)
        for (int y = -1; y <= 1; ++y)
        {
            float pcfDepth = texture(uShadowMap, projCoords.xy + vec2(x, y) * texelSize).r;
            shadow += currentDepth - bias > pcfDepth ? 1.0 : 0.0;
        }
    return shadow / 9.0;
}

void main()
{
    vec3 norm = normalize(Normal);
    vec3 lightDir = normalize(-uSunDirection);
    vec3 viewDir = normalize(uCameraPos - FragPos);

    // Material properties
    float luminance = dot(VertexColor, vec3(0.299, 0.587, 0.114));
    float metallic = smoothstep(0.32, 0.52, luminance) * 0.7;
    float roughness = mix(0.25, 0.85, 1.0 - metallic);

    // --- Material texture sampling via triplanar in object space ---
    float texScale = 2.5;
    vec3 localNorm = normalize(Normal);
    vec3 metalSample = triplanarSample(uMetalTex, LocalPos, localNorm, texScale);
    vec3 leatherSample = triplanarSample(uLeatherTex, LocalPos, localNorm, texScale * 1.1);
    vec3 skinSample = triplanarSample(uSkinTex, LocalPos, localNorm, texScale * 1.3);
    vec3 woodSample = triplanarSample(uWoodTex, LocalPos, localNorm, texScale * 0.7);
    vec3 fabricSample = triplanarSample(uFabricTex, LocalPos, localNorm, texScale * 1.4);

    float warmth = VertexColor.r - VertexColor.b;
    float greenness = VertexColor.g - (VertexColor.r + VertexColor.b) * 0.5;

    float metalWeight = smoothstep(0.36, 0.55, luminance) * smoothstep(-0.02, 0.08, -warmth) * 1.2;
    float skinWeight = smoothstep(0.12, 0.28, warmth) * smoothstep(0.40, 0.55, luminance);
    float woodWeight = max(smoothstep(0.04, 0.20, greenness),
                           smoothstep(0.10, 0.30, warmth) * step(luminance, 0.28));
    float leatherWeight = smoothstep(0.05, 0.22, warmth) * step(luminance, 0.38) * (1.0 - skinWeight * 0.8);
    float fabricWeight = max(0.0, 1.0 - metalWeight - skinWeight - woodWeight - leatherWeight);
    float goldWeight = smoothstep(0.35, 0.55, warmth) * smoothstep(0.45, 0.60, luminance);
    metalWeight += goldWeight * 0.6;
    skinWeight *= (1.0 - goldWeight);

    float totalWeight = max(metalWeight + leatherWeight + skinWeight + woodWeight + fabricWeight, 0.001);
    metalWeight /= totalWeight;
    leatherWeight /= totalWeight;
    skinWeight /= totalWeight;
    woodWeight /= totalWeight;
    fabricWeight /= totalWeight;

    vec3 materialTex = metalSample * metalWeight + leatherSample * leatherWeight
                     + skinSample * skinWeight + woodSample * woodWeight
                     + fabricSample * fabricWeight;

    vec3 texDetail = materialTex / vec3(0.5);
    texDetail = clamp(texDetail, 0.5, 1.55);

    vec3 texturedColor = VertexColor * texDetail;
    float camDist = length(FragPos - uCameraPos);
    float textureFade = smoothstep(3.0, 50.0, camDist);
    vec3 baseColor = mix(texturedColor, VertexColor, textureFade);

    // Two Worlds style warm color grading
    baseColor *= vec3(1.02, 1.00, 0.96);

    // Wrapped diffuse
    float NdotL = dot(norm, lightDir);
    float diff = NdotL * 0.5 + 0.5;
    diff = diff * diff;

    // Material-dependent specular
    vec3 halfDir = normalize(lightDir + viewDir);
    float shininess = mix(12.0, 160.0, 1.0 - roughness);
    float spec = pow(max(dot(norm, halfDir), 0.0), shininess);
    float specIntensity = mix(0.10, 0.60, metallic);
    vec3 specColor = mix(vec3(1.0), baseColor, metallic);
    specColor = mix(specColor, vec3(1.0, 0.85, 0.5), goldWeight * 0.4);
    vec3 specular = spec * specColor * specIntensity * uSunIntensity;

    // Hemisphere ambient with deeper AO
    float hemiBlend = dot(norm, vec3(0.0, 1.0, 0.0)) * 0.5 + 0.5;
    vec3 ambient = mix(uAmbientColor * 0.40, uAmbientColor * 1.3, hemiBlend);
    float cavityAO = smoothstep(-0.4, 0.5, norm.y) * 0.25 + 0.75;
    ambient *= cavityAO;

    // Bounce light from ground
    vec3 bounceLight = vec3(0.08, 0.06, 0.03) * max(0.0, -norm.y) * uSunIntensity;

    float shadow = ShadowCalculation(FragPosLightSpace, norm, lightDir);
    vec3 diffuse = diff * uSunColor * uSunIntensity * (1.0 - shadow * 0.75);
    vec3 lighting = (ambient + diffuse + bounceLight) * baseColor + specular * (1.0 - shadow);

    for (int i = 0; i < uPointLightCount && i < 4; i++)
    {
        vec3 toLight = uPointLightPos[i] - FragPos;
        float dist = length(toLight);
        if (dist > uPointLightRadius[i]) continue;
        float attenuation = 1.0 - (dist / uPointLightRadius[i]);
        attenuation *= attenuation;
        vec3 pLightDir = normalize(toLight);
        float pointDiff = max(dot(norm, pLightDir), 0.0);
        vec3 pHalfDir = normalize(pLightDir + viewDir);
        float pSpec = pow(max(dot(norm, pHalfDir), 0.0), shininess * 0.75);
        lighting += (pointDiff * baseColor + pSpec * specColor * 0.3)
                  * uPointLightColor[i] * uPointLightIntensity[i] * attenuation;
    }

    // Rim lighting (golden atmospheric backlight)
    float rim = 1.0 - max(dot(viewDir, norm), 0.0);
    rim = smoothstep(0.35, 1.0, rim);
    vec3 rimColor = mix(uSunColor * 0.10, baseColor * 0.15, 1.0 - metallic);
    lighting += rim * rimColor * uSunIntensity;

    // Fog with warm atmospheric scattering
    float fogDist = length(FragPos - uCameraPos);
    float heightFog = max(0.0, 1.0 - (FragPos.y - uCameraPos.y) * 0.012);
    float fog = 1.0 - exp(-fogDist * uFogDensity * (1.0 + heightFog * 0.5));
    vec3 warmFog = uFogColor + vec3(0.02, 0.01, -0.01);
    lighting = mix(lighting, warmFog, clamp(fog, 0.0, 1.0));

    FragColor = vec4(lighting, 1.0);
}
";

    // ─── Post-processing shaders ──────────────────────────────────────
    public const string PostProcessVertex = @"
#version 330 core
layout (location = 0) in vec2 aPosition;
out vec2 TexCoord;
void main()
{
    TexCoord = aPosition * 0.5 + 0.5;
    gl_Position = vec4(aPosition, 0.0, 1.0);
}
";

    public const string BloomExtractFragment = @"
#version 330 core
in vec2 TexCoord;
uniform sampler2D uScene;
uniform float uThreshold;
out vec4 FragColor;
void main()
{
    vec3 color = texture(uScene, TexCoord).rgb;
    float brightness = dot(color, vec3(0.2126, 0.7152, 0.0722));
    if (brightness > uThreshold)
        FragColor = vec4(color * (brightness - uThreshold), 1.0);
    else
        FragColor = vec4(0.0, 0.0, 0.0, 1.0);
}
";

    public const string BloomBlurFragment = @"
#version 330 core
in vec2 TexCoord;
uniform sampler2D uImage;
uniform int uHorizontal;
out vec4 FragColor;
void main()
{
    float weights[5] = float[](0.227027, 0.1945946, 0.1216216, 0.054054, 0.016216);
    vec2 texOffset = 1.0 / textureSize(uImage, 0);
    vec3 result = texture(uImage, TexCoord).rgb * weights[0];
    if (uHorizontal != 0)
    {
        for (int i = 1; i < 5; ++i)
        {
            result += texture(uImage, TexCoord + vec2(texOffset.x * i, 0.0)).rgb * weights[i];
            result += texture(uImage, TexCoord - vec2(texOffset.x * i, 0.0)).rgb * weights[i];
        }
    }
    else
    {
        for (int i = 1; i < 5; ++i)
        {
            result += texture(uImage, TexCoord + vec2(0.0, texOffset.y * i)).rgb * weights[i];
            result += texture(uImage, TexCoord - vec2(0.0, texOffset.y * i)).rgb * weights[i];
        }
    }
    FragColor = vec4(result, 1.0);
}
";

    public const string ToneMappingFragment = @"
#version 330 core
in vec2 TexCoord;
uniform sampler2D uScene;
uniform sampler2D uBloom;
uniform float uBloomIntensity;
uniform float uExposure;
out vec4 FragColor;
void main()
{
    vec3 hdrColor = texture(uScene, TexCoord).rgb;
    vec3 bloomColor = texture(uBloom, TexCoord).rgb;
    hdrColor += bloomColor * uBloomIntensity;

    // Apply exposure
    hdrColor *= uExposure;

    // ACES-like tonemapping
    vec3 mapped = (hdrColor * (2.51 * hdrColor + 0.03)) / (hdrColor * (2.43 * hdrColor + 0.59) + 0.14);
    mapped = pow(mapped, vec3(1.0 / 2.2));

    // Slight desaturation (~10%) for SpellForce/Gothic muted palette
    float luma = dot(mapped, vec3(0.299, 0.587, 0.114));
    mapped = mix(vec3(luma), mapped, 0.90);

    // Stronger vignette for dark-fantasy framing
    vec2 uv = TexCoord * 2.0 - 1.0;
    float vignette = 1.0 - dot(uv * 0.55, uv * 0.55);
    mapped *= smoothstep(0.0, 1.0, vignette);

    FragColor = vec4(mapped, 1.0);
}
";
}
