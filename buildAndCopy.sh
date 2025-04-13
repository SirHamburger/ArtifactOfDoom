#Risk of Rain 2/Risk of Rain 2_Data/Managed
absolutPathToManagedFolder="/media/ssdgamedisk/SteamLibrary/steamapps/common/Risk of Rain 2/Risk of Rain 2_Data/Managed/"
absolutPathToBepInExFolder="/home/sirhamburger/.config/r2modmanPlus-local/RiskOfRain2/profiles/ArtifactOfDoomDependencys/BepInEx/"
currentDir="$(pwd)/"
echo $currentDir

buildTarget="${currentDir}/bin/Debug/netstandard2.1/"

rm -r ./libs
mkdir ./libs

cp "${absolutPathToManagedFolder}Assembly-CSharp.dll" ./libs
# cp "${absolutPathToBepInExFolder}core/BepInEx.dll" ./libs
cp "${absolutPathToBepInExFolder}plugins/MMHOOK/MMHOOK_Assembly-CSharp.dll" ./libs
cp -R "${absolutPathToBepInExFolder}core/." ./libs
# cp "${absolutPathToBepInExFolder}core/MonoMod.Utils.dll" ./libs



r2ApiDependencys=("Prefab" "ContentManagement")

for element in "${r2ApiDependencys[@]}"; do
    cp "${absolutPathToBepInExFolder}plugins/RiskofThunder-R2API_${element}/R2API.${element}/R2API.${element}.dll" ./libs
done
cp "${absolutPathToBepInExFolder}plugins/RiskofThunder-RoR2BepInExPack/RoR2BepInExPack/RoR2BepInExPack.dll" ./libs


unityDependencys=("UnityEngine.dll" "UnityEngine.AssetBundleModule.dll" "UnityEngine.CoreModule.dll" "UnityEngine.UI.dll" "UnityEngine.UIModule.dll" "RoR2.dll" "com.unity.multiplayer-hlapi.Runtime.dll" )
for element in "${unityDependencys[@]}"; do
    cp "${absolutPathToManagedFolder}${element}" ./libs
done

rm ./Release/ArtifactOfDoomThunderstore/*
rm ./Release/ArtifactOfDoomR2Modman/*

dotnet build ArtifactOfDoom.csproj 

cp "${buildTarget}/ArtifactOfDoom.dll" "./Release"
cp "${buildTarget}/ArtifactOfDoom.dll" "./Release/ArtifactOfDoomR2Modman"
cp "${buildTarget}/ArtifactOfDoom.dll" "./Release/ArtifactOfDoomThunderstore"

# wine ./NetworkWeaver/Unity.UNetWeaver.exe "${currentDir}libs/UnityEngine.CoreModule.dll" "${currentDir}libs/UnityEngine.Networking.dll" "${buildTarget}"  "${currentDir}Release/ArtifactOfDoom.dll" "${currentDir}libs"
wine ./NetworkWeaver/Unity.UNetWeaver.exe "${currentDir}libs/UnityEngine.CoreModule.dll" "${currentDir}libs/com.unity.multiplayer-hlapi.Runtime.dll" "${buildTarget}"  "${currentDir}Release/ArtifactOfDoom.dll" "${currentDir}libs"


rm -r Release/ArtifactOfDoomR2Modman
rm -r Release/ArtifactOfDoomThunderstore
mkdir Release/ArtifactOfDoomR2Modman
mkdir Release/ArtifactOfDoomThunderstore


cp ${buildTarget}ArtifactOfDoom.dll ./Release/ArtifactOfDoomR2Modman
cp ${buildTarget}ArtifactOfDoom.dll ./Release/ArtifactOfDoomThunderstore

cp "./README.md" "./Release/ArtifactOfDoomThunderstore/README.md"
sed -i 's/!\[\]( UI.png )/!\[\](https:\/\/raw.githubusercontent.com\/SirHamburger\/ArtifactOfDoom\/master\/UI.png)/g' ./Release/ArtifactOfDoomThunderstore/README.md
sed -i 's/!\[\]( curveItemGet.png )/!\[\](https:\/\/raw.githubusercontent.com\/SirHamburger\/ArtifactOfDoom\/master\/curveItemGet.png)/g' ./Release/ArtifactOfDoomThunderstore/README.md

cp "./README.md" "./Release/ArtifactOfDoomR2Modman/"
cp "./CHANGELOG.md" "./Release/ArtifactOfDoomR2Modman/"
cp "./CHANGELOG.md" "./Release/ArtifactOfDoomThunderstore/"
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

#cp ${buildTarget}/ArtifactOfDoom.dll "/media/ssdgamedisk/SteamLibrary/steamapps/common/Risk of Rain 2 Dedicated Server/BepInEx/plugins/Sir Hamburger/"
cp ${buildTarget}ArtifactOfDoom.dll "/media/ssdgamedisk/SteamLibrary/steamapps/common/Risk of Rain 2 Dedicated Server/BepInEx/plugins/Sir Hamburger/"
cp ${buildTarget}ArtifactOfDoom.dll "/media/ssdgamedisk/SteamLibrary/steamapps/common/Risk of Rain 2/BepInEx/plugins/ArtifactOfDoom/"