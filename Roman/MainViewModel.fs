namespace Roman

open ViewModule
open ViewModule.FSharp
open System.Collections.ObjectModel

type Point = { X: float; Y: float }
type Line = { Start: Point; End: Point }

type MainViewModel () =
    inherit ViewModelBase ()

    let lines = ObservableCollection<Line> ()

    do
        { Start = { X = 0.0; Y = 0.0 }; End = { X = 100.0; Y = 100.0 } }
        |> lines.Add

    member __.Lines = lines
