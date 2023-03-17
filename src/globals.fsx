let mutable pageRoot = ""
// fix urls when deployed to base url (e.g. on gh pages via subdomain)
#if WATCH
let urlPrefix =
  ""
#else
let urlPrefix =
  "https://csbiology.github.io/CSBlog"
#endif

#r "_lib/Fornax.Core.dll"
open Html

/// returns a fixed urlby prefixing `urlPrefix`
let prefixUrl url = sprintf "%s/%s" urlPrefix url

/// injects websocket code necessary for hot reload on local preview via `dotnet fornax watch`
let injectWebsocketCode (webpage:string) =
    let websocketScript =
        """
        <script type="text/javascript">
          var wsUri = "ws://localhost:8080/websocket";
      function init()
      {
        websocket = new WebSocket(wsUri);
        websocket.onclose = function(evt) { onClose(evt) };
      }
      function onClose(evt)
      {
        console.log('closing');
        websocket.close();
        document.location.reload();
      }
      window.addEventListener("load", init, false);
      </script>
        """
    let head = "<head>"
    let index = webpage.IndexOf head
    webpage.Insert ( (index + head.Length + 1),websocketScript)

/// SEO stuff
type SiteMetadata = {
    Title: string
    Description: string
    Keywords: string []
    Image: string option
} with
    static member create(title, description, ?keywords, ?image) =
        let keywords = defaultArg keywords [|"CSB"; "Computational Systems Biology"; "computational biology"; "bioinformatics"; "data science"; "datascience"; "research"; "F#"; "C#"; ".NET"; "dotnet"|]
        {
            Title = title
            Description = description
            Keywords = keywords
            Image = image
        }
    static member toMetaTags(sm: SiteMetadata) =
        [
            meta [Name "description"; Content sm.Description]
            meta [Name "tags"; Content (String.concat ", " sm.Keywords)]
            //open graph
            meta [Name "og:title"; Content sm.Title]
            meta [Name "og:description"; Content sm.Description]
            if sm.Image.IsSome then meta [Name "og:image"; Content sm.Image.Value]
            //twitter card
            meta [Name"twitter:card"; Content "summary_large_image"]
            meta [Name"twitter:site"; Content "@fslaborg"]
            //meta [Name="twitter:creator" content=""]
            //meta [Name="twitter:url" content="<Hier kommt der Link hin>"]
            meta [Name "twitter:title"; Content sm.Title]
            meta [Name "twitter:description"; Content sm.Description]
            if sm.Image.IsSome then meta [Name "twitter:image"; Content sm.Image.Value]
        ]
    static member toTags (sm: SiteMetadata) =
        [
            title [] [!!sm.Title]
            yield! (sm |> SiteMetadata.toMetaTags)
        ]
