module WordleStats.DataAccess.Tests.UserStorage.CreateUserAsync

open System.Threading
open NUnit.Framework

open WordleStats.DataAccess.Common.Dapper
open WordleStats.DataAccess.UserStorage

open WordleStats.DataAccess.Tests.Common.TestHelper
open WordleStats.DataAccess.Tests.DatabaseLayer
open WordleStats.DataAccess.Tests.DatabaseSnapshot

[<Test>]
let ``Creates user`` () =
    task {
        do! clearDatabaseAsync |> withTestSessionAsync
        registerOptionTypes ()

        let name = "test"

        let! insertedId = createUserAsync { Name = name } CancellationToken.None |> withTestSessionAsync

        let expected: User list = [
            { Id = Some insertedId; Name = name }
        ]

        let! actual = getAllUsersAsync |> withTestSessionAsync

        Assert.That(actual, Is.EqualTo(expected))
    }