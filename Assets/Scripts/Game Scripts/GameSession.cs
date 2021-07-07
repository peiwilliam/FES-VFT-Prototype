using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using KnuthShuffle;
using TMPro;

public class GameSession : MonoBehaviour
{
    [Header("Assessment")]
    [SerializeField] private List<string> _assessInstructions;
    [SerializeField] private InputField _assessInstructionsBox;

    public static bool _ecDone; //public static so cursor is able to access these values
    public static bool _eoDone;

    public delegate void OnConditionChange(string condition);
    public static event OnConditionChange ConditionChangeEvent;

    private Dictionary<string, List<WiiBoardData>> _qsAssessment;
    private Cursor _cursor;

    [Header("Limits of Stability")]
    [SerializeField] private InputField _losInstructionsBox;
    [SerializeField] private int _counter;
    [SerializeField] private float _windowSize;
    [SerializeField] private bool _shuffled;
    [SerializeField] private List<SpriteRenderer> _rectangles;
    [SerializeField] private string _direction;
    
    //event for handling the changing of directions, can't use unity event because cursor is instan. at the beginning and doesnn't exist before start of game
    public delegate void OnDirectionChange(string direction);
    public static event OnDirectionChange DirectionChangeEvent;

    private List<string> _directionNames = new List<string>(); //list of direction names, public static so cursor has access to it and not a field
    private Dictionary<string, string> _losInstructions; //dictionary of instructions text for each direction
    private Dictionary<string, List<WiiBoardData>> _directionData; //dictionary to store data from each direction
    private Dictionary<string, float> _limits; //dictionary to store the limits

    [Header("All Games")]
    [SerializeField] private GameObject _cursorPrefab;
    [SerializeField] private float _totalGameTime = 100f;
    [SerializeField] private Text _timeText;

    private float _totalGameDeltaTime = 1f; //incrementing timer by 1 sec each time, doesn't need to be changed
    private Coroutine _timer;
    private SceneLoader _sceneLoader;
    
    [Header("Ellipse Game")]
    [SerializeField] private GameObject _movingCirclePrefab;
    [SerializeField] private Ellipse _ellipse;

    private MovingCircle _movingCircle;

    // for the _ellipse variable so that moving circle can also get access to ellipse
    public Ellipse Ellipse
    {
        get => _ellipse;
    }
    public LineRenderer LineRenderer { get; set; }
    public Vector3[] Positions { get; set; }
    public int EllipseIndex { get; private set; }
    public int EllipseScore { get; private set; }

    [Header("Colour Matching Game")]
    [SerializeField] private float _colourDuration = 10f;
    [SerializeField] private bool _conditionColourMet;
    [SerializeField] private List<ColourCircle> _colourCircles;
    //default colours, need to change if the colours have been changed
    [SerializeField] private List<string> _colourTexts;
    [SerializeField] private UnityEvent _colourChangeEvent;

    private TextMeshProUGUI _colourText;
    private Coroutine _changeColour;

    public int ColourMatchingScore { get; private set; }
    public ColourCircle TargetColourCircle { get; private set; }
    //for the _conditionColourMet variable so that other classes can easily access
    public bool ConditionColourMet 
    {
        get => _conditionColourMet;
        set => _conditionColourMet = value;
    }

    [Header("Hunting Game")]
    [SerializeField] private GameObject _huntingCirclePrefab;
    [SerializeField] private float _minX = 0f;
    [SerializeField] private float _maxX = 2f*5f*16f/9f; //2*height*aspect ratio
    [SerializeField] private float _minY = 0f;
    [SerializeField] private float _maxY = 5f*2f; //2*camera size
    [SerializeField] private float _huntingDuration = 10f;
    [SerializeField] private bool _conditionHuntingMet;
    
    private HuntingCircle _huntingCircle;

    public int HuntingScore { get; private set; }
    //for the _conditionHuntingMet variable so that other classes can easily access
    public bool ConditionHuntingMet
    {
        get => _conditionHuntingMet;
        set => _conditionHuntingMet = value;
    }

    [Header("Target Game")]
    [SerializeField] private List<TargetCircle> _targetCircles;
    [SerializeField] private float _deltaTimeScore = 0.25f;

