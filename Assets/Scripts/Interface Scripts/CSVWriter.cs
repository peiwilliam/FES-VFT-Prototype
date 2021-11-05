using System;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CSV
{
    public class CSVWriter
    {
        private StringBuilder _header;
        private string _fileName;
        private string _extension;
        private string _path;
        private int _count; // iterator to create unique csv file each time.
        private string _condition;

        public CSVWriter(string condition = "")
        {
            _header = new StringBuilder();
            _fileName = SceneManager.GetActiveScene().name; //file name is name of scene
            _extension = ".csv";
            _path = Directory.GetCurrentDirectory();
            _condition = condition;
        }

        public void WriteHeader() //writes the header but also creates the csv file
        {
            if (_fileName == "LOS" || _fileName == "Assessment")
                _header.AppendLine("Time, COPx, COPy, TopLeft, TopRight, BottomLeft, BottomRight, fCOPx, fCOPy, TargetX, TargetY");
            else
                _header.AppendLine("Time, COPx, COPy, TopLeft, TopRight, BottomLeft, BottomRight, fCOPx, fCOPy, TargetX, TargetY, TargetXFiltered, TargetYFiltered, ShiftedfCOPy, ShiftedTargetx, ShiftedTargety, TargetVertAngle, COMVertAngle, AngleErr, RPFStim, RDFStim, LPFStim, LDFStim, Ramping");

            _count = 1;
            var di = new DirectoryInfo(_path);

            foreach (var file in di.GetFiles())
            {
                if (file.Name.Contains(_fileName + _condition + Convert.ToString(_count)))
                    _count++;
            }

            File.AppendAllText(_path + @"\" + _fileName + _condition + _count + _extension, _header.ToString());
        }

        public async void WriteDataAsync(WiiBoardData data, Vector2 targetCoords) //make this async so it doesn't potentially slow down the game, for LOS and assessment
        {
            using (var w = new StreamWriter(_path + @"\" + _fileName + _condition + _count + _extension, true)) // true to append and not overwrite
            {
                var line = $"{data.time}, {data.copX}, {data.copY}, {data.topLeft}, {data.topRight}, {data.bottomLeft}, {data.bottomRight}, {data.fCopX}, {data.fCopY}, {targetCoords.x}, {targetCoords.y}";
                await w.WriteLineAsync(line);
            }
        }

        public async void WriteDataAsync(WiiBoardData data, Vector2 targetCoords, Vector2 targetCoordsFiltered, ControllerData controllerData) //make this async so it doesn't potentially slow down the game, for games
        {
            using (var w = new StreamWriter(_path + @"\" + _fileName + _condition + _count + _extension, true)) // true to append and not overwrite
            {
                var line = $"{data.time}, {data.copX}, {data.copY}, {data.topLeft}, {data.topRight}, {data.bottomLeft}, {data.bottomRight}, {data.fCopX}, {data.fCopY}, {targetCoords.x}, {targetCoords.y}, {targetCoordsFiltered.x}, {targetCoordsFiltered.y}, {controllerData.shiftedCOMy}, {controllerData.shiftedTargetCoordsX}, {controllerData.shiftedTargetCoordsY}, {controllerData.targetVertAng}, {controllerData.comVertAng}, {controllerData.angErr}, {controllerData.rpfStim}, {controllerData.rdfStim}, {controllerData.lpfStim}, {controllerData.ldfStim}, {controllerData.ramp}";
                await w.WriteLineAsync(line);
            }
        }
    }
}