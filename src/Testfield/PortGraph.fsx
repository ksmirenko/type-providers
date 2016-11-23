#time "on"
// must be built prior to launching or even editing this file in Visual Studio
#r @"../../TypeProviders/TypeProviders/bin/Release/TypeProviders.dll"

open TypeProviders.PortJsonGraph
open System

let simpleNode = Provided.SimpleNode("Simple", Guid(), "A SimpleNode instance")
let splitNode = Provided.SplitNode("Split", Guid(), "A SplitNode instance")

printfn "%A" simpleNode.InputPorts.Input
printfn "%A" splitNode.OutputPorts.Output1
printfn "%A" splitNode.OutputPorts.Output2

printfn "simpleNode:\n\tdescription: %A\n\tinput: %A" simpleNode.Description simpleNode.InputPorts.Input
