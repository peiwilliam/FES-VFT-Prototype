using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CSV
{
    public class CSVWriter
    {
        private string _header;
        private string _fileName;
        private string _extension;
        private string _path;
        private string _condition;
        private int _index;

        public CSVWriter(string condition = "")
        {
            _fileName = SceneManager.GetActiveScene().name; //file name is name of scene
            _extension = ".csv";
            _path = Directory.GetCurrentDirectory();
            _condition = condition;
        }

        public async void WriteHeader(Stimulation stimulation = null) //writes the header but also creates the csv file
        {
            if (_fileName == "LOS" || _fileName == "Assessment")
                _header = "Time, COPx, COPy, TopLeft, TopRight, BottomLeft, BottomRight, fCOPx, fCOPy, TargetX, TargetY\n";
            else
            {
                //extracts the keys as a list and joins it with commas, utlizes linq methods
                _header += String.Join<string>(", ", SettingsManager._fieldNamesAndTypes.Keys.ToList());
                _header += "\n"; //need to add this so that the values of the parameters are added on the next line

                foreach (var field in SettingsManager._fieldNamesAndTypes)
                {
                    if (field.Value == "int")
                        _header += PlayerPrefs.GetInt(field.Key, 123).ToString() + ", "; //123 means that the value wasn't stored on the computer
                    else if (field.Value == "float")
                        _header += PlayerPrefs.GetFloat(field.Key, 123f) + ", "; //123f means that the value wasn't stored on the computer
                    else
                        _header += PlayerPrefs.GetString(field.Key, "abc") + ", "; //abc means that the value wasn't stored on the computer
                }

                _header += "\n";
                
                foreach (var constants in stimulation.ControllerConstants)
                {
                    if (constants.Key == "Constants")
                        _header += "Calculated " + constants.Key;
                    else
                        _header += constants.Key;

                    _header += "\n";
                    _header += String.Join<string>(", ", constants.Value.Keys.ToList());
                    _header += "\n";
                    _header += String.Join<float>(", ", constants.Value.Values.ToList());
                    _header += "\n";
                }
                
                _header += "\nTime, COPx, COPy, TopLeft, TopRight, BottomLeft, BottomRight, fCOPx, fCOPy, TargetX, TargetY, TargetXFiltered, TargetYFiltered, ShiftedfCOPx, ShiftedfCOPy, ShiftedTargetx, ShiftedTargety, TargetVertAngle, COMVertAngle, AngleErr, RPFStim, RDFStim, LPFStim, LDFStim, Ramping";
            }

            var di = new DirectoryInfo(_path);
            var files = di.GetFiles(_fileName + _condition + "*"); //only find the relevant csv files
            
            if (files.Length == 0) //if no files with the appropriate name exist, start from 1
                _index = 1;
            else // go through the files that exist and find the highest index, new file will be highest index + 1
            {
                var indices = new List<int>(
                from file in files 
                select Convert.ToInt32(file.Name.Substring((_fileName + _condition).Length, file.Name.IndexOf('.') - (_fileName + _condition).Length)));
                _index = indices.Max() + 1;
            }

            using (var w = new StreamWriter(_path + @"\" + _fileName + _condition + _index + _extension, true))
                await w.WriteLineAsync(_header);
        }

        public async void WriteDataAsync(WiiBoardData data, Vector2 targetCoords) //make this async so it doesn't potentially slow down the game, for LOS and assessment
        {
            using (var w = new StreamWriter(_path + @"\" + _fileName + _condition + _index + _extension, true)) // true to append and not overwrite
            {
                var line = $"{data.time}, {data.copX}, {data.copY}, {data.topLeft}, {data.topRight}, {data.bottomLeft}, {data.bottomRight}, {data.fCopX}, {data.fCopY}, {targetCoords.x}, {targetCoords.y}";
                await w.WriteLineAsync(line);
            }
        }

        public async void WriteDataAsync(WiiBoardData data, Vector2 targetCoords, Vector2 targetCoordsFiltered, ControllerData controllerData) //make this async so it doesn't potentially slow down the game, for games
        {
            using (var w = new StreamWriter(_path + @"\" + _fileName + _condition + _index + _extension, true)) // true to append and not overwrite
            {
                var line = $"{data.time}, {data.copX}, {data.copY}, {data.topLeft}, {data.topRight}, {data.bottomLeft}, {data.bottomRight}, {data.fCopX}, {data.fCopY}, {targetCoords.x}, {targetCoords.y}, {targetCoordsFiltered.x}, {targetCoordsFiltered.y}, {controllerData.comX}, {controllerData.shiftedComY}, {controllerData.shiftedTargetCoordsX}, {controllerData.shiftedTargetCoordsY}, {controllerData.targetVertAng}, {controllerData.comVertAng}, {controllerData.angErr}, {controllerData.rpfStim}, {controllerData.rdfStim}, {controllerData.lpfStim}, {controllerData.ldfStim}, {controllerData.ramp}";
                await w.WriteLineAsync(line);
            }
        }
    }
}