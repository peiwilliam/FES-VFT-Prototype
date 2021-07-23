using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using CSV;
using FilterManager;

public class Cursor : MonoBehaviour
{
    [SerializeField] private float _minX = 0f;
    [SerializeField] private float _maxX = 2f*5f*16f/9f; //2*height*aspect ratio
    [SerializeField] private float _minY = 0f;
    [SerializeField] private float _maxY = 5f*2f; //2*camera size

    private const float _XWidth = 433; // mm
    private const float _YLength = 228; // mm
    private const float _G = 9.81f; // m/s^2
    private float _mass;
    private float _height;
    private float _m; // kg
    private float _h; // m
    private float _i; // kgm^2
    private List<float> _limits;
    private float _lengthOffset;
    private string _sceneName;
    private GameObject _targetCircle;
    private Filter _filterX;
    private Filter _filterY;
    private CSVWriter _writer;

    public WiiBoardData Data { get; private set; }
    public Vector2 TargetCoords {get; private set; }
    public bool LOSStarted { get; private set; }
    public bool ECAssessmentStarted { get; private set; }
    public bool EOAssessmentStarted { get; private set; }

    private void Awake() //want to compute these values before anything starts
    {
        _mass = PlayerPrefs.GetFloat("Mass");
        _height = PlayerPrefs.GetFloat("Height")/100f; //convert to m
        _m = PlayerPrefs.GetFloat("Ankle Mass Fraction")*_mass;
        _h = PlayerPrefs.GetFloat("CoM Fraction")*_height;
        _i = PlayerPrefs.GetFloat("Inertia Coefficient")*_mass*Mathf.Pow(_height, 2);
        _sceneName = SceneManager.GetActiveScene().name;

        if (_sceneName == "LOS" || _sceneName == "Assessment") //only shift and scale cop when it's the games
        {
            _limits = new List<float>() {1.0f, 1.0f, 1.0f, 1.0f}; //front, back, left, right

            if (_sceneName == "LOS")
                _lengthOffset = PlayerPrefs.GetFloat("Length Offset", 0.0f);
        }
        else
        {
            _limits = new List<float>() 
            {
                PlayerPrefs.GetFloat("Limit of Stability Front", 1.0f),
                PlayerPrefs.GetFloat("Limit of Stability Back", 1.0f),
                PlayerPrefs.GetFloat("Limit of Stability Left", 1.0f),
                PlayerPrefs.GetFloat("Limit of Stability Right", 1.0f)
            };

            _lengthOffset = PlayerPrefs.GetFloat("Length Offset", 0.0f);
        }
    }

    // private void OnEnable() //subscribing to event handled here
    // {
    //     GameSession.DirectionChangeEvent += ChangeFileLOS;
    //     GameSession.ConditionChangeEvent += ChangeFileAssessment;
    // }

    private void Start() 
    {
        if ((bool)FindObjectOfType<WiiBoard>() && Wii.IsActive(0) && Wii.GetExpType(0) == 3)
            SetInitialConditions();
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void SetInitialConditions()
    {
        if (PlayerPrefs.GetInt("Filter Data", 0) == 1) //set 0 as default in case it isn't set
        {
            _filterX = new Filter(PlayerPrefs.GetInt("Filter Order")); //moving average, doesn't work with wii balance board right now
            _filterY = new Filter(PlayerPrefs.GetInt("Filter Order"));

            _filterX = new Filter(0.4615f, 1.0f / Time.fixedDeltaTime, PlayerPrefs.GetInt("Filter Order")); //bw temporary for now
            _filterY = new Filter(0.4615f, 1.0f / Time.fixedDeltaTime, PlayerPrefs.GetInt("Filter Order"));
        }
    }

    private void Move()
    {
        if ((bool)FindObjectOfType<WiiBoard>() && Wii.IsActive(0) && Wii.GetExpType(0) == 3)
        {
            Data = GetBoardValues();

            var pos = new Vector2(transform.position.x, transform.position.y);
            var com = new Vector2();

            if (PlayerPrefs.GetInt("Filter Data", 0) == 1) //set default to zero in case it isn't set
                com = new Vector2(Data.fCopX, Data.fCopY);
            else
                com = new Vector2(Data.copX, Data.copY); //com == cop if no filtering

            // subtract the offset and scale the cursor on screen to the individual's max in each direction
            if (com.x > 0)
                pos.x = Mathf.Clamp((com.x / _limits[3]) * (_maxX / 2) + Camera.main.transform.position.x, _minX, _maxX);
            else
                pos.x = Mathf.Clamp((com.x / _limits[2]) * (_maxX / 2) + Camera.main.transform.position.x, _minX, _maxX);

            if (com.y > 0)
                pos.y = Mathf.Clamp(((com.y) / _limits[0]) * (_maxY / 2) + Camera.main.transform.position.y, _minY, _maxY);
            else
                pos.y = Mathf.Clamp(((com.y) / _limits[1]) * (_maxY / 2) + Camera.main.transform.position.y, _minY, _maxY);

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
        cop.y -= _lengthOffset; //subtract offset from AP direction

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
        var fcopY = 0.0f; //apply offset to cop

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

    // private void ChangeFileAssessment(string condition) //activates when starting or changing condition in assessment
    // {
    //     _writer = new CSVWriter(condition);
    //     _writer.WriteHeader();

    //     if (!GameSession._ecDone && !GameSession._eoDone) //check which condition it is and ensure that the correct files are created
    //         ECAssessmentStarted = true;
    //     else if (!GameSession._eoDone)
    //         EOAssessmentStarted = true;
    // }

    // private void ChangeFileLOS(string direction) //activates when direction changes in los
    // {
    //     _writer = new CSVWriter(direction);
    //     _writer.WriteHeader();
    //     LOSStarted = true;
    // }

    // private void OnDisable() //unsubscribe when cursor is destroyed to avoid memory leaks
    // {
    //     GameSession.DirectionChangeEvent -= ChangeFileLOS;
    //     GameSession.ConditionChangeEvent -= ChangeFileAssessment;
    // }
}