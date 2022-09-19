#r "../_lib/Fornax.Core.dll"
open System.IO
open System.Text
open System.Text.RegularExpressions

let titleRegex = Regex("\"#(?<header>.*?)\"")

type Notebook = {
    filename: string
    title:string
    category: string
    htmlPath: string
    originalPath: string
} with
    static member create(filename, title, category, htmlpath, ogpath) =
        {
            filename = filename
            title = title
            category = category
            htmlPath = htmlpath
            originalPath = ogpath
        }

let loader (projectRoot: string) (siteContent: SiteContents) =
    let notebookRootPath = Path.Combine(projectRoot, "posts")

    Directory.GetFiles(notebookRootPath,"*.ipynb",SearchOption.AllDirectories)
    |> Array.iter (fun (filename:string) ->
        let splt = filename.Replace(notebookRootPath, "").Split([|'\\';'/'|]) |> Array.skip 1
        if splt.Length <> 2 then
            failwith $"""notebookpath is wrong wtf?: {filename} => {splt |> String.concat "/"}"""
        else
            let title =
                let m = titleRegex.Match(File.ReadAllText(filename))
                if m.Success then
                    m.Groups.Item("header").Value.Replace(@"\n","")
                else
                    Path.GetFileNameWithoutExtension(filename)

            let [|category; filename|] = splt

            let nb =
                Notebook.create(
                    filename = filename,
                    category = category,
                    title = title,
                    ogpath = Path.Combine([|notebookRootPath; yield! splt|]),
                    htmlpath = (["";"posts"; yield! splt] |> String.concat "/").Replace("ipynb","html")
                )
            siteContent.Add(nb)
            printfn $"loaded notebook: {nb.category}/{nb.filename}"
    )

    siteContent
