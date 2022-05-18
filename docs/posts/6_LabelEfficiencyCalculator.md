---
title: Label Efficiency Calculator
category: Learning resources
categoryindex: 3
index: 7
---

# Label Efficiency Calculator
_[Jonathan Ott](https://github.com/Joott)_ ~ _last updated: 2022-05-19_

## Isotopic Distribution
Peptide signals exhibit a characteristic shape in the mass spectrum that depend on their isotopic profile, which is defined by 
the number of naturally occurring isotopes in the peptide. The occurrence probabilities of natural isotopes are reflected in the mass 
spectrum by the relative heights of the peak series belonging to the respective peptide. The frequency at which natural isotopes occur 
is known and can be used to compute the isotope distribution of a molecule. The isotopic distribution for a given peptide molecule 
C(v)H(w)N(x)O(y)S(z) is described by the following product of polynomials:

![](https://latex.codecogs.com/png.image?\dpi{110}\bg{white}\large&space;\newline(&space;{}^{12}\textrm{C}&space;&plus;&space;{}^{13}\textrm{C})^{v}&space;\times&space;({}^{1}\textrm{H}&plus;{}^{2}\textrm{H})^{w}&space;\times&space;({}^{14}\textrm{N}&plus;{}^{15}\textrm{N})^{x}\times({}^{16}\textrm{O}&plus;{}^{17}\textrm{O}&space;&plus;&space;{}^{18}\textrm{O})^{y}\newline\times({}^{32}\textrm{S}&plus;{}^{33}\textrm{S}&plus;{}^{34}\textrm{S}&plus;{}^{36}\textrm{S})^{z})

Symbolic expansion of the polynomials results in many product terms, which correspond to different isotopic variants of a molecule. 
Even for molecules of a medium size, the straightforward expansion of the polynomials leads to an explosion regarding the number of product terms. 
Due to this complexity, there was a need to develop algorithms for efficient computation. The different strategies comprise pruning the 
polynomials to discard terms with coefficients below a threshold (Yergey 1983) combined with a recursive 
computation (Claesen et al. 2012), and Fourier Transformation for a more efficient convolution of the isotope distributions of 
individual elements (Rockwood et al. 1995), or rely on dynamic programming (Snider 2007). 
MIDAs (Alves and Yu 2005) is one of the more elaborate algorithms to predict an isotope cluster based on a given peptide sequence.

## Simulating Isotopic Clusters for peptides

```Fsharp
// create chemical formula for amino acid and add water to reflect hydrolysed state in mass spectrometer
let toFormula bioseq =  
    bioseq
    |> BioSeq.toFormula
    |> Formula.add Formula.Table.H2O
```

```Fsharp
// Predicts an isotopic distribution of the given formula at the given charge, 
// normalized by the sum of probabilities, using the MIDAs algorithm
let generateIsotopicDistribution (charge:int) (f:Formula.Formula) =
    IsotopicDistribution.MIDA.ofFormula 
        IsotopicDistribution.MIDA.normalizeByProbSum
        0.01
        0.01
        charge
        f
```

```Fsharp
"AAGVLDNFSEGEK"
|> BioSeq.ofAminoAcidString
|> toFormula
|> generateIsotopicDistribution 2
|> fun dist -> Chart.Column (dist, Width = 0.1)
```
![](../img/6_label_efficiency_calculator/AAGVLDNFSEGEK.png)

```Fsharp
let label n15LableEfficiency formula =
    let heavyN15 = Elements.Di (Elements.createDi "N15" (Isotopes.Table.N15,n15LableEfficiency) (Isotopes.Table.N14,1.-n15LableEfficiency) )
    Formula.replaceElement formula Elements.Table.N heavyN15
```

```Fsharp
"AAGVLDNFSEGEK"
|> BioSeq.ofAminoAcidString
|> toFormula
|> label 0.95
|> generateIsotopicDistribution 2
|> fun dist -> Chart.Column (dist, Width = 0.1)
```

![](../img/6_label_efficiency_calculator/AAGVLDNFSEGEK_95LE.png)

```Fsharp
let mz = [|675.7949645701999;676.2983662177933;676.8017942912567|]
let intensity = [|0.0846659638221692;0.5856855554667739;0.3296484807110569|]
Chart.Column(intensity,mz, Width = 0.1)
```

![](../img/6_label_efficiency_calculator/AAGVLDNFSEGEK_real.png)