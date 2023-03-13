#r "../_lib/Fornax.Core.dll"
#load "../globals.fsx"
#load "layout.fsx"
#if !FORNAX
#load "../loaders/postloader.fsx"
#endif

open Postloader
open Layout
open Html

open System.IO
open System.Diagnostics
open System.Text.RegularExpressions

let processConvertedNotebook (content:string) =
    let nb_start_tag = """<body class="jp-Notebook" data-jp-theme-light="true" data-jp-theme-name="JupyterLab Light">"""
    let body_start_index = content.IndexOf(nb_start_tag) + nb_start_tag.Length
    let body_end_index = content.IndexOf "</body>" + 1
    content[body_start_index .. body_end_index]

let fixNotebookJson (nb_path:string) =
    let content = File.ReadAllText(nb_path)
    if not (content.Contains("\"language_info\": {\"name\": \"F#\"}")) then
        printfn $"fixing notebook json for {nb_path}"
        let metadataSection = "\"metadata\": {"
        let metadata_start_index = content.LastIndexOf(metadataSection)
        File.WriteAllText(nb_path, (content[0..metadata_start_index + metadataSection.Length] + "\"language_info\": {\"name\": \"F#\"}," + content[metadata_start_index + metadataSection.Length..]))

let anchorRegex = Regex("<a class=\"anchor-link\" href=\"(?<link>#\\S*)\"", RegexOptions.Compiled)
let getTOC (content:string) =
    let anchors =
        anchorRegex.Matches(content)
        |> Seq.map (fun x -> x.Groups.Item("link").Value)
        |> List.ofSeq

    ul [] (
        anchors
        |> List.map (fun link ->
            let title = link.Replace("-"," ").Replace("#","")
            li [] [
                a [Href link; Class "is-csb-darkblue"] [!!title]
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
        let tmp_output = Path.GetFileName(full_path).Replace("ipynb","html")
        let output_path = Path.Combine(tmp_path, tmp_output)

        full_path |> fixNotebookJson

        printfn $"[post generator]: starting jupyter --output-dir='{tmp_path}' nbconvert --to html {full_path}"
        let psi = ProcessStartInfo()
        psi.FileName <- "jupyter"
        psi.Arguments <- $"nbconvert --output-dir='{tmp_path}' --to html {full_path}"
        psi.CreateNoWindow <- true
        psi.WindowStyle <- ProcessWindowStyle.Hidden
        psi.UseShellExecute <- true
        try
            let proc = Process.Start psi
            proc.WaitForExit()
            let notebook_content = File.ReadAllText output_path
            File.Delete output_path

            let processed_notebook = processConvertedNotebook notebook_content
            let toc = getTOC processed_notebook

            let content =
                Layout.postLayout ctx post.post_config toc "Posts" [
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
            Layout.layout ctx "" List.empty
            |> Layout.render ctx
    )
