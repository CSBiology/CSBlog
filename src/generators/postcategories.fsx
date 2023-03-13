#r "../_lib/Fornax.Core.dll"
#load "../globals.fsx"
#load "layout.fsx"
#if !FORNAX
#load "../loaders/postloader.fsx"
#endif

open Postloader
open Layout
open Html

open System.IO
open System.Diagnostics


let generate (ctx : SiteContents) (projectRoot: string) (page: string) =

    ctx.TryGetValues<NotebookPost>()
    |> Option.defaultValue Seq.empty
    |> List.ofSeq
    |> List.groupBy (fun post -> post.post_config.category)
    |> List.map (fun (category, posts) ->
        Path.Combine([|projectRoot; "_public"; "posts"; "categories"; $"{category}.html"|]),
        Layout.layout ctx "Posts" [
            section [Class "hero is-small is-warning is-bold"] [
                div [Class "hero-body"] [
                    div [Class "container has-text-centered"] [
                        div [Class "main-TextField"] [
                            h1 [Class "title is-capitalized is-black is-inline-block is-emphasized-csb-darkblue mb-4"] [!! $"{category |> PostCategory.toString} posts"]
                            h3 [Class "subtitle is-size-4 is-black mt-4"] [!! (category |> PostCategory.getDescription)]
                        ]
                    ]
                ]
            ]
            section [] [
                div [Class "container"] [
                    div [Class "timeline is-centered"] (
                        posts
                        |> List.sortByDescending (fun p -> p.post_config.date)
                        |> List.map (fun post ->
                            div [Class"timeline-item is-darkmagenta"] [
                                div [Class "timeline-marker"] []
                                div [Class "timeline-content"] [
                                    div [Class "content"] [
                                        p [Class "heading is-size-4"] [!! $"{post.post_config.date.Year}-{post.post_config.date.Month}-{post.post_config.date.Day}"]
                                        Layout.postPreview post
                                    ]
                                ]
                            ]
                        )
                    )
                ]


            ]
        ]
        |> Layout.render ctx
    )
