(**

---
title: How to develop and use a JSON Schema
category: Implementation
categoryindex: 1
index: 0
---

*)

(**

# How to develop and use a JSON Schema

<br></br>

## Content
- [What are JSON Schemas?](#What-are-JSON-Schemas)
- [JSON syntax](#JSON-syntax)
- [The basic structure of a JSON Schema](#The-basic-structure-of-a-JSON-Schema)
- [References](#References)
- [Validation of JSON files by a Schema](#Validation-of-JSON-files-by-a-Schema)
- [Further reading](#Further-reading)

## What are JSON Schemas?

JSON (JavaScript Object Notation) is a common data format for storing data and metadata and widely used for data exchange purposes. It is easily human- as well as machine-readable.  
While JSON files and formats can be written in a simple and non-standardized way (as long as the syntax is respected), standardizing a specific JSON format might be useful when considering validation, documentation, and interaction control.

JSON Schemas are data models that are build out of JSON syntax themselves.

## JSON syntax

Since the syntax of JSON is very simple and easy to understand, here's everything to know:  

- A JSON file consists of at least one empty object
- Objects are opened with `{` and closed with `}`
- Objects consist of key/value (aka name/value) pairs. Keys (or names) are identifiers realized as strings, values are corresponding information realized via primitive data types, both are separated via the character `:`
- There are 6 basic primitive data types:
  - null: no value (`null`)
  - boolean: `true` or `false` value
  - number: decimal number value, e.g. `7` or `1.337`
  - string: unicode text value, e.g. `"Hello World!"`
  - object: unordered set of properties, consists of key/value pairs, e.g. `{ "name": "Max Mustermann", "age": 44 }`
  - array: ordered list of instances, consists of values, e.g. `[ "Hello ", "World!" ]`
- As seen above, `,` is used to separate different key/value pairs in objects and instances in arrays
- Objects can be nested infinitely
- Scoping is similar to F#, both `[ "Hello ", "World!" ]` as well as  
![image](https://user-images.githubusercontent.com/47781170/131900223-3fa36d2f-d65a-44ce-9913-69a2f42b35f2.png)  
  is possible

## The basic structure of a JSON Schema

Since JSON Schemas are JSON files in a sense, they are written very similarly.  
Let's develop a JSON schema for ‒ say – future blogposts. It should consist of an identifier, a title, a category to which it belongs, a value if it is already uploaded or not, and an array of tags.  
A finished JSON file following this schema should therefore look like this:

```javascript
{
  "identifier": 0,
  "title": "How to develop and use a JSON Schema",
  "category": "Implementation",
  "uploaded": true,
  "tags": [ "implementation", "JSON", "standardization" ]
}
```

JSON Schemas must start with 2 unique key/value pairs that are referencing them as specific JSON Schemas:

```javascript
{
  "$schema": "https://json-schema.org/draft/2020-12/schema",
  "$id": "https://raw.githubusercontent.com/omaus/CSBlogpost/main/blogpost.schema.json",
}
```

The first key, `"$schema"` references a JSON Schema to a so-called Metaschema, here of the ID "https://json-schema.org/draft/2020-12/schema".  
The value of second key, `"$id"`, is a unique identifier for this JSON Schema, typically in the form of a URI.  
The `$` is a special character and marks Schema-related keywords.

There's more to add: Our JSON Schema needs a title and a description:

```javascript
{
  "$schema": "https://json-schema.org/draft/2020-12/schema",
  "$id": "https://raw.githubusercontent.com/omaus/CSBlogpost/main/blogpost.schema.json",
  "title": "Blogpost entry",
  "description": "An entry of our blogpost series, consisting of an identifier, a title, a category, the upload status, and some tags.",
}
```

These are called "Schema Annotations" since they annotate our Schema and provide some information about it.
The last thing to add is a so-called "Validation keyword":

```javascript
{
  "$schema": "https://json-schema.org/draft/2020-12/schema",
  "$id": "https://raw.githubusercontent.com/omaus/CSBlogpost/main/blogpost.schema.json",
  "title": "Blogpost entry",
  "description": "An entry of our blogpost series, consisting of an identifier, a title, a category, the upload status, and some tags.",
  "type": "object"
}
```

The JSON files of our Schema must have a type. In our case, it's an object but the primitive data types as well as `integer` (a `number` without fractional parts) are also applicable. We will examine that later.
There are a lot of other Schema Keywords, Schema Annotations and Validation Keywords. We will see some of them later but there are also some that are not important now and can be looked at later if required.

Now, since we wrote the "metadata" of our JSON Schema, we want to add the properties of our blogpost JSON file seen above:

```javascript
{
  "$schema": "https://json-schema.org/draft/2020-12/schema",
  "$id": "https://raw.githubusercontent.com/omaus/CSBlogpost/main/blogpost.schema.json",
  "title": "Blogpost entry",
  "description": "An entry of our blogpost series, consisting of an identifier, a title, a category, the upload status, and some tags.",
  "type": "object",
  "properties": {
    "identifier": {
      "description": "The identifier of a blogpost entry, an integer.",
      "type": "integer"
    },
    "title": {
      "description": "The title of a blogpost entry.",
      "type": "string"
    },
    "category": {
      "description": "The category a blogpost entry belongs to.",
      "type": "string"
    },
    "uploaded": {
      "description": "A value that tells if a blogpost entry has been already uploaded to a repository or not.",
      "type": "boolean"
    },
    "tags": {
        "description": "A list of tags that help describing and categorizing a blogpost entry.",
        "type": "array",
        "items": {
          "type": "string"
        },
        "uniqueItems": true
    }
  },
  "required":  [ "identifier", "title", "category" ]
}
```

`"properties"` is an object consisting of objects. The properties set in our original JSON file are realized here. As for the `"tags"` property, we define that the tags must be different.  
There's another Validation Keyword we added here: The `"required"` key. It is an array consisting of all the properties we think are mandatory for the JSON file to be valid. In our case, we think that the tags and the upload status should rather be optional information and thus are not added to the required list.

Since we are dealing with this JSON file and its Schema right now we are realizing that an author is missing! 😱

```javascript
{
  "identifier": 0,
  "title": "How to develop and use a JSON Schema",
  "category": "Implementation",
  "uploaded": true,
  "tags": [ "implementation", "JSON", "standardization" ],
  "authors": [
    {
      "name": "Oliver Maus",
      "organization": "CSBiology"
    }
  ]
}
```

There's something new: We now must add a nested type to our Schema:

```javascript
{
  "$schema": "https://json-schema.org/draft/2020-12/schema",
  "$id": "https://raw.githubusercontent.com/omaus/CSBlogpost/main/blogpost.schema.json",
  "title": "Blogpost entry",
  "description": "An entry of our blogpost series, consisting of an identifier, a title, a category, the upload status, and some tags.",
  "type": "object",
  "properties": {
    "identifier": {
      "description": "The identifier of a blogpost entry, an integer.",
      "type": "integer"
    },
    "title": {
      "description": "The title of a blogpost entry.",
      "type": "string"
    },
    "category": {
      "description": "The category a blogpost entry belongs to.",
      "type": "string"
    },
    "uploaded": {
      "description": "A value that tells if a blogpost entry has been already uploaded to a repository or not.",
      "type": "boolean"
    },
    "tags": {
      "description": "A list of tags that help describing and categorizing a blogpost entry.",
      "type": "array",
      "items": {
        "type": "string"
      },
      "uniqueItems": true
    },
    "authors": {
      "description": "A list of all authors associated with a blogpost entry.",
      "type": "array",
      "items": {
        "type": "object",
        "properties": {
          "name": {
            "type": "string"
          },
          "organization": {
            "type": "string"
          }
        }
      },
      "minItems": 1,
      "uniqueItems": true
    }
  },
  "required":  [ "identifier", "title", "category", "authors" ]
}
```

We show our Schema to our boss but unfortunately he's not satisfied. He reckons that there should be a license associated with it.

## References

There's already a JSON Schema for licenses. So we want to incorporate it into ours.

```javascript
{
  "$schema": "https://json-schema.org/draft/2020-12/schema",
  "$id": "https://raw.githubusercontent.com/omaus/CSBlogpost/main/license.schema.json",
  "title": "Copyright license",
  "description": "A license regarding copyright",
  "type": "object",
  "properties": {
    "name": {
      "description": "The name of the license",
      "type": "string"
    },
    "version": {
      "description": "The version of this license in the form of 'v(Major).(Minor).(Patch)'.",
      "type": "string"
    },
    "author": {
      "description": "The name of the author of a license.",
      "type": "string"
    },
    "date": {
      "description": "The exact date when a license was published.",
      "type": "string"
    }
  },
  "required": [ "name", "version", "author", "date" ]
}
```

Our new Blogpost Schema:

```javascript
{
  "$schema": "https://json-schema.org/draft/2020-12/schema",
  "$id": "https://raw.githubusercontent.com/omaus/CSBlogpost/main/blogpost.schema.json",
  "title": "Blogpost entry",
  "description": "An entry of our blogpost series, consisting of an identifier, a title, a category, the upload status, and some tags.",
  "type": "object",
  "properties": {
    "identifier": {
      "description": "The identifier of a blogpost entry, an integer.",
      "type": "integer"
    },
    "title": {
      "description": "The title of a blogpost entry.",
      "type": "string"
    },
    "category": {
      "description": "The category a blogpost entry belongs to.",
      "type": "string"
    },
    "uploaded": {
      "description": "A value that tells if a blogpost entry has been already uploaded to a repository or not.",
      "type": "boolean"
    },
    "tags": {
      "description": "A list of tags that help describing and categorizing a blogpost entry.",
      "type": "array",
      "items": {
        "type": "string"
      },
      "uniqueItems": true
    },
    "authors": {
      "description": "A list of all authors associated with a blogpost entry.",
      "type": "array",
      "items": {
        "type": "object",
        "properties": {
          "name": {
            "type": "string"
          },
          "organization": {
            "type": "string"
          }
        }
      },
      "minItems": 1,
      "uniqueItems": true
    },
    "license": {
      "description": "The license used for a blogpost entry.",
      "$ref": "https://raw.githubusercontent.com/omaus/CSBlogpost/main/license.schema.json"
    }
  },
  "required":  [ "identifier", "title", "category", "authors", "license" ]
}
```

Let's look at the finished JSON file for this blogpost:

```javascript
{
  "identifier": 0,
  "title": "How to develop and use a JSON Schema",
  "category": "Implementation",
  "uploaded": true,
  "tags": [ "implementation", "JSON", "standardization" ],
  "authors": [
    {
      "name": "Oliver Maus",
      "organization": "CSBiology"
    }
  ],
  "license": {
    "name": "Creative Commons Attribution 4.0 International Public License",
    "version": "v4.0.0",
    "author": null,
    "date": null
  }
}
```

## Validation of JSON files by a Schema

The Newtonsoft.Json library provides great JSON Schema support regarding validating your JSON files by a given Schema.

*)

#r "nuget: FSharp.Data"
#r "nuget: Newtonsoft.Json"
#r "nuget: Newtonsoft.Json.Schema"

open FSharp.Data
open Newtonsoft.Json.Linq
open Newtonsoft.Json.Schema

// We download the Json strings via FSharp.Data and parse them into a JSchema or JObject, respectively.
let licenseSchema = Http.RequestString @"https://raw.githubusercontent.com/omaus/CSBlogpost/main/license.schema.json" |> JSchema.Parse
let ccLicense = Http.RequestString @"https://raw.githubusercontent.com/omaus/CSBlogpost/main/CCAtt4IPL.json" |> JObject.Parse

// We match the outcome of the IsValid method to get error messages, if available.
match (ccLicense.IsValid(licenseSchema)) : (bool * #seq<string>) with
| (false,msg) -> printfn "not valid due to: %A" msg
| _ -> printfn "valid"
(*** include-output ***)

(**

Oh no, our file seems to be invalid.  
When giving it a closer look, we see that the required properties "author" and "date" are set to null. Thus, we replace them with empty strings and start anew.

*)

let ccLicenseNew = Http.RequestString @"https://raw.githubusercontent.com/omaus/CSBlogpost/main/CCAtt4IPL_new.json" |> JObject.Parse

match (ccLicenseNew.IsValid(licenseSchema)) : (bool * #seq<string>) with
| (false,msg) -> printfn "not valid due to: %A" msg
| _ -> printfn "valid"
(*** include-output ***)

(**

Let's now test this with our blogpost Schema and the corresponding JSON file.  
Since this Schema references another one, we need a resolver:

*)

let resolver = JSchemaUrlResolver()

let blogpostSchema = JSchema.Parse(Http.RequestString @"https://raw.githubusercontent.com/omaus/CSBlogpost/main/blogpost.schema.json", resolver)
let blogpost0 = Http.RequestString @"https://raw.githubusercontent.com/omaus/CSBlogpost/main/blogpost0.json" |> JObject.Parse

match (blogpost0.IsValid(blogpostSchema)) : (bool * #seq<string>) with
| (false,msg) -> printfn "not valid due to: %A" msg
| _ -> printfn "valid"
(*** include-output ***)

(**

## Further reading

- [Understanding JSON](https://json-schema.org/understanding-json-schema/index.html)
- [Official JSON Schema Homepage](https://json-schema.org/)
- [Schema Core Specification](https://json-schema.org/draft/2020-12/json-schema-core.html)
- [Schema Validation Specification](https://json-schema.org/draft/2020-12/json-schema-validation.html)
- [Relative JSON pointers Specification](https://json-schema.org/draft/2020-12/relative-json-pointer.html)

*)