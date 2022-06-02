(**

---
title: SAM
category: Learning resources
categoryindex: 4
index: 2
---
*)


(***hide***)
#r "nuget: FSharpAux, 1.1.0"
#r "nuget: Plotly.NET, 2.0.0"
#r "nuget: FSharp.Stats, 0.4.7"

open Plotly.NET
open Plotly.NET.StyleParam
open Plotly.NET.LayoutObjects

module Chart = 
    let myAxis name = LinearAxis.init(Title=Title.init name,Mirror=StyleParam.Mirror.All,Ticks=StyleParam.TickOptions.Inside,ShowGrid=false,ShowLine=true)
    let withAxisTitles x y chart = 
        chart 
        |> Chart.withTemplate ChartTemplates.lightMirrored
        |> Chart.withXAxis (myAxis x) 
        |> Chart.withYAxis (myAxis y)
    
(**
# SAM


_[Selina Ziegler](https://github.com/zieglerSe), June 2022_
*)
