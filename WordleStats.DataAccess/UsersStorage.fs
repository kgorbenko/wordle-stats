module WordleStats.DataAccess.UsersStorage

open System.Threading
open System.Threading.Tasks

open Amazon.DynamoDBv2
open Amazon.DynamoDBv2.Model

open WordleStats.DataAccess.DynamoDb
open WordleStats.Common.Utils

type User = {
    Name: string
    Token: string
    PasswordHash: string option
}

type UserSearchSpecification =
    | ByName of Name: string
    | ByToken of Token: string

module UsersSchema =

    let tableName = "WordleStats.Users"

    let nameAttributeName = "Name"
    let tokenAttributeName = "Token"
    let passwordHashAttributeName = "PasswordHash"

    let allAttributes = Set [
        nameAttributeName
        tokenAttributeName
        passwordHashAttributeName
    ]

let private attributesValuesToUser (attributes: Map<string, AttributeValue>): User =
    {
        Name = readStringAttribute attributes UsersSchema.nameAttributeName
        Token = readStringAttribute attributes UsersSchema.tokenAttributeName
        PasswordHash = readOptionStringAttribute attributes UsersSchema.passwordHashAttributeName
    }

let private attributeValuesFromUser (user: User): Map<string, AttributeValue> =
    Map [
        UsersSchema.nameAttributeName, getStringAttributeValue user.Name
        UsersSchema.tokenAttributeName, getStringAttributeValue user.Token

        if user.PasswordHash.IsSome then
            UsersSchema.passwordHashAttributeName, getStringAttributeValue user.PasswordHash.Value
    ]
let rec updateUserAsync
    (user: User)
    (cancellationToken: CancellationToken)
    (client: AmazonDynamoDBClient)
    : unit Task =
    task {
        let passwordHashExpression, passwordHashAttribute, passwordHashValue =
            match user.PasswordHash with
            | Some hash ->
                ", #PasswordHash = :passwordHash",
                Some ("#PasswordHash", UsersSchema.passwordHashAttributeName),
                Some (":passwordHash", getStringAttributeValue hash)
            | None ->
                "REMOVE #PasswordHash",
                Some ("#PasswordHash", UsersSchema.passwordHashAttributeName),
                None

        let request = UpdateItemRequest()
        request.TableName <- UsersSchema.tableName
        request.Key <- Map [ UsersSchema.tokenAttributeName, getStringAttributeValue user.Token ] |> mapToDict
        request.ConditionExpression <- "attribute_exists(#Token)"
        request.UpdateExpression <- $"SET #Name = :name {passwordHashExpression}"
        request.ExpressionAttributeNames <- Map [
            "#Token", UsersSchema.tokenAttributeName
            "#Name", UsersSchema.nameAttributeName

            if passwordHashAttribute.IsSome then
                passwordHashAttribute.Value
        ] |> mapToDict
        request.ExpressionAttributeValues <- Map [
            ":name", getStringAttributeValue user.Name

            if passwordHashValue.IsSome then
                passwordHashValue.Value
        ] |> mapToDict

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