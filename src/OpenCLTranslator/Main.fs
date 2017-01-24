module OpenCLTranslator.Main

open Validator  
open OpenCLTranslator.Lexer
open Yard.Generators.Common.AST
open Yard.Generators.RNGLR.Parser
open Brahma.FSharp.OpenCL.AST

let parseCLCode (code) =
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
        | Success (sppf, t, d) -> (OpenCLTranslator.Parser.translate translateArgs sppf d) :> List<List<FunDecl<Lang>>>
        | Error (pos,errs,msg,dbg,_) -> failwithf "Error: %A    %A \n %A"  pos errs msg

    let res = tree.[0]
    List.iter (fun x -> checkFunDecl x) res
    res

let funDecls = parseCLCode("__kernel void foo(__global const int *bar, uint baz) {\n int foo = 1; return; \n}")
printfn "%A" funDecls
