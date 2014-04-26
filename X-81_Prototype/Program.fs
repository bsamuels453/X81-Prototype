open SFML.Window;
open SFML.Graphics;
open SFML.Audio;
open System;
open GameTypes;
open System.Threading;
open System.IO;
open System.Diagnostics;
open MParticles.MGObjects;
open MParticles.Renderers.SFML;
open ParticleSys;

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

    let mutable gameState = Initialize.genDefaultGameState (win.GetView())
    let mutable renderState = Draw.genDefaultRenderState win
    let mutable mouseState = Control.initMouseControl win

    let gameTime = ref (new GameTime());
    let runningTime = new Stopwatch();
    runningTime.Start()

    SpriteGen.genDefaultScene gameState gameTime
    let stopwatch = new Stopwatch()

        
    
    let mutable accumulated = new TimeSpan()

    while win.IsOpen() do
        throttleTo60fps()
        stopwatch.Start()
        runningTime.Stop()
        let elapsed = runningTime.Elapsed
        runningTime.Restart()
        accumulated <- accumulated + elapsed
        (!gameTime).ElapsedGameTime <- elapsed
        (!gameTime).TotalGameTime <- accumulated

        win.DispatchEvents()
        let keyboardState = Control.pollKeyboard()
        mouseState <- Control.pollMouse mouseState win (ViewFuncs.screenToWorld gameState.GameView)

        let newGameView = Update.zoomTick gameState mouseState
        gameState <- {gameState with GameView = newGameView}

        let newShip = Update.playerShipTick gameState.PlayerShip keyboardState mouseState
        gameState <- {gameState with PlayerShip=newShip}

        renderState <- Draw.updateRenderState renderState gameState resources

        Draw.draw win renderState gameTime
        

        win.Display()
        let idleTime = getIdleTime stopwatch
        executeEveryHundred (fun () -> System.Console.WriteLine("Idle during " + string (100.0 - idleTime * 100.0) + "% of 16.6ms timeslice"))
        stopwatch.Reset()
    0 // return an integer exit code
