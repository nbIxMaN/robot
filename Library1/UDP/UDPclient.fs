namespace UDPClient
open System
open System.Net
open System.Net.Sockets
open System.Text
open System.Net.NetworkInformation
open System.Collections.Generic
open Collection

type UDPClient(ip, port) = 
    let macAndIpCome_src = new Event<byte[]>()
    let macAndIpCome = macAndIpCome_src.Publish
    let macAndIpCome_t = macAndIpCome_src.Trigger
    let requestIP = Encoding.ASCII.GetBytes("requestIP")
    let listenerEndPoint = new IPEndPoint(IPAddress.Any, port)
    let senderEndPoint = new IPEndPoint(IPAddress.Parse ip, port)
    let mutable (Udp : UdpClient option) = None
    let mutable working = false
    let rec ipRequest() = async{
        if working then
            Udp.Value.Send(requestIP, requestIP.Length, senderEndPoint) |> ignore
            System.Threading.Thread.Sleep(1000)
            return! ipRequest()
        }
    let rec getMessage() =
        try
        let message = Udp.Value.Receive (ref listenerEndPoint)
        if message.Length = 10 then 
            macAndIpCome_t message
        else
            printfn "%A" message
//            getMessage()
        with | _ -> printfn "Recieve break by close, nothing serious" 
    let rec loop() = async{
        if working then
            //ipRequest()
            getMessage()
            return! loop()}
    member x.MACAndIPCome = macAndIpCome
    member x.ServerStart() = match Udp with
                             | None -> Udp <- Some (new UdpClient(port))
                                       working <- true
                                       Async.Start(loop())
                                       Async.Start(ipRequest())
                             | _ -> printfn "UDP already running"
    member x.ServerStop() = match Udp with
                             | None -> printfn "useless trying stop server"
                             | Some server -> printfn "UDP begin stoped"
                                              working <- false
                                              try
                                                server.Close()
                                              with
                                                | _ -> printfn "EXCEPTION in UdpClient.Close()"
                                              Udp <- None
                                              printfn "UDP successfully stoped"