if ((test-path "bin\Debug")) {
    remove-item "bin\Debug" -Force -Recurse
}

#write-host "pwd=$PWD"
#$dir = [System.IO.Directory]::GetCurrentDirectory()
#write-host (get-item .).FullName
#write-host "dir=$dir"
write-host "pwd=$PWD"
#dnu --version
#dnvm use default
#dnvm list
dnu restore
dnu pack 
remove-item -Path "bin\Debug\*.symbols.nupkg"
nuget push -source "http://nuget.legimi.com" "bin\Debug\*.nupkg"