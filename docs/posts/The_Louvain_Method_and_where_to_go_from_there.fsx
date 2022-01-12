(**

---
title: The Louvain Method and where to go from there
category: Implementation
categoryindex: 3
index: 4
---
*)
(***hide***)

#r "nuget: FSharp.Data"
#r "nuget: Deedle"
#r "nuget: FSharp.Stats"
#r "nuget: Cyjs.NET"
#r "nuget: Plotly.NET, 2.0.0-preview.16"
#r "nuget: FSharp.FGL, 0.0.2"
#r "nuget: FSharp.FGL.ArrayAdjacencyGraph, 0.0.2"

// The edge filtering method presented in this tutorial requires an Eigenvalue decomposition. 
// FSharp.Stats uses the one implemented in the LAPACK library. 
// To enable it just reference the lapack folder in the FSharp.Stats nuget package:
FSharp.Stats.ServiceLocator.setEnvironmentPathVariable @"C:\Users\USERNAME\.nuget\packages\fsharp.stats\0.4.2\netlib_LAPACK" // 
FSharp.Stats.Algebra.LinearAlgebra.Service()

open Deedle
open Plotly.NET
open FSharp.Data
open FSharp.Stats
open Cyjs.NET
open FSharp.FGL.ArrayAdjacencyGraph


