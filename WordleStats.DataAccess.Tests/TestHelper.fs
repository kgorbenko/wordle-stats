namespace WordleStats.DataAccess.Tests.Common

open Microsoft.Data.SqlClient
open Microsoft.Extensions.Configuration
open System
open System.Data
open System.Threading.Tasks

open WordleStats.DataAccess.Tests

module TestHelper =

    [<Literal>]
    let private testConfigurationFileName = "appsettings.test.json"

    [<Literal>]
    let private defaultTestConnectionStringName = "WordleStatsDefault"

    let private configuration =
        ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile(testConfigurationFileName)
            .AddEnvironmentVariables()
            .Build()

    let private getTestConnectionString () =
        configuration.GetConnectionString(defaultTestConnectionStringName)

    let withTestSessionAsync (doAsync: IDbConnection -> IDbTransaction -> 'a Task): 'a Task =
        task {
            let testConnectionString = getTestConnectionString ()
            use connection = new SqlConnection(testConnectionString)
            do! connection.OpenAsync()

            use! transaction = connection.BeginTransactionAsync()

            let! result = doAsync connection transaction

            do! transaction.CommitAsync()

            return result
        }

    let clearDatabaseAsync (connection: IDbConnection) (transaction: IDbTransaction): unit Task =
        task {
            do! DatabaseLayer.clearDatabaseAsync connection transaction
        }