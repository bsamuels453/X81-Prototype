namespace global

module Log =
    open System

    type Category = 
        | Navigation
        | Input
        | AI
        | System
        | Drawing


    let private displayNav = true
    let private displayInput = true
    let private displayAI = true
    let private displaySystem = false
    let private displayDrawing = true

    let private write (str:string) cat =
        let doDisplay = 
            match cat with
            | Category.Navigation -> displayNav
            | Category.Input -> displayInput
            | Category.AI -> displayAI
            | Category.System -> displaySystem
            | Category.Drawing -> displayDrawing

        if doDisplay then System.Console.WriteLine(str) else ()


    let debug str cat =
        write str cat

    let error str cat =
        write str cat

    let warn str cat =
        write str cat

    let fatal str cat =
        write str cat
