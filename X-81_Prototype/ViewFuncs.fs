namespace global

[<AutoOpen>]
module ViewFuncs =
    let screenToWorld gameView mousePos : Vec2<m>=
        //let mx =  (float viewbounds.Width *1.0<m>) / (float Consts.screenWidth * 1.0<px>)
        //let my = (float viewbounds.Height *1.0<m>) / (float Consts.screenHeight * 1.0<px>)
        let mx = gameView.ViewScale
        let my = gameView.ViewScale

        {
            X=(mousePos.X * mx + gameView.BoundingBox.Origin.X)
            Y=(mousePos.Y * mx + gameView.BoundingBox.Origin.Y)
        }

    let createDefaultView() =
        let bounds = {Origin={X= -1100.0<m>; Y= -900.0<m>;}; Width=2200.0<m>; Height=1800.0<m>}
        let vscale = (float bounds.Width *1.0<m>) / (float Consts.screenWidth * 1.0<px>)
        {BoundingBox=bounds; ViewScale=vscale;}

    let modifyViewScale view scaleQty =
        let newViewScale =
            match (scaleQty+view.ViewScale) with
            | (s) when s < Consts.zoomInLimit -> Consts.zoomInLimit + 0.00000001<m/px>
            | (s) when s > Consts.zoomOutLimit -> Consts.zoomOutLimit - 0.00000001<m/px>
            | (s) -> view.ViewScale + scaleQty

        let newBoundsWidth =  (float Consts.screenWidth) * 1.0<px> * newViewScale
        let newBoundsHeight = (float Consts.screenHeight) * 1.0<px> * newViewScale
        let center = Rectangle.center view.BoundingBox
        let newOrigin = {X=center.X-newBoundsWidth/2.0; Y=center.Y-newBoundsHeight/2.0}
        {
            BoundingBox = {Origin=newOrigin; Width=newBoundsWidth; Height=newBoundsHeight}
            ViewScale=newViewScale
        }