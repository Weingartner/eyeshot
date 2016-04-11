// This script simplifies the update process of the Eyeshot NuGet package.
// Run this after installing a new version of Eyeshot on your machine.
// This script will copy the new dll's and commit the changes.
// After running this script check the commit and change or push it.

#r @"tools\FAKE\tools\FakeLib.dll"

open Fake
open Fake.VersionHelper

let baseDir = __SOURCE_DIRECTORY__
let sourceDir = @"C:\Program Files\devDept Software\Eyeshot Nurbs 9.0\Bin"
let targetDir = baseDir @@ "binaries"

module Option =
    let ofBoolList x =
        if Seq.contains false x then None
        else Some ()

let filePattern =
    !!"*.dll"
    ++"*.xml"

let files =
    filePattern
    |> SetBaseDir sourceDir
    |> Seq.toList

let eachFileAlreadyExists =
    let existingFiles =
        filePattern
        |> SetBaseDir targetDir
        |> Seq.map fileNameWithoutExt
        |> Set.ofSeq
    let newFiles =
        files
        |> Seq.map fileNameWithoutExt
        |> Set.ofSeq
        
    newFiles = existingFiles

if not eachFileAlreadyExists then failwith "File names do not match"

files
|> CopyFiles targetDir

let version =
    FindFirstMatchingFile "*.dll" targetDir
    |> GetAssemblyVersionString

filePattern
|> SetBaseDir targetDir
|> Seq.map (Git.Staging.StageFile baseDir)
|> Seq.map (fun (a, b, c) -> a)
|> Option.ofBoolList
|> Option.map (fun () ->
    Git.Commit.Commit baseDir (sprintf "Update to version %s" version)
)
