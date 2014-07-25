namespace TCP
open System
open System.Net
open System.Net.Sockets
open System.Net.NetworkInformation
open System.Text

type TCPclient(port, IP:byte[], mac:string) = 
    let connectionInterrupted_src = new Event<string>()
    let connectionInterrapted_p = connectionInterrupted_src.Publish
    let connectionInterrupted = connectionInterrupted_src.Trigger
    let parse (str:string) = 
        str.Split(';') |> Array.map Byte.Parse
    let client = new TcpClient()
    let rec loop() = async{
        try
            if not client.Connected then
                client.Connect(IPAddress(IP), port)
                return! loop()
        with
        | _ ->
            printfn "Exeption" 
            connectionInterrupted (mac)}
    do Async.Start(loop())
       printfn "connected"
        
    member x.Send message = 
        let command = message |> parse
        client.GetStream().Write(command, 0, command.Length)
    member x.Close() = client.Close()
    member x.Conneted = connectionInterrapted_p
