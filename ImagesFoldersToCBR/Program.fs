// For more information see https://aka.ms/fsharp-console-apps

open System.IO
open System.IO.Compression

let imageExtension = ".jpg"
let imageSearchPatterns = ["*.jpg"; "*.img"]
let folderName = @"D:\books\hakuneko\20th Century Boys"
let outputFolder = $"{folderName}_cbr"

let mapFormats extension =
    match extension with
    | ".img" -> ".jpg"
    | e -> e

let findExtension (path: string) =
    Path.GetExtension path

let changeExtensions path =
    let ext = findExtension path
    let replace = mapFormats ext
    if replace = ext 
    then path
    else path.Replace(ext, replace)

let fileName (file: FileInfo) =
    file.FullName

let renameIfChange (names: string*string) =
    if (fst names) <> (snd names) then
        printf "r: %s -> %s\n" (fst names) (snd names)
        names |> File.Move

let changeAllFiles (dir: DirectoryInfo) =
    dir.GetFiles()
    |> Array.map fileName
    |> Array.map (fun n -> n, changeExtensions n)
    |> Array.map renameIfChange
    |> ignore

let hasFiles (dir: DirectoryInfo) (pattern: string) =
    dir.GetFiles(pattern).Length <> 0


let getFolders (folder: string) =
   let info = new DirectoryInfo(folder)
   info.GetDirectories()

let getFiles (dir: DirectoryInfo) =
    imageSearchPatterns
    |> List.fold (fun result p -> result || hasFiles dir p) false

let rec findImageFolders (dir: DirectoryInfo) =
    match getFiles dir with
    | false ->
        dir.GetDirectories() 
        |> Array.fold (fun list d -> list |> List.append (findImageFolders d)) []
    | true -> [dir]

let doIfFileDoNotExist filename f =
    match File.Exists filename with
    | false ->
        printf "+ %s - handler file\n" filename
        f filename
    | _ ->
        printf "- %s - file alredy exist\n" filename
        ()

let createZip (dir: DirectoryInfo) =
    let name = dir.Name.Replace(' ', '_')
    let zipName = $"{outputFolder}\{name}.cbr"
    doIfFileDoNotExist zipName 
        <| fun f -> ZipFile.CreateFromDirectory(dir.FullName, f)

let createFolderIfNeed dir =
    if not (Directory.Exists(dir)) then 
       Directory.CreateDirectory(outputFolder)
       |> ignore


let loop () =
    createFolderIfNeed outputFolder
    let folders =
        folderName
        |> getFolders
        |> Array.fold (fun list d -> list |> List.append (findImageFolders d)) [] 
    folders |> List.iter changeAllFiles
    folders |> List.map createZip

printfn "Start"
loop() |> ignore