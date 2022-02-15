using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExperimentationTransition : MonoBehaviour
{
    [Tooltip("The text that is used to inform the player about what's coming in the familiarization")]
    [SerializeField] private List<string> _infoText; //0 is beginning experimentation, 1 is during experimentation

    private void Start()
    {
        var experimentationStarted = SceneLoader.GetExperimentation();
        var gameIndicesIndex = SceneLoader.GetGameIndicesIndex();
        var trialIndex = SceneLoader.GetTrialIndex();

        if (experimentationStarted && gameIndicesIndex == 0 && trialIndex == 1)
            gameObject.GetComponent<Text>().text = _infoText[0]; //only show this when experimentation has started
        else if (experimentationStarted && gameIndicesIndex < 4)
            gameObject.GetComponent<Text>().text = _infoText[1];
        else
        {
            var numberIndex = _infoText[2].IndexOf("__numberhere__");
            var instructionText = _infoText[2].Substring(0, numberIndex) + trialIndex.ToString() + _infoText[2].Substring(numberIndex + "__numberhere__".Length);
            gameObject.GetComponent<Text>().text = instructionText;
        }  
    }
}
