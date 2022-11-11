using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using KnuthShuffle;
using TMPro;

/// <summary>
/// This is the main class that handles the game logic and keeping track of the score. In here you can find private members for each
/// game as well as methods that pertain to certain games. A more detailed description of the methods can be found before each method.
/// </summary>
public class GameSession : MonoBehaviour
{
    [Header("All Games")]
    [Tooltip("Place to put the cursor prefab so that it is instantiated for all games")]
    [SerializeField] private GameObject _cursorPrefab;
    [Tooltip("The object that holds how much time is left in the game")]
    [SerializeField] private Text _timeText;

    private float _totalGameDeltaTime = 1f; //incrementing timer by 1 sec each time, doesn't need to be changed
    private float _totalGameTime;
    private string _sceneName;
    private Coroutine _timer;
    private SceneLoader _sceneLoader;

    [Header("Assessment")]
    [Tooltip("Instructions to the player when doing the quiet standing assessment")]
    [SerializeField] private List<string> _assessInstructions;
    [Tooltip("The box where the instructions are displayed")]
    [SerializeField] private InputField _assessInstructionsBox;

    public static bool ecDone; //public static so cursor is able to access these values
    public static bool eoDone;

    public delegate void OnConditionChange(string condition);
    public static event OnConditionChange ConditionChangeEvent;

    private Dictionary<string, List<WiiBoardData>> _qsAssessment;
    private Cursor _cursor;
    private float _assessmentTime;

    [Header("Limits of Stability")]
    [Tooltip("Instructions to the player when doing the limits of stability test")]
    [SerializeField] private InputField _losInstructionsBox;
    [Tooltip("For debugging purposes only, keeps track of how many of the directiosn have been done")]
    [SerializeField] private int _counter;
    [Tooltip("For debugging purposes only, shows if the order that the directions show up in has been shuffled or not")]
    [SerializeField] private bool _shuffled;
    [Tooltip("The list of rectangle objects in the limits of stability test")]
    [SerializeField] private List<SpriteRenderer> _rectangles;
    [Tooltip("For debugging purposes only, shows which direction is the current direction being tested")]
    [SerializeField] private string _direction;
    
    //event for handling the changing of directions, can't use unity event because cursor is instan. at the beginning and doesn't exist before start of game
    public delegate void OnDirectionChange(string direction);
    public static event OnDirectionChange DirectionChangeEvent;

    private List<string> _directionNames = new List<string>(); //list of direction names, public static so cursor has access to it and not a field
    private Dictionary<string, string> _losInstructions; //dictionary of instructions text for each direction
    private Dictionary<string, List<WiiBoardData>> _directionData; //dictionary to store data from each direction
    private Dictionary<string, float> _limits; //dictionary to store the limits
    
    [Header("Ellipse Game")]
    [Tooltip("Place to put the moving circle prefab so that it's instantiated for the ellipse game")]
    [SerializeField] private GameObject _movingCirclePrefab;
    [Tooltip("Place to put the ellipse object so that moving circle has access to it, more efficient than FindObjectOfType")]
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
    [Tooltip("For determining if the player has stayed in the target circle long enough")]
    [SerializeField] private bool _conditionColourMet;
    [Tooltip("For storing the colour circle objects")]
    [SerializeField] private List<ColourCircle> _colourCircles;
    [Tooltip("Text of the names of the colours")]
    [SerializeField] private List<string> _colourTexts; //default colours, need to change if the colours have been changed
    [Tooltip("For handling target changing when the target circle changes")]
    [SerializeField] private UnityEvent _colourChangeEvent;

    private float _colourDuration;
    private TextMeshProUGUI _colourText;
    private Coroutine _changeColour;

    public int ColourMatchingScore { get; private set; }
    public ColourCircle TargetColourCircle { get; private set; }
    //for the _conditionColourMet variable so that colour circle has access to this variable
    public bool ConditionColourMet 
    {
        get => _conditionColourMet;
        set => _conditionColourMet = value;
    }

    [Header("Hunting Game")]
    [Tooltip("For storing the hunting circle prefab so that it can be instantiated during the game")]
    [SerializeField] private GameObject _huntingCirclePrefab;
    [Tooltip("The minimum x or the left side edge of the camera")]
    [SerializeField] private float _minX = 0f;
    [Tooltip("The maximum x or the right side edge of the camera")]
    [SerializeField] private float _maxX = 2f*5f*16f/9f; //2*height*aspect ratio
    [Tooltip("The minimum y or the bottom side edge of the camera")]
    [SerializeField] private float _minY = 0f;
    [Tooltip("The maximum y or the top side edge of the camera")]
    [SerializeField] private float _maxY = 5f*2f; //2*camera size
    [Tooltip("For determining if the player has met the condition to switch targets")]
    [SerializeField] private bool _conditionHuntingMet;
    
