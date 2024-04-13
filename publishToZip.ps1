dotnet build -c Release
$sh_v_str = Get-Content -Path SafeAltF4/ModFolder/everest.yaml -Raw
$sh_v = [regex]::Match($sh_v_str, "(?<=Version:\s)(.*?)\n").Value.Trim()
Compress-Archive SafeAltF4/ModFolder/* "SafeAltF4 v$sh_v.zip" -Force
dotnet clean