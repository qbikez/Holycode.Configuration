import-module pathutils
(get-item "./.tools/dotnet").FullName | add-topath

write-host "dotnet found at:"
where-is "dotnet"
dotnet --info

pushd
try {
    dotnet build Holycode.Configuration.sln
    # dotnet build test/Holycode.Configuration.Tests.dotnet
    # dotnet build test/Holycode.Configuration.log4net.Tests
    # dotnet build src/Holycode.AspNetCore.Middleware
    
} finally {
    popd
}

if ($lastexitcode -ne 0){ exit $lastexitcode }