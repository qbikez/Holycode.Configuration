$o = hcfg.exe @args

try {
    $cfg = $o | out-string | ConvertFrom-Json 
    return $cfg.psobject.members | ? { $_.MemberType -eq "NoteProperty" } | select Name,Value
    
} catch {
    return $o
}