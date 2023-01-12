using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This class is responsible for handling the transitions for an experiment session. This works in tandem with GameSession and
/// SceneLoader to handle randomization of the games and in between set displays.
/// </summary>
public class ExperimentationTransition : MonoBehaviour
{
    [Tooltip("The text that is used to inform the player about what's coming in the familiarization")]
    [SerializeField] private List<string> _infoText; //0 is beginning experimentation, 1 is during experimentation

    private void Start() //only runs at the instatiation of the object
    {
        //SceneLoader has static variables stored for whether or not these games are part of experimentation and what trial and what
        //game in the trial the experiment's at
        var experimentationStarted = SceneLoader.GetExperimentation();
        var gameIndicesIndex = SceneLoader.GetGameIndicesIndex();
        var trialIndex = SceneLoader.GetTrialIndex();

        if (experimentationStarted && gameIndicesIndex == 0 && trialIndex == 1)
            gameObject.GetComponent<Text>().text = _infoText[0]; //only show this when experimentation has started
        else if (experimentationStarted && gameIndicesIndex < 4)
            gameObject.GetComponent<Text>().text = _infoText[1];
        else //once a trial is complete, indicate to the player that the trial is complete if there are any more trials
        {
            var numberIndex = _infoText[2].IndexOf("__numberhere__");
            var instructionText = _infoText[2].Substring(0, numberIndex) + trialIndex.ToString() + _infoText[2].Substring(numberIndex + "__numberhere__".Length);
            gameObject.GetComponent<Text>().text = instructionText;
        }  
    }
}
