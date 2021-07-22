using FilterManager;
using ControllerManager;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Cursor : MonoBehaviour
{
    [SerializeField] private float _minX = 0f;
    [SerializeField] private float _maxX = 2f*5f*16f/9f; //2*height*aspect ratio
    [SerializeField] private float _minY = 0f;
    [SerializeField] private float _maxY = 5f*2f; //2*camera size

    private const float _Length = 433; // mm
    private const float _Width = 228; // mm
    private const float _G = 9.81f; // m/s^2
    private float _mass;
    private float _height;
    private float _m; // kg
    private float _h; // m
    private float _i; // kgm^2
    private float _limitFront;
    private float _limitBack;
    private float _limitLeft;
    private float _limitRight;
    private float _offset;
    private bool _losStarted;
    private bool _ecAssessmentStarted;
    private bool _eoAssessmentStarted;
    private string _sceneName;
    private GameObject _targetCircle;
    private Filter _filterX;
    private Filter _filterY;
    private CSVWriter _writer;
    private GameSession _gameSession;
    private Controller _controller;

    public WiiBoardData Data { get; private set; }
    public Vector2 TargetCoords {get; private set; }

    private void Awake() //want to compute these values before anything starts
    {
        _mass = PlayerPrefs.GetFloat("Mass");
        _height = PlayerPrefs.GetFloat("Height");
        _m = PlayerPrefs.GetFloat("Ankle Mass Fraction")*_mass;
        _h = PlayerPrefs.GetFloat("CoM Fraction")*_height;
        _i = PlayerPrefs.GetFloat("Inertia Coefficient")*_mass*Mathf.Pow(_height, 2);
        _sceneName = SceneManager.GetActiveScene().name;

        if (_sceneName == "LOS" || _sceneName == "Assessment") //only shift and scale cop when it's the games
        {
            _limitFront = 1.0f;
            _limitBack = 1.0f;
            _limitLeft = 1.0f;
            _limitRight = 1.0f;

            if (_sceneName == "Assessment")
                _offset = 0.0f;
            else
                _offset = PlayerPrefs.GetFloat("Length Offset");
        }
        else
        {
            _limitFront = PlayerPrefs.GetFloat("Limit of Stability Front");
            _limitBack = PlayerPrefs.GetFloat("Limit of Stability Back");
            _limitLeft = PlayerPrefs.GetFloat("Limit of Stability Left");
            _limitRight = PlayerPrefs.GetFloat("Limit of Stability Right");
            _offset = PlayerPrefs.GetFloat("Length Offset");
        }
    }

    private void OnEnable() //subscribing to event handled here
    {
        GameSession.DirectionChangeEvent += ChangeFileLOS;
        GameSession.ConditionChangeEvent += ChangeFileAssessment;
        GameSession.ColourChangeEvent += ChangeTarget;
        GameSession.TargetChangeEvent += ChangeTarget;
    }

    private void Start() 
    {
        _gameSession = FindObjectOfType<GameSession>();
        _sceneName = SceneManager.GetActiveScene().name;

        if ((bool)FindObjectOfType<WiiBoard>() && Wii.IsActive(0) && Wii.GetExpType(0) == 3)
        {
            SetBoardConditions();
            _controller = new Controller();
        }
        
        if (_sceneName == "Ellipse") //since it's the same one circle in ellipse game, find it initially in start
            _targetCircle = FindObjectOfType<MovingCircle>().gameObject;
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void SetBoardConditions()
    {
        if (PlayerPrefs.GetInt("Filter Data", 0) == 1) //set 0 as default in case it isn't set
        {
            _filterX = new Filter(PlayerPrefs.GetInt("Filter Order")); //moving average, doesn't work with wii balance board right now
            _filterY = new Filter(PlayerPrefs.GetInt("Filter Order"));

            _filterX = new Filter(0.4615f, 1.0f / Time.fixedDeltaTime, PlayerPrefs.GetInt("Filter Order")); //bw temporary for now
            _filterY = new Filter(0.4615f, 1.0f / Time.fixedDeltaTime, PlayerPrefs.GetInt("Filter Order"));
        }

        if (_sceneName != "LOS" && _sceneName != "Assessment") //LOS and assessment handled differently from games
        {
            _writer = new CSVWriter();
            _writer.WriteHeader();
        }
    }

    private void Move()
    {
        if ((bool)FindObjectOfType<WiiBoard>() && Wii.IsActive(0) && Wii.GetExpType(0) == 3)
        {
            Data = GetBoardValues();
            GetTargetCoords();

            if (_sceneName != "LOS" && _sceneName != "Assessment") //LOS and assessment handled differently from games
                _writer.WriteDataAsync(Data, TargetCoords);
            else if (_losStarted || _ecAssessmentStarted) //wait for user to click the button to start recording for los and assessment
            {
                if (!GameSession._ecDone)
                    _writer.WriteDataAsync(Data, TargetCoords);
                else if (_eoAssessmentStarted && !GameSession._eoDone)
                    _writer.WriteDataAsync(Data, TargetCoords);
            }

            var pos = new Vector2(transform.position.x, transform.position.y);
            var com = new Vector2();

            if (PlayerPrefs.GetInt("Filter Data", 0) == 1) //set default to zero in case it isn't set
                com = new Vector2(Data.fCopX, Data.fCopY);
            else
                com = new Vector2(Data.copX, Data.copY); //com == cop if no filtering

            // subtract the offset and scale the cursor on screen to the individual's max in each direction
            if (com.x > 0)
                pos.x = Mathf.Clamp((com.x / _limitRight) * (_maxX / 2) + Camera.main.transform.position.x, _minX, _maxX);
            else
                pos.x = Mathf.Clamp((com.x / _limitLeft) * (_maxX / 2) + Camera.main.transform.position.x, _minX, _maxX);

            if (com.y > 0)
                pos.y = Mathf.Clamp(((com.y) / _limitFront) * (_maxY / 2) + Camera.main.transform.position.y, _minY, _maxY);
            else
                pos.y = Mathf.Clamp(((com.y) / _limitBack) * (_maxY / 2) + Camera.main.transform.position.y, _minY, _maxY);

            transform.position = pos;
        }
        else
        {
            // debugging using mouse
            var pos = new Vector2(transform.position.x, transform.position.y);
            pos.x = Mathf.Clamp(Input.mousePosition.x / Screen.width * _maxX, _minX, _maxX);
            pos.y = Mathf.Clamp(Input.mousePosition.y / Screen.height * _maxY, _minY, _maxY);
            transform.position = pos;
        }
    }

    private WiiBoardData GetBoardValues()
    {
        var boardSensorValues = Wii.GetBalanceBoard(0);
        var cop = Wii.GetCenterOfBalance(0);
        cop.y -= _offset; //apply offset to cop

        if (Mathf.Abs(cop.x) > 1f || Mathf.Abs(cop.y) > 1f) //com should not extend outside the range of the board
        {
            if (cop.x > 1f)
                cop.x = 1f;
            else
                cop.x = 0f; // if it's not above 1 then it has to be below -1

            if (cop.y > 1f)
                cop.y = 1f;
            else
                cop.y = 0f; // if it's not above 1 then it has to be below -1
        }

        var fcopX = 0.0f;
        var fcopY = 0.0f;

        //set 0 to default in case it isn't set, also don't want filtering in LOS or assessment
        if (PlayerPrefs.GetInt("Filter Data", 0) == 1 && _sceneName != "Assessment" && _sceneName != "LOS") 
        {
            // comX = taredCOP.x - _i/(_m*_G*_h); //incomplete, need to figure out a way to get COM from wii balance board
            // comY = taredCOP.y - _i/(_m*_G*_h);

            fcopX = _filterX.ComputeBW(cop.x);
            fcopY = _filterY.ComputeBW(cop.y);
        }
        else
        {
            fcopX = cop.x;
            fcopY = cop.y;
        }
        
        var data = new WiiBoardData(Time.fixedUnscaledTime, 
                                    cop.x, cop.y, 
                                    boardSensorValues.y, 
                                    boardSensorValues.x, 
                                    boardSensorValues.w, 
                                    boardSensorValues.z,
                                    fcopX, fcopY);
        return data;
    }

    private void GetTargetCoords()
    {
        TargetCoords = new Vector2();
        //finding circle for colour and hunting handled here since it changes constantly in game
        switch (_sceneName)
        {
            case "Colour Matching":
                if (_targetCircle == null)
                    //using findgameobjectwithtag is faster since it's more like searching through dict
                    _targetCircle = GameObject.FindGameObjectWithTag("Target");

                break;
            case "Hunting":
                if (_targetCircle == null)
                    //using findgameobjectwithtag is faster since it's more like searching through dict
                    _targetCircle = GameObject.FindGameObjectWithTag("Target");

                break;
        }

        if (_sceneName != "Target")
            TargetCoords = _targetCircle.transform.position;
        else //target game circle never leaves the centre
            TargetCoords = new Vector2(0.0f, 0.0f);
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

    private void ChangeTarget() => _targetCircle = null; //set _targetCircle to null so that the new target circle can be selected

    private void OnDisable() //unsubscribe when cursor is destroyed to avoid memory leaks
    {
        GameSession.DirectionChangeEvent -= ChangeFileLOS;
        GameSession.ConditionChangeEvent -= ChangeFileAssessment;
        GameSession.ColourChangeEvent -= ChangeTarget;
        GameSession.TargetChangeEvent -= ChangeTarget;
    }
}