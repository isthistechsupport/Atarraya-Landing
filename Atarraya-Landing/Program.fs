open System

[<EntryPoint>]
let main argv = 
    let cts = WebServer.start ()

    Console.ReadKey true |> ignore

    cts.Cancel()

    0 // return an integer exit code
