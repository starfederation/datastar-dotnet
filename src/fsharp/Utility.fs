namespace StarFederation.Datastar.FSharp

module internal Utility =
    open System
    open System.Text
    open Microsoft.Extensions.Primitives

    module internal String =
        let dotSeparator = [| '.' |]
        let newLines = [| "\r\n"; "\n"; "\r" |]
        let newLineChars = [| '\r'; '\n' |]

        // New zero-allocation version using StringTokenizer
        let inline splitLinesToSegments (text: string) =
            StringTokenizer(text, newLineChars)
            |> Seq.filter (fun segment -> segment.Length > 0)

        let isPopulated = (String.IsNullOrWhiteSpace >> not)

        let toKebab (pascalString: string) =
            let sb = StringBuilder(pascalString.Length * 2)
            let chars = pascalString.ToCharArray()

            for i = 0 to chars.Length - 1 do
                let chr = chars.[i]

                if Char.IsUpper(chr) && i > 0 then
                    sb.Append('-').Append(Char.ToLower(chr)) |> ignore
                else
                    sb.Append(Char.ToLower(chr)) |> ignore

            sb.ToString()
