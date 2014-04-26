namespace global

[<AutoOpen>]
module ViewTypes =

    type GameView = {
        BoundingBox : Rectangle<m>
        HScale : float<m/px>
        VScale : float<m/px>
    }