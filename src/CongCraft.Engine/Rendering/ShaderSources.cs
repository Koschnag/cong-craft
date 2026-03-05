namespace CongCraft.Engine.Rendering;

/// <summary>
/// All GLSL shaders embedded as string constants. No external files needed.
/// </summary>
public static class ShaderSources
{
    public const string TerrainVertex = @"
#version 330 core
layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec3 aNormal;
layout (location = 2) in vec2 aTexCoord;

uniform mat4 uModel;
uniform mat4 uView;
uniform mat4 uProjection;

out vec3 FragPos;
out vec3 Normal;
out vec2 TexCoord;
out float Height;

void main()
{
    vec4 worldPos = uModel * vec4(aPosition, 1.0);
    FragPos = worldPos.xyz;
    Normal = mat3(transpose(inverse(uModel))) * aNormal;
    TexCoord = aTexCoord;
    Height = aPosition.y;
    gl_Position = uProjection * uView * worldPos;
}
";

    public const string TerrainFragment = @"
#version 330 core
in vec3 FragPos;
in vec3 Normal;
in vec2 TexCoord;
in float Height;

uniform vec3 uSunDirection;
uniform vec3 uSunColor;
uniform vec3 uAmbientColor;
uniform float uSunIntensity;
uniform float uFogDensity;
uniform vec3 uFogColor;
uniform vec3 uCameraPos;

out vec4 FragColor;

void main()
{
    vec3 norm = normalize(Normal);
    float slope = 1.0 - abs(dot(norm, vec3(0.0, 1.0, 0.0)));

    // Height-based color blending: grass -> dirt -> stone -> snow
    vec3 grassColor = vec3(0.15, 0.35, 0.08);
    vec3 dirtColor = vec3(0.35, 0.25, 0.15);
    vec3 stoneColor = vec3(0.45, 0.42, 0.38);
    vec3 snowColor = vec3(0.85, 0.85, 0.9);

    vec3 baseColor;
    if (Height < 2.0)
        baseColor = mix(dirtColor, grassColor, clamp((Height + 1.0) / 3.0, 0.0, 1.0));
    else if (Height < 15.0)
        baseColor = mix(grassColor, stoneColor, clamp((Height - 2.0) / 13.0, 0.0, 1.0));
    else
        baseColor = mix(stoneColor, snowColor, clamp((Height - 15.0) / 10.0, 0.0, 1.0));

    // Steep slopes get stone
    baseColor = mix(baseColor, stoneColor, smoothstep(0.4, 0.7, slope));

    // Directional lighting
    float diff = max(dot(norm, normalize(-uSunDirection)), 0.0);
    vec3 diffuse = diff * uSunColor * uSunIntensity;
    vec3 lighting = (uAmbientColor + diffuse) * baseColor;

    // Distance fog
    float dist = length(FragPos - uCameraPos);
    float fog = 1.0 - exp(-dist * uFogDensity);
    lighting = mix(lighting, uFogColor, clamp(fog, 0.0, 1.0));

    FragColor = vec4(lighting, 1.0);
}
";

    public const string BasicVertex = @"
#version 330 core
layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec3 aNormal;
layout (location = 2) in vec3 aColor;

uniform mat4 uModel;
uniform mat4 uView;
uniform mat4 uProjection;

out vec3 FragPos;
out vec3 Normal;
out vec3 VertexColor;

void main()
{
    vec4 worldPos = uModel * vec4(aPosition, 1.0);
    FragPos = worldPos.xyz;
    Normal = mat3(transpose(inverse(uModel))) * aNormal;
    VertexColor = aColor;
    gl_Position = uProjection * uView * worldPos;
}
";

    public const string BasicFragment = @"
#version 330 core
in vec3 FragPos;
in vec3 Normal;
in vec3 VertexColor;

uniform vec3 uSunDirection;
uniform vec3 uSunColor;
uniform vec3 uAmbientColor;
uniform float uSunIntensity;
uniform float uFogDensity;
uniform vec3 uFogColor;
uniform vec3 uCameraPos;

out vec4 FragColor;

void main()
{
    vec3 norm = normalize(Normal);
    float diff = max(dot(norm, normalize(-uSunDirection)), 0.0);
    vec3 diffuse = diff * uSunColor * uSunIntensity;
    vec3 lighting = (uAmbientColor + diffuse) * VertexColor;

    float dist = length(FragPos - uCameraPos);
    float fog = 1.0 - exp(-dist * uFogDensity);
    lighting = mix(lighting, uFogColor, clamp(fog, 0.0, 1.0));

    FragColor = vec4(lighting, 1.0);
}
";

    public const string WaterVertex = @"
#version 330 core
layout (location = 0) in vec3 aPosition;

uniform mat4 uModel;
uniform mat4 uView;
uniform mat4 uProjection;
uniform float uTime;

out vec3 FragPos;

void main()
{
    vec3 pos = aPosition;
    pos.y += sin(pos.x * 0.5 + uTime * 1.5) * 0.1 + cos(pos.z * 0.3 + uTime) * 0.08;
    vec4 worldPos = uModel * vec4(pos, 1.0);
    FragPos = worldPos.xyz;
    gl_Position = uProjection * uView * worldPos;
}
";

