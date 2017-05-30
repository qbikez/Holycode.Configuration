import-module require
req pathutils

(get-item "./.tools/dotnet").FullName | add-topath

write-host "dotnet found at:"
where-is "dotnet"
dotnet --info

dotnet restore src/
dotnet restore test/
