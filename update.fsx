// This script simplifies the update process of the Eyeshot NuGet package.
// Run this after installing a new version of Eyeshot on your machine.
// This script will copy the new dll's and commit the changes.
// After running this script check the commit and change or push it.

#r @"tools\FAKE\tools\FakeLib.dll"

open Fake
open Fake.VersionHelper

let baseDir = __SOURCE_DIRECTORY__
let sourceDir = @"C:\Program Files\devDept Software\Eyeshot Ultimate 11\Bin"
let targetDir = baseDir @@ "binaries"

module Option =
    let ofBoolList x =
        if Seq.contains false x then None
        else Some ()

let filePattern = [| 
    "devDept.Geometry.v11.dll"
    "devDept.Graphics.Shaders.v11.dll"
    "devDept.Graphics.Wpf.v11.dll"
    "devDept.Eyeshot.Control.Wpf.v11.dll"
|]

let files =
    filePattern
    |> Seq.map ( fun file -> sourceDir @@ file )

files
|> CopyFiles targetDir

let version =
    FindFirstMatchingFile "*.dll" targetDir
    |> GetAssemblyVersionString

filePattern
|> Seq.map ( fun file -> targetDir @@ file )
|> Seq.map (Git.Staging.StageFile baseDir)
|> Seq.map (fun (a, b, c) -> a)
|> Option.ofBoolList
|> Option.map (fun () ->
    Git.Commit.Commit baseDir (sprintf "Update to version %s" version)
)
