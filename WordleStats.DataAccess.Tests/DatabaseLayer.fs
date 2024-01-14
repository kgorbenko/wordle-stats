module WordleStats.DataAccess.Tests.DatabaseLayer

open System.Data
open System.Threading
open System.Threading.Tasks

open Dapper.FSharp.MSSQL

open WordleStats.DataAccess.Common.Dapper
open WordleStats.DataAccess.Tests.DatabaseSnapshot

[<Literal>]
let UsersTableName = "Users"

let usersTable = table'<User> UsersTableName

let private selectAsync<'a>
    (connection: IDbConnection)
    (transaction: IDbTransaction)
    (selectQuery: SelectQuery)
    : Task<'a list> =
    task {
        let! result = connection.SelectAsync<'a>(selectQuery, trans = transaction)

        return result |> List.ofSeq
    }

let private insertOutputAsync<'input, 'output>
    (connection: IDbConnection)
    (transaction: IDbTransaction)
    (insertFunc: InsertQuery<'input>)
    : Task<'output list> =
    task {
        let! result = connection.InsertOutputAsync<'input, 'output>(insertFunc, trans = transaction)

        return result |> List.ofSeq
    }

let clearDatabaseAsync
    (connection: IDbConnection)
    (transaction: IDbTransaction)
    : unit Task =
    task {
        let sql = """
delete from Users
dbcc checkident (Users, reseed, 0)
"""

        let command = makeCommand sql null transaction CancellationToken.None
        do! connection |> executeAsync command
    }

let insertUserAsync
    (user: User)
    (connection: IDbConnection)
    (transaction: IDbTransaction)
    : Task<int> =
    task {
        let! result =
            insert {
                into usersTable
                values [user]
            }
            |> insertOutputAsync connection transaction

        return result
            |> List.exactlyOne
            |> _.Id.Value
    }

let getAllUsersAsync
    (connection: IDbConnection)
    (transaction: IDbTransaction)
    : User list Task =
    task {
        return!
            select {
                for user in usersTable do
                selectAll
            }
            |> selectAsync<User> connection transaction
    }