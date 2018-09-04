module WebServer

    open Extensions
    open Data
    open Suave
    open Suave.Filters
    open Suave.Operators
    open Suave.Successful
    open Suave.Writers
    open System
    open System.IO
    open System.Net
    open System.Threading

    type Message =
        {message: string}

        member x.ToJson () =
            sprintf "{\"message\":\"%s\"}" <| String.escapeJsonString x.message

    /// Starts the WebServer asyncronically and returns a CancellationTokenSource that can be used to stop its execution
    let start () =
        let cts = new CancellationTokenSource()
        let config =
            let ipZero = IPAddress.Parse("0.0.0.0")
            {defaultConfig with
                bindings = [(HttpBinding.create HTTP ipZero (uint16 80))]
                homeFolder = Some (Path.GetFullPath "./wwwroot/")}

        let setCorsHeaders = setHeader "Allow" "GET, POST"
                             >=> setHeader "Access-Control-Allow-Methods" "GET, POST"
                             >=> setHeader "Access-Control-Allow-Origin" "*"
                             >=> setHeader "Access-Control-Allow-Headers" "Content-Type"

        let preflightReq =
            request (fun r ->
                printfn "[%s INF] Received %s request from %s at url %s. Details:\n%s\nProcessing..."
                    (DateTime.Now.ToLongTimeString()) (r.method.ToString().ToUpper()) (r.host) (r.url.ToString()) (r.ToString())
                setCorsHeaders >=> OK "")

        let req404 =
            request (fun r ->
                printfn "[%s WAR] Received request to unexisting or unrecognized path from %s at url %s. Details:\n%s\nReturning a 404 error..."
                    (DateTime.Now.ToLongTimeString()) (r.host) (r.url.ToString()) (r.ToString())
                setMimeType "application/json; charset=utf-8" >=> setCorsHeaders >=> RequestErrors.not_found (UTF8.bytes <| {message = "Error: page not found"}.ToJson()))

        let req400 =
            request (fun r ->
                printfn "[%s WAR] Received a bad request from %s at url %s. Details:\n%s\nReturning a 400 error"
                    (DateTime.Now.ToLongTimeString()) (r.host) (r.url.ToString()) (r.ToString())
                setMimeType "application/json; charset=utf-8" >=> setCorsHeaders >=> RequestErrors.bad_request (UTF8.bytes <| {message = "Error: Unrecognized method or request syntax"}.ToJson()))

        let postSubmit creator parser =
            request (fun r ->
                printfn "[%s INF] Received %s request from %s at url %s. Details:\n%s\nContent:\n%A\nProcessing..."
                    (DateTime.Now.ToLongTimeString()) (r.method.ToString().ToUpper()) (r.host) (r.url.ToString()) (r.ToString()) (fst r.form.Head)
                try
                    creator << parser <| fst r.form.Head
                    printfn "[%s INF] Success! Request fulfilled. Responding..." (DateTime.Now.ToLongTimeString())
                    setMimeType "application/json; charset=utf-8" >=> setCorsHeaders >=> ok (UTF8.bytes <| {message = "Element created correctly"}.ToJson())
                with
                | ex -> printfn "[%s ERR] Request unfulfilled. Responding...\nDetails:\n%s" (DateTime.Now.ToLongTimeString()) (ex.ToString())
                        setMimeType "application/json; charset=utf-8" >=> setCorsHeaders >=> Suave.ServerErrors.internal_error (UTF8.bytes <| {message = (ex.Message)}.ToJson()))

        let getHome =
            request (fun r ->
                printfn "[%s INF] Received %s request from %s at url %s. Details:\n%s\nProcessing..."
                    (DateTime.Now.ToLongTimeString()) (r.method.ToString().ToUpper()) (r.host) (r.url.ToString()) (r.ToString())
                setCorsHeaders >=> Files.file "./wwwroot/index.html")

        let getFile =
            request (fun r ->
                printfn "[%s INF] Received %s request from %s at url %s. Details:\n%s\nProcessing..."
                    (DateTime.Now.ToLongTimeString()) (r.method.ToString().ToUpper()) (r.host) (r.url.ToString()) (r.ToString())
                setCorsHeaders >=> Files.browseHome)

        let main : WebPart =
            choose
                [GET >=> choose
                    [path "/" >=>
                        getHome
                     pathStarts "/" >=>
                        getFile
                     req404]
                 POST >=> choose
                    [path "/api/submitEmail" >=>
                        postSubmit createEmail (Email.Parse)
                     req404]
                 OPTIONS >=>
                    preflightReq
                 req400]
        
        let _, server = startWebServerAsync config main

        Async.Start (server, cts.Token)
        printfn "Make requests now"

        cts