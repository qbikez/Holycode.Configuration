import-module require
req pathutils
req process

(get-item "./.tools/dotnet").FullName | add-topath


if (!(test-path "test\.results")) {
    mkdir "test\.results"
}

$tests = @(
    "Holycode.Configuration.Tests.dotnet"
    "Holycode.Configuration.log4net.Tests"
)

$errors = @()

foreach($test in $tests) {
    pushd 
    try {
    
        cd "test\$test"
        invoke dotnet restore -nothrow
        if ($LASTEXITCODE -ne 0) { errors += "dotnet restore FAILED for $test" }
        invoke dotnet test -xml "..\.results\$($test).xml" -nothrow
        if ($LASTEXITCODE -ne 0) { errors += "dotnet test FAILED for $test" }
    } finally {
        popd
    }
}

if ($errors.Count -gt 0) {
    $msg = "tests failed:`r`n" + [string]::Join("`r`n", $errors)
    throw $msg
}
