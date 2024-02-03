module WordleStats.DataAccess.UsersStorage

open System.Threading
open System.Threading.Tasks

open Amazon.DynamoDBv2
open Amazon.DynamoDBv2.Model

open WordleStats.DataAccess.DynamoDb
open WordleStats.Common.Utils

type Password = {
    Hash: string
    Salt: string
}

type User = {
    Token: string
    Name: string option
    Password: Password option
}

type UserSearchSpecification =
    | ByName of Name: string
    | ByToken of Token: string

module UsersSchema =

    let tableName = "WordleStats.Users"

    let tokenAttributeName = "Token"
    let nameAttributeName = "Name"
    let passwordAttributeName = "Password"

    module Password =

        let hashAttributeName = "Hash"
        let saltAttributeName = "Salt"

    let allAttributes = Set [
        tokenAttributeName
        nameAttributeName
        passwordAttributeName
    ]

let private attributeValuesToPassword (attributes: Map<string, AttributeValue>): Password =
    {
        Hash = readStringAttribute attributes UsersSchema.Password.hashAttributeName
        Salt = readStringAttribute attributes UsersSchema.Password.saltAttributeName
    }

let private attributeValuesFromPassword (password: Password): Map<string, AttributeValue> =
    Map [
        UsersSchema.Password.hashAttributeName, getStringAttributeValue password.Hash
        UsersSchema.Password.saltAttributeName, getStringAttributeValue password.Salt
    ]

let private attributesValuesToUser (attributes: Map<string, AttributeValue>): User =
    {
        Token = readStringAttribute attributes UsersSchema.tokenAttributeName
        Name = readOptionStringAttribute attributes UsersSchema.nameAttributeName
        Password = readOptionMapAttribute attributes UsersSchema.passwordAttributeName |> Option.map attributeValuesToPassword
    }

let private attributeValuesFromUser (user: User): Map<string, AttributeValue> =
    Map [
        UsersSchema.tokenAttributeName, getStringAttributeValue user.Token

        if user.Name.IsSome then
            UsersSchema.nameAttributeName, getStringAttributeValue user.Name.Value

        if user.Password.IsSome then
            UsersSchema.passwordAttributeName, user.Password.Value |> attributeValuesFromPassword |> getMapAttributeValue
    ]

type private SetUserAttributeParameters = {
    Condition: string
    AttributeName: string * string
    AttributeValue: string * AttributeValue
}

type private RemoveUserAttributeParameters = {
    Condition: string
    AttributeName: string * string
}

let rec updateUserAsync
    (user: User)
    (cancellationToken: CancellationToken)
    (client: AmazonDynamoDBClient)
    : unit Task =
    task {
        let updatableAttributesMapping = [
            UsersSchema.nameAttributeName, "#Name", ":name", user.Name.IsSome, (fun () -> user.Name.Value |> getStringAttributeValue)
            UsersSchema.passwordAttributeName, "#PasswordHash", ":passwordHash", user.Password.IsSome, (fun () -> user.Password.Value |> attributeValuesFromPassword |> getMapAttributeValue)
        ]

        let makeSetParameters attributeName attributeNameAlias attributeValueAlias attributeValueAccessor =
            {
                Condition = $"{attributeNameAlias} = {attributeValueAlias}"
                AttributeName = attributeNameAlias, attributeName
                AttributeValue = attributeValueAlias, attributeValueAccessor ()
            }

        let makeRemoveParameters attributeName attributeNameAlias =
            {
                Condition = $"{attributeNameAlias}"
                AttributeName = attributeNameAlias, attributeName
            }

        let setParameters, removeParameters =
            updatableAttributesMapping
            |> partitionMap (fun (name, nameAlias, valueAlias, isSome, attributeValueAccessor) ->
                match isSome with
                | true -> Choice1Of2 (makeSetParameters name nameAlias valueAlias attributeValueAccessor)
                | false -> Choice2Of2 (makeRemoveParameters name nameAlias)
            )

        let updateExpression =
            let buildSetExpression (parameters: SetUserAttributeParameters list) =
                parameters |> Seq.map _.Condition |> String.concat ", "

            let buildRemoveExpression (parameters: RemoveUserAttributeParameters list) =
                parameters |> Seq.map _.Condition |> String.concat ", "

            match setParameters, removeParameters with
            | [], [] -> failwith "expected at lease one update operations"
            | set, [] -> $"""SET {buildSetExpression set}"""
            | [], remove -> $"""REMOVE {buildRemoveExpression remove}"""
            | set, remove -> $"""SET {buildSetExpression set} REMOVE {buildRemoveExpression remove}"""

        let keyAttributes = ["#Token", UsersSchema.tokenAttributeName]
        let setAttributes = setParameters |> Seq.map _.AttributeName |> List.ofSeq
        let removeAttributes = removeParameters |> Seq.map _.AttributeName |> List.ofSeq
        let setValues = setParameters |> Seq.map _.AttributeValue |> List.ofSeq

        let request = UpdateItemRequest()
        request.TableName <- UsersSchema.tableName
        request.Key <- Map [ UsersSchema.tokenAttributeName, getStringAttributeValue user.Token ] |> mapToDict
        request.ConditionExpression <- "attribute_exists(#Token)"
        request.UpdateExpression <- updateExpression
        request.ExpressionAttributeNames <- keyAttributes @ setAttributes @ removeAttributes |> Map |> mapToDict
        request.ExpressionAttributeValues <- setValues |> Map |> mapToDict

        let! response = client.UpdateItemAsync(request, cancellationToken)

        response |> ensureSuccessStatusCode (nameof updateUserAsync)

        return ()
    }

let rec findUserBySpecificationAsync
    (specification: UserSearchSpecification)
    (cancellationToken: CancellationToken)
    (client: AmazonDynamoDBClient)
    : User option Task =
    task {
        let filterAttributeNameAlias = "#filter"
        let filterAttributeValuePlaceholder = ":value"
        let filterExpression = $"{filterAttributeNameAlias} = {filterAttributeValuePlaceholder}"

        let filterAttributeName, filterAttributeValue =
            match specification with
            | ByName n -> UsersSchema.nameAttributeName, getStringAttributeValue n
            | ByToken t -> UsersSchema.tokenAttributeName, getStringAttributeValue t

        let request = ScanRequest(UsersSchema.tableName)
        request.FilterExpression <- filterExpression
        request.ExpressionAttributeNames <- Map [ (filterAttributeNameAlias, filterAttributeName) ] |> mapToDict
        request.ExpressionAttributeValues <- Map [ (filterAttributeValuePlaceholder, filterAttributeValue) ] |> mapToDict

        let! response = client.ScanAsync(request, cancellationToken)

        response |> ensureSuccessStatusCode (nameof findUserBySpecificationAsync)

        return response.Items
            |> Seq.tryExactlyOne
            |> Option.map (dictToMap >> attributesValuesToUser)
    }