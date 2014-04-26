namespace global

[<AutoOpen>]
module Monads =
    type ListBuilder() =
        member this.Bind(m, f) = 
            m |> List.collect f

        member this.Zero() = 
            []
        
        member this.Yield(x) = 
            [x]

        member this.YieldFrom(m) = 
            m

        member this.For(m,f:'c->'d list) =
            this.Bind(m,f)
        
        member this.Combine (a,b) = 
            List.concat [a;b]

        member this.Delay(f) = 
            f()

    type ArrayBuilder() =
        member this.Bind(m, f) = 
            m |> Array.collect f

        member this.Zero() = 
            []
        
        member this.Yield(x) = 
            [|x|]

        member this.YieldFrom(m) = 
            m

        member this.For(m,f:'c->'d array) =
            this.Bind(m,f)
        
        member this.Combine (a,b) = 
            Array.concat [a;b]

        member this.Delay(f) = 
            f()