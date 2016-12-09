module TypeProviders.KernelProvider

open ProviderImplementation.ProvidedTypes
open Microsoft.FSharp.Core.CompilerServices
open System
open System.Reflection

[<TypeProvider>]
type KernelProvider(config : TypeProviderConfig) as this =
    inherit TypeProviderForNamespaces ()

    let nspace = "TypeProviders.KernelProvider.Provided"
    let assembly = Assembly.GetExecutingAssembly()

    let kernelProvider = ProvidedTypeDefinition(assembly, nspace, "KernelProvider", Some(typeof<obj>))

    let parameters = [ProvidedStaticParameter("PathToFile", typeof<string>)]

    do kernelProvider.DefineStaticParameters(parameters, fun typeName args ->
        let pathToFile = args.[0] :?> string

        let provider = ProvidedTypeDefinition(assembly, nspace, typeName, Some typeof<obj>, HideObjectMethods = true)

        // TODO: parse code file, provide a method

        provider
    )

    do
        this.AddNamespace(nspace, [kernelProvider])

[<assembly:TypeProviderAssembly>]
do ()
