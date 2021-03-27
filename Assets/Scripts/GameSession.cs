using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSession : MonoBehaviour
{
    [SerializeField] private GameObject _cursorPrefab;
    [SerializeField] private MovingCircle _targetPrefab;

    private Ellipse _ellipse;

    public int EllipseIndex { get; private set; }

    // private void Awake() 
    // {
    //     SetUpSingleton(); 
    // }

    // private void SetUpSingleton()
    // {
    //     var numberOfSessions = FindObjectsOfType<GameSession>().Length;

    //     if (numberOfSessions > 1)
    //         Destroy(gameObject);
    //     else
    //         DontDestroyOnLoad(gameObject);
    // }

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
                break;
        }
    }

    private void EllipseGame()
    {
        // const float PI = Mathf.PI;
        
        _ellipse = FindObjectOfType<Ellipse>();
        var radii = _ellipse.GetRadii();
        var centre = _ellipse.GetCentre();
        
        var lineRenderer = _ellipse.GetComponent<LineRenderer>();
        var positions = new Vector3[lineRenderer.positionCount];

        lineRenderer.GetPositions(positions); //pos has an out on it, so values are stored within pos only in the scope of the method
        EllipseIndex = Random.Range(0, positions.Length);
        // var angle = Random.Range(0, 2*PI); //get some random position along the ellipse
        // var x = radii[0]*Mathf.Cos(angle);
        // var y = radii[1]*Mathf.Sin(angle);
        // Instantiate(_targetPrefab, new Vector3(x + centre[0], y + centre[1], 0), Quaternion.identity);
        Instantiate(_targetPrefab, new Vector3(positions[EllipseIndex].x, positions[EllipseIndex].y, 0), Quaternion.identity);
    }
}
