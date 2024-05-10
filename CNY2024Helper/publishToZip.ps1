dotnet build -c Release
$sh_v_str = Get-Content -Path ModFolder/everest.yaml -Raw
$sh_v = [regex]::Match($sh_v_str, "(?<=Version:\s)(.*?)\n").Value.Trim()
Compress-Archive ModFolder/* "CNY2024Helper_renew v$sh_v.zip" -Force
dotnet clean