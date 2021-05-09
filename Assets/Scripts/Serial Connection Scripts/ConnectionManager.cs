using System;
using System.IO.Ports;
using UnityEngine;

public class ConnectionManager : MonoBehaviour
{
    private SerialPort _stream;

    private void Start()
    {
        _stream = new SerialPort("COM4", 9600);
        _stream.ReadTimeout = 50;
        _stream.Open();
    }

    private void Update()
    {
        
    }

    public string ReadFromArduino(int timeout = 0)
    {
        _stream.ReadTimeout = timeout;

        try
        {
            return _stream.ReadLine();
        }
        catch (TimeoutException exception)
        {
            Debug.Log(exception.Message);
            return null;
        }
    }
}
