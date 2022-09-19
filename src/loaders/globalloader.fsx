#r "../_lib/Fornax.Core.dll"

type SiteInfo = {
    title: string
    description: string
}

let loader (projectRoot: string) (siteContent: SiteContents) =
    let siteInfo =
        {
            title = "CSBlog";
            description = "The blog about all things CSB."
        }
    siteContent.Add(siteInfo)

    siteContent
