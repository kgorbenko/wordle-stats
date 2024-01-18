module WordleStats.DataAccess.Tests.Common

open Microsoft.Extensions.Configuration
open System
open System.Threading.Tasks

open Amazon.DynamoDBv2

[<CLIMutable>]
type DynamoDbConfiguration = {
    AwsAccessKeyId: string
    AwsSecretAccessKey: string
    ServiceUrl: string
}

[<Literal>]
let private testConfigurationFileName = "appsettings.test.json"

[<Literal>]
let private dynamoDbConfigurationSectionName = "DynamoDbTestConfiguration"

let private getConfiguration (): DynamoDbConfiguration =
    let configuration =
        ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile(testConfigurationFileName)
            .AddEnvironmentVariables()
            .Build()

    configuration
        .GetRequiredSection(dynamoDbConfigurationSectionName)
        .Get<DynamoDbConfiguration>()

let withTestDbClientAsync (doAsync: AmazonDynamoDBClient -> Task<'a>): Task<'a> =
    task {
        let configuration = getConfiguration ()

        let dynamoDbConfig = AmazonDynamoDBConfig()
        dynamoDbConfig.ServiceURL <- configuration.ServiceUrl

        use client = new AmazonDynamoDBClient(
            configuration.AwsAccessKeyId,
            configuration.AwsSecretAccessKey,
            dynamoDbConfig
        )

        return! doAsync client
    }