using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

public class CSVWriter
{
    private StringBuilder _csv;
    private string _fileName = "data";
    private string _extension = ".csv";
    private string _path = Directory.GetCurrentDirectory();
    private int _count; // iterator to create unique csv file each time.

    public void WriteHeader()
    {
        _csv = new StringBuilder();
        _csv.AppendLine("Time, COPx, COPy, TopLeft, TopRight, BottomLeft, BottomRight");

        _count = 1;
        var di = new DirectoryInfo(_path);

        foreach (var file in di.GetFiles())
        {
            if (file.Name.Contains(_fileName + Convert.ToString(_count)))
            {
                _count++; 
            }
        }

        File.AppendAllText(_path + @"\" + _fileName + _count + _extension, _csv.ToString());
    }

    public void WriteData(WiiBoardData data)
    {
        using (var w = new StreamWriter(_path + @"\" + _fileName + _count + _extension, true)) // true to append and not overwrite
        {
            var line = $"{data.Time}, {data.COPx}, {data.COPy}, {data.TopLeft}, {data.TopRight}, {data.BottomLeft}, {data.BottomRight}";
            w.WriteLine(line);
            w.Flush();
        }
    }
}
