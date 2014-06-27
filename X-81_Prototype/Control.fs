namespace global

module Control =
    open SFML.Window;
    open SFML.Graphics;
    open System;
    open System.Diagnostics;

    let getKeyState keyboardState desiredKey =
        let matchKey key keyComp =
            match keyComp with
            | {KeyStateChange.Key = skey} when skey=key -> true
            | _ -> false

        keyboardState
        |> Array.find (matchKey desiredKey)



    let pollKeyboard() =
        let arrayBuilder = new ArrayBuilder()
        let keys = Enum.GetNames(typedefof<Keyboard.Key>)
        let keyVals = Array.map (fun k -> Enum.Parse(typedefof<Keyboard.Key>, k) :?> Keyboard.Key ) keys
    
        arrayBuilder{
            for key in keyVals do
                match Keyboard.IsKeyPressed(key) with
                | true -> yield {KeyState=Pressed; Key=key}
                | _ -> yield {KeyState=Released; Key=key}
        }

    let mutable private wheelDelta = 0


    let private updateButton mousePos isCurrentlyPressed prevState =
        let onMouseDown() =
            {
                ClickCompleted = false
                DragOrigin = mousePos
                PressedTimer = Some(Stopwatch.StartNew())
                IsButtonPressed = true
                DraggedArea = Some(Rectangle<m>.fromVecs mousePos mousePos)
                DragCompleted = false
            }
        let onMouseHeldDown() =
            let area = Some(Rectangle<m>.fromVecs prevState.DragOrigin mousePos)
            match prevState.PressedTimer with
            | None -> {prevState with DraggedArea = area}
            | Some(timer) -> 
                timer.Stop()
                let elapsed = timer.Elapsed.TotalMilliseconds
                timer.Start()
                if elapsed > 300.0 then
                    { 
                        prevState with
                            PressedTimer = None
                            DraggedArea = area
                    }
                else
                    {prevState with DraggedArea = area}

        let onMouseUp() =
            let area = Some(Rectangle<m>.fromVecs prevState.DragOrigin mousePos)
            let dragTermination() =
                {
                prevState with
                    DraggedArea = area
                    IsButtonPressed = false
                    DragCompleted = true
                }
            let clickTermination() =
                {
                prevState with
                    DraggedArea = None
                    IsButtonPressed = false
                    ClickCompleted = true
                }

            match prevState.PressedTimer with
            | Some(_) -> 
                match prevState.DraggedArea with
                | Some(area) -> 
                    if area.Width <> 0.0<m> || area.Height <> 0.0<m> then dragTermination()
                    else clickTermination()
                | None -> clickTermination()
            | None -> dragTermination()


        
        let onMouseHeldUp() =
            {
                ClickCompleted = false
                DragOrigin = Vec2<m>.zero()
                DraggedArea = None
                PressedTimer = None
                IsButtonPressed = false
                DragCompleted = false
            }
    
        if isCurrentlyPressed && not prevState.IsButtonPressed then
            onMouseDown()
        elif isCurrentlyPressed && prevState.IsButtonPressed then
            onMouseHeldDown()
        elif not isCurrentlyPressed && prevState.IsButtonPressed then
            onMouseUp()
        else 
            onMouseHeldUp()

    let convertDragToClick button =
        if button.DragCompleted then
            {button with DragCompleted = false; ClickCompleted = true}
        else
            button

    let pollMouse prevMouseState win screenToWorld=
        let leftPressed = Mouse.IsButtonPressed Mouse.Button.Left
        let rightPressed = Mouse.IsButtonPressed Mouse.Button.Right
        let middlePressed = Mouse.IsButtonPressed Mouse.Button.Middle
        let pos = Mouse.GetPosition win
        let vecPos:Vec2<px> = {X=float pos.X * 1.0<px>; Y=float pos.Y * 1.0<px>}
        let worldPos = screenToWorld vecPos
        let delta = wheelDelta
        wheelDelta <- 0

        let newLeft = updateButton worldPos leftPressed prevMouseState.Left
        let newRight = convertDragToClick (updateButton worldPos rightPressed prevMouseState.Right)
        let newMiddle = updateButton worldPos middlePressed prevMouseState.Middle

        {
            Left = newLeft
            Right = newRight
            Middle = newMiddle
            ScrollWheelDelta = delta
            ScreenPosition=vecPos
            WorldPosition=worldPos
        }


    let private defaultButtonState() =
        {
            ClickCompleted = false
            DragOrigin = Vec2<m>.zero()
            DraggedArea = None
            PressedTimer = None
            IsButtonPressed = false    
            DragCompleted = false    
        }

    let initMouseControl (win:RenderWindow) =
        win.MouseWheelMoved.Add (fun arg -> wheelDelta <- arg.Delta + wheelDelta)
        {
            Left = defaultButtonState()
            Right = defaultButtonState()
            Middle = defaultButtonState()
            ScrollWheelDelta = 0
            ScreenPosition={X=0.0<px>;Y=0.0<px>}
            WorldPosition={X=0.0<m>;Y=0.0<m>}
        }