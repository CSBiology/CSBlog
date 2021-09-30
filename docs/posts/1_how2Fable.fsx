(**

---
title: How to use Fable
category: Implementation
categoryindex: 3
index: 1
---

# What is Fable and how to use it

<br></br>

## Content
- [What is Fable?](#What-is-Fable)
- [Fable Example](#Fable-Example)
    - [Fable Template](#Fable-Template)
    - [Fable REPL code](#Fable-REPL-code)
- [F# to JS](#FSharp-to-JS)
- [JS in FSharp](#JS-in-FSharp)
    - [Dynamic casting](#Dynamic-casting)
    - [Setting up an interface](#Setting-up-an-interface)
    - [Typesafety?](#Typesafety)
- [Further reading](#Further-reading)

## What is Fable

> Fable is a compiler that lets you use F# to build applications that run in the JavaScript ecosystem.

What does this exactly mean?

You can write F# code and let fable translate it to javascript. (The following window shows [Fable REPL](https://fable.io/repl/) and in-browser tool live translating F# code to javascript.)

<br>

<details>
<summary>Click to show Fable REPL</summary>

<iframe height="600px" width="100%" src="https://fable.io/repl/#?code=LYewJgrgNgpgBAFRBATgOgKoDsCWIsDOAsAFCkD05cAYiiMHABYAuzADgQFyVggDGBNMBx86BEADNmaPvXIwsAWggFyvZlhjNyEgowCGKNuWbIUFKgDUcBHMzjNG8KDiwBrOPoBGIAG7wJEBQ4UBR4V0CUYH1mPCw4fDgYfT5GBxA2EQs4AE1kOD59eP0ocTgJVzAQoOdklFwsAHM4MPFUPhgCT3sWdi5KXQMjNCDG8lJSDIU4AGUATwJmGGAJklBIWDgAERtRHGEsGJgwbDiugF5VuGu4SioEJ3KQKCgQAHdXZrC2VoVmLsc8AIEDsCQknjgbCg+jmnwKhjAaFINwcczY8BmIPslxIKJRAB84AAJOr-ZF466EgDCUAgXmIuIpcEJO30oCwYAZTOZszY+jAnSuNzucAAgttdih9q4jlVTolCsVSiA4F54CpjukWjAfp0-g5Higih5JBCoTC4YUUIjyddmGj4AAlY1wHHckWOnW-LD-A3wI3uMHw61dABMcDQaDgAEYAAy2vGEywlCDwU2uZgJglijpZm6EgDSnzzlLgAEVUwoSzyAFIpNxCikinYEPYHWVweWEeFKsr7KHLfUgLwAKxgfGYiiCOD+muAyzVKEE1cWMREIQXMGCAHEtKKXs73AQABQASld1ZRAG04LCYFAqqLc4zuSjInAcB-4uHIzHY3BeFvGcHzgZNaXCS88TvEC6z4BsX1fa5oKqCsYCrBDEOQuAiyaOAAF1G1uShEEYGwPy6fRtVka1UXRA0YgKegvFcToIUxUEiiqSjDzcJEEJFABJZgAHIulkYB2S1DVVRARw4E9ajOU8DkJVbKV2yWOVcHwLo3iceJvm9WJcLAGJ9D4lF7ToqkEVdOAAG9ZixTgnLsABueTjRcni4AAX0IkUHjI8S2AgJYKLgFxFm1XUCD+OESigP1gyU1xkoFOCLJuWB7AkaAoC2ccPDdJkr0Jd9gVBNKbxJQx-g81l2U5DyaTpAgPJmPkBQIPCAJASC3yCFoXTSni0F3Zh9ygHiT3PXgBqZLDHPY5hzkq5gPJ484Aw8XzmQIsh+OIoKuhgAAPNkB0YrB-BQX1KOEmzrWEhJR3HexTAhRYpSaLLrhyuA9HeAAFaFYSaJ6qmPPgXMh88SopAGdpmZgfuaBHX2iZhUgKNAfI+RwFsJJ94EUAA+OAACIScponsLhcmqZwxpaYwplCVQ6ZGcpzmsFZxDS1gjxuaF-mBaTFN4HiRnvrhLBqwB9aUbRi82YpLGcb4NAVrgAnGDp1r6Tgbm+FpekxcQlkcDZfAlO5sBraaggLdfQlOv5VjuYILrOhd7lCVqu6um5pw6ud6tkdRuE4AAaipoNKdjwGsWV4tDpRQLSNOi7gCun4Mwol5kqtVLikhMG4Qy3iEwB-OfSmyHZtV7l3xLr9ynywq4L6ha6+YCR4kpgBSZ24GPIG3lBi0IdskvT0IgGwCNN4AHlNFFDlgZ+5hIbPZumSRzi7M0N5ZgWJZgDxzj6DPas8peLveKvANEQAOXO5hj3vgqirQAAZBQjRHCnl6tWfEFNx6MBBhXGeNEyYUz7gPKmI9Kbz0OgmJe+hV7r03tvXeaCUSYOwTADeYAt4ZnwRg5ea8SG4IoQiW+6DDpAA&html=DwCwLgtgNgfAsAKAAQqaApgQwCb2ag4CdMTJcMABwFp0BHAVwEsA3AXgCIBhAewDsw6AdQAqAT0roOSAMb9BAzoIAeYAPThoAbhkhMAJwDOJNgzAAzagA4OeQhqy5EhAEY9sYu6mBq3HvD6asEA&css=Q"></iframe>

</details>

<br>

Target runtime is JavaScript! _All code execution will be done on a JavaScript level_. Fable output can only be executed in the Browser.

## Fable Example

All further examples are based on the standard Fable template, which you can install as described [here](https://fable.io/docs/2-steps/your-first-fable-project.html).

### Fable Template

Short summary:

- `dotnet new --install Fable.Template` &#10142; install newest Fable template.
- `dotnet new fable` &#10142; create new Fable template in open folder.
- `npm install` &#10142; install all JS dependencies.
- `npm start` &#10142; install all .NET dependencies and start local webpack server with fable output.

You can find the example project in the CSBlogpost repository (*How2Fable*-Folder).

First to the important elements in the fable folder:

- `package.json`
    - `scripts` defines for example `npm start` as `dotnet fable watch src --run webpack-dev-server`
    -  `devDependencies` reference of all JS dependencies.
- `webpack.config.js` all settings for webapplication. For exmaple, determines localhost port.
- `public\index.html` the base [html](https://www.w3schools.com/html/default.asp) page displayed in browser.
- `public\bundle.js` all Fable output bundled together.
- `src`, this folder contains the F# project, which will be compiled to JS.
    - Can use [paket](https://fsprojects.github.io/Paket/) or nuget references in ``.fsproj`` file for dependencies.
    - Can consist of multiple ``.fs`` files.

### Fable REPL code

In the next step i added the code from the Fable REPL above to the Fable template project and adjusted the button for it.
This will result in a button (which can be found in the ``index.html``), which displays on click one drawn card.async

<details>
<summary>index.html code</summary>

```html
<!--index.html-->
<body>
    <p>Fable is running</p>
    <p>You can click on this button:</p>
    <!--This is our button-->
    <button class="my-button">Click me</button>
    <script src="bundle.js"></script>
</body>
```
</details>

<details>
<summary>App.fs code</summary>

```fsharp
/// App.fs
/// 
module Cards =

    /// The following represents the suit of a playing card.
    type Suit =
        | Hearts
        | Clubs
        | Diamonds
        | Spades

    /// A Discriminated Union can also be used to represent the rank of a playing card.
    type Rank =
        /// Represents the rank of cards 2 .. 10
        | Value of int
        | Ace
        | King
        | Queen
        | Jack

        /// Discriminated Unions can also implement object-oriented members.
        static member GetAllRanks() =
            [ yield Ace
              for i in 2 .. 10 do yield Value i
              yield Jack
              yield Queen
              yield King ]

    /// This is a record type that combines a Suit and a Rank.
    /// It's common to use both Records and Discriminated Unions when representing data.
    type Card = { Suit: Suit; Rank: Rank }

    /// This computes a list representing all the cards in the deck.
    let fullDeck =
        [| for suit in [ Hearts; Diamonds; Clubs; Spades] do
              for rank in Rank.GetAllRanks() do
                  yield { Suit=suit; Rank=rank } |]

    /// This example converts a 'Card' object to a string.
    let showPlayingCard (c: Card) =
        let rankString =
            match c.Rank with
            | Ace -> "Ace"
            | King -> "King"
            | Queen -> "Queen"
            | Jack -> "Jack"
            | Value n -> string n
        let suitString =
            match c.Suit with
            | Clubs -> "clubs"
            | Diamonds -> "diamonds"
            | Spades -> "spades"
            | Hearts -> "hearts"
        rankString  + " of " + suitString

    /// This example prints all the cards in a playing deck.
    let printAllCards() =
        for card in fullDeck do
            printfn "%s" (showPlayingCard card)

    let drawOneAndPrintCard() =
        let rand = new System.Random()
        fullDeck.[rand.Next(fullDeck.Length)] 
        |> (showPlayingCard)

// Mutable variable to count the number of times we clicked the button
let mutable drawnCard = ""

// Get a reference to our button (in file: index.html) and cast the Element to an HTMLButtonElement.
/// The last step is necessary as the compiler does otherwise not understand which type the html element has.
let myButton = document.querySelector(".my-button") :?> Browser.Types.HTMLButtonElement

// Register our listener
myButton.onclick <- fun _ ->
    drawnCard <- Cards.drawOneAndPrintCard()
    myButton.innerText <- sprintf "You pulled: %s " drawnCard
```

</details>

The example will at this point, and after clicking the button, look like this:

![Image of Fable with the code implemented](../img/How2Fable_example1.png)

## FSharp to JS

How can Fable translate our F# code to JS? Because most functions of the core F# libraries were redone in JS and Fable downloads these functions to ``src\.fable``.
These are then used to compile the F# source files into their JS counterparts. After ``npm start``, Fable will create ``.fs.js`` (default) files from all F# source files.
For example ``App.fs``  &#10142; ``App.fs.js``. If we look into these, we can see that the modules are imported from the ``.fable`` folder and used to translate our code.
This translated code will be used for the webpack app.

To further showcase this, i created a new source file `SearchMe.fs` with just one binding and call this binding from the App.fs file.
This is necessary, as `App.fs` is defined as our app entry in the `webpack.config.js` and Fable will only compile referenced F# code to JS, to minimize bloat.

```fsharp
/// SearchMe.fs
///
module SearchMe

let iAmAVeryLongAndEasilySearchableName = "Hello World"
```

```fsharp
/// App.fs
///
printfn $"{SearchMe.iAmAVeryLongAndEasilySearchableName}"
```

`npm run build` will bundle all Fable-compiled JS files into `public\bundle.js`. This file will contain imports from `./src/.fable/`, 
but also one for each F# source file. If we search it for *iAmAVeryLongAndEasilySearchableName* or *drawOneAndPrintCard* we will find the respective JS code.

<br>

Because Fable can only compile code patterns and the functions from the modules in ``src\.fable`` it is **not** possible to compile any F# library with Fable out of the box.

If we add `<PackageReference Include="FSharp.Stats" Version="0.4.2" />` to `App.fsproj` it will return an error.

```fsharp
/// App.fs
///
open FSharp.Stats

// Example from https://fslab.org/FSharp.Stats/BasicStats.html
let mean1 = 
    [10; 2; 19; 24; 6; 23; 47; 24; 54; 77;]
    |> Seq.meanBy float

printfn $"{mean1}"
```

Error in powershell/cmd.
```powershell
F# compilation finished in 59ms
C:/Users/User/source/repos/How2Fable/src/App.fs(70,13): (70,18) error FSHARP: The namespace 'Stats' is not defined. (code 39)
C:/Users/User/source/repos/How2Fable/src/App.fs(74,12): (74,18) error FSHARP: The value, constructor, namespace or type 'meanBy' is not defined. Maybe you want one of the following:
   maxBy
   minBy
   maxBy
   minBy (code 39)
Watching src
```

Fable has no idea about the `Seq.meanBy` function and cannot translate it to JS. For a list of Fable compatible libraries you can look 
[here](https://fable.io/docs/dotnet/compatibility.html) for F# Core libraries and [here](https://fable.io/resources.html#Libraries) for community ressources.

It is also **possible to make F# libraries Fable compatible** as described [here](https://fable.io/docs/your-fable-project/author-a-fable-library.html). 
This was for example done for the [ISADotNet library](https://github.com/nfdi4plants/ISADotNet/blob/developer/src/ISADotnet/ISADotNet.fsproj#L65) with a 
conditional trigger, to publish two nuget package versions. One with and one without Fable compatibility.


<details>
<summary>App.fsproj code</summary>
```xml
<!--App.fsproj-->
<ItemGroup Condition=" '$(PackageId)' == 'ISADotNet.Fable' ">
  <Content Include="*.fsproj; **\*.fs; **\*.fsi" PackagePath="fable\" />
</ItemGroup>
```
</details>


<details>
<summary>build.fsx code</summary>

This code is a modified version of the build target from the [libary development knowledgebase article](https://github.com/CSBiology/KnowledgeBase/blob/main/knowledgebase/devops/library-development/full-guide.md#packing-a-nuget-package).

```fsharp
/// build.fsx
///
let pack = BuildTask.create "Pack" [clean; build; runTests; copyBinaries] {
    if promptYesNo (sprintf "creating stable package with version %s OK?" stableVersionTag ) 
        then
            !! "src/**/*.*proj"
            |> Seq.iter (Fake.DotNet.DotNet.pack (fun p ->
                let msBuildParams =
                    {p.MSBuildParams with 
                        Properties = ([
                            "Version",stableVersionTag
                            "PackageReleaseNotes",  (release.Notes |> List.map replaceCommitLink |> String.concat "\r\n")
                        ] @ p.MSBuildParams.Properties)
                    }
                {
                    p with 
                        MSBuildParams = msBuildParams
                        OutputPath = Some pkgDir
                }
            ))
            /// This is used to create ISADotNet.Fable with the Fable subfolder as explained here:
            /// https://fable.io/docs/your-fable-project/author-a-fable-library.html
            "src/ISADotNet/ISADotNet.fsproj"
            |> Fake.DotNet.DotNet.pack (fun p ->
                let msBuildParams =
                    {p.MSBuildParams with 
                        Properties = ([
                            "PackageId", "ISADotNet.Fable"
                            "Version",stableVersionTag
                            "Description","Fable compliant release for the ISA compliant experimental metadata toolkit in F#. Additionally to the compiled library, it is shipped with the uncompiled code."
                            "PackageTags","F# FSharp dotnet .Net bioinformatics biology datascience metadata investigation study assay ISA Json Fable"
                            "PackageReleaseNotes",  (release.Notes |> List.map replaceCommitLink |> String.concat "\r\n")
                        ] @ p.MSBuildParams.Properties)
                    }
                let test = p
                {
                    p with 
                        MSBuildParams = msBuildParams
                        OutputPath = Some pkgDir
                }
            )
    else failwith "aborted"
}

```
</details>

*(ISADotNet was changed and is at the time of writing not Fable compatible, because of the JSON parser library.)*

## JS in FSharp

Some of the following text is based on an explanation from the awesome guys at [@compositionalit](https://twitter.com/compositionalit)!

One of the big advantages of Fable is not only that F# code becomes accessible with JavaScript, but also that one can use JavaScript libraries in F#.

The javascript library that we'll be interacting with is [shuffle](https://www.npmjs.com/package/shuffle), because why should we implement a all those cards functions if they already exist ..

### Installing the shuffle npm package

1. Install the `shuffle` package using `npm` or `yarn`

```powershell
npm install shuffle
```

### Dynamic casting

```fsharp
/// App.fs
///
open Fable.Core.JsInterop

let shuffle: obj = importDefault "shuffle"

console.log(shuffle)
```

> For more information on import statements in fable visit the [SAFE documentation](https://safe-stack.github.io/docs/recipes/javascript/import-js-module/) or the [offical Fable docs](https://fable.io/docs/).

1. To check that the package has been brought in add `console.log(shuffle)` within the `App.fs` file

2. Open the browsers console window (Shift + ⌘ + J (on macOS) or Shift + CTRL + J (on Windows/Linux)) and you should see something like:

```javascript
{ Object
    playingCards: ƒ ()
    shuffle: ƒ (options)
    [[Prototype]]: Object }
```

If you've done something wrong such as mispelling the import you may see:

```javascript
client:159 ./src/App.fs.js
Module not found: Error: Can't resolve 'shufle' in 'C:\Users\User\source\repos\How2Fable\src'
```

### Use dynamic casting to access members.

Dynamic casting relies on your understanding of the code. The `?` operator allows us to access any members of our JsInterop elements.

The following is taken from the official documentation. We want to access those functions with the `?` operator.

```javascript
var Shuffle = require('shuffle');
var deck = Shuffle.shuffle();
var card = deck.draw();
```

```fsharp
let shuffle: obj = importDefault "shuffle"
let deck : obj = shuffle?shuffle()
let card : obj = deck?draw()
console.log(card)
```

We check our console and see that it worked and we got a random card consisting of 3 values: suit, description and sort.
In addition a `card` has two more members `toShortDisplayString` and `toString`. Which we can access in the same way.

```javascript
// console.log(card)

module.exports {suit: 'Spade', description: 'Ten', sort: 10, toString: ƒ, toShortDisplayString: ƒ}
description: "Ten"
sort: 10
suit: "Spade"
toShortDisplayString: ƒ ()
toString: ƒ ()
```

```fsharp
console.log(card?toShortDisplayString())
console.log(card?toString())
```

```
10S
Ten of Spades
```

We implement our dynamic casting functions for a button, by duplicating the button functionality with `myButton2`.


<details>
<summary>myButton2</summary>

```html
<!--index.html-->
<body>
    <p>Fable is running</p>
    <p>You can click on this button:</p>
    <button class="my-button">Click me</button>
    <!--This is our new button-->
    <button class="my-button2">Click me</button>
    <script src="bundle.js"></script>
</body>
```

```fsharp
/// App.fs
///
open Fable.Core.JsInterop

let mutable drawnCard2 = ""

let myButton2 = document.querySelector(".my-button2") :?> Browser.Types.HTMLButtonElement
myButton2.onclick <- fun _ ->
    drawnCard2 <- JsInteropDynamicCasting.deck?drawRandom()?toString()
    myButton2.innerText <- sprintf "You pulled: %s " drawnCard2
```

</details>

And it works! 🎉

![Fable example for dynamic casting button](../img/How2Fable_example2.png)

### Setting up an interface

Rather than unsafely accessing the `shuffle` it would be better to use an interface 
so that we can use the normal `.` notation to call methods on `shuffle`.

1. Declare an interface type called `Shuffle`. This interface will have to be declared before:

```fsharp
let Shuffle: obj = importDefault "shuffle"
```

2. The interface will need an ``abstract`` member called `shuffle` that is a function of type `unit -> obj`.

```fsharp
type Shuffle =
    abstract member shuffle: unit -> obj
```

3. Update the `shuffle` import to use the new type `Shuffle` rather than `obj`.

```diff
--  let Shuffle: obj        = importDefault "shuffle"
++  let Shuffle: Shuffle    = importDefault "shuffle"  
```

In the end you can write Fable bindings for all functions of the npm library which could look like the following:

```fsharp
module JsInteropInterface =

    open Fable.Core.JsInterop   

    type Cards = 
        abstract member suit: string with get, set
        abstract member description: string with get, set
        abstract member sort: int with get, set
        abstract member toString: unit -> string
        abstract member toShortDisplayString: unit -> string

    type Deck =
        abstract member length : int
        abstract member cards: Cards []
        abstract member draw : int -> Cards
        abstract member draw : unit -> Cards
        abstract member drawRandom : int -> Cards
        abstract member drawRandom : unit -> Cards
        abstract member putOnTopOfDeck: Cards -> unit
        abstract member putOnTopOfDeck: Cards [] -> unit
        abstract member putOnBottomOfDeck: Cards -> unit
        abstract member putOnBottomOfDeck: Cards [] -> unit

    type Shuffle =
        abstract member shuffle: unit -> Deck
        abstract member shuffle: 'a -> Deck
        abstract member playingCards: unit -> obj

    let Shuffle: Shuffle = importAll "shuffle"  
```

With this done we can update our `myButton2` to use our typesafe bindings. While we are at it, we change the mutable to an `option` and 
stop initializing a new deck everytime we press the button. 

```fsharp
let mutable drawnCard2 : Cards option = None
let deck = Shuffle.shuffle()

let myButton2 = document.querySelector(".my-button2") :?> Browser.Types.HTMLButtonElement
myButton2.onclick <- fun _ ->
    drawnCard2 <- deck.draw() |> Some
    console.log deck.length
    myButton2.innerText <- sprintf "You pulled: %s " (drawnCard2.Value.toString())
```

### Typesafety?

JavaScript is not type safe and while we are writing F# we use the JS runtime to execute our code. This is best shown in the following example.
The implemented Deck.putOnTopOfDeck uses a card as input. Sadly, the author of `shuffle` did not intend us to add more standard cards to the deck 
and does *not* expose the create function for us to use in Fable. Altough there are some options on how to circumvent this.

#### Goal

Add a button which puts a specific card on top of the deck with `deck.putOnTopOfDeck`.

<details>
<summary>Button html</summary>

```html
<!--index.html-->
<body>
    <p>Fable is running</p>
    <p>You can click on this button:</p>
    <!--This are our buttons-->
    <button class="my-button">Click me</button>
    <button class="my-button2">Click me</button>
    <button class="my-button3">Is this allowed?</button>
    <script src="bundle.js"></script>
</body>
```

</details>

Use `deck.putOnTopOfDeck` (with the array parameter overload, as the single card version seems to be buggy and does *not* work), but we still need the new card.

```fsharp
let myButton3 = document.querySelector(".my-button3") :?> Browser.Types.HTMLButtonElement
myButton3.onclick <- fun _ ->
    deck.putOnTopOfDeck [||]
    console.log deck.length
```

#### Option 1: Search and explicit import.

It is possible to search through the JS library (`"\node_modules\shuffle\src\playingCard.js"`) to find the `card` creation and definition logic.
And even though it is not exported to the main index.js we can access the exact file and use it to create `card`s.

```fsharp
let playingCard : obj = importDefault ("../node_modules/shuffle/src/playingCard.js")
// createNew is a Fable function to simulate the JS `new` operator
let newCard = createNew playingCard ("Heart", "Five", 5) :?> Cards
```

This is only necessary because the author did not intend this behavior, instead we are meant to write our own card type, as shown in the following example from the docs.

```javascript
var Shuffle = require('shuffle');
var goFish = [{color: 'red', number: 1}, {color: 'blue', number: 2}, ...];
var deck = Shuffle.shuffle({deck: goFish});
```

But more on that later.

#### Option 2: Anonymous records and createObject

We could imitate the JS `card` object with different options. Fable translates F# anonymous records to JS objects.

```fsharp
let newCard : Cards = !!{|suit = "Heart"; description = "Five"; sort = 5|}
```

Altough deck.putOnTopOfDeck is of type `Cards [] -> unit` we can handle the anonymous record as such, because of the `!!` operator. 
It more or less tells the dotnet compiler to not worry about typesafety for whatever follows it. Therefore we can declare `newCard : Cards`.

If we just add this card to the input array we will get an error, because we try to call the `toString()` method on the drawn cards and our anonymous record type has no such member.
_But_ we can replace `drawnCard2.Value.toString()` with for example `drawnCard2.Value.suit` and it will work just fine! 🎉
The Javascript compiler will try to access the `suit` member and both `Cards` as well as our anonymous record have this member and we could change the print output accordingly

Fable provides us with even more alternatives, which work just like the anonymous recordType as they *will not contain the `toString()` method*.

<details>
<summary>Alternative 1</summary>

```fsharp
let newCard = createEmpty<Cards>
newCard.suit <- "Heart"
newCard.description <- "Five"
newCard.sort <- 5
```
</details>


<details>
<summary>Alternative 2</summary>

```fsharp
let newCard = jsOptions<Cards>(fun newCard ->
    newCard.suit <- "Heart"
    newCard.description <- "Five"
    newCard.sort <- 5
)
```
</details>

<br>

#### Option 3: F# Record type

As we just saw we can circumvent type safety quite easily and the author already intends us to write our own card type, so we will do exactly that. <sub>(Please don't cringe about the following joke)</sub>

```fsharp
// https://fable.io/docs/communicate/fable-from-js.html
[<AttachMembers>]
type MyCard = {
    Name : string
} with
    static member create name = {Name = name}
    member this.toString() =
        match this.Name with
        | "Exodia" -> $"{this.Name} 😱. That means you win!"
        | anythingElse -> $"{anythingElse}?? .."
```

This type is rather different to the `Cards` type, except it also contains a `.toString()` method. So we will of course tell 
the typesafe dotnet compiler to not worry and will add our Joker to the pile of poker cards.
Because the only time we interact with the card will be when we call the `toString()` method it will work without any problems.

```fsharp
let myButton3 = document.querySelector(".my-button3") :?> Browser.Types.HTMLButtonElement
myButton3.onclick <- fun _ ->
    let newCard = !!MyCard.create "Exodia"
    deck.putOnTopOfDeck [|newCard|]
    console.log deck.length
```

![Fable example ignoring type safety button](../img/How2Fable_example3.png)

This is an extreme example on how JS and F# will interact through Fable. By ignoring the F# typesafety with `!!` or 
using dynamic casting we make our code error-prone. 

## Further reading

- [Official Fable docs](https://fable.io/docs/)
- [Computational IT Blogs](https://www.compositional-it.com/news-blog/)

Blogpost ressources

- [Fable REPL](https://fable.io/repl/)
- [Fable compatible F# Core libraries](https://fable.io/docs/dotnet/compatibility.html)
- [Fable compatible community ressources](https://fable.io/resources.html#Libraries)
- [Shuffle](https://www.npmjs.com/package/shuffle)
- [SAFE stack](https://safe-stack.github.io/docs/intro/)
*)