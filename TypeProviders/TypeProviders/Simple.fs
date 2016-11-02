module TypeProviders.Simple

open ProviderImplementation.ProvidedTypes
open Microsoft.FSharp.Core.CompilerServices

[<TypeProvider>]
type SimpleProvider (config : TypeProviderConfig) as this =
    inherit TypeProviderForNamespaces ()

[<assembly:TypeProviderAssembly>]
do ()