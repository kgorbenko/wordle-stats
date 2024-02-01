module WordleStats.DataAccess.Tests.DatabaseLayer

open System.Collections.Generic
open System.Net
open System.Threading.Tasks

open Amazon.DynamoDBv2
open Amazon.DynamoDBv2.Model

open WordleStats.Common.Utils
open WordleStats.DataAccess.ResultsStorage
open WordleStats.DataAccess.UsersStorage
open WordleStats.DataAccess.Tests.DatabaseSnapshot

module Schemas =

    let createUsersTableAsync (client: AmazonDynamoDBClient): Task<unit> =
        task {
            let keySchemas = [
                KeySchemaElement(UsersSchema.nameAttributeName, KeyType.HASH)
            ]

            let attributes = [
                AttributeDefinition(UsersSchema.nameAttributeName, ScalarAttributeType.S)
            ]

            let request = CreateTableRequest(UsersSchema.tableName, keySchemas |> List, attributes |> List, ProvisionedThroughput(3, 3))

            let! response = client.CreateTableAsync(request)

            if response.HttpStatusCode <> HttpStatusCode.OK then
                failwith $"Unable to create Users table. HTTP status code: {response.HttpStatusCode}"
        }

    let createResultsTableAsync (client: AmazonDynamoDBClient): Task<unit> =
        task {
            let getDateGlobalIndexDefinition () =
                let keySchemas = [
                    KeySchemaElement(ResultsSchema.dateAttributeName, KeyType.HASH)
                    KeySchemaElement(ResultsSchema.userAttributeName, KeyType.RANGE)
                ]

                let projection = Projection()
                projection.ProjectionType <- ProjectionType.ALL

                let index = GlobalSecondaryIndex()
                index.IndexName <- ResultsSchema.dateIndexName
                index.KeySchema <- keySchemas |> List
                index.Projection <- projection
                index.ProvisionedThroughput <- ProvisionedThroughput(3, 3)
                index


            let keySchemas = [
                KeySchemaElement(ResultsSchema.userAttributeName, KeyType.HASH)
                KeySchemaElement(ResultsSchema.dateAttributeName, KeyType.RANGE)
            ]

            let attributes = [
                AttributeDefinition(ResultsSchema.userAttributeName, ScalarAttributeType.S)
                AttributeDefinition(ResultsSchema.dateAttributeName, ScalarAttributeType.S)
            ]

            let request = CreateTableRequest(ResultsSchema.tableName, keySchemas |> List, attributes |> List, ProvisionedThroughput(3, 3))
            request.GlobalSecondaryIndexes <- [getDateGlobalIndexDefinition ()] |> List

            let! response = client.CreateTableAsync(request)

            if response.HttpStatusCode <> HttpStatusCode.OK then
                failwith $"Unable to create Users table. HTTP status code: {response.HttpStatusCode}"
        }

let private getStringAttributeValue (value: string) =
    let attribute = AttributeValue()
    attribute.S <- value
    attribute

let private getNumberAttributeValue (value: string) =
    let attribute = AttributeValue()
    attribute.N <- value
    attribute

let private readString (attributeName: string) (attributes: Map<string, AttributeValue>) =
    attributes |> Map.find attributeName |> _.S

let private readNumber (attributeName: string) (attributes: Map<string, AttributeValue>) =
    attributes |> Map.find attributeName |> _.N

let private readOptionNumber (attributeName: string) (attributes: Map<string, AttributeValue>) =
    attributes |> Map.tryFind attributeName |> Option.map _.N

let private toResult (attributes: Map<string, AttributeValue>): Result =
    {
        User = readString ResultsSchema.userAttributeName attributes
        Date = readString ResultsSchema.dateAttributeName attributes
        Wordle = readOptionNumber ResultsSchema.wordleAttributeName attributes
        Worldle = readOptionNumber ResultsSchema.worldleAttributeName attributes
        Waffle = readOptionNumber ResultsSchema.waffleAttributeName attributes
    }

