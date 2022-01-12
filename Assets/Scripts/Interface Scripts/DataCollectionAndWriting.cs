using UnityEngine;
using UnityEngine.SceneManagement;
using CSV;

public class DataCollectionAndWriting : MonoBehaviour //separate object for writing data, set in unity to execute later than other classes
{
    private string _sceneName;
    private bool _ecAssessmentStarted;
    private bool _eoAssessmentStarted;
    private bool _losStarted;
    private GameObject _targetCircle;
    private CSVWriter _writer;
    private Cursor _cursor;
    private Stimulation _stimulation;

    private void Awake() 
    {
        _sceneName = SceneManager.GetActiveScene().name;
    }

    private void OnEnable() //subscribing to event handled here
    {
        GameSession.DirectionChangeEvent += ChangeFileLOS;
        GameSession.ConditionChangeEvent += ChangeFileAssessment;
    }

    private void Start()
    {
        _cursor = FindObjectOfType<Cursor>();

        PrepWriter(); //create the writer object if it's not assessment or los. los and assessment handled differently

        if (_sceneName == "Ellipse") //since it's the same one circle in ellipse game, find it initially in start
            _targetCircle = FindObjectOfType<MovingCircle>().gameObject;

        if (_sceneName != "LOS" && _sceneName != "Assessment")
            _stimulation = FindObjectOfType<Stimulation>();
    }

    private void FixedUpdate()
    {
        GetData();
    }

    private void PrepWriter()
    {
        if (_sceneName != "LOS" && _sceneName != "Assessment") //LOS and assessment handled differently from games
        {
            _writer = new CSVWriter();
            _writer.WriteHeader();
        }
    }

    private void GetData()
    {
        var data = _cursor.Data;
        var targetCoords = GetTargetCoords();

        if (_sceneName != "LOS" && _sceneName != "Assessment") //LOS and assessment handled differently from games
            _writer.WriteDataAsync(data, targetCoords, _stimulation.TargetPositionFiltered, _stimulation.ControllerData);
        else if (_losStarted || _ecAssessmentStarted) //wait for user to click the button to start recording for los and assessment
        {
            if (!GameSession._ecDone)
                _writer.WriteDataAsync(data, targetCoords);
            else if (_eoAssessmentStarted && !GameSession._eoDone)
                _writer.WriteDataAsync(data, targetCoords);
        }
    }

    private Vector2 GetTargetCoords()
    {
        var targetCoords = new Vector2();

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

        if (_sceneName != "Target" && _sceneName != "Assessment" && _sceneName != "LOS")
            targetCoords = _targetCircle.transform.position;
        else //target game circle never leaves the centre, also not applicable for LOS and assessment
            targetCoords = new Vector2(0.0f, 0.0f);

        return targetCoords;
    }

    private void ChangeFileAssessment(string condition) //activates when starting or changing condition in assessment
    {
        _writer = new CSVWriter(condition);
        _writer.WriteHeader();

        if (!GameSession._ecDone && !GameSession._eoDone) //check which condition it is and ensure that the correct files are created
            _ecAssessmentStarted = true;
        else if (!GameSession._eoDone)
            _eoAssessmentStarted = true;
    }

    private void ChangeFileLOS(string direction) //activates when direction changes in los
    {
        _writer = new CSVWriter(direction);
        _writer.WriteHeader();
        _losStarted = true;
    }

    private void OnDisable() //unsubscribe when cursor is destroyed to avoid memory leaks
    {
        GameSession.DirectionChangeEvent -= ChangeFileLOS;
        GameSession.ConditionChangeEvent -= ChangeFileAssessment;
    }
}
