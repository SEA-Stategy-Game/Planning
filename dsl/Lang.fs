module Lang

    type Buildings =
        | Base
        | GoldMine

    type Resources =
        | Tree
        | Rock
        | Gold

    type MoveParams =
        | Buildings
        | Resources

    type StepType = 
        | Action of string

    type ActionType =
        | MoveTo of MoveParams
        | Collect of Resources


    type Step = {
        stepIndex: int
        stepType: string // <- stepTypes i stedet
        actionType: string // <- actionTypes i stedet
        parameters: string list// liste af resourcer og andet typer
    }

    type UnitPlanHeader = {
        unitId: string
        steps: Step list        
    }

    type PlanHeader = {
        schemaVersion: int
        gameId: string
        playerId: string
    }

    type Plan = {
        planHeader: PlanHeader 
        unitPlans: UnitPlanHeader list
    }