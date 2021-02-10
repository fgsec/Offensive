# SERVER.ps1

$server = { 
    Param($pipe_name)

    write-host "Starting Named Pipe Server" -f gray
    $pipe = new-object System.IO.Pipes.NamedPipeServerStream("\\.\pipe\$pipe_name");
    write-host "[Server] Running..." -f green
    $pipe.WaitForConnection(); 
    $sreader = new-object System.IO.StreamReader($pipe); 
    while (( $output = $sreader.ReadLine()) -ne 'end') {
        write-host "[Server] Received: $output" -f Yellow
    }; 
    $sreader.Dispose();
    $pipe.Dispose();
}
Invoke-Command $server -ArgumentList "PIPENAME"



# CLIENT.ps1 

$client = { 
    Param($pipe_name, $message)
    
    write-host "Starting Named Pipe Client" -f gray
    $pipe = new-object System.IO.Pipes.NamedPipeClientStream("\\.\pipe\$pipe_name");
    $pipe.Connect(); 
    $swriter = new-object System.IO.StreamWriter($pipe);
    $swriter.WriteLine($message); 
    $swriter.WriteLine("end"); 
    $swriter.Dispose(); 
    $pipe.Dispose();
} 

Invoke-Command $client -ArgumentList "PIPENAME","SUPER SECRET MESSAGE"
