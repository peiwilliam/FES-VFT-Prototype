using System;
using UnityEngine;
using UnityEngine.UI;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;

public class BluetoothPin : MonoBehaviour
{
    private BluetoothClient _btClient = new BluetoothClient();
    public string BTPin { get; set; }
    
    private void Start()
    {
        SetPin();
    }

    private void SetPin()
    {
        var BTPin = AddressToWiiPin(BluetoothRadio.Default.LocalAddress.ToString());
        var text = gameObject.GetComponent<InputField>();
        text.SetTextWithoutNotify(BTPin);
        text.readOnly = true;
    }

    private string AddressToWiiPin(string btAddress)
    {
        if (btAddress.Length != 12) 
            throw new Exception("Invalid Bluetooth MAC Address: " + btAddress);

        var bluetoothPin = "";
        var doubleZeroInAddr = false;

        for (int i = btAddress.Length - 2; i >= 0; i -= 2)
        {
            string hex = btAddress.Substring(i, 2);
            bluetoothPin += (char)Convert.ToInt32(hex, 16);
            if (hex == "00") doubleZeroInAddr = true;
        }
        if (doubleZeroInAddr)
        { 
            throw new Exception("Invalid Bluetooth MAC address");
        }

        return bluetoothPin;
    }
}
