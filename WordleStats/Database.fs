module WordleStats.Database

open Microsoft.FSharp.Core
open System.Threading.Tasks

open Amazon.DynamoDBv2

type DynamoDbConfiguration = {
    AwsAccessKeyId: string
    AwsSecretAccessKey: string
    Service: ServiceConfiguration
}

and ServiceConfiguration =
    | URL of string

let makeConfiguration (configuration: WordleStats.DynamoDbConfiguration): DynamoDbConfiguration =
    {
        AwsAccessKeyId = configuration.AwsAccessKeyId
        AwsSecretAccessKey = configuration.AwsSecretAccessKey
        Service = URL configuration.ServiceUrl
    }

let withDbClientAsync
    (configuration: DynamoDbConfiguration)
    (doAsync: AmazonDynamoDBClient -> Task<'a>)
    : Task<'a> =
    task {
        let dynamoDbConfig = AmazonDynamoDBConfig()

        match configuration.Service with
        | URL url -> dynamoDbConfig.ServiceURL <- url

        use client = new AmazonDynamoDBClient(
            configuration.AwsAccessKeyId,
            configuration.AwsSecretAccessKey,
            dynamoDbConfig
        )

        return! doAsync client
    }