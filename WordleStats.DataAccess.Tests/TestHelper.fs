module WordleStats.DataAccess.Tests.TestHelper

open System.Threading.Tasks

open Amazon.DynamoDBv2

let prepareDatabaseAsync (client: AmazonDynamoDBClient): unit Task =
    task {
        do! DatabaseLayer.ensureTablesCreatedAsync client
        do! DatabaseLayer.clearAllTablesAsync client
    }