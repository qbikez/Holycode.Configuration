import-module pathutils
(get-item "./.tools/dotnet").FullName | add-topath


dotnet restore src/
dotnet restore test/
