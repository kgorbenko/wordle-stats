namespace WordleStats.DataAccess.Common

open System.Threading
open System.Threading.Tasks

module Database =

    open System.Data
    open Microsoft.Data.SqlClient

    let withConnectionAsync
        (connectionString: string)
        (cancellationToken: CancellationToken)
        (doAsync: IDbConnection -> Task<'a>)
        : Task<'a> =
        task {
            use connection = new SqlConnection(connectionString)
            do! connection.OpenAsync(cancellationToken)

            let! result = doAsync connection

            return result
        }

    let inTransactionAsync
        (connection: IDbConnection)
        (doAsync: IDbConnection -> IDbTransaction -> Task<'a>)
        : Task<'a> =
        task {
            use transaction = connection.BeginTransaction()

            let! result = doAsync connection transaction
            transaction.Commit()

            return result
        }

module Dapper =

    open Dapper
    open System.Data

    let registerOptionTypes () =
        Dapper.FSharp.MSSQL.OptionTypes.register()

    let makeCommand (sql: string) (parameters: obj) (transaction: IDbTransaction) (cancellationToken: CancellationToken) =
        CommandDefinition (
             commandText = sql,
             parameters = parameters,
             transaction = transaction,
             cancellationToken = cancellationToken
        )

    let executeAsync (command: CommandDefinition) (connection: IDbConnection): unit Task =
        task {
            let! _ = connection.ExecuteAsync(command)
            ()
        }

    let queryAsync<'a> (command: CommandDefinition) (connection: IDbConnection): 'a list Task =
        task {
            let! result = connection.QueryAsync<'a> command

            return result
                |> List.ofSeq
        }

    let querySingleAsync<'a> (command: CommandDefinition) (connection: IDbConnection): 'a Task =
        task {
            return! connection.QuerySingleAsync<'a>(command)
        }