    private Coroutine _increaseScore;

    public float TargetScore { get; private set; }
    //for the _deltaTimeScore variable so that other classes can easily access
    public float DeltaTimeScore
    {
        get => _deltaTimeScore;
        set => _deltaTimeScore = value;
    }

    private void Start()
    {
        Instantiate(_cursorPrefab, new Vector3(0, 0, 0), Quaternion.identity);

        var sceneName = SceneManager.GetActiveScene().name;
        _sceneLoader = FindObjectOfType<SceneLoader>();

        switch (sceneName)
        {
            case "Assessment":
                _qsAssessment = new Dictionary<string, List<WiiBoardData>>() 
                {
                    ["EC"] = new List<WiiBoardData>(),
                    ["EO"] = new List<WiiBoardData>()
                };

                SetupAssessment();
                
                break;
            case "LOS":
                _cursor = FindObjectOfType<Cursor>();
                _directionNames = new List<string>(from rectangle in _rectangles select rectangle.name); //linq syntax
                //first one is for the keys, the second is for the values
                _directionData = _directionNames.ToDictionary(v => v, v => new List<WiiBoardData>());
                _losInstructions = _directionNames.ToDictionary(v => v, v => "Please lean " + v.ToLower() + " as far as you can and hold for 3 seconds.");
                _losInstructionsBox.text = "Press Start to start the Limits of Stability Assessment. Be prepared to lean in one of the 8 directions.";

                break;
            case "Colour Matching":
                _colourCircles = FindObjectsOfType<ColourCircle>().ToList();
                _colourTexts = new List<string>(from circle in _colourCircles select circle.name); //linq syntax

                if (_colourChangeEvent == null)
                    _colourChangeEvent = new UnityEvent(); 

                ColourMatchingGame();

                break;
            case "Ellipse":
                EllipseGame();
                _movingCircle = FindObjectOfType<MovingCircle>();

                break;
            case "Hunting":
                HuntingGame();

                break;
            case "Target":
                _targetCircles = FindObjectsOfType<TargetCircle>().ToList();

                break;
        }

        if (sceneName != "Assessment" && sceneName != "LOS") //timer for assessment and LOS started manually
            StartCoroutine(StartTimer());
    }

    private void Update() //probably use update, though case can be made for fixedupdate
    {
        // with these two games, the scores are not dependent on getting to targets, so it's just one cumulative score
        switch (SceneManager.GetActiveScene().name)
        {
            case "LOS":
                LOS();
                break;
            case "Ellipse":
                EllipseGameScore();
                break;
            case "Target":
                TargetGameScore();
                break;
        }

        if (_timeText != null) //LOS doesn't have this so don't want to create null exception
            _timeText.text = _totalGameTime.ToString();

        //how to handle transitions for all scenes other than assessment
        if ((_totalGameTime <= 0 && SceneManager.GetActiveScene().name != "Assessment") || _eoDone)
        {
            if (SceneLoader.GetFamiliarization() && SceneLoader.GetGameIndex() < 4)
                _sceneLoader.Familiarization();
            else if (SceneLoader.GetExperimentation() && SceneLoader.GetGameIndicesIndex() < 4)
                _sceneLoader.Experimentation();
            else   
                _sceneLoader.LoadStartScene();
        }
    }

    private void FixedUpdate() //fixed update to keep it in sync with cursor data
    {
        switch (SceneManager.GetActiveScene().name)
        {
            case "Assessment":
                if (_timer != null) //timer for the assessment has started
                {
                    Assessment();

                    if (_totalGameTime <= 0) //reset for eyes open condition
                    {
                        if (_ecDone && !_eoDone)
                        {
                            _timer = null;
                            _assessInstructionsBox.text = _assessInstructions[1];
                            _totalGameTime = 100;
                        }
                        else if (_ecDone && _eoDone)
                        {
                            _sceneLoader.LoadStartScene();
                            var eoOrEc = true; //temporary eo = true, ec = false
                            List<float> yValues;

                            if (eoOrEc)
                                yValues = new List<float>(from value in _qsAssessment["EO"] select value.copY); //linq syntax
                            else
                                yValues = new List<float>(from value in _qsAssessment["EC"] select value.copY);

                            PlayerPrefs.SetFloat("Length Offset", yValues.Average());
                        }
                    }
                }
                break;
            case "LOS":
                var data = _cursor.GetBoardValues();

                if (!string.IsNullOrEmpty(_direction))
                    _directionData[_direction].Add(data);

                break;
        }
    }

