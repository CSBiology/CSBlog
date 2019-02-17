(*** hide ***)
#I @"../../packages"
#r @"../../lib/Formatting/FSharp.Plotly.dll"
open FSharp.Plotly
(**
#Creating a Documentation
##Introduction

This documentation is aimed at people of the CSB team who want to document their stuff by using the FSharp.Formatting formatter. 
There of course is an [in depth documentation of how to use FSharp.Formatting by the creators](http://fsprojects.github.io/FSharp.Formatting/). 
Also there's a good overview of the different mark down commands on [wikipedia](https://de.wikipedia.org/wiki/Markdown). 
Those things can be very cryptical at the beginning though. 
Therefore with this tutorial I want to give a quick and structured introduction into this topic. 
For further information check out the aforementioned websites.  
The basic structure is as follows:

* [At the start I want to introduce you to the **ProjectScaffold**](documentation.html#PS)
* [Afterwards I'll explain **how to get started** and where to find what](documentation.html#Basics)
* [Then I'll give a little introduction on **how you should write the documentian** for your code](documentation.html#What)
* [I also include a small overview over the **different features** you have with markdown language and the formatted F#](documentation.html#Legend)
* [At the end I'll explain **how to test your documentation**](documentation.html#Testing)  

<a name="PS"></a>
##What is Project Scaffold?

The libraries of the CSBiology organization are built using the [ProjectScaffold](https://github.com/fsprojects/ProjectScaffold). In principle, this is a blueprint project to make creating and maintaining a F# project easier.
Besides many other features, this includes automatic Documentation via the [FSharp.Formatting](https://github.com/fsprojects/FSharp.Formatting) formatter. 
This formatter is called when one executes the **build.cmd** which is located in the project folder. Opening this build.cmd does not only compile the project, it also formats the <b>.fsx</b>(FSharp script file) documents to <b>.html</b> documents by using the FSharpFormatting formatter, besides other things. 
The script files which are formatted have to be placed in "..*project name*\docsrc\content". The formatted html files are then automatically placed in "..*project name*\docs". Keep in mind that by using the build.cmd, those files are built and stored only locally.

Now that that's covered let's get to the important questions:
*)
(**
<a name="Basics"></a>
##Where to put what?
Let's say you want to write a documentation about the topic `topic`

* Place `topic.fsx` in "..*project name*\docsrc\content"
* Write the documentation in this `topic.fsx` as follows

   `(*** hide ***)` in front of helpful library calls; Those won't be formatted.  
   Put text into a comment as follows: `(** text *)`. This text will be formatted by markdown.  
   Everything outside those brackets will be formatted as an F#-snippet  

* In the "..*project name*docsrc\tools\templates\template.cshtml": Put a link to the html using the following line in the place you want your link to be created:

   `<li><a href="@Root/topic.html">Topic</a></li>`
   If you have some experience with HTML, how to do this comes quite naturally. If not, you can either do it via [Trial and Error](documentation.html#Testing) or even easier: just ask.

* Commit your changes and create a pull request to the upstream repository. The added htmls will be included in the next release.


<a name="What"></a>
##What should I write about? 

Good question! But all good questions of course also have good answers (unfortunately not really). 
The people who will read your tutorial are of course the ones who want to use your functions. So your task in the tutorial is basically to tell them <b>how to use</b> it, not necessarily how it works. 
The core of your documentation therefore should be code snippets of the function being applied with real values, not snippets of the function definition:

<b class="redText"> Bad Snippet: </b>
"As you can see this function takes an int and a string and multiplies every letter in the string by using an enumerator..."
*)

let multiplyLetters (count:int) (word:string) = 
    let en = word.GetEnumerator()
    let rec loop l =
        match en.MoveNext() with
        | true -> loop (l @ List.init count (fun i -> en.Current))
        | false -> l
    loop []
    |> fun chars -> new string [|for c in chars -> c|]
  
(**
Showing the definition will just result in the reader to lose a lot of time deciphering your code. The snippet should only show how to apply the function, so that the user can pick it up quick and apply it to his values. What the function does behind the screens should be explained separately.

<b class="greenText"> Good Snippet: </b>
"As you can see this function takes an int and a string and multiplies the number of every letter in the string by the int"
*)

let originalString = "Hello!"
let newString = multiplyLetters 5 originalString

(**
In many cases the user will only look for the snippet because he already knows the algorithm but is not sure about how to use the implementation. 
Of course also delivering an explanation of the background of your implementation is highly advised. Especially when the algorithms used are not as trivial as the example above. For this you can give a short intro to the topic in general and even include <a href="@Root/Documentation.html#Links">links</a>.
*)





(**
<a name="Legend"></a>
##What are the features?

As already mentioned, there are already many lists for the standard mark down commands available. A quick google search yields:

* [quick intro by wikipedia](https://de.wikipedia.org/wiki/Markdown)
* [very in-depth list by adam-p](https://github.com/adam-p/markdown-here/wiki/Markdown-Cheatsheet)

For the matter of completion I'll still give a little overview on the different `markdown-commands`:  

* [Headers](documentation.html#Headers) 
* Highlighting
* [Links](documentation.html#Links) 
* [Images](documentation.html#Images) 
* [Lists](documentation.html#Lists) 
* [Escaping Markdown](documentation.html#[Escaping-Markdown) 

What's more important here though are the extra `features of FSharp.Formatting`, which are a bit hard to find otherwise:

* Highlighting
* [f# interactive output](documentation.html#ConsoleOutput) 
* [Plots by FSharp.Plotly](documentation.html#Plots)  


###Headers
For headers, you can just put a number of '#' at the start of the line, the header gets smaller, the more '#' you use:  
#One hashtag
`# One hashtag`

##Two hashtags
`## Two hashtags`

###Three hashtag
`### Three hashtags`

####Four hashtags
`#### Four hashtags`

<br>

###Links
You can easily link external websites or internal documents.

The basic command for this is
<br>
 `[text](link)`
<br>
The text you insert into the square brackets will be formatted to a clickable text. When you click it, the link will be opened. What you link to is your choice. You could for example link to external websites:  
`[Wikipedia](https://de.wikipedia.org/wiki/Wikipedia)` will be [Wikipedia](https://de.wikipedia.org/wiki/Wikipedia).  
On the other hand you can link local files:  
`[index](index.html)` will be [index](index.html)


###Images

Images are included similarly to normal links, but with a `!` in front of them. So instead of "\[text](link)" you use  
`![image name](image link)`  
Again you can link external images with a full url:  
`![Marshmallow](\https://www.sammobile.com/wp-content/uploads/2015/12/android-marshmallow.jpg)` will be  
<br>
![Marshmallow](https://www.sammobile.com/wp-content/uploads/2015/12/android-marshmallow.jpg)

On the other hand you can link local files:  
`![logo](img/logo.png)` will be ![logo](img/logo.png) 
Make sure that the image you want to reference is located at "..*project name*/docs/files/img". Also at the moment only **.png**s can be used for local referencing because only those are copied to the gh-pages in the building process.

###Lists

There are unnumbered and numbered lists. For unnumbered lists, do a break and write consecutive lines with `*` at the beginning. 
For numbered lists, do the same but with numbers+dot instead.
Unnumbered Lists:  

`* Entry1`  
`* Entry2`  
`* Entry3 also I include a piece of text so you get the idea of how the text will align when it reaches the end of the line.`  

will be  

* Entry1
* Entry2
* Entry3 also I include a piece of text so you get the idea of how the text will align when it reaches the end of the line.

Numbered Lists:  

`1. Entry1`  
`2. Entry2`  
`3. Entry3 also I include a piece of text so you get the idea of how the text will align when it reaches the end of the line.`  

will be  

1. Entry1
2. Entry2
3. Entry3 also I include a piece of text so you get the idea of how the text will align when it reaches the end of the line.  

When you want to write **indented paragraphs** for one bullet point, 
after writing the text which is supposed to stick to the bulletpoint, do: break, leave a line, tap space 3 times and write the text.  
`* Entry1`  
`* Entry2 this is the text which sticks to this point, after this text I'll do the aforementioned things for an indented paragraph  `  
``  
`   This is the next paragraph of this bulletpoint  `  
``  
`* Entry3`  

will be  

* Entry1
* Entry2 this is the text which sticks to this point, after this text I'll do the aforementioned things for an indented paragraph  

   This is the next paragraph of this bulletpoint  

* Entry3



###Escaping Markdown

Sometimes you need to use characters for your documentation, which also are used by markdown. 
This can lead to weird outcomes where characters are missing. 
To circumvent this, you can use a `\` (backslash) infront of those characters.  
E.g.:

* `There were two ** in the night sky` will be `There were two in the night sky`  
* `There were two \*\* in the night sky` will be `There were two ** in the night sky`

<a name="ConsoleOutput"></a>
###Console Output
With the FSharp.Formatting you can not only include stylized code snippets, but also include console output of the F# interactive. 

For this, just put (\*\*\* include-value:*ValueName* \*\*\*\) at the position in your document where you want your output to end up. The *ValueName* is the name to which a value has to be bound.

E.g.:
<pre>
let whatAGreatName = 4 + 5

(\*\*\* include-value:whatAGreatName \*\*\*\) 
</pre>
will be

*)

let whatAGreatName = 4 + 5

(*** include-value:whatAGreatName ***) 

(**
<a name="Plots"></a>
###Plots
For an in depth FSharp.Plotly tutorial, look [here](http://muehlhaus.github.io/FSharp.Plotly/).

Plots can also be included via the same procedure:
<pre>
let myChart = Array.init 10 id |> Array.indexed |> Chart.Point

(\*\*\* include-value:myChart \*\*\*\) 
</pre>
will be
*)

let myChart = Array.init 10 id |> Array.indexed |> Chart.Point

(*** include-value:myChart ***) 

(**
<a name="Testing"></a>
##How can I test my formatted documentation?
You can play around with the markdown text in an online tool like [dillinger.io](http://dillinger.io/).  
This is not really helpful though, if you want to test how the formattet site will look like on the project website. [As stated above](documentation.html#How), the build.cmd will create the `htmls`s of the `fsx`s which are located at "..*project name*\docsrc\content" and automatically put them into "..*project name*\docs" together with all other needed files.
You can access the htmls there after building, but the references to the css styling files and all other ressources won't work. Therefore you will get a very plain looking, unstyled website.  
To bring everything in good form. A new buildtarget "**releaseLocal**" was included in the CSB-projects. To access this target:

###With VS2017

* you have to **open your windows command prompt**
* afterwards **navigate to the repository** 

   e.g. `cd C:\Users\*ExampleUser*\Source\Repos\*project name*`

* run "**build -t releaselocal**"

   Besides compiling the repository, this will create the html files and copy everything from "..*project name*\docs" to "..*project name*\temp\localDocs". 

When this is done, you can just open the "temp/localDocs" folder and browse the htmls in a stylized form.

*)
