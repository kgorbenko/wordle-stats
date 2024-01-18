module WordleStats.DataAccess.Tests.DatabaseLayer

open System.Collections.Generic
open System.Net
open System.Threading.Tasks

open Amazon.DynamoDBv2
open Amazon.DynamoDBv2.Model

open WordleStats.Common.Utils
open WordleStats.DataAccess.ResultsStorage
open WordleStats.DataAccess.Tests.DatabaseSnapshot

let private toResult (attributes: Map<string, AttributeValue>): Result =
    let readString (attributeName: string) =
        attributes |> Map.find attributeName |> _.S

    let readOptionString (attributeName: string) =
        attributes |> Map.tryFind attributeName |> Option.map _.N

    {
        User = readString ResultsSchema.userAttributeName
        Date = readString ResultsSchema.dateAttributeName
        Wordle = readOptionString ResultsSchema.wordleAttributeName
        Worldle = readOptionString ResultsSchema.worldleAttributeName
        Waffle = readOptionString ResultsSchema.waffleAttributeName
    }

let getAllResultsAsync (client: AmazonDynamoDBClient): Task<Result list> =
    task {
        let! response = client.ScanAsync(ResultsSchema.tableName, List<string>(ResultsSchema.allAttributes))

        if response.HttpStatusCode <> HttpStatusCode.OK then
            failwith $"Unable to get results. HTTP status code: {response.HttpStatusCode}"

        return response.Items
            |> Seq.map (fun x -> x |> Seq.map (fun y -> y.Key, y.Value) |> Map.ofSeq)
            |> Seq.map toResult
            |> List.ofSeq
    }

let private getResultsTableKeysToDelete (results: Result list): Map<string, AttributeValue> list =
    results
    |> Seq.map (fun x -> Map [
        ResultsSchema.userAttributeName, x.User |> getStringAttributeValue
        ResultsSchema.dateAttributeName, x.Date |> getStringAttributeValue
    ])
    |> List.ofSeq

let clearAllTablesAsync (client: AmazonDynamoDBClient): Task<unit> =
    task {
        let! allResults = getAllResultsAsync client

        let batchWriteParameters =
            [ ResultsSchema.tableName, getResultsTableKeysToDelete allResults ]
            |> Seq.map (fun x ->
                let writeDeleteRequests =
                    snd x
                    |> Seq.map mapToDict
                    |> Seq.map (DeleteRequest >> WriteRequest)
                    |> List

                fst x, writeDeleteRequests
            )
            |> Map.ofSeq
            |> mapToDict

        let! response = client.BatchWriteItemAsync(batchWriteParameters)

        if response.HttpStatusCode <> HttpStatusCode.OK then
            failwith $"Unable to delete all results. HTTP status code: {response.HttpStatusCode}"
    }