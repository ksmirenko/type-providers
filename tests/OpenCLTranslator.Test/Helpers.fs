module OpenCLTranslator.Test.Helpers

open Brahma.FSharp.OpenCL.AST

let emptyBody = new StatementBlock<_>(new ResizeArray<_>())

/// DeclSpecifierPack with type only
let tdecl t = DeclSpecifierPack<Lang>(typeSpec=t)

let func specs name args = FunDecl<Lang>(specs, name, args, emptyBody)

let arg specs name = FunFormalArg<Lang>(specs, name)
