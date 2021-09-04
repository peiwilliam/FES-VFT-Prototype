using UnityEngine;
using ControllerManager;

public class Stimulation : MonoBehaviour
{
    [SerializeField] private GameSession _gameSession;

    private Controller _controller;
    private Cursor _cursor;
    private GameObject _targetCircle;

    private void Start()
    {
        _controller = new Controller();
        _cursor = FindObjectOfType<Cursor>();
    }

    private void FixedUpdate()
    {
        var stimOutput = _controller.Stimulate(_cursor.Data, _targetCircle.transform.position);
    }
}
