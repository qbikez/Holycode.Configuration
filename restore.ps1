import-module require
req pathutils

(get-item "./.tools/dotnet").FullName | add-topath

write-host "dotnet found at:"
$r = where-is "dotnet"
$r | format-table | out-string | write-host
dotnet --info

dotnet restore 
