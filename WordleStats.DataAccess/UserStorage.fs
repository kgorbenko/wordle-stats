module WordleStats.DataAccess.UserStorage

open System.Data
open System.Threading
open System.Threading.Tasks

open WordleStats.DataAccess.Common.Dapper

type CreateUserParameters = {
    Name: string
}

let createUserAsync
    (parameters: CreateUserParameters)
    (cancellationToken: CancellationToken)
    (connection: IDbConnection)
    (transaction: IDbTransaction)
    : Task<int> =
    task {
        let sql = """
insert into Users (Name)
output inserted.Id
values (@Name)
"""

        let parameters = {| Name = parameters.Name |}
        let command = makeCommand sql parameters transaction cancellationToken

        return! connection |> querySingleAsync<int> command
    }