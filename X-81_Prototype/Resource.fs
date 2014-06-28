namespace global

module Resource =
    open SFML.Graphics;
    open SFML.Audio;

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

    let loadResources () : GameResources=
        let listbuilder = new ListBuilder()
        let textures = listbuilder{
            yield "purple", new Texture("purple.jpg")
        }
        {Textures=textures}
    ()