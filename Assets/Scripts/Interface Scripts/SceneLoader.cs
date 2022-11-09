using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using KnuthShuffle;

public class SceneLoader : MonoBehaviour
{
    [Tooltip("Stores the zero board object")]
    [SerializeField] private ZeroBoard _zeroBoard;

    private static bool _firstOpen;
    private static bool _beginFamiliarization;
    private static bool _beginExperimentation;
    private static bool _indicesRandomized;
    private static int _gameIndex = 1; //starting value
    private static int _gameIndicesIndex;
    private static int _trialIndex;
    private static List<int> _gameIndices = new List<int> {2, 3, 4, 5}; //starting values +1 from 1,2,3,4 because of build index

    private void Awake() 
    {
        if (!_firstOpen) //only run this part of the code once when the game is first opened
        {
            if (!PlayerPrefs.HasKey("Resolution") && !PlayerPrefs.HasKey("Fullscreen")) //if it's a fresh start, will need to get resolution manually
            {
                //want to set the resolution to a 16:9 resolution upon starting the program
                if (!Mathf.Approximately(Convert.ToSingle(Screen.currentResolution.width)/Convert.ToSingle(Screen.currentResolution.height), 16f/9f))
                {
                    //pick only the 16:9 resolutions from the available resolutions
                    var validRes = Screen.resolutions.ToList().FindAll(res => Mathf.Approximately(Convert.ToSingle(res.width)/Convert.ToSingle(res.height), 16f/9f));

                    if (validRes.Count == 0)
                        Debug.LogWarning("System doesn't allow for 16:9 resolutions. Please use a system that allows for 16:9 resolutions");
                    else //resolution is listed from smallest to largest, so the largest 16:9 resolution is the last one in the list
                        Screen.SetResolution(validRes[validRes.Count-1].width, validRes[validRes.Count-1].height, true);
                }
            }
            else
            { //if already saved, just grab it form there
                var res = Array.ConvertAll(PlayerPrefs.GetString("Resolution").Split('x'), s => Convert.ToInt32(s));
                Screen.SetResolution(res[0], res[1], Convert.ToBoolean(PlayerPrefs.GetInt("Fullscreen", 1)));
            }
        }

        _firstOpen = true;
    }
    
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
            if (_gameIndicesIndex == _gameIndices.Count)
                _gameIndicesIndex = 0;

            _gameIndex = _gameIndices[_gameIndicesIndex++]; //current value then increment
            SceneManager.LoadScene(currentSceneIndex + _gameIndex);
        }
        else
            SceneManager.LoadScene(currentSceneIndex + _gameIndex);
    }

    public void LoadStartScene() 
    {
        SceneManager.LoadScene(0);

        //when we return to start scene, we want to reset all of the static variables
        _beginFamiliarization = false;
        _beginExperimentation = false;
        _indicesRandomized = false;
        _trialIndex = 0;
        _gameIndex = 1;
        _gameIndicesIndex = 0;
    }

    public void ConnectToBoard() => SceneManager.LoadScene("Bluetooth PIN");

    public void ConnectionInstructions() => SceneManager.LoadScene("Connection Instructions");

    public void LoadSettings() => SceneManager.LoadScene("Settings");

    public void ZeroBoard() => Instantiate(_zeroBoard);

    public void LoadColourMatching() => SceneManager.LoadScene("Colour Matching");

    public void LoadEllipse() => SceneManager.LoadScene("Ellipse");

    public void LoadTarget() => SceneManager.LoadScene("Target");

    public void LoadHunting() => SceneManager.LoadScene("Hunting");

    public void QuitGame() => Application.Quit();

    public void BeginAssesment() => SceneManager.LoadScene("Assessment"); 

    public void BeginSensitivity() => SceneManager.LoadScene("LOS");

    public void Familiarization()
    {
        //mark that experimentation has begun
        if (!_beginFamiliarization)
            _beginFamiliarization = true;
        else if (_gameIndex <= 4) //increase the game index while we haven't gone through all the games yet
            _gameIndex++;

        SceneManager.LoadScene("Familiarization Transition");
    }

    public void Experimentation() 
    {
        //mark that experimentation as begun
        if (!_beginExperimentation)
            _beginExperimentation = true;

        //increase trial index and prepare to shuffle if number of trials hasn't been reached yet
        if (_gameIndicesIndex == _gameIndices.Count)
        {
            _indicesRandomized = false;

            if (_trialIndex < PlayerPrefs.GetInt("Number of Trials", 2))
                _trialIndex++;
        } 

        //check if unshuffled and the trial number, no need to shuffle if last trial
        if (!_indicesRandomized && _trialIndex < PlayerPrefs.GetInt("Number of Trials", 2)) 
        {
            _gameIndices = KnuthShuffler.Shuffle(_gameIndices);
            _indicesRandomized = true;
        }    

        //when we have reached the number of trials and have finished the last trial, go back to the start
        if (_trialIndex == PlayerPrefs.GetInt("Number of Trials", 2) && _gameIndicesIndex == _gameIndices.Count)
            LoadStartScene();
        else
            SceneManager.LoadScene("Experimentation Transition");
    }

    public static bool GetFamiliarization() => _beginFamiliarization;

    public static int GetGameIndex() => _gameIndex;

    public static bool GetExperimentation() => _beginExperimentation;

    public static int GetGameIndicesIndex() => _gameIndicesIndex;

    public static int GetTrialIndex() => _trialIndex;
}