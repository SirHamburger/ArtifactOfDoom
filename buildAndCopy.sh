
dotnet build ArtifactOfDoom.csproj 
cp /home/sirhamburger/Git/ArtifactOfDoom/bin/Debug/netstandard2.0/ArtifactOfDoom.dll "/media/ssdGamedisk/SteamLibrary/steamapps/common/Risk of Rain 2/BepInEx/plugins/asdf"
cp /home/sirhamburger/Git/ArtifactOfDoom/bin/Debug/netstandard2.0/ArtifactOfDoom.dll "/media/ssdGamedisk/SteamLibrary/steamapps/common/Risk of Rain 2 Dedicated Server/BepInEx/plugins/asdf"

#Release
cp /home/sirhamburger/Git/ArtifactOfDoom/bin/Debug/netstandard2.0/ArtifactOfDoom.dll ./Release

cp .//README.md ./Release

