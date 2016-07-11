pushd 
try {
	cd test\holycode.configuration.tests.dotnet
	dotnet restore
	dotnet test
} finally {
popd
}

pushd 
try {
	cd test\holycode.Configuration.Tests.dnx
	dnu restore
    dnu build
	dnx test
	
	if ($lastexitcode -ne 0) { exit $lastexitcode }
} finally {
popd
}