# Gakumas-PMX-Exporter
A unity project to export character models into .pmx files based on the work of "IDOL" project by https://github.com/croakfang

## Purpose
1. The main purpose of this repo is to Help MMD Creators by improving the humanoid model exporting algorithm with open-source community.
2. Help modders or anyone else interested in extracting the models from the specific Unity game.

## How to run
1. Import asset bundles you need from the game client. Thanks to this repo https://github.com/nijinekoyo/Gakuen-idolmaster-ab-decrypt
2. Create new character config file by right-clicking and navigate to "Export/Character" in the menu.
   <img width="542" alt="image" src="https://github.com/user-attachments/assets/be53fa6d-fc7d-4cdf-9e92-865029cccb39" />
5. Drag and drop or click on the "dot" icon to the right in each cell of the 3xN table to assign asset bundle objects in the order of "Face, Hair, Body" and make sure you leave no null pointers on this table.
   <img width="830" alt="image" src="https://github.com/user-attachments/assets/16afaa7c-136f-4406-85a7-d9b661e0c583" />
   <img width="564" alt="image" src="https://github.com/user-attachments/assets/7f0d78cc-fecf-48df-ab77-df2cf1016f86" />
7. Assign this config file of yours to the "ModelLoader" script attached to the "MainCamera" in scene "SampleScene".

   <img width="589" alt="image" src="https://github.com/user-attachments/assets/c17e87bc-a6cd-4889-b2ab-df4c19602004" />

8. Cltr+P or click "Play" to run the game to enter play mode then click on the "Export" button and wait until it is done.

    <img width="121" alt="image" src="https://github.com/user-attachments/assets/a22082ac-437c-438b-8255-a52f2d1a5733" />
    <img width="422" alt="image" src="https://github.com/user-attachments/assets/ef5b6606-8d4a-48cd-a9c2-1c637bfaf031" />

9.  (Optional) Run the "_magick_process.bat" batch file to convert some textures, and it is best to be done outside of UnityEditor to prevent it from importing back all the files you just exported and generateing all the .meta files you don't need.

    <img width="156" alt="image" src="https://github.com/user-attachments/assets/a0a4c8b6-6197-4724-adc8-db82df65140e" />

## Samples
- Asset bundle sample and model samples can be found in "Assets/ExportSample/"

## Credits
- Asset decryption https://github.com/nijinekoyo/Gakuen-idolmaster-ab-decrypt
- Model extraction https://github.com/croakfang
- Model exporting https://github.com/croakfang/UnityPMXExporter
