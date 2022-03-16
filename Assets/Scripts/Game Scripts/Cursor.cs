using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using FilterManager;

public class Cursor : MonoBehaviour
{
    [Tooltip("The minimum x or the left side edge of the camera")]
    [SerializeField] private float _minX = 0f;
    [Tooltip("The maximum x or the right side edge of the camera")]
    [SerializeField] private float _maxX = 2f*5f*16f/9f; //2*height*aspect ratio
    [Tooltip("The minimum y or the bottom edge of the camera")]
    [SerializeField] private float _minY = 0f;
    [Tooltip("The maximum y or the top edge of the camera")]
    [SerializeField] private float _maxY = 5f*2f; //2*camera size
    [Tooltip("The shift in the centre of the LOS rectangles wrt. centre of the camera. Get this value from the difference between the rectangles prefab vs the camera")]
    [SerializeField] private float _rectanglesShift = 0.3f; //can be adjusted if the position of rectangles changes in the future.

    private string _sceneName;
    private float _mass;
    private float _height;
    private float _m; // kg
    private float _h; // m
    private float _i; // kgm^2
    private float _lengthOffset;
    private float _ankleLength;
    private float _ankleDisplacement; //to shift everything to the reference point of the ankle (ie. ankle is at y = 0)
    private List<float> _limits;
    private GameObject _rectangles;
    private Vector4 _zero;
    private Filter _filterX;
    private Filter _filterY;

    private const float _XWidth = 433f; // mm
    private const float _YLength = 238f; // mm
    private const float _HeelLocation = 90; //mm, measured manually from centre of board to bottom of indicated feet area

    public WiiBoardData Data { get; private set; }
    public float LOSShift
    {
        get => _rectanglesShift;
    }

    private void Awake() //want to compute these values before anything starts
    {
        _sceneName = SceneManager.GetActiveScene().name;
        _mass = PlayerPrefs.GetFloat("Mass");
        _height = PlayerPrefs.GetInt("Height")/100f; //convert to m
        _ankleLength = PlayerPrefs.GetFloat("Ankle Fraction")*_height/100f; //convert percent to fraction, cm to m
        _m = PlayerPrefs.GetFloat("Ankle Mass Fraction")*_mass;
        _h = PlayerPrefs.GetFloat("CoM Fraction")*_height;
        _i = PlayerPrefs.GetFloat("Inertia Coefficient")*_mass*Mathf.Pow(_height, 2);
        _ankleDisplacement = _HeelLocation/1000f - _ankleLength;
        _zero = new Vector4(PlayerPrefs.GetFloat("Top Right Sensor", 0f), PlayerPrefs.GetFloat("Top Left Sensor", 0f), PlayerPrefs.GetFloat("Bottom Right Sensor", 0f), PlayerPrefs.GetFloat("Bottom Left Sensor", 0f));

        if (_sceneName == "LOS")
            _rectangles = GameObject.Find("Rectangles"); //find the rectangles for los to get shifted y coord

        if (_sceneName == "LOS" || _sceneName == "Assessment") //only shift and scale cop when it's the games
        {
            _limits = new List<float>() {1.0f, 1.0f, 1.0f, 1.0f}; //front, back, left, right

            if (_sceneName == "LOS") //offset applied for LOS only, for qs assessment offset is default zero
                _lengthOffset = PlayerPrefs.GetFloat("Length Offset", 0.0f)/100f; //convert from percent to fraction
        }
        else
        {
            // need to convert from percent to fraction
            // los is in qs frame of reference but need to remove shift that's inherent to los
            _limits = new List<float>()
            {
                PlayerPrefs.GetFloat("Limit of Stability Front", 1.0f)/100f - _rectanglesShift*2f/_maxY,
                PlayerPrefs.GetFloat("Limit of Stability Back", 1.0f)/100f - _rectanglesShift*2f/_maxY,
                PlayerPrefs.GetFloat("Limit of Stability Left", 1.0f)/100f,
                PlayerPrefs.GetFloat("Limit of Stability Right", 1.0f)/100f
            };

            _lengthOffset = PlayerPrefs.GetFloat("Length Offset", 0.0f)/100f; //convert from percent to fraction
        }
    }

