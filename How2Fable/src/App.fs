module App

open Browser.Dom
open Fable.Core

printfn $"{SearchMe.iAmAVeryLongAndEasilySearchableName}"

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

// check your Browser console for output
module JsInteropDynamicCasting =

    open Fable.Core.JsInterop   

    let shuffle: obj = importDefault "shuffle"

    //console.log(shuffle)

    let deck : obj = shuffle?shuffle()
    let card : obj = deck?draw()
    //console.log(deck)
    
    //console.log(card)
    //console.log(card?toShortDisplayString())
    //console.log(card?toString())

let myDeck = JsInteropDynamicCasting.deck

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

    let Shuffle: Shuffle = importDefault "shuffle"  

    let playingCard : obj = importDefault ("../node_modules/shuffle/src/playingCard.js")
    let newCard = createNew playingCard ("Heart", "Five", 5) :?> Cards

open Fable.Core.JsInterop
open JsInteropInterface

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


let mutable drawnCard2 : Cards option = None
let deck = Shuffle.shuffle()

let myButton2 = document.querySelector(".my-button2") :?> Browser.Types.HTMLButtonElement
myButton2.onclick <- fun _ ->
    drawnCard2 <- deck.draw() |> Some
    console.log deck.length
    myButton2.innerText <- sprintf "You pulled: %s " (drawnCard2.Value.toString())

let myButton3 = document.querySelector(".my-button3") :?> Browser.Types.HTMLButtonElement
myButton3.onclick <- fun _ ->
    let newCard = !!MyCard.create "Exodia"
    deck.putOnTopOfDeck [|newCard|]
    console.log deck.length
