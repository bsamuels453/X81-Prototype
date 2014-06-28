namespace global

open GameTypes;

module Initialize =
    open SFML.Window;
    open SFML.Graphics;
    open MParticles.MGObjects;
    open System;

    let initWindow() =
        let win = new RenderWindow(new VideoMode(uint32 Consts.screenWidth,uint32 Consts.screenHeight), "X-81 Prototype")
        win.Closed.Add (fun _ -> win.Close())
        win.SetKeyRepeatEnabled false
        win

    let private defaultShipAttribs =
        let dims = {X=10.0<m>; Y=20.0<m>}
        let hypot = sqrt (dims.X*dims.X+dims.Y*dims.Y)
        {
            Dimensions = dims
            MaxVel = 600.0<m/s>
            VelBoost = 300.0<m/s>
            AccelerationFactor = 20.0
            MaxAngVel = 5.0<rad/s>
            AngVelScale = 10.0</s>
            AABBShape = {Origin={X= -hypot/2.0; Y= -hypot/2.0}; Width=hypot; Height=hypot}
        }

    let genPlayerShip s : ShipState=
        let r = new Random(s)
        let offset = {X=(float <| r.Next(0,100)-50) * 1.0<m>; Y=(float <| r.Next(0,100)-50) * 1.0<m>}
        {
            Id = GameFuncs.generateObjectId(); 
            Position = {X=0.0<m>; Y=0.0<m>} +. offset
            Velocity = {X=0.0<m/s>; Y=0.0<m/s>}
            Rotation = 0.0<rad>
            RotVelocity = 0.0<rad/s>
            Attribs = defaultShipAttribs
            Acceleration = {X=0.0<m/s^2>; Y=0.0<m/s^2>}
            AiMovementState = AiMovementState.Idle
            AiCombatState = AiCombatState.Idle
            Affiliation = Affiliation.Blue
            AABB = {X=0.0<m>; Y=0.0<m>} +. defaultShipAttribs.AABBShape
            PlayerControlled = true
        }

    let genEnemyShip() : ShipState=
        {
            Id = GameFuncs.generateObjectId()
            Position = {X=200.0<m>; Y=50.0<m>}
            Velocity = {X=0.0<m/s>; Y=0.0<m/s>}
            Rotation = 0.0<rad>
            RotVelocity = 0.0<rad/s>
            Attribs = defaultShipAttribs
            Acceleration = {X=0.0<m/s^2>; Y=0.0<m/s^2>}
            AiMovementState = AiMovementState.Idle
            AiCombatState = AiCombatState.Idle
            Affiliation = Affiliation.Red
            AABB = {X=200.0<m>; Y=50.0<m>} +. defaultShipAttribs.AABBShape
            PlayerControlled = false
        }

    let genDefaultGameState view =
        
        {
            GameView = ViewFuncs.createDefaultView()
            SelectedShips = []
            Ships = [genPlayerShip 5; genPlayerShip 6;genPlayerShip 7; genEnemyShip()]
            ActiveProjectiles = []
        }
        
    ()