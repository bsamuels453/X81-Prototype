namespace global

module AiTypes =
    type AiId = AiId of int


    type MoveToPoint = {
        Dest : Vec2<m>
        SlowingRadius : float<m>
    }
        with
            static member construct dest shipPos =
                let dist = Vec2<m>.distance dest shipPos
                {Dest = dest; SlowingRadius = dist * 0.3}

    type AiMicroMovementState =
        | Idle
        | MovingToPoint of MoveToPoint
        | KeepingShipAtRange of ObjectId * float<m>
        | DeceleratingToStop

    type AiMicroCombatState =
        | Idle
        | AttackingEnemShip of ObjectId

    type AiMicroAggressionState =
        | Passive
        | Aggressive
        | Defensive


    type FleetFormation = 
        | Default
        | Scrambled
        | SquareGrid

    type AiMoveInstruction = {
        Id : AiId
        Target : Vec2<m>
        Formation : FleetFormation
    }

    type FleetFormationDetail = {
        Type : FleetFormation
    }

    type AiMacroMovementState =
        | Idle
        | Holding
        | MovingToPoint of FleetFormationDetail

    type AiMacroGroup = {
        Id : AiId
        Subordinates : ObjectId list
        MovementState : AiMacroMovementState
    }
        