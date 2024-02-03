module WordleStats.PasswordHash

open System
open System.Security.Cryptography
open System.Text
open System.Threading.Tasks
open Konscious.Security.Cryptography

let private saltLength = 16
let private hashLength = 64

type HashedPassword = {
    Hash: string
    Salt: string;
}

let private hashAsync (password: string) (saltBytes: byte array) (hashLength: int): string Task =
    task {
        let passwordBytes = Encoding.UTF8.GetBytes password

        use argon2 = new Argon2id(passwordBytes)
        argon2.Salt <- saltBytes
        argon2.DegreeOfParallelism <- 1
        argon2.Iterations <- 2
        argon2.MemorySize <- 2024

        let! hashBytes = argon2.GetBytesAsync hashLength
        return Convert.ToBase64String hashBytes
    }

let generateAsync (password: string): HashedPassword Task =
    task {
        let generateSalt saltLength =
            RandomNumberGenerator.GetBytes(saltLength)

        let saltBytes = generateSalt saltLength
        let! hash = hashAsync password saltBytes hashLength

        let salt = Convert.ToBase64String saltBytes

        return
            { Hash = hash
              Salt = salt }
    }

let verifyAsync (password: string) (currentHash: HashedPassword): bool Task =
    task {
        let saltBytes = Convert.FromBase64String currentHash.Salt
        let! hash = hashAsync password saltBytes hashLength

        return currentHash.Hash = hash
    }