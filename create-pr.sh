#!/bin/bash
set -e

cd /workspaces/cong-craft

gh pr create \
  --title "feat: integrate generated dark-fantasy texture assets" \
  --body "## Summary

Integrates 35 generated dark-fantasy style texture assets into the project, replacing procedural fallback textures with proper PNG files.

### Assets Added

**Terrain Textures** (512x512, tileable):
- \`terrain_grass.png\` - Dark earthy grass
- \`terrain_stone.png\` - Weathered stone
- \`terrain_dirt.png\` - Dark brown dirt
- \`terrain_snow.png\` - Frost-touched snow
- \`terrain_path.png\` - Worn travel path

**Material Textures** (512x512, tileable):
- \`mat_metal.png\` - Dark iron/steel
- \`mat_leather.png\` - Worn leather
- \`mat_skin.png\` - Character skin
- \`mat_wood.png\` - Aged oak
- \`mat_fabric.png\` - Rough woven cloth

**Water Maps** (512x512):
- \`water_normal.png\` - Normal map for water surface
- \`water_dudv.png\` - Distortion map for water refraction

**Icons** (64x64, transparent):
- 3 weapon icons (rusty sword, dark sword, bone club)
- 4 armor icons (leather helm, chain armor, leather armor, iron greaves)
- 6 material icons (wolf pelt, iron ore, wood, cloth, bone, troll hide)
- 2 item icons (herb, strength potion)
- 1 clothing icon (wolf cloak)

**Portraits** (128x128):
- Player portrait
- Guard NPC portrait
- Elder Maren NPC portrait

**Particle Sprites** (64x64, transparent):
- Fire, rain, blood, magic particles

### Code Changes

- Extended \`AssetPaths\` with \`Icons\`, \`Portraits\`, \`Particles\` helper properties
- Added \`IconFile()\`, \`PortraitFile()\`, \`ParticleFile()\` convenience methods
- Updated \`.gitignore\` for temporary integration files

### How It Works

The existing \`TextureLoader.LoadOrGenerate()\` system automatically picks up the new PNG files:
- Terrain textures loaded in \`TerrainSystem\`
- Material textures loaded in \`MaterialTextures\`
- Falls back to procedural generation if files are missing

### Testing

- Build: ✅ All 3 projects compile successfully
- Tests: ✅ 615/615 tests pass
" \
  --base main \
  --head feat/integrate-generated-assets

echo "=== PR CREATED ==="
