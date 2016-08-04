[<Fable.Core.Erase>]
module internal Fable.Helpers.ReactNativeSimpleStore

open System
open Fable.Import.ReactNative
open Fable.Core
open Fable.Core.JsInterop
open Fable.Import
open Fable.Core.JsInterop

module DB =
    [<Literal>]    
    let private modelsKey = "models/"
    type Table<'a> = {
        TotalRows : int
        AutoInc: int
        Rows : 'a[]
    }
        with
            static member Empty() = {
                TotalRows = 0
                AutoInc = 0
                Rows = [||]
            } 

    // Removes all rows from the model.
    let inline clear<'a>() =
       let key = modelsKey + typeof<'a>.FullName
       async {
            let s:string = toJson(Table<'a>.Empty())
            let! _ = Globals.AsyncStorage.setItem(key,s) |> Async.AwaitPromise
            ()
       }

    /// Creates a new model.
    let inline private getModel<'a> (key) : Async<Table<'a>> = async {
        let! v = Globals.AsyncStorage.getItem (key) |> Async.AwaitPromise
        match v with
        | null -> return Table<'a>.Empty()
        | _ -> return ofJson v
    }

    // Adds a row to a model
    let inline add<'a>(data:'a) =
        let key = modelsKey + typeof<'a>.FullName
        async {
            let! model = getModel<'a> key
            let newId = model.AutoInc + 1
            let newModel : string =
                { TotalRows = model.TotalRows + 1
                  AutoInc = newId
                  Rows = Array.append [| data |] model.Rows }
                |> toJson
            let! _ = Globals.AsyncStorage.setItem(key,newModel) |> Async.AwaitPromise
            return newId
        }

    // Gets a row from the model
    let inline get<'a>(index:int) = 
        let key = modelsKey + typeof<'a>.FullName
        async {
            let! model = getModel<'a> key
            return model.Rows.[index]
        }

    // Gets all rows from the model
    let inline getAll<'a>(index:int) = 
        let key = modelsKey + typeof<'a>.FullName
        async {
            let! model = getModel<'a> key
            return model.Rows
        }

    // Gets the row count from the model
    let inline count<'a>() = 
        let key = modelsKey + typeof<'a>.FullName
        async {
            let! model = getModel<'a> key
            return model.TotalRows
        }