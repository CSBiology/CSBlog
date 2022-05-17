(**

---
title: Visualizing relationships between sets with Plotly.NET
category: Visualization
categoryindex: 2
index: 2
---
*)

(***hide***)
#r "nuget: Deedle, 2.3.0"
#r "nuget: Plotly.NET, 2.0.0"
#r "nuget: FSharp.Stats, 0.4.1"
#r "nuget: FSharpAux"
#r "nuget: BioFSharp.Vis, 3.0.1"

open Deedle
open Plotly.NET
open Plotly.NET.LayoutObjects
open FSharp.Stats
open BioFSharp.Vis
open BioFSharp.Vis.UpSet
open BioFSharp.Vis.Venn
open System
open System.IO

do fsi.AddPrinter(fun (printer:Deedle.Internal.IFsiFormattable) -> "\n" + (printer.Format()))

let movieFrame =
    Frame.ReadCsv(Path.Combine(__SOURCE_DIRECTORY__,"movies.csv"),separators=";")
    |> Frame.sliceCols ["Name";"Action";"Comedy";"Drama";"AvgRating"]
    |> Frame.filterRows (fun k s ->
        s.GetAs<bool>("Action")   ||
        s.GetAs<bool>("Comedy")   ||
        s.GetAs<bool>("Drama")
    )

let getSetByGenre (category: string) (frame: Frame<int,string>) =
    frame
    |> Frame.indexRowsUsing(fun s ->
        {|
            Name = s.GetAs<string>("Name");
            Genre = s.GetAs<bool>(category)
        |}
    )
    |> fun f -> f.RowKeys
    |> Seq.toArray
    |> Array.filter (fun x -> x.Genre = true)
    |> Array.map (fun x -> x.Name)
    |> Set.ofArray

let getScoreMap (frame: Frame<int,string>) =
    frame
    |> Frame.indexRowsUsing(fun s ->
        s.GetAs<string>("Name"),
        s.GetAs<float>("AvgRating")
    )
    |> fun f -> f.RowKeys
    |> Map.ofSeq

