using System.Collections.Generic;
using UnityEngine;
using ControllerManager;

public class Stimulation : MonoBehaviour
{
    private Controller _controller;
    private Cursor _cursor;
    private GameSession _gameSession;

    private void Start()
    {
        _controller = new Controller();
        _cursor = FindObjectOfType<Cursor>();
        _gameSession = FindObjectOfType<GameSession>();
    }

    private void FixedUpdate()
    {
        _controller.Stimulate(_cursor.Data, _cursor.TargetCoords);
    }
}
