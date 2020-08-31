namespace TigerGraph

open System
open WebSharper
open WebSharper.JavaScript
open WebSharper.JQuery
open WebSharper.UI
open WebSharper.UI.Client
open WebSharper.UI.Templating

[<JavaScript>]
module Client =
    // The templates are loaded from the DOM, so you just can edit index.html
    // and refresh your browser, no need to recompile unless you add or remove holes.
    type IndexTemplate = Template<"wwwroot/index.html", ClientLoad.FromDocument>

    let ajaxCall reqType url contentTy headers data =
            let settings = JQuery.AjaxSettings(
                            Url = "http://localhost/api/" + url,
                            Type = reqType,
                            DataType = JQuery.DataType.Text,
                            Success = Action<obj, string, JqXHR>(fun a b c -> ()),
                            Error = Action<JqXHR,string,string>(fun a b c -> ())
                            )
            do headers   |> Option.iter (fun h -> settings.Headers <- h)
            contentTy |> Option.iter (fun c -> settings.ContentType <- c)
            data      |> Option.iter (fun d -> settings.Data <- d)
            
            //JQuery.Ajax(settings) |> ignore

    [<SPAEntryPoint>]
    let Main () =
        Doc.Empty
        //JQuery.Ajax(settings)
        //JQuery.AjaxPrefilter()
        //Doc.RunById "main"
