using System.Linq;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class is a helperd class used to the event system work for the colour matching game. This is attached to the circle container
/// object.
/// </summary>
public class GetNewColourCircle : MonoBehaviour
{
    [Tooltip("Object that contains all of the colour circles")]
    [SerializeField] private List<ColourCircle> _colourCircles;

    private void Start() //runs only at the instantiation of the object
    {
        _colourCircles = FindObjectsOfType<ColourCircle>().ToList();
    }

    /// <summary>
    /// This method is responsible for setting the new colour circle target. This method is called via an UnityEvent on GameSession.
    /// </summary>
    public void NewCircle()
    {
        var targetCircle = _colourCircles.Find(n => n.tag == "Target");
        targetCircle.GetNewCircle();
    }
}
