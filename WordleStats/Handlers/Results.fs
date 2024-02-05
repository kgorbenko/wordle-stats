module WordleStats.Handlers.Results

open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Routing
open Microsoft.Extensions.Options
open System
open System.Threading
open System.Threading.Tasks

open FluentValidation

open WordleStats
open WordleStats.DataAccess.ResultsStorage
open WordleStats.DataAccess.UsersStorage
open WordleStats.Database
open WordleStats.Results

[<Literal>]
let UnattemptedValue = -2

[<Literal>]
let FailedValue = -1

type WordleResult =
    | Unattempted = -2
    | Failed = -1
    | Scored1 = 1
    | Scored2 = 2
    | Scored3 = 3
    | Scored4 = 4
    | Scored5 = 5
    | Scored6 = 6

type WorldleResult =
    | Unattempted = -2
    | Failed = -1
    | Scored1 = 1
    | Scored2 = 2
    | Scored3 = 3
    | Scored4 = 4
    | Scored5 = 5
    | Scored6 = 6

type WaffleResult =
    | Unattempted = -2
    | Failed = -1
    | Scored0 = 0
    | Scored1 = 1
    | Scored2 = 2
    | Scored3 = 3
    | Scored4 = 4
    | Scored5 = 5

type PostResultRequest = {
    Token: string
    Date: DateOnly
    Wordle: WordleResult Nullable
    Worldle: WorldleResult Nullable
    Waffle: WaffleResult Nullable
}

type PostResultResponse = {
    TotalScore: int
}

type GetResultsByDateRequest = {
    Token: string
    Date: DateOnly
    UserFilter: string
}

type ByDateResult = {
    User: string
    Wordle: WordleResult
    WorldleResult: WorldleResult
    WaffleResult: WaffleResult
}

type GetResultsResponse = {
    Results: ByDateResult list
}

type PostResultRequestFunc =
    Func<
        IOptions<WordleStats.DynamoDbConfiguration>,
        PostResultRequest,
        CancellationToken,
        Task<IResult>
    >

type GetResultsByDateRequestFunc =
    Func<
        IOptions<WordleStats.DynamoDbConfiguration>,
        string,
        DateOnly,
        string,
        CancellationToken,
        Task<IResult>
    >

module private Validators =

    type PostResultRequestValidator() =
        inherit AbstractValidator<PostResultRequest>()

        do
            base.RuleFor(fun x -> x.Token).NotEmpty() |> ignore
            base.RuleFor(fun x -> x.Date).NotEmpty() |> ignore
            base.RuleFor(fun x -> x.Wordle).IsInEnum() |> ignore
            base.RuleFor(fun x -> x.Worldle).IsInEnum() |> ignore
            base.RuleFor(fun x -> x.Waffle).IsInEnum() |> ignore

    type GetResultsRequestValidator() =
        inherit AbstractValidator<GetResultsByDateRequest>()

        do
            base.RuleFor(fun x -> x.Token).NotEmpty() |> ignore
            base.RuleFor(fun x -> x.Date).NotEmpty() |> ignore

    let private postResultRequestValidator = PostResultRequestValidator()
    let private getResultsRequestValidator = GetResultsRequestValidator()

    let validatePostResultRequest (request: PostResultRequest) =
        postResultRequestValidator.Validate request

    let validateGetResultsRequest (request: GetResultsByDateRequest) =
        getResultsRequestValidator.Validate request

let private mapWordleResult (result: WordleResult): Results.WordleResult =
    match result with
    | WordleResult.Unattempted -> Wordle.unattempted ()
    | WordleResult.Failed -> Wordle.failed ()
    | WordleResult.Scored1 -> Wordle.solved 1
    | WordleResult.Scored2 -> Wordle.solved 2
    | WordleResult.Scored3 -> Wordle.solved 3
    | WordleResult.Scored4 -> Wordle.solved 4
    | WordleResult.Scored5 -> Wordle.solved 5
    | WordleResult.Scored6 -> Wordle.solved 6
    | _ -> failwith $"Unexpected Wordle result value: {result}"

