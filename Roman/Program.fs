open System
open FsXaml

type App = XAML<"App.xaml">

let [<EntryPoint; STAThread>] main _argv =
    Wpf.installBlendSupport ()
    (App ()).Run ()
