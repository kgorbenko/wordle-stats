namespace WordleStats.DataAccess.Tests.DatabaseSnapshot

type Snapshot = {
    Results: Result list
}

and Result = {
    User: string
    Date: string
    Wordle: string option
    Worldle: string option
    Waffle: string option
}

[<RequireQualifiedAccess>]
module Snapshot =

    let create () =
        { Results = [] }

    let withResults (results: Result list) (snapshot: Snapshot) =
        { Results = snapshot.Results @ results }

    let withResult (result: Result) (snapshot: Snapshot) =
        withResults [result] snapshot