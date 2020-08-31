namespace TigerGraph

open WebSharper
open WebSharper.JavaScript

[<AutoOpen; JavaScript>]
module ClientUtils =
    
    let jserror = JQuery.JQuery.Error 
    
    let info = Console.Info
        
    let error = Console.Error
    
    let debug (loc:string) t = info <| sprintf "DEBUG: %s: %A" (loc.ToUpper()) t