    private float _huntingDuration;
    private HuntingCircle _huntingCircle;

    public int HuntingScore { get; private set; }
    //for the _conditionHuntingMet variable so that other classes can easily access
    public bool ConditionHuntingMet
    {
        get => _conditionHuntingMet;
        set => _conditionHuntingMet = value;
    }

    [Header("Target Game")]
    [Tooltip("For storing the circles that compose the target")]
    [SerializeField] private List<TargetCircle> _targetCircles;
    [Tooltip("The rate at which score increases for the target game. It's here instead of in TargetCircle because each circle needs to use the same timer")]
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
        Instantiate(_cursorPrefab, new Vector3(0, 0, 0), Quaternion.identity); //the first thing we want to do is instantiate the cursor

        _sceneName = SceneManager.GetActiveScene().name;
        _sceneLoader = FindObjectOfType<SceneLoader>();
        _totalGameTime = PlayerPrefs.GetInt("Game Duration", 100);

        switch (_sceneName)
        {
            case "Assessment":
                _qsAssessment = new Dictionary<string, List<WiiBoardData>>() 
                {
                    ["EC"] = new List<WiiBoardData>(),
                    ["EO"] = new List<WiiBoardData>()
                };

                _assessmentTime = PlayerPrefs.GetInt("Assessment Duration", 100);

                SetupAssessment();
                
                break;
            case "LOS":
                _cursor = FindObjectOfType<Cursor>();
                _directionNames = new List<string>(from rectangle in _rectangles select rectangle.name); //linq syntax
                //first lambda is for the keys, the second lambda is for the values
                _directionData = _directionNames.ToDictionary(v => v, v => new List<WiiBoardData>());
                _losInstructions = _directionNames.ToDictionary(v => v, v => "Please lean " + v.ToLower() + " as far as you can and hold for 3 seconds.");
                _losInstructionsBox.text = "Press Go! to start the Limits of Stability Assessment. Be prepared to lean in one of the 8 directions.";

                break;
            case "Colour Matching":
                _colourCircles = FindObjectsOfType<ColourCircle>().ToList();
                _colourTexts = new List<string>(from circle in _colourCircles select circle.name); //linq syntax
                _colourDuration = PlayerPrefs.GetInt("Duration of Target", 10);
                
                if (_colourChangeEvent == null)
                    _colourChangeEvent = new UnityEvent();

                ColourMatchingGame();

                break;
            case "Ellipse":
                EllipseGame();
                _movingCircle = FindObjectOfType<MovingCircle>();

                break;
            case "Hunting":
                _huntingDuration = PlayerPrefs.GetInt("Duration of Target", 10);

                HuntingGame();

                break;
            case "Target":
                _targetCircles = FindObjectsOfType<TargetCircle>().ToList();

                break;
        }

