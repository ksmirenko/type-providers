module OpenCLTranslator.Main

open OpenCLTranslator.Lexer
open Yard.Generators.Common.AST
open Yard.Generators.RNGLR.Parser
open Brahma.FSharp.OpenCL.AST
open System.Collections.Generic

let execute (code) =
    let lexbuf = Microsoft.FSharp.Text.Lexing.LexBuffer<_>.FromString code
    let allTokens =
        seq
            {
                while not lexbuf.IsPastEndOfStream do yield token lexbuf
            }

    let translateArgs = {
        tokenToRange = fun x -> 0UL, 0UL
        zeroPosition = 0UL
        clearAST = false //try to make a tree (default is false assuming OK)
        filterEpsilons = true // filtering eps-cycles
    }

    let tree =
        match OpenCLTranslator.Parser.buildAst allTokens with
        | Success (sppf, t, d) -> OpenCLTranslator.Parser.translate translateArgs sppf d
        | Error (pos,errs,msg,dbg,_) -> failwithf "Error: %A    %A \n %A"  pos errs msg

    tree.[0]


let tree = execute("__kernel void foo(__global const float *bar, uint baz);")
printfn "%A" tree
