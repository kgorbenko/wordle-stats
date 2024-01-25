﻿module WordleStats.DataAccess.ResultsStorage

open System
open System.Globalization
open System.Net
open System.Threading
open System.Threading.Tasks

open Amazon.DynamoDBv2
open Amazon.DynamoDBv2.Model

open WordleStats.DataAccess.DynamoDb

type Result = {
    User: string
    Date: DateTimeOffset
    Wordle: int option
    Worldle: int option
    Waffle: int option
}

[<RequireQualifiedAccess>]
module ResultsSchema =
    let tableName = "WordleStats.Results"

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

let private formatDate (dateTime: DateTimeOffset) (format: string) =
    dateTime.ToString(format, CultureInfo.InvariantCulture)

let private parseDate (format: string) (value: string) =
    DateTimeOffset.ParseExact(value, format, CultureInfo.InvariantCulture)

let private attributesValuesToResult (attributes: Map<string, AttributeValue>): Result =
    {
        User = readStringAttribute attributes ResultsSchema.userAttributeName
        Date = readStringAttribute attributes ResultsSchema.dateAttributeName |> parseDate ResultsSchema.dateFormat
        Wordle = readOptionNumberAttribute attributes ResultsSchema.wordleAttributeName
        Worldle = readOptionNumberAttribute attributes ResultsSchema.worldleAttributeName
        Waffle = readOptionNumberAttribute attributes ResultsSchema.waffleAttributeName
    }

let private attributeValuesFromResult (result: Result): Map<string, AttributeValue> =
    Map [
        ResultsSchema.userAttributeName, getStringAttributeValue result.User
        ResultsSchema.dateAttributeName, getStringAttributeValue (formatDate result.Date ResultsSchema.dateFormat)

        if result.Wordle.IsSome then
            ResultsSchema.wordleAttributeName, getNumberAttributeValue result.Wordle.Value

        if result.Worldle.IsSome then
            ResultsSchema.worldleAttributeName, getNumberAttributeValue result.Worldle.Value

        if result.Waffle.IsSome then
            ResultsSchema.waffleAttributeName, getNumberAttributeValue result.Waffle.Value
    ]

let insertResultAsync
    (result: Result)
    (cancellationToken: CancellationToken)
    (client: AmazonDynamoDBClient)
    : unit Task =
    task {
        let itemToInsert = attributeValuesFromResult result

        let request = putItemRequest ResultsSchema.tableName itemToInsert

        let! response = client.PutItemAsync(request, cancellationToken)

        if response.HttpStatusCode <> HttpStatusCode.OK then
            failwith (sprintf "Unable to put item %A into table %s" itemToInsert ResultsSchema.tableName)

        return ()
    }