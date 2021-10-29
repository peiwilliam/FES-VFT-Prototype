public struct WiiBoardData
{
    public float time;
    public float copX;
    public float copY;
    public float gameCopX;
    public float gameCopY;
    public float topLeft;
    public float topRight;
    public float bottomLeft;
    public float bottomRight;
    public float fCopX;
    public float fCopY;
    public float fGameCopX;
    public float fGameCopY;

    public WiiBoardData(float time, float copX, float copY, float gameCopX,
                        float gameCopY, float topLeft, float topRight, float bottomLeft, 
                        float bottomRight, float fCopX, float fCopY, float fGameCopX, float fGameCopY)
    {
        this.time = time;
        this.copX = copX;
        this.copY = copY;
        this.gameCopX = gameCopX;
        this.gameCopY = gameCopY;
        this.topLeft = topLeft;
        this.topRight = topRight;
        this.bottomLeft = bottomLeft;
        this.bottomRight = bottomRight;
        this.fCopX = fCopX;
        this.fCopY = fCopY;
        this.fGameCopX = fGameCopX;
        this.fGameCopY = fGameCopY;
    }
}
