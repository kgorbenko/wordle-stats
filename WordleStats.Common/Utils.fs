module WordleStats.Common.Utils

open System.Collections.Generic

let dictToMap (dict: IDictionary<'key, 'value>): Map<'key, 'value> =
    dict
    |> Seq.map (fun x -> x.Key, x.Value)
    |> Map.ofSeq

let mapToDict (map: Map<'key, 'value>): Dictionary<'key, 'value> =
    map |> Dictionary

let partitionMap<'a, 'b, 'c> (mapper: 'a -> Choice<'b, 'c>) (items: 'a seq): 'b list * 'c list =
    Seq.foldBack (fun item (b, c) ->
        match mapper item with
        | Choice1Of2 value -> (value :: b, c)
        | Choice2Of2 value -> (b, value :: c)
    ) items ([], [])