    public const string WaterFragment = @"
#version 330 core
in vec3 FragPos;

uniform vec3 uSunDirection;
uniform vec3 uSunColor;
uniform float uSunIntensity;
uniform vec3 uAmbientColor;
uniform float uFogDensity;
uniform vec3 uFogColor;
uniform vec3 uCameraPos;

out vec4 FragColor;

void main()
{
    vec3 waterColor = vec3(0.05, 0.15, 0.25);
    vec3 norm = vec3(0.0, 1.0, 0.0);

    float diff = max(dot(norm, normalize(-uSunDirection)), 0.0);
    vec3 lighting = (uAmbientColor + diff * uSunColor * uSunIntensity) * waterColor;

    // Specular highlight
    vec3 viewDir = normalize(uCameraPos - FragPos);
    vec3 reflectDir = reflect(normalize(uSunDirection), norm);
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), 32.0);
    lighting += spec * uSunColor * 0.5;

    float dist = length(FragPos - uCameraPos);
    float fog = 1.0 - exp(-dist * uFogDensity);
    lighting = mix(lighting, uFogColor, clamp(fog, 0.0, 1.0));

    FragColor = vec4(lighting, 0.6);
}
";

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

out vec4 FragColor;

void main()
{
    float t = TexCoord.y;
    vec3 sky = mix(uHorizonColor, uZenithColor, pow(t, 1.5));

    // Sun glow near horizon
    float sunDot = max(dot(normalize(vec3(TexCoord.x * 2.0 - 1.0, t, 0.5)), normalize(-uSunDirection)), 0.0);
    sky += pow(sunDot, 64.0) * vec3(1.0, 0.8, 0.4) * 0.5;

    FragColor = vec4(sky, 1.0);
}
";

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

void main()
{
    vec3 worldPos = uPosition
        + uCameraRight * aQuad.x * uSize
        + uCameraUp * aQuad.y * uSize;
    gl_Position = uProjection * uView * vec4(worldPos, 1.0);
}
";

    public const string ParticleFragment = @"
#version 330 core
uniform vec4 uColor;

out vec4 FragColor;

void main()
{
    // Soft circle falloff
    vec2 center = gl_PointCoord - vec2(0.5);
    float dist = length(center) * 2.0;
    float alpha = uColor.a * smoothstep(1.0, 0.3, dist);
    FragColor = vec4(uColor.rgb, alpha);
}
";

    // --- Text rendering shader (textured quads with color tint) ---
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
    FragColor = vec4(uColor.rgb, uColor.a * alpha);
}
";

    // --- Textured HUD shader (for ornate panels with texture + color tint) ---
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

    public const string BasicVertexPointLights = @"
#version 330 core
layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec3 aNormal;
layout (location = 2) in vec3 aColor;

uniform mat4 uModel;
uniform mat4 uView;
uniform mat4 uProjection;

out vec3 FragPos;
out vec3 Normal;
out vec3 VertexColor;

void main()
{
    vec4 worldPos = uModel * vec4(aPosition, 1.0);
    FragPos = worldPos.xyz;
    Normal = mat3(transpose(inverse(uModel))) * aNormal;
    VertexColor = aColor;
    gl_Position = uProjection * uView * worldPos;
}
";

    public const string BasicFragmentPointLights = @"
#version 330 core
in vec3 FragPos;
in vec3 Normal;
in vec3 VertexColor;

uniform vec3 uSunDirection;
uniform vec3 uSunColor;
uniform vec3 uAmbientColor;
uniform float uSunIntensity;
uniform float uFogDensity;
uniform vec3 uFogColor;
uniform vec3 uCameraPos;

// Point lights
uniform int uPointLightCount;
uniform vec3 uPointLightPos[4];
uniform vec3 uPointLightColor[4];
uniform float uPointLightIntensity[4];
uniform float uPointLightRadius[4];

out vec4 FragColor;

void main()
{
    vec3 norm = normalize(Normal);

    // Directional light (sun)
    float diff = max(dot(norm, normalize(-uSunDirection)), 0.0);
    vec3 diffuse = diff * uSunColor * uSunIntensity;
    vec3 lighting = (uAmbientColor + diffuse) * VertexColor;

    // Point lights
    for (int i = 0; i < uPointLightCount && i < 4; i++)
    {
        vec3 toLight = uPointLightPos[i] - FragPos;
        float dist = length(toLight);
        if (dist > uPointLightRadius[i]) continue;

        float attenuation = 1.0 - (dist / uPointLightRadius[i]);
        attenuation = attenuation * attenuation; // Quadratic falloff
        float pointDiff = max(dot(norm, normalize(toLight)), 0.0);
        lighting += pointDiff * uPointLightColor[i] * uPointLightIntensity[i] * attenuation * VertexColor;
    }

    // Distance fog
    float fogDist = length(FragPos - uCameraPos);
    float fog = 1.0 - exp(-fogDist * uFogDensity);
    lighting = mix(lighting, uFogColor, clamp(fog, 0.0, 1.0));

    FragColor = vec4(lighting, 1.0);
}
";
}
