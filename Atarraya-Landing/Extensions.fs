module Extensions

    module String =

        let escapeJsonString str =
            str |> String.map (fun c -> match c with
                                        | c when c = '"' -> '\''
                                        | c              -> c)
                |> String.filter ((fun c -> (c = '\n' || c = '\r')) >> not)

        /// Converts a string into a list of characters.
        let explode (s:string) =
            [for c in s -> c]

        /// Converts a list of characters into a string.
        let implode (xs:char list) =
            let sb = System.Text.StringBuilder(xs.Length)
            xs |> List.iter (sb.Append >> ignore)
            sb.ToString()