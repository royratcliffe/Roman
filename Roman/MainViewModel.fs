namespace Roman

open ViewModule
open ViewModule.FSharp
open System.Collections.ObjectModel
open System.Windows.Input
open FsXaml
open System.Windows

type Point = { X: float; Y: float }
type Line = { Start: Point; End: Point }

type MouseCapture = Captured | Released

type MouseEvent =
| MouseCaptureEvent of capture: MouseCapture
| MouseMoveEvent of capture: MouseCapture * point: Point

module internal Mouse =

    let buttonCapture (args: MouseButtonEventArgs) =
        match args.ButtonState with
        | MouseButtonState.Pressed ->
            let source = args.OriginalSource :?> UIElement
            source.CaptureMouse ()
            |> ignore
            MouseCaptureEvent(Captured)
        | _ ->
            let source = args.OriginalSource :?> UIElement
            source.ReleaseMouseCapture ()
            MouseCaptureEvent(Released)

    let move (args: MouseEventArgs) =
        let source = args.OriginalSource :?> IInputElement
        let captured =
            if System.Object.ReferenceEquals (Mouse.Captured, source)
            then MouseCapture.Captured
            else MouseCapture.Released
        let point = args.GetPosition (source)
        MouseMoveEvent(captured, { X = point.X; Y = point.Y })

type MouseButtonCaptureConverter () =
    inherit EventArgsConverter<MouseButtonEventArgs, MouseEvent> (Mouse.buttonCapture, MouseCaptureEvent(Released))

type MouseMoveConverter () =
    inherit EventArgsConverter<MouseEventArgs, MouseEvent> (Mouse.move, MouseCaptureEvent(Released))

type MainViewModel () as me =
    inherit EventViewModelBase<MouseEvent> ()

    let lines = ObservableCollection<Line> ()

    let mouseCommand = me.Factory.EventValueCommand ()

    let handleMouseEvent =
        function
        | MouseMoveEvent(MouseCapture.Captured, startPoint), MouseMoveEvent(_, endPoint) ->
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
