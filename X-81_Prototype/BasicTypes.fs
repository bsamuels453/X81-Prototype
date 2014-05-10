namespace global

[<AutoOpen>]
module BasicTypes =
    open System;
    open System.Diagnostics;
    open SFML.Window
    open MParticles.MGObjects;
    
    [<Measure>] type rad
    [<Measure>] type s
    [<Measure>] type m
    [<Measure>] type deg
    [<Measure>] type px

    let radToDeg (r:float<rad>) = float (r * 57.2957795)

    [<DebuggerDisplay("x = {X} y = {Y}")>]
    type Vec2<[<Measure>] 'u> = {X : float<'u>; Y : float<'u>}
        with 
            static member ( *. ) (f : float<'i>, a) = {X=a.X*f; Y=a.Y*f}
            static member ( *. ) (a, f : float<'i>) = {X=a.X*f; Y=a.Y*f}
            static member ( /. ) (a, f : float) = {X=a.X/f; Y=a.Y/f}
            static member ( +. ) (a, b) = {X=a.X+b.X; Y=a.Y+b.Y}
            static member ( -. ) (a, b) = {X=a.X-b.X; Y=a.Y-b.Y}

            static member getFromAngle (angle:float<rad>) (length:float<'u>) =
                let xunit = float <| Math.Cos (float angle)
                let yunit = float <| Math.Sin (float angle)
                {X=xunit*length; Y=yunit*length}

            static member zero() =
                {X=0.0<_>; Y=0.0<_>}

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

            static member distance (vec1:Vec2<'u>) (vec2:Vec2<'u>) =
                let x = vec1.X - vec2.X
                let y = vec1.Y - vec2.Y
                sqrt(x*x+y*y)
    
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
            static member ( +. ) (rect, vec) = {rect with Origin=rect.Origin +. vec}
            static member ( +. ) (vec, rect) = {rect with Origin=rect.Origin +. vec}


            static member fromVecs vec1 vec2 =
                let width = abs(vec1.X - vec2.X)
                let height = abs(vec1.Y - vec2.Y)
                let originX = if vec1.X < vec2.X then vec1.X else vec2.X
                let originY = if vec1.Y < vec2.Y then vec1.Y else vec2.Y
                {Origin={X=originX;Y=originY}; Width=width; Height=height}

            static member containsVec (rect:Rectangle<'u>) (vec2:Vec2<'u>) =
                ((((rect.Origin.X <= vec2.X) && (vec2.X < (rect.Origin.X + rect.Width))) && (rect.Origin.Y <= vec2.Y)) && (vec2.Y < (rect.Origin.Y + rect.Height)));

            static member contains (rect:Rectangle<'u>) (x:float<'u>) (y:float<'u>) =
                ((((rect.Origin.X <= x) && (x < (rect.Origin.X + rect.Width))) && (rect.Origin.Y <= y)) && (y < (rect.Origin.Y + rect.Height)));

            static member offsetVec (rect:Rectangle<'u>) (offset:Vec2<'u>) =
                {rect with Origin={X=rect.Origin.X + offset.X; Y=rect.Origin.Y + offset.Y}}

            static member offset (rect:Rectangle<'u>) (x:float<'u>) (y:float<'u>) =
                {rect with Origin={X=rect.Origin.X + x; Y=rect.Origin.Y + y}}

            static member inflate (rect:Rectangle<'u>) (horiz:float<'u>) (verti:float<'u>) =
                {rect with 
                    Origin = {X=rect.Origin.X - horiz; Y=rect.Origin.Y - verti};
                    Width = rect.Width + horiz * 2.0
                    Height = rect.Height + verti * 2.0
                }

            static member deflate (rect:Rectangle<'u>) (horiz:float<'u>) (verti:float<'u>) =
                {rect with 
                    Origin = {X=rect.Origin.X + horiz; Y=rect.Origin.Y + verti};
                    Width = rect.Width - horiz * 2.0
                    Height = rect.Height - verti * 2.0
                }

            static member center (rect:Rectangle<'u>) =
                {X=rect.Origin.X + rect.Width/2.0; Y=rect.Origin.Y + rect.Height/2.0}

            static member extractDims (rect:Rectangle<'u>) =
                {X=rect.Width; Y=rect.Height}