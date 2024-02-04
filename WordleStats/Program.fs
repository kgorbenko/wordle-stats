open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open System
open System.Diagnostics

open WordleStats
open WordleStats.Configuration
open WordleStats.Handlers.Identity
open WordleStats.Handlers.Results

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
    ).WithTags("Default") |> ignore

    app.MapGroup("auth").WithTags("Auth") |> mapIdentityApi

    app.MapGroup("results").WithTags("Results") |> mapResultsApi

    app.UseSwagger() |> ignore
    app.UseSwaggerUI() |> ignore

    app.Run()

    0