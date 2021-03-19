using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BatteryLevel : MonoBehaviour
{
    private float _batteryLevel;
    
    private void Start() 
    {
        var text = gameObject.GetComponent<InputField>();
        text.text = GetBattery().ToString();
        text.readOnly = true;
    }

    private float GetBattery()
    {
        _batteryLevel = Wii.GetBattery(0);
        return _batteryLevel;
    }
}
