(**

---
title: q values
category: Learning resources
categoryindex: 4
index: 0
---
*)

(***hide***)
#r "nuget: FSharp.Stats, 0.4.1"
#r "nuget: FSharpAux, 1.1.0"

#r "nuget: Plotly.NET, 2.0.0-preview.11"

open Plotly.NET
open Plotly.NET.StyleParam
open Plotly.NET.LayoutObjects



module Chart = 

    let myAxis name = LinearAxis.init(Title=Title.init name,Mirror=StyleParam.Mirror.All,Ticks=StyleParam.TickOptions.Inside,ShowGrid=false,ShowLine=true)
    let withAxisTitles x y chart = chart |> Chart.withXAxis (myAxis x) |> Chart.withYAxis (myAxis y)
    


(**
# q values

_[Benedikt Venn](https://github.com/bvenn)_


### Table of contents

- [Introduction](#Introduction)
- [The multiple testing problem](#The-multiple-testing-problem)
- [False discovery rate](#False-discovery-rate)
    - [q values](#q-values)
- [Quality plots](#Quality-plots)
- [Definitions and Notes](#Definitions-and-Notes)
- [References](#References)

## Introduction

High throughput techniques like microarrays with its successor RNA-Seq and mass spectrometry proteomics lead to an huge data amount.
Thousands of features (e.g. transcripts or proteins) are measured simultaneously. Differential expression analysis aims to identify features, that change significantly
between two conditions. A common experimental setup is the analysis of which genes are over- or underexpressed between a wild type and a mutant.

Hypothesis tests aim to identify differences between two or more samples. The most common statistical test is the t test that tests a difference of means. Hypothesis tests report 
a p value, that correspond the probability of obtaining results at least as extreme as the observed results, assuming that the null hypothesis is correct. In other words:



_<center>If there is no effect (no mean difference), a p value of 0.05 indicates that in 5 % of the tests a false positive is reported.</center>_
 

<hr>

Consider two population distributions that follow a normal distribution. Both have the same mean and standard deviation.

*)
open FSharpAux
open FSharp.Stats
open FSharp.Stats.Distributions


let distributionA = Continuous.normal 10.0 1.0
let distributionB = Continuous.normal 10.0 1.0

(***hide***)
let distributionChartAB = 
    [
        Chart.Area([5. .. 0.01 .. 15.] |> List.map (fun x -> x,distributionA.PDF x),"distA")
        Chart.Area([5. .. 0.01 .. 15.] |> List.map (fun x -> x,distributionB.PDF x),"distB")
    ]
    |> Chart.combine
    |> Chart.withAxisTitles "variable X" "relative count"
    |> Chart.withSize (1000.,600.)
    |> Chart.withTitle "null hypothesis"

(***hide***)
distributionChartAB |> GenericChart.toChartHTML
(***include-it-raw***)


(**

Samples with sample size 5 are randomly drawn from both population distributions.
Both samples are tested if a mean difference exist using a two sample t test where equal variances of the underlying population distribution are assumed.


*)

let getSample n (dist: Distributions.Distribution<float,float>) =
    Vector.init n (fun _ -> dist.Sample())
    
let sampleA = getSample 5 distributionA
let sampleB = getSample 5 distributionB

let pValue = (Testing.TTest.twoSample true sampleA sampleB).PValue

(***hide***)
pValue
(***include-it***)

(**
10,000 tests are performed, each with new randomly drawn samples. This correspond to an experiment in which none of the features changed. The resulting p values are 
uniformely distributed between 0 and 1.

<br>

<img style="max-width:60%" src="/img/qvalue_01.svg"></img>

_Fig 1: p value distribution of the null hypothesis._
<hr>
<br>

*)
(***hide***)
let nullDist = 
    Array.init 10000 (fun x -> 
        let sA = getSample 5 distributionA
        let sB = getSample 5 distributionB
        (Testing.TTest.twoSample true sA sB).PValue
        )


let nullDistributionChart = 
    nullDist 
    |> Distributions.Frequency.create 0.025 
    |> Map.toArray 
    |> Array.map (fun (k,c) -> k,float c) 
    |> Chart.StackedColumn 
    |> Chart.withTraceName "alt"
    |> Chart.withAxisTitles "pvalue" "frequency"

let thresholdLine =
    Shape.init(ShapeType.Line,0.05,0.05,0.,300.)




(**

Samples are called significantly different, if their p value is below a certain significance threshold ($\alpha$ level). While "the lower the better", a common threshold
is a p value of 0.05 or 0.01. In the presented case in average $10,000 * 0.05 = 500$ tests are significant (red box), even if the populations do not differ. They are false 
positives (FP). Now lets repeat the same experiment, but this time sample 70% of the time from null features (no difference) and add 30% samples of truly 
differing distributions. Therefore a third populations is generated, that differ in mean, but has equal standard deviations:



*)


let distributionC = Continuous.normal 11.5 1.0


(***hide***)
let distributionChartAC = 
    [
        Chart.Area([5. .. 0.01 .. 15.] |> List.map (fun x -> x,distributionA.PDF x),"distA")
        Chart.Area([5. .. 0.01 .. 15.] |> List.map (fun x -> x,distributionC.PDF x),"distC")
    ]
    |> Chart.combine
    |> Chart.withAxisTitles  "variable X" "relative count"
    |> Chart.withSize (1000.,600.)
    |> Chart.withTitle "alternative hypothesis"


//distributionChartAC |> GenericChart.toChartHTML


(**

<img style="max-width:60%" src="/img/qvalue_02.svg"></img>

_Fig 2: p value distribution of the alternative hypothesis. Blue coloring indicate p values deriving from distribution A and B. 
Orange coloring indicate p values deriving from distribution A and C._



The pvalue distribution of the tests resulting from truly differing populations are right skewed, while the null tests again show a homogeneous distribution between 0 and 1. 
Many, but not all of the tests that come from the truly differing populations are below 0.05, and therefore would be reported as significant.


##The multiple testing problem

The hypothesis testing framework was developed for performing just one test. If many tests are performed, like in modern high throuput studies, the probability to obtain a 
false positive result increases. The probability of at least one false positive is called Familywise error rate (FWER) and can be determined by $FWER=1-(1-\alpha)^m$ where 
$\alpha$ corresponds to the significance threshold and $m$ is the number of tests performed.


*)

(***hide***)

let bonferroniLine = 
    Shape.init(ShapeType.Line,0.,35.,0.05,0.05,Line=Line.init(Dash=DrawingStyle.Dash))

let fwer = 
    [1..35]
    |> List.map (fun x -> 
        x,(1. - (1. - 0.05)**(float x))
        )
    |> Chart.Point
    |> Chart.withAxisTitles "#tests" "p(at least 1 FP)" 
    |> Chart.withShape bonferroniLine
    |> Chart.withTitle "FWER"

fwer |> GenericChart.toChartHTML
(***include-it-raw***)


(**

_Fig 3: Familiy wise error rate depending on number of performed tests. The dashed line indicates the Bonferroni corrected FWER by $p^* = \frac{\alpha}{m}$._


When 10,000 null features are tested with a p value threshold of 0.05, in average 500 tests are reported significant even if there is not a single comparisons in which the 
population differ. If some of the features are in fact different, the number of false positives consequently decreases (remember, the p value is defined for tests of null features). 

Why the interpretation of high throughput data based on p values is difficult: The more features are measured, the more false positives you can expect. If 100 differentially 
expressed genes are identified by p value thresholding, without further information about the magnitude of expected changes and the total number of measured transcripts, the 
data is useless. 

The p value threshold has no straight-forward interpretation when many tests are performed. Sidenote: Of course you could restrict the family wise error rate to 0.05, regardless 
how many tests are performed. This is realized by dividing the $\alpha$ significance threshold by the number of tests, which is known as Bonferroni correction: $p^* = \frac{\alpha}{m}$.
This correction drastically limit the false positive rate, but in an experiment with a huge count of expected changes, it additionally would result in many false negatives. The 
FWER should be chosen if the costs of follow up studies to tests the candidates are dramatic or there is a huge waste of time to potentially study false positives.

##False discovery rate

A more reasonable measure of significance with a simple interpretation is the so called false discovery rate (FDR). It describes the rate of expected false positives within the 
overall reported significant features. The goal is to identify as many sig. features as possible while incurring a relatively low proportion of false positives.
Consequently a set of reported significant features together with the FDR describes the confidence of this set, without the requirement to 
somehow incorporate the uncertainty that is introduced by the total number of tests performed. In the simulated case of 7,000 null tests and 3,000 tests resulting from truly 
differing distributions above, the FDR can be calculated exactly. Therefore at e.g. a p value of 0.05 the number of false positives (blue in red box) are devided by the number 
of significant reported tests (false positives + true positives). 




<br>
<hr>

<img style="max-width:75%" src="/img/qvalue_03.svg"></img>

_Fig 4: p value distribution of the alternative hypothesis._
<hr>
<br>

Given the conditions described in the first chapter, the FDR of this experiment with a p value threshold of 0.05 is 0.198. Out of the 2019 reported significant comparisons, in average 350 
are expected to be false positives, which gives an straight forward interpretation of the data confidence. In real-world experiments the proportion of null tests and tests 
deriving from an actual difference is of course unknown. The proportion of null tests however tends to be distributed equally in the p value histogram. By identification of 
the average null frequency, a proportion of FP and TP can be determined and the FDR can be defined. This frequency estimate is called $\pi_0$, which leads to an FDR definition of:



<br>


<img style="max-width:75%" src="/img/qvalue_04.svg"></img>

_Fig 5: FDR calculation on simulated data._

<br>

*)



(**



### q values

Consequently for each presented p value a corresponding FDR can be calculated. The minimum local FDR at each p value is called q value. 

$$\hat q(p_i) = min_{t \geq p_i} \hat{FDR}(p_i)$$


Since the q value is not monotonically increasing, it is smoothed by assigning the lowest FDR of all p values, that are bigger or equal to the current one.

By defining $\pi_0$, all other parameters can be calculated from the given p value distribution to determine the all q values. The most prominent 
FDR-controlling method is known as Benjamini-Hochberg correction. It sets $\pi_0$ as 1, assuming that all features are null. In studies with an expected high proportion of true 
positives, a $\pi_0$ of 1 is too conservative, since there definitely are true positives in the data. 

A better estimation of $\pi_0$ is given in the following:

True positives are assumed to be right skewed while null tests are distributed equally between 0 and 1. Consequently the right flat region of the p value histogram tends to correspond 
to the true frequency of null comparisons (Fig 6). As real world example 9856 genes were measured in triplicates under two conditions (control, and treatment). The p value distribution of two 
sample ttests looks as follows:


*)
(***hide***)
let examplePVals =
    System.IO.File.ReadAllLines(@"../files/pvalExample.txt")
    |> Array.tail
    |> Array.map float

//number of tests
let m =  
    examplePVals
    |> Array.length
    |> float

let nullLine =
    Shape.init(ShapeType.Line,0.,1.,1.,1.,Line=Line.init(Dash=DrawingStyle.Dash))

let empLine =
    Shape.init(ShapeType.Line,0.,1.,0.4,0.4,Line=Line.init(Dash=DrawingStyle.DashDot,Color=Color.fromHex "#FC3E36"))

let exampleDistribution = 
    [
        [
        examplePVals
        |> Distributions.Frequency.create 0.025
        |> Map.toArray 
        |> Array.map (fun (k,c) -> k,float c / (m * 0.025))
        |> Chart.Column
        |> Chart.withTraceName "density"
        |> Chart.withAxisTitles "p value" "density"
        |> Chart.withShapes [nullLine;empLine]

        examplePVals
        |> Distributions.Frequency.create 0.025
        |> Map.toArray 
        |> Array.map (fun (k,c) -> k,float c)
        |> Chart.Column
        |> Chart.withTraceName "gene count"
        |> Chart.withAxisTitles "p value" "gene count"
        ]
    ]
    |> Chart.Grid()
    |> Chart.withSize(1100.,550.)

exampleDistribution |> GenericChart.toChartHTML
(***include-it-raw***)

(**
<br>

_Fig 6: p value distributions of real world example. The frequency is given on the right, its density on the left. The dashed line indicates the distribution, if all features
were null. The dash-dotted line indicates the visual estimated pi0._

<br>
<hr>



By performing t tests for all comparisons 3743 (38 %) of the genes lead to a pvalue lower than 0.05.
By eye, you would estimate $\pi_0$ as 0.4, indicating, only a small fraction of the genes are unaltered (null). After q value calculations, you would filter for a specific FDR (e.g. 0.05) and 
end up with an p value threshold of 0.04613, indicating a FDR of max. 0.05 in the reported 3642 genes. 

```no-highlight
pi0     = 0.4
m       = 9856
D(p)    = number of sig. tests at given p
FP(p)   = p*0.4*9856
FDR(p)  = FP(p) / D(p)
```

FDR(0.04613) = 0.4995 



<br>
<hr>

*)
(***hide***)
let pi0 = 0.4
let getD p = 
    examplePVals 
    |> Array.sumBy (fun x -> if x < p then 1. else 0.) 
let getFP p = p * pi0 * m
let getFDR p = (getFP p) / (getD p)
let qvaluesNotSmoothed = 
    examplePVals
    |> Array.sort
    |> Array.map (fun x -> 
        x, getFDR x)
    |> Chart.Line 
    |> Chart.withTraceName "not smoothed"
let qvaluesSmoothed = 
    let pValsSorted =
        examplePVals
        |> Array.sortDescending
    let rec loop i lowest acc  = 
        if i = pValsSorted.Length then 
            acc |> List.rev
        else 
            let p = pValsSorted.[i]
            let q = getFDR p
            if q > lowest then  
                loop (i+1) lowest ((p,lowest)::acc)
            else loop (i+1) q ((p,q)::acc)
    loop 0 1. []
    |> Chart.Line
    |> Chart.withTraceName "smoothed"
let eXpos = examplePVals |> Array.filter (fun x -> x <= 0.046135) |> Array.length
let p2qValeChart =
    [qvaluesNotSmoothed;qvaluesSmoothed]
    |> Chart.combine
    |> Chart.withAxisTitles "p value" "q value"
    |> Chart.withShape empLine
    |> Chart.withTitle (sprintf "#[genes with q value < 0.05] = %i" eXpos)

p2qValeChart |> GenericChart.toChartHTML
(***include-it-raw***)

(**

_Fig 7: FDR calculation on experiment data_
<hr>



###The automatic detection of $\pi_0$ is facilitated as follows


For a range of $\lambda$ in e.g. $\{0.0  ..  0.05  ..  0.95\}$, calculate $\hat \pi_0 (\lambda) = \frac {\#[p_j > \lambda]}{m(1 - \lambda)}$

*)


let pi0Est = 
    [|0. .. 0.05 .. 1.|]
    |> Array.map (fun lambda -> 
        let num = 
            examplePVals 
            |> Array.sumBy (fun x -> if x > lambda then 1. else 0.) 
        let den = float examplePVals.Length * (1. - lambda)
        lambda, num/den
        )

(***hide***)
let pi0EstChart = 
    pi0Est 
    |> Chart.Point
    |> Chart.withAxisTitles "$\lambda$" "$\hat \pi_0(\lambda)$"
    |> Chart.withMathTex(true)

pi0EstChart|> GenericChart.toChartHTML
(***include-it-raw***)

(**
_Fig 8: pi0 estimation._
<hr>

The resulting diagram shows, that with increasing $\lambda$ its function value $\hat \pi_0(\lambda)$ tends to $\pi_0$. The formula relates the actual proportion of tests greater than $\lambda$ to the proportion of $\lambda$ range the corresponding p values are in.
In Storey & Tibshirani 2003 this curve is fitted with a cubic spline. A weighting of the knots by $(1 - \lambda)$ is recommended 
but not specified in the final publication. Afterwards the function value at $\hat \pi_0(1)$ is defined as final estimator of $\pi_0$. 

Another method, that does not depend on fitting is based on bootstrapping and was introduced in Storey et al. (2004). It is implemented in FSharp.Stats:

  1. Determine the minimal $\hat \pi_0$ and call it $min \hat \pi_0$ . 

  2. For each $\lambda$, bootstrap the p values (e.g. 100 times) and calculate the mean squared error (MSE) from the difference of resulting $\hat \pi_0^b$ to $min  \hat \pi_0$. The minimal MSE indicates the best $\lambda$. With $\lambda$ 
defined, $\pi_0$ can be calculated.



*)


(***hide***)
let getpi0Bootstrap (lambda:float[]) (pValues:float[]) =
    let rnd = System.Random()
    let m = pValues.Length |> float
    let getpi0hat lambda pVals=
        let hits = 
            pVals 
            |> Array.sumBy (fun x -> if x > lambda then 1. else 0.) 
        hits / (m * (1. - lambda))
    
    //calculate MSE for each lambda
    let getMSE lambda =
        let mse = 
            //generate 100 bootstrap samples of p values and calculate the MSE at given lambda
            Array.init 100 (fun b -> 
                Array.sampleWithReplacement rnd pValues pValues.Length  
                |> getpi0hat lambda
                )
        mse
    lambda
    |> Array.map (fun l -> l,getMSE l)
    

let minimalpihat = 
    //FSharp.Stats.Testing.MultipleTesting.Qvalues.pi0hats  [|0. .. 0.05 .. 0.96|] examplePVals |> Array.minBy snd |> snd
    0.3686417749

let minpiHatShape = 
    Shape.init(ShapeType.Line,0.,1.,minimalpihat,minimalpihat,Line=Line.init(Dash=DrawingStyle.Dash))

let bootstrappedPi0 =
    getpi0Bootstrap [|0. .. 0.05 .. 0.96|] examplePVals
    |> Array.map (fun (l,x) -> 
        Chart.BoxPlot(x=Array.init x.Length (fun _ -> l),y=x,Fillcolor=Color.fromHex"#1F77B4",MarkerColor=Color.fromHex"#1F77B4",Name=sprintf "%.2f" l))
    |> Chart.combine
    |> Chart.withAxisTitles "$\lambda$" "$\hat \pi_0$"
    |> Chart.withMathTex(true)
    |> Chart.withShape minpiHatShape

bootstrappedPi0 |> GenericChart.toChartHTML
(***include-it-raw***)







(**

_Fig 9: Bootstrapping for pi0 estimation. The dashed line indicates the minimal pi0 from Fig. 8.
The bootstrapped pi0 distribution that shows the least variation to the dashed line is the optimal. In the presented example it is either 0.8 or 0.85._
<hr>

For an $\lambda$, range of $\{0.0  ..  0.05  ..  0.95\}$ the bootstrapping method determines either 0.8 or 0.85 as optimal $\lambda$ and therefore $optimal  \hat \pi_0$ is either $0.3703$ or $0.3686$.

The automated estimation of $\pi_0$ based on bootstrapping is implemented in `FSharp.Stats.Testing.MultipleTesting.Qvalues`.

*)
open Testing.MultipleTesting

let pi0Stats = Qvalues.pi0BootstrapWithLambda [|0.0 .. 0.05 .. 0.95|] examplePVals

(***hide***)
pi0Stats
(***include-it***)




(**
Subsequent to $\pi_0$ estimation the q values can be determined from a list of p values.
*)

let qValues = Qvalues.ofPValues pi0Stats examplePVals

(***hide***)
qValues
(***include-it***)




(**

##Quality plots

*)

let pi0Line = 
    Shape.init(ShapeType.Line,0.,1.,pi0Stats,pi0Stats,Line=Line.init(Dash=DrawingStyle.Dash))

// relates the q value to each p value
let p2q = 
    Array.zip examplePVals qValues
    |> Array.sortBy fst
    |> Chart.Line
    |> Chart.withShape pi0Line
    |> Chart.withAxisTitles "p value" "q value"

// shows the p values distribution for an visual inspection of pi0 estimation
let pValueDistribution =
    let frequencyBins = 0.025 
    let m = examplePVals.Length |> float
    examplePVals 
    |> Distributions.Frequency.create frequencyBins 
    |> Map.toArray 
    |> Array.map (fun (k,c) -> k,float c / frequencyBins / m) 
    |> Chart.StackedColumn 
    |> Chart.withTraceName "p values"
    |> Chart.withAxisTitles "p value" "frequency density"
    |> Chart.withShape pi0Line

// shows pi0 estimation in relation to lambda
let pi0Estimation = 
    //Testing.MultipleTesting.Qvalues.pi0hats [|0. .. 0.05 .. 0.96|] examplePVals
    [|0. .. 0.05 .. 0.95|]
    |> Array.map (fun lambda -> 
        let num =   
            examplePVals 
            |> Array.sumBy (fun x -> if x > lambda then 1. else 0.)
        let den = float examplePVals.Length * (1. - lambda)
        lambda, num/den
        )
    |> Chart.Point
    |> Chart.withAxisTitles "$\lambda$" "$\hat \pi_0(\lambda)$"
    |> Chart.withMathTex(true)



(***hide***)
p2q |> GenericChart.toChartHTML
(***include-it-raw***)

(**
_Fig 10: p value relation to q values_
*)

(***hide***)
pValueDistribution |> GenericChart.toChartHTML
(***include-it-raw***)

(**
_Fig 11: p value density distribution. The dashed line indicates pi0 estimated by bootstrapping method._
*)

(***hide***)
pi0Estimation|> GenericChart.toChartHTML
(***include-it-raw***)

(**
_Fig 12: Visual pi0 estimation._
*)

(**
##Definitions and Notes
  - q values are not always greater than their associated p values. q values can maximal be $\pi_0$.
  - Storey & Tibshirani (2003):
    - _"The q-value for a particular feature is the expected proportion of false positives occurring up through that feature on the list."_
    - _"The precise definition of the q-value for a particular feature is the following. The q-value for a particular feature is the minimum false discovery rate that can be attained when calling all features up through that one on the list significant."_
    - _"The 0.05 q-value cut-off is arbitrary, and we do not recommend that this value necessarily be used."_
    - _"The Benjamini & Hochberg (1995) methodology also forces one to choose an acceptable FDR level before any data are seen, which is often going to be impractical."_
  - A method exists, to improve the q value estimation if the effects are asymmetric, meaning that negative effects are stronger than positives, or vice versa. This method published in 2014 by Orr et al. estimates a global $m_0$ and then splits the p values 
  in two groups before calulating q values for each p value set. The applicability of this strategy however is questionable, as the number of up- and downregulated features must be equal, which is not the case in most biological experimental setups.
  


##References
  - Statistical significance for genomewide studies, John D. Storey, Robert Tibshirani, Proceedings of the National Academy of Sciences Aug 2003, 100 (16) 9440-9445; [DOI: 10.1073/pnas.1530509100](https://www.pnas.org/content/100/16/9440)
  - Strong Control, Conservative Point Estimation and Simultaneous Conservative Consistency of False Discovery Rates: A Unified Approach, Storey, John D., Jonathan E. Taylor, and David Siegmund, Journal of the Royal Statistical Society. Series B (Statistical Methodology), vol. 66, no. 1, [Royal Statistical Society, Wiley], 2004, pp. 187-205, [http://www.jstor.org/stable/3647634](https://www.jstor.org/stable/3647634?seq=1#metadata_info_tab_contents).
  - An improved method for computing q-values when the distribution of effect sizes is asymmetric, Orr M, Liu P, Nettleton D., Bioinformatics. 2014 Nov 1;30(21):3044-53. [doi: 10.1093/bioinformatics/btu432](https://www.ncbi.nlm.nih.gov/pmc/articles/PMC4609005/). Epub 2014 Jul 14. PMID: 25024290; PMCID: PMC4609005.


*)

