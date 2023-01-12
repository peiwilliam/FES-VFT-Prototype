using UnityEngine;
using TMPro;

/// <summary>
/// This class is responsible for indicating to the player whether or not the connection to the wii balance board was successful or
/// not.
/// </summary>
public class IsWiiBoardConnected : MonoBehaviour
{
    [Tooltip("Time the text appears on the screen")]
    [SerializeField] private float _time = 3f;

    private TMP_Text _text; //the text that tells the player if the connection was successful

    private void OnEnable() //subscribe to the connection event on creation of object
    {
        WiiBoard.ConnectionEvent += ChangeConnectionText;
    }

    private void Start() //runs only once at the beginnign when the object is instantiated
    {
        _text = gameObject.GetComponent<TMP_Text>();
    }

    private void Update() //runs at every frame update
    {
        if (_time > 0)
            _time -= Time.unscaledDeltaTime;
        else
            _text.alpha = 0f;
    }

    private void ChangeConnectionText() //this event triggers when the players presses connect on the bluetooth connection scene
    {
        if (Wii.GetExpType(0) != 3 || !Wii.IsActive(0)) //assumes an index of 0 if there's only one device connected, 3 is the id for wbb
        {
            _text.text = "Could not connect";
            _text.alpha = 1f;

            return;
        }

        _text.text = "Connection successful";
        _text.alpha = 1f;
    }

    private void OnDisable() //unsubscribe from event on disable to prevent memory leaks, happens when the object is destroyed
    {
        WiiBoard.ConnectionEvent -= ChangeConnectionText;
    }
}
