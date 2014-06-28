namespace global

[<AutoOpen>]
module DrawTypes =
    open SFML.Window
    open SFML.Graphics

    type RenderState = {
        Drawables : DrawableState list
        View : View
    }
    and DrawableState = {
        Id : ObjectId
        ZLayer : float
        AutoUpdate : bool
        Update : (GameState -> MouseState -> DrawableState -> DrawableState)
        Draw : (DrawableState -> RenderWindow -> unit)
        Dispose : (unit -> unit)
    }