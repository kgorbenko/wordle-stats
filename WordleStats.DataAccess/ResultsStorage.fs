module WordleStats.DataAccess.ResultsStorage

open System
open System.Globalization
open System.Threading
open System.Threading.Tasks

open Amazon.DynamoDBv2
open Amazon.DynamoDBv2.Model

open WordleStats.Common.Utils
open WordleStats.DataAccess.DynamoDb

type Result = {
    User: string
    Date: DateOnly
    Wordle: int option
    Worldle: int option
    Waffle: int option
}

type ByUserSearchSpecification = {
    User: string
    DateTo: DateOnly
    DateFrom: DateOnly
}

type ByDateSearchSpecification = {
    Date: DateOnly
    User: string option
}

[<RequireQualifiedAccess>]
module ResultsSchema =
    let tableName = "WordleStats.Results"

    let userAttributeName = "User"
    let dateAttributeName = "Date"
    let wordleAttributeName = "Wordle"
    let worldleAttributeName = "Worldle"
    let waffleAttributeName = "Waffle"

    let dateIndexName = "DateIndex"

    let dateFormat = "yyyy-MM-dd"

    let allAttributes = Set [
        userAttributeName
        dateAttributeName
        wordleAttributeName
        worldleAttributeName
        waffleAttributeName
    ]

let private formatDate (format: string) (dateTime: DateOnly) =
    dateTime.ToString(format, CultureInfo.InvariantCulture)

let private parseDate (format: string) (value: string) =
    DateOnly.ParseExact(value, format, CultureInfo.InvariantCulture)

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
        ResultsSchema.dateAttributeName, result.Date |> formatDate ResultsSchema.dateFormat |> getStringAttributeValue

        if result.Wordle.IsSome then
            ResultsSchema.wordleAttributeName, getNumberAttributeValue result.Wordle.Value

        if result.Worldle.IsSome then
            ResultsSchema.worldleAttributeName, getNumberAttributeValue result.Worldle.Value

        if result.Waffle.IsSome then
            ResultsSchema.waffleAttributeName, getNumberAttributeValue result.Waffle.Value
    ]

let rec putResultAsync
    (result: Result)
    (cancellationToken: CancellationToken)
    (client: AmazonDynamoDBClient)
    : unit Task =
    task {
        let itemToInsert = attributeValuesFromResult result

        let request = putItemRequest ResultsSchema.tableName itemToInsert

        let! response = client.PutItemAsync(request, cancellationToken)

        response |> ensureSuccessStatusCode (nameof putResultAsync)

        return ()
    }

let rec findResultsByUserSpecificationAsync
    (specification: ByUserSearchSpecification)
    (cancellationToken: CancellationToken)
    (client: AmazonDynamoDBClient)
    : Result list Task =
    task {
        let keyCondition = "#User = :user and #Date between :dateFrom and :dateTo"
        let keyAttributes = [
            "#User", ResultsSchema.userAttributeName
            "#Date", ResultsSchema.dateAttributeName
        ]
        let keyValues = [
            ":user", specification.User |> getStringAttributeValue
            ":dateFrom", specification.DateFrom |> formatDate ResultsSchema.dateFormat |> getStringAttributeValue
            ":dateTo", specification.DateTo |> formatDate ResultsSchema.dateFormat |> getStringAttributeValue
        ]

        let request = QueryRequest(ResultsSchema.tableName)
        request.KeyConditionExpression <- keyCondition
        request.ExpressionAttributeNames <- keyAttributes |> Map |> mapToDict
        request.ExpressionAttributeValues <- keyValues |> Map |> mapToDict

        let! response = client.QueryAsync(request, cancellationToken)

        response |> ensureSuccessStatusCode (nameof findResultsByUserSpecificationAsync)

        return response.Items
            |> Seq.map dictToMap
            |> Seq.map attributesValuesToResult
            |> List.ofSeq
    }

let rec findResultsByDateSpecificationAsync
    (specification: ByDateSearchSpecification)
    (cancellationToken: CancellationToken)
    (client: AmazonDynamoDBClient)
    : Result list Task =
    task {
        let userCondition, userAttribute, userValue =
            match specification.User with
            | None -> None, None, None
            | Some user ->
                let userAttribute = "#User", ResultsSchema.userAttributeName
                let userValue = ":user", getStringAttributeValue user

                Some "#User = :user", Some userAttribute, Some userValue

        let request = QueryRequest(ResultsSchema.tableName)
        request.IndexName <- ResultsSchema.dateIndexName
        request.KeyConditionExpression <- [
            "#Date = :date"

            if userCondition.IsSome then
                userCondition.Value
        ] |> String.concat " and "

        request.ExpressionAttributeNames <- [
            "#Date", ResultsSchema.dateAttributeName

            if userAttribute.IsSome then
                userAttribute.Value
        ] |> Map |> mapToDict

        request.ExpressionAttributeValues <- [
            ":date", specification.Date |> formatDate ResultsSchema.dateFormat |> getStringAttributeValue

            if userValue.IsSome then
                userValue.Value
        ] |> Map |> mapToDict

        let! response = client.QueryAsync(request, cancellationToken)

        response |> ensureSuccessStatusCode (nameof findResultsByUserSpecificationAsync)

        return response.Items
            |> Seq.map dictToMap
            |> Seq.map attributesValuesToResult
            |> List.ofSeq
    }