namespace global

module ControlUpdate =
    open SFML.Window
    open AiTypes;
    let selectionTick mouseState gameState =
        let newSelectedShips = 
            if mouseState.Left.DragCompleted then
                let isShipWithinArea area ship =
                    Monads.condition {
                        do! Rectangle.containsVec area ship.Position
                        do! ship.PlayerControlled
                        return true
                        }
                match mouseState.Left.DraggedArea with
                    |Some(area) -> 
                        let selectedShips = gameState.Ships |> List.filter (isShipWithinArea area)
                        selectedShips |> List.map (fun s -> s.Id)
                    | None -> 
                        failwith "that shouldnt happen"; []
            elif mouseState.Left.ClickCompleted then
                let isCursorWithinArea cursor ship =
                    Monads.condition {
                        do! Rectangle.containsVec ship.AABB cursor
                        do! ship.PlayerControlled
                        return true                
                        }
                let selectedShip = gameState.Ships |> List.tryFind (isCursorWithinArea mouseState.WorldPosition)
                match selectedShip with
                | Some(ship) -> [ship.Id]
                | None -> []
            else
                gameState.SelectedShips
        {gameState with SelectedShips = newSelectedShips}


    let rightClickTrig mouseState gameState=
        let moveShipToPoint (ship:ShipState) =
            if List.exists ship.Id.Equals gameState.SelectedShips then
                let dest = MoveToPoint.construct mouseState.WorldPosition ship.Position
                Log.debug ("ship destination set to: " + string mouseState.WorldPosition.X + "," + string mouseState.WorldPosition.Y) Log.Category.Navigation
                {ship with AiMovementState = AiMicroMovementState.MovingToPoint(dest)}
            else
                ship

        let updatedShips = 
            if mouseState.Right.ClickCompleted then
                gameState.Ships |> List.map moveShipToPoint
            else
                gameState.Ships
        {gameState with Ships = updatedShips}


    let zoomTick mouseState gameState =
        if mouseState.ScrollWheelDelta <> 0 then
            let scaledView = modifyViewScale gameState.GameView ((float mouseState.ScrollWheelDelta) * -0.2<m/px>)
            
            //we only want to re-adjust center when zooming in
            if mouseState.ScrollWheelDelta > 0 then
                let xOffset = scaledView.BoundingBox.Width / 2.0
                let yOffset = scaledView.BoundingBox.Height / 2.0
                let mousePos = mouseState.WorldPosition -. {X=xOffset; Y=yOffset}

                let curCenter = scaledView.BoundingBox.Origin
                //proportional
                let newcenter = (curCenter *. 0.75) +. (mousePos *. 0.25)

                let newbb = {scaledView.BoundingBox with Origin = newcenter}
                let offsetvs = {scaledView with BoundingBox=newbb}
            
                {gameState with GameView = offsetvs}
            else
                {gameState with GameView = scaledView}
        else
            gameState

    let panTick (keyboardState:KeyStateChange array) gameState =
        let wState = Control.getKeyState keyboardState Keyboard.Key.W
        let aState = Control.getKeyState keyboardState Keyboard.Key.A
        let sState = Control.getKeyState keyboardState Keyboard.Key.S
        let dState = Control.getKeyState keyboardState Keyboard.Key.D

        let timeDelta = 1.0<s>/60.0

        let vertDelta =
            match (wState.KeyState, sState.KeyState) with
            | (KeyState.Pressed, KeyState.Pressed) -> 0.0<s>
            | (KeyState.Released, KeyState.Released) -> 0.0<s>
            | (KeyState.Pressed, KeyState.Released) -> -timeDelta
            | (KeyState.Released, KeyState.Pressed) -> timeDelta

        let horizDelta = 
            match (aState.KeyState, dState.KeyState) with
            | (KeyState.Pressed, KeyState.Pressed) -> 0.0<s>
            | (KeyState.Released, KeyState.Released) -> 0.0<s>
            | (KeyState.Pressed, KeyState.Released) -> -timeDelta
            | (KeyState.Released, KeyState.Pressed) -> timeDelta

        
        let view = gameState.GameView
        let scale = view.ViewScale

        let scaledDelta = {
            X=horizDelta * scale * Consts.horizScrollSpeed; 
            Y=vertDelta * scale * Consts.vertScrollSpeed}

        let newBB = {view.BoundingBox with Origin = view.BoundingBox.Origin +. scaledDelta}

        let newView = {view with BoundingBox = newBB}
        {gameState with GameView = newView}