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
        | Irrelevant
        | Scrambled
        | SquareGrid

    type AiMoveInstruction = {
        Id : AiId
        Target : Vec2<m>
        DestFormationType : FleetFormation
    }

    type AiInstructions =
    | AiMoveInstruction of AiMoveInstruction

    type FleetFormationDetail = {
        DestFormationType : FleetFormation
        ShipDestLookup : (ObjectId * Vec2<m>) list
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
        