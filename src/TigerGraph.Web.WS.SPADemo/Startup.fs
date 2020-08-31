namespace TigerGraph.Web.WS.SPADemo

open System

open Microsoft.AspNetCore
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting

open WebSharper.AspNetCore
open Serilog

type Startup() =

    member this.ConfigureServices(services: IServiceCollection) =
        ()

    member this.Configure(app: IApplicationBuilder, env: IWebHostEnvironment) =
        if env.IsDevelopment() then app.UseDeveloperExceptionPage() |> ignore

        app.UseDefaultFiles()
            .UseStaticFiles()
            .UseWebSharper(fun builder -> builder.UseSitelets(false) |> ignore)
            .Run(fun context ->
                context.Response.StatusCode <- 404
                context.Response.WriteAsync("Page not found"))

module Program =
    [<EntryPoint>]
    let main args =
        let config = new LoggerConfiguration()
        Log.Logger <- config.MinimumLevel.Debug().Enrich.FromLogContext().WriteTo.Console().CreateLogger()
        do WebHost
            .CreateDefaultBuilder(args)
            .UseSerilog()
            .UseStartup<Startup>()
            .Build()
            .Run()
        do Log.CloseAndFlush()
        0
