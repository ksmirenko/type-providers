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

let readKernels filename = // TODO
    let funDecls = System.IO.File.ReadAllText(filename) |> parseCLCode
    ProvidedMethod(
        "kernelName", //kerName
        [], //kerParams
        typeof<Void>,
        IsStaticMethod = true,
        InvokeCode = (fun args -> <@@ ignore() @@>))
