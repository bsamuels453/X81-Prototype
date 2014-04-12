namespace global

module Matrix =
    open System;
    (*
    type Matrix(
                _m11:float, _m12:float, _m13:float, _m14:float, 
                _m21:float, _m22:float, _m23:float, _m24:float, 
                _m31:float, _m32:float, _m33:float, _m34:float, 
                _m41:float, _m42:float, _m43:float, _m44:float) =

        member this.m11 = _m11
        member this.m12 = _m12
        member this.m13 = _m13
        member this.m14 = _m14
        member this.m21 = _m21
        member this.m22 = _m22
        member this.m23 = _m23
        member this.m24 = _m24
        member this.m31 = _m31
        member this.m32 = _m32
        member this.m33 = _m33
        member this.m34 = _m34
        member this.m41 = _m41
        member this.m42 = _m42
        member this.m43 = _m43
        member this.m44 = _m44

        member this.getForward = {X= -_m31; Y= -_m32; Z= -_m33}
        member this.getBackward = {X= _m31; Y= _m32; Z= _m33}
        member this.getDown = {X= -_m21; Y= -_m22; Z= -_m23}
        member this.getUp = {X= _m21; Y= _m22; Z= _m23}
        member this.getLeft = {X= -_m11; Y= -_m12; Z= -_m13}
        member this.getRight = {X= _m11; Y= _m12; Z= _m13}
        member this.getTranslation = {X= _m41; Y= _m42; Z= _m43}
        
        static member (+) (a:Matrix, b:Matrix) = new Matrix(a.m11+b.m11, a.m12+b.m12, a.m13+b.m13, a.m14+b.m14, a.m21+b.m21, a.m22+b.m22, a.m23+b.m23, a.m24+b.m24, a.m31+b.m31, a.m32+b.m32, a.m33+b.m33, a.m34+b.m34, a.m41+b.m41, a.m42+b.m42, a.m43+b.m43, a.m44+b.m44)
    
        static member createFromAxisAngle axis angle = 
            let x = float axis.X
            let y = float axis.Y
            let z = float axis.Z
            let num2 = Math.Sin angle
            let num = Math.Cos(angle);
            let num11 = x * x;
            let num10 = y * y;
            let num9 = z * z;
            let num8 = x * y;
            let num7 = x * z;
            let num6 = y * z;
            let m = new Matrix(
                        num11 + (num * (1.0 - num11)),
                        (num8 - (num * num8)) + (num2 * z),
                        (num7 - (num * num7)) - (num2 * y),
                        0.0,
                        (num8 - (num * num8)) - (num2 * z),
                        num10 + (num * (1.0 - num10)),
                        (num6 - (num * num6)) + (num2 * x),
                        0.0,
                        (num7 - (num * num7)) + (num2 * y),
                        (num6 - (num * num6)) - (num2 * x),
                        num9 + (num * (1.0 - num9)),
                        0.0,
                        0.0,
                        0.0,
                        0.0,
                        1.0
            )
            m

            *)
    ()