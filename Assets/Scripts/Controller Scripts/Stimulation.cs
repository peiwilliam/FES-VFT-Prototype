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
        _filterTargetX = new Filter(2);
        _filterTargetY = new Filter(2);
        _cursor = FindObjectOfType<Cursor>();
        _sceneName = SceneManager.GetActiveScene().name;

        // only create instance of controller when board is connected
        if ((bool)FindObjectOfType<WiiBoard>() && Wii.IsActive(0) && Wii.GetExpType(0) == 3)
            _controller = new Controller(_cursor); //pass in the cursor object so that we can access 

        if (_sceneName == "Ellipse") //since it's the same one circle in ellipse game, find it initially in start
            _targetCircle = FindObjectOfType<MovingCircle>().gameObject;
    }

    private void FixedUpdate()
    {
        if (_controller != null) //if it's just debugging with the cursor, there is no controller object, so want to prevent null reference error
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
            ControllerData = new ControllerData(stimOutput, _controller.RampPercentage, _controller.Angles, _controller.ShiftedPos);
        }
    }
}
