using System.Collections.Generic;

namespace ControllerManager
{
    public struct ControllerData
    {
        public float comX;
        public float shiftedComY;
        public float shiftedTargetCoordsX;
        public float shiftedTargetCoordsY;
        public float targetVertAng;
        public float comVertAng;
        public float angErr;
        public float neuralTorque;
        public float mechTorque;
        public float rawRpfStim;
        public float rawRdfStim;
        public float rawLpfStim;
        public float rawLdfStim;
        public float rpfStim;
        public float rdfStim;
        public float lpfStim;
        public float ldfStim;
        public float ramp;

        public ControllerData(Dictionary<string, Dictionary<string, float>> stimOutputs, float ramp, List<float> angles, List<float> shiftedPos, float neuralTorque, float mechTorque)
        {
            this.comX = shiftedPos[0];
            this.shiftedComY = shiftedPos[1];
            this.shiftedTargetCoordsX = shiftedPos[2];
            this.shiftedTargetCoordsY = shiftedPos[3];
            this.targetVertAng = angles[0];
            this.comVertAng = angles[1];
            this.angErr = angles[2];
            this.neuralTorque = neuralTorque;
            this.mechTorque = mechTorque;
            this.rawRpfStim = stimOutputs["Unbiased"]["RPF"];
            this.rawRdfStim = stimOutputs["Unbiased"]["RDF"];
            this.rawLpfStim = stimOutputs["Unbiased"]["LPF"];
            this.rawLdfStim = stimOutputs["Unbiased"]["LDF"];
            this.rpfStim = stimOutputs["Actual"]["RPF"];
            this.rdfStim = stimOutputs["Actual"]["RDF"];
            this.lpfStim = stimOutputs["Actual"]["LPF"];
            this.ldfStim = stimOutputs["Actual"]["LDF"];
            this.ramp = ramp;
        }
    }
}
