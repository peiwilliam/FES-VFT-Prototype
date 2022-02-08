using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExperimentationTransition : MonoBehaviour
{
    [Tooltip("The text that is used to inform the player about what's coming in the familiarization")]
    [SerializeField] private List<string> _infoText; //0 is beginning experimentation, 1 is during experimentation

    private void Start()
    {
        if (SceneLoader.GetExperimentation() && SceneLoader.GetGameIndicesIndex() == 0)
            gameObject.GetComponent<Text>().text = _infoText[0]; //only show this when experimentation has started
        else
            gameObject.GetComponent<Text>().text = _infoText[1];
    }
}
