import-module require
req pathutils

(get-item "./.tools/dotnet").FullName | add-topath

write-host "dotnet found at:"
$r = where-is "dotnet"
$r | format-table | otu-string | write-host
dotnet --info

dotnet restore src/
dotnet restore test/
