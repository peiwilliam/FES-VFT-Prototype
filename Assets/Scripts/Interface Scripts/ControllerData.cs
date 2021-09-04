using System.Collections.Generic;

public struct ControllerData
{
    public float rpfStim;
    public float rdfStim;
    public float lpfStim;
    public float ldfStim;
    public float ramp;

    public ControllerData(Dictionary<string, float> stimOutput, float ramp)
    {
        this.rpfStim = stimOutput["RPF"];
        this.rdfStim = stimOutput["RDF"];
        this.lpfStim = stimOutput["LPF"];
        this.ldfStim = stimOutput["LDF"];
        this.ramp = ramp;
    }
}