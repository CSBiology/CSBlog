#r "../_lib/Fornax.Core.dll"
#load "../globals.fsx"
#if !FORNAX
#load "../loaders/globalloader.fsx"
#load "../loaders/notebookloader.fsx"
#endif

open Html
open Globals

let layout (ctx : SiteContents) activePost bodyCnt =

    let siteInfo = ctx.TryGetValue<Globalloader.SiteInfo>()
    let ttl =
        siteInfo
        |> Option.map (fun si -> si.title)
        |> Option.defaultValue ""

    let graphs = ctx.TryGetValues<Notebookloader.Notebook>() |> Option.defaultValue [] |> List.ofSeq

    let menuItems =
        graphs
        |> List.groupBy (fun n -> n.category)
        |> List.map (fun (category,notebooks) ->
            [
                p [Class "menu-label"] [!!category]
                ul [Class "menu-list"] (
                    notebooks
                    |> List.map (fun nb ->
                        let isActive = nb.filename = activePost
                        a [
                            Href (prefixPath nb.htmlPath)
                            if isActive then Class "is-active"
                        ] [!!nb.title]
                    )
                )
            ]
        )
        |> List.concat

    let menu = aside [Class "menu p-4"; Id "menu"] menuItems

    html [] [
        head [] [
            meta [CharSet "utf-8"]
            meta [Name "viewport"; Content "width=device-width, initial-scale=1"]
            title [] [!! ttl]
            link [Rel "stylesheet"; Type "text/css"; Href (prefixPath "/styles/custom.css")]
            link [Rel "stylesheet"; Type "text/css"; Href "https://cdn.jsdelivr.net/npm/bulma@0.9.4/css/bulma.min.css"]
            link [Rel "stylesheet"; Type "text/css"; Href "https://cdn.jsdelivr.net/npm/@creativebulma/bulma-collapsible@1.0.4/dist/css/bulma-collapsible.min.css"]
            script [Src "https://cdn.jsdelivr.net/npm/@creativebulma/bulma-collapsible@1.0.4/dist/js/bulma-collapsible.min.js"] []
            script [Src (prefixPath "/js/main.js")] []
            // script [Type "text/x-mathjax-config;executed=true"] [!!"MathJax.Hub.Config({tex2jax: {inlineMath: [['$','$'], ['\\(','\\)']], processEscapes: true}})"]
            // script [Type "text/javascript"; Src "https://cdnjs.cloudflare.com/ajax/libs/mathjax/2.7.5/MathJax.js?config=TeX-AMS-MML_HTMLorMML%2CSafe.js&ver=4.1"] []
            script [] [!!"""MathJax = {tex: {inlineMath: [['$', '$'], ['\\(', '\\)']]}}"""]
            script [Src """https://cdn.jsdelivr.net/npm/mathjax@3.2.0/es5/tex-svg.js"""] []
        ]
        body [] [
            div [Class "columns is-fullheight m-0"] [
                div [Class "column is-2 is-paddingless box m-0"] [menu]
                div [Class "column is-10 is-paddingless pl-1"] [
                    nav [
                        Class "navbar is-csb-darkblue is-dark is-spaced"
                        Role "navigation"
                        HtmlProperties.Custom("aria-label","main navigation")
                    ] [
                        div [Class "navbar-brand"] [
                            a [Class "navbar-item"; Href (prefixPath "/")] [
                                img [Src (prefixPath "/img/logo_small.png");]
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
        ]
    ]

let render (ctx : SiteContents) cnt =
    cnt
    |> HtmlElement.ToString
    #if WATCH
    |> injectWebsocketCode
    #endif
