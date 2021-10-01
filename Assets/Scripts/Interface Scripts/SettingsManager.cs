using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SettingsManager : MonoBehaviour
{
    // dictionaries for iterating through values easier
    // default values
    private static Dictionary<string, object> _defaultValues = new Dictionary<string, object>()
    {
        ["Controller Frequency"] = 20,
        ["Trial Duration"] = 100,
        ["Ramp Duration"] = 1.0f,
        ["Max A/P Fraction"] = 18.0926f,
        ["Ankle Fraction"] = 2.0f,
        ["Number of Trials"] = 2,
        ["Filter Order"] = 2,
        ["Length Offset"] = 0.0f,
        ["Star Multiplier"] = 120,
        ["Config Root Path"] = Directory.GetCurrentDirectory(),
        ["Rolling Average Window"] = 1,
        ["RPF Max"] = 50,
        ["RDF Max"] = 50,
        ["LPF Max"] = 50,
        ["LDF Max"] = 50,
        ["Height"] = 170,
        ["Mass"] = 65f,
        ["Ankle Mass Fraction"] = 0.971f,
        ["CoM Height"] = 0.547f,
        ["Inertia Coefficient"] = 0.319f,
        ["Kp Coefficient"] = 0.24432f,
        ["Kd Coefficient"] = 0.22418f,
        ["K Coefficient"] = 0.75024f,
        ["Duration of Target"] = 10f,
        ["Duration to Get Points"] = 3f,
        ["Limit of Stability Front"] = 1f, //default needs to be one, if zero then it's a divide by zero error
        ["Limit of Stability Back"] = 1f,
        ["Limit of Stability Left"] = 1f,
        ["Limit of Stability Right"] = 1f,
    };
    private static Dictionary<string, InputField> _fields = new Dictionary<string, InputField>();
    private static Dictionary<string, string> _fieldNamesAndTypes = new Dictionary<string, string>() 
    {
        ["Controller Frequency"] = "int",
        ["Trial Duration"] = "int",
        ["Ramp Duration"] = "float",
        ["Max A/P Fraction"] = "float",
        ["Ankle Fraction"] = "float",
        ["Number of Trials"] = "int",
        ["Filter Order"] = "int",
        ["Length Offset"] = "float",
        ["Star Multiplier"] = "int",
        ["Config Root Path"] = "string",
        ["Rolling Average Window"] = "int",
        ["RPF Max"] = "int",
        ["RDF Max"] = "int",
        ["LPF Max"] = "int",
        ["LDF Max"] = "int",
        ["Height"] = "int",
        ["Mass"] = "float",
        ["Ankle Mass Fraction"] = "float",
        ["CoM Height"] = "float",
        ["Inertia Coefficient"] = "float",
        ["Kp Coefficient"] = "float",
        ["Kd Coefficient"] = "float",
        ["K Coefficient"] = "float",
        ["Duration of Target"] = "float",
        ["Duration to Get Points"] = "float",
        ["Limit of Stability Front"] = "float",
        ["Limit of Stability Back"] = "float",
        ["Limit of Stability Left"] = "float",
        ["Limit of Stability Right"] = "float"
    };

    private void Awake() 
    {
        if (SceneManager.GetActiveScene().buildIndex == 0) //only do this at the start screen
        {
            foreach (var nameAndType in _fieldNamesAndTypes)
            {
                if (!PlayerPrefs.HasKey(nameAndType.Key)) //if opening up game for first time, set all values to default, missing values also set to default
                {
                    if (nameAndType.Value == "int")
                        PlayerPrefs.SetInt(nameAndType.Key, (int)_defaultValues[nameAndType.Key]);
                    else if (nameAndType.Value == "float")
                        PlayerPrefs.SetFloat(nameAndType.Key, (float)_defaultValues[nameAndType.Key]);
                    else
                        PlayerPrefs.SetString(nameAndType.Key, (string)_defaultValues[nameAndType.Key]);
                }
            }
        }
    }

    public void SetInputFields() 
    {
        _fields.Clear(); //because objects get refreshed every time scene is reloaded, need to add "new" fields to dictionary

        var fields = FindObjectsOfType<InputField>(); //array form, want to conver to dictionary instead - easier to work with

        foreach (var field in fields)
            _fields.Add(field.name, field);

        SetInputs();
    }

    public void SaveSettings()
    {
        foreach (var nameAndType in _fieldNamesAndTypes) //saves new set values
        {
            if (nameAndType.Value == "int")
                PlayerPrefs.SetInt(nameAndType.Key, Convert.ToInt32(_fields[nameAndType.Key].text));
            else if (nameAndType.Value == "float")
                PlayerPrefs.SetFloat(nameAndType.Key, float.Parse(_fields[nameAndType.Key].text));
            else 
            {
                if (Directory.Exists(_fields[nameAndType.Key].text))
                    PlayerPrefs.SetString(nameAndType.Key, _fields[nameAndType.Key].text);
                else
                    _fields["Info"].text = "Config path doesn't exist. Please provide a valid path";
            }
        }

        PlayerPrefs.Save();
    }

    public void ResetToDefaults()
    {
        PlayerPrefs.DeleteAll();
        SetInputs();
    }

    //toggles are saved separately
    public void ZeroBoard() 
    {
        var isChecked = GameObject.Find("Zero Board").GetComponent<Toggle>().isOn;

        if (isChecked)
            PlayerPrefs.SetInt("Zero Board", 1);
        else
            PlayerPrefs.SetInt("Zero Board", 0);
    }

    public void FilterData()
    {
        var isChecked = GameObject.Find("Filter Data").GetComponent<Toggle>().isOn;

        if (isChecked)
            PlayerPrefs.SetInt("Filter Data", 1);
        else
            PlayerPrefs.SetInt("Filter Data", 0);
    }

    public void ECOrEO()
    {
        var isChecked = GameObject.Find("Eyes Condition").GetComponent<Toggle>().isOn;

        if (isChecked)
            PlayerPrefs.SetInt("EC or EO", 1);
        else
            PlayerPrefs.SetInt("EC or EO", 0);
    }

    private void SetInputs()
    {
        foreach (var nameAndType in _fieldNamesAndTypes)
        {
            if (nameAndType.Value == "int")
                _fields[nameAndType.Key].text = PlayerPrefs.GetInt(nameAndType.Key, (int)_defaultValues[nameAndType.Key]).ToString();
            else if (nameAndType.Value == "float")
                _fields[nameAndType.Key].text = PlayerPrefs.GetFloat(nameAndType.Key, (float)_defaultValues[nameAndType.Key]).ToString();
            else
                _fields[nameAndType.Key].text = PlayerPrefs.GetString(nameAndType.Key, (string)_defaultValues[nameAndType.Key]).ToString();

            if (!PlayerPrefs.HasKey(nameAndType.Key)) //sets to default values if the keys don't current exist
            {
                if (nameAndType.Value == "int")
                    PlayerPrefs.SetInt(nameAndType.Key, (int)_defaultValues[nameAndType.Key]);
                else if (nameAndType.Value == "float")
                    PlayerPrefs.SetFloat(nameAndType.Key, (float)_defaultValues[nameAndType.Key]);
                else
                    PlayerPrefs.SetString(nameAndType.Key, (string)_defaultValues[nameAndType.Key]);
            }
        }

        var toggles = FindObjectsOfType<Toggle>();

        foreach (var toggle in toggles)
        {
            if (toggle.name == "Zero Board")
                toggle.GetComponent<Toggle>().isOn = Convert.ToBoolean(PlayerPrefs.GetInt("Zero Board"));
            else if (toggle.name == "Filter Data")
                toggle.GetComponent<Toggle>().isOn = Convert.ToBoolean(PlayerPrefs.GetInt("Filter Data"));
            else if (toggle.name == "Eyes Condition")
                toggle.GetComponent<Toggle>().isOn = Convert.ToBoolean(PlayerPrefs.GetInt("EC or EO"));
        }
    }
}