    private void SetupAssessment() => _assessInstructionsBox.text = _assessInstructions[0];

    private void Assessment() //I think this works? needs more testing
    {
        var data = _cursor.GetBoardValues();

        if (!_ecDone)
            _qsAssessment["EC"].Add(data);
        else
            _qsAssessment["EO"].Add(data);
    }

    public void StartAssessmentTimer()
    {
        _cursor = FindObjectOfType<Cursor>();
        _cursor.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0); //alpha is zero to make it invisible
        
        _timer = StartCoroutine(StartTimer());

        if (ConditionChangeEvent != null)
        {
            if (!_ecDone)
                ConditionChangeEvent(_qsAssessment.Keys.ToList()[0]);
            else if (!_eoDone)
                ConditionChangeEvent(_qsAssessment.Keys.ToList()[1]);
        }
    }
    
    private void LOS()
    {
        foreach (var rectangle in _rectangles)
        {
            if (rectangle.tag == "Target" && rectangle.color != Color.green)
            {
                rectangle.color = Color.green;
                rectangle.sortingOrder = 1;
            }
            else if (rectangle.tag == "Untagged" && rectangle.color != new Color(0, 0.2775006f, 1f))
            {
                rectangle.color = new Color(0f, 0.2775006f, 1f);
                rectangle.sortingOrder = 0;
            }
        }
    }

    public void StartLOS()
    {
        if (FindObjectOfType<Cursor>() == null) // make sure only one cursor is spawned
            Instantiate(_cursorPrefab, new Vector3(0, 0, 0), Quaternion.identity); //instantiate at button click instead of at beginning

        if (GameObject.FindGameObjectWithTag("Target") != null) //once we press the button, we want to switch to a new target
            GameObject.FindGameObjectWithTag("Target").tag = "Untagged";

        if (_counter == _directionNames.Count) //do this check at the beginning so that the last direction isn't just ended
        {
            _sceneLoader.LoadStartScene();
            var windowLength = PlayerPrefs.GetInt("Rolling Average Window");
            
            GetLimits(windowLength);
        }
        else
        {
            if (!_shuffled)
            {
                _directionNames = KnuthShuffler.Shuffle(_directionNames);
                _shuffled = true;
            }

            _direction = _directionNames[_counter++]; //want incrementation after using _counter
            _losInstructionsBox.text = _losInstructions[_direction];
            var rectangle = _rectangles.Find(n => n.name == _direction);
            rectangle.tag = "Target";

            if (DirectionChangeEvent != null) //invoke the direction change event
                DirectionChangeEvent(_direction);
        }
    }

    private void GetLimits(int windowLength) //calculation of limits current hella incorrect
    {
        _limits = new Dictionary<string, float>();
        var averages = new List<float>();
        
        if (_directionData.Values != null) //check if the board was used, if it's just cursor, the vales will be null
        {
            foreach (var direction in _directionData)
            {
                print(direction.Key);
                switch (direction.Key)
                {
                    case "Forward":
                    case "Back":
                        for (var i = 0; i <= direction.Value.Count - windowLength; i++)
                        {
                            // Makes each value absolute value because we only care about the magnitude
                            var rangeOfValues = new List<float>(from value in direction.Value.GetRange(i, windowLength) select Mathf.Abs(value.copY));
                            averages.Add(rangeOfValues.Average());
                        }
                        print("here1");

                        break;
                    case "Left":
                    case "Right":
                        for (var i = 0; i <= direction.Value.Count - windowLength; i++)
                        {
                            // Makes each value absolute value because we only care about the magnitude
                            var rangeOfValues = new List<float>(from value in direction.Value.GetRange(i, windowLength) select Mathf.Abs(value.copX));
                            averages.Add(rangeOfValues.Average());
                        }
                        print("here2");

                        break;
                }

                _limits.Add(direction.Key, averages.Max());
            }

            PlayerPrefs.SetFloat("Limit of Stability Front", _limits["Forward"] * 0.8f); //store values, want just 80% of max
            PlayerPrefs.SetFloat("Limit of Stability Back", _limits["Back"] * 0.8f);
            PlayerPrefs.SetFloat("Limit of Stability Left", _limits["Left"] * 0.8f);
            PlayerPrefs.SetFloat("Limit of Stability Right", _limits["Right"] * 0.8f);
        }
        else
            Debug.Log("Cursor was used, no limit data colleccted.");
        
        return;
    }

    private void ColourMatchingGame()
    {
        _colourText = FindObjectOfType<TextMeshProUGUI>();
        
        var averageDistance = 0f;

        foreach (var circle in _colourCircles)
            foreach (var otherCircle in _colourCircles)
                averageDistance += Mathf.Sqrt(Mathf.Pow(circle.transform.position.x - otherCircle.transform.position.x, 2) + 
                                              Mathf.Pow(circle.transform.position.y - otherCircle.transform.position.y, 2));

        averageDistance /= Mathf.Pow(_colourCircles.Count, 2);

        _changeColour = StartCoroutine(ColourSelection(averageDistance));
    }

    private void EllipseGame()
    {
        LineRenderer = Ellipse.GetComponent<LineRenderer>();
        Positions = new Vector3[LineRenderer.positionCount];
        LineRenderer.GetPositions(Positions); //pos has an out on it
        EllipseIndex = Random.Range(0, Positions.Length);
        Instantiate(_movingCirclePrefab, new Vector3(Positions[EllipseIndex].x, Positions[EllipseIndex].y, 0), Quaternion.identity);
    }

    private void HuntingGame() => StartCoroutine(SpawnCircles());

    private IEnumerator SpawnCircles()
    {
        var rand = new System.Random(); //use system.random because I have more control over what I randomize
        var quads = new List<int>() {1, 2, 3, 4}; //quadrants
        var pos = new float[2]; //always will be size 2
        var prevQuad = 0;

        while (true) //keeps looping thorugh coroutine while it's active
        {
            var quad = quads[rand.Next(quads.Count)];
            var oldPos = pos;
            
            switch (quad)
            {
                case 1:
                    pos = GetPositions(_maxX / 2f, _maxX - _huntingCirclePrefab.transform.localScale.x, 
                                    _maxY / 2f, _maxY - _huntingCirclePrefab.transform.localScale.y, oldPos);
                    break;
                case 2:
                    pos = GetPositions(_minX + _huntingCirclePrefab.transform.localScale.x, _maxX/2f, 
                                    _maxY / 2f, _maxY - _huntingCirclePrefab.transform.localScale.y, oldPos);
                    break;
                case 3:
                    pos = GetPositions(_minX + _huntingCirclePrefab.transform.localScale.x, _maxX/2f, 
                                    _minY + _huntingCirclePrefab.transform.localScale.y, _maxY/2f, oldPos);
                    break;
                case 4:
                    pos = GetPositions(_maxX/2f, _maxX - _huntingCirclePrefab.transform.localScale.x, 
                                    _minY + _huntingCirclePrefab.transform.localScale.y, _maxY/2f, oldPos);
                    break;
            }

            Instantiate(_huntingCirclePrefab, new Vector3(pos[0], pos[1], 0), Quaternion.identity);
            
            _huntingCircle = FindObjectOfType<HuntingCircle>();

            while (_huntingDuration > 0 && !_conditionHuntingMet)
            {
                _huntingDuration -= Time.deltaTime;

                yield return null;
            }

            Destroy(FindObjectOfType<HuntingCircle>().gameObject);
            HuntingGameScore();

            if (quads.Count == 4) //when the game initially starts there are still four quads, so this takes care of that
            {
                prevQuad = quad;
                quads.Remove(quad);
            }
            else
            {
                quads.Add(prevQuad);
                prevQuad = quad;
                quads.Remove(quad);
            }

            _huntingDuration += 10f; //reset values to loop again, += to account for when the time is negative and to subtract form 10f
            _conditionHuntingMet = false;
        }   
    }

    private float[] GetPositions(float minX, float maxX, float minY, float maxY, float[] oldPos)
    {
        var xPos = Random.Range(minX, maxX);
        var yPos = Random.Range(minY, maxY);

        if (oldPos == new float[] {0f, 0f})
            return new float[] {xPos, yPos};
        else //checks to make sure that the new circle always spawns a decent distance away, threshold set at quarter of total x dist
        {
            while (Mathf.Sqrt(Mathf.Pow(xPos - oldPos[0], 2) + Mathf.Pow(yPos - oldPos[1], 2)) <= 2f*5f*16f/9f*0.25f)
            {
                xPos = Random.Range(minX, maxX);
                yPos = Random.Range(minY, maxY);
            }

            return new float[] {xPos, yPos};
        }   
    }

    private IEnumerator ColourSelection(float averageDistance)
    {
        ColourCircle oldCircle = null;

        while (true)
        {
            PickColour(averageDistance, oldCircle);
            _colourChangeEvent.Invoke();

            while (_colourDuration > 0 && !_conditionColourMet)
            {
                _colourDuration -= Time.unscaledDeltaTime;

                yield return null;
            }

            TargetColourCircle.gameObject.tag = "Untagged";
            oldCircle = TargetColourCircle;
            ColourGameScore();

            _colourDuration = 10f; //reset values to loop again, += to account for when the time is negative and to subtract form 10f
            _conditionColourMet = false;
        }
    }

    private void PickColour(float averageDistance, ColourCircle oldCircle)
    {
        var randText = _colourTexts[Random.Range(0, _colourTexts.Count)];
        TargetColourCircle = _colourCircles[Random.Range(0, _colourCircles.Count)];
        var randColour = TargetColourCircle.GetComponent<SpriteRenderer>().color;

        if (oldCircle != null) //only start checking distances after the first circle is spawned
        {
            var dist = Mathf.Sqrt(Mathf.Pow(TargetColourCircle.transform.position.x - oldCircle.transform.position.x, 2) +
                                  Mathf.Pow(TargetColourCircle.transform.position.y - oldCircle.transform.position.y, 2));

            while (dist < averageDistance)
            {
                TargetColourCircle = _colourCircles[Random.Range(0, _colourCircles.Count)];
                randColour = TargetColourCircle.GetComponent<SpriteRenderer>().color;

                dist = Mathf.Sqrt(Mathf.Pow(TargetColourCircle.transform.position.x - oldCircle.transform.position.x, 2) +
                                  Mathf.Pow(TargetColourCircle.transform.position.y - oldCircle.transform.position.y, 2));
            }
        }

        // once a colour is picked, set text, colour and add taget to target circle
        _colourText.text = randText;
        _colourText.color = randColour;
        TargetColourCircle.gameObject.tag = "Target";
    }

    private IEnumerator StartTimer()
    {
        while (_totalGameTime > 0)
        {
            yield return new WaitForSecondsRealtime(_totalGameDeltaTime);
            _totalGameTime -= _totalGameDeltaTime;
        }

        if (SceneManager.GetActiveScene().name == "Assessment")
        {
            if (!_ecDone)
            {
                _ecDone = true;
                Destroy(_cursor);
            }
            else if (!_eoDone)
                _eoDone = true;
        }
    }

    private void EllipseGameScore() => EllipseScore = _movingCircle.GetScore();

    private void ColourGameScore() => ColourMatchingScore += TargetColourCircle.GetScore();

    private void HuntingGameScore() => HuntingScore += _huntingCircle.GetScore();

    private void TargetGameScore()
    {
        var updatedScore = 0f;

        foreach (var circle in _targetCircles) //each circle stores an independent score, so need to check each one to get total
            updatedScore += circle.GetScore();

        TargetScore = updatedScore;
    }

    private void OnDisable() //mostly so that _ecDone and _eoDone are false again once assessment is done
    {
        _ecDone = false;
        _eoDone = false;
    }
}