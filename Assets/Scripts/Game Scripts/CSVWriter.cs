using System;
using System.IO;
using System.Text;
using UnityEngine.SceneManagement;

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
        _fileName = SceneManager.GetActiveScene().name;
        _extension = ".csv";
        _path = Directory.GetCurrentDirectory();
        _condition = condition;
    }

    public void WriteHeader() //writes the header but also creates the csv file
    {
        _header.AppendLine("Time, COPx, COPy, TopLeft, TopRight, BottomLeft, BottomRight, fCOPx, fCOPy");

        _count = 1;
        var di = new DirectoryInfo(_path);

        foreach (var file in di.GetFiles())
        {
            if (file.Name.Contains(_fileName + _condition + Convert.ToString(_count)))
                _count++;
        }

        File.AppendAllText(_path + @"\" + _fileName + _condition + _count + _extension, _header.ToString());
    }

    public async void WriteDataAsync(WiiBoardData data) //make this async so it doesn't potentially slow down the game
    {
        using (var w = new StreamWriter(_path + @"\" + _fileName + _condition + _count + _extension, true)) // true to append and not overwrite
        {
            var line = $"{data.time}, {data.copX}, {data.copY}, {data.topLeft}, {data.topRight}, {data.bottomLeft}, {data.bottomRight}, {data.fCopX}, {data.fCopY}";
            await w.WriteLineAsync(line);
        }
    }
}
