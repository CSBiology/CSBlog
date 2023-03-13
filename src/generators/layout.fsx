#r "../_lib/Fornax.Core.dll"
#load "../globals.fsx"
#if !FORNAX
#load "../loaders/globalloader.fsx"
#load "../loaders/postloader.fsx"
#endif

open Html
open Globals
open Postloader

let layout (ctx : SiteContents) activePost bodyCnt =

    let siteInfo = ctx.TryGetValue<Globalloader.SiteInfo>()
    let ttl =
        siteInfo
        |> Option.map (fun si -> si.title)
        |> Option.defaultValue ""

    html [] [
        head [] [
            meta [CharSet "utf-8"]
            meta [Name "viewport"; Content "width=device-width, initial-scale=1"]
            title [] [!! ttl]
            link [Rel "icon"; Type "image/png"; Sizes "32x32"; Href (Globals.prefixUrl "img/favicon.png")]
            link [Rel "stylesheet"; Type "text/css"; Href (Globals.prefixUrl "/styles/custom.css")]
            link [Rel "stylesheet"; Type "text/css"; Href "https://cdn.jsdelivr.net/npm/bulma@0.9.4/css/bulma.min.css"]
            link [Rel "stylesheet"; Type "text/css"; Href "https://cdn.jsdelivr.net/npm/@creativebulma/bulma-collapsible@1.0.4/dist/css/bulma-collapsible.min.css"]
            script [Src "https://cdn.jsdelivr.net/npm/@creativebulma/bulma-collapsible@1.0.4/dist/js/bulma-collapsible.min.js"] []
            script [Src (Globals.prefixUrl "/js/main.js")] []
            // script [Type "text/x-mathjax-config;executed=true"] [!!"MathJax.Hub.Config({tex2jax: {inlineMath: [['$','$'], ['\\(','\\)']], processEscapes: true}})"]
            // script [Type "text/javascript"; Src "https://cdnjs.cloudflare.com/ajax/libs/mathjax/2.7.5/MathJax.js?config=TeX-AMS-MML_HTMLorMML%2CSafe.js&ver=4.1"] []
            script [] [!!"""MathJax = {tex: {inlineMath: [['$', '$'], ['\\(', '\\)']]}}"""]
            script [Src """https://cdn.jsdelivr.net/npm/mathjax@3.2.0/es5/tex-svg.js"""] []
            link [Rel "stylesheet"; Href "https://cdn.jsdelivr.net/npm/bulma-timeline@3.0.5/dist/css/bulma-timeline.min.css"]
            link [Rel "stylesheet"; Href (Globals.prefixUrl "styles/notebook.css")]
            link [Rel "stylesheet"; Href (Globals.prefixUrl "styles/custom.css")]
        ]
        body [] [
            nav [
                Class "navbar is-dark is-spaced"
                Role "navigation"
                HtmlProperties.Custom("aria-label","main navigation")
            ] [
                div [Class "navbar-brand"] [
                    a [Class "navbar-item"; Href (Globals.prefixUrl "/")] [
                        img [Src (Globals.prefixUrl "img/logo_small.png");]
                    ]
                    a [
                        Role "button"
                        Class "navbar-burger burger"
                        HtmlProperties.Custom("aria-label","menu")
                        HtmlProperties.Custom ("aria-expanded", "false")
                        HtmlProperties.Custom ("data-target","navbar")
                    ] [
                        span [ HtmlProperties.Custom ("aria-hidden","true")] []
                        span [ HtmlProperties.Custom ("aria-hidden","true")] []
                        span [ HtmlProperties.Custom ("aria-hidden","true")] []
                    ]
                ]
                div [Id "navbar"; Class "navbar-menu"] [
                    div [Class "navbar-start"] [
                        a [Class "navbar-item"] [!!"Home"]
                        a [Class "navbar-item"] [!!"About"]
                    ]
                    div [Class "navbar-end"] [
                        a [Href"#"; Class "navbar-item"] [i [Class "fa fa-search"] []]
                        a [Href"#"; Class "navbar-item"] [i [Class "fa fa-shopping-bag"] []]
                    ]
                ]
            ]
            yield! bodyCnt
        ]
    ]


