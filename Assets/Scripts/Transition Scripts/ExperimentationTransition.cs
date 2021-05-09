using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExperimentationTransition : MonoBehaviour
{
    [SerializeField] private List<string> _infoText; //0 is beginning familiarization, 1 is during familiarization

    private void Start()
    {
        if (SceneLoader.GetExperimentation() && SceneLoader.GetGameIndicesIndex() == 0)
            gameObject.GetComponent<Text>().text = _infoText[0]; //only show this when familiarization has started
        else
            gameObject.GetComponent<Text>().text = _infoText[1];
    }
}
