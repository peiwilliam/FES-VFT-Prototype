using System;
using System.Collections.Generic;
using InTheHand.Net.Sockets;
using InTheHand;
using InTheHand.Net;
using InTheHand.Net.Bluetooth;
using WiimoteLib;
using UnityEngine;

public class BluetoothConnect : MonoBehaviour
{
    // 44:B3:BE:33:91:80 reverse of bluetooth mac address on my computer
    // board unique identifier 00:24:44:0a:2a:65
    // bluetooth service guid {00001124-0000-1000-8000-00805f9b34fb}
    // pin for board D³¾3
    private BluetoothClient _btClient;
    private WiiBoard _wiiBoard;

    private void Awake() 
    {
        SetUpSingleton(); 
    }

    private void SetUpSingleton()
    {
        var numberOfBt = FindObjectsOfType<BluetoothConnect>().Length;

        if (numberOfBt > 1)
            Destroy(gameObject);
        else
            DontDestroyOnLoad(gameObject);
    }
    
    private void Start()
    {
        _btClient = new BluetoothClient();
        _wiiBoard = new WiiBoard();
        EstablishConnection();
        _wiiBoard.ConnectionToBoard();
    }
    
    private void Update() 
    {
        
    }

    public void EstablishConnection()
    {
        foreach (var device in _btClient.DiscoverDevices())
        {
            if (device.DeviceName.Contains("Nintendo"))
            {
                var btPin = AddressToWiiPin(BluetoothRadio.Default.LocalAddress.ToString());
                device.SetServiceState(BluetoothService.HumanInterfaceDevice, true);

                try
                {
                    BluetoothSecurity.PairRequest(device.DeviceAddress, btPin); // Null forces legacy pin request instead of SSP authentication.
                    //_btClient.Connect(device.DeviceAddress, BluetoothService.SerialPort);
                }
                catch (Exception exception)
                {
                    Debug.Log(exception.Message);
                }
                break;
            }
        }

        
    }

    private string AddressToWiiPin(string btAddress)
    {
        if (btAddress.Length != 12) 
            throw new Exception("Invalid Bluetooth Address: " + btAddress);

        var bluetoothPin = "";
        bool doubleZeroInAddr = false;
        for (int i = btAddress.Length - 2; i >= 0; i -= 2)
        {
            string hex = btAddress.Substring(i, 2);
            bluetoothPin += (char)Convert.ToInt32(hex, 16);
            if (hex == "00") doubleZeroInAddr = true;
        }
        if (doubleZeroInAddr)
        { 
            return "Invalid bt MAC address";
        }
        return bluetoothPin;
    }
}
