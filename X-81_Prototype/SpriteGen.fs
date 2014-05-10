namespace global

module SpriteGen =
    open System;
    open SFML.Graphics;
    open SFML.Window;
    open System.Diagnostics;
    open MParticles.MGObjects;
    open MParticles.Renderers.SFML;
    open ParticleSys;

    let private extractShip gameState id =
        gameState.Ships |> List.find (fun s -> s.Id = id)

    let private updateShipSprite trackedId (sprite:Shape) gameState _ (spriteState:DrawableState)  =
        let ship:ShipState = extractShip gameState trackedId
        sprite.Position <- Vec2<m>.toVec2f ship.Position
        sprite.Rotation <- float32 (radToDeg ship.Rotation)
        spriteState

    let private createShipSprite (updateSprite) (ship:ShipState) (resources:GameResources) =
        let texture = resources.GetTexture "purple"
        let sprite = new RectangleShape(Vec2<m>.toVec2f ship.Attribs.Dimensions)
        sprite.Texture <- texture
        sprite.Position <- Vec2<m>.toVec2f ship.Position
        sprite.Rotation <- float32 (radToDeg ship.Rotation)
        sprite.Origin <- Vec2<m>.toVec2f (ship.Attribs.Dimensions /. 2.0)
        let draw state win =
            sprite.Draw (win,RenderStates.Default)
        let dispose() =
            sprite.Dispose()
        {
            Id=GameFuncs.generateObjectId(); 
            ZLayer= 1.0; 
            Update=(updateSprite sprite); 
            AutoUpdate=true; 
            Draw = draw; 
            Dispose = dispose
        }

    let private genPlayerShipSpriteState (ship:ShipState) =
        Draw.queueDrawableAddition ((createShipSprite (updateShipSprite ship.Id)) ship)

    let private genEnemyShipSpriteState (ship:ShipState) =
        Draw.queueDrawableAddition ((createShipSprite (updateShipSprite ship.Id)) ship)

    let private updateParticleSys extractShip forwardVec gameTimeRef (particleSys:SmokeParticleSystem) gameState _ spriteState =
        let ship:ShipState = extractShip gameState
        let actualForward = (Vec2.unit (Vec2.rotate forwardVec ship.Rotation)) *. 1.0<s>
        let mag = Vec2.project ship.Acceleration actualForward
        if mag > 0.0 && (Vec2<_>.length ship.Acceleration) > 0.1 then
            particleSys.SetEmitterMag(float32 mag)
            particleSys.ResumeEmission()
            particleSys.SetEmitterPos(Vec2<m>.toVector2 ship.Position)
            particleSys.SetEmissionVec (Vec2<_>.toVector2 actualForward)
            particleSys.SetBaseVelocity (Vec2<_>.toVector2 (ship.Velocity /. 2.0))
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
        {
            Id=GameFuncs.generateObjectId();
            ZLayer= 1.0;
            Update=updateSys;
            AutoUpdate=true;
            Draw = draw;
            Dispose = dispose
        }


    let private genDefaultEnginePSystems forwardVec gameTimeRef =
        let  particles = new SmokeParticleSystem(new Vector2(0.0f,0.0f), new Vector2(0.0f,0.0f))
        let update = updateParticleSys (fun gs -> gs.Ships.[0]) forwardVec gameTimeRef particles
        let createParticleSys = createParticleSys gameTimeRef update particles
        Draw.queueDrawableAddition createParticleSys
        ()

    let private updateAABB targetId (sprite:Shape) gameState _ (spriteState:DrawableState)  =
        let aabb = (extractShip gameState targetId).AABB
        sprite.Position <- Vec2<m>.toVec2f aabb.Origin
        spriteState

    let private showShipAABB ship =
        let createGhost (resources:GameResources)=
            let v = Vec2<_>.toVec2f (Rectangle.extractDims ship.AABB)
            let sprite = new RectangleShape(v)
            sprite.Position <- new Vector2f(0.0f, 0.0f)
            sprite.Rotation <- 0.0f
            sprite.Origin <- new Vector2f(0.0f, 0.0f)
            sprite.OutlineThickness <- 3.0f
            sprite.FillColor <- new Color(0uy, 0uy, 0uy, 0uy)

            let draw state win =
                sprite.Draw(win, RenderStates.Default)
            let dispose() = sprite.Dispose()

            {
                Id=GameFuncs.generateObjectId();
                ZLayer= 1.0;
                Update=(updateAABB ship.Id sprite);
                AutoUpdate=true;
                Draw = draw;
                Dispose = dispose
            }

        Draw.queueDrawableAddition (createGhost)
        ()

    let private showAABBs gameState =
        gameState.Ships |> List.map showShipAABB |> ignore

    let genTargetGhost() =
        let doDraw = ref false

        let updateGhost (sprite:RectangleShape) gameState _ drawableState =
            match gameState.SelectedShips with
            |[] -> 
                doDraw := false
            |[shipId] ->
                doDraw := true
                let ship = extractShip gameState shipId
                sprite.Position <- Vec2<_>.toVec2f (Rectangle.center ship.AABB)
            drawableState

        let createGhost (resources:GameResources) =
            let sprite = new RectangleShape(new Vector2f(50.0f, 50.0f))
            sprite.Position <- new Vector2f(0.0f, 0.0f)
            sprite.Rotation <- 0.0f
            sprite.Origin <- new Vector2f(25.0f, 25.0f)
            sprite.OutlineThickness <- 3.0f
            sprite.FillColor <- new Color(0uy, 0uy, 0uy, 0uy)
            sprite.OutlineColor <- new Color(0uy, 255uy, 0uy, 255uy)

            let draw state win =
                if !doDraw then
                    sprite.Draw(win, RenderStates.Default)
                else ()

            let dispose() = sprite.Dispose()

            {
                Id=GameFuncs.generateObjectId();
                ZLayer= 1.0;
                Update=(updateGhost sprite);
                AutoUpdate=true;
                Draw = draw;
                Dispose = dispose
            }
        Draw.queueDrawableAddition (createGhost)
        ()

    let genSelectionGhost() =
        let doDraw = ref true
        let updateGhost (sprite:RectangleShape) _ mouseState drawableState =
            match mouseState.Left.DraggedArea with
            | Some(area) ->
                doDraw := true
                sprite.Position <- Vec2<_>.toVec2f mouseState.Left.DragOrigin
                sprite.Size <- new Vector2f(float32 area.Width, float32 area.Height)
            | None -> 
                doDraw := false               
            drawableState

        let createGhost (resources:GameResources) =
            let sprite = new RectangleShape(new Vector2f(100.0f, 100.0f))
            sprite.OutlineThickness <- 3.0f
            sprite.OutlineColor <- new Color(255uy, 0uy, 0uy)
            sprite.FillColor <- new Color(0uy, 0uy, 0uy, 0uy)
            
            let draw state win =
                if !doDraw then
                    sprite.Draw(win, RenderStates.Default)
                else ()
            let dispose() = sprite.Dispose()

            {
                Id=GameFuncs.generateObjectId();
                ZLayer= 1.0;
                Update=(updateGhost sprite);
                AutoUpdate=true;
                Draw = draw;
                Dispose = dispose
            }
        Draw.queueDrawableAddition (createGhost)

    let genDefaultScene gameState gameTimeRef=
        genPlayerShipSpriteState gameState.Ships.[0]
        genEnemyShipSpriteState gameState.Ships.[1]
        //genDefaultEnginePSystems {X=0.0<m/s^2>; Y= -1.0<m/s^2>} gameTimeRef
        //genDefaultEnginePSystems {X=0.0<m/s^2>; Y= 1.0<m/s^2>} gameTimeRef
        //genDefaultEnginePSystems {X=1.0<m/s^2>; Y= 0.0<m/s^2>} gameTimeRef
        //genDefaultEnginePSystems {X= -1.0<m/s^2>; Y= 0.0<m/s^2>} gameTimeRef
        showAABBs gameState
        genTargetGhost()
        genSelectionGhost()
        ()
    ()