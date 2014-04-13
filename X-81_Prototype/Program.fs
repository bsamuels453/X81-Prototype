open SFML.Window;
open SFML.Graphics;
open SFML.Audio;
open System;
open GameTypes;
open System.Threading;
open System.IO;
open System.Diagnostics;

let throttleTo60fps =
    let sw = new Stopwatch()
    sw.Start()
    let rec loopUntilFrameEnd() =
        sw.Stop()
        let elapsed = sw.Elapsed.TotalMilliseconds
        if elapsed >= 16.6 then
            sw.Restart()
        else
            sw.Start()
            let a = [1..100]
            loopUntilFrameEnd()
    loopUntilFrameEnd
    
let getIdleTime (sw:Stopwatch) =
    sw.Stop()
    let elapsed = sw.Elapsed.TotalMilliseconds
    elapsed / 16.6

let executeEveryHundred =
    let count = ref 0
    (fun c -> 
        if !count = 100 then
            count:=0
            c()
        else
            count := !count + 1    
    )

[<EntryPoint>]
[<STAThread>]
let main argv = 
    let win = Initialize.initWindow()
    let resources = Resource.loadResources()

    let mutable gameState = Initialize.genDefaultGameState()
    let mutable renderState = Draw.genDefaultRenderState win

    SpriteGen.genDefaultScene gameState
    let stopwatch = new Stopwatch()
    let screenToWorld gameState mousePos =
        {
            X=mousePos.X * 1.0<m/px> + gameState.PlayerShip.Position.X - (float Consts.screenWidth) *  0.5<m>
            Y= mousePos.Y * 1.0<m/px> + gameState.PlayerShip.Position.Y - (float Consts.screenHeight) * 0.5<m>
        }

    while win.IsOpen() do
        throttleTo60fps()
        stopwatch.Start()
        win.DispatchEvents()
        let keyboardState = Control.pollKeyboard()
        let mouseState = Control.pollMouse win (screenToWorld gameState)

        let newShip = Update.playerShipTick gameState.PlayerShip keyboardState mouseState
        gameState <- {gameState with PlayerShip=newShip}

        renderState <- Draw.updateRenderState renderState gameState resources
        Draw.draw win renderState

        let idleTime = getIdleTime stopwatch
        executeEveryHundred (fun () -> System.Console.WriteLine("Idle during " + string (100.0 - idleTime * 100.0) + "% of 16.6ms timeslice"))
        stopwatch.Reset()
    0 // return an integer exit code
