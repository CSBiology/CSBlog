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
        h1 [Class "title is-capitalized is-inline-block is-emphasized-csb-darkblue is-size-3"] [!!"Latest post"]
        Layout.postPreview latest_post
    ]

let browse_categories_display (posts: NotebookPost list) =
    div [Class "content"] [
        h1 [Class "title is-capitalized is-inline-block is-emphasized-csb-darkblue is-size-3"] [!!"Browse categories"]
        div [Class "container"] [
            ul [Class "mt-0"] [
                li [] [
                    h3 [Class "subtitle mb-1 is-size-4"] [a [Href (Globals.prefixUrl "posts/all_posts.html"); Class "orange-link"] [!! $"All [{posts.Length}]"] ]
                    p [Class "is-size-6"] [!! "Full post timeline"]
                ]
                yield!
                    posts
                    |> List.countBy (fun p -> p.post_config.category)
                    |> List.map (fun (c,count) ->
                        let link = Globals.prefixUrl $"posts/categories/{c}.html"
                        li [] [
                            h3 [Class "subtitle mb-1 is-size-4"] [a [Href link; Class "orange-link"] [!! $"{c |> PostCategory.toString} [{count}]"] ]
                            p [Class "is-size-6"] [!! (c |> PostCategory.getDescription)]
                        ]
                    )
            ]
        ]
    ]


let generate' (ctx : SiteContents) (_: string) =

    let posts =
        ctx.TryGetValues<NotebookPost>()
        |> Option.defaultValue Seq.empty
        |> List.ofSeq

    let latest_post = posts |> List.minBy (fun p -> System.DateTime.Now.Ticks - p.post_config.date.Ticks)

    let metadata =
        SiteMetadata.create(
            title = "The CSBlog - CSB study notes, research insights, and progress reports",
            description = "This is the place where members of the department of Computational Systems Biology of the TU Kaiserslautern blog about their work, research, and other loosely related stuff."
        )

    Layout.layout ctx metadata "" [
        section [Class "hero is-small is-warning is-bold"] [
            div [Class "hero-body"] [
                div [Class "container has-text-justified"] [
                    div [Class "main-TextField"] [
                        div [Class "media mb-4"] [
                            figure [Class "image is-128x128"] [
                                img [Src (Globals.prefixUrl "img/CSB.svg")]
                            ]
                            div [Class "media-content"] [
                                h1 [Class "main-title is-capitalized is-black is-inline-block is-strongly-emphasized-csb-orange mb-4"] [!! "The CSBlog"]
                            ]
                        ]
                        div [Class "block"] [
                            h2 [Class "subtitle is-size-4 is-block is-darkblue"] [!! "This is the place where members of the department of Computational Systems Biology of the TU Kaiserslautern blog about their work, research, and other loosely related stuff."]
                        ]
                    ]
                ]
            ]
        ]
        section [HtmlProperties.Style [MinHeight "30vh"]] [
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
