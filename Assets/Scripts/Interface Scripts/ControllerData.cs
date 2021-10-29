using System.Collections.Generic;

public struct ControllerData
{
    public float shiftedCOMy;
    public float shiftedTargetCoordsX;
    public float shiftedTargetCoordsY;
    public float targetVertAng;
    public float comVertAng;
    public float angErr;
    public float rpfStim;
    public float rdfStim;
    public float lpfStim;
    public float ldfStim;
    public float ramp;

    public ControllerData(Dictionary<string, float> stimOutput, float ramp, List<float> angles, List<float> shiftedPos)
    {
        this.shiftedCOMy = shiftedPos[0];
        this.shiftedTargetCoordsX = shiftedPos[1];
        this.shiftedTargetCoordsY = shiftedPos[2];
        this.targetVertAng = angles[0];
        this.comVertAng = angles[1];
        this.angErr = angles[2];
        this.rpfStim = stimOutput["RPF"];
        this.rdfStim = stimOutput["RDF"];
        this.lpfStim = stimOutput["LPF"];
        this.ldfStim = stimOutput["LDF"];
        this.ramp = ramp;
    }
}