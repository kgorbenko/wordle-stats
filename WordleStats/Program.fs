open System
open System.Diagnostics
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Hosting
open Microsoft.AspNetCore.Hosting

type ApplicationStatus = {
    Name: string
    Version: string
    Environment: string
}

let getVersion () =
    FileVersionInfo
        .GetVersionInfo(Environment.GetCommandLineArgs()[0])
        .FileVersion

[<EntryPoint>]
let main args =
    let builder = WebApplication.CreateBuilder(args)
    let app = builder.Build()

    app.MapGet("/status", Func<IWebHostEnvironment, ApplicationStatus>(
        fun (webHostEnvironment: IWebHostEnvironment) ->
            {
                Name = webHostEnvironment.ApplicationName
                Version = getVersion ()
                Environment = webHostEnvironment.EnvironmentName
            }
        )
    ) |> ignore

    app.Run()

    0