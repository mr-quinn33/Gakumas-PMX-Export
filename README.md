# Gakumas-PMX-Exporter
A unity project to export character models into .pmx files based on the work of "IDOL" project by https://github.com/croakfang

## Purpose
1. The main purpose of this repo is to Help MMD Creators by improving the humanoid model exporting algorithm with open-source community.
2. Help modders or anyone else interested in extracting the models from the specific Unity game.

## How to run
1. Import asset bundles you need from the game client. Thanks to this repo https://github.com/nijinekoyo/Gakuen-idolmaster-ab-decrypt
2. Create new character config file by right-clicking and navigate to "Export/Character" in the menu.
3. Drag and drop or click on the "dot" icon to the right in each cell of the 3xN table to assign asset bundle objects in the order of "Face, Hair, Body" and make sure you leave no null pointers on this table.
4. Assign this config file of yours to the "ModelLoader" script attached to the "MainCamera" in scene "SampleScene".
5. Run the game to enter play mode then click on the "Export" button and wait until it is done.
6. (Optional) Run the "_magick_process.bat" batch file to convert some textures, and it is best to be done outside of UnityEditor to prevent it from importing back all the files you just exported and generateing all the .meta files you don't need.

## Samples
- Asset bundle sample and model samples can be found in "Assets/ExportSample/"

## Credits
- Asset decryption https://github.com/nijinekoyo/Gakuen-idolmaster-ab-decrypt
- Model extraction https://github.com/croakfang
- Model exporting https://github.com/croakfang/UnityPMXExporter
