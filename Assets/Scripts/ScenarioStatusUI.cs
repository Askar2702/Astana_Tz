using TMPro;
using UnityEngine;

public class ScenarioStatusUI : MonoBehaviour
{
    [SerializeField] private ScenarioRunner _runner;

    [SerializeField] private TMP_Text _modeText;
    [SerializeField] private TMP_Text _activeStepsText;
    [SerializeField] private TMP_Text _progressText;
    [SerializeField] private TMP_Text _scoreText;

    private void Update()
    {
        if (_runner == null)
            return;

        _modeText.text = $"Mode: {_runner.Mode}";

        _activeStepsText.text =
            $"Active steps: {_runner.ActiveSteps}";

        _progressText.text =
            $"Progress: {_runner.CompletedSteps}/{_runner.TotalSteps}";

        _scoreText.text = _runner.IsRunning
            ? "Score: In progress"
            : $"Score: {_runner.CurrentScore}";
    }
}