using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using ControllerManager;

namespace CSV
{
    public class CSVWriter
    {
        private string _fileName;
        private string _extension;
        private string _path;
        private string _condition;
        private int _index;

        public CSVWriter(string activeSceneName, string condition = "")
        {
            //want to check is null or empty so that on repeat calls of the writer, we don't keep settign the same thing.
            if (String.IsNullOrEmpty(_fileName))
                _fileName = activeSceneName;
            
            if (String.IsNullOrEmpty(_extension))
                _extension = ".csv";
            
            if(String.IsNullOrEmpty(_path))
                _path = Directory.GetCurrentDirectory();
            
            _condition = condition;
        }

        public void WriteHeader(WiiBoardData data, Stimulation stimulation = null) //writes the header but also creates the csv file, stimulation optional
        {
            var header = GetHeader(data, stimulation);
            var di = new DirectoryInfo(_path);
            var files = di.GetFiles(_fileName + _condition + "*"); //only find the relevant csv files
            // Debug.Log(_fileName);
            // Debug.Log(_condition);
            // Debug.Log((_fileName + _condition).Length);
            // foreach (var file in files)
            // {
            //     Debug.Log(file.Name.IndexOf('.'));
            //     Debug.Log(file.Name.Substring((_fileName + _condition).Length, file.Name.IndexOf('.') - (_fileName + _condition).Length));
            // }

            SetIndex(files);

            using (var w = new StreamWriter(_path + @"\" + _fileName + _condition + _index + _extension, true))
                w.WriteLine(header);
        }

        private string GetHeader(WiiBoardData data, Stimulation stimulation)
        {
            var header = "";

            if (_fileName == "LOS" || _fileName == "Assessment")
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

                foreach (var constants in stimulation.ControllerConstants)
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

                header += "\n" + data.GetParameterNames() + ", targetX, targetY, targetXFilterd, targetYFiltered, " + stimulation.ControllerData.GetParameterNames() + ", score";
            }

            return header;
        }

        private void SetIndex(FileInfo[] files)
        {
            if (files.Length == 0) //if no files with the appropriate name exist, start from 1
                _index = 1;
            else // go through the files that exist and find the highest index, new file will be highest index + 1
            {
                var indices = new List<int>();

                try
                {
                    //need this here because of the way los files are named, it screws up the index counter
                    foreach (var file in files) 
                    {
                        // Debug.Log(file.Name);
                        // Debug.Log(Convert.ToInt32(new String(file.Name.Where(Char.IsDigit).ToArray())));
                        indices.Add(Convert.ToInt32(new String(file.Name.Where(Char.IsDigit).ToArray())));

                        // Debug.Log(file.Name);
                        // //meeting this condition means we can ignore this file when deciding what the index should be
                        // if (!int.TryParse(file.Name.Substring((_fileName + _condition).Length, file.Name.IndexOf('.') - (_fileName + _condition).Length), out int result))
                        // {
                            
                        // }
                        // else
                        // {
                        //     indices.Add(Convert.ToInt32(file.Name.Substring((_fileName + _condition).Length, 
                        //                 file.Name.IndexOf('.') - (_fileName + _condition).Length)));
                        // }
                    }

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

        public async void WriteDataAsync(WiiBoardData data, Vector2 targetCoords) //make this async so it doesn't potentially slow down the game, for LOS and assessment
        {
            using (var w = new StreamWriter(_path + @"\" + _fileName + _condition + _index + _extension, true)) // true to append and not overwrite
            {
                var line = data.GetParameterValues() + $", {targetCoords.x}, {targetCoords.y}";
                await w.WriteLineAsync(line);
            }
        }

        public async void WriteDataAsync(WiiBoardData data, Vector2 targetCoords, Vector2 targetCoordsFiltered, ControllerData controllerData, GameSession gameSession) //make this async so it doesn't potentially slow down the game, for games
        {
            using (var w = new StreamWriter(_path + @"\" + _fileName + _condition + _index + _extension, true)) // true to append and not overwrite
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