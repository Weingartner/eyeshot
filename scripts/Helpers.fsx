#r "paket:
nuget FSharp.Core 6.0
nuget Fake.IO.FileSystem
nuget Fake.Core.SemVer
//"
#load @".\Constants.fsx"

open Fake.Core
open Fake.IO.FileSystemOperators
open Fake.IO.Globbing.Operators
open System.Reflection

let firstDllIn path =
    let files = !! (path @@ "*.dll")
    Seq.tryHead files

let getAssemblySemVer path = 
    firstDllIn (path @@ "**") 
    |> Option.map AssemblyName.GetAssemblyName
    |> Option.map (fun asmName -> asmName.Version.ToString(3))
    |> Option.map SemVer.parse