let private toUser (attributes: Map<string, AttributeValue>): User =
    {
        Name = readString UsersSchema.nameAttributeName attributes
        Token = readString UsersSchema.tokenAttributeName attributes
        PinCode = readOptionNumber UsersSchema.pinCodeAttributeName attributes
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

let getAllUsersAsync (client: AmazonDynamoDBClient): Task<User list> =
    task {
        let! response = client.ScanAsync(UsersSchema.tableName, List<string>(UsersSchema.allAttributes))

        if response.HttpStatusCode <> HttpStatusCode.OK then
            failwith $"Unable to get users. HTTP status code: {response.HttpStatusCode}"

        return response.Items
            |> Seq.map (fun x -> x |> Seq.map (fun y -> y.Key, y.Value) |> Map.ofSeq)
            |> Seq.map toUser
            |> List.ofSeq
    }

let insertResultsAsync (results: Result list) (client: AmazonDynamoDBClient) =
    task {
        let batchWriteParameters =
            results
            |> Seq.map (fun x -> Map [
                ResultsSchema.userAttributeName, x.User |> getStringAttributeValue
                ResultsSchema.dateAttributeName, x.Date |> getStringAttributeValue

                if x.Wordle.IsSome then
                    ResultsSchema.wordleAttributeName, x.Wordle.Value |> getNumberAttributeValue

                if x.Worldle.IsSome then
                    ResultsSchema.worldleAttributeName, x.Worldle.Value |> getNumberAttributeValue

                if x.Waffle.IsSome then
                    ResultsSchema.waffleAttributeName, x.Waffle.Value |> getNumberAttributeValue
            ])
            |> Seq.map mapToDict
            |> Seq.map (PutRequest >> WriteRequest)
            |> (fun x -> Map [ ResultsSchema.tableName, List(x) ] |> dictToMap)
            |> mapToDict

        let! response = client.BatchWriteItemAsync(batchWriteParameters)

        if response.HttpStatusCode <> HttpStatusCode.OK then
            failwith $"Unable to insert results. HTTP status code: {response.HttpStatusCode}"
    }

let insertUsersAsync (users: User list) (client: AmazonDynamoDBClient) =
    task {
        let batchWriteParameters =
            users
            |> Seq.map (fun x -> Map [
                UsersSchema.nameAttributeName, x.Name |> getStringAttributeValue
                UsersSchema.tokenAttributeName, x.Token |> getStringAttributeValue

                if x.PinCode.IsSome then
                    UsersSchema.pinCodeAttributeName, x.PinCode.Value |> getNumberAttributeValue
            ])
            |> Seq.map mapToDict
            |> Seq.map (PutRequest >> WriteRequest)
            |> (fun x -> Map [ UsersSchema.tableName, List(x) ] |> dictToMap)
            |> mapToDict

        let! response = client.BatchWriteItemAsync(batchWriteParameters)

        if response.HttpStatusCode <> HttpStatusCode.OK then
            failwith $"Unable to insert users. HTTP status code: {response.HttpStatusCode}"
    }

let private getResultsTableKeysToDelete (results: Result list): Map<string, AttributeValue> list =
    results
    |> Seq.map (fun x -> Map [
        ResultsSchema.userAttributeName, x.User |> getStringAttributeValue
        ResultsSchema.dateAttributeName, x.Date |> getStringAttributeValue
    ])
    |> List.ofSeq

let private getUsersTableKeysToDelete (users: User list): Map<string, AttributeValue> list =
    users
    |> Seq.map (fun x -> Map [
        UsersSchema.nameAttributeName, x.Name |> getStringAttributeValue
    ])
    |> List.ofSeq

let clearAllTablesAsync (client: AmazonDynamoDBClient): Task<unit> =
    task {
        let! allUsers = getAllUsersAsync client
        let! allResults = getAllResultsAsync client

        let keysToDelete = [
            if not allUsers.IsEmpty then
                UsersSchema.tableName, getUsersTableKeysToDelete allUsers

            if not allResults.IsEmpty then
                ResultsSchema.tableName, getResultsTableKeysToDelete allResults
        ]

        match keysToDelete with
        | [] -> return ()
        | keys ->
            let batchWriteParameters =
                keys
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

let insertSnapshotAsync (snapshot: Snapshot) (client: AmazonDynamoDBClient): Task<unit> =
    task {
        if not snapshot.Users.IsEmpty then
            do! insertUsersAsync snapshot.Users client

        if not snapshot.Results.IsEmpty then
            do! insertResultsAsync snapshot.Results client
    }

let ensureTablesCreatedAsync (client: AmazonDynamoDBClient): Task<unit> =
    task {
        let! listTablesResponse = client.ListTablesAsync()

        if listTablesResponse.HttpStatusCode <> HttpStatusCode.OK then
            failwith $"Unable to list tables. HTTP status code: {listTablesResponse.HttpStatusCode}"

        let tableNames = listTablesResponse.TableNames |> Set.ofSeq

        if tableNames |> Set.contains UsersSchema.tableName |> not then
            do! Schemas.createUsersTableAsync client

        if tableNames |> Set.contains ResultsSchema.tableName |> not then
            do! Schemas.createResultsTableAsync client

        ()
    }