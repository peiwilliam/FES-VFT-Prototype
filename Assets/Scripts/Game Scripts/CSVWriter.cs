using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

public class CSVWriter
{
    private StringBuilder _csv;
    private string _fileName = "data"; //todo: make name of file dependent on game
    private string _extension = ".csv";
    private string _path = Directory.GetCurrentDirectory();
    private int _count; // iterator to create unique csv file each time.

    public void WriteHeader()
    {
        _csv = new StringBuilder();
        _csv.AppendLine("Time, COPx, COPy, TopLeft, TopRight, BottomLeft, BottomRight, fCOPx, fCOPy");

        _count = 1;
        var di = new DirectoryInfo(_path);

        foreach (var file in di.GetFiles())
        {
            if (file.Name.Contains(_fileName + Convert.ToString(_count)))
                _count++;
        }

        File.AppendAllText(_path + @"\" + _fileName + _count + _extension, _csv.ToString());
    }

    public void WriteData(List<WiiBoardData> dataList)
    {
        using (var w = new StreamWriter(_path + @"\" + _fileName + _count + _extension, true)) // true to append and not overwrite
        {
            foreach (var data in dataList)
            {
                var line = $"{data.time}, {data.copX}, {data.copY}, {data.topLeft}, {data.topRight}, {data.bottomLeft}, {data.bottomRight}, {data.fCopX}, {data.fCopY}";
                w.WriteLine(line);
                w.Flush();
            }
        }
    }
}
