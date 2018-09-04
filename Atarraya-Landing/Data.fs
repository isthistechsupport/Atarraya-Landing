module Data

    open FSharp.Data
    open System.IO

    type Email = JsonProvider<"""{"email":"test1@test.com"}""">

    let createEmail (email : Email.Root) =        
        if FileInfo("emails.dat").Length > 100L || (UTF8.bytes email.Email).Length > 100 then
            failwith "This demo site only allows for databases of up to 100 bytes. This is not a production site. Please refer to the site vendor for assistance."
        else
            System.IO.File.AppendText("emails.dat").WriteLine(email.Email)
