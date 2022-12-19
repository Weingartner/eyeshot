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
open Fake.Core.CommandLineParsing
open Fake.DotNet.NuGet
open Fake.IO

let cli = """
usage: build.fsx [options]

options:
  --beta      create a package with beta pre release tag
"""
let ctx = Context.forceFakeContext ()
let args = ctx.Arguments
let parser = Docopt(cli)
let parsedArguments = parser.Parse(args)
let isBetaBuild = DocoptResult.hasFlag "--beta" parsedArguments

let dllPath = Constants.binDir
let nuspecTemplate = "template.nuspec"

let assemblyVersion = 
    let preTag = 
      if isBetaBuild then PreRelease.TryParse "beta"
      else None
    let packageVersion =
      Helpers.getAssemblySemVer dllPath
      |> Option.map (fun v -> {v with PreRelease = preTag})

    match packageVersion with
      | Some(ver) -> 
          match ver.PreRelease with
            | Some(pre) -> sprintf "%d.%d.%d-%s" ver.Major ver.Minor ver.Patch pre.Name
            | None -> ver.AsString
      | None -> failwith "could not create package version"

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
            { FrameworkVersion = ".NETFramework4.7.2"
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

Target.runOrDefaultWithArguments "All"
