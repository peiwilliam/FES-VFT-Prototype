using System;
using System.Text;
using UnityEngine;

/// <summary>
/// This class is responsible for communcation between the arduino and the program. It sends commands to the arduino to tell it
/// how high stimulation should be in the four different channels (i.e to each of the four muscles). There is also an option to
/// receive messages from the arduino, but that is optional.
/// </summary>
public class CommunicationManager : MonoBehaviour
{
    [Tooltip("For storing the stimulation object associated with the game")]
    [SerializeField] private Stimulation _stimulation;
    
    private bool _readArduino;
    private SerialControllerCustomDelimiter _serialController;

    private void Start() //runs at the creation of the game object
    {
        _serialController = GetComponent<SerialControllerCustomDelimiter>(); //we fidn the serialcontroller object in the scene
        _readArduino = Convert.ToBoolean(PlayerPrefs.GetInt("Read From Arduino", 0));
        _serialController.ReadArdino(_readArduino); //setting this to false so the abstract serial thread won't try to read from arduino
    }

    private void FixedUpdate() //runs at regular intervalsz
    {
        //formats the requested stimulation into a format understandable by the arduino
        //said formaat is channel followed by the stimulation value with a leading zero for single digit stimulation values
        //a is for left plantarflexor, b is for left dorsiflexor, c is for rightplantarflexor, and d is for right dorsiflexor
        //example request: a12, b05, c14, d03
        var stimulation = new string[] {"a" + _stimulation.ControllerData.lpfStim.ToString("00"),
                                        "b" + _stimulation.ControllerData.ldfStim.ToString("00"),
                                        "c" + _stimulation.ControllerData.rpfStim.ToString("00"),
                                        "d" + _stimulation.ControllerData.rdfStim.ToString("00")};

        foreach (var stim in stimulation)
        {
            _serialController.SendSerialMessage(Encoding.ASCII.GetBytes(stim)); //sends the signal to arduino, we use ascii encoding

            if (_readArduino) //used for debugging the arduino
            {
                Debug.Log(stim + " send"); 
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
