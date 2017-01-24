namespace OpenCLTranslator.Test

open NUnit.Framework
open OpenCLTranslator.Main
open Brahma.FSharp.OpenCL.AST

[<TestFixture>]
type TestClass() =
    let emptyBody = new StatementBlock<_>(new ResizeArray<_>())

    let testSuccess code res =
        let actualRes = parseCLCode code
        Assert.True(res.Equals(actualRes))

    [<Test>]
    member this.``Single parameterless kernel``() =
        let f1 = FunDecl<Lang>(
                    DeclSpecifierPack<Lang>(funQual=Kernel, typeSpec=PrimitiveType<Lang>(Void)),
                    "epicKernel",
                    [],
                    emptyBody 
                  )
        testSuccess "__kernel void epicKernel() { return; }" [f1]

    [<Test>]
    member this.``Three functions in one file``() =
        let code = "__kernel void epicKernel1() { return; }\n\
                    __kernel void epicKernel2(int bar) { return; }\n\
                    float epicFun(double baz) { return 2.0; }\n"
        let f1 = FunDecl<Lang>(
                    DeclSpecifierPack<Lang>(funQual=Kernel, typeSpec=PrimitiveType<Lang>(Void)),
                    "epicKernel1",
                    [],
                    emptyBody 
                  )
        let f2 = FunDecl<Lang>(
                    DeclSpecifierPack<Lang>(funQual=Kernel, typeSpec=PrimitiveType<Lang>(Void)),
                    "epicKernel2",
                    [FunFormalArg<Lang>(DeclSpecifierPack<Lang>(typeSpec=PrimitiveType<Lang>(Int)), "bar")],
                    emptyBody 
                  )
        let f3 = FunDecl<Lang>(
                    DeclSpecifierPack<Lang>(typeSpec=PrimitiveType<Lang>(Float)),
                    "epicFun",
                    [FunFormalArg<Lang>(DeclSpecifierPack<Lang>(typeSpec=PrimitiveType<Lang>(Double)), "baz")],
                    emptyBody 
                  )
        testSuccess code [f1; f2; f3]
