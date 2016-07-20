import-module pathutils
(get-item "./.tools/dotnet").FullName | add-topath


if (!(test-path "test\.results")) {
    mkdir "test\.results"
}

$tests = @(
    "Holycode.Configuration.Tests.dotnet"
    "Holycode.Configuration.log4net.Tests"
)

foreach($test in $tests) {
    pushd 
    try {
    
        cd "test\$test"
        dotnet restore
        dotnet test -xml "..\.results\$($test).xml"
    } finally {
        popd
    }
}
