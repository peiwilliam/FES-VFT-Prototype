using UnityEngine;
using TMPro;

public class IsWiiBoardConnected : MonoBehaviour
{
    [Tooltip("Time the text appears on the screen")]
    [SerializeField] private float _time = 3f;

    private TMP_Text _text;

    private void OnEnable() //subscribe to event on creation of object
    {
        WiiBoard.ConnectionEvent += ChangeConnectionText;
    }

    private void Start()
    {
        _text = gameObject.GetComponent<TMP_Text>();
    }

    private void Update() 
    {
        _time -= Time.unscaledDeltaTime;

        if (_time < 0)
            _text.alpha = 0f;
    }

    private void ChangeConnectionText()
    {
        if (Wii.GetExpType(0) != 3 || !Wii.IsActive(0)) // assumes an index of 0 if there's only one device connected, 3 is the id for wbb
        {
            _text.text = "Could not connect";
            _text.alpha = 1f;

            return;
        }

        _text.text = "Connection successful";
        _text.alpha = 1f;
    }

    private void OnDisable() // remove event on disable to prevent memory leaks
    {
        WiiBoard.ConnectionEvent -= ChangeConnectionText;
    }
}
