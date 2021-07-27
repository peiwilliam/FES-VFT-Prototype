using UnityEngine;
using UnityEngine.SceneManagement;
using CSV;

public class DataCollectionAndWriting : MonoBehaviour //separate class for writing all game data, set in unity to execute later than other classes
{
    private string _sceneName;
    private bool _ecAssessmentStarted;
    private bool _eoAssessmentStarted;
    private bool _losStarted;
    private GameObject _targetCircle;
    private CSVWriter _writer;
    private Cursor _cursor;

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

        if ((bool)FindObjectOfType<WiiBoard>() && Wii.IsActive(0) && Wii.GetExpType(0) == 3)
            PrepWriter();

        if (_sceneName == "Ellipse") //since it's the same one circle in ellipse game, find it initially in start
            _targetCircle = FindObjectOfType<MovingCircle>().gameObject;
    }

    private void FixedUpdate()
    {
        if ((bool)FindObjectOfType<WiiBoard>() && Wii.IsActive(0) && Wii.GetExpType(0) == 3)
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
            _writer.WriteDataAsync(data, targetCoords);
        else if (_losStarted || _ecAssessmentStarted) //wait for user to click the button to start recording for los and assessment
        {
            if (!GameSession.ecDone)
                _writer.WriteDataAsync(data, targetCoords);
            else if (_eoAssessmentStarted && !GameSession.eoDone)
                _writer.WriteDataAsync(data, targetCoords);
        }
    }

    private Vector2 GetTargetCoords()
    {
        var targetCoords = new Vector2();

        //finding circle for colour and hunting handled here since it changes constantly in game
        switch (_sceneName)
        {
            case "Colour Matching":
                if (_targetCircle == null || _targetCircle.tag != "Target")
                    //using findgameobjectwithtag is faster since it's more like searching through dict
                    _targetCircle = GameObject.FindGameObjectWithTag("Target");

                break;
            case "Hunting":
                if (_targetCircle == null || _targetCircle.tag != "Target")
                    //using findgameobjectwithtag is faster since it's more like searching through dict
                    _targetCircle = GameObject.FindGameObjectWithTag("Target");

                break;
        }

        if (_sceneName != "Target" && _sceneName != "Assessment" && _sceneName != "LOS")
            targetCoords = _targetCircle.transform.position;
        else //target game circle never leaves the centre
            targetCoords = new Vector2(0.0f, 0.0f);

        return targetCoords;
    }

    private void ChangeFileAssessment(string condition) //activates when starting or changing condition in assessment
    {
        _writer = new CSVWriter(condition);
        _writer.WriteHeader();

        if (!GameSession.ecDone && !GameSession.eoDone) //check which condition it is and ensure that the correct files are created
            _ecAssessmentStarted = true;
        else if (!GameSession.eoDone)
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
