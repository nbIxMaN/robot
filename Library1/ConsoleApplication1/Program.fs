open System
open ServerControl

let udpClient = new Server()
let rec loop() =
    let command = (System.Console.ReadLine())
    if command <> "exit" then
        let robot = udpClient.GetMac (command.[0])
//        printfn "%A" (Convert.ToInt32(command))
        udpClient.SendMessage robot command.[2..(command.Length-1)]
        //printfn "%A" System.Console.ReadKey
        loop()
loop()
System.Console.ReadLine() |> ignore