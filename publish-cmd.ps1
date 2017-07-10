param($type="choco",$publishdir = $null)

& "$psscriptroot\build-cmd.ps1"
#if (!(test-path $publishdir)) { $null = mkdir $publishdir }
#cp "$psscriptroot\.build\hcfg" $publishdir -Recurse -Force

ipmo require
req process
req pathutils

pushd
try {
    switch($type) {
        "octo" {
            cd "$psscriptroot\.build\hcfg"

            if ($null -eq (where-is octo)) {
                choco install -y octopustools
            }

            $r = invoke octo pack -id "tools.cfg" --outFolder="$psscriptroot\.nupkg\hcfg" -passthru
            $line = $r | ? { $_ -match "Saving ([^\s]*) to" }
            $nupkg = $matches[1]

            write-output "$psscriptroot\.nupkg\hcfg\$nupkg"
            # $src = $env:NUGET_PUSH_SOURCE
            # 3invoke nuget push $nupkg -source $src
        }
        "choco" {
            $chocoDir = "$psscriptroot\.choco"
            if (!(test-path "$chocoDir\bin")) { $null = mkdir "$chocoDir\bin" }
            cp "$psscriptroot\.build\hcfg\*" "$chocoDir\bin" -Recurse -Force
            cd $chocoDir

            $nuspec = get-content "hcfg.nuspec"
            $version = invoke "bin\hcfg" "--version" -passthru
            if ($version -match "version\s*(.*)") { $version = $matches[1] }
            $nuspec = $nuspec | % { $_ -replace "{version}",$version }
            [System.IO.File]::WriteAllLines((get-item "hcfg.nuspec").FullName, $nuspec, [System.Text.Encoding]::UTF8)

            invoke cpack

            # $src = $env:CHOCO_PUSH_SOURCE
            # 3invoke choco push $nupkg -source $src
        }
    }
} finally {
    popd
}

