#time "on"
// must be built prior to launching or even editing this file in VS
#r @"../../TypeProviders/TypeProviders/bin/Release/TypeProviders.dll"

(*open TypeProviders.Simple.Provided

// test static property
printfn "%A\n" SimpleType.Hello

// test parameterless constructor
let simple = SimpleType()
printfn "1:\t%A\n" simple

// test parameterized constructor and non-static property
let simple1 = SimpleType "Gaben"
printfn "2:\t%A\n" simple1
printfn "\t%A\n" simple1.Info
*)