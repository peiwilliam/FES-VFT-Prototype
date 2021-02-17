using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WiimoteLib;

public class WiiBoard
{
    private Wiimote _wiiDevice;

    public void ConnectionToBoard()
    {
        var deviceCollection = new WiimoteCollection(); // find all connected wii devices
        deviceCollection.FindAllWiimotes();
        _wiiDevice = deviceCollection[0]; // get the wiiboard, assumes no other wii devices are connected
        _wiiDevice.Connect();

        if (_wiiDevice.WiimoteState.ExtensionType != ExtensionType.BalanceBoard)
        {
            using (StreamWriter w = File.AppendText("log.txt"))
            {
                w.WriteLine("Error: The device connected is not a Wii Balance Board. \n");
            }

            Application.Quit(); //quit the application when wrong device is connected
        }
    }

    public void GetSensorValues()
    {
        //called center of gravity but actually centre of pressure
        var centreOfPressureX = _wiiDevice.WiimoteState.BalanceBoardState.CenterOfGravity.X; 
        var centreOfPressureY = _wiiDevice.WiimoteState.BalanceBoardState.CenterOfGravity.Y;
    }
}
