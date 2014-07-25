namespace Collection
open System.Collections.Generic

type Command =
    | Add
    | Delete

type Message(command:Command, mac: string, ip) =
    member x.Command = command
    member x.Mac = mac
    member x.Ip = ip
//type MyComparator() =
//    inherit IEqualityComparer<byte[]>
//type MyComparator : IEqualityComparer<byte> = 
//     interface IEqualityComparer<byte> with
//         member x.Equals (y,z)= true
//         member x.GetHashCode y = 1
type Collection(?number) =
    let number = defaultArg number 1
    let AllRobotsFound_src = new Event<bool>()
    let AllRobotsFound = AllRobotsFound_src.Publish
    let AllRobotsFounded = AllRobotsFound_src.Trigger
    let TCPadded_src = new Event<string>()
    let TCPadded = TCPadded_src.Publish
    let TCPadd = TCPadded_src.Trigger
    let rec contains list array = 
        match list with
        | head::tail -> if array = head then true
                        else contains tail array
        | [] -> false
    let IPlist = new Dictionary<string, byte[]>()
    let mailBox = new MailboxProcessor<Message>(fun x ->
        let rec loop() = async{
            let! message = x.Receive()
            printfn "this %A" message
            match message.Command with
            |Add -> if not (contains (Seq.toList (IPlist.Keys)) (message.Mac)) then
                        IPlist.Add ((message.Mac), (message.Ip))
                        Seq.iter (fun x -> printfn " %A" x ) IPlist.Keys
                        printfn "add %A" message.Mac
                        printfn "         %A" IPlist.Count
                        System.Threading.Thread.Sleep(1000)
                        printfn "%A" (IPlist.Item  (Seq.last(IPlist.Keys)))
                        if IPlist.Count = number then
                            printfn "egeregdfgdfgdgf" 
                            AllRobotsFounded true
                        TCPadd message.Mac
            |Delete -> if IPlist.Remove(message.Mac) then printfn "Deleted"
                                                                else printfn "Key not found"
                       AllRobotsFounded false
            return! loop()
        }
        loop())
    do AllRobotsFounded false
       mailBox.Start()

    member x.SendMacAndIp mac ip = mailBox.Post(new Message(Add, mac, ip))
    member x.DeleteMacAndIp mac ip = mailBox.Post(new Message(Delete, mac, ip))
    member x.GetValue key = IPlist.Item(key)
    member x.GetMac y =
        let mutable enumerator = IPlist.Keys.GetEnumerator()
        for i = 1 to y do
            enumerator.MoveNext() |> ignore
        enumerator.Current
    member x.GetAllMac = IPlist.Keys
    member x.AllRobotFound = AllRobotsFound
    member x.TCPAdded = TCPadded