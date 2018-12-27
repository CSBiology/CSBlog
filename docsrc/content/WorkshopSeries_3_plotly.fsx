(***hide***)
#I @"../../packages"
#r @"../../lib/Formatting/FSharp.Plotly.dll"
#r @"FSharpAux/lib/net47/FSharpAux.dll"
open FSharp.Plotly
open FSharpAux

let xSin = [0. .. 0.01 .. (4. * System.Math.PI)]
let ySin = xSin |> List.map sin
let sinChart = 
    Chart.Spline(xSin,ySin)
    |> Chart.withTitle("sin(x)")
    |> Chart.withX_AxisStyle("x",Showgrid=false,Showline=true)
    |> Chart.withY_AxisStyle("y",Showgrid=false,Showline=true)


(**

![HeaderPicture](img/Plotly_HeadPic.png)

#Introduction to Data visualization using FSharp.Plotly
_Kevin Schneider_

In the scope of this post, I will shortly introduce Plotly.js and our FSharp counterpart FSharp.Plotly.
I will go over the basic workflow to generate charts from data and how to style and combine charts. Afterwards we will use
the library to visualize the results we got so far during the course of re-investigating the data of the [paper](https://www.ncbi.nlm.nih.gov/pubmed/25277243).

</br>

##Plotly.js

[Plotly.js](https://plot.ly/javascript/) is an open source charting library written in javascript.

![Plotly.js](img/Plotly_1.png)

Various chart types are supported. From simple scatter and line plots over heatmaps and various 3D-plots to Map charts, 
you will find something fitting your needs for visualization. 

One of the coolest things about these charts is that all of them are interactive, as you can see in the example below. You can Zoom in and out,
resize and move the axis, and much more.
*)

(***include-value:sinChart***)

(**
Plotly does not only generate your charts. It provides a service to change style and data of your chart after generation, which is called the chart studio.
Ýou can access the chart studio by clicking on the 'edit in chart studio' icon above the chart.

</br>

##FSharp.Plotly

[FSharp.Plotly](https://github.com/muehlhaus/FSharp.Plotly) is a FSharp wrapper for Plotly.js. It supports various programming styles
Something morte about it .... SOOOS

</br>

##Chart functions

All chart functions (Point,Line,Heatmap,etc.) are provided as static methods of the `Chart` Class. All of them take the input data and map
to a `GenericChart` type. Creating a chart can be as easy as this:
*)

open FSharp.Plotly

//Create some example data

///x values ranging from 0 to 4π
let xVals = [0. .. 0.01 .. (4. * System.Math.PI)]
///cosine values from 0 to 4 π
let yVals = xVals |> List.map cos

///GenericChart containing an X/Y scatter plot of the values above
let cosChart = Chart.Point(xVals,yVals)

(**

</br>

##Rendering Charts

All cool and good, but how to actually render a chart? This is pretty easy. We simply use the `Chart.Show` function, which can take any
`GenericChart`, generate the respective html file, and display it in your default browser:

*)
(***do-not-eval***)
//Render the chart from the example above in your browser
cosChart
|> Chart.Show
(**

This will display the following chart in your browser:

*)

(***include-value:cosChart***)

(**

</br>

##Styling your Charts

I dont know about you, but i think this chart could look a little bit better.

Styling charts with FSharp.Plotly can be done in multiple ways. We can use the optional parameters when initializing a chart:

*)

open FSharpAux.Colors

let cosChart2 = 
    Chart.Point
        (
            xVals,
            yVals,
            MarkerSymbol = StyleParam.Symbol.Square,
            Color = (Table.Office.lightGreen |> toWebColor),
            TextFont = Font.init(StyleParam.FontFamily.Droid_Sans_Mono)
        )

(***include-value:cosChart2***)

(** 

Or use a more functional style and pipe our chart into styling functions, which have even more control:
We first apply a similar styling as in the example above, but then additionally take control over the axis stylings(title,line style, and grid display):
*)

let sinChart2 =
    Chart.Point(xVals,yVals)
    |> Chart.withMarkerStyle(Size=1,Color=(Table.Office.darkBlue |> toWebColor),Symbol=StyleParam.Symbol.Square)
    |> Chart.withTitle("sin(x)")
    |> Chart.withX_AxisStyle("x",Showline=true,Showgrid=false,MinMax=(0.,(4.* System.Math.PI)))
    |> Chart.withY_AxisStyle("y",Showline=true,Showgrid=false)

(***include-value:sinChart2***)

(**
I am a fan of mirrored axis. There is no option to do that in `Chart.withX_AxisStyle`.
To have even more control over the Axis, we can initialize custom axis:
*)

let myXAxis = 
    Axis.LinearAxis.init(
        Title   = "x",
        Showgrid= false,
        Showline= true,
        Mirror  = StyleParam.Mirror.All,
        Range   = StyleParam.Range.MinMax (0.,(4. * System.Math.PI)),
        Tickmode = StyleParam.TickMode.Array,
        Tickvals = ([|0. .. (0.5 * System.Math.PI) .. (4. * System.Math.PI)|] |> Array.map (round 2)),
        Ticks   = StyleParam.TickOptions.Inside
        )

let myYAxis = 
    Axis.LinearAxis.init(
        Title   = "y",
        Showgrid= false,
        Showline= true,
        Mirror  = StyleParam.Mirror.AllTicks,
        Range   = StyleParam.Range.MinMax (-1.,1.),
        Tickmode = StyleParam.TickMode.Auto,
        Ticks   = StyleParam.TickOptions.Inside
        )

let mirroredSinChart =
    Chart.Point(xVals,yVals)
    |> Chart.withMarkerStyle(Size=1,Color=(Table.Office.darkBlue |> toWebColor),Symbol=StyleParam.Symbol.Square)
    |> Chart.withTitle("sin(x)")
    |> Chart.withX_Axis(myXAxis)
    |> Chart.withY_Axis(myYAxis)
    |> Chart.withSize(750.,750.)

(***include-value:mirroredSinChart***)

(**


</br>

##Multicharts - Combined Charts

</br>

##Multicharts - Stacked Charts

</br>

#Visualizing our data



*)