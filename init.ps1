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

    Install-Module pathutils
    refresh-env

    $dotnetver = "1.0.0-preview3-003171"
    if ((get-command "dotnet" -ErrorAction Ignore) -eq $null -or ((dotnet --version) -ne $dotnetver)) {
        wget "https://raw.githubusercontent.com/dotnet/cli/rel/1.0.0/scripts/obtain/dotnet-install.ps1" -UseBasicParsing -OutFile ".scripts/dotnet-install.ps1" 
        ./.scripts/dotnet-install.ps1 -Version $dotnetver    
        if (test-path "./.tools/dotnet") { remove-item "./.tools/dotnet" -force }
    } 
    
    if (!(test-path "./.tools/dotnet")) {
        $p = split-path -parent (get-command dotnet).source
        if (!(test-path "./.tools")) { mkdir "./.tools" }
        cmd /c mklink ".\.tools\dotnet" $p /J
        (get-item "./.tools/dotnet").FullName | add-topath
    }

    dotnet --info


    wget "https://raw.githubusercontent.com/aspnet/Home/dev/dnvminstall.ps1" -UseBasicParsing -OutFile ".scripts/dnvm-install.ps1" 
    ./.scripts/dnvm-install.ps1
    dnvm install 1.0.0-rc1-update1 -alias default
    dnvm use default

    dnu --version


} finally {
	popd
}