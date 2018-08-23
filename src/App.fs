module App

open Elmish
open Elmish.React
open Fable.Helpers.React
open Fable.Helpers.React.Props

// MODEL

module Hanoi = 

    type Disk = int
    type Poles = Disk list list
    type Pole = int
    
    // Update Helpers

    let private moveDiskHelper (movingDisk:Disk) (targetPole:Pole) (currentPole:Pole) disksOnPole =
      if currentPole  = targetPole then
          movingDisk :: disksOnPole
      else
          disksOnPole

    /// Move a disk from one pole to another
    /// will return poles unchanged if move is illegal:
    ///  - to-pole is out of bounds
    ///  - moving disk is larger than top of to-pole
    let moveDisk movingDisk toPole (poles:Poles) : Poles =
        poles
        |> List.map (List.filter (fun disk ->disk <> movingDisk))
        |> List.mapi (moveDiskHelper movingDisk toPole)

    // View Helpers

    let private canMoveHelper (disk:Disk) previous (topOfPole:Disk option)  =
      if previous then 
          true
      else
          match topOfPole with
          | Some topDisk -> topDisk > disk
          | None -> true

    /// Checks if a disk (int) can move, based on whatever is on top of the 3 poles 
    /// If no pole is empty or has a larger disk on top, the result is false
    let canMove (disk:Disk) (poles:Poles) =
        poles
        |> List.map List.tryHead
        |> List.fold (canMoveHelper disk) false

    /// Checks if a moving disk can be dropped on a disklist
    /// True if list is empty or if top of list is larger than moving disk
    let isDroppableOn disksOnPole movingDisk =
        List.tryHead disksOnPole
        |> Option.map (fun top -> top > movingDisk)
        |> Option.defaultValue true

module Types = 

    type Msg =
        | Move of Hanoi.Disk
        | CancelMove
        | DropOn of Hanoi.Pole

    type Model =
        {
            poles: Hanoi.Poles
            movingDisk : Hanoi.Disk option
        }

module State =
    open Types 

    let init() : Model = 
        {
            poles = [[0..6]; []; []]
            movingDisk = None
        }

    let update (msg:Msg) (model:Model) =
        match msg with
        | Move selectedDisk ->
            { model with movingDisk = Some selectedDisk }
        | CancelMove ->
            {model with movingDisk = None}
        | DropOn newPole ->
            let newPoles =
              model.movingDisk
              |> Option.map (fun movingDisk -> Hanoi.moveDisk movingDisk newPole model.poles)
              |> Option.defaultValue model.poles
            { model with poles = newPoles; movingDisk = None}

module View =

    module Styles =
        let mainDiv: CSSProp list =
            [
                Display "flex"
                Width "420px"
                Margin "auto"
                Height "300px"
                AlignItems "flex-end"
            ]

        let column: CSSProp list =
            [
                Flex "0 0 180px"
                Display "flex"
                FlexDirection "column"
                AlignItems "center"
                JustifyContent "center"
                Position "relative"
            ]

        let disk: CSSProp list =
            [
                Width "80px"
                Height "20px"
                Margin "2px"
                BackgroundColor "#03A9F4"
                CSSProp.Custom ("user-select","none")
                BorderRadius "2px"
                BoxShadow "0 2px 4px rgba(0,0,0,.17)"
                ZIndex "1"
            ]

        let pole: CSSProp list =
            [
                Width "16px"
                Height "200px"
                BackgroundColor "#3E2723"
                BorderRadius "8px 8px 0 0"
                Position "absolute"
                Bottom "0"
                Left "82px"
            ]

    open Types
    let (++) = List.append

    let viewDisk model dispatch idx (disk:Hanoi.Disk)  =
        let width = 20 * disk + 40
        let widthStyles = [ Width (sprintf "%ipx" width) ]
        let moveableStyles, (moveAbleProps:IHTMLProp list) =
            match model.movingDisk with
            | None ->
                // if there is no moving disk yet, and this is top disk, 
                // and there are valid moves to other poles, then we're good
                if idx =0 && Hanoi.canMove disk model.poles then
                    [
                        BackgroundColor "#7CB342"
                    ],
                    [
                        Draggable true 
                        OnDragStart (fun ev ->
                            ev.dataTransfer.setData("text/plain","dummy") |> ignore
                            Msg.Move disk |> dispatch) 
                    ]
                else [], []
            | Some movingDisk ->
                if disk = movingDisk then
                    // this is the moving disk
                    [
                        Opacity "0.1"
                    ],
                    [
                        Draggable true 
                        OnDragEnd (fun _ -> Msg.CancelMove |> dispatch) 
                    ]
                else [], []    
        div   
            (
                Style ( Styles.disk ++ widthStyles ++ moveableStyles ) :> IHTMLProp
                :: moveAbleProps
            )
            [ str (sprintf "%i" disk) ]

    let viewPole model dispatch pole (diskList: Hanoi.Disk list )  = 
        let isDroppable =
            //  True if there is a moving disk and current pole is empty or has larger disk on top
            model.movingDisk
            |> Option.map (Hanoi.isDroppableOn diskList)
            |> Option.defaultValue false
        
        let (droppableStyles, (droppableProps:IHTMLProp list)) =
            if isDroppable then
                [
                   BackgroundColor "#7CB342" 
                ],
                [
                    OnDragOver (fun ev -> ev.preventDefault()) 
                    OnDrop (fun ev -> Msg.DropOn pole |> dispatch) 
                ]
            else [],[]
        div
            (
                Style (Styles.column) :> IHTMLProp
                :: droppableProps
            )
            (
                div 
                    [
                        Style ( Styles.pole ++ droppableStyles )
                    ] []
                :: (diskList |> List.mapi ( viewDisk model dispatch))
            ) 


    let view model dispatch =
      div [Style Styles.mainDiv]
        (model.poles |> List.mapi (viewPole model dispatch))
         

// App
Program.mkSimple State.init State.update View.view
|> Program.withReact "elmish-app"
|> Program.withConsoleTrace
|> Program.run