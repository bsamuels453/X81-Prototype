namespace global

[<AutoOpen>]
module StateTypes =
    open AiTypes;
    type ShipState = {
        Id : ObjectId
        Position : Vec2<m>
        Velocity : Vec2<m/s>
        RotVelocity : float<rad/s>
        Rotation : float<rad>
        Acceleration : Vec2<m/s^2>
        AiMovementState : AiMicroMovementState
        AiCombatState : AiMicroCombatState
        AABB : Rectangle<m>
        Affiliation : Affiliation
        PlayerControlled : bool
        Attribs: ShipAttribs
    }

    type GameState = {
        GameView : GameView
        SelectedShips : ObjectId list
        Ships : ShipState list
        ActiveProjectiles : Projectile list
    }