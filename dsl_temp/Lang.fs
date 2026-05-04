module Lang

open System.Collections.Generic

// Internal AST used during parsing (commands can be nested under `If`)
type Command =
    | Action of string * Dictionary<string, string>   // actionType, params
    | If of string * Command list                     // condition text, body
    | UnitCommand of string * string list             // unit id digits, dot path

// Output shape — matches the backend's PlanStepIR exactly when serialized.
type Step = {
    stepIndex: int
    stepType: string
    actionType: string
    parameters: Dictionary<string, string>
    body: Step list
}

type UnitPlan = {
    unitId: string
    steps: Step list
}

type PlanSubmission = {
    schemaVersion: string
    gameId: string
    playerId: string
    unitPlans: UnitPlan list
}
