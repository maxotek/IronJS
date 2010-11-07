﻿namespace IronJS.Native

open System
open IronJS
open IronJS.Api.Extensions
open IronJS.Utils.Patterns
open IronJS.Api.Object.Property.Extensions

module Function =

  //----------------------------------------------------------------------------
  // 15.3.2
  let private constructor' (f:IjsFunc) (_:IjsObj) (args:IjsBox array) : IjsFunc =
      let args, body = 
        if args.Length = 0 then "", ""
        else 
          let body = args.[args.Length-1] |> Api.TypeConverter.toString
          let args = 
            args 
            |> Seq.take (args.Length-1) 
            |> Seq.map Api.TypeConverter.toString
            |> String.concat ", "
          args, body

      let func = sprintf "(function(){ return function(%s){%s}; })();" args body
      let tree = Parsers.Ecma3.parseGlobalSource f.Env func
      let analyzed = Ast.Analyzers.applyDefault tree None
      let compiled = Compiler.Core.compileAsGlobal f.Env analyzed
      (compiled.DynamicInvoke(f, f.Env.Globals) |> Utils.unboxObj) :?> IjsFunc
    
  //----------------------------------------------------------------------------
  let setupConstructor (env:IjsEnv) =
    let ctor =
      (Api.HostFunction.create env
        (new Func<IjsFunc, IjsObj, IjsBox array, IjsFunc>(constructor')))
      
    ctor.ConstructorMode <- ConstructorModes.Host
    ctor.Prototype <- env.Prototypes.Function

    ctor.put(
      "prototype", 
      env.Prototypes.Function, 
      DescriptorAttrs.Immutable)

    env.Globals.put("Function", ctor)
    env.Constructors <- {env.Constructors with Function = ctor}

  //----------------------------------------------------------------------------
  // 15.3.4
  let private prototype (f:IjsFunc) _ =
    Utils.BoxedConstants.undefined
    
  //----------------------------------------------------------------------------
  // 15.3.4.2
  let toString (toString:IjsFunc) (o:IjsObj) =
    if o :? IjsFunc then
      let f = o :?> IjsFunc

      match f.Env.FunctionSourceStrings.TryGetValue f.FunctionId with
      | true, value -> value
      | _ -> "function() { [native code] }"

    else
      Api.Environment.raiseTypeError toString.Env ""
      
  //----------------------------------------------------------------------------
  // 15.3.4.3
  let apply (apply:IjsFunc) (func:IjsObj) (this:IjsObj) (args:IjsObj) : IjsBox =
    match func with
    | IsFunction f ->
      match args with
      | IsArrayOrArguments -> 
        let args =
          (fun i -> args.Methods.GetIndex.Invoke(args, uint32 i))
          |> Seq.init (int args.IndexLength)
          |> Seq.cast<obj>
          |> Array.ofSeq

        let argTypes = Utils.addInternalArgs [for a in args -> a.GetType()]
        let type' = Utils.createDelegate argTypes
        let args = Array.append [|func :> obj; this :> obj|] args

        Utils.box (f.Compiler.compile(f, type').DynamicInvoke args)

      | IsOther -> Api.Environment.raiseTypeError apply.Env ""

    | _ -> Api.Environment.raiseTypeError apply.Env ""
 
  //----------------------------------------------------------------------------
  // 15.3.4.4
  let call (_:IjsFunc) (func:IjsObj) (this:IjsObj) (args:obj array) : IjsBox =
    match func with
    | IsFunction f ->
      let argTypes = Utils.addInternalArgs [for a in args -> a.GetType()]
      let type' = Utils.createDelegate argTypes
      let args = Array.append [|func :> obj; this :> obj|] args
      Utils.box (f.Compiler.compile(f, type').DynamicInvoke args)

    | _ -> failwith "Que?"
    
  //----------------------------------------------------------------------------
  let createPrototype (env:IjsEnv) objPrototype =
    let prototype =
      (Api.HostFunction.create env
        (new Func<IjsFunc, IjsObj, IjsBox>(prototype)))

    prototype.Prototype <- objPrototype
    prototype
      
  //----------------------------------------------------------------------------
  let setupPrototype (env:IjsEnv) =
    env.Prototypes.Function.put(
      "call",
      Api.HostFunction.create 
        env (new Func<IjsFunc, IjsObj, IjsObj, obj array, IjsBox>(call)),
      DescriptorAttrs.DontEnum)

    env.Prototypes.Function.put(
      "apply",
      Api.HostFunction.create 
        env (new Func<IjsFunc, IjsObj, IjsObj, IjsObj, IjsBox>(apply)),
      DescriptorAttrs.DontEnum)
    
    env.Prototypes.Function.put(
      "toString",
      Api.HostFunction.create 
        env (new Func<IjsFunc, IjsObj, IjsStr>(toString)),
      DescriptorAttrs.DontEnum)

    env.Prototypes.Function.put(
      "constructor", 
      env.Constructors.Function,
      DescriptorAttrs.DontEnum)