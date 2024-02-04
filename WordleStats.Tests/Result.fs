module WordleStats.Tests.Result

open System
open NUnit.Framework

open WordleStats.Results

module Wordle =

    [<Test>]
    let ``Create Wordle unattempted result`` () =
        let actual = Wordle.unattempted ()

        Assert.That(actual, Is.EqualTo(WordleResult.Unattempted))

    [<Test>]
    let ``Create Wordle failed result`` () =
        let actual = Wordle.failed ()

        Assert.That(actual, Is.EqualTo(WordleResult.Failed))

    [<Test>]
    [<TestCase(Int32.MinValue)>]
    [<TestCase(-100)>]
    [<TestCase(-1)>]
    [<TestCase(0)>]
    [<TestCase(7)>]
    [<TestCase(100)>]
    [<TestCase(Int32.MaxValue)>]
    let ``Create Wordle solved result with invalid score`` (score: int) =
        Assert.Throws<Exception>(
            fun () -> Wordle.solved score |> ignore
        ) |> ignore

    [<Test>]
    [<TestCase(1)>]
    [<TestCase(2)>]
    [<TestCase(3)>]
    [<TestCase(4)>]
    [<TestCase(5)>]
    [<TestCase(6)>]
    let ``Create Wordle solved result `` (score: int) =
        let actual = Wordle.solved score

        Assert.That(actual, Is.EqualTo(WordleResult.Solved score))

module Worldle =

    [<Test>]
    let ``Create Worldle unattempted result`` () =
        let actual = Worldle.unattempted ()

        Assert.That(actual, Is.EqualTo(WorldleResult.Unattempted))

    [<Test>]
    let ``Create Worldle failed result`` () =
        let actual = Worldle.failed ()

        Assert.That(actual, Is.EqualTo(WorldleResult.Failed))

    [<Test>]
    [<TestCase(Int32.MinValue)>]
    [<TestCase(-100)>]
    [<TestCase(-1)>]
    [<TestCase(0)>]
    [<TestCase(7)>]
    [<TestCase(100)>]
    [<TestCase(Int32.MaxValue)>]
    let ``Create Worldle solved result with invalid score`` (score: int) =
        Assert.Throws<Exception>(
            fun () -> Worldle.solved score |> ignore
        ) |> ignore

    [<Test>]
    [<TestCase(1)>]
    [<TestCase(2)>]
    [<TestCase(3)>]
    [<TestCase(4)>]
    [<TestCase(5)>]
    [<TestCase(6)>]
    let ``Create Worldle solved result `` (score: int) =
        let actual = Worldle.solved score

        Assert.That(actual, Is.EqualTo(WorldleResult.Solved score))

module Waffle =

    [<Test>]
    let ``Create Waffle unattempted result`` () =
        let actual = Waffle.unattempted ()

        Assert.That(actual, Is.EqualTo(WaffleResult.Unattempted))

    [<Test>]
    let ``Create Waffle failed result`` () =
        let actual = Waffle.failed ()

        Assert.That(actual, Is.EqualTo(WaffleResult.Failed))

    [<Test>]
    [<TestCase(Int32.MinValue)>]
    [<TestCase(-100)>]
    [<TestCase(-1)>]
    [<TestCase(6)>]
    [<TestCase(100)>]
    [<TestCase(Int32.MaxValue)>]
    let ``Create Waffle solved result with invalid score`` (score: int) =
        Assert.Throws<Exception>(
            fun () -> Waffle.solved score |> ignore
        ) |> ignore

    [<Test>]
    [<TestCase(0)>]
    [<TestCase(1)>]
    [<TestCase(2)>]
    [<TestCase(3)>]
    [<TestCase(4)>]
    [<TestCase(5)>]
    let ``Create Waffle solved result `` (score: int) =
        let actual = Waffle.solved score

        Assert.That(actual, Is.EqualTo(WaffleResult.Solved score))

[<Test>]
let ``Calculate total score all unattempted results`` () =
    let wordle = WordleResult.Unattempted
    let worldle = WorldleResult.Unattempted
    let waffle = WaffleResult.Unattempted

    let actual = calculateTotalScore wordle worldle waffle

    Assert.That(actual, Is.EqualTo(0))

[<Test>]
let ``Calculate total score all failed results`` () =
    let wordle = WordleResult.Unattempted
    let worldle = WorldleResult.Unattempted
    let waffle = WaffleResult.Unattempted

    let actual = calculateTotalScore wordle worldle waffle

    Assert.That(actual, Is.EqualTo(0))

[<Test>]
[<TestCase(1, 6)>]
[<TestCase(2, 5)>]
[<TestCase(3, 4)>]
[<TestCase(4, 3)>]
[<TestCase(5, 2)>]
[<TestCase(6, 1)>]
let ``Calculate total score only Wordle scored results`` (wordleScore: int) (totalScore: int) =
    let wordle = WordleResult.Solved wordleScore
    let worldle = WorldleResult.Unattempted
    let waffle = WaffleResult.Unattempted

    let actual = calculateTotalScore wordle worldle waffle

    Assert.That(actual, Is.EqualTo(totalScore))

[<Test>]
[<TestCase(1, 6)>]
[<TestCase(2, 5)>]
[<TestCase(3, 4)>]
[<TestCase(4, 3)>]
[<TestCase(5, 2)>]
[<TestCase(6, 1)>]
let ``Calculate total score only Worldle scored results`` (worldleScore: int) (totalScore: int) =
    let wordle = WordleResult.Unattempted
    let worldle = WorldleResult.Solved worldleScore
    let waffle = WaffleResult.Unattempted

    let actual = calculateTotalScore wordle worldle waffle

    Assert.That(actual, Is.EqualTo(totalScore))

[<Test>]
[<TestCase(0, 1)>]
[<TestCase(1, 2)>]
[<TestCase(2, 3)>]
[<TestCase(3, 4)>]
[<TestCase(4, 5)>]
[<TestCase(5, 6)>]
let ``Calculate total score only Waffle scored results`` (waffleScore: int) (totalScore: int) =
    let wordle = WordleResult.Unattempted
    let worldle = WorldleResult.Unattempted
    let waffle = WaffleResult.Solved waffleScore

    let actual = calculateTotalScore wordle worldle waffle

    Assert.That(actual, Is.EqualTo(totalScore))

[<Test>]
[<TestCase(1, 1, 5, 18)>]
[<TestCase(4, 1, 5, 15)>]
[<TestCase(1, 3, 3, 14)>]
[<TestCase(1, 5, 4, 13)>]
[<TestCase(5, 2, 2, 10)>]
[<TestCase(6, 6, 0, 3)>]
let ``Calculate total score by all games``
    (wordleScore: int)
    (worldleScore: int)
    (waffleScore: int)
    (totalScore: int) =
    let wordle = WordleResult.Solved wordleScore
    let worldle = WorldleResult.Solved worldleScore
    let waffle = WaffleResult.Solved waffleScore

    let actual = calculateTotalScore wordle worldle waffle

    Assert.That(actual, Is.EqualTo(totalScore))