(**

---
title: Computation expressions: Usage examples
category: Implementation
categoryindex: 3
index: 5
---


# Computation expressions: Usage examples
_[Heinrich Lukas Weil](https://github.com/hlweil)_

## Content
- [What are Computation expressions?](#What-are-Computation-expressions?)
- [Bind based computation expressions](#Bind-based-computation-expressions)
- [Yield based computation expressions](#Yield-based-computation-expressions)
- [Custom Operator based computation expressions](#Custom-Operator-based-computation-expressions)
- [Further reading](#Further-reading)

## What are Computation expressions?

This question has been thoroughly answered in technical precision in many places over the internet (Check out [Further reading](#Further-reading)). My aim here is not trying to give a better explanation.
Instead, in this blogpost, I want to focus on practicle use cases of computation expressions. So, to answer this question in the context of this blogbost:

*Computation expressions are a versatile tool to express complex behaviour in a simple syntax*. 
You open a computation expression (CE) with the following syntax: 

```fsharp
computation-expression-name {computation-expression-body}
```

Inside these `{ }` brackets, an encapsulated environment with its own set of predefined rules exist. 
These rules create a behaviour in the background that can be complex or simple, while exposing an easy to grasp language to the user. 
An user can also create their own `Computation expression builder` with it`s own set of behaviours for every function. 
There are around 20 different Members with predefined names a programmer can implement, exposing around 10 different keywords to the user. The full list can be found [here](https://docs.microsoft.com/en-us/dotnet/fsharp/language-reference/computation-expressions#creating-a-new-type-of-computation-expression).

The possibilities for different computation expressions are basically limitless. As a starting point for programmers new to computation expressions I therefore want to present a few often used ways for creating their own Computation Expression builder.

## *Bind* based computation expressions

Bind based computation expressions make especially use of the `let!` and the `return` keywords.

Here is an example for an 'async' computation expression, taken from [the official F# reference](https://docs.microsoft.com/en-us/dotnet/fsharp/language-reference/computation-expressions):

*)

let fetchAndDownload url =
    // Keyword for starting the async computation expression
    async {

        let client = new System.Net.WebClient()

        // client.AsyncDownloadData returns a value of type Async<byte []>.
        // but by using the let! keyword, bound to "data" is actually the unwrapped inner value of type byte [].
        let! data = client.AsyncDownloadData url

        // This unwrapped value can then conveniently fed into a follow up function.
        let processedData = System.Text.Encoding.ASCII.GetString data

        // The computation expression is finished by returning the result string.
        // This value is again wrapped in the Async wrapper type, resulting in a return value of type Async<string>.
        return processedData
    }

1+1

fun x -> x + 3

(** include-it **)

(**

The main idea here is, that you can work on the return values directly, rather than on the values wrapped into the Async type. 
All the waiting for the thread to finish, wrapping and unwrapping is done implicitly for you, simply by using the `let!` keyword.

![](../img/CE_Bind.png)

So, let's get started creating our own Computation expression builder.

*)

type OptionBuilder() =


    member this.Return(value : 'T) : 'T Option = 
        printfn $"Return {value}"
        Some value

    member this.Bind(wrappedValue : 'T option, f : 'T -> 'T option) : 'T option =
        printfn $"Bind {wrappedValue}"
        match wrappedValue with
        | Some unwrappedValue -> 
            f unwrappedValue
        | None -> None

let option = OptionBuilder()

(**

Explanation

*)

option {

    let! a = Some 6
    let! b = None
    let! c = Some 8

    return (a + b + c)
}


(**

## *Yield* based computation expressions

*)

seq {
    // Implicit yield
    1
    2
    3
    yield! [4;5;6]
}
|> Seq.toList
(**

![](../img/CE_Yield.png)  

*)

(**

Explanation

*)

(**

## *Custom Operator* based computation expressions

*)

/// from Chapter8/Semantic.Trading.Fsharp.fs
type TradeBuilder() =
    member x.Yield (()) = Items []

    [<CustomOperation("buy")>]
    member x.Buy (Items sources, i: int, s: string, sh: Shares, a: At, m: PriceType, p: int) =
        Items [ yield! sources
                yield LineItem(Security(i, s), Buy, Price(m, p)) ]
  
    [<CustomOperation("sell")>]
    member x.Sell (Items sources, i: int, s: string, sh: Shares, a: At, m: PriceType, p: int) =
        Items [ yield! sources
                yield LineItem(Security(i, s), Sell, Price(m, p)) ]  

let trade = TradeBuilder()

let example = 
    trade {
        buy 100 "IBM" Shares At Max 45
        sell 40 "Sun" Shares At Min 24
        buy 25 "CISCO" Shares At Max 56 
    }

(**

Explanation

*)

(**

## Further reading

- [Basic introduction](https://en.wikibooks.org/wiki/F_Sharp_Programming/Computation_Expressions)
- [In depth introduction](https://fsharpforfunandprofit.com/series/computation-expressions/)
- [Official reference](https://docs.microsoft.com/en-us/dotnet/fsharp/language-reference/computation-expressions)

- [Good video for practical introduction](https://www.youtube.com/watch?v=pC4ZIeOmgB0)

*)