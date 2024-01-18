module WordleStats.DataAccess.DynamoDb

open System.Globalization

open Amazon.DynamoDBv2.Model

open WordleStats.Common.Utils

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