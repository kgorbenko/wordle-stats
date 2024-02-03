namespace WordleStats

[<CLIMutable>]
type DynamoDbConfiguration = {
    AwsAccessKeyId: string
    AwsSecretAccessKey: string
    ServiceUrl: string
}

module Configuration =

    [<Literal>]
    let dynamoDbConfigurationSectionName = "DynamoDbConfiguration"