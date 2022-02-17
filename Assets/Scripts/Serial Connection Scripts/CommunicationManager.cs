using System;
using System.Text;
using UnityEngine;

public class CommunicationManager : MonoBehaviour
{
    [Tooltip("For storing the stimulation object associated with the game")]
    [SerializeField] private Stimulation _stimulation;
    
    private bool _readArduino;
    private SerialControllerCustomDelimiter _serialController;

    private void Start()
    {
        _serialController = GetComponent<SerialControllerCustomDelimiter>();
        _readArduino = Convert.ToBoolean(PlayerPrefs.GetInt("Read From Arduino", 0));
        _serialController.ReadArdino(_readArduino); //setting this to false so the abstract serial thread won't try to read from arduino
    }

    private void FixedUpdate()
    {
        var stimulation = new string[] {"a" + _stimulation.ControllerData.lpfStim.ToString("00"),
                                        "b" + _stimulation.ControllerData.ldfStim.ToString("00"),
                                        "c" + _stimulation.ControllerData.rpfStim.ToString("00"),
                                        "d" + _stimulation.ControllerData.rdfStim.ToString("00")};

        foreach (var stim in stimulation)
        {
            _serialController.SendSerialMessage(Encoding.ASCII.GetBytes(stim));

            if (_readArduino)
            {
                Debug.Log(stim + " send"); //used for debugging the arduino
                var message = _serialController.ReadSerialMessage();

                if (message == null)
                    continue;
                    
                Debug.Log(Encoding.ASCII.GetString(message) + " receive");
            }
        }
    }

    //this needs to be added since if we leave the game on non-zero stimulation, the arduino will constantly send this signal
    private void OnDisable() 
    {
        var stimulation = new string[] {"a00", "b00", "c00", "d00"};

        foreach (var stim in stimulation)
            _serialController.SendSerialMessage(Encoding.ASCII.GetBytes(stim));
    }
}
