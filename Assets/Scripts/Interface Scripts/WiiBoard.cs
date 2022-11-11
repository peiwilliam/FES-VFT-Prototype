using UnityEngine;

public class WiiBoard : MonoBehaviour //this class doesn't actually do that much, it's mostly just for the user experience
{
    private int _whichRemote = 0;

    public delegate void OnConnection();
    public static event OnConnection ConnectionEvent;

    private void OnEnable() 
    {
        Wii.OnDiscoveryFailed += OnDiscoveryFailed;
		Wii.OnWiimoteDiscovered += OnWiimoteDiscovered;
		Wii.OnWiimoteDisconnected += OnWiimoteDisconnected;
    }
    
    private void Awake() 
    {
        SetUpSingleton(); //set a singleton so only one wiiboard object
    }

    private void SetUpSingleton()
    {
        var numberOfObj = FindObjectsOfType<WiiBoard>().Length;

        if (numberOfObj > 1)
            Destroy(gameObject);
        else
            DontDestroyOnLoad(gameObject);
    }

    private void Start() 
    {
        Wii.StartSearch();
        Invoke("InvokeEvent", 0.01f);
    }

    //this method is specially created so that the event can be invoked with a delay
    //this is because for some reason, once the StartSearch method is done, it doesn't immediately sense that device is a wbb
    //and takes a non-negligible amount of time to initialize.
    private void InvokeEvent() => ConnectionEvent(); 

	public void OnDiscoveryFailed(int i) 
    {
		Debug.Log("Error: " + i);
	}
	
	public void OnWiimoteDiscovered(int thisRemote)
    {
        Debug.Log("Found this one: " + thisRemote);

        if (!Wii.IsActive(_whichRemote))
            _whichRemote = thisRemote;
    }
	
	public void OnWiimoteDisconnected(int whichRemote)
    {
        Debug.Log("Lost this one: " + whichRemote);	
	}
}
