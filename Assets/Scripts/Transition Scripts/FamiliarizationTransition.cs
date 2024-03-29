﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This class is responsible for handling the transitions for the familiarization session. This works in tandem with GameSession and
/// SceneLoader to handle randomization of the games and in between set displays.
/// </summary>
public class FamiliarizationTransition : MonoBehaviour
{
    [Tooltip("The text that is used to inform the player about what's coming in the familiarization")]
    [SerializeField] private List<string> _infoText; //0 is beginning familiarization, 1 is during familiarization

    private void Start()
    {
        if (SceneLoader.GetFamiliarization() && SceneLoader.GetGameIndex() == 1)
            gameObject.GetComponent<Text>().text = _infoText[0]; //only show this text when familiarization has started
        else
            gameObject.GetComponent<Text>().text = _infoText[1];
    }
}
