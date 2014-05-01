namespace global

module Update =
    open System;
    open SFML.Window;

    let private calcVelCeil (curVel:float<m/s>) shipAttribs =
        let ceilCandid = ((curVel * (shipAttribs.SlowingFactor - 1.0)) + shipAttribs.MaxVel)/shipAttribs.SlowingFactor
        if ceilCandid < shipAttribs.VelBoost  then
            shipAttribs.VelBoost
        else
            ceilCandid

    let private getLinearVelCeil ship dest : Vec2<m/s> =
        let velocity = ship.Velocity
        let rotation = ship.Rotation
        let shipAttribs = ship.Attribs
        (*
        let wState = (Control.getKeyState keyboardState Keyboard.Key.W).KeyState
        let aState = (Control.getKeyState keyboardState Keyboard.Key.A).KeyState
        let sState = (Control.getKeyState keyboardState Keyboard.Key.S).KeyState
        let dState = (Control.getKeyState keyboardState Keyboard.Key.D).KeyState
        *)
        let forwardUnit = Vec2.getFromAngle (rotation) 1.0
        let sidewaysUnit = Vec2.getFromAngle (rotation-1.55<rad>) 1.0
        let sidewaysProjection = velocity.X*sidewaysUnit.X + velocity.Y*sidewaysUnit.Y
        let forwardProjection = velocity.X*forwardUnit.X + velocity.Y*forwardUnit.Y
        (*
        let forwardAccel = 
            match wState with 
            | KeyState.Pressed -> - calcVelCeil sidewaysProjection shipAttribs
            | _ -> 0.0<m/s>
        let reverseAccel = 
            match sState with 
            | KeyState.Pressed -> calcVelCeil -sidewaysProjection shipAttribs
            | _ -> 0.0<m/s>
        let portAccel = 
            match aState with 
            | KeyState.Pressed -> -calcVelCeil -forwardProjection shipAttribs
            | _ -> 0.0<m/s>
        let starboardAccel = 
            match dState with 
            | KeyState.Pressed -> calcVelCeil forwardProjection shipAttribs
            | _ -> 0.0<m/s>
        *)
        let forwardAccel = - calcVelCeil sidewaysProjection shipAttribs
        let reverseAccel = 0.0<m/s>
        let portAccel = 0.0<m/s>
        let starboardAccel = 0.0<m/s>

        let sumForward = forwardAccel+reverseAccel
        let sumSideways = starboardAccel + portAccel
        
        //let x = forwardUnit.X * sumSideways - sumForward * forwardUnit.Y
        //let y = forwardUnit.Y * sumSideways + sumForward * forwardUnit.X

        {X=sumSideways; Y=sumForward}

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

    let private applyLinFriction vel accel : Vec2<m/s^2>=
        let decelConst = Consts.linAccel * 2.0
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

    let private calcRotVelFromMousepos (prevShipState:ShipState) target =        
        let posDiff = target - prevShipState.Position
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


    let private rotationTick (shipState:ShipState) dest=
        let rotVel =
            //if mouseState.RightPressed then
            calcRotVelFromMousepos shipState dest
            (*else
                let rawRotAccel = getShipInputRotAccel keyboardState
                let accel = applyFriction prevShipState.RotVelocity rawRotAccel Consts.rotAccel
                prevShipState.RotVelocity + accel * (1.0<s>/60.0)*)

        let newShipRot = shipState.Rotation + rotVel * (1.0<s>/60.0)
        (newShipRot, rotVel)


    let private linearTick (shipState:ShipState) dest =
        let newVelCeil =  getLinearVelCeil shipState dest

        let newShipSpeed = ((shipState.Velocity * (20.0 - 1.0)) + newVelCeil)/20.0
        let newShipPos = shipState.Position + shipState.Velocity * (1.0<s>/60.0)
        let shipAccel = (newShipSpeed - shipState.Velocity) * (1.0/60.0<s>)
        let newAABBpos = shipState.Attribs.AABBShape + newShipPos
        (newShipPos, newShipSpeed, shipAccel, newAABBpos)

    let private moveToTarget (shipState:ShipState) dest =
        let (newShipRot, newShipRotVel) = rotationTick shipState dest
        let (newShipPos, newShipSpeed, newAccel, newAABB) = linearTick shipState dest

        Draw.addWorldDebugLine [newShipPos; dest]
        let l = (Vec2<m>.getFromAngle (newShipRot - Math.PI / 2.0 * 1.0<rad>) 50.0<m>) + newShipPos
        Draw.addWorldDebugLine [l;newShipPos]

        {shipState with Position=newShipPos; Velocity=newShipSpeed; Rotation=newShipRot; RotVelocity=newShipRotVel; Acceleration=newAccel; AABB=newAABB}


    let private shipAiTick ship =
        let postMovementTick = 
            match ship.AiMovementState with
                | AiMovementState.Idle -> ship
                | AiMovementState.MovingToPoint(pt) -> moveToTarget ship pt
                | _ -> failwith "not supported"
        let postCombatTick =
            match ship.AiCombatState with
                | AiCombatState.Idle -> postMovementTick
                | _ -> failwith "not supported"

        postCombatTick


    let allShipsTick curShips keyboardState mouseState=
        let tick ship =
            match ship.PlayerControlled with
            | true -> shipAiTick ship
            | false -> ship
        let newShipStates = curShips |> List.map tick

        newShipStates

    let selectionTick gameState mouseState : ObjectId list =
        if mouseState.Left.DragCompleted then
            let isShipWithinArea area ship =
                Monads.condition {
                    do! Rectangle.containsVec area ship.Position
                    do! ship.PlayerControlled
                    return true
                    }
            match mouseState.Left.DraggedArea with
                |Some(area) -> 
                    let selectedShips = gameState.Ships |> List.filter (isShipWithinArea area)
                    selectedShips |> List.map (fun s -> s.Id)
                | None -> 
                    failwith "that shouldnt happen"; []
        elif mouseState.Left.ClickCompleted then
            let isCursorWithinArea cursor ship =
                Monads.condition {
                    do! Rectangle.containsVec ship.AABB cursor
                    do! ship.PlayerControlled
                    return true                
                    }
            let selectedShip = gameState.Ships |> List.tryFind (isCursorWithinArea mouseState.WorldPosition)
            match selectedShip with
            | Some(ship) -> [ship.Id]
            | None -> []
        else
            gameState.SelectedShips


    let zoomTick gameState mouseState =
        if mouseState.ScrollWheelDelta <> 0 then
            modifyViewScale gameState.GameView ((float mouseState.ScrollWheelDelta) * -0.2<m/px>)
        else
            gameState.GameView

