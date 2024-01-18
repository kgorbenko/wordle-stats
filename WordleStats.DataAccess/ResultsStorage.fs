module WordleStats.DataAccess.ResultsStorage

open System
open System.Globalization
open System.Net
open System.Threading
open System.Threading.Tasks

open Amazon.DynamoDBv2
open Amazon.DynamoDBv2.Model

open WordleStats.Common.Utils

type Result = {
    User: string
    Date: DateTimeOffset
    Wordle: int option
    Worldle: int option
    Waffle: int option
}

[<RequireQualifiedAccess>]
module ResultsSchema =
    let tableName = "wordle"

    let userAttributeName = "User"
    let dateAttributeName = "Date"
    let wordleAttributeName = "Wordle"
    let worldleAttributeName = "Worldle"
    let waffleAttributeName = "Waffle"

    let dateFormat = "yyyy-MM-dd"

    let allAttributes = Set [
        userAttributeName
        dateAttributeName
        wordleAttributeName
        worldleAttributeName
        waffleAttributeName
    ]

let getStringAttributeValue (value: string) =
    let attributeValue = AttributeValue()
    attributeValue.S <- value
    attributeValue

let getNumberAttributeValue (value: int) =
    let attributeValue = AttributeValue()
    attributeValue.N <- value.ToString(CultureInfo.InvariantCulture)
    attributeValue

let putItemRequest (tableName: string) (itemToPut: Map<string, AttributeValue>) =
    let request = PutItemRequest()
    request.TableName <- tableName
    request.Item <- itemToPut |> mapToDict
    request

let insertResultAsync
    (result: Result)
    (cancellationToken: CancellationToken)
    (client: AmazonDynamoDBClient)
    : unit Task =
    task {
        let itemToInsert = Map<string, AttributeValue> [
            ResultsSchema.userAttributeName, getStringAttributeValue result.User
            ResultsSchema.dateAttributeName, getStringAttributeValue (result.Date.Date.ToString(ResultsSchema.dateFormat, CultureInfo.InvariantCulture))

            if result.Wordle.IsSome then
                ResultsSchema.wordleAttributeName, getNumberAttributeValue result.Wordle.Value

            if result.Worldle.IsSome then
                ResultsSchema.worldleAttributeName, getNumberAttributeValue result.Worldle.Value

            if result.Waffle.IsSome then
                ResultsSchema.waffleAttributeName, getNumberAttributeValue result.Waffle.Value
        ]

        let request = putItemRequest ResultsSchema.tableName itemToInsert

        let! response = client.PutItemAsync(request, cancellationToken)

        if response.HttpStatusCode <> HttpStatusCode.OK then
            failwith (sprintf "Unable to put item %A into table %s" itemToInsert ResultsSchema.tableName)

        return ()
    }