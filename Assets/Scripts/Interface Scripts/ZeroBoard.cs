using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ZeroBoard : MonoBehaviour
{
    private float _timeLeft;
    private bool _foundWiiBoard;
    private List<Vector4> _values;

    private void Start() 
    {
        if ((bool)FindObjectOfType<WiiBoard>() && Wii.IsActive(0) && Wii.GetExpType(0) == 3)
        {
            _timeLeft = PlayerPrefs.GetInt("Zeroing Time", 3); //3 seconds is default
            _values = new List<Vector4>();
            _foundWiiBoard = true;
        }
        else
        {
            Debug.Log("No Wii Balance Board found, no zeroing will be done");
        }
    }
    
    private void FixedUpdate() 
    {
        if (_foundWiiBoard)
        {
            _timeLeft -= Time.fixedDeltaTime;

            if (_timeLeft > 0)
            {
                var boardValues = Wii.GetBalanceBoard(0); //assume wii board only wii device

                _values.Add(boardValues);
            }
            else
            {
                var topLeft = _values.Select(value => value.y).Average();
                var topRight = _values.Select(value => value.x).Average();
                var bottomLeft = _values.Select(value => value.w).Average();
                var bottomRight = _values.Select(value => value.z).Average();

                PlayerPrefs.SetFloat("Top Left Sensor", topLeft);
                PlayerPrefs.SetFloat("Top Right Sensor", topRight);
                PlayerPrefs.SetFloat("Bottom Left Sensor", bottomLeft);
                PlayerPrefs.SetFloat("Bottom Right Sensor", bottomRight);
            }
        }

        Destroy(gameObject);
    }

    private void OnEnable() 
    {
        Debug.Log("Getting data");
    }

    private void OnDestroy() 
    {
        Debug.Log("Finished getting data");
    }
}