        if (_sceneName != "Assessment" && _sceneName != "LOS" && _timer == null) //timer for assessment started manually and los doesn't have timer
            _timer = StartCoroutine(StartTimer());
    }

    private void Update() //probably use update, though case can be made for fixedupdate
    {
        // with these two games, the scores are not dependent on getting to targets, so it's just one cumulative score
        switch (_sceneName)
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
        {
            if (_sceneName != "Assessment")
                _timeText.text = _totalGameTime.ToString();
            else
                _timeText.text = _assessmentTime.ToString();
        }

        //how to handle transitions for all scenes other than assessment
        if ((_totalGameTime <= 0 && _sceneName != "Assessment"))
        {
            if (SceneLoader.GetFamiliarization() && SceneLoader.GetGameIndex() < 4)
                _sceneLoader.Familiarization();
            else if (SceneLoader.GetExperimentation() && SceneLoader.GetGameIndicesIndex() <= 4 
                     && SceneLoader.GetTrialIndex() <= PlayerPrefs.GetInt("Number of Trials"))
                _sceneLoader.Experimentation();
            else   
                _sceneLoader.LoadStartScene();
        }
    }

    private void FixedUpdate() //fixed update to keep it in sync with cursor data
    {
        switch (_sceneName)
        {
            case "Assessment":
                if (_timer == null) //timer for the assessment hasn't started, don't go past this point
                    break;

                Assessment();

                if (_assessmentTime <= 0) //reset for eyes open condition
                {
                    if (ecDone && !eoDone) //change the instructions and reset timer for next condition
                    {
                        _timer = null;
                        _assessInstructionsBox.text = _assessInstructions[1];
                        _assessmentTime = PlayerPrefs.GetInt("Assessment Duration", 100);
                    }
                    else if (ecDone && eoDone) //set length offset when assessment is done
                        ComputeLengthOffset();
                }

                break;
            case "LOS":
                var data = _cursor.Data;

                if (!string.IsNullOrEmpty(_direction))
                    _directionData[_direction].Add(data);

                break;
        }
    }

    private void SetupAssessment()
    {
        _cursor = FindObjectOfType<Cursor>();
        _cursor.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0); //alpha is zero to make it invisible
        _assessInstructionsBox.text = _assessInstructions[0];
    }

    private void Assessment()
    {
        var data = _cursor.Data;

        if (!ecDone)
            _qsAssessment["EC"].Add(data);
        else
            _qsAssessment["EO"].Add(data);
    }

    public void StartAssessmentTimer() // started on button click
    {
        if (_timer == null) //prevent double clicks with the start button causing a second timer to go
        {
            _timer = StartCoroutine(StartTimer());

            if (ConditionChangeEvent != null)
            {
                if (!ecDone)
                    ConditionChangeEvent(_qsAssessment.Keys.ToList()[0]);
                else if (!eoDone)
                    ConditionChangeEvent(_qsAssessment.Keys.ToList()[1]);
            }
        }
    }

    private void ComputeLengthOffset()
    {
        var yValues = new List<float>();

        //default to eyes open if for some reason this key doesn't exist
        if (Convert.ToBoolean(PlayerPrefs.GetInt("Eyes Condition", 1)))
            yValues = new List<float>(from value in _qsAssessment["EO"] select value.copY); //linq syntax
        else
            yValues = new List<float>(from value in _qsAssessment["EC"] select value.copY); //linq syntax

        PlayerPrefs.SetFloat("Length Offset", yValues.Average()*100f);

        _sceneLoader.LoadStartScene();
    }
    
    private void LOS() //changes the colour of the target direction
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

    public void StartLOS() //initiated from button click
    {
        if (FindObjectOfType<Cursor>() == null) // make sure only one cursor is spawned
            Instantiate(_cursorPrefab, new Vector3(0, 0, 0), Quaternion.identity); //instantiate at button click

        if (GameObject.FindGameObjectWithTag("Target") != null) //once we press the button, we want to switch to a new target
            GameObject.FindGameObjectWithTag("Target").tag = "Untagged";

        if (_counter == _directionNames.Count) //check when _counter is greater than the max index
        {
            var windowLength = Mathf.FloorToInt(PlayerPrefs.GetInt("Rolling Average Window")*1/Time.fixedDeltaTime);
            
            GetLimits(windowLength);
            _sceneLoader.LoadStartScene();
        }
        else
        {
            if (!_shuffled)
            {
                _directionNames = KnuthShuffler.Shuffle(_directionNames); //order of directions needs to be randomized
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

    private void GetLimits(int windowLength) 
    {
        _limits = new Dictionary<string, float>();
        var averages = new List<float>();
        
        if (_directionData.Values != null) // if this is null, something weird happened
        {
            foreach (var direction in _directionData)
            {
                switch (direction.Key)
                {
                    case "Forward":
                    case "Back":
                        for (var i = 0; i <= direction.Value.Count - windowLength; i++)
                        {
                            // Makes each value absolute value because we only care about the magnitude
                            var rangeOfValues = new List<float>(from value 
                                                                in direction.Value.GetRange(i, windowLength) 
                                                                select Mathf.Abs(value.copY));
                            averages.Add(rangeOfValues.Average());
                        }

                        break;
                    case "Left":
                    case "Right":
                        for (var i = 0; i <= direction.Value.Count - windowLength; i++)
                        {
                            // Makes each value absolute value because we only care about the magnitude
                            var rangeOfValues = new List<float>(from value 
                                                                in direction.Value.GetRange(i, windowLength) 
                                                                select Mathf.Abs(value.copX));
                            averages.Add(rangeOfValues.Average());
                        }

                        break;
                }

                //since the list for the diagonal directions is currently not coded in, it throws an error, so just want to account for that
                if (averages.Count != 0)
                    _limits.Add(direction.Key, averages.Max()*100f);
                    

                averages.Clear(); //clear the list so that it's a new one next loop
            }

            var factor = 1f; //factor to multiply the limits by to increase or decrease difficulty, use to be 0.9 but now just 1

            PlayerPrefs.SetFloat("Limit of Stability Front", _limits["Forward"] * factor); //store values, want just 90% of max
            PlayerPrefs.SetFloat("Limit of Stability Back", _limits["Back"] * factor);
            PlayerPrefs.SetFloat("Limit of Stability Left", _limits["Left"] * factor);
            PlayerPrefs.SetFloat("Limit of Stability Right", _limits["Right"] * factor);
        }
        else
            Debug.LogWarning("Something weird happened, no limit data colleccted.");
    }

    private void ColourMatchingGame()
    {
        _colourText = FindObjectOfType<TextMeshProUGUI>();
        
        var averageDistance = 0f;

        foreach (var circle in _colourCircles)
            foreach (var otherCircle in _colourCircles)
                averageDistance += Mathf.Sqrt(Mathf.Pow(circle.transform.position.x - otherCircle.transform.position.x, 2) + 
                                              Mathf.Pow(circle.transform.position.y - otherCircle.transform.position.y, 2));
            
        averageDistance /= Mathf.Pow(_colourCircles.Count, 2); //we're dividing by count^2 because there are count^2 of possible distances between circles in colour matching game

        _changeColour = StartCoroutine(ColourSelection(averageDistance));
    }

    private IEnumerator ColourSelection(float averageDistance)
    {
        ColourCircle oldCircle = null;

        while (true)
        {
            PickColour(averageDistance, oldCircle);
            
            if (_colourChangeEvent != null)
                _colourChangeEvent.Invoke();

            while (_colourDuration > 0 && !_conditionColourMet)
            {
                _colourDuration -= Time.unscaledDeltaTime;

                yield return null;
            }

            TargetColourCircle.gameObject.tag = "Untagged";
            oldCircle = TargetColourCircle;
            ColourGameScore();

            //reset values to loop again, += to account for when the time is negative and to subtract form 10f
            _colourDuration = 10f; 
            _conditionColourMet = false;
        }
    }

    private void PickColour(float averageDistance, ColourCircle oldCircle)
    {
        var randText = _colourTexts[UnityEngine.Random.Range(0, _colourTexts.Count)];
        TargetColourCircle = _colourCircles[UnityEngine.Random.Range(0, _colourCircles.Count)];
        var randColour = TargetColourCircle.GetComponent<SpriteRenderer>().color;

        if (oldCircle != null) //only start checking distances after the first circle is spawned
        {
            var dist = Mathf.Sqrt(Mathf.Pow(TargetColourCircle.transform.position.x - oldCircle.transform.position.x, 2) +
                                  Mathf.Pow(TargetColourCircle.transform.position.y - oldCircle.transform.position.y, 2));

            while (dist < averageDistance)
            {
                TargetColourCircle = _colourCircles[UnityEngine.Random.Range(0, _colourCircles.Count)];
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

    private void EllipseGame()
    {
        LineRenderer = Ellipse.GetComponent<LineRenderer>();
        Positions = new Vector3[LineRenderer.positionCount];
        LineRenderer.GetPositions(Positions); //position has an out on it
        EllipseIndex = UnityEngine.Random.Range(0, Positions.Length); //any instances of UnityEngine.Ranodm is because Random exists in both System and UnityEngine, so need to clarify
        Instantiate(_movingCirclePrefab, new Vector3(Positions[EllipseIndex].x, Positions[EllipseIndex].y, 0), Quaternion.identity);
    }

    private void HuntingGame() => StartCoroutine(SpawnCircles());

    private IEnumerator SpawnCircles()
    {
        //we always want to spawn circle in a different quadrant than the current one and sufficiently far away
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
            else //eliminate the current quad as a place the next circle can spawn
            {
                quads.Add(prevQuad);
                prevQuad = quad;
                quads.Remove(quad);
            }

            //reset values to loop again, += to account for when the time is negative and to subtract form 10f
            _huntingDuration += 10f; 
            _conditionHuntingMet = false;
        }   
    }

    private float[] GetPositions(float minX, float maxX, float minY, float maxY, float[] oldPos)
    {
        var xPos = UnityEngine.Random.Range(minX, maxX);
        var yPos = UnityEngine.Random.Range(minY, maxY);

        if (oldPos == new float[] {0f, 0f})
            return new float[] {xPos, yPos};
        else //checks to make sure that the new circle always spawns a decent distance away, threshold set at quarter of total x dist
        {
            while (Mathf.Sqrt(Mathf.Pow(xPos - oldPos[0], 2) + Mathf.Pow(yPos - oldPos[1], 2)) <= 2f*5f*16f/9f*0.25f)
            {
                xPos = UnityEngine.Random.Range(minX, maxX);
                yPos = UnityEngine.Random.Range(minY, maxY);
            }

            return new float[] {xPos, yPos};
        }   
    }

    private IEnumerator StartTimer()
    {
        if (_sceneName != "Assessment")
        {
            while (_totalGameTime > 0)
            {
                yield return new WaitForSecondsRealtime(_totalGameDeltaTime);
                _totalGameTime -= _totalGameDeltaTime;
            }
        }
        else
        {
            while (_assessmentTime > 0)
            {
                yield return new WaitForSecondsRealtime(_totalGameDeltaTime);
                _assessmentTime -= _totalGameDeltaTime;
            }

            if (!ecDone)
                ecDone = true;
            else if (!eoDone)
                eoDone = true;
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

    private void OnDisable() //done so that _ecDone and _eoDone are false again once assessment is done
    {
        ecDone = false;
        eoDone = false;
    }
}