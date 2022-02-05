using System.Text;
using UnityEngine;

public class CommunicationManager : MonoBehaviour
{
    [SerializeField] private Stimulation _stimulation;
    private SerialControllerCustomDelimiter _serialController;

    private void Start()
    {
        _serialController = GetComponent<SerialControllerCustomDelimiter>();
    }

    private void FixedUpdate()
    {
        var stimulation = new string[] {"c" + _stimulation.ControllerData.rpfStim.ToString("00"),
                                        "d" + _stimulation.ControllerData.rdfStim.ToString("00"),
                                        "a" + _stimulation.ControllerData.lpfStim.ToString("00"),
                                        "b" + _stimulation.ControllerData.ldfStim.ToString("00")};

        foreach (var stim in stimulation)
            _serialController.SendSerialMessage(Encoding.ASCII.GetBytes(stim));
           
    }
}
