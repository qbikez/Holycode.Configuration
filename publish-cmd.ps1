param($publishdir = "e:\progz")

& "$psscriptroot\build-cmd.ps1"
#if (!(test-path $publishdir)) { $null = mkdir $publishdir }
cp "$psscriptroot\.build\hcfg" $publishdir -Recurse -Force