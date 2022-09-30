# CSBlog v2

this new version of our blog aims to get rid of all the baggage caused by fsdocs and package incompatibilities.

Additionally, it should make authoring content a lot easier.

**Blog posts are now a single `.ipynb` notebook. Thats it.**

This also comes with the advantage that we now support **all programming languages that provide a kernel for jupyter**.

## How to author content:

- select a fitting category in the [`src/posts/`](src/posts/) subfolder
- if you need a new category, create a new folder
- create a notebook file using your language of choice. 
- write your post as a combination of code and markdown cells.
- make sure to **execute all cells and save the notebook with output, do not clear cell output** (or the output will also be removed from the rendered website)

## How to develop/inspect the website locally

! Note that this is only needed if you want to preview the website locally. 

- install `dotnet sdk 6.x.x +`
- install a conda distribution (i would recommend [miniconda](https://docs.conda.io/en/latest/miniconda.html) for a minimal installation, or [anaconda](https://www.anaconda.com/) if you need the full thing for other use cases as well)
- run `conda init` (if you chose not to add conda to path, you'll need to navigate to the conda install location)
- run `conda install nbconvert -y` (this is the tool that is used by the blog to convert `.ipynb` files to html)
- `dotnet tool restore`
- `dotnet fornax watch`
