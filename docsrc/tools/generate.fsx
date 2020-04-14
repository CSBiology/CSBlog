// --------------------------------------------------------------------------------------
// Builds the documentation from `.fsx` and `.md` files in the 'docs/content' directory
// (the generated documentation is stored in the 'docs/output' directory)
// --------------------------------------------------------------------------------------

// --------------------------------------------------------------------------------------
// Helpers
// --------------------------------------------------------------------------------------

#I __SOURCE_DIRECTORY__
#I "../../packages/FSharp.Formatting/lib/netstandard2.0"
#I @"..\..\packages\FSharp.Compiler.Service\lib\netstandard2.0\"
#r @"..\..\packages\FSharp.Compiler.Service\lib\netstandard2.0\FSharp.Compiler.Service.dll"
#r "FSharp.CodeFormat.dll"
#r "FSharp.Literate.dll"
#r "FSharp.Markdown.dll"
#r "FSharp.MetadataFormat.dll"
#r "FSharp.Formatting.Common.dll"
#r "Microsoft.AspNetCore.Razor.dll"
#r "Microsoft.AspNetCore.Razor.Runtime.dll"
#r "Microsoft.AspNetCore.Razor.Language.dll"
#r "RazorEngine.NetCore.dll"
#r "FSharp.Formatting.Razor.dll"

let website = "/CSBlog"

open System.IO
open FSharp.Formatting.Razor
open System.Collections.Generic

let subDirectories (dir: string) = Directory.EnumerateDirectories dir 

let rec copyRecursive dir1 dir2 =
    Directory.CreateDirectory dir2 |> ignore
    for subdir1 in Directory.EnumerateDirectories dir1 do
         let subdir2 = Path.Combine(dir2, Path.GetFileName subdir1)
         copyRecursive subdir1 subdir2
    for file in Directory.EnumerateFiles dir1 do
         File.Copy(file, file.Replace(dir1, dir2), true)
// Web site location for the generated documentation

// Specify more information about your project
let info =
  [ "project-name", "CSBlog"
    "project-author", "Kevin Schneider, Heinrich Lukas Weil"
    "project-summary", "The Blog about all things CSB"
    "project-github", "https://github.com/CSBiology/CSBlog"]

#load "formatters.fsx"

// When called from 'build.fsx', use the public project URL as <root>
// otherwise, use the current 'output' directory.
#if RELEASE
let root = website
#else
let root = "file://" + (__SOURCE_DIRECTORY__ + "/../../docs/output")
#endif

// Paths with template/source/output locations
let content    = __SOURCE_DIRECTORY__ + "/../content"
let output     = __SOURCE_DIRECTORY__ + "/../../docs/output"
let files      = __SOURCE_DIRECTORY__ + "/../files"
let templates  = __SOURCE_DIRECTORY__ + "/templates"
let formatting = __SOURCE_DIRECTORY__ + "/../../packages/FSharp.Formatting/"
let docTemplate = formatting + "/templates/docpage.cshtml"
let referenceOut = (output + "/reference")
    
// Where to look for *.csproj templates (in this order)
let layoutRoots =
    [ templates; formatting + "templates";
    formatting + "/templates/reference" ]
let layoutRootsAll = Dictionary<string, string list>()
layoutRootsAll.Add("en",layoutRoots)
    
let copyFiles () = copyRecursive files output

// Build documentation from `fsx` and `md` files in `docs/content`
let buildDocumentation () =
    printfn "building docs..."
    let subdirs = [ content, docTemplate ]
    let fsiEvaluator = Formatters.createFsiEvaluator root output
    for dir, template in subdirs do
        let sub = "." // Everything goes into the same output directory here
        let langSpecificPath(lang, path:string) =
            path.Split([|'/'; '\\'|], System.StringSplitOptions.RemoveEmptyEntries)
            |> Array.exists(fun i -> i = lang)
        let layoutRoots =
            let key = layoutRootsAll.Keys |> Seq.tryFind (fun i -> langSpecificPath(i, dir))
            match key with
            | Some lang -> layoutRootsAll.[lang]
            | None -> layoutRootsAll.["en"] // "en" is the default language
        RazorLiterate.ProcessDirectory
            ( dir, template, output + "/" + sub, replacements = ("root", root)::info,
            layoutRoots = layoutRoots,
            generateAnchors = true,
            processRecursive = false,
            includeSource = true,
            fsiEvaluator = fsiEvaluator
            )
// Generate
copyFiles()
buildDocumentation()