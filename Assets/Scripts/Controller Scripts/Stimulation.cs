using System.Collections.Generic;
using UnityEngine;
using ControllerManager;

public class Stimulation : MonoBehaviour
{
    [SerializeField] private GameSession _gameSession;

    private Controller _controller;
    private Cursor _cursor;
    private GameObject _targetCircle;

    public ControllerData ControllerData { get; private set; }

    private void Start()
    {
        _controller = new Controller();
        _cursor = FindObjectOfType<Cursor>();
    }

    private void FixedUpdate()
    {
        var stimOutput = _controller.Stimulate(_cursor.Data, _targetCircle.transform.position);
        ControllerData = new ControllerData(stimOutput, _controller.RampPercentage);
    }
}
