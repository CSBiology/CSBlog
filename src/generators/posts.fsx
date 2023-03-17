#r "../_lib/Fornax.Core.dll"
#load "../globals.fsx"
#load "layout.fsx"

open Postloader
open Layout
open Html
open Globals

open System.IO
open System.Diagnostics
open System.Text.RegularExpressions

let processConvertedNotebook (content:string) =
    let nb_start_tag = """<body class="jp-Notebook" data-jp-theme-light="true" data-jp-theme-name="JupyterLab Light">"""
    let body_start_index = content.IndexOf(nb_start_tag) + nb_start_tag.Length
    let body_end_index = content.IndexOf "</body>" - 1
    let image_url = Globals.prefixUrl "img"
    content[body_start_index .. body_end_index]
        .Replace(
            "src=\"../../img",
            $"src=\"{image_url}"
        )

let fixNotebookJson (language:string) (nb_path:string) (target_path:string) =
    let content = File.ReadAllText(nb_path)
    if language = "fsharp" then
        if content.Contains("\"name\": \"polyglot-notebook\"") then
            printfn $"fixing fsharp notebook json for {nb_path} into {target_path}"
            File.WriteAllText(target_path, (content.Replace("\"name\": \"polyglot-notebook\"","\"name\": \"F#\"")))
        elif not (content.Contains("\"language_info\": {\"name\": \"F#\"}")) then
            printfn $"fixing fsharp notebook json for {nb_path} into {target_path}"
            let metadataSection = "\"metadata\": {"
            let metadata_start_index = content.LastIndexOf(metadataSection)
            File.WriteAllText(target_path, (content[0..metadata_start_index + metadataSection.Length] + "\"language_info\": {\"name\": \"F#\"}," + content[metadata_start_index + metadataSection.Length..]))
        else
            printfn $"writing unmodified notebook to {target_path}"
            File.WriteAllText(target_path, content)
    elif language = "csharp" then
        if content.Contains("\"name\": \"polyglot-notebook\"") then
            printfn $"fixing fsharp notebook json for {nb_path} into {target_path}"
            File.WriteAllText(target_path, (content.Replace("\"name\": \"polyglot-notebook\"","\"name\": \"C#\"")))
        elif not (content.Contains("\"language_info\": {\"name\": \"C#\"}")) then
            printfn $"fixing fsharp notebook json for {nb_path} into {target_path}"
            let metadataSection = "\"metadata\": {"
            let metadata_start_index = content.LastIndexOf(metadataSection)
            File.WriteAllText(target_path, (content[0..metadata_start_index + metadataSection.Length] + "\"language_info\": {\"name\": \"C#\"}," + content[metadata_start_index + metadataSection.Length..]))
        else
            printfn $"writing unmodified notebook to {target_path}"
            File.WriteAllText(target_path, content)

let anchorRegex = Regex("<a class=\"anchor-link\" href=\"(?<link>#\\S*)\"", RegexOptions.Compiled)

let getNotebookTOC (content:string) =
    let anchors =
        anchorRegex.Matches(content)
        |> Seq.map (fun x -> x.Groups.Item("link").Value)
        |> List.ofSeq

    ul [] (
        anchors
        |> List.map (fun link ->
            let title = link.Replace("-"," ").Replace("#","")
            li [] [
                a [Href link; Class "orange-link"] [!!title]
            ]
        )
    )

let generate (ctx : SiteContents) (projectRoot: string) (page: string) =

    let posts =
        ctx.TryGetValues<NotebookPost>()
        |> Option.defaultValue Seq.empty
        |> List.ofSeq

    posts
    |> List.map (fun post ->
        let full_path = post.original_path
        let tmp_path = Path.GetTempPath ()

        let fileName = Path.GetFileName(full_path)
        let html_filename = fileName.Replace("ipynb","html")

        /// save temporary notebook to convert here
        let fixed_nb_file = Path.Combine(tmp_path, fileName)
        /// save nbconvert result here temporarily
        let nbconvert_temp_output_file = Path.Combine(tmp_path, html_filename)

        fixNotebookJson "fsharp" full_path fixed_nb_file

        printfn $"[post generator]: starting jupyter --output-dir='{tmp_path}' nbconvert --to html {fixed_nb_file}"
        let psi = ProcessStartInfo()
        psi.FileName <- "jupyter"
        psi.Arguments <- $"nbconvert --output-dir='{tmp_path}' --to html {fixed_nb_file}"
        psi.CreateNoWindow <- true
        psi.WindowStyle <- ProcessWindowStyle.Hidden
        psi.UseShellExecute <- true
        try
            let proc = Process.Start psi
            proc.WaitForExit()
            let notebook_content = File.ReadAllText nbconvert_temp_output_file

            File.Delete nbconvert_temp_output_file
            File.Delete fixed_nb_file

            let processed_notebook = processConvertedNotebook notebook_content
            let toc = getNotebookTOC processed_notebook

            let metadata =
                SiteMetadata.create(
                    title = post.post_config.title,
                    description = (post.post_config.summary |> Option.defaultValue post.post_config.title),
                    ?image = (post.post_config.preview_image |> Option.map Globals.prefixUrl)
                )

            let content =
                Layout.postLayout ctx metadata post.post_config toc "Posts" [
                    div [
                        Class "content jp-Notebook"
                        HtmlProperties.Custom ("data-jp-theme-light","true")
                        HtmlProperties.Custom ("data-jp-theme-name","JupyterLab Light")
                    ] [!!processed_notebook]
                ]
                |> Layout.render ctx

            printfn $"[post generator]: generated {post.html_path}"
            (
                post.html_path,
                content
            )
        with
        | ex ->
            printfn "[post generator] EX: %s" ex.Message
            printfn "make sure to set up a conda distribution with nbconvert installed."
            "",
            ""
    )
