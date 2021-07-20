using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ControllerManager;

public class Stimulation : MonoBehaviour
{
    private Controller _stimController;

    private void Awake() 
    {
        _stimController = new Controller(); //instantiate instance of controller
    }

    private void FixedUpdate()
    {
        _stimController.Stimulate();
    }
}
