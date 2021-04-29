using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class GetNewColourCircle : MonoBehaviour //helper class to help make event system for colour game work, attached to circle cont.
{
    [SerializeField] private List<ColourCircle> _colourCircles;

    private void Start()
    {
        _colourCircles = FindObjectsOfType<ColourCircle>().ToList();
    }

    public void NewCircle()
    {
        var targetCircle = _colourCircles.Find(n => n.tag == "Target");
        targetCircle.GetNewCircle();
    }
}
