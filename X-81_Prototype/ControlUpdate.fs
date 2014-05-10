namespace global

module ControlUpdate =
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


    let movementTarSelectTick mouseState gameState=
        let updatedShips = 
            if mouseState.Right.ClickCompleted then
                gameState.Ships |> List.map (
                    fun ship ->
                        if List.exists ship.Id.Equals gameState.SelectedShips then
                            let dest = MoveToPoint.construct mouseState.WorldPosition ship.Position
                            {ship with AiMovementState = AiMovementState.MovingToPoint(dest)}
                        else
                            ship
                )
            else
                gameState.Ships
        {gameState with Ships = updatedShips}


    let zoomTick mouseState gameState =
        if mouseState.ScrollWheelDelta <> 0 then
            let vs = modifyViewScale gameState.GameView ((float mouseState.ScrollWheelDelta) * -0.2<m/px>)
            {gameState with GameView = vs}
        else
            gameState