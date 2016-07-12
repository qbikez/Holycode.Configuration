pushd 
try {
	cd test\holycode.configuration.tests.dotnet
	dotnet restore
	dotnet test
} finally {
popd
}

pushd 
