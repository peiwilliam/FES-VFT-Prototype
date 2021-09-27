using UnityEngine;
using UnityEngine.SceneManagement;
using ControllerManager;
using FilterManager;

public class Stimulation : MonoBehaviour
{
    private Controller _controller;
    private Cursor _cursor;
    private GameObject _targetCircle;
    private string _sceneName;
    private Filter _filterTargetX;
    private Filter _filterTargetY;

    public ControllerData ControllerData { get; private set; }
    public Vector2 TargetPositionFiltered { get; private set; }

    private void Start()
    {
        _controller = new Controller();
        _filterTargetX = new Filter(2);
        _filterTargetY = new Filter(2);
        _cursor = FindObjectOfType<Cursor>();
        _sceneName = SceneManager.GetActiveScene().name;

        if (_sceneName == "Ellipse") //since it's the same one circle in ellipse game, find it initially in start
            _targetCircle = FindObjectOfType<MovingCircle>().gameObject;
    }

    private void FixedUpdate()
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
        
        TargetPositionFiltered = _targetCircle.transform.position;

        if (_sceneName != "Target")
        {
            var stimOutput = _controller.Stimulate(_cursor.Data, TargetPositionFiltered);
            ControllerData = new ControllerData(stimOutput, _controller.RampPercentage);
        }
        else
        {
            var stimOutput = _controller.Stimulate(_cursor.Data, new Vector2(0f, 0f));
            ControllerData = new ControllerData(stimOutput, _controller.RampPercentage);
        }
    }
}
