// Copyright (C) 2013 Michael Newton
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software
// and associated documentation files (the "Software"), to deal in the Software without restriction,
// including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so,
// subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE
// AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

module TypeProviders.JsonGraph

open Microsoft.FSharp.Core.CompilerServices
open Newtonsoft.Json
open ProviderImplementation.ProvidedTypes
open System
open System.Reflection

// Classes for deserialization

type Id () =
    member val UniqueId = Guid() with get, set
    member val Name = "" with get, set

type Port () =
    member val Id = Id() with get, set
    member val Type = "" with get, set

type Node () =
    member val Id = Id() with get, set
    member val Ports = Collections.Generic.List<Port>() with get, set

// We separate input and output ports as different types
type InputPort = InputPort of Port
type OutputPort = OutputPort of Port

// A type for specific instances of a node type
type nodeInstance =
    {
        Node : Node
        InstanceId : Id
        Description : string
    }

// A "static constructor" for nodeInstance
module private NodeInstance =
    let create node name guid description =
        { Node = node; InstanceId = Id(Name = name, UniqueId = guid); Description = description }

// Deserialize the graph and give access to all nodes and ports

let private nodes = JsonConvert.DeserializeObject<seq<Node>>(IO.File.ReadAllText(@"C:\PortGraph.json"))
                    |> Seq.map (fun n -> n.Id.UniqueId.ToString(), n)
                    |> Map.ofSeq

let GetNode id = nodes.[id]

let private ports =
    nodes
    |> Map.toSeq
    |> Seq.map (fun (_, node) -> node.Ports)
    |> Seq.concat
    |> Seq.map (fun p -> p.Id.UniqueId.ToString(), p)
    |> Map.ofSeq

let GetPort id = ports.[id]


[<TypeProvider>]
type JsonGraphProvider (config : TypeProviderConfig) as this =
    inherit TypeProviderForNamespaces ()

    let ns = "TypeProviders.JsohGraph.Provided"
    let asm = Assembly.GetExecutingAssembly()

    let addInputPort (inputs : ProvidedTypeDefinition) (port : Port) =
        let port = ProvidedProperty(
                        port.Id.Name, 
                        typeof<InputPort>, 
                        GetterCode = fun args -> 
                            let id = port.Id.UniqueId.ToString()
                            <@@ GetPort id |> InputPort @@>) // note: accessing private helper method GetPort
        inputs.AddMember(port)

    let addOutputPort (outputs : ProvidedTypeDefinition) (port : Port) =
        let port = ProvidedProperty(
                        port.Id.Name, 
                        typeof<OutputPort>, 
                        GetterCode = fun args -> 
                            let id = port.Id.UniqueId.ToString()
                            <@@ GetPort id |> OutputPort @@>)
        outputs.AddMember(port)

    // adds ports from a sequence to generated type definitions
    let addPorts inputs outputs (portList : seq<Port>) =
        portList
        |> Seq.iter (fun port ->
                        match port.Type with
                        | "input" -> addInputPort inputs port
                        | "output" -> addOutputPort outputs port
                        | _ -> failwithf "Unknown port type for port %s/%s" port.Id.Name (port.Id.UniqueId.ToString()))

    let createNodeType id (node : Node) =
        let nodeType = ProvidedTypeDefinition(asm, ns, node.Id.Name, Some typeof<nodeInstance>)
        // constructor for a nodeInstance
        let ctor = ProvidedConstructor(
                    [
                        ProvidedParameter("Name", typeof<string>)
                        ProvidedParameter("UniqueId", typeof<Guid>)
                        ProvidedParameter("Description", typeof<string>)
                    ],
                    InvokeCode =
                        (fun args ->
                            match args with
                            | [name; uniqueId; descr] ->
                                <@@
                                    NodeInstance.create (GetNode id) (%%name:string) (%%uniqueId:Guid) (%%descr:string)
                                @@>
                            | _ -> failwithf "Invalid constructor arguments %A" args))
        nodeType.AddMember(ctor)

        let inputs = ProvidedTypeDefinition("Inputs", Some typeof<obj>)
        let inputCtor = ProvidedConstructor([], InvokeCode = fun args -> <@@ obj() @@>)
        inputs.AddMember(inputCtor)
        inputs.HideObjectMethods <- true

        let outputs = ProvidedTypeDefinition("Outputs", Some typeof<obj>)
        let outputCtor = ProvidedConstructor([], InvokeCode = fun args -> <@@ obj() @@>)
        outputs.AddMember(outputCtor)
        outputs.HideObjectMethods <- true

        addPorts inputs outputs node.Ports

        // Add the inputs and outputs types of nested types under the Node type
        nodeType.AddMembers([inputs; outputs])

        // Add some instance properties to expose them on a node instance
        let inputPorts = ProvidedProperty("InputPorts", inputs, [],
                            GetterCode = fun args -> <@@ obj() @@>)
        let outputPorts = ProvidedProperty("OutputPorts", outputs, [],
                            GetterCode = fun args -> <@@ obj() @@>)

        nodeType.AddMembers([inputPorts;outputPorts])

        nodeType

    let createTypes () =
        nodes |> Map.map createNodeType |> Map.toList |> List.map (fun (k, v) -> v)

    do
        this.AddNamespace(ns, createTypes())

[<assembly:TypeProviderAssembly>]
do ()
