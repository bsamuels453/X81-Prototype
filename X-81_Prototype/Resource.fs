namespace global

module Resource =
    open SFML.Graphics;
    open SFML.Audio;

    let loadResources () : GameResources=
        let listbuilder = new ListBuilder()
        let textures = listbuilder{
            yield "purple", new Texture("purple.jpg")
        }
        {Textures=textures}
    ()