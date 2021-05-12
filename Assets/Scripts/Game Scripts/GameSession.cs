using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using KnuthShuffle;

public class GameSession : MonoBehaviour
{
    [Header("Assessment")]
    [SerializeField] private bool _ecDone;
    [SerializeField] private bool _eoDone;
    [SerializeField] private List<string> _assessInstructions;
    [SerializeField] private InputField _assessInstructionsBox;

    private Cursor _cursor;

    private static List<float> _xPosAssessEC; //cursor position during ec assessment
    private static List<float> _yPosAssessEC;
    private static List<float> _xPosAssessEO; //cursor position during eo assessment
    private static List<float> _yPosAssessEO;

    [Header("Limits of Stability")]
    [SerializeField] private InputField _losInstructionsBox;
    
    private Dictionary<string, string> _losInstructions;

    //dictionary to keep track of which positions have been done
    private static Dictionary<string, bool> _directions = new Dictionary<string, bool>() 
    {
        ["Forward"] = false,
        ["Backward"] = false,
        ["Left"] = false,
        ["Right"] = false,
        ["Forward Left"] = false,
        ["Backward Right"] = false,
        ["Forward Right"] = false,
        ["Backward Left"] = false
    };

    [Header("All Games")]
    [SerializeField] private GameObject _cursorPrefab;
    [SerializeField] private float _totalGameTime = 100f;
    [SerializeField] private Text _timeText;

    private float _totalGameDeltaTime = 1f;
    private Coroutine _timer;
    private SceneLoader _sceneLoader;
    
    [Header("Ellipse Game")]
    [SerializeField] private GameObject _movingCirclePrefab;

    private MovingCircle _movingCircle;
    public Ellipse Ellipse { get; set; }
    public LineRenderer LineRenderer { get; set; }
    public Vector3[] Positions { get; set; }
    public int EllipseIndex { get; private set; }
    public int EllipseScore { get; private set; }

    [Header("Colour Matching Game")]
    [SerializeField] private float _colourDuration = 10f;
    [SerializeField] private bool _conditionColourMet;
    [SerializeField] private List<ColourCircle> _colourCircles;
    //default colours, need to change if the colours have been changed
    [SerializeField] private List<string> _colourTexts = new List<string>() {"White", "Black", "Blue", "Green", "Purple", 
                                                                             "Yellow", "Cyan", "Pink", "Grey", "Beige", 
                                                                             "Brown", "Orange"};
    [SerializeField] private UnityEvent _colourChangeEvent;
    
    private Text _colourText;
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
        Instantiate(_cursorPrefab, new Vector3(0, 0, 0), Quaternion.identity); //need cursor for all games
        
        _sceneLoader = FindObjectOfType<SceneLoader>();

        switch (SceneManager.GetActiveScene().name)
        {
            case "Assessment":
                _xPosAssessEC = new List<float>();
                _yPosAssessEC = new List<float>();
                _xPosAssessEO = new List<float>();
                _yPosAssessEO = new List<float>();

                _cursor = FindObjectOfType<Cursor>();

                SetupAssessment();
                break;
            case "LOS":
                _cursor = FindObjectOfType<Cursor>();

                break;
            case "Colour Matching":
                var colourCircles = FindObjectsOfType<ColourCircle>();
                _colourCircles = colourCircles.ToList();

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

        var sceneName = SceneManager.GetActiveScene().name;

        if (sceneName != "Assessment" || sceneName != "LOS") //timer for assessment started manually
            StartCoroutine(StartTimer());
    }

    private void Update() //probably use update, though case can be made for fixedupdate
    {
        // with these two games, the scores are not dependent on getting to targets, so it's just one cumulative score
        switch (SceneManager.GetActiveScene().name)
        {
            case "Ellipse":
                EllipseGameScore();
                break;
            case "Target":
                TargetGameScore();
                break;
        }

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
        if (SceneManager.GetActiveScene().name == "Assessment")
        {
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
                        _sceneLoader.LoadStartScene();
                }
            }
        }
    }

    private void SetupAssessment() => _assessInstructionsBox.text = _assessInstructions[0];

    private void Assessment()
    {
        if (!_ecDone)
        {
            var data = _cursor.GetBoardValues();
            _xPosAssessEC.Add(data.copX);
            _yPosAssessEC.Add(data.copY);
        }
        else
        {
            var data = _cursor.GetBoardValues();
            _xPosAssessEO.Add(data.copX);
            _yPosAssessEO.Add(data.copY);
        }
    }

    public void StartAssessmentTimer()
    {
        _timer = StartCoroutine(StartTimer());
    }

    public void StartLOS()
    {
        _losInstructions = new Dictionary<string, string>()
        {
            ["Forward"] = "Please lean forward as far as you can and hold for 3 seconds.",
            ["Backward"] = "Please lean backward as far as you can and hold for 3 seconds.",
            ["Left"] = "Please lean left as far as you can and hold for 3 seconds.",
            ["Right"] = "Please lean right as far as you can and hold for 3 seconds.",
            ["Forward Right"] = "Please lean forward right as far as you can and hold for 3 seconds.",
            ["Backward Left"] = "Please lean backward left as far as you can and hold for 3 seconds.",
            ["Forward Left"] = "Please lean forward left as far as you can and hold for 3 seconds.",
            ["Backward Right"] = "Please lean backward right as far as you can and hold for 3 seconds."
        };

        var names = _losInstructions.Keys.ToList(); //make a list of the directions
        var shuffler = new KnuthShuffler();
        shuffler.KnuthShuffle(names);

    }

    private void ColourMatchingGame()
    {
        //need to do it this convoluted way because findobjectsoftype finds other text objects in the game
        _colourText = FindObjectOfType<Canvas>().transform.Find("Colour Text").gameObject.GetComponent<Text>();
        
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
        Ellipse = FindObjectOfType<Ellipse>();
        var radii = Ellipse.GetRadii();
        var centre = Ellipse.GetCentre();
        LineRenderer = Ellipse.GetComponent<LineRenderer>();
        Positions = new Vector3[LineRenderer.positionCount];

        LineRenderer.GetPositions(Positions); //pos has an out on it
        EllipseIndex = UnityEngine.Random.Range(0, Positions.Length);
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
        if (oldPos == new float[] {0f, 0f})
        {
            var xPos = UnityEngine.Random.Range(minX, maxX);
            var yPos = UnityEngine.Random.Range(minY, maxY);

            return new float[] {xPos, yPos};
        }
        else //checks to make sure that the new circle always spawns a decent distance away, threshold set at quarter of total x dist
        {
            var xPos = UnityEngine.Random.Range(minX, maxX);
            var yPos = UnityEngine.Random.Range(minY, maxY);

            while (Mathf.Sqrt(Mathf.Pow(xPos - oldPos[0], 2) + Mathf.Pow(yPos - oldPos[1], 2)) <= 2f*5f*16f/9f*0.25f)
            {
                xPos = UnityEngine.Random.Range(minX, maxX);
                yPos = UnityEngine.Random.Range(minY, maxY);
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
                _ecDone = true;
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
}