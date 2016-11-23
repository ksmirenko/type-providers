module TypeProviders.Simple

open ProviderImplementation.ProvidedTypes
open Microsoft.FSharp.Core.CompilerServices
open System.Reflection

[<TypeProvider>]
type SimpleProvider (config : TypeProviderConfig) as this =
    inherit TypeProviderForNamespaces ()

    let ns = "TypeProviders.Simple.Provided"
    let asm = Assembly.GetExecutingAssembly()

    let createTypes () =
        let simpleType = ProvidedTypeDefinition(asm, ns, "SimpleType", Some typeof<obj>)

        let helloProp = ProvidedProperty("Hello",
                                         typeof<string>,
                                         IsStatic = true,
                                         GetterCode = (fun args -> <@@ "Oh, hello there!" @@>))
        simpleType.AddMember(helloProp)

        // A parameterless constructor.
        // Note: we can store anything as the internal representation of an instance of our type
        //       (underlying CLR type is `object`)
        // Note: the InvokeCode parameter of the constructor must return a quotation that will
        //       return the internal representation of the object when it’s evaluated
        let ctor = ProvidedConstructor([],
                                       InvokeCode =
                                            fun args -> <@@ "SimpleType instance, which is actually a string" :> obj @@>)
        simpleType.AddMember(ctor)

        // A constructor with a parameter.
        let ctor2 = ProvidedConstructor([ProvidedParameter("Name", typeof<string>)],
                                        InvokeCode =
                                            fun args -> <@@ sprintf "SimpleType instance with arg: %s" (%%(args.[0]):string) :> obj @@>)
        simpleType.AddMember(ctor2)

        // arg[0] is the instance itself for non-static properties
        let info = ProvidedProperty("Info",
                                    typeof<string>,
                                    GetterCode = fun args -> <@@ sprintf "Your object is %A" %%(args.[0]) @@>)
        simpleType.AddMember(info)

        [simpleType]

    do
        this.AddNamespace(ns, createTypes())

[<assembly:TypeProviderAssembly>]
do ()
