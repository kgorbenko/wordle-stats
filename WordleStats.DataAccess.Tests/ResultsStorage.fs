module WordleStats.DataAccess.Tests.ResultsStorage

open System
open System.Threading
open NUnit.Framework

open WordleStats.DataAccess.ResultsStorage

open Common
open WordleStats.DataAccess.Tests.TestHelper
open WordleStats.DataAccess.Tests.DatabaseLayer

[<Test>]
let ``Creates result`` () =
    task {
        do! prepareDatabaseAsync |> withTestDbClientAsync

        let user = "boris"
        let date = DateOnly(year = 2024, month = 01, day = 02)

        let result: Result = {
            User = user
            Date = date
            Wordle = Some 1
            Worldle = Some 2
            Waffle = Some 3
        }

        do! insertResultAsync result CancellationToken.None |> withTestDbClientAsync

        let! actual = getAllResultsAsync |> withTestDbClientAsync

        let expected: DatabaseSnapshot.Result list = [
            {
                User = user
                Date = "2024-01-02"
                Wordle = Some "1"
                Worldle = Some "2"
                Waffle = Some "3"
            }
        ]

        Assert.That(actual, Is.EqualTo(expected))
    }

[<Test>]
let ``Creates result without Wordle score`` () =
    task {
        do! prepareDatabaseAsync |> withTestDbClientAsync

        let user = "boris"
        let date = DateOnly(year = 2024, month = 01, day = 01)

        let result: Result = {
            User = user
            Date = date
            Wordle = None
            Worldle = Some 2
            Waffle = Some 3
        }

        do! insertResultAsync result CancellationToken.None |> withTestDbClientAsync

        let! actual = getAllResultsAsync |> withTestDbClientAsync

        let expected: DatabaseSnapshot.Result list = [
            {
                User = user
                Date = "2024-01-01"
                Wordle = None
                Worldle = Some "2"
                Waffle = Some "3"
            }
        ]

        Assert.That(actual, Is.EqualTo(expected))
    }

[<Test>]
let ``Creates result without Worldle score`` () =
    task {
        do! prepareDatabaseAsync |> withTestDbClientAsync

        let user = "boris"
        let date = DateOnly(year = 2024, month = 01, day = 01)

        let result: Result = {
            User = user
            Date = date
            Wordle = Some 1
            Worldle = None
            Waffle = Some 3
        }

        do! insertResultAsync result CancellationToken.None |> withTestDbClientAsync

        let! actual = getAllResultsAsync |> withTestDbClientAsync

        let expected: DatabaseSnapshot.Result list = [
            {
                User = user
                Date = "2024-01-01"
                Wordle = Some "1"
                Worldle = None
                Waffle = Some "3"
            }
        ]

        Assert.That(actual, Is.EqualTo(expected))
    }

[<Test>]
let ``Creates result without Waffle score`` () =
    task {
        do! prepareDatabaseAsync |> withTestDbClientAsync

        let user = "boris"
        let date = DateOnly(year = 2024, month = 01, day = 01)

        let result: Result = {
            User = user
            Date = date
            Wordle = Some 1
            Worldle = Some 2
            Waffle = None
        }

        do! insertResultAsync result CancellationToken.None |> withTestDbClientAsync

        let! actual = getAllResultsAsync |> withTestDbClientAsync

        let expected: DatabaseSnapshot.Result list = [
            {
                User = user
                Date = "2024-01-01"
                Wordle = Some "1"
                Worldle = Some "2"
                Waffle = None
            }
        ]

        Assert.That(actual, Is.EqualTo(expected))
    }