(**
# Visualizing relationships between sets with Plotly.NET

### Table of contents

- [Visualizing set relationships with Venn diagrams](#Visualizing-set-relationships-with-Venn-diagrams)
    - [Construction Venn](#Construction-Venn)
    - [Usage Venn](#Usage-Venn)
- [Visualizing set relationships with UpSet plots](#Visualizing-set-relationships-with-UpSet-plots)
    - [Construction UpSet](#Construction-UpSet)
    - [Usage UpSet](#Usage-UpSet)

Visualizing and understanding relationships between sets plays an important role in analyzing data at hand. A widely used visualization method is the 
[Venn diagram](https://en.wikipedia.org/wiki/Venn_diagram). But Venn diagrams are limited in their capability. While two, three, or even four sets may be easily visualizable, they struggle 
with higher set counts. To address this issue, the concept of [UpSet](https://upset.app/) plots was developed by Lex et al. in 2014. In this blogpost I will demonstrate how to visualize 
sets with Venn diagrams and UpSet plots. I use a dataframe containing information about movies as source for our sets. The comparisons will be based on the genre of the movies.
*)

(***hide***)
movieFrame
(*** include-it ***)

(**
## Visualizing set relationships with Venn diagrams
### Construction Venn
A Venn diagram uses simple closed shapes to represent sets. Those shapes are often circles or ellipses.
Let`s start with a simple comparison of two sets using circles as our shape. For that we take genres action and comedy and determine their intersections:
*)

let actionSet =
    movieFrame
    |> getSetByGenre "Action"

let comedySet =
    movieFrame
    |> getSetByGenre "Comedy"

let intersectionCount =
    Venn.ofSetList [|"Action";"Comedy"|] [|actionSet;comedySet|]
    |> Venn.toVennCount
(***hide***)
intersectionCount
(*** include-it ***)
(**
Now we can start building our Venn diagram with Plotly. First of all we need to create two shapes for the circles at the correct position and put them in a layout.
*)

let axis =
    LinearAxis.init(
        ShowTickLabels = false,
        ShowGrid = false,
        ZeroLine = false
    )

let circleAction =
    Shape.init(
        Opacity = 0.3,
        Xref = "x",
        Yref = "y",
        Fillcolor = Color.fromKeyword Red,
        X0 = 0,
        Y0 = 0,
        X1 = 2,
        Y1 = 2,
        ShapeType = StyleParam.ShapeType.Circle,
        Line = Line.init(Color = Color.fromKeyword Red)
    )

let circleComedy =
    Shape.init(
        Opacity = 0.3,
        Xref = "x",
        Yref = "y",
        Fillcolor = Color.fromKeyword Blue,
        X0 = 1.5,
        Y0 = 0,
        X1 = 3.5,
        Y1 = 2,
        ShapeType = StyleParam.ShapeType.Circle,
        Line = Line.init(Color = Color.fromKeyword Blue)
    )

let layout =
    Layout.init(
        Shapes = [circleAction;circleComedy],
        Margin = 
            Margin.init(
                Left = 20,
                Right = 20,
                Bottom = 100
            )
    )
    |> Layout.updateLinearAxisById(StyleParam.SubPlotId.XAxis 1, axis)
    |> Layout.updateLinearAxisById(StyleParam.SubPlotId.YAxis 1, axis)

(**
Next, we need some text to describe our sets and intersection counts. This can be achieved via `Chart.Scatter`.
*)

let vennChart =
    Trace2D.initScatter(
        Trace2DStyle.Scatter(
            X = [|1.; 2.5; 1.75|],
            Y = [|1.; 1.; 1.|],
            Mode = StyleParam.Mode.Text,
            MultiText = ["Action<br>438";"Comedy<br>1135";"65"],
            TextFont =
                Font.init (
                    Family = StyleParam.FontFamily.Arial,
                    Size = 18.,
                    Color = Color.fromString "black"
                )
        )
    )
    |> GenericChart.ofTraceObject true
    |> Chart.withSize (400.,400.)

(**
We can now complete our Venn diagram by adding our previously created layout to the `Chart.Scatter` 
*)

vennChart
|> Chart.withLayout layout

(***hide***)
"""<div id="01237ef1-7c52-4d9e-bcbe-4cda31ddd24f"><!-- Plotly chart will be drawn inside this DIV --></div>
<script type="text/javascript">

          var renderPlotly_01237ef17c524d9ebcbe4cda31ddd24f = function() {
          var fsharpPlotlyRequire = requirejs.config({context:'fsharp-plotly',paths:{plotly:'https://cdn.plot.ly/plotly-2.6.3.min'}}) || require;
          fsharpPlotlyRequire(['plotly'], function(Plotly) {

          var data = [{"type":"scatter","mode":"text","x":[1.0,2.5,1.75],"y":[1.0,1.0,1.0],"text":["Action<br>438","Comedy<br>1135","65"],"textfont":{"family":"Arial","size":18.0,"color":"black"}}];
          var layout = {"width":400,"height":400,"template":{"layout":{"paper_bgcolor":"white","plot_bgcolor":"white","xaxis":{"showline":true,"zeroline":true},"yaxis":{"showline":true,"zeroline":true}},"data":{}},"margin":{"l":20,"r":20,"b":100},"shapes":[{"type":"circle","xref":"x","x0":0,"x1":2,"yref":"y","y0":0,"y1":2,"opacity":0.3,"line":{"color":"rgba(255, 0, 0, 1.0)"},"fillcolor":"rgba(255, 0, 0, 1.0)"},{"type":"circle","xref":"x","x0":1.5,"x1":3.5,"yref":"y","y0":0,"y1":2,"opacity":0.3,"line":{"color":"rgba(0, 0, 255, 1.0)"},"fillcolor":"rgba(0, 0, 255, 1.0)"}],"xaxis":{"showticklabels":false,"showgrid":false,"zeroline":false},"yaxis":{"showticklabels":false,"showgrid":false,"zeroline":false}};
          var config = {"responsive":true};
          Plotly.newPlot('01237ef1-7c52-4d9e-bcbe-4cda31ddd24f', data, layout, config);
});
          };
          if ((typeof(requirejs) !==  typeof(Function)) || (typeof(requirejs.config) !== typeof(Function))) {
              var script = document.createElement("script");
              script.setAttribute("src", "https://cdnjs.cloudflare.com/ajax/libs/require.js/2.3.6/require.min.js");
              script.onload = function(){
                  renderPlotly_01237ef17c524d9ebcbe4cda31ddd24f();
              };
              document.getElementsByTagName("head")[0].appendChild(script);
          }
          else {
              renderPlotly_01237ef17c524d9ebcbe4cda31ddd24f();
          }
</script>
"""
(***include-it-raw***)
(**
### Usage Venn

This required a lot of manual formatting. Luckily, [BioFSharp.Vis](https://github.com/CSBiology/BioFSharp.Vis) contains chart extensions 
for Venn diagrams with two and three sets.
*)

let dramaSet =
    movieFrame
    |> getSetByGenre "Drama"

Chart.Venn (
    [|"Action";"Comedy";"Drama"|],
    [|actionSet;comedySet;dramaSet|]
)

(***hide***)
"""<div id="9c33a2a9-282c-43ce-b2a7-d204c9312c02"><!-- Plotly chart will be drawn inside this DIV --></div>
<script type="text/javascript">

          var renderPlotly_9c33a2a9282c43ceb2a7d204c9312c02 = function() {
          var fsharpPlotlyRequire = requirejs.config({context:'fsharp-plotly',paths:{plotly:'https://cdn.plot.ly/plotly-2.6.3.min'}}) || require;
          fsharpPlotlyRequire(['plotly'], function(Plotly) {

          var data = [{"type":"scatter","mode":"text","x":[1.0,2.5,1.75,1.75,1.325,2.125,1.75],"y":[1.0,1.0,2.25,1.0,1.6625,1.6625,1.45],"text":["Action<br>348","Comedy<br>919","Drama<br>1287","55","90","216","10"],"textfont":{"family":"Arial","size":18.0,"color":"black"}}];
          var layout = {"width":600,"height":600,"template":{"layout":{"title":{"x":0.05},"font":{"color":"rgba(42, 63, 95, 1.0)"},"paper_bgcolor":"rgba(255, 255, 255, 1.0)","plot_bgcolor":"rgba(229, 236, 246, 1.0)","autotypenumbers":"strict","colorscale":{"diverging":[[0.0,"#8e0152"],[0.1,"#c51b7d"],[0.2,"#de77ae"],[0.3,"#f1b6da"],[0.4,"#fde0ef"],[0.5,"#f7f7f7"],[0.6,"#e6f5d0"],[0.7,"#b8e186"],[0.8,"#7fbc41"],[0.9,"#4d9221"],[1.0,"#276419"]],"sequential":[[0.0,"#0d0887"],[0.1111111111111111,"#46039f"],[0.2222222222222222,"#7201a8"],[0.3333333333333333,"#9c179e"],[0.4444444444444444,"#bd3786"],[0.5555555555555556,"#d8576b"],[0.6666666666666666,"#ed7953"],[0.7777777777777778,"#fb9f3a"],[0.8888888888888888,"#fdca26"],[1.0,"#f0f921"]],"sequentialminus":[[0.0,"#0d0887"],[0.1111111111111111,"#46039f"],[0.2222222222222222,"#7201a8"],[0.3333333333333333,"#9c179e"],[0.4444444444444444,"#bd3786"],[0.5555555555555556,"#d8576b"],[0.6666666666666666,"#ed7953"],[0.7777777777777778,"#fb9f3a"],[0.8888888888888888,"#fdca26"],[1.0,"#f0f921"]]},"hovermode":"closest","hoverlabel":{"align":"left"},"coloraxis":{"colorbar":{"outlinewidth":0.0,"ticks":""}},"geo":{"showland":true,"landcolor":"rgba(229, 236, 246, 1.0)","showlakes":true,"lakecolor":"rgba(255, 255, 255, 1.0)","subunitcolor":"rgba(255, 255, 255, 1.0)","bgcolor":"rgba(255, 255, 255, 1.0)"},"mapbox":{"style":"light"},"polar":{"bgcolor":"rgba(229, 236, 246, 1.0)","radialaxis":{"linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)","ticks":""},"angularaxis":{"linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)","ticks":""}},"scene":{"xaxis":{"ticks":"","linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)","gridwidth":2.0,"zerolinecolor":"rgba(255, 255, 255, 1.0)","backgroundcolor":"rgba(229, 236, 246, 1.0)","showbackground":true},"yaxis":{"ticks":"","linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)","gridwidth":2.0,"zerolinecolor":"rgba(255, 255, 255, 1.0)","backgroundcolor":"rgba(229, 236, 246, 1.0)","showbackground":true},"zaxis":{"ticks":"","linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)","gridwidth":2.0,"zerolinecolor":"rgba(255, 255, 255, 1.0)","backgroundcolor":"rgba(229, 236, 246, 1.0)","showbackground":true}},"ternary":{"aaxis":{"ticks":"","linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)"},"baxis":{"ticks":"","linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)"},"caxis":{"ticks":"","linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)"},"bgcolor":"rgba(229, 236, 246, 1.0)"},"xaxis":{"title":{"standoff":15},"ticks":"","automargin":true,"linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)","zerolinecolor":"rgba(255, 255, 255, 1.0)","zerolinewidth":2.0},"yaxis":{"title":{"standoff":15},"ticks":"","automargin":true,"linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)","zerolinecolor":"rgba(255, 255, 255, 1.0)","zerolinewidth":2.0},"annotationdefaults":{"arrowcolor":"#2a3f5f","arrowhead":0,"arrowwidth":1},"shapedefaults":{"line":{"color":"rgba(42, 63, 95, 1.0)"}},"colorway":["rgba(99, 110, 250, 1.0)","rgba(239, 85, 59, 1.0)","rgba(0, 204, 150, 1.0)","rgba(171, 99, 250, 1.0)","rgba(255, 161, 90, 1.0)","rgba(25, 211, 243, 1.0)","rgba(255, 102, 146, 1.0)","rgba(182, 232, 128, 1.0)","rgba(255, 151, 255, 1.0)","rgba(254, 203, 82, 1.0)"]},"data":{"bar":[{"marker":{"line":{"color":"rgba(229, 236, 246, 1.0)","width":0.5},"pattern":{"fillmode":"overlay","size":10,"solidity":0.2}},"error_x":{"color":"rgba(42, 63, 95, 1.0)"},"error_y":{"color":"rgba(42, 63, 95, 1.0)"}}],"barpolar":[{"marker":{"line":{"color":"rgba(229, 236, 246, 1.0)","width":0.5},"pattern":{"fillmode":"overlay","size":10,"solidity":0.2}}}],"carpet":[{"aaxis":{"linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)","endlinecolor":"rgba(42, 63, 95, 1.0)","minorgridcolor":"rgba(255, 255, 255, 1.0)","startlinecolor":"rgba(42, 63, 95, 1.0)"},"baxis":{"linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)","endlinecolor":"rgba(42, 63, 95, 1.0)","minorgridcolor":"rgba(255, 255, 255, 1.0)","startlinecolor":"rgba(42, 63, 95, 1.0)"}}],"choropleth":[{"colorbar":{"outlinewidth":0.0,"ticks":""},"colorscale":[[0.0,"#0d0887"],[0.1111111111111111,"#46039f"],[0.2222222222222222,"#7201a8"],[0.3333333333333333,"#9c179e"],[0.4444444444444444,"#bd3786"],[0.5555555555555556,"#d8576b"],[0.6666666666666666,"#ed7953"],[0.7777777777777778,"#fb9f3a"],[0.8888888888888888,"#fdca26"],[1.0,"#f0f921"]]}],"contour":[{"colorbar":{"outlinewidth":0.0,"ticks":""},"colorscale":[[0.0,"#0d0887"],[0.1111111111111111,"#46039f"],[0.2222222222222222,"#7201a8"],[0.3333333333333333,"#9c179e"],[0.4444444444444444,"#bd3786"],[0.5555555555555556,"#d8576b"],[0.6666666666666666,"#ed7953"],[0.7777777777777778,"#fb9f3a"],[0.8888888888888888,"#fdca26"],[1.0,"#f0f921"]]}],"contourcarpet":[{"colorbar":{"outlinewidth":0.0,"ticks":""}}],"heatmap":[{"colorbar":{"outlinewidth":0.0,"ticks":""},"colorscale":[[0.0,"#0d0887"],[0.1111111111111111,"#46039f"],[0.2222222222222222,"#7201a8"],[0.3333333333333333,"#9c179e"],[0.4444444444444444,"#bd3786"],[0.5555555555555556,"#d8576b"],[0.6666666666666666,"#ed7953"],[0.7777777777777778,"#fb9f3a"],[0.8888888888888888,"#fdca26"],[1.0,"#f0f921"]]}],"heatmapgl":[{"colorbar":{"outlinewidth":0.0,"ticks":""},"colorscale":[[0.0,"#0d0887"],[0.1111111111111111,"#46039f"],[0.2222222222222222,"#7201a8"],[0.3333333333333333,"#9c179e"],[0.4444444444444444,"#bd3786"],[0.5555555555555556,"#d8576b"],[0.6666666666666666,"#ed7953"],[0.7777777777777778,"#fb9f3a"],[0.8888888888888888,"#fdca26"],[1.0,"#f0f921"]]}],"histogram":[{"marker":{"pattern":{"fillmode":"overlay","size":10,"solidity":0.2}}}],"histogram2d":[{"colorbar":{"outlinewidth":0.0,"ticks":""},"colorscale":[[0.0,"#0d0887"],[0.1111111111111111,"#46039f"],[0.2222222222222222,"#7201a8"],[0.3333333333333333,"#9c179e"],[0.4444444444444444,"#bd3786"],[0.5555555555555556,"#d8576b"],[0.6666666666666666,"#ed7953"],[0.7777777777777778,"#fb9f3a"],[0.8888888888888888,"#fdca26"],[1.0,"#f0f921"]]}],"histogram2dcontour":[{"colorbar":{"outlinewidth":0.0,"ticks":""},"colorscale":[[0.0,"#0d0887"],[0.1111111111111111,"#46039f"],[0.2222222222222222,"#7201a8"],[0.3333333333333333,"#9c179e"],[0.4444444444444444,"#bd3786"],[0.5555555555555556,"#d8576b"],[0.6666666666666666,"#ed7953"],[0.7777777777777778,"#fb9f3a"],[0.8888888888888888,"#fdca26"],[1.0,"#f0f921"]]}],"mesh3d":[{"colorbar":{"outlinewidth":0.0,"ticks":""}}],"parcoords":[{"line":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"pie":[{"automargin":true}],"scatter":[{"marker":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"scatter3d":[{"marker":{"colorbar":{"outlinewidth":0.0,"ticks":""}},"line":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"scattercarpet":[{"marker":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"scattergeo":[{"marker":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"scattergl":[{"marker":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"scattermapbox":[{"marker":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"scatterpolar":[{"marker":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"scatterpolargl":[{"marker":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"scatterternary":[{"marker":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"surface":[{"colorbar":{"outlinewidth":0.0,"ticks":""},"colorscale":[[0.0,"#0d0887"],[0.1111111111111111,"#46039f"],[0.2222222222222222,"#7201a8"],[0.3333333333333333,"#9c179e"],[0.4444444444444444,"#bd3786"],[0.5555555555555556,"#d8576b"],[0.6666666666666666,"#ed7953"],[0.7777777777777778,"#fb9f3a"],[0.8888888888888888,"#fdca26"],[1.0,"#f0f921"]]}],"table":[{"cells":{"fill":{"color":"rgba(235, 240, 248, 1.0)"},"line":{"color":"rgba(255, 255, 255, 1.0)"}},"header":{"fill":{"color":"rgba(200, 212, 227, 1.0)"},"line":{"color":"rgba(255, 255, 255, 1.0)"}}}]}},"shapes":[{"type":"circle","xref":"x","x0":0.0,"x1":2.0,"yref":"y","y0":0.0,"y1":2.0,"opacity":0.3,"line":{"color":"rgba(0, 0, 255, 1.0)"},"fillcolor":"rgba(0, 0, 255, 1.0)"},{"type":"circle","xref":"x","x0":1.5,"x1":3.5,"yref":"y","y0":0.0,"y1":2.0,"opacity":0.3,"line":{"color":"rgba(255, 0, 0, 1.0)"},"fillcolor":"rgba(255, 0, 0, 1.0)"},{"type":"circle","xref":"x","x0":0.75,"x1":2.75,"yref":"y","y0":1.3,"y1":3.3,"opacity":0.3,"line":{"color":"rgba(0, 128, 0, 1.0)"},"fillcolor":"rgba(0, 128, 0, 1.0)"}],"xaxis":{"showticklabels":false,"showgrid":false,"zeroline":false},"yaxis":{"showticklabels":false,"showgrid":false,"zeroline":false}};
          var config = {"responsive":true};
          Plotly.newPlot('9c33a2a9-282c-43ce-b2a7-d204c9312c02', data, layout, config);
});
          };
          if ((typeof(requirejs) !==  typeof(Function)) || (typeof(requirejs.config) !== typeof(Function))) {
              var script = document.createElement("script");
              script.setAttribute("src", "https://cdnjs.cloudflare.com/ajax/libs/require.js/2.3.6/require.min.js");
              script.onload = function(){
                  renderPlotly_9c33a2a9282c43ceb2a7d204c9312c02();
              };
              document.getElementsByTagName("head")[0].appendChild(script);
          }
          else {
              renderPlotly_9c33a2a9282c43ceb2a7d204c9312c02();
          }
</script>
"""
(***include-it-raw***)

(**
## Visualizing set relationships with UpSet plots
### Construction UpSet

Since Venn diagrams with more than three sets are increasingly difficult to model and read, BioFSharp.Vis also includes UpSet plots. 
UpSet plots consist of three basic parts. The first is a matrix representing the intersection between sets. Each row corresponds to a set and each column to 
an intersection. Sets taht are part of that particular intersection are marked with a filled in dot and connected by a line. We can try to create the 
intersection matrix for the three sets used in the previous Venn diagramm. We start again by computing the intersections.
*)

let intersections = Venn.ofSetList [|"Action";"Comedy";"Drama"|] [|actionSet;comedySet;dramaSet|]

(**
Now we need the sets that are part of each intersection. We also need a row position for each set in the matrix.
*)

let intersectingSets =
    intersections
    |> Map.toArray
    |> Array.map (snd >> (fun v -> v.Label))
    |> Array.filter (List.isEmpty >> not)

(***hide***)
intersectingSets
(*** include-it ***)

let setPositions =
    [|
        "Action", 0
        "Comedy", 1
        "Drama" , 2
    |]

(**
With this information we can create the first column of the intersection matrix:
*)

let createIntersectionMatrixPart (setPos: (string*int)[]) (iSet: string list) (position: int) =
    // Creates the part of the intersection matrix representing the current intersection. 
    // The position on the y-Axis is based on the order the labels and sets are given in. 
    // The position on the x-Axis is based on the given position (determined by intersection size).
    UpSetParts.createIntersectionPlotPart
        position
        iSet
        setPos
        25
        (Color.fromKeyword DarkBlue)
        (Color.fromKeyword LightBlue)

let intersectionMatrixPart =
    createIntersectionMatrixPart
        setPositions
        intersectingSets.[0]
        0
intersectionMatrixPart
|> Chart.show
(***hide***)
"""<div id="e2ab3c14-e879-493a-b96a-c2d96ad35a47"><!-- Plotly chart will be drawn inside this DIV --></div>
<script type="text/javascript">

          var renderPlotly_e2ab3c14e879493ab96ac2d96ad35a47 = function() {
          var fsharpPlotlyRequire = requirejs.config({context:'fsharp-plotly',paths:{plotly:'https://cdn.plot.ly/plotly-2.6.3.min'}}) || require;
          fsharpPlotlyRequire(['plotly'], function(Plotly) {

          var data = [{"type":"scatter","mode":"markers","x":[0,0],"y":[1,2],"marker":{"color":"rgba(173, 216, 230, 1.0)","size":25,"symbol":"0"},"line":{}},{"type":"scatter","mode":"lines+markers","x":[0],"y":[0],"marker":{"size":25,"symbol":"0"},"line":{"color":"rgba(0, 0, 139, 1.0)","width":5.0,"dash":"solid"}}];
          var layout = {"width":600,"height":600,"template":{"layout":{"paper_bgcolor":"white","plot_bgcolor":"white","xaxis":{"showline":true,"zeroline":true},"yaxis":{"showline":true,"zeroline":true}},"data":{}}};
          var config = {"responsive":true};
          Plotly.newPlot('e2ab3c14-e879-493a-b96a-c2d96ad35a47', data, layout, config);
});
          };
          if ((typeof(requirejs) !==  typeof(Function)) || (typeof(requirejs.config) !== typeof(Function))) {
              var script = document.createElement("script");
              script.setAttribute("src", "https://cdnjs.cloudflare.com/ajax/libs/require.js/2.3.6/require.min.js");
              script.onload = function(){
                  renderPlotly_e2ab3c14e879493ab96ac2d96ad35a47();
              };
              document.getElementsByTagName("head")[0].appendChild(script);
          }
          else {
              renderPlotly_e2ab3c14e879493ab96ac2d96ad35a47();
          }
</script>
"""
(***include-it-raw***)

(**
We can apply this function now to all intersections and add the correct labels to the rows:
*)

let intersectionMatrix =
    intersectingSets
    |> Array.mapi (fun i iS ->
        createIntersectionMatrixPart
            setPositions
            iS
            i
    )
    |> Chart.combine
    // Axis styling
    |> Chart.withYAxis (
        LinearAxis.init(
            ShowGrid=false,
            ShowLine=false,
            ShowTickLabels=true,
            ZeroLine=false,
            TickMode=StyleParam.TickMode.Array,
            TickVals=[0 .. setPositions.Length - 1],
            TickText=(setPositions |> Array.map fst)
        )
    )
    |> Chart.withXAxis (
        LinearAxis.init(
            ShowGrid=false,
            ShowLine=false,
            ShowTickLabels=false,
            ZeroLine=false,
            Domain=StyleParam.Range.MinMax (0.4,1.)
        )
    )
    |> Chart.withLegend false

(***hide***)
"""<div id="49cfa00c-cbb6-4fc6-aa5b-f7460fc43ab7"><!-- Plotly chart will be drawn inside this DIV --></div>
<script type="text/javascript">

          var renderPlotly_49cfa00ccbb64fc6aa5bf7460fc43ab7 = function() {
          var fsharpPlotlyRequire = requirejs.config({context:'fsharp-plotly',paths:{plotly:'https://cdn.plot.ly/plotly-2.6.3.min'}}) || require;
          fsharpPlotlyRequire(['plotly'], function(Plotly) {

          var data = [{"type":"scatter","mode":"markers","x":[0,0],"y":[1,2],"marker":{"color":"rgba(173, 216, 230, 1.0)","size":25,"symbol":"0"},"line":{},"xaxis":"x4","yaxis":"y4"},{"type":"scatter","mode":"lines+markers","x":[0],"y":[0],"marker":{"size":25,"symbol":"0"},"line":{"color":"rgba(0, 0, 139, 1.0)","width":5.0,"dash":"solid"},"xaxis":"x4","yaxis":"y4"},{"type":"scatter","mode":"markers","x":[1],"y":[2],"marker":{"color":"rgba(173, 216, 230, 1.0)","size":25,"symbol":"0"},"line":{},"xaxis":"x4","yaxis":"y4"},{"type":"scatter","mode":"lines+markers","x":[1,1],"y":[0,1],"marker":{"size":25,"symbol":"0"},"line":{"color":"rgba(0, 0, 139, 1.0)","width":5.0,"dash":"solid"},"xaxis":"x4","yaxis":"y4"},{"type":"scatter","mode":"markers","x":[],"y":[],"marker":{"color":"rgba(173, 216, 230, 1.0)","size":25,"symbol":"0"},"line":{},"xaxis":"x4","yaxis":"y4"},{"type":"scatter","mode":"lines+markers","x":[2,2,2],"y":[0,1,2],"marker":{"size":25,"symbol":"0"},"line":{"color":"rgba(0, 0, 139, 1.0)","width":5.0,"dash":"solid"},"xaxis":"x4","yaxis":"y4"},{"type":"scatter","mode":"markers","x":[3],"y":[1],"marker":{"color":"rgba(173, 216, 230, 1.0)","size":25,"symbol":"0"},"line":{},"xaxis":"x4","yaxis":"y4"},{"type":"scatter","mode":"lines+markers","x":[3,3],"y":[0,2],"marker":{"size":25,"symbol":"0"},"line":{"color":"rgba(0, 0, 139, 1.0)","width":5.0,"dash":"solid"},"xaxis":"x4","yaxis":"y4"},{"type":"scatter","mode":"markers","x":[4,4],"y":[0,2],"marker":{"color":"rgba(173, 216, 230, 1.0)","size":25,"symbol":"0"},"line":{},"xaxis":"x4","yaxis":"y4"},{"type":"scatter","mode":"lines+markers","x":[4],"y":[1],"marker":{"size":25,"symbol":"0"},"line":{"color":"rgba(0, 0, 139, 1.0)","width":5.0,"dash":"solid"},"xaxis":"x4","yaxis":"y4"},{"type":"scatter","mode":"markers","x":[5],"y":[0],"marker":{"color":"rgba(173, 216, 230, 1.0)","size":25,"symbol":"0"},"line":{},"xaxis":"x4","yaxis":"y4"},{"type":"scatter","mode":"lines+markers","x":[5,5],"y":[1,2],"marker":{"size":25,"symbol":"0"},"line":{"color":"rgba(0, 0, 139, 1.0)","width":5.0,"dash":"solid"},"xaxis":"x4","yaxis":"y4"},{"type":"scatter","mode":"markers","x":[6,6],"y":[0,1],"marker":{"color":"rgba(173, 216, 230, 1.0)","size":25,"symbol":"0"},"line":{},"xaxis":"x4","yaxis":"y4"},{"type":"scatter","mode":"lines+markers","x":[6],"y":[2],"marker":{"size":25,"symbol":"0"},"line":{"color":"rgba(0, 0, 139, 1.0)","width":5.0,"dash":"solid"},"xaxis":"x4","yaxis":"y4"}];
          var layout = {"width":600,"height":600,"template":{"layout":{"title":{"x":0.05},"font":{"color":"rgba(42, 63, 95, 1.0)"},"paper_bgcolor":"rgba(255, 255, 255, 1.0)","plot_bgcolor":"rgba(229, 236, 246, 1.0)","autotypenumbers":"strict","colorscale":{"diverging":[[0.0,"#8e0152"],[0.1,"#c51b7d"],[0.2,"#de77ae"],[0.3,"#f1b6da"],[0.4,"#fde0ef"],[0.5,"#f7f7f7"],[0.6,"#e6f5d0"],[0.7,"#b8e186"],[0.8,"#7fbc41"],[0.9,"#4d9221"],[1.0,"#276419"]],"sequential":[[0.0,"#0d0887"],[0.1111111111111111,"#46039f"],[0.2222222222222222,"#7201a8"],[0.3333333333333333,"#9c179e"],[0.4444444444444444,"#bd3786"],[0.5555555555555556,"#d8576b"],[0.6666666666666666,"#ed7953"],[0.7777777777777778,"#fb9f3a"],[0.8888888888888888,"#fdca26"],[1.0,"#f0f921"]],"sequentialminus":[[0.0,"#0d0887"],[0.1111111111111111,"#46039f"],[0.2222222222222222,"#7201a8"],[0.3333333333333333,"#9c179e"],[0.4444444444444444,"#bd3786"],[0.5555555555555556,"#d8576b"],[0.6666666666666666,"#ed7953"],[0.7777777777777778,"#fb9f3a"],[0.8888888888888888,"#fdca26"],[1.0,"#f0f921"]]},"hovermode":"closest","hoverlabel":{"align":"left"},"coloraxis":{"colorbar":{"outlinewidth":0.0,"ticks":""}},"geo":{"showland":true,"landcolor":"rgba(229, 236, 246, 1.0)","showlakes":true,"lakecolor":"rgba(255, 255, 255, 1.0)","subunitcolor":"rgba(255, 255, 255, 1.0)","bgcolor":"rgba(255, 255, 255, 1.0)"},"mapbox":{"style":"light"},"polar":{"bgcolor":"rgba(229, 236, 246, 1.0)","radialaxis":{"linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)","ticks":""},"angularaxis":{"linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)","ticks":""}},"scene":{"xaxis":{"ticks":"","linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)","gridwidth":2.0,"zerolinecolor":"rgba(255, 255, 255, 1.0)","backgroundcolor":"rgba(229, 236, 246, 1.0)","showbackground":true},"yaxis":{"ticks":"","linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)","gridwidth":2.0,"zerolinecolor":"rgba(255, 255, 255, 1.0)","backgroundcolor":"rgba(229, 236, 246, 1.0)","showbackground":true},"zaxis":{"ticks":"","linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)","gridwidth":2.0,"zerolinecolor":"rgba(255, 255, 255, 1.0)","backgroundcolor":"rgba(229, 236, 246, 1.0)","showbackground":true}},"ternary":{"aaxis":{"ticks":"","linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)"},"baxis":{"ticks":"","linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)"},"caxis":{"ticks":"","linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)"},"bgcolor":"rgba(229, 236, 246, 1.0)"},"xaxis":{"title":{"standoff":15},"ticks":"","automargin":true,"linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)","zerolinecolor":"rgba(255, 255, 255, 1.0)","zerolinewidth":2.0},"yaxis":{"title":{"standoff":15},"ticks":"","automargin":true,"linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)","zerolinecolor":"rgba(255, 255, 255, 1.0)","zerolinewidth":2.0},"annotationdefaults":{"arrowcolor":"#2a3f5f","arrowhead":0,"arrowwidth":1},"shapedefaults":{"line":{"color":"rgba(42, 63, 95, 1.0)"}},"colorway":["rgba(99, 110, 250, 1.0)","rgba(239, 85, 59, 1.0)","rgba(0, 204, 150, 1.0)","rgba(171, 99, 250, 1.0)","rgba(255, 161, 90, 1.0)","rgba(25, 211, 243, 1.0)","rgba(255, 102, 146, 1.0)","rgba(182, 232, 128, 1.0)","rgba(255, 151, 255, 1.0)","rgba(254, 203, 82, 1.0)"]},"data":{"bar":[{"marker":{"line":{"color":"rgba(229, 236, 246, 1.0)","width":0.5},"pattern":{"fillmode":"overlay","size":10,"solidity":0.2}},"error_x":{"color":"rgba(42, 63, 95, 1.0)"},"error_y":{"color":"rgba(42, 63, 95, 1.0)"}}],"barpolar":[{"marker":{"line":{"color":"rgba(229, 236, 246, 1.0)","width":0.5},"pattern":{"fillmode":"overlay","size":10,"solidity":0.2}}}],"carpet":[{"aaxis":{"linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)","endlinecolor":"rgba(42, 63, 95, 1.0)","minorgridcolor":"rgba(255, 255, 255, 1.0)","startlinecolor":"rgba(42, 63, 95, 1.0)"},"baxis":{"linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)","endlinecolor":"rgba(42, 63, 95, 1.0)","minorgridcolor":"rgba(255, 255, 255, 1.0)","startlinecolor":"rgba(42, 63, 95, 1.0)"}}],"choropleth":[{"colorbar":{"outlinewidth":0.0,"ticks":""},"colorscale":[[0.0,"#0d0887"],[0.1111111111111111,"#46039f"],[0.2222222222222222,"#7201a8"],[0.3333333333333333,"#9c179e"],[0.4444444444444444,"#bd3786"],[0.5555555555555556,"#d8576b"],[0.6666666666666666,"#ed7953"],[0.7777777777777778,"#fb9f3a"],[0.8888888888888888,"#fdca26"],[1.0,"#f0f921"]]}],"contour":[{"colorbar":{"outlinewidth":0.0,"ticks":""},"colorscale":[[0.0,"#0d0887"],[0.1111111111111111,"#46039f"],[0.2222222222222222,"#7201a8"],[0.3333333333333333,"#9c179e"],[0.4444444444444444,"#bd3786"],[0.5555555555555556,"#d8576b"],[0.6666666666666666,"#ed7953"],[0.7777777777777778,"#fb9f3a"],[0.8888888888888888,"#fdca26"],[1.0,"#f0f921"]]}],"contourcarpet":[{"colorbar":{"outlinewidth":0.0,"ticks":""}}],"heatmap":[{"colorbar":{"outlinewidth":0.0,"ticks":""},"colorscale":[[0.0,"#0d0887"],[0.1111111111111111,"#46039f"],[0.2222222222222222,"#7201a8"],[0.3333333333333333,"#9c179e"],[0.4444444444444444,"#bd3786"],[0.5555555555555556,"#d8576b"],[0.6666666666666666,"#ed7953"],[0.7777777777777778,"#fb9f3a"],[0.8888888888888888,"#fdca26"],[1.0,"#f0f921"]]}],"heatmapgl":[{"colorbar":{"outlinewidth":0.0,"ticks":""},"colorscale":[[0.0,"#0d0887"],[0.1111111111111111,"#46039f"],[0.2222222222222222,"#7201a8"],[0.3333333333333333,"#9c179e"],[0.4444444444444444,"#bd3786"],[0.5555555555555556,"#d8576b"],[0.6666666666666666,"#ed7953"],[0.7777777777777778,"#fb9f3a"],[0.8888888888888888,"#fdca26"],[1.0,"#f0f921"]]}],"histogram":[{"marker":{"pattern":{"fillmode":"overlay","size":10,"solidity":0.2}}}],"histogram2d":[{"colorbar":{"outlinewidth":0.0,"ticks":""},"colorscale":[[0.0,"#0d0887"],[0.1111111111111111,"#46039f"],[0.2222222222222222,"#7201a8"],[0.3333333333333333,"#9c179e"],[0.4444444444444444,"#bd3786"],[0.5555555555555556,"#d8576b"],[0.6666666666666666,"#ed7953"],[0.7777777777777778,"#fb9f3a"],[0.8888888888888888,"#fdca26"],[1.0,"#f0f921"]]}],"histogram2dcontour":[{"colorbar":{"outlinewidth":0.0,"ticks":""},"colorscale":[[0.0,"#0d0887"],[0.1111111111111111,"#46039f"],[0.2222222222222222,"#7201a8"],[0.3333333333333333,"#9c179e"],[0.4444444444444444,"#bd3786"],[0.5555555555555556,"#d8576b"],[0.6666666666666666,"#ed7953"],[0.7777777777777778,"#fb9f3a"],[0.8888888888888888,"#fdca26"],[1.0,"#f0f921"]]}],"mesh3d":[{"colorbar":{"outlinewidth":0.0,"ticks":""}}],"parcoords":[{"line":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"pie":[{"automargin":true}],"scatter":[{"marker":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"scatter3d":[{"marker":{"colorbar":{"outlinewidth":0.0,"ticks":""}},"line":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"scattercarpet":[{"marker":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"scattergeo":[{"marker":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"scattergl":[{"marker":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"scattermapbox":[{"marker":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"scatterpolar":[{"marker":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"scatterpolargl":[{"marker":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"scatterternary":[{"marker":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"surface":[{"colorbar":{"outlinewidth":0.0,"ticks":""},"colorscale":[[0.0,"#0d0887"],[0.1111111111111111,"#46039f"],[0.2222222222222222,"#7201a8"],[0.3333333333333333,"#9c179e"],[0.4444444444444444,"#bd3786"],[0.5555555555555556,"#d8576b"],[0.6666666666666666,"#ed7953"],[0.7777777777777778,"#fb9f3a"],[0.8888888888888888,"#fdca26"],[1.0,"#f0f921"]]}],"table":[{"cells":{"fill":{"color":"rgba(235, 240, 248, 1.0)"},"line":{"color":"rgba(255, 255, 255, 1.0)"}},"header":{"fill":{"color":"rgba(200, 212, 227, 1.0)"},"line":{"color":"rgba(255, 255, 255, 1.0)"}}}]}},"xaxis4":{"showticklabels":false,"showline":false,"showgrid":false,"zeroline":false,"domain":[0.4,1.0]},"yaxis4":{"tickmode":"array","tickvals":[0,1,2],"ticktext":["Action","Comedy","Drama"],"showticklabels":true,"showline":false,"showgrid":false,"zeroline":false},"showlegend":false,"xaxis2":{"showticklabels":false,"showline":false,"showgrid":false,"zeroline":false,"domain":[0.4,1.0]},"yaxis2":{"tickmode":"array","tickvals":[0,1,2],"ticktext":["Action","Comedy","Drama"],"showticklabels":true,"showline":false,"showgrid":false,"zeroline":false}};
          var config = {"responsive":true};
          Plotly.newPlot('49cfa00c-cbb6-4fc6-aa5b-f7460fc43ab7', data, layout, config);
});
          };
          if ((typeof(requirejs) !==  typeof(Function)) || (typeof(requirejs.config) !== typeof(Function))) {
              var script = document.createElement("script");
              script.setAttribute("src", "https://cdnjs.cloudflare.com/ajax/libs/require.js/2.3.6/require.min.js");
              script.onload = function(){
                  renderPlotly_49cfa00ccbb64fc6aa5bf7460fc43ab7();
              };
              document.getElementsByTagName("head")[0].appendChild(script);
          }
          else {
              renderPlotly_49cfa00ccbb64fc6aa5bf7460fc43ab7();
          }
</script>
"""
(***include-it-raw***)

(**
The next part is a bar chart representing the size of each set. The bar for each set gets placed next to the row representing the set in the matrix.
*)

let setSizeBar =
    // Creates a bar chart with the set sizes
    UpSetParts.createSetSizePlot
        (setPositions |> Array.map fst)
        [|actionSet;comedySet;dramaSet|]
        2.5
        (Color.fromKeyword DarkBlue)
        (0.,0.3)
        (Font.init(StyleParam.FontFamily.Arial, Size=20.))

[
    setSizeBar
    intersectionMatrix
]
|> Chart.Grid (1,2)
|> Chart.withSize (900.,600.)

(***hide***)
"""<div id="23ed001b-7de9-4b47-ad94-d48dd7847aa7"><!-- Plotly chart will be drawn inside this DIV --></div>
<script type="text/javascript">

          var renderPlotly_23ed001b7de94b47ad94d48dd7847aa7 = function() {
          var fsharpPlotlyRequire = requirejs.config({context:'fsharp-plotly',paths:{plotly:'https://cdn.plot.ly/plotly-2.6.3.min'}}) || require;
          fsharpPlotlyRequire(['plotly'], function(Plotly) {

          var data = [{"type":"bar","x":[503,1200,1603],"y":["Action","Comedy","Drama"],"orientation":"h","marker":{"color":"rgba(0, 0, 139, 1.0)","pattern":{}},"showlegend":false,"xaxis":"x","yaxis":"y"},{"type":"scatter","mode":"markers","x":[0,0],"y":[1,2],"marker":{"color":"rgba(173, 216, 230, 1.0)","size":25,"symbol":"0"},"line":{},"xaxis":"x2","yaxis":"y2"},{"type":"scatter","mode":"lines+markers","x":[0],"y":[0],"marker":{"size":25,"symbol":"0"},"line":{"color":"rgba(0, 0, 139, 1.0)","width":5.0,"dash":"solid"},"xaxis":"x2","yaxis":"y2"},{"type":"scatter","mode":"markers","x":[1],"y":[2],"marker":{"color":"rgba(173, 216, 230, 1.0)","size":25,"symbol":"0"},"line":{},"xaxis":"x2","yaxis":"y2"},{"type":"scatter","mode":"lines+markers","x":[1,1],"y":[0,1],"marker":{"size":25,"symbol":"0"},"line":{"color":"rgba(0, 0, 139, 1.0)","width":5.0,"dash":"solid"},"xaxis":"x2","yaxis":"y2"},{"type":"scatter","mode":"markers","x":[],"y":[],"marker":{"color":"rgba(173, 216, 230, 1.0)","size":25,"symbol":"0"},"line":{},"xaxis":"x2","yaxis":"y2"},{"type":"scatter","mode":"lines+markers","x":[2,2,2],"y":[0,1,2],"marker":{"size":25,"symbol":"0"},"line":{"color":"rgba(0, 0, 139, 1.0)","width":5.0,"dash":"solid"},"xaxis":"x2","yaxis":"y2"},{"type":"scatter","mode":"markers","x":[3],"y":[1],"marker":{"color":"rgba(173, 216, 230, 1.0)","size":25,"symbol":"0"},"line":{},"xaxis":"x2","yaxis":"y2"},{"type":"scatter","mode":"lines+markers","x":[3,3],"y":[0,2],"marker":{"size":25,"symbol":"0"},"line":{"color":"rgba(0, 0, 139, 1.0)","width":5.0,"dash":"solid"},"xaxis":"x2","yaxis":"y2"},{"type":"scatter","mode":"markers","x":[4,4],"y":[0,2],"marker":{"color":"rgba(173, 216, 230, 1.0)","size":25,"symbol":"0"},"line":{},"xaxis":"x2","yaxis":"y2"},{"type":"scatter","mode":"lines+markers","x":[4],"y":[1],"marker":{"size":25,"symbol":"0"},"line":{"color":"rgba(0, 0, 139, 1.0)","width":5.0,"dash":"solid"},"xaxis":"x2","yaxis":"y2"},{"type":"scatter","mode":"markers","x":[5],"y":[0],"marker":{"color":"rgba(173, 216, 230, 1.0)","size":25,"symbol":"0"},"line":{},"xaxis":"x2","yaxis":"y2"},{"type":"scatter","mode":"lines+markers","x":[5,5],"y":[1,2],"marker":{"size":25,"symbol":"0"},"line":{"color":"rgba(0, 0, 139, 1.0)","width":5.0,"dash":"solid"},"xaxis":"x2","yaxis":"y2"},{"type":"scatter","mode":"markers","x":[6,6],"y":[0,1],"marker":{"color":"rgba(173, 216, 230, 1.0)","size":25,"symbol":"0"},"line":{},"xaxis":"x2","yaxis":"y2"},{"type":"scatter","mode":"lines+markers","x":[6],"y":[2],"marker":{"size":25,"symbol":"0"},"line":{"color":"rgba(0, 0, 139, 1.0)","width":5.0,"dash":"solid"},"xaxis":"x2","yaxis":"y2"}];
          var layout = {"width":900,"height":600,"template":{"layout":{"title":{"x":0.05},"font":{"color":"rgba(42, 63, 95, 1.0)"},"paper_bgcolor":"rgba(255, 255, 255, 1.0)","plot_bgcolor":"rgba(229, 236, 246, 1.0)","autotypenumbers":"strict","colorscale":{"diverging":[[0.0,"#8e0152"],[0.1,"#c51b7d"],[0.2,"#de77ae"],[0.3,"#f1b6da"],[0.4,"#fde0ef"],[0.5,"#f7f7f7"],[0.6,"#e6f5d0"],[0.7,"#b8e186"],[0.8,"#7fbc41"],[0.9,"#4d9221"],[1.0,"#276419"]],"sequential":[[0.0,"#0d0887"],[0.1111111111111111,"#46039f"],[0.2222222222222222,"#7201a8"],[0.3333333333333333,"#9c179e"],[0.4444444444444444,"#bd3786"],[0.5555555555555556,"#d8576b"],[0.6666666666666666,"#ed7953"],[0.7777777777777778,"#fb9f3a"],[0.8888888888888888,"#fdca26"],[1.0,"#f0f921"]],"sequentialminus":[[0.0,"#0d0887"],[0.1111111111111111,"#46039f"],[0.2222222222222222,"#7201a8"],[0.3333333333333333,"#9c179e"],[0.4444444444444444,"#bd3786"],[0.5555555555555556,"#d8576b"],[0.6666666666666666,"#ed7953"],[0.7777777777777778,"#fb9f3a"],[0.8888888888888888,"#fdca26"],[1.0,"#f0f921"]]},"hovermode":"closest","hoverlabel":{"align":"left"},"coloraxis":{"colorbar":{"outlinewidth":0.0,"ticks":""}},"geo":{"showland":true,"landcolor":"rgba(229, 236, 246, 1.0)","showlakes":true,"lakecolor":"rgba(255, 255, 255, 1.0)","subunitcolor":"rgba(255, 255, 255, 1.0)","bgcolor":"rgba(255, 255, 255, 1.0)"},"mapbox":{"style":"light"},"polar":{"bgcolor":"rgba(229, 236, 246, 1.0)","radialaxis":{"linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)","ticks":""},"angularaxis":{"linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)","ticks":""}},"scene":{"xaxis":{"ticks":"","linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)","gridwidth":2.0,"zerolinecolor":"rgba(255, 255, 255, 1.0)","backgroundcolor":"rgba(229, 236, 246, 1.0)","showbackground":true},"yaxis":{"ticks":"","linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)","gridwidth":2.0,"zerolinecolor":"rgba(255, 255, 255, 1.0)","backgroundcolor":"rgba(229, 236, 246, 1.0)","showbackground":true},"zaxis":{"ticks":"","linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)","gridwidth":2.0,"zerolinecolor":"rgba(255, 255, 255, 1.0)","backgroundcolor":"rgba(229, 236, 246, 1.0)","showbackground":true}},"ternary":{"aaxis":{"ticks":"","linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)"},"baxis":{"ticks":"","linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)"},"caxis":{"ticks":"","linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)"},"bgcolor":"rgba(229, 236, 246, 1.0)"},"xaxis":{"title":{"standoff":15},"ticks":"","automargin":true,"linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)","zerolinecolor":"rgba(255, 255, 255, 1.0)","zerolinewidth":2.0},"yaxis":{"title":{"standoff":15},"ticks":"","automargin":true,"linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)","zerolinecolor":"rgba(255, 255, 255, 1.0)","zerolinewidth":2.0},"annotationdefaults":{"arrowcolor":"#2a3f5f","arrowhead":0,"arrowwidth":1},"shapedefaults":{"line":{"color":"rgba(42, 63, 95, 1.0)"}},"colorway":["rgba(99, 110, 250, 1.0)","rgba(239, 85, 59, 1.0)","rgba(0, 204, 150, 1.0)","rgba(171, 99, 250, 1.0)","rgba(255, 161, 90, 1.0)","rgba(25, 211, 243, 1.0)","rgba(255, 102, 146, 1.0)","rgba(182, 232, 128, 1.0)","rgba(255, 151, 255, 1.0)","rgba(254, 203, 82, 1.0)"]},"data":{"bar":[{"marker":{"line":{"color":"rgba(229, 236, 246, 1.0)","width":0.5},"pattern":{"fillmode":"overlay","size":10,"solidity":0.2}},"error_x":{"color":"rgba(42, 63, 95, 1.0)"},"error_y":{"color":"rgba(42, 63, 95, 1.0)"}}],"barpolar":[{"marker":{"line":{"color":"rgba(229, 236, 246, 1.0)","width":0.5},"pattern":{"fillmode":"overlay","size":10,"solidity":0.2}}}],"carpet":[{"aaxis":{"linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)","endlinecolor":"rgba(42, 63, 95, 1.0)","minorgridcolor":"rgba(255, 255, 255, 1.0)","startlinecolor":"rgba(42, 63, 95, 1.0)"},"baxis":{"linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)","endlinecolor":"rgba(42, 63, 95, 1.0)","minorgridcolor":"rgba(255, 255, 255, 1.0)","startlinecolor":"rgba(42, 63, 95, 1.0)"}}],"choropleth":[{"colorbar":{"outlinewidth":0.0,"ticks":""},"colorscale":[[0.0,"#0d0887"],[0.1111111111111111,"#46039f"],[0.2222222222222222,"#7201a8"],[0.3333333333333333,"#9c179e"],[0.4444444444444444,"#bd3786"],[0.5555555555555556,"#d8576b"],[0.6666666666666666,"#ed7953"],[0.7777777777777778,"#fb9f3a"],[0.8888888888888888,"#fdca26"],[1.0,"#f0f921"]]}],"contour":[{"colorbar":{"outlinewidth":0.0,"ticks":""},"colorscale":[[0.0,"#0d0887"],[0.1111111111111111,"#46039f"],[0.2222222222222222,"#7201a8"],[0.3333333333333333,"#9c179e"],[0.4444444444444444,"#bd3786"],[0.5555555555555556,"#d8576b"],[0.6666666666666666,"#ed7953"],[0.7777777777777778,"#fb9f3a"],[0.8888888888888888,"#fdca26"],[1.0,"#f0f921"]]}],"contourcarpet":[{"colorbar":{"outlinewidth":0.0,"ticks":""}}],"heatmap":[{"colorbar":{"outlinewidth":0.0,"ticks":""},"colorscale":[[0.0,"#0d0887"],[0.1111111111111111,"#46039f"],[0.2222222222222222,"#7201a8"],[0.3333333333333333,"#9c179e"],[0.4444444444444444,"#bd3786"],[0.5555555555555556,"#d8576b"],[0.6666666666666666,"#ed7953"],[0.7777777777777778,"#fb9f3a"],[0.8888888888888888,"#fdca26"],[1.0,"#f0f921"]]}],"heatmapgl":[{"colorbar":{"outlinewidth":0.0,"ticks":""},"colorscale":[[0.0,"#0d0887"],[0.1111111111111111,"#46039f"],[0.2222222222222222,"#7201a8"],[0.3333333333333333,"#9c179e"],[0.4444444444444444,"#bd3786"],[0.5555555555555556,"#d8576b"],[0.6666666666666666,"#ed7953"],[0.7777777777777778,"#fb9f3a"],[0.8888888888888888,"#fdca26"],[1.0,"#f0f921"]]}],"histogram":[{"marker":{"pattern":{"fillmode":"overlay","size":10,"solidity":0.2}}}],"histogram2d":[{"colorbar":{"outlinewidth":0.0,"ticks":""},"colorscale":[[0.0,"#0d0887"],[0.1111111111111111,"#46039f"],[0.2222222222222222,"#7201a8"],[0.3333333333333333,"#9c179e"],[0.4444444444444444,"#bd3786"],[0.5555555555555556,"#d8576b"],[0.6666666666666666,"#ed7953"],[0.7777777777777778,"#fb9f3a"],[0.8888888888888888,"#fdca26"],[1.0,"#f0f921"]]}],"histogram2dcontour":[{"colorbar":{"outlinewidth":0.0,"ticks":""},"colorscale":[[0.0,"#0d0887"],[0.1111111111111111,"#46039f"],[0.2222222222222222,"#7201a8"],[0.3333333333333333,"#9c179e"],[0.4444444444444444,"#bd3786"],[0.5555555555555556,"#d8576b"],[0.6666666666666666,"#ed7953"],[0.7777777777777778,"#fb9f3a"],[0.8888888888888888,"#fdca26"],[1.0,"#f0f921"]]}],"mesh3d":[{"colorbar":{"outlinewidth":0.0,"ticks":""}}],"parcoords":[{"line":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"pie":[{"automargin":true}],"scatter":[{"marker":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"scatter3d":[{"marker":{"colorbar":{"outlinewidth":0.0,"ticks":""}},"line":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"scattercarpet":[{"marker":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"scattergeo":[{"marker":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"scattergl":[{"marker":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"scattermapbox":[{"marker":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"scatterpolar":[{"marker":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"scatterpolargl":[{"marker":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"scatterternary":[{"marker":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"surface":[{"colorbar":{"outlinewidth":0.0,"ticks":""},"colorscale":[[0.0,"#0d0887"],[0.1111111111111111,"#46039f"],[0.2222222222222222,"#7201a8"],[0.3333333333333333,"#9c179e"],[0.4444444444444444,"#bd3786"],[0.5555555555555556,"#d8576b"],[0.6666666666666666,"#ed7953"],[0.7777777777777778,"#fb9f3a"],[0.8888888888888888,"#fdca26"],[1.0,"#f0f921"]]}],"table":[{"cells":{"fill":{"color":"rgba(235, 240, 248, 1.0)"},"line":{"color":"rgba(255, 255, 255, 1.0)"}},"header":{"fill":{"color":"rgba(200, 212, 227, 1.0)"},"line":{"color":"rgba(255, 255, 255, 1.0)"}}}]}},"yaxis":{},"xaxis":{},"showlegend":false,"xaxis2":{"showticklabels":false,"showline":false,"showgrid":false,"zeroline":false,"domain":[0.4,1.0]},"yaxis2":{"tickmode":"array","tickvals":[0,1,2],"ticktext":["Action","Comedy","Drama"],"showticklabels":true,"showline":false,"showgrid":false,"zeroline":false},"grid":{"rows":1,"columns":2,"pattern":"independent"},"xaxis3":{"title":{"text":"Set Size","font":{"family":"Arial","size":20.0}},"range":[1603.0,0.0],"domain":[0.0,0.3]},"yaxis3":{"range":[-0.5,2.5],"showticklabels":false,"showline":false,"showgrid":false,"zeroline":false},"xaxis4":{"showticklabels":false,"showline":false,"showgrid":false,"zeroline":false,"domain":[0.4,1.0]},"yaxis4":{"tickmode":"array","tickvals":[0,1,2],"ticktext":["Action","Comedy","Drama"],"showticklabels":true,"showline":false,"showgrid":false,"zeroline":false}};
          var config = {"responsive":true};
          Plotly.newPlot('23ed001b-7de9-4b47-ad94-d48dd7847aa7', data, layout, config);
});
          };
          if ((typeof(requirejs) !==  typeof(Function)) || (typeof(requirejs.config) !== typeof(Function))) {
              var script = document.createElement("script");
              script.setAttribute("src", "https://cdnjs.cloudflare.com/ajax/libs/require.js/2.3.6/require.min.js");
              script.onload = function(){
                  renderPlotly_23ed001b7de94b47ad94d48dd7847aa7();
              };
              document.getElementsByTagName("head")[0].appendChild(script);
          }
          else {
              renderPlotly_23ed001b7de94b47ad94d48dd7847aa7();
          }
</script>
"""
(***include-it-raw***)

(**
Lastly we come to our third basic part. It is a bar chart representing the size of each intersection, which it placed atop of the column representing each intersection.
*)

let intersectionCounts =
    intersections
    |> Map.toArray
    |> Array.map (fun (_,labelSet) -> 
        labelSet.Label, labelSet.Set.Count
    )
    |> Array.filter (fun (id,_) -> not id.IsEmpty)

let intersectionSizeBar =
    // Creates a bar chart with the intersection sizes
    UpSetParts.createIntersectionSizePlots
        intersectionCounts
        (float intersectionCounts.Length - 0.5)
        (Color.fromKeyword DarkBlue)
        (0.4, 1.)
        (Font.init(StyleParam.FontFamily.Arial, Size=20.))

[|
    Chart.Invisible()
    intersectionSizeBar
    setSizeBar
    intersectionMatrix
|]
|> Chart.Grid(2,2)
|> Chart.withSize (900.,600.)

(***hide***)
"""<div id="c988a9ba-ea3b-4e4b-8906-50df25881023"><!-- Plotly chart will be drawn inside this DIV --></div>
<script type="text/javascript">

          var renderPlotly_c988a9baea3b4e4b890650df25881023 = function() {
          var fsharpPlotlyRequire = requirejs.config({context:'fsharp-plotly',paths:{plotly:'https://cdn.plot.ly/plotly-2.6.3.min'}}) || require;
          fsharpPlotlyRequire(['plotly'], function(Plotly) {

          var data = [{"type":null,"xaxis":"x","yaxis":"y"},{"type":"bar","y":[348,55,10,90,919,216,1287],"orientation":"v","marker":{"color":"rgba(0, 0, 139, 1.0)","pattern":{}},"showlegend":false,"xaxis":"x2","yaxis":"y2"},{"type":"bar","x":[503,1200,1603],"y":["Action","Comedy","Drama"],"orientation":"h","marker":{"color":"rgba(0, 0, 139, 1.0)","pattern":{}},"showlegend":false,"xaxis":"x3","yaxis":"y3"},{"type":"scatter","mode":"markers","x":[0,0],"y":[1,2],"marker":{"color":"rgba(173, 216, 230, 1.0)","size":25,"symbol":"0"},"line":{},"xaxis":"x4","yaxis":"y4"},{"type":"scatter","mode":"lines+markers","x":[0],"y":[0],"marker":{"size":25,"symbol":"0"},"line":{"color":"rgba(0, 0, 139, 1.0)","width":5.0,"dash":"solid"},"xaxis":"x4","yaxis":"y4"},{"type":"scatter","mode":"markers","x":[1],"y":[2],"marker":{"color":"rgba(173, 216, 230, 1.0)","size":25,"symbol":"0"},"line":{},"xaxis":"x4","yaxis":"y4"},{"type":"scatter","mode":"lines+markers","x":[1,1],"y":[0,1],"marker":{"size":25,"symbol":"0"},"line":{"color":"rgba(0, 0, 139, 1.0)","width":5.0,"dash":"solid"},"xaxis":"x4","yaxis":"y4"},{"type":"scatter","mode":"markers","x":[],"y":[],"marker":{"color":"rgba(173, 216, 230, 1.0)","size":25,"symbol":"0"},"line":{},"xaxis":"x4","yaxis":"y4"},{"type":"scatter","mode":"lines+markers","x":[2,2,2],"y":[0,1,2],"marker":{"size":25,"symbol":"0"},"line":{"color":"rgba(0, 0, 139, 1.0)","width":5.0,"dash":"solid"},"xaxis":"x4","yaxis":"y4"},{"type":"scatter","mode":"markers","x":[3],"y":[1],"marker":{"color":"rgba(173, 216, 230, 1.0)","size":25,"symbol":"0"},"line":{},"xaxis":"x4","yaxis":"y4"},{"type":"scatter","mode":"lines+markers","x":[3,3],"y":[0,2],"marker":{"size":25,"symbol":"0"},"line":{"color":"rgba(0, 0, 139, 1.0)","width":5.0,"dash":"solid"},"xaxis":"x4","yaxis":"y4"},{"type":"scatter","mode":"markers","x":[4,4],"y":[0,2],"marker":{"color":"rgba(173, 216, 230, 1.0)","size":25,"symbol":"0"},"line":{},"xaxis":"x4","yaxis":"y4"},{"type":"scatter","mode":"lines+markers","x":[4],"y":[1],"marker":{"size":25,"symbol":"0"},"line":{"color":"rgba(0, 0, 139, 1.0)","width":5.0,"dash":"solid"},"xaxis":"x4","yaxis":"y4"},{"type":"scatter","mode":"markers","x":[5],"y":[0],"marker":{"color":"rgba(173, 216, 230, 1.0)","size":25,"symbol":"0"},"line":{},"xaxis":"x4","yaxis":"y4"},{"type":"scatter","mode":"lines+markers","x":[5,5],"y":[1,2],"marker":{"size":25,"symbol":"0"},"line":{"color":"rgba(0, 0, 139, 1.0)","width":5.0,"dash":"solid"},"xaxis":"x4","yaxis":"y4"},{"type":"scatter","mode":"markers","x":[6,6],"y":[0,1],"marker":{"color":"rgba(173, 216, 230, 1.0)","size":25,"symbol":"0"},"line":{},"xaxis":"x4","yaxis":"y4"},{"type":"scatter","mode":"lines+markers","x":[6],"y":[2],"marker":{"size":25,"symbol":"0"},"line":{"color":"rgba(0, 0, 139, 1.0)","width":5.0,"dash":"solid"},"xaxis":"x4","yaxis":"y4"}];
          var layout = {"xaxis":{"showticklabels":false,"showline":false,"showgrid":false,"zeroline":false},"yaxis":{"showticklabels":false,"showline":false,"showgrid":false,"zeroline":false},"width":900,"height":600,"template":{"layout":{"title":{"x":0.05},"font":{"color":"rgba(42, 63, 95, 1.0)"},"paper_bgcolor":"rgba(255, 255, 255, 1.0)","plot_bgcolor":"rgba(229, 236, 246, 1.0)","autotypenumbers":"strict","colorscale":{"diverging":[[0.0,"#8e0152"],[0.1,"#c51b7d"],[0.2,"#de77ae"],[0.3,"#f1b6da"],[0.4,"#fde0ef"],[0.5,"#f7f7f7"],[0.6,"#e6f5d0"],[0.7,"#b8e186"],[0.8,"#7fbc41"],[0.9,"#4d9221"],[1.0,"#276419"]],"sequential":[[0.0,"#0d0887"],[0.1111111111111111,"#46039f"],[0.2222222222222222,"#7201a8"],[0.3333333333333333,"#9c179e"],[0.4444444444444444,"#bd3786"],[0.5555555555555556,"#d8576b"],[0.6666666666666666,"#ed7953"],[0.7777777777777778,"#fb9f3a"],[0.8888888888888888,"#fdca26"],[1.0,"#f0f921"]],"sequentialminus":[[0.0,"#0d0887"],[0.1111111111111111,"#46039f"],[0.2222222222222222,"#7201a8"],[0.3333333333333333,"#9c179e"],[0.4444444444444444,"#bd3786"],[0.5555555555555556,"#d8576b"],[0.6666666666666666,"#ed7953"],[0.7777777777777778,"#fb9f3a"],[0.8888888888888888,"#fdca26"],[1.0,"#f0f921"]]},"hovermode":"closest","hoverlabel":{"align":"left"},"coloraxis":{"colorbar":{"outlinewidth":0.0,"ticks":""}},"geo":{"showland":true,"landcolor":"rgba(229, 236, 246, 1.0)","showlakes":true,"lakecolor":"rgba(255, 255, 255, 1.0)","subunitcolor":"rgba(255, 255, 255, 1.0)","bgcolor":"rgba(255, 255, 255, 1.0)"},"mapbox":{"style":"light"},"polar":{"bgcolor":"rgba(229, 236, 246, 1.0)","radialaxis":{"linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)","ticks":""},"angularaxis":{"linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)","ticks":""}},"scene":{"xaxis":{"ticks":"","linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)","gridwidth":2.0,"zerolinecolor":"rgba(255, 255, 255, 1.0)","backgroundcolor":"rgba(229, 236, 246, 1.0)","showbackground":true},"yaxis":{"ticks":"","linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)","gridwidth":2.0,"zerolinecolor":"rgba(255, 255, 255, 1.0)","backgroundcolor":"rgba(229, 236, 246, 1.0)","showbackground":true},"zaxis":{"ticks":"","linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)","gridwidth":2.0,"zerolinecolor":"rgba(255, 255, 255, 1.0)","backgroundcolor":"rgba(229, 236, 246, 1.0)","showbackground":true}},"ternary":{"aaxis":{"ticks":"","linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)"},"baxis":{"ticks":"","linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)"},"caxis":{"ticks":"","linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)"},"bgcolor":"rgba(229, 236, 246, 1.0)"},"xaxis":{"title":{"standoff":15},"ticks":"","automargin":true,"linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)","zerolinecolor":"rgba(255, 255, 255, 1.0)","zerolinewidth":2.0},"yaxis":{"title":{"standoff":15},"ticks":"","automargin":true,"linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)","zerolinecolor":"rgba(255, 255, 255, 1.0)","zerolinewidth":2.0},"annotationdefaults":{"arrowcolor":"#2a3f5f","arrowhead":0,"arrowwidth":1},"shapedefaults":{"line":{"color":"rgba(42, 63, 95, 1.0)"}},"colorway":["rgba(99, 110, 250, 1.0)","rgba(239, 85, 59, 1.0)","rgba(0, 204, 150, 1.0)","rgba(171, 99, 250, 1.0)","rgba(255, 161, 90, 1.0)","rgba(25, 211, 243, 1.0)","rgba(255, 102, 146, 1.0)","rgba(182, 232, 128, 1.0)","rgba(255, 151, 255, 1.0)","rgba(254, 203, 82, 1.0)"]},"data":{"bar":[{"marker":{"line":{"color":"rgba(229, 236, 246, 1.0)","width":0.5},"pattern":{"fillmode":"overlay","size":10,"solidity":0.2}},"error_x":{"color":"rgba(42, 63, 95, 1.0)"},"error_y":{"color":"rgba(42, 63, 95, 1.0)"}}],"barpolar":[{"marker":{"line":{"color":"rgba(229, 236, 246, 1.0)","width":0.5},"pattern":{"fillmode":"overlay","size":10,"solidity":0.2}}}],"carpet":[{"aaxis":{"linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)","endlinecolor":"rgba(42, 63, 95, 1.0)","minorgridcolor":"rgba(255, 255, 255, 1.0)","startlinecolor":"rgba(42, 63, 95, 1.0)"},"baxis":{"linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)","endlinecolor":"rgba(42, 63, 95, 1.0)","minorgridcolor":"rgba(255, 255, 255, 1.0)","startlinecolor":"rgba(42, 63, 95, 1.0)"}}],"choropleth":[{"colorbar":{"outlinewidth":0.0,"ticks":""},"colorscale":[[0.0,"#0d0887"],[0.1111111111111111,"#46039f"],[0.2222222222222222,"#7201a8"],[0.3333333333333333,"#9c179e"],[0.4444444444444444,"#bd3786"],[0.5555555555555556,"#d8576b"],[0.6666666666666666,"#ed7953"],[0.7777777777777778,"#fb9f3a"],[0.8888888888888888,"#fdca26"],[1.0,"#f0f921"]]}],"contour":[{"colorbar":{"outlinewidth":0.0,"ticks":""},"colorscale":[[0.0,"#0d0887"],[0.1111111111111111,"#46039f"],[0.2222222222222222,"#7201a8"],[0.3333333333333333,"#9c179e"],[0.4444444444444444,"#bd3786"],[0.5555555555555556,"#d8576b"],[0.6666666666666666,"#ed7953"],[0.7777777777777778,"#fb9f3a"],[0.8888888888888888,"#fdca26"],[1.0,"#f0f921"]]}],"contourcarpet":[{"colorbar":{"outlinewidth":0.0,"ticks":""}}],"heatmap":[{"colorbar":{"outlinewidth":0.0,"ticks":""},"colorscale":[[0.0,"#0d0887"],[0.1111111111111111,"#46039f"],[0.2222222222222222,"#7201a8"],[0.3333333333333333,"#9c179e"],[0.4444444444444444,"#bd3786"],[0.5555555555555556,"#d8576b"],[0.6666666666666666,"#ed7953"],[0.7777777777777778,"#fb9f3a"],[0.8888888888888888,"#fdca26"],[1.0,"#f0f921"]]}],"heatmapgl":[{"colorbar":{"outlinewidth":0.0,"ticks":""},"colorscale":[[0.0,"#0d0887"],[0.1111111111111111,"#46039f"],[0.2222222222222222,"#7201a8"],[0.3333333333333333,"#9c179e"],[0.4444444444444444,"#bd3786"],[0.5555555555555556,"#d8576b"],[0.6666666666666666,"#ed7953"],[0.7777777777777778,"#fb9f3a"],[0.8888888888888888,"#fdca26"],[1.0,"#f0f921"]]}],"histogram":[{"marker":{"pattern":{"fillmode":"overlay","size":10,"solidity":0.2}}}],"histogram2d":[{"colorbar":{"outlinewidth":0.0,"ticks":""},"colorscale":[[0.0,"#0d0887"],[0.1111111111111111,"#46039f"],[0.2222222222222222,"#7201a8"],[0.3333333333333333,"#9c179e"],[0.4444444444444444,"#bd3786"],[0.5555555555555556,"#d8576b"],[0.6666666666666666,"#ed7953"],[0.7777777777777778,"#fb9f3a"],[0.8888888888888888,"#fdca26"],[1.0,"#f0f921"]]}],"histogram2dcontour":[{"colorbar":{"outlinewidth":0.0,"ticks":""},"colorscale":[[0.0,"#0d0887"],[0.1111111111111111,"#46039f"],[0.2222222222222222,"#7201a8"],[0.3333333333333333,"#9c179e"],[0.4444444444444444,"#bd3786"],[0.5555555555555556,"#d8576b"],[0.6666666666666666,"#ed7953"],[0.7777777777777778,"#fb9f3a"],[0.8888888888888888,"#fdca26"],[1.0,"#f0f921"]]}],"mesh3d":[{"colorbar":{"outlinewidth":0.0,"ticks":""}}],"parcoords":[{"line":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"pie":[{"automargin":true}],"scatter":[{"marker":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"scatter3d":[{"marker":{"colorbar":{"outlinewidth":0.0,"ticks":""}},"line":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"scattercarpet":[{"marker":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"scattergeo":[{"marker":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"scattergl":[{"marker":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"scattermapbox":[{"marker":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"scatterpolar":[{"marker":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"scatterpolargl":[{"marker":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"scatterternary":[{"marker":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"surface":[{"colorbar":{"outlinewidth":0.0,"ticks":""},"colorscale":[[0.0,"#0d0887"],[0.1111111111111111,"#46039f"],[0.2222222222222222,"#7201a8"],[0.3333333333333333,"#9c179e"],[0.4444444444444444,"#bd3786"],[0.5555555555555556,"#d8576b"],[0.6666666666666666,"#ed7953"],[0.7777777777777778,"#fb9f3a"],[0.8888888888888888,"#fdca26"],[1.0,"#f0f921"]]}],"table":[{"cells":{"fill":{"color":"rgba(235, 240, 248, 1.0)"},"line":{"color":"rgba(255, 255, 255, 1.0)"}},"header":{"fill":{"color":"rgba(200, 212, 227, 1.0)"},"line":{"color":"rgba(255, 255, 255, 1.0)"}}}]}},"xaxis2":{"range":[-0.5,6.5],"showticklabels":false,"showline":false,"showgrid":false,"zeroline":false,"domain":[0.4,1.0]},"yaxis2":{"title":{"text":"Intersection Size","font":{"family":"Arial","size":20.0}},"tickmode":"array","tickvals":[0,1,2],"ticktext":["Action","Comedy","Drama"],"showticklabels":true,"showline":false,"showgrid":false,"zeroline":false},"showlegend":false,"grid":{"rows":2,"columns":2,"pattern":"independent"},"xaxis3":{"title":{"text":"Set Size","font":{"family":"Arial","size":20.0}},"range":[1603.0,0.0],"domain":[0.0,0.3]},"yaxis3":{"range":[-0.5,2.5],"showticklabels":false,"showline":false,"showgrid":false,"zeroline":false},"xaxis4":{"showticklabels":false,"showline":false,"showgrid":false,"zeroline":false,"domain":[0.4,1.0]},"yaxis4":{"tickmode":"array","tickvals":[0,1,2],"ticktext":["Action","Comedy","Drama"],"showticklabels":true,"showline":false,"showgrid":false,"zeroline":false}};
          var config = {"responsive":true};
          Plotly.newPlot('c988a9ba-ea3b-4e4b-8906-50df25881023', data, layout, config);
});
          };
          if ((typeof(requirejs) !==  typeof(Function)) || (typeof(requirejs.config) !== typeof(Function))) {
              var script = document.createElement("script");
              script.setAttribute("src", "https://cdnjs.cloudflare.com/ajax/libs/require.js/2.3.6/require.min.js");
              script.onload = function(){
                  renderPlotly_c988a9baea3b4e4b890650df25881023();
              };
              document.getElementsByTagName("head")[0].appendChild(script);
          }
          else {
              renderPlotly_c988a9baea3b4e4b890650df25881023();
          }
</script>
"""
(***include-it-raw***)

(**
### Usage UpSet

We now have a basic UpSet plot. There is also a chart extension for UpSet plot in `BioFSharp.Vis`.
*)

Chart.UpSet(
    [|"Action";"Comedy";"Drama"|],
    [|actionSet;comedySet;dramaSet|]
)
|> Chart.withSize (1400, 800)
|> Chart.withTemplate ChartTemplates.light

(***hide***)
"""<div id="e6ff1f79-2632-4113-a3b9-9e629a38eaad"><!-- Plotly chart will be drawn inside this DIV --></div>
<script type="text/javascript">

          var renderPlotly_e6ff1f7926324113a3b99e629a38eaad = function() {
          var fsharpPlotlyRequire = requirejs.config({context:'fsharp-plotly',paths:{plotly:'https://cdn.plot.ly/plotly-2.6.3.min'}}) || require;
          fsharpPlotlyRequire(['plotly'], function(Plotly) {

          var data = [{"type":null,"xaxis":"x","yaxis":"y"},{"type":"bar","y":[1287,919,348,216,90,55,10],"orientation":"v","marker":{"color":"rgba(0, 0, 139, 1.0)","pattern":{}},"showlegend":false,"xaxis":"x2","yaxis":"y2"},{"type":"bar","x":[503,1200,1603],"y":["Action","Comedy","Drama"],"orientation":"h","marker":{"color":"rgba(0, 0, 139, 1.0)","pattern":{}},"showlegend":false,"xaxis":"x3","yaxis":"y3"},{"type":"scatter","mode":"markers","x":[0,0],"y":[0,1],"marker":{"color":"rgba(173, 216, 230, 1.0)","size":25,"symbol":"0"},"line":{},"showlegend":false,"xaxis":"x4","yaxis":"y4"},{"type":"scatter","mode":"lines+markers","x":[0],"y":[2],"marker":{"size":25,"symbol":"0"},"line":{"color":"rgba(0, 0, 139, 1.0)","width":5.0,"dash":"solid"},"showlegend":false,"xaxis":"x4","yaxis":"y4"},{"type":"scatter","mode":"markers","x":[1,1],"y":[0,2],"marker":{"color":"rgba(173, 216, 230, 1.0)","size":25,"symbol":"0"},"line":{},"showlegend":false,"xaxis":"x4","yaxis":"y4"},{"type":"scatter","mode":"lines+markers","x":[1],"y":[1],"marker":{"size":25,"symbol":"0"},"line":{"color":"rgba(0, 0, 139, 1.0)","width":5.0,"dash":"solid"},"showlegend":false,"xaxis":"x4","yaxis":"y4"},{"type":"scatter","mode":"markers","x":[2,2],"y":[1,2],"marker":{"color":"rgba(173, 216, 230, 1.0)","size":25,"symbol":"0"},"line":{},"showlegend":false,"xaxis":"x4","yaxis":"y4"},{"type":"scatter","mode":"lines+markers","x":[2],"y":[0],"marker":{"size":25,"symbol":"0"},"line":{"color":"rgba(0, 0, 139, 1.0)","width":5.0,"dash":"solid"},"showlegend":false,"xaxis":"x4","yaxis":"y4"},{"type":"scatter","mode":"markers","x":[3],"y":[0],"marker":{"color":"rgba(173, 216, 230, 1.0)","size":25,"symbol":"0"},"line":{},"showlegend":false,"xaxis":"x4","yaxis":"y4"},{"type":"scatter","mode":"lines+markers","x":[3,3],"y":[1,2],"marker":{"size":25,"symbol":"0"},"line":{"color":"rgba(0, 0, 139, 1.0)","width":5.0,"dash":"solid"},"showlegend":false,"xaxis":"x4","yaxis":"y4"},{"type":"scatter","mode":"markers","x":[4],"y":[1],"marker":{"color":"rgba(173, 216, 230, 1.0)","size":25,"symbol":"0"},"line":{},"showlegend":false,"xaxis":"x4","yaxis":"y4"},{"type":"scatter","mode":"lines+markers","x":[4,4],"y":[0,2],"marker":{"size":25,"symbol":"0"},"line":{"color":"rgba(0, 0, 139, 1.0)","width":5.0,"dash":"solid"},"showlegend":false,"xaxis":"x4","yaxis":"y4"},{"type":"scatter","mode":"markers","x":[5],"y":[2],"marker":{"color":"rgba(173, 216, 230, 1.0)","size":25,"symbol":"0"},"line":{},"showlegend":false,"xaxis":"x4","yaxis":"y4"},{"type":"scatter","mode":"lines+markers","x":[5,5],"y":[0,1],"marker":{"size":25,"symbol":"0"},"line":{"color":"rgba(0, 0, 139, 1.0)","width":5.0,"dash":"solid"},"showlegend":false,"xaxis":"x4","yaxis":"y4"},{"type":"scatter","mode":"markers","x":[],"y":[],"marker":{"color":"rgba(173, 216, 230, 1.0)","size":25,"symbol":"0"},"line":{},"showlegend":false,"xaxis":"x4","yaxis":"y4"},{"type":"scatter","mode":"lines+markers","x":[6,6,6],"y":[0,1,2],"marker":{"size":25,"symbol":"0"},"line":{"color":"rgba(0, 0, 139, 1.0)","width":5.0,"dash":"solid"},"showlegend":false,"xaxis":"x4","yaxis":"y4"}];
          var layout = {"xaxis":{"showticklabels":false,"showline":false,"showgrid":false,"zeroline":false},"yaxis":{"showticklabels":false,"showline":false,"showgrid":false,"zeroline":false},"width":1400,"height":800,"template":{"layout":{"paper_bgcolor":"white","plot_bgcolor":"white","xaxis":{"showline":true,"zeroline":true},"yaxis":{"showline":true,"zeroline":true}},"data":{}},"xaxis2":{"range":[-0.5,6.5],"showticklabels":false,"showline":false,"showgrid":false,"zeroline":false,"domain":[0.3,1.0]},"yaxis2":{"title":{"text":"Intersection Size","font":{"family":"Arial","size":20.0}}},"xaxis3":{"title":{"text":"Set Size","font":{"family":"Arial","size":20.0}},"range":[1603.0,0.0],"domain":[0.0,0.2]},"yaxis3":{"range":[-0.5,2.5],"showticklabels":false,"showline":false,"showgrid":false,"zeroline":false},"xaxis4":{"range":[-0.5,6.5],"showticklabels":false,"showline":false,"showgrid":false,"zeroline":false,"domain":[0.3,1.0]},"yaxis4":{"range":[-0.5,2.5],"tickmode":"array","tickvals":[0,1,2],"ticktext":["Action","Comedy","Drama"],"showticklabels":true,"tickfont":{"family":"Arial","size":20.0},"showline":false,"showgrid":false,"zeroline":false},"grid":{"rows":2,"columns":2,"pattern":"independent"}};
          var config = {"responsive":true};
          Plotly.newPlot('e6ff1f79-2632-4113-a3b9-9e629a38eaad', data, layout, config);
});
          };
          if ((typeof(requirejs) !==  typeof(Function)) || (typeof(requirejs.config) !== typeof(Function))) {
              var script = document.createElement("script");
              script.setAttribute("src", "https://cdnjs.cloudflare.com/ajax/libs/require.js/2.3.6/require.min.js");
              script.onload = function(){
                  renderPlotly_e6ff1f7926324113a3b99e629a38eaad();
              };
              document.getElementsByTagName("head")[0].appendChild(script);
          }
          else {
              renderPlotly_e6ff1f7926324113a3b99e629a38eaad();
          }
</script>
"""
(***include-it-raw***)

(**
The UpSet plot can be augmented by different charts representing features of the intersections. We just need a map connecting set elements to the 
feature and a charting function with a title
*)

Chart.UpSet(
    [|"Action";"Comedy";"Drama"|],
    [|actionSet;comedySet;dramaSet|],
    [|(getScoreMap movieFrame)|],
    [|(fun y -> Chart.BoxPlot(Y = y)),"Score"|]
)
|> Chart.withSize (1400., 800.)
|> Chart.withTemplate ChartTemplates.light
|> Chart.show

(**
![heatmap](/img/UpsetScore.png)
*)

(**
We can theoretically plot multiple different features with individual charts for our intersections. We also are not as limited in the number of sets as we are with 
Venn diagrams.Even though the UpSet plot gets also more complex with increasing number of sets, it less extreme than with a Venn diagram. Here is a small example:
*)

(**
![heatmap](/img/UpsetComplex.png)
*)