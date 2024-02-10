open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open System
open System.Diagnostics

open WordleStats
open WordleStats.Configuration
open WordleStats.Handlers.Identity
open WordleStats.Handlers.Results

let allowSpaProxyPolicy = "AllowSPAProxy"

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

    builder.Services.AddCors(fun options ->
        options.AddPolicy(allowSpaProxyPolicy, (fun x ->
            x.WithOrigins("https://localhost:5173")
                .AllowAnyHeader()
                .AllowAnyMethod() |> ignore
            )
        )
    ) |> ignore

    let app = builder.Build()

    app.UseDefaultFiles() |> ignore
    app.UseStaticFiles() |> ignore

    app.UseHttpsRedirection() |> ignore

    app.MapGroup("/api")
    |> (fun builder ->

        builder.MapGet("/status", Func<IWebHostEnvironment, ApplicationStatus>(
            fun (webHostEnvironment: IWebHostEnvironment) ->
                {
                    Name = webHostEnvironment.ApplicationName
                    Version = getVersion ()
                    Environment = webHostEnvironment.EnvironmentName
                }
            )
        ).WithTags("Default") |> ignore

        builder.MapGroup("/auth").WithTags("Auth") |> mapIdentityApi

        builder.MapGroup("/results").WithTags("Results") |> mapResultsApi
    )

    app.MapFallbackToFile("/index.html") |> ignore

    if app.Environment.IsDevelopment() then
        app.UseSwagger() |> ignore
        app.UseSwaggerUI() |> ignore
        app.UseCors("AllowSPAProxy") |> ignore

    app.Run()

    0