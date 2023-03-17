#r "../_lib/Fornax.Core.dll"
#load "../globals.fsx"
#load "layout.fsx"

open Postloader
open Layout
open Html
open Globals

open System.IO
open System.Diagnostics

let categoryTimeline ctx category posts =

    let metadata =
        SiteMetadata.create(
            title = $"The CsBlog - {PostCategory.toString category} posts",
            description = $"The {PostCategory.toString category} category contains {PostCategory.getDescription category}"
        )


    Layout.layout ctx metadata "Posts" [
        section [Class "hero is-small is-warning is-bold"] [
            div [Class "hero-body"] [
                div [Class "container has-text-centered"] [
                    div [Class "main-TextField"] [
                        h1 [Class "title is-capitalized is-black is-inline-block is-emphasized-csb-orange mb-4"] [!! $"{category |> PostCategory.toString} posts"]
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

let generate (ctx : SiteContents) (projectRoot: string) (page: string) =

    let all_posts =
        ctx.TryGetValues<NotebookPost>()
        |> Option.defaultValue Seq.empty
        |> List.ofSeq

    let categories =
        all_posts
        |> List.groupBy (fun post -> post.post_config.category)
        |> List.map (fun (category, posts) ->
            Path.Combine([|projectRoot; "_public"; "posts"; "categories"; $"{category}.html"|]),
            categoryTimeline ctx category posts
            |> Layout.render ctx
        )

    let full_timeline =
        all_posts
        |> List.map (fun posts ->
            Path.Combine([|projectRoot; "_public"; "posts"; "all_posts.html"|]),
            categoryTimeline ctx (PostCategory.Other "All") all_posts
            |> Layout.render ctx
        )

    full_timeline@categories
