#r "paket:
nuget FSharp.Core 6.0
nuget Fake.BuildServer.TeamCity
nuget Fake.Core.Target
nuget Fake.DotNet.NuGet
nuget Fake.IO.FileSystem
//"
#load ".fake/build.fsx/intellisense.fsx"
#load "./scripts/Constants.fsx"
#load "./scripts/Helpers.fsx"

open Fake.BuildServer
open Fake.Core
open Fake.Core.TargetOperators
open Fake.DotNet.NuGet
open Fake.IO
open Fake.IO.FileSystemOperators

Target.initEnvironment ()

let dllPath = Constants.binDir
let nuspecFilePath = "devDept.Eyeshot.nuspec"
let nuspecTemplate = "template.nuspec"

let assemblyVersion = 
    match Helpers.getAssemblySemVer dllPath with
    | Some(ver) -> ver.AsString
    | None -> failwith $"could not find dlls in path {dllPath} to get a version"

BuildServer.install [TeamCity.Installer]
Trace.setBuildNumber assemblyVersion

Target.create "Clean" (fun _ ->
    Shell.cleanDir Constants.packageOutput
)

Target.create "CreatePackage" (fun _ ->
    Trace.trace ( "create nuget package version: " + assemblyVersion)
    NuGet.NuGetPack (fun ps -> 
      { ps with
          Version = assemblyVersion
          OutputPath = Constants.packageOutput
          WorkingDir = "."
          Publish = false
          DependenciesByFramework = [
            { FrameworkVersion = "net472"
              Dependencies = []}
            { FrameworkVersion = "net6.0-windows7.0"
              Dependencies = [
                "System.Management","6.0.0"
                "System.ServiceModel.Primitives","4.5.3"]}
          ]
          Files = [
            (@"binaries\net472\*.*", Some @"lib\net472", None)
            (@"binaries\net6.0-windows\*.*", Some @"lib\net6.0-windows7.0", None)
          ]
      })
      nuspecTemplate
)

Target.create "All" (fun _ -> 
    Trace.trace "nuget package created"
)

"Clean"
  ==> "CreatePackage"
  ==> "All"

Target.runOrDefault "All"
