import-module pathutils
(get-item "./.tools/dotnet").FullName | add-topath


msbuild Holycode.Configuration.sln

if ($lastexitcode -ne 0){ exit $lastexitcode }