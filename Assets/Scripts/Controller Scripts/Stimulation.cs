using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using ControllerManager;
using FilterManager;

/// <summary>
/// This class controls the Stimulation game object. The class is responsible for controlling the data fed into the controller
/// and then acquiring all of the relevant data from the controller including the requested stimulation.
/// </summary>
public class Stimulation : MonoBehaviour
{
    private string _sceneName;
    private Controller _controller;
    private Cursor _cursor;
    private GameObject _targetCircle;
    private Filter _filterTargetX;
    private Filter _filterTargetY;

    /// <summary>
    /// Property for getting storing the controller constants that are calculated when the controller is instantiated.
    /// </summary>
    public Dictionary<string, Dictionary<string, float>> ControllerConstants { get; private set; }
    /// <summary>
    /// Property for storing the stimulation maximums and baselines. This is primarily here so that the CSVWriter class can directly
    /// access these values.
    /// </summary>
    public Dictionary<string, Dictionary<string, float>> ControllerStimConstants { get; private set; } 
    /// <summary>
    /// Property for storing the relevant controller values at the current iteration. This is used by the CSVWRiter class.
    /// </summary>
    public ControllerData ControllerData { get; private set; }
    /// <summary>
    /// Property for getting the current target position. This is used by the CSVWRiter class.
    /// </summary>
    public Vector2 TargetPositionFiltered { get; private set; }

    private void Start() //rusn only at the creation of the game object
    {
        _filterTargetX = new Filter(2); //instantiate filters for the target
        _filterTargetY = new Filter(2);
        _cursor = FindObjectOfType<Cursor>(); //find the in game cursor object
        _sceneName = SceneManager.GetActiveScene().name;
        var foundWiiBoard = (bool)FindObjectOfType<WiiBoard>(); //true if the wiiboard object is found, false is not found
        _controller = new Controller(_cursor, foundWiiBoard);
        var slopesCondensed = new Dictionary<string, float>(); //we want to condense the original dictionary to a string, float dictionary

        foreach (var control in _controller.Slopes) //condense the slopes into a string, float dictionary
        {
            foreach (var muscle in control.Value)
                slopesCondensed[control.Key + " " + muscle.Key] = muscle.Value;
        }

        ControllerConstants = new Dictionary<string, Dictionary<string, float>>()
        {
            ["Constants"] = _controller.CalculatedConstants,
            ["Slopes"] = slopesCondensed,
            ["Intercepts"] = _controller.Intercepts
        };

        ControllerStimConstants = _controller.CalculatedStimBaselines;

        if (_sceneName == "Ellipse") //since it's the same one circle in ellipse game, find it initially in start
            _targetCircle = FindObjectOfType<MovingCircle>().gameObject;
    }

    private void FixedUpdate() //runs at fixed time intervals
    {
        if (_controller == null) // just in case the controller object is null but it should never be null
            return;

        //finding circle for colour and hunting handled here since it changes constantly in game
        //need the second condition since the targets don't despawn in colour matching, only the circle with the "Target" tag changes
        if (_targetCircle == null || _targetCircle.tag != "Target")
        {
            switch (_sceneName)
            {
                case "Colour Matching":
                case "Hunting":
                    //using findgameobjectwithtag is faster since it's more like searching through dict
                    _targetCircle = GameObject.FindGameObjectWithTag("Target");
                    break;
            }
        }

        if (_sceneName != "Target")
        {
            if (_sceneName != "Ellipse")
                TargetPositionFiltered =  new Vector2(_filterTargetX.ComputeMA(_targetCircle.transform.position.x), _filterTargetY.ComputeMA(_targetCircle.transform.position.y));
            else
                TargetPositionFiltered = _targetCircle.transform.position; //no filtering done for ellipse
        }
        else
            TargetPositionFiltered = new Vector2(0f, 0f); //no filtering for target either
        
        var stimOutput = _controller.Stimulate(_cursor.Data, TargetPositionFiltered);
        ControllerData = new ControllerData(stimOutput, _controller.Biases, _controller.RampPercentage, _controller.Angles, 
                                            _controller.MlAngles, _controller.ShiftedPos, _controller.NeuralTorque, 
                                            _controller.MechanicalTorque);
    }
}
