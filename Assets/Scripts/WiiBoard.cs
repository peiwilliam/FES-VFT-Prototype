using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

public class WiiBoard : MonoBehaviour
{
    //private Wiimote _wiiDevice;

    [SerializeField] private GameObject _wiiBoard;
    
    private int whichRemote;

    private void Awake() 
    {
        SetUpSingleton(); //set a singleton so only one wiiboard object
    }

    private void SetUpSingleton()
    {
        var numberOfBt = FindObjectsOfType<WiiBoard>().Length;

        if (numberOfBt > 1)
            Destroy(gameObject);
        else
            DontDestroyOnLoad(gameObject);
    }

    private void OnEnable() 
    {
        _wiiBoard.gameObject.SetActive(false);

        Wii.OnDiscoveryFailed     += OnDiscoveryFailed;
		Wii.OnWiimoteDiscovered   += OnWiimoteDiscovered;
		Wii.OnWiimoteDisconnected += OnWiimoteDisconnected;
    }

    // private void Start()
    // {
    //     var deviceCollection = new WiimoteCollection(); // find all connected wii devices
    //     deviceCollection.FindAllWiimotes();
    //     _wiiDevice = deviceCollection[0]; // get the wiiboard, assumes no other wii devices are connected

    //     // Setup update handlers.
    //     _wiiDevice.WiimoteChanged          += WiiDeviceWiimoteChanged;
    //     _wiiDevice.WiimoteExtensionChanged += WiiDeviceWiimoteExtensionChanged;

    //     _wiiDevice.Connect();
    //     // _wiiDevice.SetReportType(InputReport.IRAccel, false); // FALSE = DEVICE ONLY SENDS UPDATES WHEN VALUES CHANGE!
    //     // _wiiDevice.SetLEDs(true, false, false, false);

    //     if (_wiiDevice.WiimoteState.ExtensionType != ExtensionType.BalanceBoard)
    //     {
    //         using (StreamWriter w = File.AppendText("log.txt"))
    //         {
    //             w.WriteLine("Error: The device connected is not a Wii Balance Board. \n");
    //         }

    //         Application.Quit(); //quit the application when wrong device is connected
    //     }
    // }

    // public float[] GetSensorValues()
    // {
    //     var cop = new float[] {
    //         _wiiDevice.WiimoteState.BalanceBoardState.CenterOfGravity.X, //called center of gravity but actually centre of pressure
    //         _wiiDevice.WiimoteState.BalanceBoardState.CenterOfGravity.Y};
        
    //     return cop;
    // }

    // private void OnDestroy() 
    // {
    //     _wiiDevice.Disconnect();
    // }

    public void BeginSearch()
	{
		//searching = true;
		Wii.StartSearch();   
		Time.timeScale = 1.0f;
	}

	public void OnDiscoveryFailed(int i) 
    {
		Debug.Log("Error:" + i + ". Try Again.");
		//searching = false;
	}
	
	public void OnWiimoteDiscovered (int thisRemote) 
    {
		Debug.Log("found this one: " + thisRemote);
		if(!Wii.IsActive(whichRemote))
			whichRemote = thisRemote;
	}
	
	public void OnWiimoteDisconnected (int whichRemote) 
    {
		Debug.Log("lost this one: " + whichRemote);	
	}
}
