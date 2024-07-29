$mod_name = "BitsHelper"
dotnet build -c Release
$v_str = Get-Content -Path BitsHelper/ModFolder/everest.yaml -Raw
$v = [regex]::Match($v_str, "(?<=Version:\s)(.*?)\n").Value.Trim()
Compress-Archive BitsHelper/ModFolder/* "$mod_name v$v.zip" -Force
dotnet clean