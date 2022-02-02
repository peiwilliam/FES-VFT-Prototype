using UnityEngine;

public class SerialListener : MonoBehaviour //keep this here for now, but probably don't need it for compex
{
    // Start is called before the first frame update
    private void Start()
    {
        
    }

    // Update is called once per frame
    private void Update()
    {
        
    }

    //Invoked when a line of data is received from the serial device
    private void OnMessageArrived(string message)
    {
        Debug.Log("Arrived: " + message);
    }

    //Invoked when a connect/disconnect evevnt occurs. The paramter 'success' will be 'true' upon connection, and 'false' upon
    //disconnection or failure to connect
    private void OnConnectionEvent(bool success)
    {
        Debug.Log(success ? "Device connected" : "Device disconnected");
    }
}