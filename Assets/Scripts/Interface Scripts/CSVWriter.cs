using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using ControllerManager;

namespace CSV
{
    /// <summary>
    /// This class is resposnible for writing CSV files to record data from the games.
    /// </summary>
    public class CSVWriter
    {
        private string _fileName; //name of the file being written, it's just name of trial plus a number
        private string _extension; //obviously .csv
        private string _path; //path is defined in the settings
        private string _condition; //condition only applies to LOS and QS assessment
        private string _csvFolder; //name of the folder that the csvs are stored in
        private int _index; //index is the number that goes after the filename

        /// <summary>
        /// Constructor to create an instance of the CSVWriter class. Has an optional string input of condition. This condition input
        /// parameter is only used by LOS and QS assessment.
        /// </summary>
        public CSVWriter(string activeSceneName, string condition = "")
        {
            //want to check is null or empty so that on repeat calls of the writer, we don't keep setting the same thing.
            if (String.IsNullOrEmpty(_fileName))
                _fileName = activeSceneName;
            
            if (String.IsNullOrEmpty(_extension))
                _extension = ".csv";
            
            if(String.IsNullOrEmpty(_path))
                _path = PlayerPrefs.GetString("Root Path", Directory.GetCurrentDirectory());

            if (String.IsNullOrEmpty(_csvFolder))
                _csvFolder = @"\Game Outputs";
            
            _condition = condition;
        }

