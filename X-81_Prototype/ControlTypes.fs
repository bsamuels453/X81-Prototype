namespace global

[<AutoOpen>]
module ControlTypes =
    open SFML.Window

    type KeyState =
        | Pressed
        | Released

    type KeyStateChange = {
        KeyState : KeyState
        Key : Keyboard.Key
    }

    type MouseState = {
        LeftPressed : bool
        RightPressed : bool
        MiddlePressed : bool
        ScreenPosition : Vec2<px>
        WorldPosition : Vec2<m>
    }