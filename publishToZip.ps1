$mod_name = "BetterFreezeFrames"
dotnet build -c Release
$v_str = Get-Content -Path ModFolder/everest.yaml -Raw
$v = [regex]::Match($v_str, "(?<=Version:\s)(.*?)\n").Value.Trim()
Compress-Archive ModFolder/* "$mod_name v$v.zip" -Force
dotnet clean