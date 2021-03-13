
dotnet build ArtifactOfDoom.csproj 
test -d "/media/ssdgamedisk/SteamLibrary/steamapps/common/Risk of Rain 2/BepInEx/plugins/asdf" && echo "Ready to copy" || echo "ISSUE WITH THE PATH PLEASE FIX"
cp /home/sirhamburger/Git/ArtifactOfDoom/bin/Debug/netstandard2.0/ArtifactOfDoom.dll "/media/ssdgamedisk/SteamLibrary/steamapps/common/Risk of Rain 2/BepInEx/plugins/asdf"
cp /home/sirhamburger/Git/ArtifactOfDoom/bin/Debug/netstandard2.0/ArtifactOfDoom.dll "/media/ssdgamedisk/SteamLibrary/steamapps/common/Risk of Rain 2 Dedicated Server/BepInEx/plugins/asdf"

#Release
cp /home/sirhamburger/Git/ArtifactOfDoom/bin/Debug/netstandard2.0/ArtifactOfDoom.dll ./Release

cp .//README.md ./Release

