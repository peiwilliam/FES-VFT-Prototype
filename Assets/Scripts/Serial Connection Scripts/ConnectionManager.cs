using System;
using System.Threading;
using System.Collections;
using System.IO.Ports;
using UnityEngine;

public class ConnectionManager : MonoBehaviour
{
    [SerializeField] private string _comPort = "COM4"; //port used for the serial connection, this will be different depending on arudino
    [SerializeField] private int _baudRate = 9600; //default is 9600, but can be adjusted

    private SerialPort _stream;
    private Thread _thread;

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

    public IEnumerator ReadFromArduinoAsync(Action<string> callback, Action fail = null, float timeout = float.PositiveInfinity) 
    {
        var initialTime = DateTime.Now;
        DateTime nowTime;
        TimeSpan diff = default(TimeSpan);
        string dataString = null;

        do 
        {
            try
            {
                dataString = _stream.ReadLine();
            }
            catch (TimeoutException) 
            {
                dataString = null;
            }

            if (dataString != null)
            {
                callback(dataString);
                yield break; // Terminates the Coroutine
            } 
            else
                yield return null; // Wait for next frame

            nowTime = DateTime.Now;
            diff = nowTime - initialTime;
        } while (diff.Milliseconds < timeout);

        if (fail != null)
            fail();

        yield return null;
    }
}
