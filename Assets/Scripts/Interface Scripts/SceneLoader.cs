using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] private ZeroBoard _zeroBoard;

    private static bool _beginFamiliarization;
    private static int _gameIndex = 1;

    private void Start() 
    {
        // var name = SceneManager.GetActiveScene().name;
        // var nameMatch = name == "Colour Matching" || name == "Ellipse" || name == "Target" || name == "Hunting";

        // if (_beginFamiliarization && nameMatch && !_inFamiliarization)
        //     _inFamiliarization = true;
    }

    public void LoadNextScene()
    {
        var currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex + 1);
    }

    public void LoadNextGame()
    {
        var currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex + _gameIndex);
    }

    public void LoadStartScene() => SceneManager.LoadScene(0);

    public void ConnectToBoard() => SceneManager.LoadScene("Bluetooth PIN");

    public void LoadSettings() => SceneManager.LoadScene("Settings");

    public void ZeroBoard() => Instantiate(_zeroBoard);

    public void LoadColourMatching() => SceneManager.LoadScene("Colour Matching");

    public void LoadEllipse() => SceneManager.LoadScene("Ellipse");

    public void LoadTarget() => SceneManager.LoadScene("Target");

    public void LoadHunting() => SceneManager.LoadScene("Hunting");

    public void QuitGame() => Application.Quit();

    public void BeginAssesment() => SceneManager.LoadScene("Assessment");

    // public void BeginFamiliarization()
    // {
    //     SceneManager.LoadScene("Familiarization Transition");
    //     _beginFamiliarization = true;
    // }

    // public void LoadFamiliarizationTransition() 
    // {
    //     SceneManager.LoadScene("Familiarization Transition");
    //     _gameIndex++; //increase the index whenever we go to the transition so we can go to the next game
    // } 

    public void Familiarization()
    {
        if (!_beginFamiliarization)
            _beginFamiliarization = true;
        else if (_gameIndex < 4)
            _gameIndex++;

        SceneManager.LoadScene("Familiarization Transition");
    }

    public void BeginExperimentation() 
    {
        SceneManager.LoadScene("Start Experimentation");
    }

    public static bool GetFamiliarization() => _beginFamiliarization;

    public static int GetGameIndex() => _gameIndex;

    private void OnEnable() 
    {
        if (SceneManager.GetActiveScene().name == "Settings")
        {
            var settingsManager = FindObjectOfType<SettingsManager>();
            settingsManager.SetInputFields();
        }
    }
}