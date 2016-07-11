pushd 

cd $psscriptroot

try {
    if (!(test-path ".scripts")) { mkdir ".scripts" }
    #init build tools
	wget http://bit.ly/qbootstrap1 -UseBasicParsing -OutFile ".scripts/bootstrap.ps1" 
    get-content ".scripts/bootstrap.ps1" | out-string | iex 
    #prepare packages dir
	if (!(test-path dnx-packages)) {
		cmd /c mklink dnx-packages $env:userprofile\.nuget\packages /J
	}

    $dotnetver = "1.0.0-preview3-003171"
    wget "https://raw.githubusercontent.com/dotnet/cli/rel/1.0.0/scripts/obtain/dotnet-install.ps1" -UseBasicParsing -OutFile ".scripts/dotnet-install.ps1" 
    ./.scripts/dotnet-install.ps1 -Version $dotnetver

    wget "https://raw.githubusercontent.com/aspnet/Home/dev/dnvminstall.ps1" -UseBasicParsing -OutFile ".scripts/dnvm-install.ps1" 
    ./.scripts/dnvm-install.ps1
    dnvm install 1.0.0-rc1-update1 -alias default
    dnvm use default


} finally {
	popd
}