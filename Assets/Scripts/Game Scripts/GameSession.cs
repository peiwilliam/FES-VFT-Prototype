/*
Initial code written by William Pei 2022 for his Master's thesis in MASL
*/

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
    private float _totalGameTime; //total amount of time that the games take
    private string _sceneName; //name of the current scene
    private Coroutine _timer; //coroutine for the timer
    private SceneLoader _sceneLoader; //sotres the sceneloader object

    [Header("Assessment")]
    [Tooltip("Instructions to the player when doing the quiet standing assessment")]
    [SerializeField] private List<string> _assessInstructions;
    [Tooltip("The box where the instructions are displayed")]
    [SerializeField] private InputField _assessInstructionsBox;

    /// <summary>
    /// Tells the game if the eyes closed condition of the quiet standing assessment is done or not.
    /// This has been made public static so the cursor object is able to access these values.
    ///</summary>
    public static bool ecDone; //public static so cursor is able to access these values
    /// <summary>
    /// Tells the game if the eyes open condition of the quiet standing assessment is done or not.
    /// This has been made public static so the cursor object is able to access these values.
    ///</summary>
    public static bool eoDone;

    public delegate void OnConditionChange(string condition);
    /// <summary>
    /// This event is triggered whenever the start button is clicked for quiet standing assessment and causes a new CSV file to be
    /// created for eyes closed and open condition separately.
    /// </summary>
    public static event OnConditionChange ConditionChangeEvent;

    private Dictionary<string, List<WiiBoardData>> _qsAssessment; //stores the Wii board data collected for EO and EC for QS assessment
    private Cursor _cursor; //stores the cursor object
    private float _assessmentTime; //total amount of time that the assessment takes

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
    
    public delegate void OnDirectionChange(string direction);
    /// <summary>
    // This event is triggered whenever the direction is changed in LOS and a new CSV file needs to be created for each direction.
    /// </summary>
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

    private MovingCircle _movingCircle; //stores the circle object used in ellipse game

    /// <summary>
    /// Property to get access to the ellipse object.
    /// </summary>
    public Ellipse Ellipse
    {
        get => _ellipse;
    }
    /// <summary>
    /// Property to get access to the LineRenderer object.
    ///</summary>
    public LineRenderer LineRenderer { get; private set; } 
    /// <summary>
    /// Property to get access to all the positions of the vertices in the ellipse.
    ///</summary>
    public Vector3[] Positions { get; private set; }
    /// <summary>
    /// Property to get access to the starting index of the moving circle object when the ellipse game starts.
    ///</summary>
    public int EllipseIndex { get; private set; }
    /// <summary>
    /// Property to get access to the total score for the ellipse game.
    ///</summary>
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

    private float _colourDuration; //how long each target lasts without the player getting to it in time
    private TextMeshProUGUI _colourText; //stores the text object for the text in the centre of the colour game
    private Coroutine _changeColour; //coroutine responsible for changing the colours

    /// <summary>
    /// Property for getting the total score in the colour game.
    ///</summary>
    public int ColourMatchingScore { get; private set; }
    /// <summary>
    /// Property for getting the current target circle in the colour matching game.
    ///</summary>
    public ColourCircle TargetColourCircle { get; private set; }

    /// <summary>
    /// Property for getting and setting the _conditionColourMet private attribute when the player has successfully stayed in the target.
    ///</summary>
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
    
    private float _huntingDuration; //how long each target lasts without the player getting to it in time
    private HuntingCircle _huntingCircle; //for storing the current hunting game target

    /// <summary>
    /// Property for getting access to the total score in the hunting game.
    ///</summary>
    public int HuntingScore { get; private set; }
    /// <summary>
    /// Property for getting and setting the _conditionHuntingMet private attribute when the player has successfully stayed inside the target.
    ///</summary>
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

    private Coroutine _increaseScore; //coroutine responsible for increasing the score in target as the player is inside

    /// <summary>
    /// Property for getting the total score in the target game.
    ///</summary>
    public float TargetScore { get; private set; }
    /// <summary>
    /// Property for getting the _deltaTimeScore private attribute. This value is stored in GameSession
    /// because the value is shared amongs all the target circles.
    /// </summary>
    public float DeltaTimeScore
    {
        get => _deltaTimeScore;
        set => _deltaTimeScore = value;
    }

    private void Start() //runs only at the beginning when the object is created
    {
        Instantiate(_cursorPrefab, new Vector3(0, 0, 0), Quaternion.identity); //the first thing we want to do is instantiate the cursor

        _sceneName = SceneManager.GetActiveScene().name;
        _sceneLoader = FindObjectOfType<SceneLoader>();
        _totalGameTime = PlayerPrefs.GetInt("Game Duration", 100); //if for some reason game duration isn't set, 100 is used by default

        switch (_sceneName)
        {
            case "Assessment":
                SetupAssessment();
                
                break;
            case "LOS":
                SetupLOS();

                break;
            case "Colour Matching":
                ColourMatchingGame();

                break;
            case "Ellipse":
                EllipseGame();

                break;
            case "Hunting":
                HuntingGame();

                break;
            case "Target":
                _targetCircles = FindObjectsOfType<TargetCircle>().ToList(); //get a list of all the target circl objects

                break;
        }

        if (_sceneName != "Assessment" && _sceneName != "LOS" && _timer == null) //timer for assessment started manually and los doesn't have a timer
            _timer = StartCoroutine(StartTimer());
    }

    private void Update() //runs at every frame update
    {
        // with these ellipse and target, the scores are not dependent on getting to targets, so it's just one cumulative score
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
            if (_sceneName != "Assessment") //assessment time not the same as game time, handle this here
                _timeText.text = _totalGameTime.ToString();
            else
                _timeText.text = _assessmentTime.ToString();
        }

        //how to handle transitions for all scenes other than assessment
        if (_totalGameTime <= 0 && _sceneName != "Assessment")
        {
            if (SceneLoader.GetFamiliarization() && SceneLoader.GetGameIndex() < 4) //checks if it's familiarization and how many games we've gone through
                _sceneLoader.Familiarization();
            else if (SceneLoader.GetExperimentation() && SceneLoader.GetGameIndicesIndex() <= 4 //checks if it's experimentation, how many games we've gone through, and how many trials/sets we've gone through
                     && SceneLoader.GetTrialIndex() <= PlayerPrefs.GetInt("Number of Trials"))
                _sceneLoader.Experimentation();
            else   
                _sceneLoader.LoadStartScene();
        }
    }

    private void FixedUpdate() //fixed update updates at every physics tick, by default it's 0.02s or 50 Hz
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
                if (!string.IsNullOrEmpty(_direction))
                    _directionData[_direction].Add(_cursor.Data);
        
                break;
        }
    }

    private void SetupAssessment() //initial set up for qs assessment
    {
        _qsAssessment = new Dictionary<string, List<WiiBoardData>>() //instantiate the dictionary used for storing qs assessment data
        {
            ["EC"] = new List<WiiBoardData>(),
            ["EO"] = new List<WiiBoardData>()
        };
        _assessmentTime = PlayerPrefs.GetInt("Assessment Duration", 100); //if for somereason assessment duration isn't set, 100 is used by def
        _cursor = FindObjectOfType<Cursor>();
        _cursor.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0); //alpha is zero to make it invisible, colours don't matter
        _assessInstructionsBox.text = _assessInstructions[0]; //start off with the first text
    }

    private void SetupLOS() //initial set up for los
    {
        _cursor = FindObjectOfType<Cursor>();
        _directionNames = new List<string>(from rectangle in _rectangles select rectangle.name); //store all the names of the directions
        _directionData = _directionNames.ToDictionary(v => v, v => new List<WiiBoardData>()); //dictionary to store all of the data for each direction
        _losInstructions = _directionNames.ToDictionary(v => v, v => "Please lean " + v.ToLower() + " as far as you can and hold for 3 seconds."); //LOS instructions
        _losInstructionsBox.text = "Press Go! to start the Limits of Stability Assessment. Be prepared to lean in one of the 8 directions."; //Initial LOS instructions
    }

    private void Assessment() //stores the data from the EO EC conditions of qs assessmetn to calculate length offset later
    {
        if (!ecDone)
            _qsAssessment["EC"].Add(_cursor.Data);
        else
            _qsAssessment["EO"].Add(_cursor.Data);
    }

    /// <summary>
    /// Starts the timer for quiet standing assessment.
    /// </summary>
    public void StartAssessmentTimer()
    {
        if (_timer == null) //prevent double clicks with the start button causing a second timer to go
        {
            _timer = StartCoroutine(StartTimer()); //starts the timer coroutine

            if (ConditionChangeEvent != null)
            {
                if (!ecDone) //triggers the ConditionChangeEvent and causes a new csv file to be created
                    ConditionChangeEvent(_qsAssessment.Keys.ToList()[0]);
                else if (!eoDone)
                    ConditionChangeEvent(_qsAssessment.Keys.ToList()[1]);
            }
        }
    }

    /// <summary>
    /// Skips the eyes closed assessment if it's n ot needed.
    /// </summary>
    public void SkipEyesClosedAssessment()
    {
        if (!ecDone)
        {
            ecDone = true;
            _assessInstructionsBox.text = _assessInstructions[1];
        }
    }

    private void ComputeLengthOffset() //computes the length offset after qs assessment is complete
    {
        var yValues = new List<float>();

        //default to eyes open if for some reason this key doesn't exist
        if (Convert.ToBoolean(PlayerPrefs.GetInt("Eyes Condition", 1)))
            yValues = new List<float>(from value in _qsAssessment["EO"] select value.copY); //select just the y (AP) values from the data
        else
            yValues = new List<float>(from value in _qsAssessment["EC"] select value.copY); //same as above

        PlayerPrefs.SetFloat("Length Offset", yValues.Average()*100f); //average the y values and store it

        _sceneLoader.LoadStartScene();
    }
    
    private void LOS() //manages the changes in the colour of the target direction
    {
        foreach (var rectangle in _rectangles) //loops thorugh each rectangle and ensures that only the rectangle tagged as "Target" is green and is in a higher layer than the rest
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

    /// <summary>
    /// Starts LOS and changes the directions with each button click
    /// </summary>
    public void StartLOS()
    {
        if (FindObjectOfType<Cursor>() == null) // make sure only one cursor is spawned
            Instantiate(_cursorPrefab, new Vector3(0, 0, 0), Quaternion.identity); //instantiate at button click

        if (GameObject.FindGameObjectWithTag("Target") != null) //once we press the button, we want to switch to a new target
            GameObject.FindGameObjectWithTag("Target").tag = "Untagged";

        if (_counter == _directionNames.Count) //check when _counter is greater than the max index
        {
            var windowLength = Mathf.FloorToInt(PlayerPrefs.GetInt("Rolling Average Window")*1/Time.fixedDeltaTime); //calcultes the number of data points needed to calculate the window length
            
            GetLimits(windowLength);
            _sceneLoader.LoadStartScene();
        }
        else
        {
            if (!_shuffled)
            {
                _directionNames.Shuffle(); //order of directions needs to be randomized, do only once
                _shuffled = true;
            }

            _direction = _directionNames[_counter++]; //it's a litte complicated, but essentially _counter++ does it incrementation after _counter is used to get the index
            _losInstructionsBox.text = _losInstructions[_direction];
            var rectangle = _rectangles.Find(n => n.name == _direction); //find the rectangle matching the direction selected
            rectangle.tag = "Target"; //set the tag of that rectangle to target

            if (DirectionChangeEvent != null) //invoke the direction change event
                DirectionChangeEvent(_direction);
        }
    }

    //calculates the limits for the forward, back, left, and right directions
    //diagonals haven't been implemented yet because they're not super crucial but they can be coded in if needed
    private void GetLimits(int windowLength) 
    {
        _limits = new Dictionary<string, float>();
        var averages = new List<float>();
        
        if (_directionData.Values != null) // if this is null, something weird happened, but shouldn't be null
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
                                                                in direction.Value.GetRange(i, windowLength) //only select the data points in the window
                                                                select Mathf.Abs(value.copY));
                            averages.Add(rangeOfValues.Average()); //take an average of the window of values
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
                    _limits[direction.Key] = averages.Max()*100f; //calculates the maximum value in the list of average window values. this will be the limit in that direction

                averages.Clear(); //clear the list so that it's a new one next loop
            }

            var factor = 1f; //factor to multiply the limits by to increase or decrease difficulty, use to be 0.9 but now just 1

            PlayerPrefs.SetFloat("Limit of Stability Front", _limits["Forward"] * factor);
            PlayerPrefs.SetFloat("Limit of Stability Back", _limits["Back"] * factor);
            PlayerPrefs.SetFloat("Limit of Stability Left", _limits["Left"] * factor);
            PlayerPrefs.SetFloat("Limit of Stability Right", _limits["Right"] * factor);
        }
        else
            Debug.LogWarning("Something weird happened, no limit data colleccted."); //output this warning in case something weird happens
    }

    private void ColourMatchingGame() //handles the initial set up for the colour matching game
    {
        _colourCircles = FindObjectsOfType<ColourCircle>().ToList(); //find all colour circle objects
        _colourTexts = new List<string>(from circle in _colourCircles select circle.name); //get all of the circle names
        _colourDuration = PlayerPrefs.GetInt("Duration of Target", 10); //use 10 by default for duration of target if for reason a value isn't set
        
        if (_colourChangeEvent == null) //just to ensure we don't get a null reference exception
            _colourChangeEvent = new UnityEvent();

        _colourText = FindObjectOfType<TextMeshProUGUI>(); //find the text telling which colour circle to go to
        
        var averageDistance = 0f;

        //calculate the average distance between all of the circles.
        //need to first sum up all of the distances
        //there might be a better way of doing this but I couldn't be bothered lol
        foreach (var circle in _colourCircles)
            foreach (var otherCircle in _colourCircles)
                averageDistance += Mathf.Sqrt(Mathf.Pow(circle.transform.position.x - otherCircle.transform.position.x, 2) + 
                                              Mathf.Pow(circle.transform.position.y - otherCircle.transform.position.y, 2));
            
        averageDistance /= Mathf.Pow(_colourCircles.Count, 2); //we're dividing by count^2 because there are count^2 of possible distances between circles in colour matching game

        _changeColour = StartCoroutine(ColourSelection(averageDistance)); //starting the coroutine
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

            //reset values to loop again
            _colourDuration = PlayerPrefs.GetInt("Duration of Target", 10); 
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

    private void EllipseGame() //handles the initial set up for the ellipse game
    {
        LineRenderer = _ellipse.GetComponent<LineRenderer>(); //Get the LineRenderer component in the ellipse object
        Positions = new Vector3[LineRenderer.positionCount]; //instantiate the positions array with how many positions there are in the ellipse
        LineRenderer.GetPositions(Positions); //positions has an out on it
        EllipseIndex = UnityEngine.Random.Range(0, Positions.Length); //any instances of UnityEngine.Ranodm is because Random exists in both System and UnityEngine, so need to clarify
        Instantiate(_movingCirclePrefab, new Vector3(Positions[EllipseIndex].x, Positions[EllipseIndex].y, 0), Quaternion.identity); //instantiate the moving circle
        _movingCircle = FindObjectOfType<MovingCircle>();
    }

    private void HuntingGame() //handles the initial set up for the hunting game
    {
        _huntingDuration = PlayerPrefs.GetInt("Duration of Target", 10);  //use 10 by default for duration of target if for reason a value isn't set

        StartCoroutine(SpawnCircles()); //start the coroutine of spawning circles
    }

    private IEnumerator SpawnCircles() //this coroutine handles how new circles are spawned in the hunting game
    {
        //the general way the new spawning location is selected is this:
        //1. the new location must be sufficiently far away from the previous circle, the distance is hard coded in for now
        //2. the new location must be in a different quadrant than the previous location
        var rand = new System.Random(); //use system.random because I have more control over what I randomize
        var quads = new List<int>() {1, 2, 3, 4}; //quadrants
        var pos = new float[2]; //always will be size 2
        var prevQuad = 0; //keeps track of what the previous quadrant was

        while (true) //keeps looping thorugh coroutine while it's active
        {
            var quad = quads[rand.Next(quads.Count)]; //selects new quadrant
            var oldPos = pos;
            
            //GetPositions selects a new positions based on a minx, maxx, miny, maxy bounds which is dependent on the quadrant selected
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

            Instantiate(_huntingCirclePrefab, new Vector3(pos[0], pos[1], 0), Quaternion.identity); //instantiate a new hunting circle at the new location
            
            _huntingCircle = FindObjectOfType<HuntingCircle>();

            while (_huntingDuration > 0 && !_conditionHuntingMet) //this keeps looping so long as the player hasn't stayed in the circle for long enough or has exceed the duration of the circle
            {
                _huntingDuration -= Time.unscaledDeltaTime;

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

            //reset values to loop again
            _huntingDuration = PlayerPrefs.GetInt("Duration of Target", 10); 
            _conditionHuntingMet = false;
        }   
    }

    private float[] GetPositions(float minX, float maxX, float minY, float maxY, float[] oldPos) //gets a new spawn location based on the quadrant and the previous spawn location
    {
        var xPos = UnityEngine.Random.Range(minX, maxX);
        var yPos = UnityEngine.Random.Range(minY, maxY);

        if (oldPos == new float[] {0f, 0f}) //this is only the case initially, just sets the pos to the random one generated
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

    private IEnumerator StartTimer() //coroutine for the timer that runs in qs assessment and the games
    {
        if (_sceneName != "Assessment")
        {
            while (_totalGameTime > 0) //loops continously runs while the conditions are true, same with the loop below as well
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

            if (!ecDone) //lets the game know which conditions have been complete
                ecDone = true;
            else if (!eoDone)
                eoDone = true;
        }
    }

    private void EllipseGameScore() => EllipseScore = _movingCircle.GetScore(); //gets the ellipse game score

    private void ColourGameScore() => ColourMatchingScore += TargetColourCircle.GetScore(); //gets the colour game score

    private void HuntingGameScore() => HuntingScore += _huntingCircle.GetScore(); //gets the hunting game score

    private void TargetGameScore() //gets the target game score
    {
        var updatedScore = _targetCircles.Sum(circle => circle.GetScore()); 

        TargetScore = updatedScore;
    }

    //runs when the game object is disabled on scene close
    //done so that _ecDone and _eoDone are false again once assessment is done since they are static variabels
    private void OnDisable() 
    {
        ecDone = false;
        eoDone = false;
    }
}