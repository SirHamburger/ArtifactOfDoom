
    
cp "/media/ssdgamedisk/SteamLibrary/steamapps/common/Risk of Rain 2/Risk of Rain 2_Data/Managed/Assembly-CSharp.dll" ./libs
# cp "/home/sirhamburger/.config/r2modmanPlus-local/RiskOfRain2/profiles/ArtifactOfDoomDependencys/BepInEx/core/BepInEx.dll" ./libs
cp "/home/sirhamburger/.config/r2modmanPlus-local/RiskOfRain2/profiles/ArtifactOfDoomDependencys/BepInEx/plugins/MMHOOK/MMHOOK_Assembly-CSharp.dll" ./libs
cp -R "/home/sirhamburger/.config/r2modmanPlus-local/RiskOfRain2/profiles/ArtifactOfDoomDependencys/BepInEx/core/." ./libs
# cp "/home/sirhamburger/.config/r2modmanPlus-local/RiskOfRain2/profiles/ArtifactOfDoomDependencys/BepInEx/core/MonoMod.Utils.dll" ./libs
cp "/home/sirhamburger/.config/r2modmanPlus-local/RiskOfRain2/profiles/ArtifactOfDoomDependencys/BepInEx/plugins/tristanmcpherson-R2API/R2API/R2API.dll" ./libs
cp "/media/ssdgamedisk/SteamLibrary/steamapps/common/Risk of Rain 2/Risk of Rain 2_Data/Managed/Unity.Postprocessing.Runtime.dll" ./libs
cp "/media/ssdgamedisk/SteamLibrary/steamapps/common/Risk of Rain 2/Risk of Rain 2_Data/Managed/Unity.TextMeshPro.dll" ./libs
cp "/media/ssdgamedisk/SteamLibrary/steamapps/common/Risk of Rain 2/Risk of Rain 2_Data/Managed/UnityEngine.dll" ./libs
cp "/media/ssdgamedisk/SteamLibrary/steamapps/common/Risk of Rain 2/Risk of Rain 2_Data/Managed/UnityEngine.AssetBundleModule.dll" ./libs
cp "/media/ssdgamedisk/SteamLibrary/steamapps/common/Risk of Rain 2/Risk of Rain 2_Data/Managed/UnityEngine.CoreModule.dll" ./libs
cp "/media/ssdgamedisk/SteamLibrary/steamapps/common/Risk of Rain 2/Risk of Rain 2_Data/Managed/UnityEngine.Networking.dll" ./libs
cp "/media/ssdgamedisk/SteamLibrary/steamapps/common/Risk of Rain 2/Risk of Rain 2_Data/Managed/UnityEngine.ParticleSystemModule.dll" ./libs
cp "/media/ssdgamedisk/SteamLibrary/steamapps/common/Risk of Rain 2/Risk of Rain 2_Data/Managed/UnityEngine.UI.dll" ./libs
cp "/media/ssdgamedisk/SteamLibrary/steamapps/common/Risk of Rain 2/Risk of Rain 2_Data/Managed/UnityEngine.InputModule.dll" ./libs
cp "/media/ssdgamedisk/SteamLibrary/steamapps/common/Risk of Rain 2/Risk of Rain 2_Data/Managed/UnityEngine.UIElementsModule.dll" ./libs
cp "/media/ssdgamedisk/SteamLibrary/steamapps/common/Risk of Rain 2/Risk of Rain 2_Data/Managed/UnityEngine.UIModule.dll" ./libs
cp "/media/ssdgamedisk/SteamLibrary/steamapps/common/Risk of Rain 2/Risk of Rain 2_Data/Managed/RoR2.dll" ./libs
cp "/media/ssdgamedisk/SteamLibrary/steamapps/common/Risk of Rain 2/Risk of Rain 2_Data/Managed/com.unity.multiplayer-hlapi.Runtime.dll" ./libs

