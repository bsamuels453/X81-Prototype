namespace global

module Ai =
    open AiTypes;
    let generateAiId = 
        let count = ref 0;
        (fun () -> 
            count := !count + 1
            AiId(!count)
            )
    ()