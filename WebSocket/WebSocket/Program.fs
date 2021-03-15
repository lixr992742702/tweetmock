
open Suave
open Suave.Http
open Suave.Operators
open Suave.Filters
open Suave.Successful
open Suave.Files
open Suave.RequestErrors
open Suave.Logging
open Suave.Utils

open System
open System.Net
open System.Collections.Generic

open Suave.Sockets
open Suave.Sockets.Control
open Suave.WebSocket

//let (|Prefix|_|) (p:string) (s:string) =
//    if s.StartsWith(p) then
//        Some(s.Substring(p.Length))
//    else
//        None

let strList (str:string) =
    str.Split (':','*','#','@')

let mutable Client_pw_Dic = new Dictionary<string,string>()
let mutable Client_flr_Dic = new Dictionary<string,List<string>>()
let mutable Client_sub_Dic = new Dictionary<string,List<string>>()
let mutable Client_tw_Dic = new Dictionary<string,List<int>>()
let mutable Client_mtn_Dic = new Dictionary<string,List<int>>()
let mutable Client_live_Dic = new Dictionary<string,List<int>>()
// tweets dic
let mutable Tweet_Dic = new Dictionary<int,string>()
// hastags dic
let mutable Hashtag_Dic = new Dictionary<string,List<int>>()

let ws (webSocket : WebSocket) (context: HttpContext) =
    // client information dic

    //let emptystrlist = new List<string>()
    let emptyintlist = new List<int>()
    socket {
        // if `loop` is set to false, the server will stop receiving messages
        let mutable loop = true
        let mutable connect = false
        while loop do
      // the server will wait for a message to be received without blocking the thread
        
        let! msg = webSocket.read()

        match msg with
      // the message has type (Opcode * byte [] * bool)
      //
      // Opcode type:
      //   type Opcode = Continuation | Text | Binary | Reserved | Close | Ping | Pong
      //
      // byte [] contains the actual message
      //
      // the last element is the FIN byte, explained later
        | (Text, data, true) ->
        // the message can be converted to a string
            let str = UTF8.toString data // 1. transfer byte to string
            let strlist = strList (str) // 2. transfer string to string list [0:function,1: username, 2: infor1, 3: infor2, 4: infor3]
            //let response = sprintf "response to %s" str
            let byteResponse (message: string)=         // 3. transfer string to byte
                message
                |> System.Text.Encoding.ASCII.GetBytes
                |> ByteSegment
            match strlist.[0] with
            | "Register" ->
                if Client_pw_Dic.ContainsKey(strlist.[1]) then
                    do! webSocket.send Text (byteResponse ("Username exist, choose another username!")) true
                else
                    Client_pw_Dic.Add(strlist.[1],strlist.[2])
                    Client_flr_Dic.Add(strlist.[1], (new List<string>()))
                    Client_sub_Dic.Add(strlist.[1], (new List<string>()))
                    Client_tw_Dic.Add(strlist.[1], (new List<int>()))
                    Client_mtn_Dic.Add(strlist.[1], (new List<int>()))
                    Client_live_Dic.Add(strlist.[1], (new List<int>()))
                    do! webSocket.send Text (byteResponse ("Register succeed!")) true
            | "Login" ->
                if Client_pw_Dic.ContainsKey(strlist.[1]) then
                    if strlist.[2] = Client_pw_Dic.Item(strlist.[1]) then
                        do! webSocket.send Text (byteResponse ("Login succeed! Username: " + strlist.[1] + " Password: " + strlist.[2])) true
                      
                        // try build onmessage connect
                    else
                        do! webSocket.send Text (byteResponse ("Password is wrong, Please enter again!" )) true
                else
                    do! webSocket.send Text (byteResponse ("Account doesn't exist, Please enter again!" )) true
            | "Logout" ->
                connect <- false
                do! webSocket.send Text (byteResponse ("Logout succeed!")) true

                // close onmessage connect
            | "Subscribe" ->
                if Client_flr_Dic.ContainsKey(strlist.[2]) then 
                    if Client_flr_Dic.Item(strlist.[2]).Contains(strlist.[1]) then
                        do! webSocket.send Text (byteResponse ("Already follow this account!")) true
                    else
                        Client_flr_Dic.Item(strlist.[2]).Add(strlist.[1])
                        Client_sub_Dic.Item(strlist.[1]).Add(strlist.[2])
                        do! webSocket.send Text (byteResponse ("Subscribe succeed, follow tweet from: " + strlist.[2] )) true
                else
                    do! webSocket.send Text (byteResponse ("You can't subscribe a unexist account!")) true
            | "Tweet" -> //strlist.[1] = user strlist.[2] = content, strlist.[3] = hashtag, strlist.[4] = mention
                let mutable tid = Tweet_Dic.Count
                Tweet_Dic.Add(tid, (strlist.[1] + "*" + strlist.[2] + "#" + strlist.[3] + "@" + strlist.[4]))
                do! webSocket.send Text (byteResponse ("Tweet Succeed!" + " Tid: " + string(tid) + ":" + strlist.[1] + "*" + strlist.[2] + "#" + strlist.[3] + "@" + strlist.[4])) true
                //add tweet to client tweet list and live tweet to follower !!!!!!!!!!!
                Client_tw_Dic.Item(strlist.[1]).Add(tid)
                for i in Client_flr_Dic.Item(strlist.[1]) do
                    Client_live_Dic.Item(i).Add(tid)
                //add tweet to hashtag list
                if strlist.[3] <> "" then
                    if Hashtag_Dic.ContainsKey(strlist.[3]) then
                        Hashtag_Dic.Item(strlist.[3]).Add(tid)
                    else
                        let tmp = new List<int>()
                        tmp.Add(tid)
                        Hashtag_Dic.Add((strlist.[3]), tmp)
                //add tweet to client mention list and live tweet to this user !!!!!!!!!!
                if strlist.[4] <> "" then
                    if Client_mtn_Dic.ContainsKey(strlist.[4]) then
                        Client_mtn_Dic.Item(strlist.[4]).Add(tid)
                        Client_live_Dic.Item(strlist.[4]).Add(tid)
            | "Retweet" ->
                let mutable tmp = Tweet_Dic.Item(int(strlist.[2])) 
                do! webSocket.send Text (byteResponse ("Retweet: " + tmp )) true

            | "HashtagSearch" ->
                if Hashtag_Dic.ContainsKey(strlist.[2]) then
                    for i in Hashtag_Dic.Item(strlist.[2]) do
                        let mutable tmp = Tweet_Dic.Item(i)
                        do! webSocket.send Text (byteResponse (tmp)) true  
                else
                    do! webSocket.send Text (byteResponse ("Hashtag search fail. This hashtag not exist!")) true
            | "SubscribedTweets" ->
                for i in Client_sub_Dic.Item(strlist.[1]) do
                    for j in Client_tw_Dic.Item(i) do
                        let mutable tmp = Tweet_Dic.Item(j)
                        do! webSocket.send Text (byteResponse (tmp)) true                        
            | "MyMention" ->
                for i in Client_mtn_Dic.Item(strlist.[1]) do
                    let mutable tmp = Tweet_Dic.Item(i)
                    do! webSocket.send Text (byteResponse (tmp)) true  
            | "Refresh" ->

                if (Client_live_Dic.Item(strlist.[1])).Count > 0  then
                    let mutable tmp = Tweet_Dic.Item((Client_live_Dic.Item(strlist.[1])).[0])
                    do! webSocket.send Text (byteResponse("Live tweet: " + "tid:" + string((Client_live_Dic.Item(strlist.[1])).[0]) + " " + tmp)) true
                    (Client_live_Dic.Item(strlist.[1])).RemoveAt(0)
            | _ -> 
                do! webSocket.send Text (byteResponse ("Failed!")) true  
        // the response needs to be converted to a ByteSegment

        // the `send` function sends a message back to the client
        //do! webSocket.send Text byteResponse true

        | (Close, _, _) ->
            let emptyResponse = [||] |> ByteSegment
            do! webSocket.send Close emptyResponse true

        // after sending a Close message, stop the loop
            loop <- false

        | _ -> ()
    }

