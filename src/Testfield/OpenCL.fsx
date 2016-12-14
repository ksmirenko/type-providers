#r @"../OpenCLProvider/bin/Debug/OpenCLProvider.dll"

open TypeProviders.KernelProvider.Provided

type TestKernel = KernelProvider<"testkernel.txt">

let quotation = <@@
                    let a = 5
                    let b = 2.0
                    let c = -42
                    TestKernel.MyKernel(a, b, c)
                @@>
