﻿namespace Sylvester

open Sylvester.Arithmetic.N10
open Sylvester.Collections

/// Set of elements closed under some binary operation.
type Groupoid<'U when 'U: equality>(set:Set<'U>, op:'U->'U->'U) =
    inherit Struct<'U, one>(set, arrayOf1 (Binary(op)))