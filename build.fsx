// include Fake lib
#r @"tools\FAKE\tools\FakeLib.dll"
#r "System.Xml.Linq"
open Fake
open Fake.VersionHelper
open Fake.NuGetHelper

let assemblyPattern = "devDept.*.dll"
let nugetSpecFilePath = "devDept.Eyeshot.nuspec"

let assemblyFile = FindFirstMatchingFile assemblyPattern "."
let version = GetAssemblyVersionString assemblyFile

Target "CreateNuspecFile" (fun _ ->
    NuGetPack (fun p -> { p with
        Version = version
        OutputPath = "."
        WorkingDir = "."
    }) nugetSpecFilePath
)

Target "Default" (fun _ -> trace "packaged Eyeshot")

"CreateNuspecFile"
    ==> "Default"
    
RunTargetOrDefault "Default"