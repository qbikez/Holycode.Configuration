import-module pathutils
(get-item "./.tools/dotnet").FullName | add-topath


pushd 
try {
    if (!(test-path "test\.results")) {
        mkdir "test\.results"
    }
	cd test\holycode.configuration.tests.dotnet
	dotnet restore
	dotnet test -xml ..\.results\Holycode.Configuration.Tests.dotnet.xml
} finally {
popd
}

pushd 
