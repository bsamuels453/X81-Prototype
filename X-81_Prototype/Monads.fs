namespace global

[<AutoOpen>]
module Monads =
    type State<'a, 's> = State of ('s -> 'a * 's)

    let runState (State s) a = s a
    let getState = State (fun s -> (s,s))
    let putState s = State (fun _ -> ((),s))

    type StateBuilder() =
        member this.Return(a) = 
            State (fun s -> (a,s))
        member this.Bind(m,k) =          
            State (fun s -> 
                let (a,s') = runState m s 
                runState (k a) s')
        member this.ReturnFrom (m) = m
    
    let state = StateBuilder()
    let liftState f = state {
        let! s = getState 
        return! putState (f s)
        }
        

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


    type ConditionBuilder() =
        member x.Bind(v, f) = if v then f() else false
        member x.Return(v) = v

    let condition = ConditionBuilder()