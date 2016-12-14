module TypeProviders.KernelProvider

open ProviderImplementation.ProvidedTypes
open Microsoft.FSharp.Core.CompilerServices
open System
open System.Reflection

// A dummy parser
let parseKernel filePath =
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

[<TypeProvider>]
type KernelProvider(config : TypeProviderConfig) as this =
    inherit TypeProviderForNamespaces ()

    let nspace = "TypeProviders.KernelProvider.Provided"
    let assembly = Assembly.GetExecutingAssembly()

    let kernelProvider = ProvidedTypeDefinition(assembly, nspace, "KernelProvider", Some(typeof<obj>))

    let parameters = [ProvidedStaticParameter("PathToFile", typeof<string>)]

    do kernelProvider.DefineStaticParameters(parameters, fun typeName args ->
        let filePath = args.[0] :?> string

        let retProvider = ProvidedTypeDefinition(assembly, nspace, typeName, Some typeof<obj>, HideObjectMethods = true)

        let kernelName, kernelParams = parseKernel filePath

        // Provide a method parametetized exactly as the kernel
        let addKernelMethod kerName kerParams =
            let kernelMethod =
                ProvidedMethod(
                    kerName,
                    kerParams,
                    typeof<Void>,
                    IsStaticMethod = true,
                    InvokeCode = (fun args -> <@@ ignore() @@>))
            retProvider.AddMember kernelMethod

        addKernelMethod kernelName kernelParams

        retProvider
    )

    do
        this.AddNamespace(nspace, [kernelProvider])

[<assembly:TypeProviderAssembly>]
do ()
