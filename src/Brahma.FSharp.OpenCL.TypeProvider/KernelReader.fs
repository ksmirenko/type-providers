// Copyright (c) 2017 Kirill Smirenko <k.smirenko@gmail.com>
// All rights reserved.
//
// The contents of this file are made available under the terms of the
// Eclipse Public License v1.0 (the "License") which accompanies this
// distribution, and is available at the following URL:
// http://www.opensource.org/licenses/eclipse-1.0.php
//
// Software distributed under the License is distributed on an "AS IS" basis,
// WITHOUT WARRANTY OF ANY KIND, either expressed or implied. See the License for
// the specific language governing rights and limitations under the License.
//
// By using this software in any fashion, you are agreeing to be bound by the
// terms of the License.

module Brahma.FSharp.OpenCL.TypeProvider.KernelReader

open Brahma.FSharp.OpenCL.AST
open Brahma.FSharp.OpenCL.OpenCLTranslator.Main
open ProviderImplementation.ProvidedTypes
open System

let dummyParseKernel filePath =
    let parseId _id =
        match _id with
        | "int" | "float" -> failwithf "%s is not a valid identifier" _id
        | _ -> _id
    let parseType _type =
        match _type with
        | "int"     -> typeof<int>
        | "float"   -> typeof<float>
        | _         -> failwithf "%s is not a valid type" _type
    let rec foldParams outList inList =
        match inList with
        | [] -> outList |> List.rev
        | _id::_type::rest ->
            let id' = parseId _id
            let type' = parseType _type
            foldParams (ProvidedParameter(id', type')::outList) rest
        | _ -> failwith "foldParams failed"
    match System.IO.File.ReadLines(filePath) |> Seq.toList with
    | kernelName::paramLines -> kernelName, (foldParams [] paramLines)
    | _ -> failwith "couldn't parse kernel"

let parsePrimitiveType pType =
    match pType with
    | Bool          -> typeof<bool>
    | Char          -> typeof<sbyte>
    | UChar         -> typeof<byte>
    | Short         -> typeof<int16>
    | UShort        -> typeof<uint16>
    | Int           -> typeof<int>
    | UInt          -> typeof<uint32>
    | Long          -> typeof<int64>
    | ULong         -> typeof<uint64>
    | Float         -> typeof<single>
    | Double        -> typeof<double>
    | Void          -> typeof<unit>
    | Half          -> failwith "Half type is not supported"
    | TypeName _    -> failwith "Custom types are not supported"

let buildProvidedMethod (funDecl:FunDecl<Lang>) =
    let buildProvidedParameter (funFormalArg:FunFormalArg<Lang>) =
        let rec parseType (t:Type<Lang>) =
            match t with
            | :? PrimitiveType<Lang> as t ->
                parsePrimitiveType t.Type
            | :? ArrayType<Lang> as t ->
                (parseType t.BaseType).MakeGenericType()
            | :? StructType<Lang> as t ->
                // TODO
                failwith "Not implemented yet"
            | :? RefType<Lang> as t ->
                (parseType t.BaseType).MakePointerType()
            | _ -> failwithf "Unknown type: %A" t
        let parsedType =
            match funFormalArg.DeclSpecs.Type with
            | Some t -> parseType t
            | None -> failwithf "Couldn't parse parameter %s: type field was empty" funFormalArg.Name
        ProvidedParameter(funFormalArg.Name, parsedType)
    ProvidedMethod(
        funDecl.Name,
        funDecl.Args |> List.map buildProvidedParameter,
        typeof<Void>, // all kernels have void return type
        IsStaticMethod = true,
        InvokeCode = (fun args -> <@@ ignore() @@>))

let readKernels filename =
    let isKernelFun (funDecl:FunDecl<Lang>) =
        match funDecl.DeclSpecs.FunQual with
        | Some Kernel -> true
        | None -> false
    System.IO.File.ReadAllText(filename)
    |> parseCLCode
    |> List.filter isKernelFun
    |> List.map buildProvidedMethod