rm ./Release/ArtifactOfDoomThunderstore/*
rm ./Release/ArtifactOfDoomR2Modman/*

dotnet build ArtifactOfDoom.csproj 

cp "/home/sirhamburger/Git/ArtifactOfDoom/bin/Debug/netstandard2.0/ArtifactOfDoom.dll" "./Release"
cp "/home/sirhamburger/Git/ArtifactOfDoom/bin/Debug/netstandard2.0/ArtifactOfDoom.dll" "./Release/ArtifactOfDoomR2Modman"
cp "/home/sirhamburger/Git/ArtifactOfDoom/bin/Debug/netstandard2.0/ArtifactOfDoom.dll" "./Release/ArtifactOfDoomThunderstore"

# wine ./NetworkWeaver/Unity.UNetWeaver.exe "/home/sirhamburger/Git/ArtifactOfDoom/libs/UnityEngine.CoreModule.dll" "/home/sirhamburger/Git/ArtifactOfDoom/libs/UnityEngine.Networking.dll" "/home/sirhamburger/Git/ArtifactOfDoom/libs/Patched/"  "/home/sirhamburger/Git/ArtifactOfDoom/Release/ArtifactOfDoom.dll" "/home/sirhamburger/Git/ArtifactOfDoom/libs"
wine ./NetworkWeaver/Unity.UNetWeaver.exe "/home/sirhamburger/Git/ArtifactOfDoom/libs/UnityEngine.CoreModule.dll" "/home/sirhamburger/Git/ArtifactOfDoom/libs/com.unity.multiplayer-hlapi.Runtime.dll" "/home/sirhamburger/Git/ArtifactOfDoom/libs/Patched/"  "/home/sirhamburger/Git/ArtifactOfDoom/Release/ArtifactOfDoom.dll" "/home/sirhamburger/Git/ArtifactOfDoom/libs"

cp /home/sirhamburger/Git/ArtifactOfDoom/libs/Patched/ArtifactOfDoom.dll ./Release/ArtifactOfDoomR2Modman
cp /home/sirhamburger/Git/ArtifactOfDoom/libs/Patched/ArtifactOfDoom.dll ./Release/ArtifactOfDoomThunderstore



sed 's/!\[\]( UI.png )/!\[\](https:\/\/raw.githubusercontent.com\/SirHamburger\/ArtifactOfDoom\/master\/UI.png)/g' README.md > ./Release/ArtifactOfDoomThunderstore/README.md

sed 's/!\[\]( curveItemGet.png )/!\[\](https:\/\/raw.githubusercontent.com\/SirHamburger\/ArtifactOfDoom\/master\/curveItemGet.png)/g' README.md > ./Release/ArtifactOfDoomThunderstore/README.md
cp "./README.md" "./Release/ArtifactOfDoomR2Modman/"
cp "./Release/icon.png" "./Release/ArtifactOfDoomR2Modman/"
cp "./Release/icon.png" "./Release/ArtifactOfDoomThunderstore/"
cp "./Release/manifest.json" "./Release/ArtifactOfDoomThunderstore/"
cp "./Release/manifestR2Modman.json" "./Release/ArtifactOfDoomR2Modman/manifest.json"
#
cd ./Release/ArtifactOfDoomR2Modman
zip ArtifactOfDoomR2Modman *
cd ..
cd ArtifactOfDoomThunderstore
zip ArtifactOfDoom *

#cp /home/sirhamburger/Git/ArtifactOfDoom/bin/Debug/netstandard2.0/ArtifactOfDoom.dll "/media/ssdgamedisk/SteamLibrary/steamapps/common/Risk of Rain 2 Dedicated Server/BepInEx/plugins/Sir Hamburger/"
cp /home/sirhamburger/Git/ArtifactOfDoom/libs/Patched/ArtifactOfDoom.dll "/media/ssdgamedisk/SteamLibrary/steamapps/common/Risk of Rain 2 Dedicated Server/BepInEx/plugins/Sir Hamburger/"
cp /home/sirhamburger/Git/ArtifactOfDoom/libs/Patched/ArtifactOfDoom.dll "/media/ssdgamedisk/SteamLibrary/steamapps/common/Risk of Rain 2/BepInEx/plugins/ArtifactOfDoom/"