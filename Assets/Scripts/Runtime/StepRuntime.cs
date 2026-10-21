public class StepRuntime
{
    public ScenarioStepSO Config { get; }

    public StepStatus Status { get; private set; }
    public int CurrentConditionIndex { get; private set; }
    public int Attempts { get; private set; }
    public int Violations { get; private set; }

    public bool IsActive => Status == StepStatus.Active;
    public bool IsCompleted => Status == StepStatus.Completed;
    public bool IsFinished =>
        Status == StepStatus.Completed ||
        Status == StepStatus.Failed ||
        Status == StepStatus.Skipped;

    public StepRuntime(ScenarioStepSO config)
    {
        Config = config;
        Reset();
    }

    public void Activate()
    {
        if (Status == StepStatus.Inactive)
            Status = StepStatus.Active;
    }

    public StepEventResult ProcessEvent(ScenarioEvent scenarioEvent)
    {
        if (!IsActive || Config.Conditions.Count == 0)
            return StepEventResult.Ignored;

        var condition = Config.Conditions[CurrentConditionIndex];

        if (condition.EventId != scenarioEvent.EventId)
        {
            RegisterViolation();
            return StepEventResult.Violation;
        }

        CurrentConditionIndex++;

        if (CurrentConditionIndex >= Config.Conditions.Count)
        {
            Attempts++;
            Status = StepStatus.Completed;
            return StepEventResult.Completed;
        }

        return StepEventResult.Progress;
    }


    public void RegisterViolation()
    {
        if (!IsActive)
            return;

        Violations++;
        Attempts++;
        CurrentConditionIndex = 0;
    }



    public void Fail()
    {
        if (Status == StepStatus.Active ||
            Status == StepStatus.Inactive)
        {
            Status = StepStatus.Failed;
            CurrentConditionIndex = 0;
        }
    }

    public void Skip()
    {
        if (!IsFinished)
        {
            Status = StepStatus.Skipped;
            CurrentConditionIndex = 0;
        }
    }


    public void Reset()
    {
        Status = StepStatus.Inactive;
        CurrentConditionIndex = 0;
        Attempts = 0;
        Violations = 0;
    }
}
