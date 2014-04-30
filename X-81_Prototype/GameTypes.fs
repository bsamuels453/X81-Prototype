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
        SlowingFactor: float
        MaxAngVel: float<rad/s>
        AngVelScale : float</s>
        AABBShape : Rectangle<m>
    }

    type Affiliation =
    |Red
    |Blue

    type ShipState = {
        Id : ObjectId
        Position : Vec2<m>
        Velocity : Vec2<m/s>
        RotVelocity : float<rad/s>
        Rotation : float<rad>
        Acceleration : Vec2<m/s^2>
        Destination : Vec2<m>
        AABB : Rectangle<m>
        Affiliation : Affiliation
        PlayerControlled : bool
        Attribs: ShipAttribs
    }

    type GameState = {
        GameView : GameView
        SelectedObj : ObjectId list
        Ships : ShipState list
    }
