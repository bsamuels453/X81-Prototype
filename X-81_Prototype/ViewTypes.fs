namespace global

[<AutoOpen>]
module ViewTypes =

    type GameView = {
        BoundingBox : Rectangle<m>
        ViewScale : float<m/px>
    }