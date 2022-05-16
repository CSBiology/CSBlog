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
#r "nuget: Plotly.NET, 2.0.0-preview.12"
#r "nuget: FSharp.Stats, 0.4.1"
#r "nuget: FSharpAux"
#r @"C:\Users\jonat\source\repos\BioFSharp.Vis\bin\BioFSharp.Vis\net47\BioFSharp.Vis.dll"

open Deedle
open Plotly.NET
open Plotly.NET.LayoutObjects
open FSharp.Stats
open BioFSharp.Vis
open BioFSharp.Vis.Upset
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
        Fillcolor = Color.Table.Office.red,
        X0 = 0,
        Y0 = 0,
        X1 = 2,
        Y1 = 2,
        ShapeType = StyleParam.ShapeType.Circle,
        Line = Line.init(Color = Color.Table.Office.red)
    )

let circleComedy =
    Shape.init(
        Opacity = 0.3,
        Xref = "x",
        Yref = "y",
        Fillcolor = Color.Table.Office.blue,
        X0 = 1.5,
        Y0 = 0,
        X1 = 3.5,
        Y1 = 2,
        ShapeType = StyleParam.ShapeType.Circle,
        Line = Line.init(Color = Color.Table.Office.blue)
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
    |> Layout.AddLinearAxis(StyleParam.SubPlotId.XAxis 1, axis)
    |> Layout.AddLinearAxis(StyleParam.SubPlotId.YAxis 1, axis)

(**
Next, we need some text to describe our sets and intersection counts. This can be achieved via `Chart.Scatter`.
*)

let vennChart =
    Chart.Scatter(
        [|1.,1.;2.5,1.;1.75,1.|],
        StyleParam.Mode.Text,
        Labels = ["Action<br>438";"Comedy<br>1135";"65"],
        TextFont = 
            Font.init (
                Family = StyleParam.FontFamily.Arial,
                Size = 18.,
                Color = Color.fromString "black"
            )
    )
    |> Chart.withSize (400.,400.)

(**
We can now complete our Venn diagram by adding our previously created layout to the `Chart.Scatter` 
*)

vennChart
|> Chart.withLayout layout

(***hide***)
vennChart
|> Chart.withLayout layout
|> GenericChart.toChartHTML
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
Chart.Venn (
    [|"Action";"Comedy";"Drama"|],
    [|actionSet;comedySet;dramaSet|]
)
|> GenericChart.toChartHTML
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
    UpsetParts.createIntersectionPlotPart
        position
        iSet
        setPos
        25
        Color.Table.Office.darkBlue
        Color.Table.Office.lightBlue

let intersectionMatrixPart =
    createIntersectionMatrixPart
        setPositions
        intersectingSets.[0]
        0

(***hide***)
intersectionMatrixPart
|> GenericChart.toChartHTML
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
intersectionMatrix
|> GenericChart.toChartHTML
(***include-it-raw***)

(**
The next part is a bar chart representing the size of each set. The bar for each set gets placed next to the row representing the set in the matrix.
*)

let setSizeBar =
    // Creates a bar chart with the set sizes
    UpsetParts.createSetSizePlot
        (setPositions |> Array.map fst)
        [|actionSet;comedySet;dramaSet|]
        2.5
        Color.Table.Office.darkBlue
        (0.,0.3)
        (Font.init(StyleParam.FontFamily.Arial, Size=20.))

[
    setSizeBar
    intersectionMatrix
]
|> Chart.Grid (1,2)
|> Chart.withSize (900.,600.)

(***hide***)
[
    setSizeBar
    intersectionMatrix
]
|> Chart.Grid (1,2)
|> Chart.withSize (900.,600.)
|> GenericChart.toChartHTML
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
    UpsetParts.createIntersectionSizePlots
        intersectionCounts
        (float intersectionCounts.Length - 0.5)
        Color.Table.Office.darkBlue
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
[|
    Chart.Invisible()
    intersectionSizeBar
    setSizeBar
    intersectionMatrix
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
|]
|> Chart.Grid(2,2)
|> Chart.withSize (900.,600.)
|> GenericChart.toChartHTML
(***include-it-raw***)

(**
### Usage UpSet

We now have a basic UpSet plot. There is also a chart extension for UpSet plot in `BioFSharp.Vis`.
*)

Chart.Upset(
    [|"Action";"Comedy";"Drama"|],
    [|actionSet;comedySet;dramaSet|]
)

(**
![heatmap](/img/Upset.png)
*)

(**
The UpSet plot can be augmented by different charts representing features of the intersections. We just need a map connecting set elements to the 
feature and a charting function with a title
*)

Chart.Upset(
    [|"Action";"Comedy";"Drama"|],
    [|actionSet;comedySet;dramaSet|],
    [|(getScoreMap movieFrame)|],
    [|(fun y -> Chart.BoxPlot(y = y)),"Score"|]
)
|> Chart.withSize (1600., 800.)

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