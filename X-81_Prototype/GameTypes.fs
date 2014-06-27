namespace global

[<AutoOpen>]
module GameTypes =
    open System;
    open SFML.Window
    open SFML.Graphics
    open SFML.Audio;
    open System.Diagnostics;
    open MParticles;
    open MParticles.MGObjects;
    open MParticles.Renderers.SFML;

    type ObjectId = ObjectId of int

    type ShipAttribs = {
        Dimensions : Vec2<m>
        MaxVel: float<m/s>
        VelBoost: float<m/s>
        AccelerationFactor: float
        MaxAngVel: float<rad/s>
        AngVelScale : float</s>
        AABBShape : Rectangle<m>
    }

    type Affiliation =
        | Red
        | Blue

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

    type AiCombatState =
        | Idle
        | AttackingEnemShip of ObjectId

    type AiSelfControlState =
        | Passive
        | Aggressive
        | Defensive

    type Projectile = {
        DistanceCovered : float<m>
        Position : Vec2<m>
        Velocity : Vec2<m/s>
    }

    type ShipState = {
        Id : ObjectId
        Position : Vec2<m>
        Velocity : Vec2<m/s>
        RotVelocity : float<rad/s>
        Rotation : float<rad>
        Acceleration : Vec2<m/s^2>
        AiMovementState : AiMovementState
        AiCombatState : AiCombatState
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
