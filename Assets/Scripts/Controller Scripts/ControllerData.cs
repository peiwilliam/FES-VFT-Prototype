using System;
using System.Collections.Generic;

namespace ControllerManager
{
    /// <summary>
    /// This struct stores all of the data pertaining to the controller that changes from iteration to iteration
    ///</summary>
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
        public float neuroMlAngle;
        public float mechMlAngle;
        public float mechRpfBias;
        public float mechRdfBias;
        public float mechLpfBias;
        public float mechLdfBias;
        public float neuroRpfBias;
        public float neuroRdfBias;
        public float neuroLpfBias;
        public float neuroLdfBias;
        public float rawRpfStim;
        public float rawRdfStim;
        public float rawLpfStim;
        public float rawLdfStim;
        public float rpfStim;
        public float rdfStim;
        public float lpfStim;
        public float ldfStim;
        public float ramp;

        public ControllerData(Dictionary<string, Dictionary<string, float>> stimOutputs, Dictionary<string, Dictionary<string, float>> biases, float ramp, List<float> angles, 
                              List<float> mlAngles, List<float> shiftedPos, float neuralTorque, float mechTorque)
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
            this.neuroMlAngle = mlAngles[0];
            this.mechMlAngle = mlAngles[1];
            this.neuroRpfBias = biases["Neural"]["RPF"];
            this.neuroRdfBias = biases["Neural"]["RDF"];
            this.neuroLpfBias = biases["Neural"]["LPF"];
            this.neuroLdfBias = biases["Neural"]["LDF"];
            this.mechRpfBias = biases["Mech"]["RPF"];
            this.mechRdfBias = biases["Mech"]["RDF"];
            this.mechLpfBias = biases["Mech"]["LPF"];
            this.mechLdfBias = biases["Mech"]["LDF"];
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

        /// <summary>
        /// Returns a comma separated list of the names of the variables tracked.
        /// </summary>
        public string GetParameterNames()
        {
            var names = String.Join(", ", new string[] {nameof(comX), nameof(shiftedComY), nameof(shiftedTargetCoordsX), 
                                                        nameof(shiftedTargetCoordsY), nameof(targetVertAng), nameof(comVertAng),
                                                        nameof(angErr), nameof(neuralTorque), nameof(mechTorque), nameof(neuroMlAngle),
                                                        nameof(mechMlAngle), nameof(mechRpfBias), nameof(mechRdfBias),
                                                        nameof(mechLpfBias), nameof(mechLdfBias), nameof(neuroRpfBias),
                                                        nameof(neuroRdfBias), nameof(neuroLpfBias), nameof(neuroLdfBias),
                                                        nameof(rawRpfStim), nameof(rawRdfStim), nameof(rawLpfStim),
                                                        nameof(rawLdfStim), nameof(rpfStim), nameof(rdfStim),
                                                        nameof(lpfStim), nameof(ldfStim), nameof(ramp)});
            return names;
        }
        
        /// <summary>
        /// Returns a comma separated list of the values of the variables tracked.
        /// </summary>
        public string GetParameterValues() //don't really like this since it involves boxing here and unboxing when converting to string
        {
            var values = String.Join(", ", new float[] {comX, shiftedComY, shiftedTargetCoordsX, shiftedTargetCoordsY, targetVertAng,
                                                        comVertAng, angErr, neuralTorque, mechTorque, neuroMlAngle, mechMlAngle, 
                                                        mechRpfBias, mechRdfBias, mechLpfBias, mechLdfBias, neuroRpfBias, neuroRdfBias, 
                                                        neuroLpfBias, neuroLdfBias, rawRpfStim, rawRdfStim, rawLpfStim, rawLdfStim, 
                                                        rpfStim, rdfStim, lpfStim, ldfStim, ramp});
            return values;
        }
    }
}
