﻿namespace Sylvester

open System
open System.Reflection
open Microsoft.FSharp.Reflection
open Microsoft.FSharp.Quotations

open FSharp.Quotations.Patterns
open FSharp.Quotations.DerivedPatterns
open FSharp.Quotations.ExprShape
                
[<AutoOpen>]
module FsExpr =
    let sequal (l:Expr) (r:Expr) = l.ToString() = r.ToString()
    
    let sequal2 (l1:Expr) (l2:Expr) (r1:Expr) (r2:Expr) = sequal l1 r1 && sequal l2 r2
    
    let sequal3 (l1:Expr) (l2:Expr) (l3:Expr) (r1:Expr) (r2:Expr) (r3:Expr)= sequal l1 r1 && sequal l2 r2 && sequal l3 r3

    let src expr = decompile expr
        
    let body = 
        function
        | Lambda(v1, b) -> b
        | expr -> failwithf "Expression %A is not a function." (src expr)

    let traverse expr f =
        match expr with
        | ShapeVar v -> Expr.Var v
        | ShapeLambda (v, body) -> Expr.Lambda (v, f body)
        | ShapeCombination (o, exprs) -> RebuildShapeCombination (o,List.map f exprs)

    let split =
        function
        | Lambda(v, Call(None, m, l::r::[])) -> (Expr.Lambda(v, l), Expr.Lambda(v, r))
        | expr -> failwithf "Cannot split expression %A." (src expr)

    let split_left = 
        function
        | Lambda(v1, Call(_, _, l::_::[])) -> Expr.Lambda(v1, l)
        | expr -> failwithf "Cannot split expression %A." (src expr)
    
    let split_right = 
        function
        | Lambda(v1, Call(_, _, _::r::[])) -> Expr.Lambda(v1, r)
        | expr -> failwithf "Cannot split expression %A." (src expr)



    let varx<'t> = Expr.Var(Var.Global("x", typedefof<'t>))

    /// Based on: http://www.fssnip.net/bx/title/Expanding-quotations by Tomas Petricek.
    /// Expand variables and calls to methods and propery getters.
    let expand expr =
        let rec rexpand vars expr = 
          let expanded = 
            match expr with
            | WithValue(_, _, e) -> rexpand vars e
            | Call(body, MethodWithReflectedDefinition meth, args) ->
                let this = match body with Some b -> Expr.Application(meth, b) | _ -> meth
                let res = Expr.Applications(this, [ for a in args -> [a]])
                rexpand vars res
            | PropertyGet(body, PropertyGetterWithReflectedDefinition p, []) -> 
                let this = match body with Some b -> b | None -> p
                rexpand vars this
            // If the variable has an assignment, then replace it with the expression
            | ExprShape.ShapeVar v when Map.containsKey v vars -> vars.[v]    
            // Else apply rexpand recursively on all sub-expressions
            | _ -> traverse expr (rexpand vars)
          // After expanding, try reducing the expression - we can replace 'let' expressions and applications where the first argument is lambda.
          match expanded with
          | Application(ExprShape.ShapeLambda(v, body), assign)
          | Let(v, assign, body) ->
                rexpand (Map.add v (rexpand vars assign) vars) body
          | _ -> expanded

        rexpand Map.empty expr
    
    let expandReflectedDefinitionParam = 
        function
        | WithValue(v, t, e) -> (v, t, expand e)
        | _ -> failwith "Expression is not a reflected definition parameter."