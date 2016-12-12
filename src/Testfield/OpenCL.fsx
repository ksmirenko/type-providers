#r @"../OpenCLProvider/bin/Debug/OpenCLProvider.dll"

open TypeProviders.KernelProvider.Provided

type TestKernel = KernelProvider<"/Users/ksmirenko/Workspace/type-providers/testkernel.txt">

let providedFun = TestKernel.MyKernel
