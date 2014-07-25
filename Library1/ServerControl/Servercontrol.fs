namespace ServerControl
open System
open System.Net
open System.Net.Sockets
open System.Text
open System.Net.NetworkInformation
open System.Collections.Generic
open UDPClient
open Collection
open TCP

type Server(?udpIpMask, ?portForUdp, ?portForTcp) =
    let TCPconnections = new Dictionary<string, TCPclient>()
    let collection = new Collection()
    let udpIpMask = defaultArg udpIpMask "192.168.1.255"
    let portForUdp = defaultArg portForUdp 3000
    let mutable portForTcp = defaultArg portForTcp 2000
    let UDPserver = new UDPClient(udpIpMask, portForUdp)
    let eventManager x =
        printfn "Event"
        if x then 
            UDPserver.ServerStop()
            Seq.iter (fun x -> printfn "%A" x) collection.GetAllMac

        else UDPserver.ServerStart()
    let macAndIpCome (x:byte[]) = 
        let getString (message:byte[]) = 
            let mutable result = ""
            for i = 0 to (message.Length - 1) do
                result <- result + message.[i].ToString()
            result
        collection.SendMacAndIp (getString (x.[0..5])) x.[6..9]
    let connectionEvent x =
        printfn "TCP CONNECTION INTERRAPTED"
        (TCPconnections.Item x).Close()
        TCPconnections.Remove(x) |> ignore
        collection.DeleteMacAndIp x (collection.GetValue x)
        UDPserver.ServerStart()
    
    let TCPconnect y = 
        let TCPclient = new TCPclient((portForTcp % 3000), collection.GetValue(y), y)
        if portForTcp = portForUdp - 1 then portForTcp <- portForTcp + 2
        else portForTcp <- portForTcp + 1
        TCPclient.Conneted.Add(connectionEvent)
        TCPconnections.Add(y, TCPclient)

    do collection.AllRobotFound.Add(eventManager)
       collection.TCPAdded.Add(TCPconnect)
       UDPserver.MACAndIPCome.Add(macAndIpCome)
       UDPserver.ServerStart()

//    member x.TCPconnect y =
//        let TCPclient = new TCPclient((portForTcp % 3000), collection.GetValue(y), y)
//        if portForTcp = portForUdp - 1 then portForTcp <- portForTcp + 2
//        else portForTcp <- portForTcp + 1
//        TCPclient.Conneted.Add(connectionEvent)
//        TCPconnections.Add(y, TCPclient)
    member x.GetMac (y:char) = collection.GetMac (Convert.ToInt32(y) - 48)
    member x.SendMessage tcp message = 
        (TCPconnections.Item(tcp)).Send message