/// An example of explictly fetching websocket errors and handling them in your codebase.
let wsWithErrorHandling (webSocket : WebSocket) (context: HttpContext) = 
   
   let exampleDisposableResource = { new IDisposable with member __.Dispose() = printfn "Resource needed by websocket connection disposed" }
   let websocketWorkflow = ws webSocket context
   
   async {
    let! successOrError = websocketWorkflow
    match successOrError with
    // Success case
    | Choice1Of2() -> ()
    // Error case
    | Choice2Of2(error) ->
        // Example error handling logic here
        printfn "Error: [%A]" error
        exampleDisposableResource.Dispose()
        
    return successOrError
   }

let app : WebPart = 
  choose [
    path "/websocket" >=> handShake ws
    //path "/websocketWithSubprotocol" >=> handShakeWithSubprotocol (chooseSubprotocol "test") ws
    path "/websocketWithError" >=> handShake wsWithErrorHandling
    GET >=> choose [ path "/" >=> file "Twitter Clone.html"; browseHome ]
    NOT_FOUND "Found no handlers." ]

[<EntryPoint>]
let main _ =
  startWebServer { defaultConfig with logger = Targets.create Verbose [||] } app
  0

//
// The FIN byte:
//
// A single message can be sent separated by fragments. The FIN byte indicates the final fragment. Fragments
//
// As an example, this is valid code, and will send only one message to the client:
//
// do! webSocket.send Text firstPart false
// do! webSocket.send Continuation secondPart false
// do! webSocket.send Continuation thirdPart true
//
// More information on the WebSocket protocol can be found at: https://tools.ietf.org/html/rfc6455#page-34
//