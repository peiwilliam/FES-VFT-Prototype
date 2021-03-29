using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSession : MonoBehaviour
{
    // all games
    [SerializeField] private GameObject _cursorPrefab;
    
    // ellipse game
    [SerializeField] private GameObject _movingCirclePrefab;
    public Ellipse Ellipse { get; set; }
    public int EllipseIndex { get; private set; }
    public LineRenderer LineRenderer { get; set; }
    public Vector3[] Positions { get; set; }

    // hunting game
    [SerializeField] private GameObject _stationaryCirclePrefab;
    [SerializeField] private float _minX = 0f;
    [SerializeField] private float _maxX = 2f*5f*16f/9f; //2*height*aspect ratio
    [SerializeField] private float _minY = 0f;
    [SerializeField] private float _maxY = 5f*2f; //2*camera size
    [SerializeField] private float _spawnTime = 5f;

    private void Start()
    {
        Instantiate(_cursorPrefab, new Vector3(0, 0, 0), Quaternion.identity); //need cursor for all games

        switch (SceneManager.GetActiveScene().name)
        {
            case "Colour Matching":
                break;
            case "Ellipse":
                EllipseGame();
                break;
            case "Target":
                break;
            case "Hunting":
                HuntingGame();
                break;
        }
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
        var quads = new List<int>() {1, 2, 3, 4};

        var pos = new float[2]; //always will be size 2
        var prevQuad = 0;

        while (true) //keeps looping thorugh coroutine while it's active
        {
            var quad = quads[rand.Next(quads.Count)];
            Debug.Log(quad + "quad");

            switch (quad)
            {
                case 1:
                    pos = GetPositions(_maxX / 2f, _maxX - _stationaryCirclePrefab.transform.localScale.x, 
                                       _maxY / 2f, _maxY - _stationaryCirclePrefab.transform.localScale.y);
                    break;
                case 2:
                    pos = GetPositions(_minX + _stationaryCirclePrefab.transform.localScale.x, _maxX/2f, 
                                       _maxY / 2f, _maxY - _stationaryCirclePrefab.transform.localScale.y);
                    break;
                case 3:
                    pos = GetPositions(_minX + _stationaryCirclePrefab.transform.localScale.x, _maxX/2f, 
                                       _minY + _stationaryCirclePrefab.transform.localScale.y, _maxY/2f);
                    break;
                case 4:
                    pos = GetPositions(_maxX/2f, _maxX - _stationaryCirclePrefab.transform.localScale.x, 
                                       _minY + _stationaryCirclePrefab.transform.localScale.y, _maxY/2f);
                    break;
            }

            Instantiate(_stationaryCirclePrefab, new Vector3(pos[0], pos[1], 0), Quaternion.identity);

            yield return new WaitForSeconds(_spawnTime);

            Destroy(FindObjectOfType<StationaryCircle>().gameObject);

            if (quads.Count == 4)
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
            
            foreach (var n in quads)
            {
                Debug.Log(n + "quads");
            }
        }   
    }

    private float[] GetPositions(float minX, float maxX, float minY, float maxY)
    {
        var xPos = UnityEngine.Random.Range(minX, maxX);
        var yPos = UnityEngine.Random.Range(minY, maxY);

        return new float[] {xPos, yPos};
    }
}
