let mutable pageRoot = ""

// fix urls when deployed to base url (e.g. on gh pages via subdomain)
#if WATCH
let urlPrefix =
  ""
#else
let urlPrefix =
  "https://csbiology.github.io/CSBlog"
#endif

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
