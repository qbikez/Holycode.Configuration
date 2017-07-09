param($publishdir = $null)

& "$psscriptroot\build-cmd.ps1"
#if (!(test-path $publishdir)) { $null = mkdir $publishdir }
#cp "$psscriptroot\.build\hcfg" $publishdir -Recurse -Force

pushd
try {
    cd "$psscriptroot\.build\hcfg"

    ipmo require
    req process

    $r = invoke octo pack -id "tools.cfg" --outFolder="$psscriptroot\.nupkg\hcfg" -passthru
    $line = $r | ? { $_ -match "Saving ([^\s]*) to" }
    $nupkg = $matches[1]

    write-output "$psscriptroot\.nupkg\hcfg\$nupkg"
    # $src = $env:NUGET_PUSH_SOURCE
    # nuget push $nupkg -source $src
} finally {
    popd
}

