// This script simplifies the update process of the Eyeshot NuGet package.
// Run this after installing a new version of Eyeshot on your machine.
// This script will copy the new dll's and commit the changes.
// After running this script check the commit and change or push it.

#r "paket:
nuget FSharp.Core 6.0
nuget Fake.IO.FileSystem
nuget Fake.Core.Target
nuget Fake.Tools.Git
//"
#load "./.fake/update.fsx/intellisense.fsx"
#load "./scripts/Constants.fsx"
#load "./scripts/Helpers.fsx"

open Fake.Core
open Fake.IO
open Fake.IO.FileSystemOperators
open Fake.IO.Globbing.Operators
open Fake.Core.TargetOperators
open Fake.Tools
open System.Collections.Generic

Target.initEnvironment ()

let baseDir = __SOURCE_DIRECTORY__
let dllPath = baseDir @@ Constants.binDir
let tfms = [Constants.frameworkTfm;Constants.coreTfm]

let eyeshotFilePattern = 
    !! Constants.eyeshotDllPattern
    ++ Constants.eyeshotXmlPattern

let getEyeshotFilePaths tfm = 
    let filesFound = 
        eyeshotFilePattern.SetBaseDirectory (Constants.sourceDir @@ tfm)
        :> IEnumerable<string>

    tfm, filesFound

let files = tfms |> Seq.map getEyeshotFilePaths

let copyToBuildFolder (tfm, sourceFilePaths) =
    let targetDir = Constants.binDir @@ tfm
    let traceAndCopy = (fun file ->
        Trace.trace ("copy file: " + file)
        Trace.trace (" >>>>>> to >>>>>>> " + targetDir)
        Directory.ensure targetDir
        Shell.copyFile targetDir file
    )
    let moveToSubFolders file folder =
        Directory.ensure folder
        Shell.moveFile folder file
    Seq.iter traceAndCopy sourceFilePaths

    Trace.trace ("subdividing wpf and winforms")
    !! Constants.eyeshotWpfFiles
    |> GlobbingPattern.setBaseDir targetDir
    |> Seq.iter (fun filename -> moveToSubFolders filename (targetDir @@ Constants.wpfFolder))

    !! Constants.eyeshotWinFormsFiles
    |> GlobbingPattern.setBaseDir targetDir
    |> Seq.iter (fun filename -> moveToSubFolders filename (targetDir @@ Constants.winFormsFolder))

// *****************************************************
// ******************* T A R G E T S *******************
// *****************************************************
Target.create "Clean" (fun _ ->
    Shell.cleanDir Constants.binDir
)

Target.create "CopyFiles" (fun _ ->
    files
    |> Seq.iter copyToBuildFolder
)

Target.create "Commit" (fun _ ->
    let version = 
        match Helpers.getAssemblySemVer dllPath with
        | Some(ver) -> ver.AsString
        | None -> failwith $"could not find dlls in path {dllPath} to get a version"

    Trace.trace ("assembly version = " + version)

    let stageAllFilesIn = DirectoryInfo.ofPath 
                            >> DirectoryInfo.getMatchingFilesRecursive "*" 
                            >> Seq.map (fun fileInfo -> fileInfo.FullName)
                            >> Seq.map (Git.Staging.stageFile baseDir)
                            >> Seq.map (fun (a,b,c)->a)
                            >> (fun stagingBools -> 
                                if stagingBools |> Seq.contains false then None
                                else Some())
    match stageAllFilesIn Constants.binDir with
        | Some() -> Git.Commit.exec baseDir (sprintf "Update to version %s" version)
        | None -> failwith "staging failed"
)

Target.create "All" (fun _ ->
    Trace.trace "~~~~~~~~~~~ update finished ~~~~~~~~~~~"
    Trace.trace "check the commited files and version number"
    Trace.trace "run the build script next if you want to build a local package"
)

"Clean"
  ==> "CopyFiles"
  ==> "Commit"
  ==> "All"

Target.runOrDefault "All"
