#r "../_lib/Fornax.Core.dll"
#load "layout.fsx"
#if !FORNAX
#load "../loaders/postloader.fsx"
#endif

open Html
open Globals
open Postloader
open Html

let latest_post_display (latest_post: NotebookPost) =

    div [Class "content"] [
        h1 [Class "title is-capitalized is-inline-block is-emphasized-darkmagenta is-size-3"] [!!"Latest post"]
        Layout.postPreview latest_post
    ]

let browse_categories_display (posts: NotebookPost list) =
    div [Class "content"] [
        h1 [Class "title is-capitalized is-inline-block is-emphasized-darkmagenta is-size-3"] [!!"Browse categories"]
        div [Class "container"] [
            ul [Class "mt-0"] (
                posts
                |> List.countBy (fun p -> p.post_config.category)
                |> List.map (fun (c,count) ->
                    let link = Globals.prefixUrl $"posts/categories/{c}.html"
                    li [] [
                        h3 [Class "subtitle mb-1 is-size-4"] [a [Href link; Class "is-magenta"] [!! $"{c |> PostCategory.toString} [{count}]"] ]
                        p [Class "is-size-6"] [!! (c |> PostCategory.getDescription)]
                    ]
                )
            )
        ]
    ]


let generate' (ctx : SiteContents) (_: string) =

    let posts =
        ctx.TryGetValues<NotebookPost>()
        |> Option.defaultValue Seq.empty
        |> List.ofSeq

    let latest_post = posts |> List.minBy (fun p -> System.DateTime.Now.Ticks - p.post_config.date.Ticks)


    Layout.layout ctx "" [
        section [Class "hero is-medium is-warning is-bold"] [
            div [Class "hero-body"] [
                div [Class "container has-text-justified"] [
                    div [Class "main-TextField"] [
                        div [Class "media mb-4"] [
                            figure [Class "image is-128x128"] [
                                img [Src (Globals.prefixUrl "img/CSB.svg")]
                            ]
                        ]
                        div [Class "media-content has-text-black"] [
                            h1 [Class "title is-size-1 is-capitalized is-inline-block has-text-black"] [!! "The CsBlog"]
                        ]
                    ]
                    div [Class "block"] [
                        h2 [Class "subtitle is-size-4 is-white is-block has-text-black"] [!! "This is the place where members of the department of Computational Systems Biology of the TU Kaiserslautern blog about their work, research, and other loosely related stuff."]
                    ]
                ]
            ]
        ]
        section [] [
            div [Class "container has-text-justified mt-2"] [
                div [Class "main-TextField"] [
                    div [Class "columns"] [
                        div [Class "column is-6"] [
                            latest_post_display latest_post
                        ]
                        div [Class "column is-6"] [
                            browse_categories_display posts
                        ]
                    ]
                ]
            ]
        ]
    ]

let generate (ctx : SiteContents) (projectRoot: string) (page: string) =
    generate' ctx page
    |> Layout.render ctx
