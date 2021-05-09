using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] private ZeroBoard _zeroBoard;

    private static bool _beginFamiliarization;
    private static bool _beginExperimentation;
    private static bool _indicesRandomized;
    private static int _gameIndex = 1; //starting value
    private static int _gameIndicesIndex;
    private static List<int> _gameIndices = new List<int> {2, 3, 4, 5}; //starting values +1 from 1,2,3,4 because of build index

    public void LoadNextScene()
    {
        var currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex + 1);
    }

    public void LoadNextGame()
    {
        var currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

        if (SceneManager.GetActiveScene().name == "Experimentation Transition")
        {
            _gameIndex = _gameIndices[_gameIndicesIndex++]; //current value then increment
            SceneManager.LoadScene(currentSceneIndex + _gameIndex);
        }
        else
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

    public void Familiarization()
    {
        if (!_beginFamiliarization)
            _beginFamiliarization = true;
        else if (_gameIndex <= 4)
            _gameIndex++;

        SceneManager.LoadScene("Familiarization Transition");
    }

    public void Experimentation() 
    {
        if (!_beginExperimentation)
            _beginExperimentation = true;
        
        if (!_indicesRandomized) //check if unshuffled
        {
            KnuthShuffleIndices();
            _indicesRandomized = true;
        }

        SceneManager.LoadScene("Experimentation Transition");
    }

    public static bool GetFamiliarization() => _beginFamiliarization;

    public static int GetGameIndex() => _gameIndex;

    public static bool GetExperimentation() => _beginExperimentation;

    public static int GetGameIndicesIndex() => _gameIndicesIndex;

    private void KnuthShuffleIndices() //taken from here: https://stackoverflow.com/questions/2450954/how-to-randomize-shuffle-a-javascript-array
    {
        for (var i = _gameIndices.Count - 1; i > 0; i--)
        {
            var randomValue = Random.value;

            if (Random.value == 1f) //very unlikely to happen, but want to make sure that it never happens
                randomValue = Random.value; //will almost certainly not return 1 again if it happens

            var j = Mathf.FloorToInt(randomValue * (i + 1));
            var temp = _gameIndices[i];
            _gameIndices[i] = _gameIndices[j];
            _gameIndices[j] = temp;
        }
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