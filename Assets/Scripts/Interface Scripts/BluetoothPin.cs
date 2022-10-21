using System;
using UnityEngine;
using UnityEngine.UI;
using InTheHand.Net.Bluetooth;

public class BluetoothPin : MonoBehaviour
{
    public string BTPin { get; set; }
    
    private void Start()
    {
        SetPin();
    }

    private void SetPin()
    {
        try
        {
            BTPin = AddressToWiiPin(BluetoothRadio.Default.LocalAddress.ToString());
            var text = gameObject.GetComponent<InputField>();
            text.SetTextWithoutNotify(BTPin);
            text.readOnly = true;
        }
        catch (NullReferenceException exception)
        {
            Debug.LogWarning("Object is null, did you turn on the bluetooth? " + exception.StackTrace);
        }
    }

    private string AddressToWiiPin(string btAddress)
    {
        if (btAddress.Length != 12) //address of computer needs to be 12 characters long
            throw new Exception("Invalid Bluetooth MAC Address: " + btAddress);

        var bluetoothPin = "";
        var doubleZeroInAddr = false;

        for (int i = btAddress.Length - 2; i >= 0; i -= 2) //permanent pairing code is the MAC Address converted UTF characters
        {
            string hex = btAddress.Substring(i, 2);
            bluetoothPin += (char)Convert.ToInt32(hex, 16);

            if (hex == "00") 
                doubleZeroInAddr = true;
        }

        if (doubleZeroInAddr) //apparently this is no good, not really sure why lol
            throw new Exception("Invalid Bluetooth MAC address");

        return bluetoothPin;
    }
}
