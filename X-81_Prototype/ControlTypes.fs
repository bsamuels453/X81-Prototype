namespace global

[<AutoOpen>]
module ControlTypes =
    open SFML.Window
    open System.Diagnostics

    type KeyState =
        | Pressed
        | Released

    type KeyStateChange = {
        KeyState : KeyState
        Key : Keyboard.Key
    }

    type ButtonState = {
        ClickCompleted : bool
        DragOrigin : Vec2<m>
        DraggedArea : Rectangle<m> option
        DragCompleted : bool
        PressedTimer : Stopwatch option
        IsButtonPressed : bool
    }

    type MouseState = {
        Left : ButtonState
        Right : ButtonState
        Middle : ButtonState
        ScrollWheelDelta : int
        ScreenPosition : Vec2<px>
        WorldPosition : Vec2<m>
    }