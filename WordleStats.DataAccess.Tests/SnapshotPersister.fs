[<RequireQualifiedAccess>]
module WordleStats.DataAccess.Tests.SnapshotPersister

open System.Data
open System.Threading.Tasks

open WordleStats.DataAccess.Tests.DatabaseLayer
open WordleStats.DataAccess.Tests.DatabaseSnapshot

let ensureTransient =
    function
    | Some id -> failwith $"Expected transient entity, but entity had an Id = {id}"
    | None -> ()

let persistAsync
    (snapshot: DatabaseSnapshot)
    (connection: IDbConnection)
    (transaction: IDbTransaction)
    : unit Task =
    task {
        for user in snapshot.Users do
            let! insertedId = insertUserAsync user connection transaction
            user.Id <- Some insertedId
    }