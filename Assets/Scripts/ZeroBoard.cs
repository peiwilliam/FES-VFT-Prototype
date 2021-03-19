using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ZeroBoard : MonoBehaviour
{
    private float _topLeft;
    private float _topRight;
    private float _bottomLeft;
    private float _bottomRight;
    private float _timeLeft;

    private List<Vector4> _values;

    private void Start() 
    {
        if ((bool)FindObjectOfType<WiiBoard>() && Wii.IsActive(0) && Wii.GetExpType(0) == 3)
        {
            _timeLeft = 5; //5 seconds
            _values = new List<Vector4>();
        }
        else
        {
            Debug.Log("No Wii Balance Board found");
        }
    }
    
    private void FixedUpdate() 
    {
        _timeLeft -= Time.fixedDeltaTime;

        if (_timeLeft > 0)
        {
            var boardValues = Wii.GetBalanceBoard(0); //assume wii board only wii device

            _values.Add(boardValues);
        }
        else
        {
            var topLeftValues = new List<float>();
            var topRightValues = new List<float>();
            var bottomLeftValues = new List<float>();
            var bottomRightValues = new List<float>();

            foreach (var value in _values)
            {
                topLeftValues.Add(value.y);
                topRightValues.Add(value.x);
                bottomLeftValues.Add(value.w);
                bottomRightValues.Add(value.z);
            }

            _topLeft = topLeftValues.Average();
            _topRight = topRightValues.Average();
            _bottomLeft = bottomLeftValues.Average();
            _bottomRight = bottomRightValues.Average();

            PlayerPrefs.SetFloat("Top Left Sensor", _topLeft);
            PlayerPrefs.SetFloat("Top Right Sensor", _topRight);
            PlayerPrefs.SetFloat("Bottom Left Sensor", _bottomLeft);
            PlayerPrefs.SetFloat("Bottom Right Sensor", _bottomRight);

            Destroy(gameObject);
        }
    }
}
