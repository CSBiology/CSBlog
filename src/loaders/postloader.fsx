#r "../_lib/Fornax.Core.dll"
#r "../_lib/Markdig.dll"
#load "../globals.fsx"
open System.IO
open Markdig

type PostCategory =
| Progress
| StudyNote
| Advanced
| Other of string

    static member ofString (s:string) =
        match s.Trim().ToLower() with
        | "progress" -> Progress
        | "studynote" -> StudyNote
        | "advanced" -> Advanced
        | _ -> Other s

    static member toString (pc: PostCategory) =
        match pc with
        | Progress      -> "Progress Reports"
        | StudyNote -> "Study Notes"
        | Advanced    -> "Advanced"
        | Other o     -> o

    static member getDescription (pc: PostCategory) =
        match pc with
        | Progress -> "Tell others what you are working on."
        | StudyNote -> "Short, informational posts on a wide range of topics"
        | Advanced    -> "Detailed, well-researched posts. These posts often include thesis summaries."
        | Other o     -> o

type PostConfig = {
    title: string
    author: string
    author_link: string
    category: PostCategory
    date: System.DateTime
    preview_image: string option
    summary: string option
} with
    static member create(title, author, author_link, category, date, ?preview_image, ?summary) =
        {
            title = title
            author = author
            author_link = author_link
            category = category
            date = date
            preview_image = preview_image
            summary = summary
        }
    static member ofMap (source:string) (m:Map<string,string>) =

        let mandatoryFieldMissing (fieldName: string) (source:string) (o:string Option) = if o.IsNone then failwith $"missing field {fieldName} in config from {source}" else o.Value

        let title =
            m
            |> Map.tryFind "title"
            |> mandatoryFieldMissing "title" source
            |> fun t ->
                let t = if t.StartsWith("\"") then t[1..] else t
                let t = if t.EndsWith("\"") then t[0..(t.Length-2)] else t
                t

        let author = m.TryFind "author" |> mandatoryFieldMissing "author" source
        let author_link = m.TryFind "author_link" |> mandatoryFieldMissing "author_link" source
        let category =
            let tmp = m.TryFind "category" |> mandatoryFieldMissing "category" source
            try
                PostCategory.ofString tmp
            with _ ->
                failwith $"wrong post category format in config from {source}"
        let date =
            let tmp = m.TryFind "date" |> mandatoryFieldMissing "date" source
            try
                System.DateTime.ParseExact(tmp,"yyyy-MM-dd",System.Globalization.CultureInfo.InvariantCulture)
            with _ ->
                failwith $"wrong date format in config from {source}, make sure to use YYY-MM-DD"

        let preview_image = m |> Map.tryFind "preview_image"
        let summary = m |> Map.tryFind "summary"

        PostConfig.create(
            title = title,
            author = author,
            author_link = author_link,
            category = category,
            date = date,
            ?preview_image = preview_image,
            ?summary = summary
        )



let markdownPipeline =
    MarkdownPipelineBuilder()
        .UsePipeTables()
        .UseGridTables()
        .Build()

let isSeparator (input : string) =
    input.StartsWith "---"

///`fileContent` - content of page to parse. Usually whole content of `.md` file
///returns content of config that should be used for the page
let getConfig (fileContent : string) =
    let fileContent = fileContent.ReplaceLineEndings(System.Environment.NewLine) // normalize line endings
    let fileContent = fileContent.Split System.Environment.NewLine
    let fileContent = fileContent |> Array.skip 1 //First line must be ---
    let indexOfSeperator = fileContent |> Array.findIndex isSeparator
    let splitKey (line: string) =
        let seperatorIndex = line.IndexOf(':')
        if seperatorIndex > 0 then
            let key = line.[.. seperatorIndex - 1].Trim().ToLower()
            let value = line.[seperatorIndex + 1 ..].Trim()
            Some(key, value)
        else
            None
    fileContent
    |> Array.splitAt indexOfSeperator
    |> fst
    |> Seq.choose splitKey
    |> Map.ofSeq

type NotebookPost = {
    file_name: string
    post_config: PostConfig
    html_path: string
    original_path: string
} with
    static member create(file_name, post_config, html_path, original_path) =
        {
            file_name = file_name
            post_config = post_config
            html_path = html_path
            original_path = original_path
        }

let loader (projectRoot: string) (siteContent: SiteContents) =
    let notebookRootPath = Path.Combine(projectRoot, "posts")

    Directory.EnumerateDirectories(notebookRootPath)
    |> Array.ofSeq
    |> Array.iter (fun (post_folder:string) ->

        let post_path = Path.Combine(post_folder,"post.ipynb")
        let config_path = Path.Combine(post_folder,"post_config.md")

        if not (File.Exists(post_path)) then failwith $"no post.ipynb in {post_folder}"
        if not (File.Exists(config_path)) then failwith $"no post_config.md in {post_folder}"

        let post_config =
            config_path
            |> File.ReadAllText
            |> getConfig
            |> PostConfig.ofMap config_path

        let postName =
            post_path
                .Replace(notebookRootPath, "")
                .Replace("\\","/")
                .Replace("/post.ipynb",".html")
                .Replace("/","")

        let post =
            NotebookPost.create(
                file_name = postName,
                post_config = post_config,
                html_path = Path.Combine([|projectRoot; "_public"; "posts"; postName|]),
                original_path = post_path
            )

        // printfn "%A" post

        siteContent.Add(post)
        printfn $"[post loader] loaded post from {post.original_path}"
    )

    siteContent
