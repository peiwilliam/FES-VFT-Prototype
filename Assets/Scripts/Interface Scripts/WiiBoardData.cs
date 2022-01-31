using System;

public struct WiiBoardData
{
    public float time;
    public float copX;
    public float copY;
    public float topLeft;
    public float topRight;
    public float bottomLeft;
    public float bottomRight;
    public float fCopX;
    public float fCopY;

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

    public string GetParameterNames()
    {
        var names = String.Join(", ", new string[] {nameof(time), nameof(copX), nameof(copY), nameof(topLeft), nameof(topRight),
                                                    nameof(bottomLeft), nameof(bottomRight), nameof(fCopX), nameof(fCopY)});
        return names;
    }

    public string GetParameterValues()
    {
        var values = String.Join(", ", new float[] {time, copX, copY, topLeft, topRight, bottomLeft, bottomRight, fCopX, fCopY});
        
        return values;
    }
}
