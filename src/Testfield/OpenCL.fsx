#r @"../OpenCLProvider/bin/Debug/OpenCLProvider.dll"

open TypeProviders.KernelProvider.Provided

type testKernel = KernelProvider<"/Users/ksmirenko/Workspace/type-providers/testkernel.txt">

let kernel = testKernel()

let providedFun = kernel.myKernel
