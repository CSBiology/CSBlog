# CSBlog v3

The csb blog

## Build the blog

### Prerequisites

- [.NET 6.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)
- Any conda distribution (e.g. [anaconda](https://www.anaconda.com/) or [miniconda](https://docs.conda.io/en/latest/miniconda.html))

- `dotnet tool restore` to install the `fornax` tool that is used to build the website
- `conda create --name csblog --file requirements.txt` to set up the conda environment

### develop locally in watch mode

- `conda activate csblog` (this has to be done once for each development session)
- go into the `/src` folder: `cd src`
- `dotnet fornax watch` to start the site generator in watch mode
- go to `127.0.0.1:8080` to see the blog


## add content

posts are generated from the contents of folders in `/src/posts`.

To add a new post:
- add a **folder with the name of your post** to `/src/posts`
- create a `post_config.md` file. this file should *only* create metadata about your post, and must have this structure:
    ```
    ---
    title: <your post title>
    author: <your name>
    author_link: <a link>
    category: <post category>
    date: <YYYY-MM-DD>
    ---
    ```
    - `title` is the title of your post
    - `author` is the author of the post (most likely your name)
    - `author_link` is a link that will be associated with your name. You can for example link your github or twitter account here
    - `category` is one of `progress`, `studynote`, `advanced`
    - `date` is the date of submission in [ISO 8601](https://en.wikipedia.org/wiki/ISO_8601)
- create a `post.ipynb` file that contains your blogpost. This notebook will be parsed and rendered to a html site. **do not forget to save the notebook with cell output**, as the notebook will not be executed on site generation.

### `post_config` Dos and Don'ts

- **Don't** use unguarded `:` characters, as they are used to separate keys and values
    ```
    title: Yes: No <- nope
    ```
- **Do** guard sentences in `"` quotation marks. That way, you can even use `:` in your post titles and summaries
    ```
    title: "Yes: No" <- yes
    ```
    
### `post.ipynb` Dos and Don'ts

- **Do** save notebook cell output. Notebooks are **not** run on site generation

- **Don't** put tables of content into your notebooks. TOC are generated on site generation for each post

- **Don't** use headers (any level of `#`) in markdown to emphasize text - it will be put into the table of contents and you might not want that if you only want to put emphasis. use `**` for that.

## Troubleshooting

```powershell
PS C:\Repos\CSBiology\CSBlog> dotnet fornax watch
Couldn't find config.fsx
Generated site with errors. Waiting for changes...
[11:16:57] Watch mode started. Press any key to exit.
[11:16:57 INF] Smooth! Suave listener started in 21.195ms with binding 127.0.0.1:8080
```

You are probably not in the ./src folder.

```powershell
.
.
.
starting jupyter --output-dir='C:\Users\<user>\AppData\Local\Temp\' nbconvert --to html C:\Repos\CSBiology\CSBlog\src\posts/implementation/consoleTools.ipynb
EX: An error occurred trying to start process 'jupyter' with working directory 'C:\Repos\CSBiology\CSBlog\src'. The system cannot find the file specified.
make sure to set up a conda distribution with nbconvert installed.
.
.
.
```
(and your page is rather empty)

You probably either have conda and/or nbconvert not installed  
➡ install it

-or- you don't have it accessible via PATH  
➡ add it to path -or-  
➡ use Anaconda Prompt instead
