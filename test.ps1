pushd 
try {
	cd test\common.configuration.tests.dotnet
	dotnet restore
	dotnet test
} finally {
popd
}

pushd 
try {
	cd test\Common.Configuration.Tests.dnx
	dnu restore
	dnx test
} finally {
popd
}