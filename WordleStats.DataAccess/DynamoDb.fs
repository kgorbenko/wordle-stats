module WordleStats.DataAccess.DynamoDb

open System
open System.Globalization
open System.Net

open Amazon.DynamoDBv2.Model
open Amazon.Runtime

open WordleStats.Common.Utils

let getStringAttributeValue (value: string) =
    let attributeValue = AttributeValue()
    attributeValue.S <- value
    attributeValue

let getNumberAttributeValue (value: int) =
    let attributeValue = AttributeValue()
    attributeValue.N <- value.ToString(CultureInfo.InvariantCulture)
    attributeValue

let readStringAttribute (attributes: Map<string, AttributeValue>) (attributeName: string) =
    attributes |> Map.find attributeName |> _.S

let readOptionStringAttribute (attributes: Map<string, AttributeValue>) (attributeName: string) =
    attributes |> Map.tryFind attributeName |> Option.map _.S

let readNumberAttribute (attributes: Map<string, AttributeValue>) (attributeName: string) =
    attributes |> Map.find attributeName |> (_.N >> Int32.Parse)

let readOptionNumberAttribute (attributes: Map<string, AttributeValue>) (attributeName: string) =
    attributes |> Map.tryFind attributeName |> Option.map (_.N >> Int32.Parse)

let putItemRequest (tableName: string) (itemToPut: Map<string, AttributeValue>) =
    let request = PutItemRequest()
    request.TableName <- tableName
    request.Item <- itemToPut |> mapToDict
    request

let ensureSuccessStatusCode (operation: string) (response: AmazonWebServiceResponse) =
    if response.HttpStatusCode <> HttpStatusCode.OK then
        failwith $"Unable to perform operation {operation}. HTTP Status Code = {response.HttpStatusCode}"