        /// <summary>
        /// Creates the .csv file and writes the header. The stimulation input is optional since LOS and QS assessment don't use 
        /// stimulation.
        /// </summary>
        public void WriteHeader(WiiBoardData data, Stimulation stimulation = null)
        {
            var header = GetHeader(data, stimulation);
            
            if (!Directory.Exists(_path + _csvFolder))
                Directory.CreateDirectory(_path + _csvFolder);

            var di = new DirectoryInfo(_path + _csvFolder);
            var files = di.GetFiles(_fileName + _condition + "*").ToList(); //only find the relevant csv files

            SetIndex(files); //set the index

            //write header
            using (var w = new StreamWriter(_path + _csvFolder + @"\" + _fileName + _condition + _index + _extension, true))
                w.WriteLine(header);
        }

        private string GetHeader(WiiBoardData data, Stimulation stimulation) //get the header of the csv file
        {
            var header = "";

            if (_fileName == "LOS" || _fileName == "Assessment") //this is essentially a null check for stimulation and avoids null exception
                header += data.GetParameterNames() + ", targetX, targetY";
            else
            {
                //extracts the keys as a list and joins it with commas, utlizes linq methods
                header += String.Join<string>(", ", SettingsManager.fieldNamesAndTypes.Keys.ToList());
                header += "\n"; //need to add this so that the values of the parameters are added on the next line

                foreach (var field in SettingsManager.fieldNamesAndTypes)
                {
                    if (field.Value == "int")
                        header += PlayerPrefs.GetInt(field.Key, 123).ToString() + ", "; //123 means that the value wasn't stored on the computer
                    else if (field.Value == "float")
                        header += PlayerPrefs.GetFloat(field.Key, 123f) + ", "; //123f means that the value wasn't stored on the computer
                    else
                        header += PlayerPrefs.GetString(field.Key, "abc") + ", "; //abc means that the value wasn't stored on the computer
                }

                header += "\n";

                foreach (var constants in stimulation.ControllerConstants) //writes out the physical constants of the controller
                {
                    if (constants.Key == "Constants")
                        header += "Calculated " + constants.Key;
                    else
                        header += constants.Key;

                    header += "\n";
                    header += String.Join<string>(", ", constants.Value.Keys.ToList());
                    header += "\n";
                    header += String.Join<float>(", ", constants.Value.Values.ToList());
                    header += "\n";
                }

                foreach (var constants in stimulation.ControllerStimConstants) //writes out the stim constants of the controller
                {
                    header += constants.Key;
                    header += "\n";
                    
                    if (constants.Key == "QS Stim")
                    {
                        header += String.Join<string>(", ", constants.Value.Keys.Where(value => value.Contains("PF")));
                        header += "\n";
                        header += String.Join<float>(", ", constants.Value.Values.Where(value => value != 0f));
                        header += "\n";
                    }
                    else
                    {
                        header += String.Join<string>(", ", constants.Value.Keys);
                        header += "\n";
                        header += String.Join<float>(", ", constants.Value.Values);
                        header += "\n";
                    }
                }

                header += "\n" + data.GetParameterNames() + ", targetX, targetY, targetXFilterd, targetYFiltered, " + stimulation.ControllerData.GetParameterNames() + ", score";
            }

            return header;
        }

        private void SetIndex(List<FileInfo> files) //this method gets the index that should be used in the file name
        {
            if (_condition == "Forward" || _condition == "Back") //filter through for forward and back because forward left and back left get included
                files.RemoveAll(file => file.Name.Contains("Left") || file.Name.Contains("Right"));

            if (files.Count == 0) //if no files with the appropriate name exist, start from 1
                _index = 1;
            else // go through the files that exist and find the highest index, new file will be highest index + 1
            {
                var indices = new List<int>();
                    
                try
                {
                    //need this here because of the way los files are named, it screws up the index counter
                    foreach (var file in files)
                        indices.Add(Convert.ToInt32(new String(file.Name.Where(Char.IsDigit).ToArray())));

                    indices.Sort();
                }
                catch (Exception ex)
                {
                    Debug.Log("File with illegal naming convention in game directory");
                    Debug.Log(ex.Message + ex.StackTrace);
                    SceneManager.LoadScene(0); //go back to the start screen if there is a file with an illegal name
                    throw;
                }

                if (indices.Count != 0) //throws an error if we don't do this, since indices is instantiated but not used
                {
                    if (indices.Max() != indices.Count) //this means that there's a smaller number that's missing, use that number instead
                        _index = indices.Where(index => index - 1 == indices.IndexOf(index)).Max() + 1;
                    else
                        _index = indices.Max() + 1;
                }
            }
        }

        /// <summary>
        /// This method writes gets called every physics tick to write the data into the file. This version of the method is used by
        /// LOS and QS assessment. This is async so that the file writing doesn't potentially slow down the game.
        /// </summary>
        public async void WriteDataAsync(WiiBoardData data, Vector2 targetCoords)
        {
            using (var w = new StreamWriter(_path + _csvFolder + @"\" + _fileName + _condition + _index + _extension, true)) // true to append and not overwrite
            {
                var line = data.GetParameterValues() + $", {targetCoords.x}, {targetCoords.y}";
                await w.WriteLineAsync(line);
            }
        }

        /// <summary>
        /// This method writes gets called every physics tick to write the data into the file. This version of the method is used by
        /// the games. This is async so that the file writing doesn't potentially slow down the game.
        /// </summary>
        public async void WriteDataAsync(WiiBoardData data, Vector2 targetCoords, Vector2 targetCoordsFiltered, ControllerData controllerData, GameSession gameSession) //make this async so it doesn't potentially slow down the game, for games
        {
            using (var w = new StreamWriter(_path + _csvFolder + @"\" + _fileName + _condition + _index + _extension, true)) // true to append and not overwrite
            {
                var score = 0f;

                switch (_fileName)
                {
                    case "Ellipse":
                        score = gameSession.EllipseScore;
                        break;
                    case "Hunting":
                        score = gameSession.HuntingScore;
                        break;
                    case "Colour Matching":
                        score = gameSession.ColourMatchingScore;
                        break;
                    case "Target":
                        score = gameSession.TargetScore;
                        break;
                }
                
                var line = data.GetParameterValues() + $", {targetCoords.x}, {targetCoords.y}, {targetCoordsFiltered.x}, {targetCoordsFiltered.y}, " + controllerData.GetParameterValues() + ", " + score.ToString();
                await w.WriteLineAsync(line);
            }
        }
    }
}