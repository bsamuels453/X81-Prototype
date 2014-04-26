namespace global

[<AutoOpen>]
module GameTypes =
    open System;
    open SFML.Window
    open SFML.Graphics
    open SFML.Audio;
    open System.Diagnostics;
    open MParticles;
    open MParticles.MGObjects;
    open MParticles.Renderers.SFML;

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

            static member fromPolar polarVec =
                let xunit = float <| Math.Cos (float polarVec.Angle)
                let yunit = float <| Math.Sin (float polarVec.Angle)
                {X=xunit*polarVec.Length; Y=yunit*polarVec.Length}

            static member toVec2f vec = new Vector2f(float32 vec.X, float32 vec.Y)

            static member toVector2 vec = new Vector2(float32 vec.X, float32 vec.Y)

            static member length vec = Math.Sqrt (float (vec.X*vec.X+vec.Y*vec.Y)) * (LanguagePrimitives.FloatWithMeasure 1.0)

            static member unit (vec:Vec2<'u>) = 
                let len = Vec2<'u>.length vec
                {X=vec.X/len; Y=vec.Y/len}

            static member toPolar (vec:Vec2<'u>) =
                let angle:float<rad> = (float (Math.Atan2(float vec.Y, float vec.X))) * 1.0<rad>
                let mag = Vec2<'u>.length vec
                {Length=mag; Angle=angle}

            static member project (src:Vec2<'u>) (dest:Vec2<'u>) =
                let numerator = src.X*dest.X + src.Y*dest.Y
                let denominator = Vec2<'u>.length src
                numerator / denominator

            static member rotate (vec:Vec2<'u>) (ang:float<rad>) =
                let polar = Vec2.toPolar vec
                let newPolar = {polar with Angle=polar.Angle + ang}
                PolVec2.toRect newPolar
    
    and [<DebuggerDisplay("Len = {Length} Ang = {Angle}")>] 
        PolVec2<[<Measure>] 'u> = {Length : float<'u>; Angle : float<rad>}
        with
            static member fromRect (vec:Vec2<'u>) =
                Vec2.toPolar vec

            static member toRect (polarVec:PolVec2<'u>) =
                Vec2<'u>.fromPolar polarVec

    [<DebuggerDisplay("Origin = {Origin} Width = {Width} Height = {Height}")>]
    type Rectangle<[<Measure>] 'u> = {Origin:Vec2<'u>; Width:float<'u>; Height:float<'u>}
        with
            static member containsVec (rect:Rectangle<'u>) (vec2:Vec2<'u>) =
                ((((rect.Origin.X <= vec2.X) && (vec2.X < (rect.Origin.X + rect.Width))) && (rect.Origin.Y <= vec2.Y)) && (vec2.Y < (rect.Origin.Y + rect.Height)));

            static member contains (rect:Rectangle<'u>) (x:float<'u>) (y:float<'u>) =
                ((((rect.Origin.X <= x) && (x < (rect.Origin.X + rect.Width))) && (rect.Origin.Y <= y)) && (y < (rect.Origin.Y + rect.Height)));

            static member offsetVec (rect:Rectangle<'u>) (offset:Vec2<'u>) =
                {rect with Origin={X=rect.Origin.X + offset.X; Y=rect.Origin.Y + offset.Y}}

            static member offset (rect:Rectangle<'u>)  (x:float<'u>) (y:float<'u>) =
                {rect with Origin={X=rect.Origin.X + x; Y=rect.Origin.Y + y}}


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
        MaxAngVel: float<rad/s>
        AngVelScale : float</s>
    }

    type ShipState = {
        Id : ObjectId
        Position : Vec2<m>
        Velocity : Vec2<m/s>
        RotVelocity : float<rad/s>
        Rotation : float<rad>
        Acceleration : Vec2<m/s^2>
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
        Drawables : DrawableState list
        View : View
    }
    and DrawableState = {
        Id : ObjectId
        ZLayer : float
        AutoUpdate : bool
        Update : (GameState -> DrawableState -> DrawableState)
        Draw : (DrawableState -> RenderWindow -> unit)
        Dispose : (unit -> unit)
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