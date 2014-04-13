namespace global

open GameTypes;

module Initialize =
    open SFML.Window;
    open SFML.Graphics;

    let initWindow() =
        let win = new RenderWindow(new VideoMode(uint32 Consts.screenWidth,uint32 Consts.screenHeight), "X-81 Prototype")
        win.Closed.Add (fun _ -> win.Close())
        win.SetKeyRepeatEnabled false
        win

    let genPlayerShip() : ShipState=
        {
            Id = GameFuncs.generateObjectId(); 
            Position = {X=350.0<m>; Y=350.0<m>}
            Velocity = {X=0.0<m/s>; Y=0.0<m/s>}
            Rotation = 0.0<rad>
            Dimensions = {X=10.0<m>; Y=20.0<m>}
            RotVelocity = 0.0<rad/s>
        }

    let genEnemyShip() : ShipState=
        {
            Id = GameFuncs.generateObjectId()
            Position = {X=200.0<m>; Y=50.0<m>}
            Velocity = {X=0.0<m/s>; Y=0.0<m/s>}
            Rotation = 0.0<rad>
            Dimensions = {X=10.0<m>; Y=10.0<m>}
            RotVelocity = 0.0<rad/s>
        }

    let genDefaultGameState() =
        {
            PlayerShip = genPlayerShip()
            EnemyShip = genEnemyShip()
            Projectiles = []
        }
        
    ()