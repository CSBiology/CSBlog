// --------------------------------------------------------------------------------------
// FAKE build script
// --------------------------------------------------------------------------------------

#r "paket: groupref FakeBuild //"

#load "./.fake/build.fsx/intellisense.fsx"

open System.IO
open Fake.Core
open Fake.Core.TargetOperators
open Fake.DotNet
open Fake.IO
open Fake.IO.FileSystemOperators
open Fake.IO.Globbing
open Fake.IO.Globbing.Operators
open Fake.DotNet.Testing
open Fake.Tools
open Fake.Api
open Fake.Tools.Git

[<AutoOpen>]
module TemporaryDocumentationHelpers =

    type LiterateArguments =
        { ToolPath : string
          Source : string
          OutputDirectory : string 
          Template : string
          ProjectParameters : (string * string) list
          LayoutRoots : string list 
          FsiEval : bool }


    let private run toolPath command = 
        if 0 <> Process.execSimple ((fun info ->
                { info with
                    FileName = toolPath
                    Arguments = command }) >> Process.withFramework) System.TimeSpan.MaxValue

        then failwithf "FSharp.Formatting %s failed." command

    let createDocs p =
        let toolPath = Tools.findToolInSubPath "fsformatting.exe" (Directory.GetCurrentDirectory() @@ "lib/Formatting")

        let defaultLiterateArguments =
            { ToolPath = toolPath
              Source = ""
              OutputDirectory = ""
              Template = ""
              ProjectParameters = []
              LayoutRoots = [] 
              FsiEval = false }

        let arguments = (p:LiterateArguments->LiterateArguments) defaultLiterateArguments
        let layoutroots =
            if arguments.LayoutRoots.IsEmpty then []
            else [ "--layoutRoots" ] @ arguments.LayoutRoots
        let source = arguments.Source
        let template = arguments.Template
        let outputDir = arguments.OutputDirectory
        let fsiEval = if arguments.FsiEval then [ "--fsieval" ] else []

        let command = 
            arguments.ProjectParameters
            |> Seq.map (fun (k, v) -> [ k; v ])
            |> Seq.concat
            |> Seq.append 
                   (["literate"; "--processdirectory" ] @ layoutroots @ [ "--inputdirectory"; source; "--templatefile"; template; 
                      "--outputDirectory"; outputDir] @ fsiEval @ [ "--replacements" ])
            |> Seq.map (fun s -> 
                   if s.StartsWith "\"" then s
                   else sprintf "\"%s\"" s)
            |> String.separated " "
        run arguments.ToolPath command
        printfn "Successfully generated docs for %s" source

// --------------------------------------------------------------------------------------
// START TODO: Provide project-specific details below
// --------------------------------------------------------------------------------------

// Information about the project are used
//  - for version and project name in generated AssemblyInfo file
//  - by the generated NuGet package
//  - to run tests and to publish documentation on GitHub gh-pages
//  - for documentation, you also need to edit info in "docsrc/tools/generate.fsx"

// The name of the project
// (used by attributes in AssemblyInfo, name of a NuGet package and directory in 'src')
let project = "CSBlog"

// Short summary of the project
// (used as description in AssemblyInfo and as a short summary for NuGet package)
let summary = "The blog about all things CSB"

// Longer description of the project
// (used as a description for NuGet package; line breaks are automatically cleaned up)
let description = "The blog about all things CSB"

// List of author names (for NuGet package)
let author = "HLWeil"

// Tags for your project (for NuGet package)
let tags = ""

// File system information
let solutionFile  = "CSBlog.sln"

// Default target configuration
let configuration = "Release"

// Pattern specifying assemblies to be tested using Expecto
let testAssemblies = "tests/**/bin" </> configuration </> "**" </> "*Tests.exe"

// Git configuration (used for publishing documentation in gh-pages branch)
// The profile where the project is posted
let gitOwner = "CSBiology"
let gitHome = sprintf "%s/%s" "https://github.com" gitOwner

// The name of the project on GitHub
let gitName = "CSBlog"

// The url for the raw files hosted
let gitRaw = Environment.environVarOrDefault "gitRaw" "https://raw.githubusercontent.com/CSBiology"

let website = "/CSBlog"

// --------------------------------------------------------------------------------------
// END TODO: The rest of the file includes standard build steps
// --------------------------------------------------------------------------------------
// Read additional information from the release notes document
let release = ReleaseNotes.load "RELEASE_NOTES.md"

// --------------------------------------------------------------------------------------
// Clean build results

let buildConfiguration = DotNet.Custom <| Environment.environVarOrDefault "configuration" configuration

Target.create "Clean" (fun _ ->
    Shell.cleanDirs ["bin"; "temp"]
)

Target.create "CleanDocs" (fun _ ->
    Shell.cleanDirs ["docs"]
)

