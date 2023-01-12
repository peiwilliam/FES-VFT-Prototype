using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using KnuthShuffle;

/// <summary>
/// This class is responsible for switching between different scenes in the program.
/// </summary>
public class SceneLoader : MonoBehaviour
{
    [Tooltip("Stores the zero board object")]
    [SerializeField] private ZeroBoard _zeroBoard;

    private static bool _firstOpen; //this is true once the game has been to the start screen once
    private static bool _beginFamiliarization; //this is true if familiarization has begun
    private static bool _beginExperimentation; //this is true is experimetnation has begun
    private static bool _indicesRandomized; //this is true if the indices for experimentation have been randomized
    private static int _gameIndex = 1; //starting value
    private static int _gameIndicesIndex; //the current index of the game index in the gameIndices list
    private static int _trialIndex; //which trial we're on in experimentation
    private static List<int> _gameIndices = new List<int> {2, 3, 4, 5}; //starting values +1 from 1,2,3,4 because of build index

    private void Awake() //runs once at the beginning when the object is instatiated, runs before start
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
    
    /// <summary>
    /// This is currently unused, but would be used to load the next scene in order of the build index.
    /// </summary>
    public void LoadNextScene()
    {
        var currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex + 1);
    }

    /// <summary>
    /// This method is used by the next button in the experimentation transition unless we just started from the start scene.
    /// </summary>
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

    /// <summary>
    /// This method loads the start scene but also resets some of the static variables.
    /// </summary>
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

    /// <summary>
    /// This method loads the scene with the bluetooth PIN and connecting the board to the program.
    /// </summary>
    public void ConnectToBoard() => SceneManager.LoadScene("Bluetooth PIN");

    /// <summary>
    /// This method loads the connection instrucitons scene.
    /// </summary>
    public void ConnectionInstructions() => SceneManager.LoadScene("Connection Instructions");

    /// <summary>
    /// This method loads the settings scene.
    /// </summary>
    public void LoadSettings() => SceneManager.LoadScene("Settings");

    /// <summary>
    /// This method creates a zero board object to collect data from an empty board to get the baseline values.
    /// </summary>
    public void ZeroBoard() => Instantiate(_zeroBoard);

    /// <summary>
    /// This method loads the colour matching game.
    /// </summary>
    public void LoadColourMatching() => SceneManager.LoadScene("Colour Matching");

    /// <summary>
    /// This method loads the ellipse game.
    /// </summary>
    public void LoadEllipse() => SceneManager.LoadScene("Ellipse");

    /// <summary>
    /// This method loads the target game.
    /// </summary>
    public void LoadTarget() => SceneManager.LoadScene("Target");

    /// <summary>
    /// This method loads the hunting game.
    /// </summary>
    public void LoadHunting() => SceneManager.LoadScene("Hunting");

    /// <summary>
    /// This method closes the game.
    /// </summary>
    public void QuitGame() => Application.Quit();

    /// <summary>
    /// This method loads the QS assessment.
    /// </summary>
    public void BeginAssesment() => SceneManager.LoadScene("Assessment"); 

    /// <summary>
    /// This method loads the LOS.
    /// </summary>
    public void BeginSensitivity() => SceneManager.LoadScene("LOS");

    /// <summary>
    /// This method starts familiarization of the games. We go through each of the games once in the order in the bulid index.
    /// </summary>
    public void Familiarization()
    {
        //mark that experimentation has begun
        if (!_beginFamiliarization)
            _beginFamiliarization = true;
        else if (_gameIndex <= 4) //increase the game index while we haven't gone through all the games yet
            _gameIndex++;

        SceneManager.LoadScene("Familiarization Transition");
    }

    /// <summary>
    /// This method starts the experimentation with a set number of trials and the games in a randomized order each trial.
    /// </summary>
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

    /// <summary>
    /// Get the whether or not famailiarization has started.
    /// </summary>
    public static bool GetFamiliarization() => _beginFamiliarization;

    /// <summary>
    /// Get the current game index.
    /// </summary>
    public static int GetGameIndex() => _gameIndex;

    /// <summary>
    /// Get the whether or not experimentation has started.
    /// </summary>
    public static bool GetExperimentation() => _beginExperimentation;

    /// <summary>
    /// Get the index of the game index in the game indces list.
    /// </summary>
    public static int GetGameIndicesIndex() => _gameIndicesIndex;

    /// <summary>
    /// Get the current trial index.
    /// </summary>
    public static int GetTrialIndex() => _trialIndex;
}