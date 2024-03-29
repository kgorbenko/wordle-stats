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
            Name = Some "some user"
            Token = "some token"
            Password = None
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
            Name = Some "some user"
            Token = "some token"
            Password = Some { Hash = "hash"; Salt = "salt" }
        }

        let matchingUserName = "matching name"
        let matchingUserToken = "another token"
        let matchingPassword = Some { Hash = "another hash"; Salt = "another salt" }
        let matchingUser: User = {
            Name = Some matchingUserName
            Token = matchingUserToken
            Password = matchingPassword
        }

        let snapshot =
            Snapshot.create ()
            |> Snapshot.withUsers [user; matchingUser]

        do! insertSnapshotAsync snapshot |> withTestDbClientAsync

        let! actual = findUserBySpecificationAsync (ByName matchingUserName) CancellationToken.None |> withTestDbClientAsync

        let expected: UsersStorage.User option =
            Some {
                Name = Some matchingUserName
                Token = matchingUserToken
                Password = Some {
                    Hash = matchingPassword.Value.Hash
                    Salt = matchingPassword.Value.Salt
                }
            }

        Assert.That(actual, Is.EqualTo(expected))
    }

[<Test>]
let ``Not found by Token substring`` () =
    task {
        do! prepareDatabaseAsync |> withTestDbClientAsync

        let user: User = {
            Name = Some "some user"
            Token = "some token"
            Password = Some { Hash = "hash"; Salt = "salt" }
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
            Name = Some "some user"
            Token = "some token"
            Password = Some { Hash = "hash"; Salt = "salt" }
        }

        let matchingUserName = "another name"
        let matchingUserToken = "matching token"
        let matchingPassword = Some { Hash = "another hash"; Salt = "another salt" }
        let matchingUser: User = {
            Name = Some matchingUserName
            Token = matchingUserToken
            Password =  matchingPassword
        }

        let snapshot =
            Snapshot.create ()
            |> Snapshot.withUsers [user; matchingUser]

        do! insertSnapshotAsync snapshot |> withTestDbClientAsync

        let! actual = findUserBySpecificationAsync (ByToken matchingUserToken) CancellationToken.None |> withTestDbClientAsync

        let expected: UsersStorage.User option =
            Some {
                Name = Some matchingUserName
                Token = matchingUserToken
                Password = Some {
                    Hash = matchingPassword.Value.Hash
                    Salt = matchingPassword.Value.Salt
                }
            }

        Assert.That(actual, Is.EqualTo(expected))
    }

[<Test>]
let ``Find User By Name no Hash`` () =
    task {
        do! prepareDatabaseAsync |> withTestDbClientAsync

        let name = "some user"
        let token = "some token"
        let user: User = {
            Name = Some name
            Token = token
            Password = None
        }

        let snapshot =
            Snapshot.create ()
            |> Snapshot.withUsers [user]

        do! insertSnapshotAsync snapshot |> withTestDbClientAsync

        let! actual = findUserBySpecificationAsync (ByName name) CancellationToken.None |> withTestDbClientAsync

        let expected: UsersStorage.User option =
            Some { Name = Some name; Token = token; Password = None }

        Assert.That(actual, Is.EqualTo(expected))
    }

[<Test>]
let ``Update User sets empty attributes`` () =
    task {
        do! prepareDatabaseAsync |> withTestDbClientAsync

        let user: User = {
            Name = None
            Token = "token"
            Password = None
        }

        let snapshot =
            Snapshot.create ()
            |> Snapshot.withUsers [user]

        do! insertSnapshotAsync snapshot |> withTestDbClientAsync

        let newName = Some "some name"
        let newPassword: UsersStorage.Password option = Some { Hash = "hash"; Salt = "salt" }
        let newUser: UsersStorage.User = {
            Name = newName
            Token = user.Token
            Password = newPassword
        }

        do! updateUserAsync newUser CancellationToken.None |> withTestDbClientAsync

        let! actual = getAllUsersAsync |> withTestDbClientAsync

        let expected: User list = [
            {
                Name = newName
                Token = user.Token
                Password = Some {
                    Hash = newPassword.Value.Hash
                    Salt = newPassword.Value.Salt
                }
            }
        ]

        Assert.That(actual, Is.EqualTo(expected))
    }

[<Test>]
let ``Update User removes empty attributes`` () =
    task {
        do! prepareDatabaseAsync |> withTestDbClientAsync

        let user: User = {
            Name = Some "name"
            Token = "token"
            Password = Some { Hash = "hash"; Salt = "salt" }
        }

        let snapshot =
            Snapshot.create ()
            |> Snapshot.withUsers [user]

        do! insertSnapshotAsync snapshot |> withTestDbClientAsync

        let newUser: UsersStorage.User = {
            Name = None
            Token = user.Token
            Password = None
        }

        do! updateUserAsync newUser CancellationToken.None |> withTestDbClientAsync

        let! actual = getAllUsersAsync |> withTestDbClientAsync

        let expected: User list = [
            {
                Name = None
                Token = user.Token
                Password = None
            }
        ]

        Assert.That(actual, Is.EqualTo(expected))
    }

[<Test>]
let ``Update User does not create a new User`` () =
    task {
        do! prepareDatabaseAsync |> withTestDbClientAsync

        let newUser: UsersStorage.User = {
            Name = None
            Token = "token"
            Password = None
        }

        Assert.ThrowsAsync<ConditionalCheckFailedException>(
            fun x -> task { do! updateUserAsync newUser CancellationToken.None |> withTestDbClientAsync }
        ) |> ignore
    }