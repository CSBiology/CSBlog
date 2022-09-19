#r "../_lib/Fornax.Core.dll"
#load "layout.fsx"

open Html
open Globals

let generate' (ctx : SiteContents) (_: string) =
    Layout.layout ctx "" [
        section [Class "hero is-medium"] [
            div [Class "hero-body"] [
                img [Src "img/CSB.png"; Alt "CSB"]
                p [Class "title"] [!!"CSBlog"]
                hr [Class "divider"]
                div [Class "block"] [
                    p [Class "subtitle"] [!!"Welcome to the CSBlog. This is the place where members of the department of Computational Systems Biology of the TU Kaiserslautern blog about their work, research, and other loosely related stuff."]
                ]
                div [Class "block"] [
                    p [Class "subtitle"] [
                        !!"For more information about our research and our group in general, head to our "
                        a [Href "https://csb.bio.uni-kl.de/"] [!!"website"]
                        !!" If you want to take a look at our projects, head over to our "
                        a [Href "https://github.com/CSBiology"] [!!"github page"]
                    ]
                ]
                div [Class "block"] [
                    p [Class "subtitle"] [!!"BlogPosts are accessible on the sidebar."]
                ]
            ]
        ]
    ]

let generate (ctx : SiteContents) (projectRoot: string) (page: string) =
    generate' ctx page
    |> Layout.render ctx
