public struct WiiBoardData
{
    public float time;
    public float copX;
    public float copY;
    public float topLeft;
    public float topRight;
    public float bottomLeft;
    public float bottomRight;

    public WiiBoardData(float time, float copX, float copY, float topLeft, float topRight, float bottomLeft, float bottomRight)
    {
        this.time = time;
        this.copX = copX;
        this.copY = copY;
        this.topLeft = topLeft;
        this.topRight = topRight;
        this.bottomLeft = bottomLeft;
        this.bottomRight = bottomRight;
    }
}
