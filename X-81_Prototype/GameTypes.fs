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

    type Projectile = {
        DistanceCovered : float<m>
        Position : Vec2<m>
        Velocity : Vec2<m/s>
    }
