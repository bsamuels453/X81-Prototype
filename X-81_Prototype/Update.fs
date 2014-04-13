namespace global

module Update =
    open System;
    open SFML.Window;

    let private getShipInputLinearAccel rotation keyboardState : Vec2<m/s^2> =
        let wState = (Control.getKeyState keyboardState Keyboard.Key.W).KeyState
        let aState = (Control.getKeyState keyboardState Keyboard.Key.A).KeyState
        let sState = (Control.getKeyState keyboardState Keyboard.Key.S).KeyState
        let dState = (Control.getKeyState keyboardState Keyboard.Key.D).KeyState

        let forwardAccel = 
            match wState with 
            | KeyState.Pressed -> -Consts.linAccel
            | _ -> 0.0<m/s^2>
        let reverseAccel = 
            match sState with 
            | KeyState.Pressed -> Consts.linAccel
            | _ -> 0.0<m/s^2>
        let portAccel = 
            match aState with 
            | KeyState.Pressed -> -Consts.linAccel
            | _ -> 0.0<m/s^2>
        let starboardAccel = 
            match dState with 
            | KeyState.Pressed -> Consts.linAccel
            | _ -> 0.0<m/s^2>


        let sumForward = forwardAccel+reverseAccel
        let sumSideways = starboardAccel + portAccel

        let unit = Vec2<_>.getFromAngle rotation 1.0

        let x = unit.X * sumSideways - sumForward * unit.Y
        let y = unit.Y * sumSideways + sumForward * unit.X

        {X=x; Y=y}

    let private getShipInputRotAccel keyboardState =
        let qState = (Control.getKeyState keyboardState Keyboard.Key.Q).KeyState
        let eState = (Control.getKeyState keyboardState Keyboard.Key.E).KeyState
        let rotAccelCCW = 
            match qState with 
            | KeyState.Pressed -> -Consts.rotAccel
            | _ -> 0.0<rad/s^2>
        let rotAccelCW = 
            match eState with 
            | KeyState.Pressed -> Consts.rotAccel
            | _ -> 0.0<rad/s^2>
        let accelSum = rotAccelCCW + rotAccelCW
        accelSum

    let private applyFriction (vel:float<'u>) (accel:float<'v>) (decelQty:float<'v>) =
        if accel = 0.0<_> then
            let sign = vel / abs(vel)
            if sign <> sign then //this returns false if the float is NaN
                0.0<_>
            else
                -sign * decelQty
        else
            accel

    let private applyLinFriction vel accel =
        let decelConst = Consts.linAccel
        let frictionedX = applyFriction vel.X accel.X decelConst
        let frictionedY = applyFriction vel.Y accel.Y decelConst
        
        {X=frictionedX; Y=frictionedY}

    let clampAngle (ang:float<rad>) =
        let rec clamp ang =
            if ang >= Math.PI * 1.0<rad> then
                clamp (ang - (Math.PI * 2.0<rad>))
            elif ang <= Math.PI * -1.0<rad> then
                clamp (ang + (Math.PI * 2.0<rad>))
            else ang
        clamp ang

    let calcRotAccelFromMousepos (prevShipState:ShipState) (mouseState:MouseState) =
        let mousePos = mouseState.Position
        
        let posDiff = {X= float mousePos.X * 1.0<m>; Y= float mousePos.Y * 1.0<m>} - prevShipState.Position
        let targetAngle = Math.Atan2(float posDiff.Y, float posDiff.X) * 1.0<rad>

        Draw.addDebugLine [prevShipState.Position; {X= float mousePos.X * 1.0<m>; Y= float mousePos.Y * 1.0<m>}]
        let l = (Vec2<m>.getFromAngle prevShipState.Rotation 50.0<m>) + prevShipState.Position
        Draw.addDebugLine [l; prevShipState.Position]

        let angleDiff = clampAngle (targetAngle - prevShipState.Rotation)
        let angleDist = abs(angleDiff)
        let slowingRadius = 0.001<rad>

        let vel = 
            if angleDist < slowingRadius then
                ()
                angleDiff * 5.0<rad> * (angleDist / slowingRadius)
            else
                ()
                angleDiff * 5.0<rad>

        vel * 1.0</(rad*s)>


    let rotationTick (prevShipState:ShipState) keyboardState mouseState=
        let rotVel =
            if mouseState.RightPressed then
                calcRotAccelFromMousepos prevShipState mouseState
            else
                let rawRotAccel = getShipInputRotAccel keyboardState
                let accel = applyFriction prevShipState.RotVelocity rawRotAccel Consts.rotAccel
                prevShipState.RotVelocity + accel * (1.0<s>/60.0)

        let newShipRot = prevShipState.Rotation + rotVel * (1.0<s>/60.0)
        (newShipRot, rotVel, prevShipState.RotVelocity )

    let movementTick (prevShipState:ShipState) keyboardState mouseState=
        let (newShipRot, newShipRotVel, oldRotVel) = rotationTick prevShipState keyboardState mouseState

        let linShipAccel = applyLinFriction prevShipState.Velocity (getShipInputLinearAccel newShipRot keyboardState)

        let newShipSpeed = prevShipState.Velocity + linShipAccel * (1.0<s>/60.0)
        let newShipPos = prevShipState.Position + newShipSpeed * (1.0<s>/60.0)
        {prevShipState with Position=newShipPos; Velocity=newShipSpeed; Rotation=newShipRot; RotVelocity=newShipRotVel}


    let playerShipTick (prevShipState:ShipState) keyboardState mouseState=
        let postMovementTick = movementTick prevShipState keyboardState mouseState

        postMovementTick
    ()