module WordleStats.Common.Utils

open System.Collections.Generic

let dictToMap (dict: IDictionary<'key, 'value>): Map<'key, 'value> =
    dict
    |> Seq.map (fun x -> x.Key, x.Value)
    |> Map.ofSeq

let mapToDict (map: Map<'key, 'value>): Dictionary<'key, 'value> =
    map |> Dictionary