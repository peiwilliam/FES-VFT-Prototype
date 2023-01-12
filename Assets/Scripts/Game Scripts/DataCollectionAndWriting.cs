using UnityEngine;
using UnityEngine.SceneManagement;
using CSV;

/// <summary>
/// This class is responsible for writing all of the game data in the current physics tick. This object is set to execute later than
/// all other objects in the Unity.
/// </summary>
public class DataCollectionAndWriting : MonoBehaviour
{
    [Tooltip("Store the game session object associated with the game")]
    [SerializeField] private GameSession _gameSession;

    private string _sceneName;
    private bool _ecAssessmentStarted; //these three bools keep track of the different conditions for QS assessment and LOS to make sure file writing is done correctly.
    private bool _eoAssessmentStarted;
    private bool _losStarted;
    private GameObject _targetCircle;
    private CSVWriter _writer;
    private Cursor _cursor;
    private Stimulation _stimulation;

    private void OnEnable() //runs at the very beginning before awake or start. Subscribing to event handled here
    {
        GameSession.DirectionChangeEvent += ChangeFileLOS; //writing for LOS and QS assessment handled via events
        GameSession.ConditionChangeEvent += ChangeFileAssessment;
    }

    private void Awake() //runs at the very beginning before start at object instantiation
    {
        _sceneName = SceneManager.GetActiveScene().name;
    }

    private void Start() //runs at the beginning at object instantiation
    {
        _cursor = FindObjectOfType<Cursor>();

        if (_sceneName == "Ellipse") //since it's the same one circle in ellipse game, find it initially in start
            _targetCircle = FindObjectOfType<MovingCircle>().gameObject;

        if (_sceneName != "LOS" && _sceneName != "Assessment")
            _stimulation = FindObjectOfType<Stimulation>();

        PrepWriter(); //create the writer object if it's not assessment or los. los and assessment handled differently
    }

    private void FixedUpdate() //runs at every physics tick. Set to 0.02s or 50 Hz by default
    {
        GetAndWriteData();
    }

    private void PrepWriter() //creates the CSVWriter object and writes the header to the CSV file
    {
        if (_sceneName == "LOS" || _sceneName == "Assessment") //LOS and assessment handled differently from games
            return;

        _writer = new CSVWriter(_sceneName);
        _writer.WriteHeader(_cursor.Data, _stimulation);
    }

    private void GetAndWriteData() //runs at every physics tick and sends the cursor and controller data to CSVWriter to be written in the cSV
    {
        var data = _cursor.Data;
        var targetCoords = GetTargetCoords();

        if (_sceneName != "LOS" && _sceneName != "Assessment") //LOS and assessment handled differently from games
            _writer.WriteDataAsync(data, targetCoords, _stimulation.TargetPositionFiltered, _stimulation.ControllerData, _gameSession);
        else if (_losStarted || _ecAssessmentStarted) //wait for user to click the button to start recording for los and assessment
        {
            if (!GameSession.ecDone)
                _writer.WriteDataAsync(data, targetCoords);
            else if (_eoAssessmentStarted && !GameSession.eoDone)
                _writer.WriteDataAsync(data, targetCoords);
        }
    }

    private Vector2 GetTargetCoords() //gets the coordinates of the target circle, kept in the game perspective.
    {
        var targetCoords = new Vector2();

        //finding circle for colour and hunting handled here since it changes constantly in game
        //need the second condition since the targets don't despawn, only the circle with the "Target" tag changes
        if (_sceneName == "Colour Matching" || _sceneName == "Hunting")
        {
            if (_targetCircle == null || _targetCircle.tag != "Target")
            {
                //using findgameobjectwithtag is faster since it's more like searching through dict
                _targetCircle = GameObject.FindGameObjectWithTag("Target");
            }
        }

        if (_sceneName != "Target" && _sceneName != "Assessment" && _sceneName != "LOS")
            targetCoords = _targetCircle.transform.position;
        else //target game circle never leaves the centre, also not applicable for LOS and assessment
            targetCoords = new Vector2(0.0f, 0.0f);

        return targetCoords;
    }

    private void ChangeFileAssessment(string condition) //activates when starting or changing condition in assessment
    {
        _writer = new CSVWriter(_sceneName, condition);
        _writer.WriteHeader(_cursor.Data);

        if (!GameSession.ecDone && !GameSession.eoDone) //check which condition it is and ensure that the correct files are created
            _ecAssessmentStarted = true;
        else if (!GameSession.eoDone)
            _eoAssessmentStarted = true;
    }

    private void ChangeFileLOS(string direction) //activates when direction changes in los
    {
        _writer = new CSVWriter(_sceneName, direction);
        _writer.WriteHeader(_cursor.Data);
        _losStarted = true;
    }

    private void OnDisable() //unsubscribe when cursor is destroyed to avoid memory leaks
    {
        GameSession.DirectionChangeEvent -= ChangeFileLOS;
        GameSession.ConditionChangeEvent -= ChangeFileAssessment;
    }
}
