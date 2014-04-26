namespace global

module Draw =
    open SFML.Window;
    open SFML.Graphics;
    open SFML.Audio;

    let mutable private drawablesToUpdate:ObjectId list = []
    let mutable private drawablesToAdd:(GameResources -> DrawableState) list = []
    let mutable private drawablesToRemove:ObjectId list = []
    let mutable private debugLinesToDraw:Vec2<m> list list = []

    let queueDrawableUpdate objectId =
        drawablesToUpdate <- List.Cons (objectId, drawablesToUpdate)

    let queueDrawableAddition spriteInitializer =
        drawablesToAdd <- List.Cons (spriteInitializer, drawablesToAdd)

    let queueDrawableDeletion objectId =
        drawablesToRemove <- List.Cons (objectId, drawablesToRemove)

    let addWorldDebugLine vertLi =
        debugLinesToDraw <- List.Cons (vertLi, debugLinesToDraw)

    let private addSprites drawableSprites spritesToAdd textures =
        let generatedSprites = spritesToAdd |> List.map (fun f -> f textures)
        generatedSprites
            |> List.append drawableSprites
            |> List.sortBy (fun elem -> elem.ZLayer)

    let private removeSprites (drawableSprites:DrawableState list) idsToRemove =
        let spritesToRemove =  drawableSprites |> List.filter (fun elem -> (List.exists (elem.Id.Equals) idsToRemove ))
        spritesToRemove |> List.map (fun s -> s.Dispose()) |> ignore
         
        drawableSprites |> List.filter (fun elem -> not (List.exists (elem.Id.Equals) idsToRemove ))

    let private updateSprites tempRenderState gameState sprites idsToUpdate=
        let rec applyUpdates (sprites:DrawableState []) updates =
            match updates with
            | [] -> ()
            | h::t ->
                let idx = sprites |> Array.findIndex (fun s -> s.Id.Equals h)
                let sprite = sprites.[idx]
                sprites.[idx] <- sprite.Update gameState sprite
                applyUpdates sprites t
                
        let autoUpdateSprites = 
            sprites 
            |> List.filter (fun sp -> sp.AutoUpdate)
            |> List.map (fun sp -> sp.Id)

        let fullIdsToUpdate = List.append idsToUpdate autoUpdateSprites

        let drawableArr = Array.ofList sprites
        applyUpdates drawableArr fullIdsToUpdate
        drawableArr

    let updateDrawablesState renderState gameState textures =
        let drawableSprites = 
            match drawablesToAdd.Length with
            | 0 -> renderState.Drawables
            | _ -> addSprites renderState.Drawables drawablesToAdd textures
        drawablesToAdd <- []

        let filteredSprites = removeSprites drawableSprites drawablesToRemove
        drawablesToRemove <- []

        let tempRenderState = {renderState with Drawables=filteredSprites}

        let updatedSprites = updateSprites tempRenderState gameState filteredSprites drawablesToUpdate
        drawablesToUpdate <- []
        updatedSprites

    let updateRenderState renderState gameState textures =
        let updatedDrawables = updateDrawablesState renderState gameState textures

        renderState.View.Center <- Vec2<m>.toVec2f (Rectangle.center gameState.ViewBounds)
        renderState.View.Size <- Vec2<m>.toVec2f {X=gameState.ViewBounds.Width; Y=gameState.ViewBounds.Height}

        {renderState with Drawables = List.ofArray updatedDrawables}

    let drawDebugLines (win:RenderWindow) =
        let view = win.GetView()
        let offsetX = view.Center.X
        let offsetY = view.Center.Y

        let lineList = 
            debugLinesToDraw 
            |> List.map (fun li -> 
                li 
                |> List.map (fun l -> new Vertex(Vec2<m>.toVec2f l)) 
                |> Array.ofList) 
            |> Array.ofList

        lineList |> Array.map (fun li -> win.Draw(li, PrimitiveType.Lines)) |> ignore
        debugLinesToDraw <- []
        ()

    let draw (win:RenderWindow) renderState gameTime =
        win.Clear (new Color(43uy, 43uy, 90uy, 255uy))
        win.SetView renderState.View

        renderState.Drawables |> List.map (fun s -> s.Draw s win) |> ignore
        drawDebugLines win
        ()

    let genDefaultRenderState (win:RenderWindow)=
        {Drawables=[]; View=win.GetView()}

    ()