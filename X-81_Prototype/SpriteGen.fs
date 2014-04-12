namespace global

module SpriteGen =
    open SFML.Graphics;
    open SFML.Window;
    open System.Diagnostics;

    let private updateShipSprite extractShip renderState gameState (sprite:SpriteState) =
        let ship:ShipState = extractShip gameState
        sprite.Sprite.Position <- ship.Position.toVec2f()
        sprite.Sprite.Rotation <- float32 (radToDeg ship.Rotation)
        sprite

    let private createShipSprite (updateSprite) (ship:ShipState) (resources:GameResources) =
        let texture = resources.GetTexture "purple"
        let sprite = new RectangleShape(ship.Dimensions.toVec2f())
        sprite.Texture <- texture
        sprite.Position <- ship.Position.toVec2f()
        sprite.Rotation <- float32 (radToDeg ship.Rotation)
        sprite.Origin <- (ship.Dimensions/2.0).toVec2f()
        {Sprite=sprite; Id=ship.Id; ZLayer= 1.0; Update=updateSprite; AutoUpdate=true}

    let private genPlayerShipSpriteState ship =
        let updateShipSprite = updateShipSprite (fun gs -> gs.PlayerShip)
        let createShipSprite = (createShipSprite updateShipSprite) ship
        Draw.queueSpriteAddition createShipSprite

    let private genEnemyShipSpriteState ship =
        let updateShipSprite = updateShipSprite (fun gs -> gs.EnemyShip)
        let createShipSprite = (createShipSprite updateShipSprite) ship
        Draw.queueSpriteAddition createShipSprite

    let genDefaultScene gameState =
        genPlayerShipSpriteState gameState.PlayerShip
        genEnemyShipSpriteState gameState.EnemyShip
        ()
    ()