using System.Linq;
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
        var stimulation = new string[] {"C" + _stimulation.ControllerData.rpfStim.ToString("00"),
                                        "D" + _stimulation.ControllerData.rdfStim.ToString("00"),
                                        "A" + _stimulation.ControllerData.lpfStim.ToString("00"),
                                        "B" + _stimulation.ControllerData.ldfStim.ToString("00")};

        foreach (var stim in stimulation)
            _serialController.SendSerialMessage(Encoding.ASCII.GetBytes(stim));
    }
}
