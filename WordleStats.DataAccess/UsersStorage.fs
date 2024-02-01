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
    PinCode: int option
}

type UserSearchSpecification =
    | ByName of Name: string
    | ByToken of Token: string
    | ByPinCode of PinCode: int

module UsersSchema =

    let tableName = "WordleStats.Users"

    let nameAttributeName = "Name"
    let tokenAttributeName = "Token"
    let pinCodeAttributeName = "PinCode"

    let allAttributes = Set [
        nameAttributeName
        tokenAttributeName
        pinCodeAttributeName
    ]

let private attributesValuesToUser (attributes: Map<string, AttributeValue>): User =
    {
        Name = readStringAttribute attributes UsersSchema.nameAttributeName
        Token = readStringAttribute attributes UsersSchema.tokenAttributeName
        PinCode = readOptionNumberAttribute attributes UsersSchema.pinCodeAttributeName
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
            | ByPinCode pc -> UsersSchema.pinCodeAttributeName, getNumberAttributeValue pc

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