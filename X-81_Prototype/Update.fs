namespace global

module Update =
    open System;
    open SFML.Window;

    let private rotationTick (shipState:ShipState) dest=
        let rotVel = MovementPhysics.calcRotVelFromTargAngle shipState dest

        let newShipRot = shipState.Rotation + rotVel * (1.0<s>/60.0)
        (newShipRot, rotVel)


    let private linearTick (shipState:ShipState) newVel =
        let newShipSpeed : Vec2<m/s> = ((shipState.Velocity *. (20.0 - 1.0)) +. newVel) /. 20.0
        let newShipPos = shipState.Position +. newShipSpeed *. (1.0<s>/60.0)

        let shipAccel = (newShipSpeed -. shipState.Velocity) *. (1.0/60.0<s>)
        let newAABBpos = shipState.Attribs.AABBShape +. newShipPos
        (newShipPos, newShipSpeed, shipAccel, newAABBpos)

    let private moveToTarget (shipState:ShipState) dest =
        let (newShipRot, newShipRotVel) = rotationTick shipState dest
        let newVel = MovementPhysics.getLinearVelCeil shipState (dest)
        let (newShipPos, newShipSpeed, newAccel, newAABB) = linearTick shipState (newVel)

        Draw.addWorldDebugLine [newShipPos; dest]
        let l = (Vec2<m>.getFromAngle (newShipRot - Math.PI / 2.0 * 1.0<rad>) 50.0<m>) +. newShipPos
        Draw.addWorldDebugLine [l;newShipPos]

        {shipState with Position=newShipPos; Velocity=newShipSpeed; Rotation=newShipRot; RotVelocity=newShipRotVel; Acceleration=newAccel; AABB=newAABB}

    let private tickIdleShip ship =
        let friction = MovementPhysics.applyLinFriction ship.Velocity ship.Acceleration
        let newShipSpeed = ship.Velocity +. friction *. 1.0<s>

        let newShipPos = ship.Position +. newShipSpeed *. (1.0<s>/60.0)

        let shipAccel = (newShipSpeed -. ship.Velocity) *. (1.0/60.0<s>)
        let newAABBpos = ship.Attribs.AABBShape +. newShipPos
        
        {ship with Position=newShipPos; Velocity=newShipSpeed; Acceleration=shipAccel; AABB=newAABBpos}

    let private shipAiTick ship =
        let postMovementTick = 
            match ship.AiMovementState with
                | AiMovementState.Idle -> tickIdleShip ship
                | AiMovementState.MovingToPoint(pt) -> moveToTarget ship pt.Dest
                | _ -> failwith "not supported"
        let postCombatTick =
            match ship.AiCombatState with
                | AiCombatState.Idle -> postMovementTick
                | _ -> failwith "not supported"

        postCombatTick


    let allShipsTick curShips keyboardState mouseState=
        let aiTick ship =
            match ship.PlayerControlled with
            | true -> shipAiTick ship
            | false -> ship
        let postAiTick = curShips |> List.map aiTick

        let postAiCleanup = postAiTick |> List.map ( fun ship ->
            match ship.AiMovementState with
            | AiMovementState.MovingToPoint(dest) -> 
                if Vec2.distance dest.Dest ship.Position < 5.0<m> then
                    {ship with AiMovementState = AiMovementState.Idle}
                else
                    ship
            | _ -> ship
        )

        //add stop-moving-to-point handler here
        postAiCleanup