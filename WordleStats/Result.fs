module WordleStats.Results

type WordleResult =
    internal
    | Unattempted
    | Failed
    | Solved of Score: int

type WorldleResult =
    internal
    | Unattempted
    | Failed
    | Solved of Score: int

type WaffleResult =
    internal
    | Unattempted
    | Failed
    | Solved of Score: int

[<RequireQualifiedAccess>]
module Wordle =

    let minScore = 1
    let maxScore = 6

    let unattempted (): WordleResult =
        WordleResult.Unattempted

    let failed (): WordleResult =
        WordleResult.Failed

    let solved (score: int): WordleResult =
        match score with
        | s when s >= minScore && s <= maxScore -> WordleResult.Solved s
        | _ -> failwith $"Incorrect Wordle score. Should be between {minScore} and {maxScore}"

[<RequireQualifiedAccess>]
module Worldle =

    let minScore = 1
    let maxScore = 6

    let unattempted (): WorldleResult =
        WorldleResult.Unattempted

    let failed (): WorldleResult =
        WorldleResult.Failed

    let solved (score: int): WorldleResult =
        match score with
        | s when s >= minScore && s <= maxScore -> WorldleResult.Solved s
        | _ -> failwith $"Incorrect Worldle score. Should be between {minScore} and {maxScore}"

[<RequireQualifiedAccess>]
module Waffle =

    let minScore = 0
    let maxScore = 5

    let unattempted (): WaffleResult =
        WaffleResult.Unattempted

    let failed (): WaffleResult =
        WaffleResult.Failed

    let solved (score: int): WaffleResult =
        match score with
        | s when s >= minScore && s <= maxScore -> WaffleResult.Solved s
        | _ -> failwith $"Incorrect Waffle score. Should be between {minScore} and {maxScore}"

let calculateTotalScore (wordle: WordleResult) (worldle: WorldleResult) (waffle: WaffleResult) =
    let wordleScore =
        match wordle with
        | WordleResult.Unattempted
        | WordleResult.Failed -> 0
        | WordleResult.Solved score -> 7 - score

    let worldleScore =
        match worldle with
        | WorldleResult.Unattempted
        | WorldleResult.Failed -> 0
        | WorldleResult.Solved score -> 7 - score

    let waffleScore =
        match waffle with
        | WaffleResult.Unattempted
        | WaffleResult.Failed -> 0
        | WaffleResult.Solved score -> score + 1

    wordleScore + worldleScore + waffleScore