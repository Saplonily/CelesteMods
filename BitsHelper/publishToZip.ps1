$yml = Get-Content -Path BitsHelper/ModFolder/everest.yaml -Raw
$modName = [regex]::Match($yml, "(?<=Name:\s)(.*?)\n").Value.Trim()
$version = [regex]::Match($yml, "(?<=Version:\s)(.*?)\n").Value.Trim()
Write-Host "Building $modName v$version..."
dotnet build -c Release
Compress-Archive BitsHelper/ModFolder/* "$modName v$version.zip" -Force
dotnet clean