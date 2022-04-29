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
    - [Option builder](#Option-builder)
- [Yield based computation expressions](#Yield-based-computation-expressions)
    - [List builder](#List-builder)
    - [Extended list builder](#Extended-List-builder)
    - [Math builder](#Math-builder)
- [Custom Operator based computation expressions](#Custom-Operator-based-computation-expressions)
    - [Cake builder](#Cake-builder)
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

### Introduction

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


(**

The main idea here is, that you can work on the return values directly, rather than on the values wrapped into the Async type. 
All the waiting for the thread to finish, wrapping and unwrapping is done implicitly for you, simply by using the `let!` keyword.

![](../img/CE_Bind.png)

### Option builder

So, let's get started creating our own Computation expression builder. This option builder again can be

*)

type OptionBuilder() =

    // Return method wraps the final return value in an option
    member this.Return(value : 'T) : 'T Option = 
        printfn $"Return {value}"
        Some value

    // Bind method contains logic for unwrapping the option value and expects an expression that has an option as an return value
    member this.Bind(wrappedValue : 'T option, f : 'T -> 'T option) : 'T option =
        printfn $"Bind {wrappedValue}"
        match wrappedValue with
        | Some unwrappedValue -> 
            f unwrappedValue
        | None -> None

// For easier initialization of the computation expression
let option = OptionBuilder()

(**

This is an very easy implementation that can be used to work on optional values in an easy way. 
Inside the initialized computation expression, the `Return` method can be called by using the `return` keyword. The `Bind` method can be called by using the `let!` keyword.

*)

let addOptions aO bO cO = 

    option {

        let! a = aO
        let! b = bO
        let! c = cO

        return (a + b + c)
    }


addOptions (Some 5) (Some 4) (Some 10)

//Bind Some(5)
//Bind Some(4)
//Bind Some(10)
//Return 19
//val it : int option = Some 19

addOptions (Some 5) None (Some 10)

//Bind Some(5)
//Bind 
//val it : int option = None


(**

As expected, in the first case, the `Bind` method gets called `3 times`, and the resulting value is 19.

In the second case, the resulting `None` was also expected. What is interesting to notice here, 
is that the `Bind` method gets called only two times. 
This can actually be a great feature, as in cases like these, you don't want the program to continue executing when something doesn't work out.
This is especially the case, when each computation is time intensive.

A good explanation for why this happens can be found [here](https://en.wikibooks.org/wiki/F_Sharp_Programming/Computation_Expressions#:~:text=an%20imperative%20style.-,The%20Maybe%20Monad,-A%20well%2Dknown)


## *Yield* based computation expressions

### Introduction

Yield based computation expressions make especially use of the `yield`, `yield!` and `for` keywords. 

The main aim here is usually collecting values and returning some kind of collection. 

Here is an example for a `seq` computation expression:


*)

seq {
    // Implicit yielding of single values
    1
    2
    3
    // Explicit yielding of a collection
    yield! [4;5;6]
}
|> Seq.toList

(**

The basic structure of operations performed in the seq computation expression can be seen in the following picture.
Each value get's yielded seperately and then combined step by step into a single sequence.

![](../img/CE_Yield.png)  

### List builder

So let's build our own bare minimum list builder:

*)

type ListBuilder() = 

    /// This will be exposed to the user as `yield`
    member this.Yield(v : 'T) = 
        printfn $"Yield {v}"
        [v]

    /// This will combine the yielded values in the background
    member this.Combine(v1 : 'T list, v2 : 'T list) =
        printfn $"Combine {v1} with {v2}"
        v1 @ v2

    /// This method is needed. It can be used to lazily evaluate the operations in the CE. Instead here, we just directly execute the given function.
    member this.Delay(f : unit -> 'T) = 
        printfn $"Delay"
        let res = f()
        printfn $"Delay result: {res}"
        res

// For easier initialization of the computation expression
let list = ListBuilder()

(**

So let's test it out

*)

list {
    1
    2
    3
}

//Delay
//Yield 1
//Delay
//Yield 2
//Delay
//Yield 3
//Delay result: [3]
//Combine [2] with [3]
//Delay result: [2; 3]
//Combine [1] with [2; 3]
//Delay result: [1; 2; 3]
//val it : int list = [1; 2; 3]

(**

Something to note here is that the `yielding` takes place from `top to bottom`, while the `combining` is actually done from `bottom to top`.

### Extended list builder

Of course in many cases you don't only want to yield single values but also expect more complex programming constructs to be usable. 
In the following is shown an extended version of the previously implemented list builder:

*)


type ExtendedListBuilder() = 

    /// This will be exposed to the user as `yield`
    member this.Yield(v : 'T) = 
        printfn $"Yield {v}"
        [v]

    /// This will combine the yielded values in the background
    member this.Combine(v1 : 'T list, v2 : 'T list) =
        printfn $"Combine {v1} with {v2}"
        v1 @ v2

    /// This method is needed. It can be used to lazily evaluate the operations in the CE. Instead here, we just directly execute the given function.
    member this.Delay(f : unit -> 'T) = 
        printfn $"Delay"
        let res = f()
        printfn $"Delay result: {res}"
        res

    /// Is called in empty else branches
    member this.Zero() = 
        printfn "Zero"
        []

    /// Allows for using for loops in the computation expression
    member this.For(vs : 'U seq, f : 'U -> 'T list) =
        printfn $"For"
        vs
        |> Seq.collect f
        |> Seq.toList

    /// This will allow for using the yield! method
    member this.YieldFrom(vs : 'T list) =
        printfn $"YieldFrom {vs}"
        vs


// For easier initialization of the computation expression
let extList = ExtendedListBuilder()

(**

Again let's test it out.

*)

extList {
    1
    2
    
    for i in [3;4] do
        i
    
    yield! [5;6]

    if true then 7

}

//Delay
//Yield 1
//Delay
//Yield 2
//Delay
//For
//Yield 3
//Yield 4
//Delay
//YieldFrom [5; 6]
//Delay
//Yield 7
//Delay result: [7]
//Combine [5; 6] with [7]
//Delay result: [5; 6; 7]
//Combine [3; 4] with [5; 6; 7]
//Delay result: [3; 4; 5; ... ]
//Combine [2] with [3; 4; 5; ... ]
//Delay result: [2; 3; 4; ... ]
//Combine [1] with [2; 3; 4; ... ]
//Delay result: [1; 2; 3; ... ]
//val it : int list = [1; 2; 3; 4; 5; 6; 7]


(**

One important thing to note here: The values returned in the `for` expression actually get `yielded` by the `Yield` method. 
In the case of `yield!`, only the `YieldFrom` method gets called. This means, that the input of type `'U` in the `For` method is actually already a yielded value.

### Math builder

Another thing you can do is to add a `Run` method. This method will be executed at the end of the Computation expression.
We can also combine this with the ability of the Computation expression to store a state in itself. By this we can control what exactly Run will do:


*)

/// Used to specify which mathematical operation should be applied on the float list to reduce it
type MathOperation = 
    | Sum
    | Multiply
    | Custom of (float list -> float)

    /// Apply the given operation on the float list, resulting in a single float
    member this.Apply(vals : float list) =

        match this with
        | Sum -> vals |> List.sum
        | Multiply -> vals |> List.reduce (fun a b -> a * b)
        | Custom operation -> operation vals


type MathBuilder() = 

    // Mutable math operation state, default value is Sum
    let mutable operation = Sum

    /// When the user yields a value of type MathOperation, this will not be added to the list of values but overwrite the state of the operation variable
    member this.Yield(newOperation : MathOperation) =
        operation <- newOperation
        []

    /// User can yield ints
    member this.Yield(v : int) = 
        printfn $"Yield {v}"
        [float v]

    /// User can yield floats
    member this.Yield(v : float) = 
        printfn $"Yield {v}"
        [v]

    /// This will combine the yielded values in the background
    member this.Combine(v1 : float list, v2 : float list) =
        printfn $"Combine {v1} with {v2}"
        v1 @ v2

    /// This method is needed. It can be used to lazily evaluate the operations in the CE. Instead here, we just directly execute the given function.
    member this.Delay(f : unit -> 'T) = 
        printfn $"Delay"
        let res = f()
        printfn $"Delay result: {res}"
        res

    /// Is called in empty else branches
    member this.Zero() = 
        printfn "Zero"
        []

    /// Allows for using for loops in the computation expression
    member this.For(vs : 'U seq, f : 'U -> float list) =
        printfn $"For"
        vs
        |> Seq.collect f
        |> Seq.toList

    /// This will allow for using the yield! method
    member this.YieldFrom(vs : 'T list) =
        printfn $"YieldFrom {vs}"
        vs

    /// This will run the operation on the final float list, returning a single float as output of the compuation expression.
    member this.Run(vs : float list) =
        printfn $"Run {vs}"
        operation.Apply vs

// For easier initialization of the computation expression
let math = MathBuilder()

(**

So let's test this out
*)

math {
    1
    2
    3
}

// Results in 6

let faculty x =
    math {
        Multiply
        for i = 1 to x do
            i
    }

faculty 4

//Delay
//Delay
//For
//Yield 1
//Yield 2
//Yield 3
//Yield 4
//Delay result: [1; 2; 3; ... ]
//Combine [] with [1; 2; 3; ... ]
//Delay result: [1; 2; 3; ... ]
//Run [1; 2; 3; ... ]
//val it : float = 24.0

(**

As you can see this approach can be used to further tune the Computation expression in a very generic way.

## *Custom Operator* based computation expressions

### Cake builder

Besides the basic keywords shared between all computation expressions, one can also define their own keywords by using the `CustomOperation` class.
Methods with this 

*)

type Cake =

    {
        Size : int
        Filling : string
        PackagedAsPresent: bool
    }

    static member Default = 
        {
            Size = 30
            Filling = "Strawberry"
            PackagedAsPresent = false
        }

type CakeBuilder() =

    member x.Yield (()) = Cake.Default

    member x.Zero () = Cake.Default

    [<CustomOperation("size")>]
    member x.Size (cake : Cake, i: int) =
        {cake with Size = i}
  
    [<CustomOperation("filling")>]
    member x.Filling (cake : Cake, s: string) =
        {cake with Filling = s}

    [<CustomOperation("package")>]
    member x.PackageAsPresent (cake : Cake) =
        {cake with PackagedAsPresent = true}


let cake = CakeBuilder()

(**

Let's bake some cakes

*)

cake {
    ()
}

//val it : Cake = { Size = 30
//                  Filling = "Strawberry"
//                  PackagedAsPresent = false }

cake {
    size 1800
    filling "Irish coffee cream"
    package
}

//val it : Cake = { Size = 1800
//                  Filling = "Irish coffee cream"
//                  PackagedAsPresent = true }


(**

For a good example of this kind of computation expression check out [Saturn](https://saturnframework.org/explanations/routing.html)

## Further reading

- [Basic introduction](https://en.wikibooks.org/wiki/F_Sharp_Programming/Computation_Expressions)
- [In depth introduction](https://fsharpforfunandprofit.com/series/computation-expressions/)
- [Official reference](https://docs.microsoft.com/en-us/dotnet/fsharp/language-reference/computation-expressions)

- [Good video for practical introduction](https://www.youtube.com/watch?v=pC4ZIeOmgB0)

*)