    private void Start() 
    {
        SetInitialConditions();
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void SetInitialConditions()
    {
        if (PlayerPrefs.GetInt("Filter Data", 0) != 1) //set 0 as default in case it isn't set
            return;

        _filterX = new Filter(PlayerPrefs.GetInt("Filter Order")); //moving average, doesn't work with wii balance board right now
        _filterY = new Filter(PlayerPrefs.GetInt("Filter Order"));

        _filterX = new Filter(0.4615f, 1.0f / Time.fixedDeltaTime, PlayerPrefs.GetInt("Filter Order")); //bw temporary for now
        _filterY = new Filter(0.4615f, 1.0f / Time.fixedDeltaTime, PlayerPrefs.GetInt("Filter Order"));
    }

    private void Move()
    {
        if ((bool)FindObjectOfType<WiiBoard>() && Wii.IsActive(0) && Wii.GetExpType(0) == 3)
        {
            Data = GetBoardValues();

            var pos = new Vector2();
            var com = new Vector2();
            var xLimit = 0.0f;
            var yLimit = 0.0f;

            if (PlayerPrefs.GetInt("Filter Data", 0) == 1) //set default to zero in case it isn't set
                com = new Vector2(Data.fCopX, Data.fCopY);
            else
                com = new Vector2(Data.copX, Data.copY); //com == cop if no filtering

            // scale the cursor on screen to the individual's max in each direction
            if (com.x > 0)
                xLimit = _limits[3];
            else
                xLimit = _limits[2];

            if (com.y > 0)
                yLimit = _limits[0];
            else
                yLimit = _limits[1];

            var multiplier = 1;

            if (_sceneName == "Target") //the cursor is twice as sensitive in target game
                multiplier = 2;

            pos.x = Mathf.Clamp(multiplier*(com.x / xLimit) * (_maxX / 2) + Camera.main.transform.position.x, _minX, _maxX);
            pos.y = Mathf.Clamp(multiplier*(com.y / yLimit) * (_maxY / 2) + Camera.main.transform.position.y, _minY, _maxY);

            transform.position = pos;
        }
        else
        {
            // debugging using mouse, mouse is already in qs frame of reference
            var pos = new Vector2();
            pos.x = Mathf.Clamp(Input.mousePosition.x / Screen.width * _maxX, _minX, _maxX);
            pos.y = Mathf.Clamp(Input.mousePosition.y / Screen.height * _maxY, _minY, _maxY);
            transform.position = pos;
            
            var xLimit = 0.0f;
            var yLimit = 0.0f;

            if (pos.x > _maxX/2f)
                xLimit = _limits[3];
            else
                xLimit = _limits[2];

            if (pos.y > _maxY/2f)
                yLimit = _limits[0];
            else
                yLimit = _limits[1];

            //transform the mouse position into a board position
            //need to account for _lengthOffset since targets in game are with respect to the cop shifted to the quiet standing centre of pressure.
            pos.x = (pos.x - Camera.main.transform.position.x)*xLimit*_XWidth/1000f/_maxX;
            pos.y = (pos.y - Camera.main.transform.position.y)*yLimit*_YLength/1000f/_maxY + _ankleDisplacement + _lengthOffset*_YLength/1000f/2f; 
            
            Data = new WiiBoardData(Time.fixedUnscaledTime, pos.x, pos.y, 0f, 0f, 0f, 0f, pos.x, pos.y); // using mouse data for controller
        }
    }

    private WiiBoardData GetBoardValues()
    {
        // Zero out the sensor values if possible, if no zeroing, then zero vector is just all zeros by default
        var boardSensorValues = Wii.GetBalanceBoard(0) - _zero;
        var totalVertical = boardSensorValues.x + boardSensorValues.y + boardSensorValues.z + boardSensorValues.w;
        var copX = (boardSensorValues.x + boardSensorValues.z - boardSensorValues.y - boardSensorValues.w) / totalVertical;
        var copY = (boardSensorValues.x + boardSensorValues.y - boardSensorValues.z - boardSensorValues.w) / totalVertical;
        var cop = new Vector2(copX, copY);

        if (_sceneName != "LOS")
            cop.y -= _lengthOffset; //subtract AP offset to centre QS to 0,0, subtracting a negative
        else 
        {
            //additional shift due to the fact that the LOS centre is slightly shifted down
            //also need to convert game coordinates to percentage of length of board
            cop.y -= _lengthOffset - _rectanglesShift*2f/_maxY;
        } 

        var fCopX = 0.0f;
        var fCopY = 0.0f;

        //set 0 to default in case it isn't set, also don't want filtering in LOS or assessment
        if (PlayerPrefs.GetInt("Filter Data", 0) == 1 && _sceneName != "Assessment" && _sceneName != "LOS") 
        {
            fCopX = _filterX.ComputeBW(cop.x);
            fCopY = _filterY.ComputeBW(cop.y);
        }
        else
        {
            fCopX = cop.x;
            fCopY = cop.y;
        }
        
        var data = new WiiBoardData(Time.fixedUnscaledTime, 
                                    cop.x, cop.y,
                                    boardSensorValues.y, boardSensorValues.x, 
                                    boardSensorValues.w, boardSensorValues.z,
                                    fCopX, fCopY);
        return data;
    }
}