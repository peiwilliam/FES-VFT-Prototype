using FilterManager;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Cursor : MonoBehaviour
{
    [SerializeField] private float _minX = 0f;
    [SerializeField] private float _maxX = 2f*5f*16f/9f; //2*height*aspect ratio
    [SerializeField] private float _minY = 0f;
    [SerializeField] private float _maxY = 5f*2f; //2*camera size
    [SerializeField] private Vector2 _initialCOP;

    private const float _Length = 433; // mm
    private const float _Width = 228; // mm
    private const float _G = 9.81f; // m/s^2
    private float _mass;
    private float _height;
    private float _m; // kg
    private float _h; // m
    private float _i; // kgm^2
    private Filter _filterX;
    private Filter _filterY;
    private CSVWriter _writer;
    private GameSession _gameSession;

    private void Awake() //want to compute these values before anything starts
    {
        _mass = PlayerPrefs.GetFloat("Mass");
        _height = PlayerPrefs.GetFloat("Height");
        _m = PlayerPrefs.GetFloat("Ankle Mass Fraction")*_mass;
        _h = PlayerPrefs.GetFloat("CoM Fraction")*_height;
        _i = PlayerPrefs.GetFloat("Inertia Coefficient")*_mass*Mathf.Pow(_height, 2);   
    }

    private void Start() 
    {
        if ((bool)FindObjectOfType<WiiBoard>() && Wii.IsActive(0) && Wii.GetExpType(0) == 3)
        {
            SetBoardConditions();
        }

        _gameSession = FindObjectOfType<GameSession>();
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void SetBoardConditions()
    {
        if (PlayerPrefs.GetInt("Zero Board", 0) == 1) //set 0 as default in case it's not set
        {
            // var initSenVals = new float[] //don't really need this atm, the idle values are really weird, so not worth doing this
            // {
            //         PlayerPrefs.GetFloat("Top Left Sensor"),
            //         PlayerPrefs.GetFloat("Top Right Sensor"),
            //         PlayerPrefs.GetFloat("Bottom Left Sensor"),
            //         PlayerPrefs.GetFloat("Bottom Right Sensor")
            // };

            // var copX = (initSenVals[1] + initSenVals[3] - initSenVals[0] - initSenVals[2]) / (initSenVals[0] + initSenVals[1] + initSenVals[2] + initSenVals[3]);
            // var copY = (initSenVals[0] + initSenVals[1] - initSenVals[2] - initSenVals[3]) / (initSenVals[0] + initSenVals[1] + initSenVals[2] + initSenVals[3]);

            // _initialCOP = new Vector2(copX, copY);
        }
        else
            _initialCOP = new Vector2(0, 0);

        if (PlayerPrefs.GetInt("Filter Data", 0) == 1) //set 0 as default in case it isn't set
        {
            _filterX = new Filter(PlayerPrefs.GetInt("Filter Order"));
            _filterY = new Filter(PlayerPrefs.GetInt("Filter Order"));
        }

        _writer = new CSVWriter();
        _writer.WriteHeader();
    }

    private void Move()
    {
        if ((bool)FindObjectOfType<WiiBoard>() && Wii.IsActive(0) && Wii.GetExpType(0) == 3)
        {
            var data = GetBoardValues();

            _writer.WriteDataAsync(data);

            var pos = new Vector2(transform.position.x, transform.position.y);
            var cop = new Vector2();

            if (PlayerPrefs.GetInt("Filter Data", 0) == 1) //set default to zero in case it isn't set
                cop = new Vector2(data.fCopX, data.fCopY);
            else
                cop = new Vector2(data.copX, data.copY);

            pos.x = Mathf.Clamp(cop.x * _maxX / 2 + Camera.main.transform.position.x, _minX, _maxX);
            pos.y = Mathf.Clamp(cop.y * _maxY / 2 + Camera.main.transform.position.y, _minY, _maxY);

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

    public WiiBoardData GetBoardValues()
    {
        var boardSensorValues = Wii.GetBalanceBoard(0);
        var taredCOP = Wii.GetCenterOfBalance(0) - _initialCOP;

        if (Mathf.Abs(taredCOP.x) > 1f || Mathf.Abs(taredCOP.y) > 1f) //cop should not extend outside the range of the board
        {
            if (taredCOP.x > 1f)
                taredCOP.x = 1f;
            else
                taredCOP.x = 0f; // if it's not above 1 then it has to be below -1

            if (taredCOP.y > 1f)
                taredCOP.y = 1f;
            else
                taredCOP.y = 0f; // if it's not above 1 then it has to be below -1
        }

        var comX = 0.0f;
        var comY = 0.0f;
        var sceneName = SceneManager.GetActiveScene().name;

        if (PlayerPrefs.GetInt("Filter Data", 0) == 1 && sceneName != "Assessment" && sceneName != "LOS") //set 0 to default in case it isn't set, also don't want filtering in LOS or assessment
        {
            comX = taredCOP.x - _i/(_m*_G*_h); //incomplete, need to figure out a way to get COM from wii balance board
            comY = taredCOP.y - _i/(_m*_G*_h);

            comX = _filterX.ComputeMA(taredCOP.x);
            comY = _filterY.ComputeMA(taredCOP.y);
        }
        
        var data = new WiiBoardData(Time.fixedUnscaledTime, 
                                    taredCOP.x, taredCOP.y, 
                                    boardSensorValues.y, 
                                    boardSensorValues.x, 
                                    boardSensorValues.w, 
                                    boardSensorValues.z,
                                    comX, comY);
        return data;
    }
}