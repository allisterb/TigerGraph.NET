namespace TigerGraph

open WebSharper
open WebSharper.JavaScript

type JSConLogger() = 
    inherit Logger()
    override x.Info(mt, args) = Console.Info(mt, args)
    override x.Debug(mt, args) = Console.Log(mt + " DEBUG: ", args)
    override x.Error(mt, args) = Console.Error(mt, args)
    override x.Error(ex, mt, args) = Console.Error(mt, args)
    override x.Begin(mt, args) = upcast(new JSConLoggerOp(x))

and JSConLoggerOp(l:JSConLogger) =
    inherit Logger.Op(l)
    let mutable completed = false
    let mutable cancelled = false
    override __.Complete() = 
        l.Info("Completed.")
        completed <- true
    override __.Cancel() = 
        l.Info("Cancelled.")
        completed <- true
    override x.Dispose() = if not(completed || cancelled) then l.Error("Cancelled.")

type Runtime() = 
    inherit Base.Runtime()
    

[<AutoOpen>]
module Runtime =
    do Runtime.SetLogger(new JSConLogger())
    
    let info = Runtime.Info

    let debug = Runtime.Debug
    
    let err = Runtime.Error
    
    let beginOp = Runtime.Begin

    let infof mt args = Runtime.Info(mt, List.toArray args)

    let debugf mt args = Runtime.Debug(mt, List.toArray args)

    let errf mt args = Runtime.Error(mt, List.toArray args)

    let errex mt args = Runtime.Error(mt, List.toArray args)