let private mapWorldleResult (result: WorldleResult): Results.WorldleResult =
    match result with
    | WorldleResult.Unattempted -> Worldle.unattempted ()
    | WorldleResult.Failed -> Worldle.failed ()
    | WorldleResult.Scored1 -> Worldle.solved 1
    | WorldleResult.Scored2 -> Worldle.solved 2
    | WorldleResult.Scored3 -> Worldle.solved 3
    | WorldleResult.Scored4 -> Worldle.solved 4
    | WorldleResult.Scored5 -> Worldle.solved 5
    | WorldleResult.Scored6 -> Worldle.solved 6
    | _ -> failwith $"Unexpected Worldle result value: {result}"

let private mapWaffleResult (result: WaffleResult): Results.WaffleResult =
    match result with
    | WaffleResult.Unattempted -> Waffle.unattempted ()
    | WaffleResult.Failed -> Waffle.failed ()
    | WaffleResult.Scored0 -> Waffle.solved 0
    | WaffleResult.Scored1 -> Waffle.solved 1
    | WaffleResult.Scored2 -> Waffle.solved 2
    | WaffleResult.Scored3 -> Waffle.solved 3
    | WaffleResult.Scored4 -> Waffle.solved 4
    | WaffleResult.Scored5 -> Waffle.solved 5
    | _ -> failwith $"Unexpected Waffle result value: {result}"

let private toPersistentWordleScore (wordleResult: Results.WordleResult) =
    match wordleResult with
    | Results.WordleResult.Unattempted -> UnattemptedValue
    | Results.WordleResult.Failed -> FailedValue
    | Results.WordleResult.Solved score -> score

let private toPersistentWorldleScore (wordleResult: Results.WorldleResult) =
    match wordleResult with
    | Results.WorldleResult.Unattempted -> UnattemptedValue
    | Results.WorldleResult.Failed -> FailedValue
    | Results.WorldleResult.Solved score -> score

let private toPersistentWaffleScore (wordleResult: Results.WaffleResult) =
    match wordleResult with
    | Results.WaffleResult.Unattempted -> UnattemptedValue
    | Results.WaffleResult.Failed -> FailedValue
    | Results.WaffleResult.Solved score -> score

module ApiMapping =

    let toApiWordleResult (score: int option): WordleResult =
        match score with
        | None
        | Some UnattemptedValue -> WordleResult.Unattempted
        | Some FailedValue -> WordleResult.Failed
        | Some 1 -> WordleResult.Scored1
        | Some 2 -> WordleResult.Scored2
        | Some 3 -> WordleResult.Scored3
        | Some 4 -> WordleResult.Scored4
        | Some 5 -> WordleResult.Scored5
        | Some 6 -> WordleResult.Scored6
        | _ -> failwith $"Unexpected Wordle score: {score}"

    let toApiWorldleResult (score: int option): WorldleResult =
        match score with
        | None
        | Some UnattemptedValue -> WorldleResult.Unattempted
        | Some FailedValue -> WorldleResult.Failed
        | Some 1 -> WorldleResult.Scored1
        | Some 2 -> WorldleResult.Scored2
        | Some 3 -> WorldleResult.Scored3
        | Some 4 -> WorldleResult.Scored4
        | Some 5 -> WorldleResult.Scored5
        | Some 6 -> WorldleResult.Scored6
        | _ -> failwith $"Unexpected Worldle score: {score}"

    let toApiWaffleResult (score: int option): WaffleResult =
        match score with
        | None
        | Some UnattemptedValue -> WaffleResult.Unattempted
        | Some FailedValue -> WaffleResult.Failed
        | Some 0 -> WaffleResult.Scored0
        | Some 1 -> WaffleResult.Scored1
        | Some 2 -> WaffleResult.Scored2
        | Some 3 -> WaffleResult.Scored3
        | Some 4 -> WaffleResult.Scored4
        | Some 5 -> WaffleResult.Scored5
        | _ -> failwith $"Unexpected Worldle score: {score}"

    let toApiByDateResult (result: Result): ByDateResult =
        {
            User = result.User
            Wordle = toApiWordleResult result.Wordle
            WorldleResult = toApiWorldleResult result.Worldle
            WaffleResult = toApiWaffleResult result.Waffle
        }

