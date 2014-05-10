namespace global

module MovementPhysics =
    open System;
    open SFML.Window;

    let private calcVelCeil (curVel:float<m/s>) shipAttribs =

        let ceilCandid = (((curVel * (shipAttribs.AccelerationFactor - 1.0)) + shipAttribs.MaxVel)/shipAttribs.AccelerationFactor)
        if ceilCandid < shipAttribs.VelBoost then
            shipAttribs.VelBoost
        else
            ceilCandid

    let getLinearVelCeil ship moveDat : Vec2<m/s> =
        let velocity = ship.Velocity
        let rotation = ship.Rotation
        let shipAttribs = ship.Attribs
        let distToDest = Vec2.distance ship.Position moveDat.Dest

        let forwardUnit = Vec2.getFromAngle (rotation) 1.0
        let sidewaysUnit = Vec2.getFromAngle (rotation-1.55<rad>) 1.0
        let sidewaysProjection = velocity.X*sidewaysUnit.X + velocity.Y*sidewaysUnit.Y
        let forwardProjection = velocity.X*forwardUnit.X + velocity.Y*forwardUnit.Y

        let forwardAccel = 
            if distToDest > moveDat.SlowingRadius then 
                -calcVelCeil sidewaysProjection shipAttribs
            else
                (-calcVelCeil sidewaysProjection shipAttribs) * (distToDest / moveDat.SlowingRadius)
        let reverseAccel = 0.0<m/s>
        let portAccel = 0.0<m/s>
        let starboardAccel = 0.0<m/s>

        let sumForward = forwardAccel + reverseAccel
        let sumSideways = starboardAccel + portAccel
        
        let x = forwardUnit.X * sumSideways - sumForward * forwardUnit.Y
        let y = forwardUnit.Y * sumSideways + sumForward * forwardUnit.X

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
                if abs(vel * (1.0/60.0)) < decelQty * 1.0<s> then
                    -sign * abs(vel) * 1.0</s>
                else
                    -sign * decelQty
        else
            accel

    let applyLinFriction vel accel : Vec2<m/s^2>=
        let decelConst = Consts.linAccel * 10.0
        let frictionedX = applyFriction vel.X accel.X decelConst
        let frictionedY = applyFriction vel.Y accel.Y decelConst
        
        {X=frictionedX; Y=frictionedY}

    let private clampAngle (ang:float<rad>) =
        let rec clamp ang =
            if ang >= Math.PI * 1.0<rad> then
                clamp (ang - (Math.PI * 2.0<rad>))
            elif ang < Math.PI * -1.0<rad> then
                clamp (ang + (Math.PI * 2.0<rad>))
            else ang
        clamp ang

    let calcRotVelFromTargAngle (prevShipState:ShipState) target =        
        let posDiff = target -. prevShipState.Position
        let targetAngle = Math.Atan2(float posDiff.Y, float posDiff.X) * 1.0<rad> + Math.PI / 2.0 * 1.0<rad>

        let angleDiff = clampAngle (targetAngle - prevShipState.Rotation)
        let angleDist = abs(angleDiff)
        let slowingRadius = 0.001<rad>

        let clampAngVel mvel vel =
            if vel > mvel then
                mvel
            elif abs(vel) > mvel then
                -mvel
            else vel

        let vel = 
            if angleDist < slowingRadius then
                let desiredVel = (angleDiff * prevShipState.Attribs.AngVelScale * (angleDist / slowingRadius))
                clampAngVel prevShipState.Attribs.MaxAngVel desiredVel
            else
                let desiredVel = (angleDiff * prevShipState.Attribs.AngVelScale)
                clampAngVel prevShipState.Attribs.MaxAngVel desiredVel
        vel