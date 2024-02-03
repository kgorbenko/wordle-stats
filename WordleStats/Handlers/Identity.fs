module WordleStats.Handlers.Identity

open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Routing
open Microsoft.Extensions.Options
open System
open System.Threading
open System.Threading.Tasks

open FluentValidation

open WordleStats
open WordleStats.Database
open WordleStats.DataAccess.UsersStorage

type RegisterRequest = {
    Token: string
    UserName: string
    Password: string
}

type RegisterRequestFunc =
    Func<
        IOptions<WordleStats.DynamoDbConfiguration>,
        RegisterRequest,
        CancellationToken,
        Task<IResult>
    >

module private Validators =

    type RegisterRequestValidator() =
        inherit AbstractValidator<RegisterRequest>()

        do
            base.RuleFor(fun x -> x.Token).NotEmpty() |> ignore
            base.RuleFor(fun x -> x.UserName).NotEmpty().MaximumLength(32) |> ignore
            base.RuleFor(fun x -> x.Password).NotEmpty().MinimumLength(8) |> ignore

    let private registerRequestValidator = RegisterRequestValidator()

    let validateRegisterRequest (request: RegisterRequest) =
        registerRequestValidator.Validate request

let private registerAsync
    (dynamoDbConfiguration: IOptions<WordleStats.DynamoDbConfiguration>)
    (request: RegisterRequest)
    (cancellationToken: CancellationToken)
    : Task<IResult> =
    task {
        let validationResult = Validators.validateRegisterRequest request

        if not validationResult.IsValid then
            return Results.ValidationProblem(validationResult.ToDictionary())
        else
            let configuration = makeConfiguration dynamoDbConfiguration.Value

            let! userByToken =
                findUserBySpecificationAsync (ByToken request.Token) cancellationToken
                |> withDbClientAsync configuration

            if userByToken.IsNone then
                return Results.BadRequest()
            else
                let! hashedPassword = PasswordHash.generateAsync request.Password

                let user = {
                    Token = userByToken.Value.Token
                    Name = Some request.UserName
                    Password = Some {
                        Hash = hashedPassword.Hash
                        Salt = hashedPassword.Salt
                    }
                }
                do!
                    updateUserAsync user cancellationToken
                    |> withDbClientAsync configuration

                return Results.Ok(userByToken.Value.Token)
    }

let mapIdentityApi (routeBuilder: IEndpointRouteBuilder) =
    routeBuilder.MapPost("/register", RegisterRequestFunc(registerAsync))