#r "_lib/Fornax.Core.dll"

open Config
open System.IO

let staticPredicate (projectRoot: string, page: string) =
    let ext = Path.GetExtension page
    let fileShouldBeExcluded =
        ext = ".fsx" ||
        ext = ".md"  ||
        ext = ".ipynb"  ||
        page.Contains "_public" ||
        page.Contains "_bin" ||
        page.Contains "_lib" ||
        page.Contains "_data" ||
        page.Contains "_settings" ||
        page.Contains "_config.yml" ||
        page.Contains ".sass-cache" ||
        page.Contains ".git" ||
        page.Contains "graphs" ||
        page.Contains ".ionide"
    fileShouldBeExcluded |> not


let config = {
    Generators = [
        // {Script = "less.fsx"; Trigger = OnFileExt ".less"; OutputFile = ChangeExtension "css" }
        {Script = "posts.fsx"; Trigger = Once; OutputFile = MultipleFiles id}
        {Script = "postcategories.fsx"; Trigger = Once; OutputFile = MultipleFiles id}
        //{Script = "notebooks.fsx"; Trigger = Once; OutputFile = MultipleFiles id}
        {Script = "staticfile.fsx"; Trigger = OnFilePredicate staticPredicate; OutputFile = SameFileName }
        {Script = "index.fsx"; Trigger = Once; OutputFile = NewFileName "index.html" }
    ]
}
