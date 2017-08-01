// --------------------------------------------------------------------------------------
// FAKE build script
// --------------------------------------------------------------------------------------

#r @"packages/build/FAKE/tools/FakeLib.dll"
open Fake
open Fake.Git
open Fake.ReleaseNotesHelper


open System
open System.IO
open System.Diagnostics

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
let description = "The official Blog of the Computational Systems Biology workgroup of the TU Kaiserslautern"

// List of author names (for NuGet package)
let authors = [ "HLWeil" ]

// Tags for your project (for NuGet package)
let tags = "Blog CSB CSBiology TUKL FSharp F# BioFSharp Webblog CSBlog"

// File system information
let solutionFile  = "CSBlog.sln"

// Default target configuration
let configuration = "Release"

// Pattern specifying assemblies to be tested using NUnit
let testAssemblies = "tests/**/bin" </> configuration </> "*Tests*.dll"

// Git configuration (used for publishing documentation in gh-pages branch)
// The profile where the project is posted
let gitOwner = "CSBiology"
let gitHome = sprintf "%s/%s" "https://github.com" gitOwner

// The name of the project on GitHub
let gitName = "CSBlog"

// The url for the raw files hosted
let gitRaw = environVarOrDefault "gitRaw" "https://raw.githubusercontent.com/HLWeil"

// --------------------------------------------------------------------------------------
// END TODO: The rest of the file includes standard build steps
// --------------------------------------------------------------------------------------

// Read additional information from the release notes document
let release = LoadReleaseNotes "RELEASE_NOTES.md"

// Helper active pattern for project types
let (|Fsproj|Csproj|Vbproj|Shproj|) (projFileName:string) =
    match projFileName with
    | f when f.EndsWith("fsproj") -> Fsproj
    | f when f.EndsWith("csproj") -> Csproj
    | f when f.EndsWith("vbproj") -> Vbproj
    | f when f.EndsWith("shproj") -> Shproj
    | _                           -> failwith (sprintf "Project file %s not supported. Unknown project type." projFileName)


// --------------------------------------------------------------------------------------
// Clean build results

let vsProjProps = 
#if MONO
    [ ("DefineConstants","MONO"); ("Configuration", configuration) ]
#else
    [ ("Configuration", configuration); ("Platform", "Any CPU") ]
#endif

Target "Clean" (fun _ ->
    !! solutionFile |> MSBuildReleaseExt "" vsProjProps "Clean" |> ignore
    CleanDirs [ "docs"]
)


// --------------------------------------------------------------------------------------
// Generate the documentation


let fakePath = "packages" </> "build" </> "FAKE" </> "tools" </> "FAKE.exe"
let fakeStartInfo script workingDirectory args fsiargs environmentVars =
    (fun (info: ProcessStartInfo) ->
        info.FileName <- System.IO.Path.GetFullPath fakePath
        info.Arguments <- sprintf "%s --fsiargs -d:FAKE %s \"%s\"" args fsiargs script
        info.WorkingDirectory <- workingDirectory
        let setVar k v =
            info.EnvironmentVariables.[k] <- v
        for (k, v) in environmentVars do
            setVar k v
        setVar "MSBuild" msBuildExe
        setVar "GIT" Git.CommandHelper.gitPath
        setVar "FSI" fsiPath)

/// Run the given buildscript with FAKE.exe
let executeFAKEWithOutput workingDirectory script fsiargs envArgs =
    let exitCode =
        ExecProcessWithLambdas
            (fakeStartInfo script workingDirectory "" fsiargs envArgs)
            TimeSpan.MaxValue false ignore ignore
    System.Threading.Thread.Sleep 1000
    exitCode

// Documentation
let buildDocumentationTarget fsiargs target =
    trace (sprintf "Building documentation (%s), this could take some time, please wait..." target)
    let exit = executeFAKEWithOutput "docsrc/tools" "generate.fsx" fsiargs ["target", target]
    if exit <> 0 then
        failwith "generating reference documentation failed"
    ()

Target "GenerateReferenceDocs" (fun _ ->
    CreateDir "docs"
    CleanDir "docs"
    buildDocumentationTarget "-d:RELEASE -d:REFERENCE" "Default"
)

let generateHelp' fail debug =
    let args =
        if debug then "--define:HELP"
        else "--define:RELEASE --define:HELP"
    try
        buildDocumentationTarget args "Default"
        traceImportant "Help generated"
    with
    | e when not fail ->
        traceImportant "generating help documentation failed"

let generateHelp fail =
    generateHelp' fail false

Target "GenerateHelpDebug" (fun _ ->
    DeleteFile "docsrc/content/release-notes.md"
    CopyFile "docsrc/content/" "RELEASE_NOTES.md"
    Rename "docsrc/content/release-notes.md" "docsrc/content/RELEASE_NOTES.md"

    DeleteFile "docsrc/content/license.md"
    CopyFile "docsrc/content/" "LICENSE.txt"
    Rename "docsrc/content/license.md" "docsrc/content/LICENSE.txt"

    generateHelp' true true
)

Target "KeepRunning" (fun _ ->
    use watcher = !! "docsrc/content/**/*.*" |> WatchChanges (fun changes ->
         generateHelp' true true
    )

    traceImportant "Waiting for help edits. Press any key to stop."

    System.Console.ReadKey() |> ignore

    watcher.Dispose()
)

Target "ReleaseLocal" (fun _ ->
    let tempDocsDir = "local"
    CreateDir tempDocsDir
    CleanDir tempDocsDir
    CopyRecursive "docs" tempDocsDir true |> tracefn "%A"
    ReplaceInFiles 
        (seq {
            yield "href=\"/" + project + "/","href=\""
            yield "src=\"/" + project + "/","src=\""}) 
        ((filesInDirMatching "*.html" (directoryInfo tempDocsDir)) |> Array.map (fun x -> tempDocsDir + "/" + x.Name))
)

Target "Release" (fun _ ->
    let tempDocsDir = "temp/gh-pages"
    CleanDir tempDocsDir
    Repository.cloneSingleBranch "" (gitHome + "/" + gitName + ".git") "gh-pages" tempDocsDir

    CopyRecursive "docs" tempDocsDir true |> tracefn "%A"
    StageAll tempDocsDir
    Git.Commit.Commit tempDocsDir (sprintf "Update generated documentation for version %s" release.NugetVersion)
    Branches.push tempDocsDir
)

let createIndexFsx lang =
    let content = """(*** hide ***)
// This block of code is omitted in the generated HTML documentation. Use
// it to define helpers that you do not want to show in the documentation.
#I "../../../bin"

(**
F# Project Scaffold ({0})
=========================
*)
"""
    let targetDir = "docsrc/content" </> lang
    let targetFile = targetDir </> "index.fsx"
    ensureDirectory targetDir
    System.IO.File.WriteAllText(targetFile, System.String.Format(content, lang))

// --------------------------------------------------------------------------------------
// Release Scripts

#load "paket-files/build/fsharp/FAKE/modules/Octokit/Octokit.fsx"
open Octokit


// --------------------------------------------------------------------------------------
// Run all targets by default. Invoke 'build <Target>' to override

Target "All" DoNothing

"GenerateReferenceDocs"
  ==> "All"
  ==> "ReleaseLocal"

"GenerateReferenceDocs"
  ==> "All"
  ==> "Release"

"GenerateHelpDebug"
  ==> "KeepRunning"

RunTargetOrDefault "All"
