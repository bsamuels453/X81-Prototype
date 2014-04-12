namespace global

module GameFuncs =
    let generateObjectId = 
        let count = ref 0;
        (fun () -> 
            count := !count + 1
            ObjectId(!count)
            )
    ()