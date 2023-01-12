using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// This class is resposnible for collecting data from the Wii Balance Board while there's nothing on it to get the baseline values for
/// the sensors.
/// </summary>
public class ZeroBoard : MonoBehaviour
{
    private float _timeLeft; //time left to collect data
    private bool _foundWiiBoard; //true if a Wii Balance Board is found
    private List<Vector4> _values; //list to store the baseline values

    private void Start() //runs once at the beginning when the object is instantiated
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
    
    private void FixedUpdate() //runs at every physics tick, which is 0.02s by default or 50 Hz
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
                var topLeft = _values.Select(value => value.y).Average(); //average each of the sensor values to get the baseline
                var topRight = _values.Select(value => value.x).Average();
                var bottomLeft = _values.Select(value => value.w).Average();
                var bottomRight = _values.Select(value => value.z).Average();

                PlayerPrefs.SetFloat("Top Left Sensor", topLeft); //save the baseline values
                PlayerPrefs.SetFloat("Top Right Sensor", topRight);
                PlayerPrefs.SetFloat("Bottom Left Sensor", bottomLeft);
                PlayerPrefs.SetFloat("Bottom Right Sensor", bottomRight);

                Destroy(gameObject);
            }
        }
        else
            Destroy(gameObject);
    }

    private void OnEnable() //when the object is instantiated, show this in the log for debugging purposes
    {
        Debug.Log("Getting data");
    }

    private void OnDestroy() //when the object is destroyed, show this in the log for debugging purposes
    {
        Debug.Log("Finished getting data");
    }
}