// --------------------------------------------------------------------------------------
// Generate the documentation

// Paths with template/source/output locations
let content    = __SOURCE_DIRECTORY__ @@ "docsrc/content"
let output     = __SOURCE_DIRECTORY__ @@ "docs"
let files      = __SOURCE_DIRECTORY__ @@ "docsrc/files"
let templates  = __SOURCE_DIRECTORY__ @@ "docsrc/tools/templates"
let formatting = __SOURCE_DIRECTORY__ @@ "docsrc/tools"
let docTemplate = "docpage.cshtml"

let github_release_user = Environment.environVarOrDefault "github_release_user" gitOwner
let githubLink = sprintf "https://github.com/%s/%s" github_release_user gitName

// Specify more information about your project
let info =
  [ "project-name", "CSBlog"
    "project-author", "Heinrich Lukas Weil"
    "project-summary", "The blog about all things CSB"
    "project-github", githubLink
    "project-nuget", "http://nuget.org/packages/CSBlog" ]

let root = website

let layoutRootsAll = new System.Collections.Generic.Dictionary<string, string list>()
layoutRootsAll.Add("en",[   templates;
                            formatting @@ "templates"
                            formatting @@ "templates/reference" ])

let copyFiles () =
    Shell.copyRecursive files output true
    |> Trace.logItems "Copying file: "
    Directory.ensure (output @@ "content")
    Shell.copyRecursive (formatting @@ "styles") (output @@ "content") true
    |> Trace.logItems "Copying styles and scripts: "

Target.create "Docs" (fun _ ->
    File.delete "docsrc/content/release-notes.md"
    Shell.copyFile "docsrc/content/" "RELEASE_NOTES.md"
    Shell.rename "docsrc/content/release-notes.md" "docsrc/content/RELEASE_NOTES.md"

    File.delete "docsrc/content/license.md"
    Shell.copyFile "docsrc/content/" "LICENSE.txt"
    Shell.rename "docsrc/content/license.md" "docsrc/content/LICENSE.txt"


    DirectoryInfo.getSubDirectories (DirectoryInfo.ofPath templates)
    |> Seq.iter (fun d ->
                    let name = d.Name
                    if name.Length = 2 || name.Length = 3 then
                        layoutRootsAll.Add(
                                name, [templates @@ name
                                       formatting @@ "templates"
                                       formatting @@ "templates/reference" ]))
    copyFiles ()

    for dir in  [ content; ] do
        let langSpecificPath(lang, path:string) =
            path.Split([|'/'; '\\'|], System.StringSplitOptions.RemoveEmptyEntries)
            |> Array.exists(fun i -> i = lang)
        let layoutRoots =
            let key = layoutRootsAll.Keys |> Seq.tryFind (fun i -> langSpecificPath(i, dir))
            match key with
            | Some lang -> layoutRootsAll.[lang]
            | None -> layoutRootsAll.["en"] // "en" is the default language

        createDocs (fun args ->
            { args with
                Source = content
                OutputDirectory = output
                LayoutRoots = layoutRoots
                ProjectParameters  = ("root", root)::info
                Template = docTemplate 
                FsiEval = true
                } )
)

// --------------------------------------------------------------------------------------
// Release Scripts

//#load "paket-files/fsharp/FAKE/modules/Octokit/Octokit.fsx"
//open Octokit
Target.create "ReleaseDocs" (fun _ ->
    let tempDocsDir = "temp/gh-pages"
    Shell.cleanDir tempDocsDir |> ignore
    Git.Repository.cloneSingleBranch "" (gitHome + "/" + gitName + ".git") "gh-pages" tempDocsDir
    Shell.copyRecursive "docs" tempDocsDir true |> printfn "%A"
    Git.Staging.stageAll tempDocsDir
    Git.Commit.exec tempDocsDir (sprintf "Update generated documentation for version %s" release.NugetVersion)
    Git.Branches.push tempDocsDir
)

Target.create "ReleaseLocal" (fun _ ->
    let tempDocsDir = "temp/local"
    Shell.cleanDir tempDocsDir |> ignore
    Shell.copyRecursive "docs" tempDocsDir true  |> printfn "%A"
    Shell.replaceInFiles 
        (seq {
            yield "href=\"/" + project + "/","href=\""
            yield "src=\"/" + project + "/","src=\""}) 
        (Directory.EnumerateFiles tempDocsDir |> Seq.filter (fun x -> x.EndsWith(".html")))
)

Target.create "GenerateDocs" ignore

// --------------------------------------------------------------------------------------
// Run all targets by default. Invoke 'build <Target>' to override


"Clean"
  ==> "CleanDocs"
  ==> "Docs"
  ==> "ReleaseLocal"
  ==> "ReleaseDocs"

Target.runOrDefaultWithArguments "ReleaseLocal"