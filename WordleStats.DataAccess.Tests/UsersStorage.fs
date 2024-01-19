module WordleStats.DataAccess.Tests.UsersStorage

open System
open System.Threading
open NUnit.Framework

open WordleStats.DataAccess
open WordleStats.DataAccess.UsersStorage

open Common
open WordleStats.DataAccess.Tests.DatabaseSnapshot
open WordleStats.DataAccess.Tests.TestHelper
open WordleStats.DataAccess.Tests.DatabaseLayer

[<Test>]
let ``Not found by Name substring`` () =
    task {
        do! prepareDatabaseAsync |> withTestDbClientAsync

        let user: User = {
            Name = "some user"
            Token = "some token"
            PinCode = "1234"
        }

        let snapshot =
            Snapshot.create ()
            |> Snapshot.withUser user

        do! insertSnapshotAsync snapshot |> withTestDbClientAsync

        let! actual = findUserBySpecificationAsync (ByName "user") CancellationToken.None |> withTestDbClientAsync

        Assert.That(actual, Is.EqualTo(None))
    }

[<Test>]
let ``Find User by Name`` () =
    task {
        do! prepareDatabaseAsync |> withTestDbClientAsync

        let user: User = {
            Name = "some user"
            Token = "some token"
            PinCode = "1234"
        }

        let matchingUserName = "matching name"
        let matchingUserToken = "another token"
        let matchingUser: User = {
            Name = matchingUserName
            Token = matchingUserToken
            PinCode = "4321"
        }

        let snapshot =
            Snapshot.create ()
            |> Snapshot.withUsers [user; matchingUser]

        do! insertSnapshotAsync snapshot |> withTestDbClientAsync

        let! actual = findUserBySpecificationAsync (ByName matchingUserName) CancellationToken.None |> withTestDbClientAsync

        let expected: UsersStorage.User option =
            Some { Name = matchingUserName; Token = matchingUserToken; PinCode = 4321 }

        Assert.That(actual, Is.EqualTo(expected))
    }

[<Test>]
let ``Not found by Token substring`` () =
    task {
        do! prepareDatabaseAsync |> withTestDbClientAsync

        let user: User = {
            Name = "some user"
            Token = "some token"
            PinCode = "1234"
        }

        let snapshot =
            Snapshot.create ()
            |> Snapshot.withUser user

        do! insertSnapshotAsync snapshot |> withTestDbClientAsync

        let! actual = findUserBySpecificationAsync (ByToken "token") CancellationToken.None |> withTestDbClientAsync

        Assert.That(actual, Is.EqualTo(None))
    }

[<Test>]
let ``Find User by Token`` () =
    task {
        do! prepareDatabaseAsync |> withTestDbClientAsync

        let user: User = {
            Name = "some user"
            Token = "some token"
            PinCode = "1234"
        }

        let matchingUserName = "another name"
        let matchingUserToken = "matching token"
        let matchingUser: User = {
            Name = matchingUserName
            Token = matchingUserToken
            PinCode = "4321"
        }

        let snapshot =
            Snapshot.create ()
            |> Snapshot.withUsers [user; matchingUser]

        do! insertSnapshotAsync snapshot |> withTestDbClientAsync

        let! actual = findUserBySpecificationAsync (ByToken matchingUserToken) CancellationToken.None |> withTestDbClientAsync

        let expected: UsersStorage.User option =
            Some { Name = matchingUserName; Token = matchingUserToken; PinCode = 4321 }

        Assert.That(actual, Is.EqualTo(expected))
    }

[<Test>]
let ``Not found by PinCode substring`` () =
    task {
        do! prepareDatabaseAsync |> withTestDbClientAsync

        let user: User = {
            Name = "some user"
            Token = "some token"
            PinCode = "1234"
        }

        let snapshot =
            Snapshot.create ()
            |> Snapshot.withUser user

        do! insertSnapshotAsync snapshot |> withTestDbClientAsync

        let! actual = findUserBySpecificationAsync (ByPinCode 123) CancellationToken.None |> withTestDbClientAsync

        Assert.That(actual, Is.EqualTo(None))
    }

[<Test>]
let ``Find User by PinCode`` () =
    task {
        do! prepareDatabaseAsync |> withTestDbClientAsync

        let user: User = {
            Name = "some user"
            Token = "some token"
            PinCode = "1234"
        }

        let matchingUserName = "another name"
        let matchingUserToken = "another token"

        let pinCode = 4321
        let stringPinCode = pinCode |> string

        let matchingUser: User = {
            Name = matchingUserName
            Token = matchingUserToken
            PinCode = stringPinCode
        }

        let snapshot =
            Snapshot.create ()
            |> Snapshot.withUsers [user; matchingUser]

        do! insertSnapshotAsync snapshot |> withTestDbClientAsync

        let! actual = findUserBySpecificationAsync (ByPinCode pinCode) CancellationToken.None |> withTestDbClientAsync

        let expected: UsersStorage.User option =
            Some { Name = matchingUserName; Token = matchingUserToken; PinCode = pinCode }

        Assert.That(actual, Is.EqualTo(expected))
    }