let postLayout (ctx : SiteContents) (post_config:PostConfig) (toc:HtmlElement) active bodyCnt =
    let siteInfo = ctx.TryGetValue<Globalloader.SiteInfo> ()
    let ttl =
        siteInfo
        |> Option.map (fun si -> si.title)
        |> Option.defaultValue ""

    let category_url = Globals.prefixUrl $"posts/categories/{post_config.category}.html"

    html [] [
        head [] [
            meta [CharSet "utf-8"]
            meta [Name "viewport"; Content "width=device-width, initial-scale=1"]
            title [] [!! ttl]
            link [Rel "stylesheet"; Type "text/css"; Href (Globals.prefixUrl "/styles/custom.css")]
            link [Rel "stylesheet"; Type "text/css"; Href "https://cdn.jsdelivr.net/npm/bulma@0.9.4/css/bulma.min.css"]
            link [Rel "stylesheet"; Type "text/css"; Href "https://cdn.jsdelivr.net/npm/@creativebulma/bulma-collapsible@1.0.4/dist/css/bulma-collapsible.min.css"]
            script [Src "https://cdn.jsdelivr.net/npm/@creativebulma/bulma-collapsible@1.0.4/dist/js/bulma-collapsible.min.js"] []
            script [Src (Globals.prefixUrl "/js/main.js")] []
            // script [Type "text/x-mathjax-config;executed=true"] [!!"MathJax.Hub.Config({tex2jax: {inlineMath: [['$','$'], ['\\(','\\)']], processEscapes: true}})"]
            // script [Type "text/javascript"; Src "https://cdnjs.cloudflare.com/ajax/libs/mathjax/2.7.5/MathJax.js?config=TeX-AMS-MML_HTMLorMML%2CSafe.js&ver=4.1"] []
            script [] [!!"""MathJax = {tex: {inlineMath: [['$', '$'], ['\\(', '\\)']]}}"""]
            script [Src """https://cdn.jsdelivr.net/npm/mathjax@3.2.0/es5/tex-svg.js"""] []
            link [Rel "stylesheet"; Href "https://cdn.jsdelivr.net/npm/bulma-timeline@3.0.5/dist/css/bulma-timeline.min.css"]
            link [Rel "stylesheet"; Href (Globals.prefixUrl "styles/notebook.css")]
            link [Rel "stylesheet"; Href (Globals.prefixUrl "styles/custom.css")]
        ]
        body [] [
            div [Class "columns is-fullheight m-0"] [
                div [Class "column is-2 is-paddingless box m-0"] [
                    aside [Class "menu p-4"; Id "graph-menu"] [
                        div [Class "content"] [
                            h3 [Class "title is-black is-capitalized is-inline-block is-emphasized-csb-darkblue mb-4"] [!! "Table of contents"]
                            toc
                        ]
                    ]
                ]
                div [Class "column is-10 is-paddingless pl-1 pr-6"] [
                    nav [
                        Class "navbar is-dark is-spaced"
                        Role "navigation"
                        HtmlProperties.Custom("aria-label","main navigation")
                    ] [
                        div [Class "navbar-brand"] [
                            a [Class "navbar-item"; Href (Globals.prefixUrl "/")] [
                                img [Src (Globals.prefixUrl "img/logo_small.png");]
                            ]
                            a [
                                Role "button"
                                Class "navbar-burger burger"
                                HtmlProperties.Custom("aria-label","menu")
                                HtmlProperties.Custom ("aria-expanded", "false")
                                HtmlProperties.Custom ("data-target","navbar")
                            ] [
                                span [ HtmlProperties.Custom ("aria-hidden","true")] []
                                span [ HtmlProperties.Custom ("aria-hidden","true")] []
                                span [ HtmlProperties.Custom ("aria-hidden","true")] []
                            ]
                        ]
                        div [Id "navbar"; Class "navbar-menu"] [
                            div [Class "navbar-start"] [
                                a [Class "navbar-item"] [!!"Home"]
                                a [Class "navbar-item"] [!!"About"]
                            ]
                            div [Class "navbar-end"] [
                               a [Href"#"; Class "navbar-item"] [i [Class "fa fa-search"] []]
                               a [Href"#"; Class "navbar-item"] [i [Class "fa fa-shopping-bag"] []]
                            ]
                        ]
                    ]
                    section [Class "hero is-small is-warning is-bold"] [
                        div [Class "hero-body"] [
                            div [Class "container has-text-justified"] [
                                div [Class "main-TextField"] [
                                    h1 [Class "title is-capitalized is-black is-inline-block is-emphasized-csb-darkblue mb-4"] [!! post_config.title]
                                    div [Class "block"] [
                                        h3 [Class "subtitle is-black is-block"] [
                                            !! $"Posted on {post_config.date.Year}-{post_config.date.Month}-{post_config.date.Day} by"
                                            a [Href post_config.author_link; Class "is-csb-darkblue"] [!! post_config.author]
                                            !! $" in "
                                            a [Href category_url; Class "is-csb-darkblue"] [!! (post_config.category |> PostCategory.toString)]
                                        ]
                                    ]
                                ]
                            ]
                        ]
                    ]
                    yield! bodyCnt
                ]
            ]
        ]
        //Components.Footer()
    ]

let postPreview (post:NotebookPost) =
    let has_image = post.post_config.preview_image.IsSome
    let has_summary = post.post_config.summary.IsSome

    let post_url = Globals.prefixUrl $"posts/{post.file_name}"
    let post_category_url = Globals.prefixUrl $"posts/categories/{post.post_config.category}.html"

    div [Class "card pt-2"] [
        if has_image then
            div [Class "card-image"] [
                a [Href post_url] [
                    figure [Class "image"] [
                        img [Src (Globals.prefixUrl post.post_config.preview_image.Value); Alt "post preview image"]
                    ]
                ]
            ]
        div [Class "card-header is-emphasized-csb-darkblue"] [
            h1 [Class "card-header-title is-size-4"] [a [Href post_url; Class "is-csb-orange"] [!!post.post_config.title]]
        ]
        div [Class "card-content is-size-6"] [
            if has_summary then div [Class "content"] [!!post.post_config.summary.Value]
            !! $"Posted on {post.post_config.date.Year}-{post.post_config.date.Month}-{post.post_config.date.Day} by "
            a [Href post.post_config.author_link; Class "is-csb-orange"] [!! post.post_config.author]
            !! "in "
            a [Href post_category_url; Class "is-csb-orange"] [!! (post.post_config.category |> PostCategory.toString)]
        ]
    ]

let render (ctx : SiteContents) cnt =
    cnt
    |> HtmlElement.ToString
    #if WATCH
    |> injectWebsocketCode
    #endif
