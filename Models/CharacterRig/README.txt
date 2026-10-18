Rebuilds a character .mesh from its MagicaVoxel source with rigid, closed body parts (one bone per part).

1. py -I fit.py Builder=<path to Инженер.vox> Combatant=<Военный.vox> Grenadier=<Подрывник.vox> Sniper=<Снайпер.vox>
   finds how each .vox lines up with the original mesh in git HEAD and writes fit.json
2. py -I build_char.py <Name> <path to .vox>
   writes Assets/Meshes/<Name>/<Name>.mesh and Assets/Resources/Materials/<Name>/<Name>_Palette.png
   (bones and bind poses are taken from git HEAD, so prefabs and animations keep working)
3. In Unity: palette import = Point filter, no compression, no mipmaps; material _BaseMap = <Name>_Palette.
