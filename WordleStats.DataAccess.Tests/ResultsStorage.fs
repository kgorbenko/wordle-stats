module WordleStats.DataAccess.Tests.ResultsStorage

open System
open System.Threading
open NUnit.Framework

open WordleStats.DataAccess
open WordleStats.DataAccess.ResultsStorage

open Common
open WordleStats.DataAccess.Tests.TestHelper
open WordleStats.DataAccess.Tests.DatabaseLayer
open WordleStats.DataAccess.Tests.DatabaseSnapshot

[<Test>]
let ``Creates result`` () =
    task {
        do! prepareDatabaseAsync |> withTestDbClientAsync

        let user = "boris"
        let date = DateOnly(year = 2024, month = 01, day = 02)

        let result: ResultsStorage.Result = {
            User = user
            Date = date
            Wordle = Some 1
            Worldle = Some 2
            Waffle = Some 3
        }

        do! putResultAsync result CancellationToken.None |> withTestDbClientAsync

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

        let result: ResultsStorage.Result = {
            User = user
            Date = date
            Wordle = None
            Worldle = Some 2
            Waffle = Some 3
        }

        do! putResultAsync result CancellationToken.None |> withTestDbClientAsync

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

        let result: ResultsStorage.Result = {
            User = user
            Date = date
            Wordle = Some 1
            Worldle = None
            Waffle = Some 3
        }

        do! putResultAsync result CancellationToken.None |> withTestDbClientAsync

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

        let result: ResultsStorage.Result = {
            User = user
            Date = date
            Wordle = Some 1
            Worldle = Some 2
            Waffle = None
        }

        do! putResultAsync result CancellationToken.None |> withTestDbClientAsync

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

[<Test>]
let ``Should update a Results when already exists`` () =
    task {
        do! prepareDatabaseAsync |> withTestDbClientAsync

        let user = "boris"
        let date = "2024-01-02"
        let result: DatabaseSnapshot.Result = {
            User = user
            Date = date
            Wordle = None
            Worldle = None
            Waffle = None
        }

        let snapshot =
            Snapshot.create ()
            |> Snapshot.withResult result

        do! insertSnapshotAsync snapshot |> withTestDbClientAsync

        let newResult: ResultsStorage.Result = {
            User = user
            Date = DateOnly(2024, 01, 02)
            Wordle = Some 1
            Worldle = Some 2
            Waffle = Some 3
        }

        do! putResultAsync newResult CancellationToken.None |> withTestDbClientAsync

        let! actual = getAllResultsAsync |> withTestDbClientAsync

        let expected: DatabaseSnapshot.Result list = [
            {
                User = user
                Date = date
                Wordle = Some "1"
                Worldle = Some "2"
                Waffle = Some "3"
            }
        ]

        Assert.That(actual, Is.EqualTo(expected))
    }

[<Test>]
let ``Find result by user specification empty table`` () =
    task {
        do! prepareDatabaseAsync |> withTestDbClientAsync

        let specification = {
            User = "boris"
            DateFrom = DateOnly.MinValue
            DateTo = DateOnly.MaxValue
        }

        let! actual = findResultsByUserSpecificationAsync specification CancellationToken.None |> withTestDbClientAsync

        Assert.That(actual, Is.Empty)
    }

[<Test>]
let ``Find result by user specification success`` () =
    task {
        do! prepareDatabaseAsync |> withTestDbClientAsync

        let user = "boris"
        let result: DatabaseSnapshot.Result = {
            User = user
            Date = "2024-01-01"
            Wordle = Some "1"
            Worldle = Some "2"
            Waffle = Some "3"
        }

        let snapshot =
            Snapshot.create ()
            |> Snapshot.withResult result

        do! insertSnapshotAsync snapshot |> withTestDbClientAsync

        let specification = {
            User = user
            DateFrom = DateOnly.MinValue
            DateTo = DateOnly.MaxValue
        }

        let! actual = findResultsByUserSpecificationAsync specification CancellationToken.None |> withTestDbClientAsync

        let expected: ResultsStorage.Result list = [
            {
                User = user
                Date = DateOnly(2024, 01, 01)
                Wordle = Some 1
                Worldle = Some 2
                Waffle = Some 3
            }
        ]

        Assert.That(actual, Is.EqualTo(expected))
    }

[<Test>]
let ``Find result by user specification user filter`` () =
    task {
        do! prepareDatabaseAsync |> withTestDbClientAsync

        let result: DatabaseSnapshot.Result = {
            User = "boris"
            Date = "2024-01-01"
            Wordle = Some "1"
            Worldle = Some "2"
            Waffle = Some "3"
        }

        let snapshot =
            Snapshot.create ()
            |> Snapshot.withResult result

        do! insertSnapshotAsync snapshot |> withTestDbClientAsync

        let specification = {
            User = "bor"
            DateFrom = DateOnly.MinValue
            DateTo = DateOnly.MaxValue
        }

        let! actual = findResultsByUserSpecificationAsync specification CancellationToken.None |> withTestDbClientAsync

        Assert.That(actual, Is.Empty)
    }

[<Test>]
let ``Find result by user specification date from filter`` () =
    task {
        do! prepareDatabaseAsync |> withTestDbClientAsync

        let user = "boris"
        let result: DatabaseSnapshot.Result = {
            User = user
            Date = "2024-01-01"
            Wordle = Some "1"
            Worldle = Some "2"
            Waffle = Some "3"
        }

        let snapshot =
            Snapshot.create ()
            |> Snapshot.withResult result

        do! insertSnapshotAsync snapshot |> withTestDbClientAsync

        let specification = {
            User = "boris"
            DateFrom = DateOnly(2024, 01, 02)
            DateTo = DateOnly.MaxValue
        }

        let! actual = findResultsByUserSpecificationAsync specification CancellationToken.None |> withTestDbClientAsync

        Assert.That(actual, Is.Empty)
    }

[<Test>]
let ``Find result by user specification date to filter`` () =
    task {
        do! prepareDatabaseAsync |> withTestDbClientAsync

        let user = "boris"
        let result: DatabaseSnapshot.Result = {
            User = user
            Date = "2024-01-02"
            Wordle = Some "1"
            Worldle = Some "2"
            Waffle = Some "3"
        }

        let snapshot =
            Snapshot.create ()
            |> Snapshot.withResult result

        do! insertSnapshotAsync snapshot |> withTestDbClientAsync

        let specification = {
            User = "boris"
            DateFrom = DateOnly.MinValue
            DateTo = DateOnly(2024, 01, 01)
        }

        let! actual = findResultsByUserSpecificationAsync specification CancellationToken.None |> withTestDbClientAsync

        Assert.That(actual, Is.Empty)
    }

[<Test>]
let ``Find result by user specification optional fields`` () =
    task {
        do! prepareDatabaseAsync |> withTestDbClientAsync

        let user = "boris"
        let result: DatabaseSnapshot.Result = {
            User = user
            Date = "2024-01-01"
            Wordle = None
            Worldle = None
            Waffle = None
        }

        let snapshot =
            Snapshot.create ()
            |> Snapshot.withResult result

        do! insertSnapshotAsync snapshot |> withTestDbClientAsync

        let specification = {
            User = user
            DateFrom = DateOnly.MinValue
            DateTo = DateOnly.MaxValue
        }

        let! actual = findResultsByUserSpecificationAsync specification CancellationToken.None |> withTestDbClientAsync

        let expected: ResultsStorage.Result list = [
            {
                User = user
                Date = DateOnly(2024, 01, 01)
                Wordle = None
                Worldle = None
                Waffle = None
            }
        ]

        Assert.That(actual, Is.EqualTo(expected))
    }

[<Test>]
let ``Find results by user specification multiple`` () =
    task {
        do! prepareDatabaseAsync |> withTestDbClientAsync

        let result1: DatabaseSnapshot.Result = {
            User = "user1"
            Date = "2024-01-01"
            Wordle = Some "1"
            Worldle = Some "2"
            Waffle = Some "3"
        }

        let result2: DatabaseSnapshot.Result = {
            User = "user1"
            Date = "2024-01-02"
            Wordle = Some "1"
            Worldle = Some "2"
            Waffle = Some "3"
        }

        let result3: DatabaseSnapshot.Result = {
            User = "user2"
            Date = "2024-01-03"
            Wordle = Some "1"
            Worldle = Some "2"
            Waffle = Some "3"
        }

        let snapshot =
            Snapshot.create ()
            |> Snapshot.withResults [result1; result2; result3]

        do! insertSnapshotAsync snapshot |> withTestDbClientAsync

        let specification = {
            User = "user1"
            DateFrom = DateOnly.MinValue
            DateTo = DateOnly.MaxValue
        }

        let! actual = findResultsByUserSpecificationAsync specification CancellationToken.None |> withTestDbClientAsync

        let expected: ResultsStorage.Result list = [
            {
                User = "user1"
                Date = DateOnly(2024, 01, 01)
                Wordle = Some 1
                Worldle = Some 2
                Waffle = Some 3
            }
            {
                User = "user1"
                Date = DateOnly(2024, 01, 02)
                Wordle = Some 1
                Worldle = Some 2
                Waffle = Some 3
            }
        ]

        Assert.That(actual, Is.EqualTo(expected))
    }

[<Test>]
let ``Find results by date specification empty table`` () =
    task {
        do! prepareDatabaseAsync |> withTestDbClientAsync

        let specification = {
            Date = DateOnly(2024, 01, 01)
            User = None
        }

        let! actual = findResultsByDateSpecificationAsync specification CancellationToken.None |> withTestDbClientAsync

        Assert.That(actual, Is.Empty)
    }

[<Test>]
let ``Find results by date specification filter by date`` () =
    task {
        do! prepareDatabaseAsync |> withTestDbClientAsync

        let user = "boris"

        let result1: DatabaseSnapshot.Result = {
            User = user
            Date = "2024-01-01"
            Wordle = Some "1"
            Worldle = Some "2"
            Waffle = Some "3"
        }

        let result2: DatabaseSnapshot.Result = {
            User = user
            Date = "2024-01-02"
            Wordle = Some "1"
            Worldle = Some "2"
            Waffle = Some "3"
        }

        let snapshot =
            Snapshot.create ()
            |> Snapshot.withResults [result1; result2]

        do! insertSnapshotAsync snapshot |> withTestDbClientAsync

        let specDate = DateOnly(2024, 01, 01)
        let specification = {
            Date = specDate
            User = None
        }

        let! actual = findResultsByDateSpecificationAsync specification CancellationToken.None |> withTestDbClientAsync

        let expected: ResultsStorage.Result list = [
            {
                User = user
                Date = specDate
                Wordle = Some 1
                Worldle = Some 2
                Waffle = Some 3
            }
        ]

        Assert.That(actual, Is.EqualTo(expected))
    }

[<Test>]
let ``Find results by date specification filter by user`` () =
    task {
        do! prepareDatabaseAsync |> withTestDbClientAsync

        let user1 = "boris"
        let result1: DatabaseSnapshot.Result = {
            User = user1
            Date = "2024-01-01"
            Wordle = Some "1"
            Worldle = Some "2"
            Waffle = Some "3"
        }

        let user2 = "foo"
        let result2: DatabaseSnapshot.Result = {
            User = user2
            Date = "2024-01-01"
            Wordle = Some "1"
            Worldle = Some "2"
            Waffle = Some "3"
        }

        let snapshot =
            Snapshot.create ()
            |> Snapshot.withResults [result1; result2]

        do! insertSnapshotAsync snapshot |> withTestDbClientAsync

        let specDate = DateOnly(2024, 01, 01)
        let specification = {
            Date = specDate
            User = Some user1
        }

        let! actual = findResultsByDateSpecificationAsync specification CancellationToken.None |> withTestDbClientAsync

        let expected: ResultsStorage.Result list = [
            {
                User = user1
                Date = specDate
                Wordle = Some 1
                Worldle = Some 2
                Waffle = Some 3
            }
        ]

        Assert.That(actual, Is.EqualTo(expected))
    }