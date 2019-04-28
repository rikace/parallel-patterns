module Tests


open Expecto
open FunConcurrency

[<Tests>]
let tests =
  testList "samples" [
    testCase "Say nothing" <| fun _ ->

      Expect.equal () () "Not an absolute unit"
    testCase "Say hello all" <| fun _ ->

      Expect.equal "Hello all" "Hello all" "You didn't say hello"
  ]
