#r @"tools\FAKE\tools\FakeLib.dll"

open Fake
open Fake.VersionHelper
open Fake.NuGetHelper

let assemblyPattern = "*.dll"
let nugetSpecFilePath = "devDept.Eyeshot.nuspec"

let assemblyFile = FindFirstMatchingFile assemblyPattern @"binaries\net472"
let version = GetAssemblyVersionString assemblyFile

TeamCityHelper.SetBuildNumber version

Target "CreateNuGetPackage" (fun _ ->
    let nugetVersion =
        System.Version version
        |> fun v -> sprintf "%d.%d.%d" v.Major v.Minor v.Build

    let setParams (p: NuGetParams) =
        { p with
            Version = nugetVersion
            OutputPath = "."
            WorkingDir = "."
        }
    NuGetPack setParams nugetSpecFilePath
)

Target "Default" DoNothing

"CreateNuGetPackage"
    ==> "Default"

RunTargetOrDefault "Default"