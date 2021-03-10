using UnityEngine;

public class WiiBoard : MonoBehaviour
{
    //private Wiimote _wiiDevice;

    //[SerializeField] private GameObject _wiiBoard;
    
    private int whichRemote = 0;

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

    private void Start() 
    {
        Wii.StartSearch();

        Wii.OnDiscoveryFailed     += OnDiscoveryFailed;
		Wii.OnWiimoteDiscovered   += OnWiimoteDiscovered;
		Wii.OnWiimoteDisconnected += OnWiimoteDisconnected;
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
