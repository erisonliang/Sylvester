﻿namespace Sylvester.tf

open System
open System.Collections.Generic;
open System.Runtime.CompilerServices

open TensorFlow

open Sylvester
open Sylvester.Arithmetic
open Sylvester.Arithmetic.N10
open Sylvester.Collections
open Sylvester.Graphs
open Sylvester.Tensors

[<AutoOpen>]
module Vector =
    
    [<StructuredFormatDisplay("{Display}")>]
    type Vector<'dim0, 't when 'dim0 :> Number and 't:> ValueType and 't : struct and 't: (new: unit -> 't) and 't :> IEquatable<'t> and 't :> IFormattable and 't :> IComparable>(graph:ITensorGraph, name:string, head:Node, output:int) =
        inherit Tensor<one, 't>(graph, name, head, output, [|number<'dim0>.Val|])
        interface IVector
        member x.Dim0:'dim0 = number<'dim0>
        member x.Display = sprintf "Vector<%i>" x.Dim0.IntVal
        

        new(name:string) = 
            let g = defaultGraph
            let shape = [|number<'dim0>.Val|]
            new Vector<'dim0, 't>(g, g.GetName name, new Node(g, g.GetName name, tf(g).Placeholder(dataType<'t>, shape, name), []), 0)