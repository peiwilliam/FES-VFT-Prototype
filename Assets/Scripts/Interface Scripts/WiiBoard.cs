using UnityEngine;

/// <summary>
/// This class is mostly for the user experience and doesn't do that much in terms of interaction with the games or connection to the
/// Wii Balance Board. It has events that do trigger, but they don't do much and don't appear when the game is built.
/// </summary>
public class WiiBoard : MonoBehaviour
{
    private int _whichRemote = 0;

    public delegate void OnConnection();
    /// <summary>
    /// This event triggers when the player tries to connect to the Wii Balance board. This event triggers methods in 
    /// IsWiiBoardConnected.
    /// </summary>
    public static event OnConnection ConnectionEvent;

    private void OnEnable() 
    {
        Wii.OnDiscoveryFailed += OnDiscoveryFailed;
		Wii.OnWiimoteDiscovered += OnWiimoteDiscovered;
		Wii.OnWiimoteDisconnected += OnWiimoteDisconnected;
    }
    
    private void Awake() //only runs at the beginning when the object is instantiated before start
    {
        SetUpSingleton(); //set a singleton so only one wiiboard object
    }

    private void SetUpSingleton() //this method ensures that only one instance of the Wiiboard object ever exists in the program
    {
        var numberOfObj = FindObjectsOfType<WiiBoard>().Length;

        if (numberOfObj > 1)
            Destroy(gameObject);
        else
            DontDestroyOnLoad(gameObject);
    }

    //only runs at the beginning when the object is instantiated after awake. The Wii Balance Board connection starts here
    //also triggers the InvkeEvent method with a delay of 0.01s (this delay value doesn't seem to matter, it just needs to be
    //something >0)
    private void Start() 
    {
        Wii.StartSearch();
        Invoke("InvokeEvent", 0.5f);
    }

    //this method is specially created so that the event can be invoked with a delay
    //this is because for some reason, once the StartSearch method is done, it doesn't immediately sense that device is a wbb
    //and takes a non-negligible amount of time to initialize.
    private void InvokeEvent() => ConnectionEvent(); 

    //this method triggers when the discovery of the Wii Balance Board fails
	private void OnDiscoveryFailed(int i) 
    {
		Debug.Log("Error: " + i);
	}
	
    //this method triggers when the Wii Balance Board is discovered and gives the id of the device
	private void OnWiimoteDiscovered(int thisRemote)
    {
        Debug.Log("Found this one: " + thisRemote);

        if (!Wii.IsActive(_whichRemote))
            _whichRemote = thisRemote;
    }
	
    //this method triggers when the Wii balance Board is disconnected from the game
	private void OnWiimoteDisconnected(int whichRemote)
    {
        Debug.Log("Lost this one: " + whichRemote);	
	}
}
