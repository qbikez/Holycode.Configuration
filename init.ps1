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
} finally {
	popd
}