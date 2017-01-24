namespace OpenCLTranslator.Test

open NUnit.Framework
open OpenCLTranslator.Main
open Brahma.FSharp.OpenCL.AST

[<TestFixture>]
type TestClass() =
    let emptyBody = new StatementBlock<_>(new ResizeArray<_>())

    let testSuccess code res =
        let actualRes = parseCLCode code
        Assert.AreEqual(res, actualRes)

    [<Test>]
    member this.``Single parameterless kernel``() =
        let code = "__kernel void epicKernel() { return; }"
        let f = FunDecl<Lang>(
                    DeclSpecifierPack<Lang>(funQual=Kernel, typeSpec=PrimitiveType<Lang>(Void)),
                    "epicKernel",
                    [],
                    emptyBody
                 )
        testSuccess code [f]

    [<Test>]
    member this.``Function body with inner braces``() =
        let code = "__kernel void epicKernel() {\n\
                    int ncols = 6, sum = 0;\n\
                    for (size_t j = 0; j < ncols; j++) { sum += 1; }\n\
                    return; }"
        let f = FunDecl<Lang>(
                    DeclSpecifierPack<Lang>(funQual=Kernel, typeSpec=PrimitiveType<Lang>(Void)),
                    "epicKernel",
                    [],
                    emptyBody
                 )
        testSuccess code [f]

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

    [<Test>]
    member this.``Extern kernel with pointer parameters``() =
        let code = "extern kernel void foo(int *a, char* *b);"
        let f = FunDecl<Lang>(
                    DeclSpecifierPack<Lang>(
                        funQual=Kernel,
                        storClassSpec=Extern,
                        typeSpec=PrimitiveType<Lang>(Void)
                    ),
                    "foo",
                    [
                        FunFormalArg<Lang>(
                            DeclSpecifierPack<Lang>(typeSpec=RefType<Lang>(PrimitiveType<Lang>(Int), [])),
                            "a");
                        FunFormalArg<Lang>(
                            DeclSpecifierPack<Lang>(
                                typeSpec=RefType<Lang>(RefType<Lang>(PrimitiveType<Lang>(Char), []), [])),
                            "b")
                    ],
                    emptyBody
                )
        testSuccess code [f]

    [<Test>]
    member this.``Distinct modifiers, signed/unsigned parameters``() =
        let code = "extern kernel void foo(global volatile restrict signed int *a,\
                    __constant volatile unsigned long *b);"
        let f = FunDecl<Lang>(
                    DeclSpecifierPack<Lang>(
                        funQual=Kernel,
                        storClassSpec=Extern,
                        typeSpec=PrimitiveType<Lang>(Void)
                    ),
                    "foo",
                    [
                        FunFormalArg<Lang>(
                            DeclSpecifierPack<Lang>(
                                addrSpaceQual=Global,
                                typeSpec=RefType<Lang>(PrimitiveType<Lang>(Int), []),
                                typeQuals=[ TypeQualifier.Restrict; TypeQualifier.Volatile ]
                            ),
                            "a"
                        );
                        FunFormalArg<Lang>(
                            DeclSpecifierPack<Lang>(
                                addrSpaceQual=Constant,
                                typeSpec=RefType<Lang>(PrimitiveType<Lang>(ULong), []),
                                typeQuals=[ TypeQualifier.Volatile ]
                            ),
                            "b"
                        )
                    ],
                    emptyBody
                )
        testSuccess code [f]

    [<Test>]
    member this.``Sneaky pointers, unsigned``() =
        let code = "unsigned int foo(const int *ptr_to_constant, unsigned int *const constant_ptr)\n\
                    { return *ptr_to_constant; }"
        let f = FunDecl<Lang>(
                    DeclSpecifierPack<Lang>(
                        typeSpec=PrimitiveType<Lang>(UInt)
                    ),
                    "foo",
                    [
                        FunFormalArg<Lang>(
                            DeclSpecifierPack<Lang>(
                                typeSpec=RefType<Lang>(PrimitiveType<Lang>(Int), []),
                                typeQuals=[ TypeQualifier.Const ]
                            ),
                            "ptr_to_constant"
                        );
                        FunFormalArg<Lang>(
                            DeclSpecifierPack<Lang>(
                                typeSpec=RefType<Lang>(PrimitiveType<Lang>(UInt), [ TypeQualifier.Const ]),
                                typeQuals=[]
                            ),
                            "constant_ptr"
                        )
                    ],
                    emptyBody
                )
        testSuccess code [f]
