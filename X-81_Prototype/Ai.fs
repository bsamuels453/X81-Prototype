namespace global

module Ai =
    open AiTypes;
    open System;

    let private generateAiId = 
        let count = ref 0;
        (fun () -> 
            count := !count + 1
            AiId(!count)
            )

    let private selectGroup groups id =
        try
            List.find (fun g -> g.Id = id) groups
        with
            | :? System.Collections.Generic.KeyNotFoundException ->
                Log.fatal "tried to look up an ai group that doesnt exist" Log.Category.AI

    let createAiGroup members = {Id = generateAiId(); Subordinates = members; MovementState = Idle}
    
    let generateIrrelevantDestLookup moveInstruct ships =
        ships |> List.map (fun s -> (s, moveInstruct.Target))
        
    let generateScrambledDestLookup moveInstruct ships =
        let rand = new Random();
        
        ships |> List.map (fun s ->
            let xoffset = float (rand.Next(0, 100) - 50) * 1.0<m>
            let yoffset = float (rand.Next(0, 100) - 50) * 1.0<m>
            let offset = {X=xoffset; Y=yoffset}
            (s, moveInstruct.Target +. offset)
        )

    let applyMoveInstruction aiGroup (moveInstruct:AiMoveInstruction) = 
        let shipDestLookup = 
            match moveInstruct.DestFormationType with
            | FleetFormation.Irrelevant -> generateIrrelevantDestLookup moveInstruct aiGroup.Subordinates
            | FleetFormation.Scrambled -> generateScrambledDestLookup moveInstruct aiGroup.Subordinates
            | FleetFormation.SquareGrid -> Log.fatal "formation not supported" Log.Category.AI
        
        let formationDetail = {DestFormationType = moveInstruct.DestFormationType; ShipDestLookup = shipDestLookup}

        {aiGroup with MovementState = AiMacroMovementState.MovingToPoint(formationDetail)}