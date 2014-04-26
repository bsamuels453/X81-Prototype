namespace global

[<AutoOpen>]
module ViewFuncs =
    let screenToWorld gameView mousePos : Vec2<m>=
        //let mx =  (float viewbounds.Width *1.0<m>) / (float Consts.screenWidth * 1.0<px>)
        //let my = (float viewbounds.Height *1.0<m>) / (float Consts.screenHeight * 1.0<px>)
        let mx = gameView.HScale
        let my = gameView.VScale

        {
            X=(mousePos.X * mx + gameView.BoundingBox.Origin.X)
            Y=(mousePos.Y * mx + gameView.BoundingBox.Origin.Y)
        }

    let createDefaultView() =
        let bounds = {Origin={X= -1100.0<m>; Y= -900.0<m>;}; Width=2200.0<m>; Height=1800.0<m>}
        let vscale = (float bounds.Width *1.0<m>) / (float Consts.screenWidth * 1.0<px>)
        let hscale = (float bounds.Height *1.0<m>) / (float Consts.screenHeight * 1.0<px>)
        {BoundingBox=bounds; VScale=vscale; HScale=hscale}