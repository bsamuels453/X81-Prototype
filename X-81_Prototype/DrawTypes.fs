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
        Update : (GameState -> DrawableState -> DrawableState)
        Draw : (DrawableState -> RenderWindow -> unit)
        Dispose : (unit -> unit)
    }

    type GameResources = {
        Textures : (string*Texture) list
    }
        with 
            member this.GetTexture (textureName) = 
                let matchText text comp =
                    match comp with
                    | (t,_) when t = text -> true
                    | _ -> false
                let (_,texture) =
                    this.Textures
                    |> List.find (matchText textureName)
                texture