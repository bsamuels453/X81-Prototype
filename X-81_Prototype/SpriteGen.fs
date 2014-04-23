namespace global

module SpriteGen =
    open System;
    open SFML.Graphics;
    open SFML.Window;
    open System.Diagnostics;
    open MParticles.MGObjects;
    open MParticles.Renderers.SFML;
    open ParticleSys;

    let private updateShipSprite extractShip (sprite:Shape) gameState (spriteState:DrawableState)  =
        let ship:ShipState = extractShip gameState
        sprite.Position <- Vec2<m>.toVec2f ship.Position
        sprite.Rotation <- float32 (radToDeg ship.Rotation)
        spriteState

    let private createShipSprite (updateSprite) (ship:ShipState) (resources:GameResources) =
        let texture = resources.GetTexture "purple"
        let sprite = new RectangleShape(Vec2<m>.toVec2f ship.Attribs.Dimensions)
        sprite.Texture <- texture
        sprite.Position <- Vec2<m>.toVec2f ship.Position
        sprite.Rotation <- float32 (radToDeg ship.Rotation)
        sprite.Origin <- Vec2<m>.toVec2f (ship.Attribs.Dimensions/2.0)
        let draw state win =
            sprite.Draw (win,RenderStates.Default)
        let dispose() =
            sprite.Dispose()
        {Id=ship.Id; ZLayer= 1.0; Update=(updateSprite sprite); AutoUpdate=true; Draw = draw; Dispose = dispose}

    let private genPlayerShipSpriteState ship =
        let createShipSprite = (createShipSprite (updateShipSprite (fun gs -> gs.PlayerShip))) ship
        Draw.queueDrawableAddition createShipSprite

    let private genEnemyShipSpriteState ship =
        let updateShipSprite = updateShipSprite (fun gs -> gs.EnemyShip)
        let createShipSprite = (createShipSprite updateShipSprite) ship
        Draw.queueDrawableAddition createShipSprite

    let private updateParticleSys extractShip forwardVec gameTimeRef (particleSys:SmokeParticleSystem) gameState spriteState =
        let ship:ShipState = extractShip gameState
        let actualForward = (Vec2.unit (Vec2.rotate forwardVec ship.Rotation)) * 1.0<s>
        let mag = Vec2.project ship.Acceleration actualForward
        if mag > 0.0 && (Vec2<_>.length ship.Acceleration) > 0.1 then
            particleSys.SetEmitterMag(float32 mag)
            particleSys.ResumeEmission()
            particleSys.SetEmitterPos(Vec2<m>.toVector2 ship.Position)
            particleSys.SetEmissionVec (Vec2<_>.toVector2 actualForward)
            particleSys.SetBaseVelocity (Vec2<_>.toVector2 (ship.Velocity/2.0))
            ()
        else
            particleSys.PauseEmission()
            ()
        particleSys.Update !gameTimeRef
        spriteState

    let private createParticleSys gameTimeRef (updateSys) (particleSys:SmokeParticleSystem) (resources:GameResources) =
        let draw state win =
            particleSys.Draw (win,!gameTimeRef)
        let dispose() =
            ()
        {Id=GameFuncs.generateObjectId(); ZLayer= 1.0; Update=updateSys; AutoUpdate=true; Draw = draw; Dispose = dispose}


    let private genDefaultEnginePSystems forwardVec gameTimeRef =
        let  particles = new SmokeParticleSystem(new Vector2(0.0f,0.0f), new Vector2(0.0f,0.0f))
        let update = updateParticleSys (fun gs -> gs.PlayerShip) forwardVec gameTimeRef particles
        let createParticleSys = createParticleSys gameTimeRef update particles
        Draw.queueDrawableAddition createParticleSys
        ()

    let genDefaultScene gameState gameTimeRef=
        genPlayerShipSpriteState gameState.PlayerShip
        genEnemyShipSpriteState gameState.EnemyShip
        genDefaultEnginePSystems {X=0.0<m/s^2>; Y= -1.0<m/s^2>} gameTimeRef
        genDefaultEnginePSystems {X=0.0<m/s^2>; Y= 1.0<m/s^2>} gameTimeRef
        genDefaultEnginePSystems {X=1.0<m/s^2>; Y= 0.0<m/s^2>} gameTimeRef
        genDefaultEnginePSystems {X= -1.0<m/s^2>; Y= 0.0<m/s^2>} gameTimeRef
        ()
    ()