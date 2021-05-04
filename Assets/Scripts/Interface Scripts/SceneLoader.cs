using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] private ZeroBoard _zeroBoard;

    public bool Familiarization { get; private set; }

    public void LoadNextScene()
    {
        var currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex + 1);
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

    public void BeginFamiliarization()
    {
        SceneManager.LoadScene("Colour Matching");
        Familiarization = true;
    }

    private void OnEnable() 
    {
        if (SceneManager.GetActiveScene().name == "Settings")
        {
            var settingsManager = FindObjectOfType<SettingsManager>();
            settingsManager.SetInputFields();
        }
    }
}