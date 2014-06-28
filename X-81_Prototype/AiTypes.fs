namespace global

[<AutoOpen>]
module AiTypes =

    type MoveToPoint = {
        Dest : Vec2<m>
        SlowingRadius : float<m>
    }
        with
            static member construct dest shipPos =
                let dist = Vec2<m>.distance dest shipPos
                {Dest = dest; SlowingRadius = dist * 0.3}

    type AiMovementState =
        | Idle
        | MovingToPoint of MoveToPoint
        | KeepingShipAtRange of ObjectId * float<m>
        | DeceleratingToStop

    type AiCombatState =
        | Idle
        | AttackingEnemShip of ObjectId

    type AiSelfControlState =
        | Passive
        | Aggressive
        | Defensive