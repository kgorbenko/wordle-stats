﻿module WordleStats.DataAccess.Tests.UsersStorage

open Amazon.DynamoDBv2.Model

open System.Threading
open NUnit.Framework

open Common
open WordleStats.DataAccess
open WordleStats.DataAccess.UsersStorage

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
            PinCode = None
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
            PinCode = Some "1234"
        }

        let matchingUserName = "matching name"
        let matchingUserToken = "another token"
        let matchingUser: User = {
            Name = matchingUserName
            Token = matchingUserToken
            PinCode = Some "4321"
        }

        let snapshot =
            Snapshot.create ()
            |> Snapshot.withUsers [user; matchingUser]

        do! insertSnapshotAsync snapshot |> withTestDbClientAsync

        let! actual = findUserBySpecificationAsync (ByName matchingUserName) CancellationToken.None |> withTestDbClientAsync

        let expected: UsersStorage.User option =
            Some { Name = matchingUserName; Token = matchingUserToken; PinCode = Some 4321 }

        Assert.That(actual, Is.EqualTo(expected))
    }

[<Test>]
let ``Not found by Token substring`` () =
    task {
        do! prepareDatabaseAsync |> withTestDbClientAsync

        let user: User = {
            Name = "some user"
            Token = "some token"
            PinCode = Some "1234"
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
            PinCode = Some "1234"
        }

        let matchingUserName = "another name"
        let matchingUserToken = "matching token"
        let matchingUser: User = {
            Name = matchingUserName
            Token = matchingUserToken
            PinCode = Some "4321"
        }

        let snapshot =
            Snapshot.create ()
            |> Snapshot.withUsers [user; matchingUser]

        do! insertSnapshotAsync snapshot |> withTestDbClientAsync

        let! actual = findUserBySpecificationAsync (ByToken matchingUserToken) CancellationToken.None |> withTestDbClientAsync

        let expected: UsersStorage.User option =
            Some { Name = matchingUserName; Token = matchingUserToken; PinCode = Some 4321 }

        Assert.That(actual, Is.EqualTo(expected))
    }

[<Test>]
let ``Not found by PinCode substring`` () =
    task {
        do! prepareDatabaseAsync |> withTestDbClientAsync

        let user: User = {
            Name = "some user"
            Token = "some token"
            PinCode = Some "1234"
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
            PinCode = Some "1234"
        }

        let matchingUserName = "another name"
        let matchingUserToken = "another token"

        let pinCode = 4321
        let stringPinCode = pinCode |> string

        let matchingUser: User = {
            Name = matchingUserName
            Token = matchingUserToken
            PinCode = Some stringPinCode
        }

        let snapshot =
            Snapshot.create ()
            |> Snapshot.withUsers [user; matchingUser]

        do! insertSnapshotAsync snapshot |> withTestDbClientAsync

        let! actual = findUserBySpecificationAsync (ByPinCode pinCode) CancellationToken.None |> withTestDbClientAsync

        let expected: UsersStorage.User option =
            Some { Name = matchingUserName; Token = matchingUserToken; PinCode = Some pinCode }

        Assert.That(actual, Is.EqualTo(expected))
    }

[<Test>]
let ``Find User by PinCode not found when None`` () =
    task {
        do! prepareDatabaseAsync |> withTestDbClientAsync

        let user: User = {
            Name = "some user"
            Token = "some token"
            PinCode = None
        }

        let snapshot =
            Snapshot.create ()
            |> Snapshot.withUsers [user]

        do! insertSnapshotAsync snapshot |> withTestDbClientAsync

        let! actual = findUserBySpecificationAsync (ByPinCode 1234) CancellationToken.None |> withTestDbClientAsync

        Assert.That(actual, Is.EqualTo(None))
    }

[<Test>]
let ``Find User By Name no PinCode`` () =
    task {
        do! prepareDatabaseAsync |> withTestDbClientAsync

        let name = "some user"
        let token = "some token"
        let user: User = {
            Name = name
            Token = token
            PinCode = None
        }

        let snapshot =
            Snapshot.create ()
            |> Snapshot.withUsers [user]

        do! insertSnapshotAsync snapshot |> withTestDbClientAsync

        let! actual = findUserBySpecificationAsync (ByName name) CancellationToken.None |> withTestDbClientAsync

        let expected: UsersStorage.User option =
            Some { Name = name; Token = token; PinCode = None }

        Assert.That(actual, Is.EqualTo(expected))
    }

[<Test>]
let ``Update User`` () =
    task {
        do! prepareDatabaseAsync |> withTestDbClientAsync

        let user: User = {
            Name = "name"
            Token = "token"
            PinCode = None
        }

        let snapshot =
            Snapshot.create ()
            |> Snapshot.withUsers [user]

        do! insertSnapshotAsync snapshot |> withTestDbClientAsync

        let newUser: UsersStorage.User = {
            Name = "another name"
            Token = user.Token
            PinCode = Some 1234
        }

        do! updateUserAsync newUser CancellationToken.None |> withTestDbClientAsync

        let! actual = getAllUsersAsync |> withTestDbClientAsync

        let expected: User list = [
            {
                Name = "another name"
                Token = user.Token
                PinCode = Some "1234"
            }
        ]

        Assert.That(actual, Is.EqualTo(expected))
    }

[<Test>]
let ``Update User removes PinCode`` () =
    task {
        do! prepareDatabaseAsync |> withTestDbClientAsync

        let user: User = {
            Name = "name"
            Token = "token"
            PinCode = Some "1234"
        }

        let snapshot =
            Snapshot.create ()
            |> Snapshot.withUsers [user]

        do! insertSnapshotAsync snapshot |> withTestDbClientAsync

        let newUser: UsersStorage.User = {
            Name = "another name"
            Token = user.Token
            PinCode = None
        }

        do! updateUserAsync newUser CancellationToken.None |> withTestDbClientAsync

        let! actual = getAllUsersAsync |> withTestDbClientAsync

        let expected: User list = [
            {
                Name = "another name"
                Token = user.Token
                PinCode = None
            }
        ]

        Assert.That(actual, Is.EqualTo(expected))
    }

[<Test>]
let ``Update User does not create a new User`` () =
    task {
        do! prepareDatabaseAsync |> withTestDbClientAsync

        let newUser: UsersStorage.User = {
            Name = "test"
            Token = "token"
            PinCode = None
        }

        Assert.ThrowsAsync<ConditionalCheckFailedException>(
            fun x -> task { do! updateUserAsync newUser CancellationToken.None |> withTestDbClientAsync }
        ) |> ignore
    }