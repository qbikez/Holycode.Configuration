param($framework="net451")

import-module pathutils
(get-item "$psscriptroot/.tools/dotnet").FullName | add-topath

$outDir = "$psscriptroot\.build\hcfg"
$bindir = "win7-x64"
pushd 
try {
    cd $psscriptroot\src\Holycode.Configuration\
    dotnet build --configuration "cmd" --framework $framework
    if (!(test-path $outDir)) { $null = mkdir $outDir }
    cp bin\cmd\$framework\$bindir\* $outdir -Recurse -Force

if ($lastexitcode -ne 0){ exit $lastexitcode }
} finally {
    popd
}