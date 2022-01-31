﻿using System;
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

        public CSVWriter(string condition = "")
        {
            _fileName = SceneManager.GetActiveScene().name; //file name is name of scene
            _extension = ".csv";
            _path = Directory.GetCurrentDirectory();
            _condition = condition;
        }

        public void WriteHeader(WiiBoardData data, Stimulation stimulation = null) //writes the header but also creates the csv file, stimulation optional
        {
            var header = "";

            if (_fileName == "LOS" || _fileName == "Assessment")
                header += data.GetParameterNames() + ", targetX, targetY";
                //header = "Time, COPx, COPy, TopLeft, TopRight, BottomLeft, BottomRight, fCOPx, fCOPy, TargetX, TargetY\n";
            else
            {
                //extracts the keys as a list and joins it with commas, utlizes linq methods
                header += String.Join<string>(", ", SettingsManager._fieldNamesAndTypes.Keys.ToList());
                header += "\n"; //need to add this so that the values of the parameters are added on the next line

                foreach (var field in SettingsManager._fieldNamesAndTypes)
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
                
                header += "\n" + data.GetParameterNames() + ", targetX, targetY, targetXFilterd, targetYFiltered, " + stimulation.ControllerData.GetParameterNames();
                //header += "\nTime, COPx, COPy, TopLeft, TopRight, BottomLeft, BottomRight, fCOPx, fCOPy, TargetX, TargetY, TargetXFiltered, TargetYFiltered, ShiftedfCOPx, ShiftedfCOPy, ShiftedTargetx, ShiftedTargety, TargetVertAngle, COMVertAngle, AngleErr, NeuralTorque, MechTorque, NeuralMLAngle, MechMLAngle, UnbiasedRPFStim, UnbiasedRDFStim, UnbiasedLPFStim, UnbiasedLDFStim, RPFStim, RDFStim, LPFStim, LDFStim, Ramping";
            }

            var di = new DirectoryInfo(_path);
            var files = di.GetFiles(_fileName + _condition + "*"); //only find the relevant csv files
            
            if (files.Length == 0) //if no files with the appropriate name exist, start from 1
                _index = 1;
            else // go through the files that exist and find the highest index, new file will be highest index + 1
            {
                List<int> indices;

                try
                {
                    indices = new List<int>(
                    from file in files
                    select Convert.ToInt32(file.Name.Substring((_fileName + _condition).Length, file.Name.IndexOf('.') - (_fileName + _condition).Length)));
                    indices.Sort();
                }
                catch (Exception ex)
                {
                    Debug.Log("File with illegal naming convention in game directory");
                    Debug.Log(ex.Message);
                    SceneManager.LoadScene(0); //go back to the start screen if there is a file with an illegal name
                    throw;
                }

                if (indices.Max() != indices.Count) //this means that there's a smaller number that's missing, use that number instead
                    _index = indices.Where(index => index - 1 == indices.IndexOf(index)).Max() + 1;
                else
                    _index = indices.Max() + 1;
            }
            
            using (var w = new StreamWriter(_path + @"\" + _fileName + _condition + _index + _extension, true))
                w.WriteLine(header);
        }

        public async void WriteDataAsync(WiiBoardData data, Vector2 targetCoords) //make this async so it doesn't potentially slow down the game, for LOS and assessment
        {
            using (var w = new StreamWriter(_path + @"\" + _fileName + _condition + _index + _extension, true)) // true to append and not overwrite
            {
                //var line = $@"{data.time}, {data.copX}, {data.copY}, {data.topLeft}, {data.topRight}, {data.bottomLeft}, {data.bottomRight}, {data.fCopX}, {data.fCopY}, {targetCoords.x}, {targetCoords.y}";
                var line = data.GetParameterValues() + $", {targetCoords.x}, {targetCoords.y}";
                await w.WriteLineAsync(line);
            }
        }

        public async void WriteDataAsync(WiiBoardData data, Vector2 targetCoords, Vector2 targetCoordsFiltered, ControllerData controllerData) //make this async so it doesn't potentially slow down the game, for games
        {
            using (var w = new StreamWriter(_path + @"\" + _fileName + _condition + _index + _extension, true)) // true to append and not overwrite
            {
                //var line = $"{data.time}, {data.copX}, {data.copY}, {data.topLeft}, {data.topRight}, {data.bottomLeft}, {data.bottomRight}, {data.fCopX}, {data.fCopY}, {targetCoords.x}, {targetCoords.y}, {targetCoordsFiltered.x}, {targetCoordsFiltered.y}, {controllerData.comX}, {controllerData.shiftedComY}, {controllerData.shiftedTargetCoordsX}, {controllerData.shiftedTargetCoordsY}, {controllerData.targetVertAng}, {controllerData.comVertAng}, {controllerData.angErr}, {controllerData.neuralTorque}, {controllerData.mechTorque}, {controllerData.neuroMlAngle}, {controllerData.mechMlAngle}, {controllerData.rawRpfStim}, {controllerData.rawRdfStim}, {controllerData.rawLpfStim}, {controllerData.rawLdfStim}, {controllerData.rpfStim}, {controllerData.rdfStim}, {controllerData.lpfStim}, {controllerData.ldfStim}, {controllerData.ramp}";
                var line = data.GetParameterValues() + $", {targetCoords.x}, {targetCoords.y}, {targetCoordsFiltered.x}, {targetCoordsFiltered.y}, " + controllerData.GetParameterValues();
                await w.WriteLineAsync(line);
            }
        }
    }
}