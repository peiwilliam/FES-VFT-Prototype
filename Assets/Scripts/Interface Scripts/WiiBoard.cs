using UnityEngine;

public class WiiBoard : MonoBehaviour //this class doesn't actually do that much, it's mostly just of the user experience
{
    private int whichRemote = 0;

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

        Wii.OnDiscoveryFailed     += OnDiscoveryFailed;
		Wii.OnWiimoteDiscovered   += OnWiimoteDiscovered;
		Wii.OnWiimoteDisconnected += OnWiimoteDisconnected;
    }

	public void OnDiscoveryFailed(int i) 
    {
		Debug.Log("Error: " + i);
	}
	
	public void OnWiimoteDiscovered(int thisRemote)
    {
        Debug.Log("Found this one: " + thisRemote);

        if (!Wii.IsActive(whichRemote))
            whichRemote = thisRemote;
    }
	
	public void OnWiimoteDisconnected(int whichRemote)
    {
        Debug.Log("Lost this one: " + whichRemote);	
	}
}
