using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using ControllerManager;
using FilterManager;

public class Stimulation : MonoBehaviour
{
    private string _sceneName;
    private Controller _controller;
    private Cursor _cursor;
    private GameObject _targetCircle;
    private Filter _filterTargetX;
    private Filter _filterTargetY;

    public Dictionary<string, Dictionary<string, float>> ControllerConstants { get; private set; }
    public ControllerData ControllerData { get; private set; }
    public Vector2 TargetPositionFiltered { get; private set; }

    private void Start()
    {
        _filterTargetX = new Filter(2);
        _filterTargetY = new Filter(2);
        _cursor = FindObjectOfType<Cursor>();
        _sceneName = SceneManager.GetActiveScene().name;

        var foundWiiBoard = (bool)FindObjectOfType<WiiBoard>();
        _controller = new Controller(_cursor, foundWiiBoard); //pass in the cursor object so that we can access
        var slopesCondensed = new Dictionary<string, float>(); // want to condense the original dictionary to a string, float dictionary

        foreach (var control in _controller.Slopes)
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

        if (_sceneName == "Ellipse") //since it's the same one circle in ellipse game, find it initially in start
            _targetCircle = FindObjectOfType<MovingCircle>().gameObject;
    }

    private void FixedUpdate()
    {
        if (_controller != null) // just in case the controller object is null but it should never be null
        {
            //finding circle for colour and hunting handled here since it changes constantly in game
            //need the second condition since the targets don't despawn, only the circle with the "Target" tag changes
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
}
