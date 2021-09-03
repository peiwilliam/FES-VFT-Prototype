using System.Collections.Generic;
using UnityEngine;
using ControllerManager;

public class Stimulation : MonoBehaviour // todo, make this script run later, like DataCollectionAndWriting
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
        _controller.Stimulate(_cursor.Data, _targetCircle.transform.position);
    }
}
