module Validate

open FParsec
open System.Collections.Generic

let str s = pstring s
let ws = spaces

let alphanumeric = many1Chars (letter <|> digit)

let private mkParams (pairs: (string * string) list) =
    let d = Dictionary<string, string>()
    for (k, v) in pairs do d.[k] <- v
    d

// Invariant decimal separator (period) — avoids da-DK serializing 1.5 as "1,5".
let private floatStr (f: float) = sprintf "%g" f

// Forward reference so `if` blocks can contain commands (including more ifs).
let pCommands, pCommandsRef = createParserForwardedToRef<Lang.Command, unit>()

// ---------- Header ----------

let pVersion : Parser<string, unit> =
    str "Schema version:" >>. spaces1 >>. (many1Chars (digit <|> pchar '.')) .>> ws

let pGameId : Parser<string, unit> =
    str "Game Id:" >>. spaces1 >>. alphanumeric .>> ws

let pPlayerId : Parser<string, unit> =
    str "Player Id:" >>. spaces1 >>. alphanumeric .>> ws

// ---------- Per-action parsers (match ActionSpec.cs exactly) ----------

let pMoveTo =
    str "MoveTo" >>. spaces1 >>.
    pipe2 (pfloat .>> spaces1) pfloat (fun x y ->
        Lang.Command.Action("MoveTo", mkParams [("x", floatStr x); ("y", floatStr y)]))

let pHarvest =
    str "Harvest" >>. spaces1 >>.
    pint32 |>> (fun id ->
        Lang.Command.Action("Harvest", mkParams [("target_id", string id)]))

let pConstruct =
    str "Construct" >>. spaces1 >>.
    pipe3 (alphanumeric .>> spaces1) (pfloat .>> spaces1) pfloat (fun scene x y ->
        Lang.Command.Action("Construct",
            mkParams [("scene", scene); ("x", floatStr x); ("y", floatStr y)]))

let pActionCommand : Parser<Lang.Command, unit> =
    pMoveTo <|> pHarvest <|> pConstruct

// ---------- If / Unit commands ----------

let pENDIF : Parser<unit, unit> =
    ws >>. str "END if" .>> ws >>% ()

let pCondition : Parser<string, unit> =
    many1Chars (noneOf "\n") .>> ws

let pIfCommand : Parser<Lang.Command, unit> =
    pipe2
        (str "if" >>. spaces1 >>. pCondition)
        (manyTill pCommands pENDIF)
        (fun cond inner -> Lang.Command.If(cond, inner))

let pUnitCommand : Parser<Lang.Command, unit> =
    pipe3
        (str "Unit")
        (many1Chars digit)
        (pchar '.' >>. sepBy1 alphanumeric (pchar '.'))
        (fun _ unitId path -> Lang.Command.UnitCommand(unitId, path))

do pCommandsRef :=
    ws >>. (attempt pIfCommand <|> attempt pUnitCommand <|> pActionCommand) .>> ws

// ---------- Plan structure ----------

let pEND : Parser<unit, unit> =
    ws >>. str "END" .>> ws >>% ()

let pUserId : Parser<string, unit> =
    str "unit" .>> ws >>. alphanumeric .>> str ":" .>> ws

let rec commandsToSteps (startIndex: int) (commands: Lang.Command list)
        : Lang.Step list * int =
    let folder (steps, idx) cmd =
        match cmd with
        | Lang.Command.Action(actionType, parameters) ->
            let step : Lang.Step = {
                stepIndex = idx
                stepType = "Action"
                actionType = actionType
                parameters = parameters
                body = []
            }
            (steps @ [step], idx + 1)

        | Lang.Command.If(cond, inner) ->
            let innerSteps, _ = commandsToSteps 0 inner
            let step : Lang.Step = {
                stepIndex = idx
                stepType = "Conditional"
                actionType = "If"
                parameters = mkParams [("condition", cond)]
                body = innerSteps
            }
            (steps @ [step], idx + 1)

        | Lang.Command.UnitCommand(id, path) ->
            let step : Lang.Step = {
                stepIndex = idx
                stepType = "Conditional"
                actionType = "UnitCommand"
                parameters = mkParams [("unit_id", id); ("path", String.concat "." path)]
                body = []
            }
            (steps @ [step], idx + 1)

    List.fold folder ([], startIndex) commands

let pUnitPlan : Parser<Lang.UnitPlan, unit> =
    pipe2 pUserId (manyTill pCommands pEND) (fun uid commands ->
        let steps, _ = commandsToSteps 0 commands
        { unitId = uid; steps = steps })

let parsePlan : Parser<Lang.PlanSubmission, unit> =
    pipe2
        (pipe3 pVersion pGameId pPlayerId (fun v g p -> (v, g, p)))
        (many pUnitPlan)
        (fun (v, g, p) units ->
            { schemaVersion = v
              gameId = g
              playerId = p
              unitPlans = units })
