namespace global

[<AutoOpen>]
module GameTypes =
    open System;
    open SFML.Window
    open SFML.Graphics
    open SFML.Audio;
    open System.Diagnostics;

    [<Measure>] type rad
    [<Measure>] type s
    [<Measure>] type m
    [<Measure>] type deg
    [<Measure>] type px

    let radToDeg (r:float<rad>) = float (r * 57.2957795)

    [<DebuggerDisplay("x = {X} y = {Y}")>]
    type Vec2<[<Measure>] 'u> = {X : float<'u>; Y : float<'u>}
        with 
            static member (*) (f : float<'i>, a) = {X=a.X*f; Y=a.Y*f}
            static member (*) (a, f : float<'i>) = {X=a.X*f; Y=a.Y*f}
            static member (/) (a, f : float) = {X=a.X/f; Y=a.Y/f}
            static member (+) (a, b) = {X=a.X+b.X; Y=a.Y+b.Y}
            static member (-) (a, b) = {X=a.X-b.X; Y=a.Y-b.Y}
            static member getFromAngle (angle:float<rad>) (length:float<'u>) =
                let xunit = float <| Math.Cos (float angle)
                let yunit = float <| Math.Sin (float angle)
                {X=xunit*length; Y=yunit*length}

            member this.toVec2f() = new Vector2f(float32 this.X, float32 this.Y)

            member this.length() = Math.Sqrt (float (this.X*this.X+this.Y*this.Y)) * (LanguagePrimitives.FloatWithMeasure 1.0)

            member this.toAngleLength() =
                let angle:float<rad> = (float (Math.Atan2(float this.Y, float this.X))) * 1.0<rad>
                let mag = this.length()
                (angle, mag)

    type ObjectId = ObjectId of int

    type KeyState =
        | Pressed
        | Released

    type KeyStateChange = {
        KeyState : KeyState
        Key : Keyboard.Key
    }

    type MouseState = {
        LeftPressed : bool
        RightPressed : bool
        MiddlePressed : bool
        ScreenPosition : Vec2<px>
        WorldPosition : Vec2<m>
    }

    type ShipAttribs = {
        Dimensions : Vec2<m>
        MaxVel: float<m/s>
        VelBoost: float<m/s>
        SlowingFactor: float
    }

    type ShipState = {
        Id : ObjectId
        Position : Vec2<m>
        Velocity : Vec2<m/s>
        RotVelocity : float<rad/s>
        Rotation : float<rad>
        Attribs: ShipAttribs
    }

    type ProjectileState = {
        Id : ObjectId
        Position : Vec2<m>
        Velocity : Vec2<m/s>
        Rotation : float<rad>
        Dimensions : Vec2<m>
    }

    type GameState = {
        PlayerShip : ShipState
        EnemyShip : ShipState
        Projectiles : ProjectileState list
    }

    type RenderState = {
        Sprites : SpriteState list
        View : View
    }
    and SpriteState = {
        Id : ObjectId
        ZLayer : float
        AutoUpdate : bool
        Sprite : Shape
        Update : (RenderState -> GameState -> SpriteState -> SpriteState)
    }

    type GameResources = {
        Textures : (string*Texture) list
    }
        with 
            member this.GetTexture (textureName) = 
                let matchText text comp =
                    match comp with
                    | (t,_) when t = text -> true
                    | _ -> false
                let (_,texture) =
                    this.Textures
                    |> List.find (matchText textureName)
                texture
    ()

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