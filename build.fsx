#r @"tools\FAKE\tools\FakeLib.dll"

open Fake
open Fake.VersionHelper
open Fake.NuGetHelper

let assemblyPattern = "*.dll"
let nugetSpecFilePath = "devDept.Eyeshot.nuspec"

let assemblyFile = FindFirstMatchingFile assemblyPattern "binaries"
let version = GetAssemblyVersionString assemblyFile


Target "CreateNuGetPackage" (fun _ ->
    let setParams (p: NuGetParams) =
        { p with
            Version = version
            OutputPath = "."
            WorkingDir = "."
        }
    NuGetPack setParams nugetSpecFilePath
)

Target "Default" DoNothing

"CreateNuGetPackage"
    ==> "Default"

RunTargetOrDefault "Default"