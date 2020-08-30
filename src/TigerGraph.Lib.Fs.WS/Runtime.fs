namespace TigerGraph

open WebSharper
open WebSharper.JavaScript

[<JavaScript>]
type JSConLogger() = 
    inherit Logger()
    override x.Info(mt, args) = Console.Info(mt, args)
    override x.Debug(mt, args) = Console.Log(mt + " DEBUG: ", args)
    override x.Error(mt, args) = Console.Error(mt, args)
    override x.Error(ex, mt, args) = Console.Error(mt, args)
    override x.Begin(mt, args) = upcast(new JSConLoggerOp(x))

and [<JavaScript>] JSConLoggerOp(l:JSConLogger) =
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

[<JavaScript>]
type Runtime() = 
    inherit Base.Runtime()
    static do Base.Runtime.SetLogger(new JSConLogger())

[<AutoOpen; JavaScript>]
module Runtime =
    
    let jserror = JQuery.JQuery.Error 
    
    let info = Console.Info
        
    let error = Console.Error
    
    let debug (loc:string) t = info <| sprintf "DEBUG: %s: %A" (loc.ToUpper()) t

