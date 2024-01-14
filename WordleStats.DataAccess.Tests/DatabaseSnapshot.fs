namespace WordleStats.DataAccess.Tests.DatabaseSnapshot

type DatabaseSnapshot = {
    Users: User list
}

and User = {
    mutable Id: int option
    Name: string
}

[<RequireQualifiedAccess>]
module Snapshot =

    let create (): DatabaseSnapshot =
        { Users = [] }

    let createUser (name: string): User =
        {
            Id = None
            Name = name
        }

    let attachUser (user: User) (snapshot: DatabaseSnapshot): DatabaseSnapshot =
        { Users = snapshot.Users @ [user] }

    let attachUsers (users: User list) (snapshot: DatabaseSnapshot): DatabaseSnapshot =
        { Users = snapshot.Users @ users }