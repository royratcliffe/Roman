namespace Roman

open ViewModule
open ViewModule.FSharp
open System.Collections.ObjectModel
open System.Windows.Input
open FsXaml
open System.Windows

type Point = { X: float; Y: float }
type Line = { Start: Point; End: Point }

module Mouse =

    type Capture = Captured | Released

    type Event =
    | Capture of Capture
    | Move of capture: Capture * position: Point

    let buttonCapture (args: MouseButtonEventArgs) =
        match args.ButtonState with
        | MouseButtonState.Pressed ->
            let source = args.OriginalSource :?> UIElement
            source.CaptureMouse ()
            |> ignore
            Capture(Captured)
        | _ ->
            let source = args.OriginalSource :?> UIElement
            source.ReleaseMouseCapture ()
            Capture(Released)

    let move (args: MouseEventArgs) =
        let source = args.OriginalSource :?> IInputElement
        let capture =
            if System.Object.ReferenceEquals (Mouse.Captured, source)
            then Captured
            else Released
        let position = args.GetPosition (source)
        Move(capture, { X = position.X; Y = position.Y })

type MouseButtonCaptureConverter () =
    inherit EventArgsConverter<MouseButtonEventArgs, Mouse.Event> (Mouse.buttonCapture, Mouse.Capture(Mouse.Released))

type MouseMoveConverter () =
    inherit EventArgsConverter<MouseEventArgs, Mouse.Event> (Mouse.move, Mouse.Capture(Mouse.Released))

type MainViewModel () as me =
    inherit EventViewModelBase<Mouse.Event> ()

    let lines = ObservableCollection<Line> ()

    let mouseCommand = me.Factory.EventValueCommand ()

    let handleMouseEvent =
        function
        | Mouse.Move(Mouse.Captured, startPoint), Mouse.Move(_, endPoint) ->
            { Start = startPoint; End = endPoint }
            |> lines.Add
        | _, _ ->
            ()

    do
        me.EventStream
        |> Observable.pairwise
        |> Observable.subscribe handleMouseEvent
        |> ignore

    member __.Lines = lines

    member __.MouseCommand = mouseCommand
