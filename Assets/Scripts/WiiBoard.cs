using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WiimoteLib;

public class WiiBoard
{
    private Wiimote _wiiDevice;

    private float naCorners     = 0f;
    private float oaTopLeft     = 0f;
    private float oaTopRight    = 0f;
    private float oaBottomLeft  = 0f;
    private float oaBottomRight = 0f;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }
    // Update is called once per frame
    void Update()
    {
        
    }

    public void ConnectionToBoard()
    {
        var deviceCollection = new WiimoteCollection(); // find all connected wii devices
        deviceCollection.FindAllWiimotes();
    }

    public void GetSensorValues()
    {
        var rwWeight      = _wiiDevice.WiimoteState.BalanceBoardState.WeightKg;

        var rwTopLeft     = _wiiDevice.WiimoteState.BalanceBoardState.SensorValuesKg.TopLeft;
        var rwTopRight    = _wiiDevice.WiimoteState.BalanceBoardState.SensorValuesKg.TopRight;
        var rwBottomLeft  = _wiiDevice.WiimoteState.BalanceBoardState.SensorValuesKg.BottomLeft;
        var rwBottomRight = _wiiDevice.WiimoteState.BalanceBoardState.SensorValuesKg.BottomRight;
    }
}
