
public class StepRuntime
{
    public ScenarioStepSO Config { get; }

    public int CurrentConditionIndex { get; private set; }
    public bool IsCompleted { get; private set; }

    public StepRuntime(ScenarioStepSO config)
    {
        Config = config;
    }

    public bool ProcessEvent(ScenarioEvent scenarioEvent)
    {
        if (IsCompleted || Config.Conditions.Count == 0)
            return false;

        var condition = Config.Conditions[CurrentConditionIndex];

        if (condition.EventId != scenarioEvent.EventId)
            return false;

        CurrentConditionIndex++;

        if (CurrentConditionIndex >= Config.Conditions.Count)
            IsCompleted = true;

        return true;
    }

    public void Reset()
    {
        CurrentConditionIndex = 0;
        IsCompleted = false;
    }
}