(**
# The Louvain Method and where to go from there
_[Christopher Lux](https://github.com/LibraChris)_, Jan 2022


## Content
- [Introduction](#Introduction)
- [Loading the previous graph](#Loading-the-previous-graph)
- [Modularity](#Modularity)
- [PCA - Principal component analysis](#PCA-Principal-component-analysis)
- [Ontology Enrichment](#Ontology-Enrichment)
- [Further reading](#Further-reading)
- []()

## Introduction

This tutorial picks up where the Fslab advanced tutorials [Correlation network](https://fslab.org/content/tutorials/009_correlation-network.html) ended.
The goal of this blogpost is to showcase how to use the Louvain methode in FSharp.FGL and what you can do with this information with the resources of our group.

We are using the following libraries:

1. [Deedle]()
2. [Plotly.NET]()
3. [FSharp.Data]()
4. [FSharp.Stats]()
5. [Cyjs.NET]()
6. [FSharp.FGL]()


## Loading the previous graph 


*)

(***hide***)
// Load the data 
let rawData = Http.RequestString @"https://raw.githubusercontent.com/HLWeil/datasets/main/data/ecoliGeneExpression.tsv"

// Create a deedle frame and index the rows with the values of the "Key" column.
let rawFrame : Frame<string,string> = 
    Frame.ReadCsvString(rawData, separators = "\t")
    |> Frame.take 500
    |> Frame.indexRows "Key"


// Get the rows as a matrix
let rows = 
    rawFrame 
    |> Frame.toJaggedArray 
    |> Matrix.ofJaggedArray

// Create a correlation network by computing the pearson correlation between every tow rows
let correlationNetwork = 
    Correlation.Matrix.rowWisePearson rows

(***hide***)
let thr = 0.8203125
(**
*)

// Set all correlations less strong than the critical threshold to 0
let filteredNetwork = 
    correlationNetwork
    |> Matrix.map (fun v -> if (abs v) > thr then v else 0.)


// The styled vertices. The size is based on the degree of this vertex, so that more heavily connected nodes are emphasized
let cytoVertices = 
    rawFrame.RowKeys
    |> Seq.toList
    |> List.indexed
    |> List.choose (fun (i,v) -> 
        let degree = 
            Matrix.getRow filteredNetwork i 
            |> Seq.filter ((<>) 0.)
            |> Seq.length
        let styling = [CyParam.label v; CyParam.weight (sqrt (float degree) + 1. |> (*) 10.)]

        if degree > 1 then 
            Some (Elements.node (string i) styling)
        else 
            None
    )

// Styled edges
let cytoEdges = 
    let len = filteredNetwork.Dimensions |> fst
    [
        for i = 0 to len - 1 do
            for j = i + 1 to len - 1 do
                let v = filteredNetwork.[i,j]
                if v <> 0. then yield i,j,v
    ]
    |> List.mapi (fun i (v1,v2,weight) -> 
        let styling = [CyParam.weight (0.2 * weight)]
        Elements.edge ("e" + string i) (string v1) (string v2) styling
    )
   
// Resulting cytograph
let cytoGraph = 

    CyGraph.initEmpty ()
    |> CyGraph.withElements cytoVertices
    |> CyGraph.withElements cytoEdges
    |> CyGraph.withStyle "node" 
        [
            CyParam.shape "circle"
            CyParam.content =. CyParam.label
            CyParam.width =. CyParam.weight
            CyParam.height =. CyParam.weight
            CyParam.Text.Align.center
            CyParam.Border.color "#A00975"
            CyParam.Border.width 3
        ]
    |> CyGraph.withStyle "edge" 
        [
            CyParam.Line.color "#3D1244"
        ]
    |> CyGraph.withLayout (Layout.initCose (Layout.LayoutOptions.Cose(NodeOverlap = 400,ComponentSpacing = 100)))  

// cytoGraph
// |> CyGraph.withSize (1300,1000)
// |> HTML.toEmbeddedHTML

// (*** include-it-raw ***)
(**
*)


(**

## Modularity

One method to check the 

*)

let vertices =
    rawFrame.RowKeys
    |> Seq.toList
    |> List.indexed
    |> List.choose (fun (i,v) -> 
        let degree = 
            Matrix.getRow filteredNetwork i 
            |> Seq.filter ((<>) 0.)
            |> Seq.length
        
        if degree > 1 then 
            Some (i,v)
        else
            None
    )

let edges =
    let len = filteredNetwork.Dimensions |> fst
    [
        for i = 0 to len - 1 do
            for j = i + 1 to len - 1 do
                let v = filteredNetwork.[i,j]
                if v <> 0. then yield i,j,v
    ]

let g = 
    Graph.createOfEdgelist vertices edges


let g2= 
    ArrayAdjacencyGraph.Algorithms.Louvain.Louvain.louvain g 0.1 


let coloredVertices = 
    g2.GetLabels()
    |> Array.map (snd)
    |> Array.countBy (fun x -> x)
    |> Array.choose(fun (m,count) -> 
        if count > 5 then
            Some m
        else
            None
        )
    |> List.ofArray

let color =
    [
        "#A00976";
        "#D68A0C";
        "#13478D";
        "#9ACB0B";
        "#C249A1";
        "#FFC360";
        "#4B74AD";
        "#CCF35C";
        "#640048";
        "#855300";
        "#052858";
        "#5E7F00";
        "#CC0000";
        "#FFC0CB"

    ]
    |> List.map2(fun community color -> (community,color)) coloredVertices
    |> Map.ofList

// Map to apply the same color as in the graph above to 
let geneNameToColor =
    
    let louvainLabelToColor= 
        g2.GetLabels()
        |> Array.countBy (fun (gene,x) -> x)
        |> Array.choose(fun (m,count) -> 
            if count > 5 then
                Some m
            else
                None
            )
        |> Array.map(fun x -> (x,color.Item (x)))
        |> Map.ofArray

    g2.GetLabels()
    |> Array.map(fun (gene,label) -> 
        (gene,
            (if (Map.containsKey label louvainLabelToColor) then Map.find label louvainLabelToColor 
            else "#808080"))
    )
    |> Map.ofArray

let cytoVertices2 =
    Array.map2 (fun v l -> (v,l)) (g2.GetVertices()) (g2.GetLabels())
    |> List.ofArray
    |> List.map(fun (v,(l,c)) ->
        let styling = [CyParam.label l;if (Map.containsKey c color) then CyParam.color (color.Item (c));CyParam.weight (sqrt (float (g2.Degree v)) + 1. |> (*) 10.)]
        (Elements.node (string v) styling)

    )

let cytoGraph2 = 

    CyGraph.initEmpty ()
    |> CyGraph.withElements cytoVertices2
    |> CyGraph.withElements cytoEdges
    |> CyGraph.withStyle "node" 
        [
            CyParam.shape "circle"
            CyParam.content =. CyParam.label
            CyParam.width =. CyParam.weight
            CyParam.height =. CyParam.weight
            CyParam.Text.Align.center
            CyParam.Border.color =. CyParam.color
            CyParam.Background.color =. CyParam.color
        ]
    |> CyGraph.withStyle "edge" 
        [
            CyParam.Line.color "#3D1244"
        ]
    |> CyGraph.withLayout (Layout.initCose (Layout.LayoutOptions.Cose(NodeOverlap = 400,ComponentSpacing = 100)))  

(***hide***)
cytoGraph2
|> CyGraph.withSize (1300,1000)
|> HTML.toEmbeddedHTML
(*** include-it-raw ***)
(**
*)


(**
## PCA - Principal component analysis

The principal component analysis is a method used to 
*)
let pcaData = 
    rows
    |> Matrix.transpose
    |> fun x ->
        let pcaComponents = ML.Unsupervised.PCA.compute (ML.Unsupervised.PCA.toAdjustCenter x) x

        let index1 = 1
        let index2 = 2
        let pComponent1 = pcaComponents.[index1 - 1]
        let pComponent2 = pcaComponents.[index2 - 1]
        let x = pComponent1.EigenVector 
        let y = pComponent2.EigenVector 
        let label = rawFrame.RowKeys |> Array.ofSeq
        
        let combined = Array.map2 (fun c1 c2 -> (c1,c2)) x y
        
        (label,combined)

let pcaChart = 
    let label,data = pcaData

    data
    |> Array.mapi(fun i x -> 
        (label.[i]),Chart.Point(
            [x],
            Text=label.[i],
            MarkerColor=(
                if (Map.containsKey (label.[i]) geneToColor) then
                    Color.fromHex(Map.find(label.[i]) geneToColor)
                else
                    Color.fromString "gray"
                    ))|> Chart.withTraceName (label.[i]))
    |> Array.choose(fun (l,chart) -> if List.contains l (vertices|>List.map snd) then Some chart else None)
    |> Chart.combine

pcaChart
|> Chart.withMarkerStyle (Size=8)
|> Chart.withSize (1300,1000)
|> GenericChart.toEmbeddedHTML
(***include-it-raw***)

(**
## Ontology Enrichment
*)


(**
## Further reading

- [Networksciencebook](http://www.networksciencebook.com)
- [Wikipedia](https://en.wikipedia.org/wiki/Graph_theory)
- [FSharp.FGL Github](https://github.com/CSBiology/FSharp.FGL)
- [Blondel, Vincent D; Guillaume, Jean-Loup; Lambiotte, Renaud; Lefebvre, Etienne (9 October 2008). "Fast unfolding of communities in large networks". Journal of Statistical Mechanics: Theory and Experiment. 2008 (10): P10008. arXiv:0803.0476. Bibcode:2008JSMTE..10..008B. doi:10.1088/1742-5468/2008/10/P10008. S2CID 334423](https://doi.org/10.1088%2F1742-5468%2F2008%2F10%2FP10008)


*)