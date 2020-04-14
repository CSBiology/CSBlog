// --------------------------------------------------------------------------------------
// FAKE build script
// --------------------------------------------------------------------------------------

#r "paket:
nuget BlackFox.Fake.BuildTask
nuget Fake.Core.Target
nuget Fake.Core.Process
nuget Fake.Core.ReleaseNotes
nuget Fake.IO.FileSystem
nuget Fake.DotNet.Cli
nuget Fake.DotNet.MSBuild
nuget Fake.DotNet.AssemblyInfoFile
nuget Fake.DotNet.Paket
nuget Fake.DotNet.FSFormatting
nuget Fake.DotNet.Fsi
nuget Fake.DotNet.NuGet
nuget Fake.Api.Github
nuget Fake.DotNet.Testing.Expecto //"

#load ".fake/build.fsx/intellisense.fsx"

open BlackFox.Fake
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
module MessagePrompts =

    let prompt (msg:string) =
        System.Console.Write(msg)
        System.Console.ReadLine().Trim()
        |> function | "" -> None | s -> Some s
        |> Option.map (fun s -> s.Replace ("\"","\\\""))

    let rec promptYesNo msg =
        match prompt (sprintf "%s [Yn]: " msg) with
        | Some "Y" | Some "y" -> true
        | Some "N" | Some "n" -> false
        | _ -> System.Console.WriteLine("Sorry, invalid answer"); promptYesNo msg

    let releaseMsg = """This will stage all uncommitted changes, push them to the origin and bump the release version to the latest number in the RELEASE_NOTES.md file. 
        Do you want to continue?"""

    let releaseDocsMsg = """This will push the docs to gh-pages. Remember building the docs prior to this. Do you want to continue?"""

Target.initEnvironment ()

let project = "CSBlog"
let gitOwner = "CSBiology"
let gitHome = sprintf "%s/%s" "https://github.com" gitOwner
let gitName = "CSBlog"
let website = "/CSBlog"


let cleanDocs = 
    BuildTask.create "cleanDocs" [] {
        Shell.cleanDirs ["docs";"temp"]
    }
// --------------------------------------------------------------------------------------
// Release Scripts

let generateLocalDocumentation = 
    BuildTask.create "generateLocalDocumentation" [cleanDocs] {
        let result =
            DotNet.exec
                (fun p -> { p with WorkingDirectory = __SOURCE_DIRECTORY__ @@ "docsrc" @@ "tools" })
                "fsi"
                "--define:REFERENCE --define:HELP --exec generate.fsx"

        if not result.OK then 
            failwith "error generating docs" 
    }

let generateReleaseDocumentation = 
    BuildTask.create "generateDocumentation" [cleanDocs] {
        let result =
            DotNet.exec
                (fun p -> { p with WorkingDirectory = __SOURCE_DIRECTORY__ @@ "docsrc" @@ "tools" })
                "fsi"
                "--define:REFERENCE --define:RELEASE --define:HELP --exec generate.fsx"

        if not result.OK then 
            failwith "error generating docs" 
    }

//#load "paket-files/fsharp/FAKE/modules/Octokit/Octokit.fsx"
//open Octokit
let askForDocReleaseConfirmation =
    BuildTask.create "askForDocReleaseConfirmation" [] {
        match promptYesNo releaseDocsMsg with | true -> () |_ -> failwith "Release canceled"
    }

let releaseDocsToGhPages = 
    BuildTask.create "releaseDocsToGhPages" [askForDocReleaseConfirmation;generateReleaseDocumentation] {
        let tempDocsDir = "temp/gh-pages"
        Shell.cleanDir tempDocsDir |> ignore
        Git.Repository.cloneSingleBranch "" (gitHome + "/" + gitName + ".git") "gh-pages" tempDocsDir
        Shell.copyRecursive "docs/output" tempDocsDir true |> printfn "%A"
        Git.Staging.stageAll tempDocsDir
        Git.Commit.exec tempDocsDir (sprintf "[%s:%s]: Fresh content" (System.DateTime.UtcNow.ToShortDateString()) (System.DateTime.UtcNow.ToShortTimeString()))
        Git.Branches.push tempDocsDir
    }

let buildLocalDocs = 
    BuildTask.create "buildLocalDocs" [generateLocalDocumentation] {
        let tempDocsDir = "temp/localDocs"
        Shell.cleanDir tempDocsDir |> ignore
        Shell.copyRecursive "docs/output" tempDocsDir true  |> printfn "%A"
        Shell.replaceInFiles 
            (seq {
                yield "href=\"/" + project + "/","href=\""
                yield "src=\"/" + project + "/","src=\""}) 
            (Directory.EnumerateFiles tempDocsDir |> Seq.filter (fun x -> x.EndsWith(".html")))
    }

let checkOutLocalDocs =
    BuildTask.create "checkOutLocalDocs" [buildLocalDocs] {
        let psi = new System.Diagnostics.ProcessStartInfo(FileName = (__SOURCE_DIRECTORY__ </> "temp/localDocs/output/index.html"), UseShellExecute = true)
        System.Diagnostics.Process.Start(psi) |> ignore
    }

BuildTask.runOrDefaultWithArguments checkOutLocalDocs