let private postResultAsync
    (dynamoDbConfiguration: IOptions<WordleStats.DynamoDbConfiguration>)
    (request: PostResultRequest)
    (cancellationToken: CancellationToken)
    : Task<IResult> =
    task {
        let validationResult = Validators.validatePostResultRequest request

        if not validationResult.IsValid then
            return Results.ValidationProblem(validationResult.ToDictionary())
        else
            let configuration = makeConfiguration dynamoDbConfiguration.Value

            let! userByToken =
                findUserBySpecificationAsync (ByToken request.Token) cancellationToken
                |> withDbClientAsync configuration

            if userByToken.IsNone || userByToken.Value.Name.IsNone || userByToken.Value.Password.IsNone then
                return Results.BadRequest()
            else
                let wordleScore = request.Wordle |> Option.ofNullable |> Option.map mapWordleResult |> Option.defaultValue (Wordle.unattempted ())
                let worldleScore = request.Worldle |> Option.ofNullable |> Option.map mapWorldleResult |> Option.defaultValue (Worldle.unattempted ())
                let waffleScore = request.Waffle |> Option.ofNullable |> Option.map mapWaffleResult |> Option.defaultValue (Waffle.unattempted ())

                let stripIfUnattempted value =
                    match value with
                    | UnattemptedValue -> None
                    | _ -> Some value

                let resultToInsert: Result = {
                    User = userByToken.Value.Name.Value
                    Date = request.Date
                    Wordle = wordleScore |> toPersistentWordleScore |> stripIfUnattempted
                    Worldle = worldleScore |> toPersistentWorldleScore |> stripIfUnattempted
                    Waffle = waffleScore |> toPersistentWaffleScore |> stripIfUnattempted
                }

                do!
                    putResultAsync resultToInsert cancellationToken
                    |> withDbClientAsync configuration

                let response = { TotalScore = calculateTotalScore wordleScore worldleScore waffleScore }

                return Results.Ok(response)
    }

let private getResultsByDateAsync
    (dynamoDbConfiguration: IOptions<WordleStats.DynamoDbConfiguration>)
    (token: string)
    (date: DateOnly)
    (userFilter: string)
    (cancellationToken: CancellationToken)
    : Task<IResult> =
    task {
        let request: GetResultsByDateRequest = { Token = token; Date = date; UserFilter = userFilter }
        let validationResult = Validators.validateGetResultsRequest request

        if not validationResult.IsValid then
            return Results.ValidationProblem(validationResult.ToDictionary())
        else
            let configuration = makeConfiguration dynamoDbConfiguration.Value

            let! userByToken =
                findUserBySpecificationAsync (ByToken request.Token) cancellationToken
                |> withDbClientAsync configuration

            if userByToken.IsNone || userByToken.Value.Name.IsNone || userByToken.Value.Password.IsNone then
                return Results.BadRequest()
            else
                let byDateSpecification: ByDateSearchSpecification = {
                    Date = request.Date
                    User = request.UserFilter |> Option.ofObj
                }

                let! results =
                    findResultsByDateSpecificationAsync byDateSpecification cancellationToken
                    |> withDbClientAsync configuration

                let byDateResults =
                    results |> Seq.map ApiMapping.toApiByDateResult |> List.ofSeq

                return Results.Ok({ Results = byDateResults })
    }

let mapResultsApi (routeBuilder: IEndpointRouteBuilder) =
    routeBuilder.MapGet("/by-date", GetResultsByDateRequestFunc(
        fun dynamoDbConfiguration
            token
            date
            userFilter
            cancellationToken -> getResultsByDateAsync dynamoDbConfiguration token date userFilter cancellationToken
        )
    ) |> ignore

    routeBuilder.MapPost("", PostResultRequestFunc(postResultAsync)) |> ignore

    ()