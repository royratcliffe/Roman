open System

type App = FsXaml.XAML<"App.xaml">

let [<EntryPoint; STAThread>] main _argv = (App ()).Run ()
