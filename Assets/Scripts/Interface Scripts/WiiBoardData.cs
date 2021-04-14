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

    public WiiBoardData(float time, float copX, float copY, 
                        float topLeft, float topRight, float bottomLeft, 
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
}
