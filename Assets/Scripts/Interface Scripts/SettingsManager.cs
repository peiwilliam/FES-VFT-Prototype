using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    // default values
    private static Dictionary<string, object> _defaultValues = new Dictionary<string, object>()
    {
        ["Controller Frequency"] = 20,
        ["Trial Duration"] = 100,
        ["CoM Height"] = 54.7f,
        ["Ramp Duration"] = 1.0f,
        ["Max A/P Fraction"] = 18.0926f,
        ["Ankle Fraction"] = 2.0f,
        ["Number of Trials"] = 2,
        ["Filter Order"] = 2,
        ["Cutoff Frequency"] = 0.4615f,
        ["Length Offset"] = 0.0f,
        ["Star Multiplier"] = 120,
        ["Config Root Path"] = Directory.GetCurrentDirectory(),
        ["Rolling Average Window"] = 20
    };

    // dictionaries for iterating through values easier
    private static Dictionary<string, InputField> _fields = new Dictionary<string, InputField>();
    private static Dictionary<string, string> _fieldNamesAndTypes = new Dictionary<string, string>() 
    {
        ["Controller Frequency"] = "int",
        ["Trial Duration"] = "int",
        ["CoM Height"] = "float",
        ["Ramp Duration"] = "float",
        ["Max A/P Fraction"] = "float",
        ["Ankle Fraction"] = "float",
        ["Number of Trials"] = "int",
        ["Filter Order"] = "int",
        ["Cutoff Frequency"] = "float",
        ["Length Offset"] = "float",
        ["Star Multiplier"] = "int",
        ["Config Root Path"] = "string",
        ["Rolling Average Window"] = "int"
    };

    private Toggle toggle;

    private void Awake() 
    {
        SetUpSingleton(); //set a singleton

        foreach (var nameAndType in _fieldNamesAndTypes)
        {
            if (!PlayerPrefs.HasKey(nameAndType.Key)) //if opening up game for first time, set all values to default
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

    private void SetUpSingleton()
    {
        var numberOfObj = FindObjectsOfType<SettingsManager>().Length;

        if (numberOfObj > 1)
            Destroy(gameObject);
        else
            DontDestroyOnLoad(gameObject);
    }

    public void SetInputFields() 
    {
        _fields.Clear(); //because objects get refreshed every time scene is reloaded, need to add "new" fields to dictionary

        var fields = FindObjectsOfType<InputField>(); //array form, want to conver to dictionary instead - easier to work with

        foreach (var field in fields)
        {
            _fields.Add(field.name, field);
        }

        _fields.Remove("Battery Level"); //remove battery level so it doesn't interfere

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
                {
                    PlayerPrefs.SetString(nameAndType.Key, _fields[nameAndType.Key].text);
                }
                else
                {
                    _fields["Info"].text = "Config path doesn't exist. Please provide a valid path";
                }
            }
        }

        PlayerPrefs.Save();
    }

    public void ResetToDefaults()
    {
        PlayerPrefs.DeleteAll();
        SetInputs();
    }

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
        }
    }
}