using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameSession : MonoBehaviour
{
    [Header("All Games")]
    [SerializeField] private GameObject _cursorPrefab;
    
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
    [SerializeField] private List<ColourCircle> _colourCircles;

    private List<string> _colourTexts;
    private Text _colourText;
    private Coroutine _changeColour;
    public int ColourMatchingScore { get; private set; }
    public ColourCircle TargetColourCircle { get; private set; }

    [Header("Hunting Game")]
    [SerializeField] private GameObject _huntingCirclePrefab;
    [SerializeField] private float _minX = 0f;
    [SerializeField] private float _maxX = 2f*5f*16f/9f; //2*height*aspect ratio
    [SerializeField] private float _minY = 0f;
    [SerializeField] private float _maxY = 5f*2f; //2*camera size
    [SerializeField] private float _spawnTime = 10f;
    
    private HuntingCircle _huntingCircle;
    public int HuntingScore { get; private set; }

    // target game
    private TargetCircle _targetCircle;
    private Coroutine _increaseScore;
    public int TargetScore { get; private set; }

    private void Start()
    {
        Instantiate(_cursorPrefab, new Vector3(0, 0, 0), Quaternion.identity); //need cursor for all games

        switch (SceneManager.GetActiveScene().name)
        {
            case "Colour Matching":
                var colourCircles = FindObjectsOfType<ColourCircle>();
                _colourCircles = colourCircles.ToList();
                ColourMatchingGame();
                break;
            case "Ellipse":
                EllipseGame();
                _movingCircle = FindObjectOfType<MovingCircle>();
                break;
            case "Hunting":
                _huntingCircle = FindObjectOfType<HuntingCircle>();
                HuntingGame();
                break;
            case "Target":
                var circles = FindObjectsOfType<TargetCircle>();

                foreach (var circle in circles)
                    if (circle.name == "Centre Target")
                        _targetCircle = circle;
                break;
        }
    }

    private void FixedUpdate()
    {
        switch (SceneManager.GetActiveScene().name)
        {
            case "Colour Matching":
                //ColourGameScore();
                break;
            case "Ellipse":
                EllipseGameScore();
                break;
            case "Hunting":
                HuntingGameScore();
                break;
            case "Target":
                TargetGameScore();
                break;
        }
    }

    private void ColourMatchingGame()
    {
        //need to do it this convoluted way because findobjectsoftype finds other text objects in the game
        _colourText = FindObjectOfType<Canvas>().transform.Find("Colour Text").gameObject.GetComponent<Text>();
        _colourTexts = new List<string>() {"White", "Red", "Blue", "Green", "Purple", 
                                           "Yellow", "Cyan", "Pink", "Grey", "Beige", 
                                           "Brown", "Orange"};
        
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

    private void EllipseGameScore() => EllipseScore = _targetCircle.GetScore();

    //private void ColourGameScore() => ColourMatchingScore = _colourCircle.GetScore();

    private void HuntingGameScore() => HuntingScore = _huntingCircle.GetScore();

    private void TargetGameScore() => TargetScore = _movingCircle.GetScore();

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

            yield return new WaitForSeconds(_spawnTime);

            Destroy(FindObjectOfType<HuntingCircle>().gameObject);

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
                print(xPos);
                print(yPos);
            }

            return new float[] {xPos, yPos};
        }   
    }

    private IEnumerator ColourSelection(float averageDistance)
    {
        ColourCircle oldCircle = null;

        while (true)
        {
            var randText = _colourTexts[Random.Range(0, _colourTexts.Count)];
            TargetColourCircle = _colourCircles[Random.Range(0, _colourCircles.Count)];
            var randColour = TargetColourCircle.GetComponent<SpriteRenderer>().color;

            if (oldCircle != null)
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

            _colourText.text = randText;
            _colourText.color = randColour;

            yield return new WaitForSeconds(_colourDuration);

            oldCircle = TargetColourCircle;
        }
    }
}