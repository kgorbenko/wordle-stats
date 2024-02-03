open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.DependencyInjection
open System
open System.Diagnostics

open WordleStats
open WordleStats.Configuration
open WordleStats.Handlers.Identity

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

    builder.Services
        .AddOptionsWithValidateOnStart<DynamoDbConfiguration>()
        .Bind(builder.Configuration.GetSection(dynamoDbConfigurationSectionName))
    |> ignore

    builder.Services.AddEndpointsApiExplorer() |> ignore
    builder.Services.AddSwaggerGen() |> ignore

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

    app.MapGroup("auth") |> mapIdentityApi |> ignore

    app.UseSwagger() |> ignore
    app.UseSwaggerUI() |> ignore

    app.Run()

    0