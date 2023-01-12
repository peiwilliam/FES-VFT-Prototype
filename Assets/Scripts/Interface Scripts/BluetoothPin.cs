using System;
using UnityEngine;
using UnityEngine.UI;
using InTheHand.Net.Bluetooth;

/// <summary>
/// This class is resp[onsible for giving the user the PIN for their computer to connect to the Wii Balance Board. The PIN is acquired
/// via the InTheHand bluetooth library for .NET.
/// </summary>
public class BluetoothPin : MonoBehaviour
{
    /// <summary>
    /// This property gives access to the unique PIN generated for a given computer.
    /// </summary>
    public string BTPin { get; set; }
    
    private void Start() //only runs at the beginning when the object is instantiated
    {
        SetPin();
    }

    private void SetPin() //this method tries to set the PIN in the game. If the bluetooth isn't on, an error message is given instead.
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

    private string AddressToWiiPin(string btAddress) //this method actually gets the unique PIN associated with a computer
    {
        if (btAddress.Length != 12) //address of computer needs to be 12 characters long
            throw new Exception("Invalid Bluetooth MAC Address: " + btAddress);

        var bluetoothPin = "";
        var doubleZeroInAddr = false;

        for (int i = btAddress.Length - 2; i >= 0; i -= 2) //permanent pairing code is the MAC Address converted UTF characters in reverse
        {
            string hex = btAddress.Substring(i, 2);
            bluetoothPin += (char)Convert.ToInt32(hex, 16);

            if (hex == "00") 
                doubleZeroInAddr = true;
        }

        if (doubleZeroInAddr) //apparently this is no good, not really sure why, though shouldn't be the case with most devices
            throw new Exception("Invalid Bluetooth MAC address");

        return bluetoothPin;
    }
}
