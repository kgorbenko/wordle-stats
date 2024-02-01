namespace WordleStats.DataAccess.Tests.DatabaseSnapshot

type Snapshot = {
    Results: Result list
    Users: User list
}

and Result = {
    User: string
    Date: string
    Wordle: string option
    Worldle: string option
    Waffle: string option
}

and User = {
    Name: string
    Token: string
    PinCode: string option
}

[<RequireQualifiedAccess>]
module Snapshot =

    let create () =
        { Results = []; Users = [] }

    let withResults (results: Result list) (snapshot: Snapshot) =
        { snapshot with Results = snapshot.Results @ results }

    let withResult (result: Result) (snapshot: Snapshot) =
        withResults [result] snapshot

    let withUsers (users: User list) (snapshot: Snapshot) =
        { snapshot with Users = snapshot.Users @ users }

    let withUser (user: User) (snapshot: Snapshot) =
        withUsers [user] snapshot