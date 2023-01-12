using System;

/// <summary>
/// This struct stores data from the Wii Balance Board as well as the calculated COP value.
/// </summary>
public struct WiiBoardData
{
    public float time; //time is time since the program was started
    public float copX;
    public float copY;
    public float topLeft; //individual sensor values
    public float topRight;
    public float bottomLeft;
    public float bottomRight;
    public float fCopX; //The filtered COP
    public float fCopY; //The filtered COP

    /// <summary>
    /// The constructor of the WiiBoardData struct.
    /// </summary>
    public WiiBoardData(float time, float copX, float copY, float topLeft, float topRight, float bottomLeft, 
                        float bottomRight, float fCopX, float fCopY)
    {
        this.time = time;
        this.copX = copX;
        this.copY = copY;
        this.topLeft = topLeft;
        this.topRight = topRight;
        this.bottomLeft = bottomLeft;
        this.bottomRight = bottomRight;
        this.fCopX = fCopX;
        this.fCopY = fCopY;
    }

    /// <summary>
    /// This method returns the names of all of the parameter variables as a comma separated string. 
    /// The names are used as the headers in the csv file.
    /// </summary>
    public string GetParameterNames()
    {
        var names = String.Join(", ", new string[] {nameof(time), nameof(copX), nameof(copY), nameof(topLeft), nameof(topRight),
                                                    nameof(bottomLeft), nameof(bottomRight), nameof(fCopX), nameof(fCopY)});
        return names;
    }

    /// <summary>
    /// This method returns the value of the parameter variables as a comma separated string. The values are saved in the csv file.
    /// </summary>
    public string GetParameterValues() //don't really like this since it involves boxing here and unboxing when converting to string
    {
        var values = String.Join(", ", new float[] {time, copX, copY, topLeft, topRight, bottomLeft, bottomRight, fCopX, fCopY});
        
        return values;
    }
}
