﻿module IronJS.Ast.Types

(*Imports*)
open IronJS
open IronJS.Utils
open Antlr.Runtime.Tree
open System.Diagnostics

(*Types*)
[<DebuggerDisplay("{GetType()}")>]
type ClosureAccess =
  | None
  | Read
  | Write

[<DebuggerDisplay("clos:{ClosureAccess.Tag}/{ParamIndex}/as:{UsedAs}/{UsedWith}/def:{InitDefault}")>]
type Local = {
  ClosureAccess: ClosureAccess
  ParamIndex: int
  UsedAs: Types.JsTypes
  UsedWith: string Set
  InitUndefined: bool
  Expr: EtParam
} with
  member self.IsClosedOver with get() = not (self.ClosureAccess = ClosureAccess.None)
  member self.IsParameter  with get() = self.ParamIndex > -1
  
[<DebuggerDisplay("Closure:{Index}")>]
type Closure = {
  Index: int
}

type CallingConvention =
  | Dynamic = 0
  | Static = 1

type Scope = {
  Locals: Map<string, Local>
  Closure: Map<string, Closure>
  Arguments: bool
  CallingConvention: CallingConvention
}

type Node =
  //Constants
  | String of string
  | Number of double
  | Pass
  | Null

  //Variables
  | Local of string
  | Closure of string
  | Global of string

  //Magic
  | Arguments
  | This
  
  //
  | Block of Node list
  | Function of Scope * Node
  | Invoke of Node * Node list
  | Assign of Node * Node
  | Return of Node

//Type Aliases
type internal Scopes = Scope list ref
type internal Generator = CommonTree -> Scopes -> Node
type internal GeneratorMap = Map<int, CommonTree -> Scopes -> Generator -> Node>
type internal LocalMap = Map<string, Local>

//Constants
let internal newScope = { 
  Locals = Map.empty
  Closure = Map.empty
  Arguments = false
  CallingConvention = CallingConvention.Static
}

let internal newLocal = {
  ClosureAccess = None
  ParamIndex = -1
  UsedAs = Types.JsTypes.None
  UsedWith = Set.empty
  InitUndefined = false
  Expr = null
}