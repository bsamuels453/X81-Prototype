﻿namespace global

module Update =
    open System;
    open SFML.Window;
    open AiTypes;

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

    let private moveToTarget (shipState:ShipState) moveDat =
        let (newShipRot, newShipRotVel) = rotationTick shipState moveDat.Dest
        let newVel = MovementPhysics.getLinearVelCeil shipState moveDat
        let (newShipPos, newShipSpeed, newAccel, newAABB) = linearTick shipState (newVel)

        Draw.addWorldDebugLine [newShipPos; moveDat.Dest]
        let l = (Vec2<m>.getFromAngle (newShipRot - Math.PI / 2.0 * 1.0<rad>) 50.0<m>) +. newShipPos
        Draw.addWorldDebugLine [l;newShipPos]

        {shipState with Position=newShipPos; Velocity=newShipSpeed; Rotation=newShipRot; RotVelocity=newShipRotVel; Acceleration=newAccel; AABB=newAABB}

    let private tickIdleShip ship =
        //not sure if this is necessary anymore with tickDeceleratingShip handling most of the decel termination
        let friction = MovementPhysics.applyLinFriction ship.Velocity ship.Acceleration
        let newShipSpeed = ship.Velocity +. friction *. 1.0<s>

        let newShipPos = ship.Position +. newShipSpeed *. (1.0<s>/60.0)

        let shipAccel = (newShipSpeed -. ship.Velocity) *. (1.0/60.0<s>)
        let newAABBpos = ship.Attribs.AABBShape +. newShipPos
        
        {ship with Position=newShipPos; Velocity=newShipSpeed; Acceleration=shipAccel; AABB=newAABBpos}

    let private tickDeceleratingShip ship =
        let newvel = ship.Velocity /. 2.0
        {ship with Velocity = newvel}

    let private aiMovementTick ship =
        let tickMovementState ship =
            match ship.AiMovementState with
                | AiMicroMovementState.Idle -> tickIdleShip ship
                | AiMicroMovementState.MovingToPoint(destDat) -> moveToTarget ship destDat
                | AiMicroMovementState.DeceleratingToStop -> tickDeceleratingShip ship
                | _ -> failwith "not supported"

        let cleanupMovementState ship =
            match ship.AiMovementState with
            | AiMicroMovementState.MovingToPoint(dest) -> 
                if Vec2.distance dest.Dest ship.Position < 50.0<m> && (Vec2<m/s>.length ship.Velocity) < 50.0<m/s> then
                    Log.debug "deceleration state entered" Log.Category.AI
                    {ship with AiMovementState = AiMicroMovementState.DeceleratingToStop}
                else
                    ship
            | _ -> ship

        let tick = Monads.state{
            do! Monads.liftState tickMovementState
            do! Monads.liftState cleanupMovementState
            }
        snd (Monads.runState tick ship)

    let updateShipAis gameState =
        let movementTick ship =
            match ship.PlayerControlled with
            | true -> aiMovementTick ship
            | false -> ship
        let postAiTick = gameState.Ships |> List.map movementTick

        {gameState with Ships=postAiTick}