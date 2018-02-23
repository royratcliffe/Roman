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
    | CaptureEvent of capture: Capture
    | MoveEvent of capture: Capture * point: Point

    let buttonCapture (args: MouseButtonEventArgs) =
        match args.ButtonState with
        | MouseButtonState.Pressed ->
            let source = args.OriginalSource :?> UIElement
            source.CaptureMouse ()
            |> ignore
            CaptureEvent(Captured)
        | _ ->
            let source = args.OriginalSource :?> UIElement
            source.ReleaseMouseCapture ()
            CaptureEvent(Released)

    let move (args: MouseEventArgs) =
        let source = args.OriginalSource :?> IInputElement
        let captured =
            if System.Object.ReferenceEquals (Mouse.Captured, source)
            then Capture.Captured
            else Capture.Released
        let point = args.GetPosition (source)
        MoveEvent(captured, { X = point.X; Y = point.Y })

type MouseButtonCaptureConverter () =
    inherit EventArgsConverter<MouseButtonEventArgs, Mouse.Event> (Mouse.buttonCapture, Mouse.CaptureEvent(Mouse.Released))

type MouseMoveConverter () =
    inherit EventArgsConverter<MouseEventArgs, Mouse.Event> (Mouse.move, Mouse.CaptureEvent(Mouse.Released))

type MainViewModel () as me =
    inherit EventViewModelBase<Mouse.Event> ()

    let lines = ObservableCollection<Line> ()

    let mouseCommand = me.Factory.EventValueCommand ()

    let handleMouseEvent =
        function
        | Mouse.MoveEvent(Mouse.Capture.Captured, startPoint), Mouse.MoveEvent(_, endPoint) ->
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
