import-module pathutils
(get-item "./.tools/dotnet").FullName | add-topath

write-host "dotnet found at:"
where-is "dotnet"
dotnet --info

msbuild Holycode.Configuration.sln

if ($lastexitcode -ne 0){ exit $lastexitcode }