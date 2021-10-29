using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace ControllerManager
{
    public class Controller
    {
        //neural controller
        private float _kdc;
        private float _kpc;
        //mechanical controller
        private float _kc;
        //phyiological constants
        private float _height;
        private float _mass;
        private float _ankleMassFraction;
        private float _comFraction;
        private float _inertiaCoeff;
        private float _ankleLength;
        private float _lengthOffset;
        private float _ankleQS;
        private float _m;
        private float _hCOM;
        private float _i;
        private float _kp;
        private float _kd;
        private float _k;
        //game screen params
        private float _maxX = 2f*5f*16f/9f; //2*height*aspect ratio
        private float _maxY = 5f*2f; //2*camera size
        private Vector2 _previousTarget;
        private int _neuralCounter;
        private List<float> _limits;
        private List<float> _coms;
        private List<float> _angles;
        private Dictionary<string, float> _stimMax;
        private Dictionary<string, List<float>> _biasCoeffs;
        //ramping function
        private Ramping _ramping;
        
        private const float _G = 9.81f; //m/s^2
        private const float _XWidth = 433f; // mm
        private const float _YLength = 238f; // mm
        private const float _MaxPFStim = 1.117055995961f; // not sure what these units are
        private const float _MaxDFStim = 1.170727515177f; //not sure what these units are either

        public float RampPercentage { get; private set; }
        public List<float> Angles { get; private set; }
        public List<float> ShiftedPos { get; private set; } //get com and target positions in real coordinates and shifted to ankle perspective

        public Controller()
        {
            //define various constants
            _kdc = PlayerPrefs.GetFloat("Kd Coefficient");
            _kpc = PlayerPrefs.GetFloat("Kp Coefficient");
            _kc = PlayerPrefs.GetFloat("K Coefficient");
            _height = PlayerPrefs.GetInt("Height")/100f; //convert from cm to m
            _mass = PlayerPrefs.GetFloat("Mass");
            _ankleMassFraction = PlayerPrefs.GetFloat("Ankle Mass Fraction");
            _comFraction = PlayerPrefs.GetFloat("CoM Height");
            _inertiaCoeff = PlayerPrefs.GetFloat("Inertia Coefficient"); //can make as a parameter in settings
            _m = _mass*_ankleMassFraction; //mass without foot
            _hCOM = _height*_comFraction; //height of COM
            _i = _inertiaCoeff*_mass*Mathf.Pow(_height, 2); //inertia
            _ankleLength = PlayerPrefs.GetFloat("Ankle Fraction")*_height/100f; //convert percent to fraction
            _lengthOffset = PlayerPrefs.GetFloat("Length Offset")*_YLength/2f/1000f/100f; //convert from percentage to length and mm to m and convert from percent to fraction, also uses half the length
            _ankleQS = _lengthOffset + _ankleLength; //needs to be plus since the length offset is for the heel and the heel is negative on WBB
            _ankleQS = -_lengthOffset + _ankleLength; //do this for now

            Debug.Log(_ankleQS);

            _limits = new List<float>() //front, back, left, right, converted to m, convert from percentage to fraction
            {
                PlayerPrefs.GetFloat("Limit of Stability Front")*_YLength/1000f/100f/2f, //percentage corresponds to length/2
                PlayerPrefs.GetFloat("Limit of Stability Back")*_YLength/1000f/100f/2f,
                PlayerPrefs.GetFloat("Limit of Stability Left")*_XWidth/1000f/100f/2f,
                PlayerPrefs.GetFloat("Limit of Stability Right")*_XWidth/1000f/100f/2f
            };

            _stimMax = new Dictionary<string, float>() //RPF, RDF, LPF, LDF
            {
                ["RPF"] = PlayerPrefs.GetInt("RPF Max"),
                ["RDF"] = PlayerPrefs.GetInt("RDF Max"),
                ["LPF"] = PlayerPrefs.GetInt("LPF Max"),
                ["LDF"] = PlayerPrefs.GetInt("LDF Max")
            };

            _biasCoeffs = new Dictionary<string, List<float>>() //obtained from fitting
            {
                ["RPF"] = new List<float>() 
                {
                    0.99526806515243f, -0.408976667987885f,
                    -0.281770983806191f, 0.134197918133647f,
                    0.0401008570179150f, -0.014741825821627f,
                    -0.001834382543371510f, 0.000541387568634961f
                },
                ["RDF"] = new List<float>()
                {
                    0.1724382284725f, -0.004599540776172f,
                    0.170648963603272f, -0.039721561143300f,
                    -0.008618639088500f, 0.004059400329855f
                },
                ["LPF"] = new List<float>() 
                {
                    0.99526806515243f, -0.408976667987885f,
                    -0.281770983806191f, 0.134197918133647f,
                    0.0401008570179150f, -0.014741825821627f,
                    -0.001834382543371510f, 0.000541387568634961f
                },
                ["LDF"] = new List<float>() 
                {
                    0.1724382284725f, -0.004599540776172f,
                    0.170648963603272f, -0.039721561143300f,
                    -0.008618639088500f, 0.004059400329855f
                }
            };

            //controller constants
            _kp = _kpc*_m*_G*_hCOM; //active/neural controller kp and kd
            _kd = _kdc*_m*_G*_hCOM; 
            _k = _kc*_m*_G*_hCOM; //mechanical/passive controller
            Debug.Log(_kp);
            Debug.Log(_kd);
            Debug.Log(_k);

            _coms = new List<float> {0f, 0f, 0f, 0f, 0f}; //mechanical controller requires 0-2 and neural controller requires 2-4 for derivative
            _angles = new List<float> {0f, 0f, 0f, 0f, 0f};

            //initialize ramping function
            _ramping = new Ramping();
        }

        public Dictionary<string, float> Stimulate(WiiBoardData data, Vector2 targetCoords)
        {
            //shift everything to the perspective of the ankles
            var shiftedCOMy = data.fCopY*_YLength/1000f/2f + _ankleQS; //convert percent to actual length
            //Debug.Log(shiftedCOMy);
            //conversion of target game coords to board coords
            var targetCoordsShifted = new Vector2(); //shifted target coords from game coords to board coords
            
            if (targetCoords.y >= _maxY / 2) //when the target is beyond half way forward in ap direction
                targetCoordsShifted.y = (targetCoords.y - Camera.main.transform.position.y)*_limits[0]*2f/_maxY + _ankleQS;
            else //if it's not greater, it has to be smaller
                targetCoordsShifted.y = (targetCoords.y - Camera.main.transform.position.y)*_limits[1]*2f/_maxY + _ankleQS;

            if (targetCoords.x >= _maxX / 2) //when the target is beyond half way right in ml direction
                targetCoordsShifted.x = (targetCoords.x - Camera.main.transform.position.x)*_limits[3]*2f/_maxX;
            else //if it's not greater, it has to be smaller
                targetCoordsShifted.x = (targetCoords.x - Camera.main.transform.position.x)*_limits[2]*2f/_maxX;

            ShiftedPos = new List<float>() {shiftedCOMy, targetCoordsShifted.x, targetCoordsShifted.y};
            
            var losF = _limits[0] + _ankleQS;
            var losB = _ankleQS - _limits[1];

            // calculating mechanical torques
            var qsTorque = _m*_G*_ankleQS;
            var losFTorque = _m*_G*losF;
            var losBTorque = _m*_G*losB;

            //angle calculations
            var targetVertAng = Mathf.Atan2(targetCoordsShifted.y, _hCOM);
            var comVertAng = Mathf.Atan2(shiftedCOMy, _hCOM);
            var qsVertAng = Mathf.Atan2(_ankleQS, _hCOM);
            var angErr = targetVertAng - comVertAng;
            Angles = new List<float>() {targetVertAng, comVertAng, angErr}; //store in prop

            //first need to adjust the stored COM values as new values are added from cursor
            for (var i = _coms.Count - 1; i >= 0; i--)
            {
                if (i == 0)
                    _coms[i] = shiftedCOMy;
                else
                    _coms[i] = _coms[i - 1];
            }

            if (targetCoords == _previousTarget)
                _neuralCounter++;
            else
            {
                _neuralCounter = 0;
                _previousTarget = targetCoords;
            }

            //controller calculations
            var neuralTorque = NeuralController(angErr); //calculate neural torque from controller output

            if (_neuralCounter <= 3) //only calculate neural torque after 3 iterations have passed
                neuralTorque = 0f; 
                
            var mechanicalTorque = MechanicalController(comVertAng); //calculate passive torque from controller output
            var slopes = Slopes(qsTorque, losFTorque, losBTorque); //calculate controller slopes
            var rawStimOutput = UnbiasedStimulationOutput(slopes, neuralTorque, mechanicalTorque, qsTorque, comVertAng, qsVertAng); //calculate unbiased output
            var neuralBiases = CalculateNeuralBiases(data, targetCoordsShifted); //calculate neural biases
            var mechBiases = CalculateMechBiases(data, shiftedCOMy); //calculate mech biases
            var actualStimOutput = CheckLimits(AdjustedCombinedStimulation(neuralBiases, mechBiases, rawStimOutput));
            
            return actualStimOutput;
        }

        private float NeuralController(float error) //calculate torque based on neural command
        {
            var derivativeError = 0.0f;

            if (!_coms.GetRange(2, 3).Any(v => v == 0.0f)) //make sure that the vector of com is filled (ie. nothing is zero), 2 point delay on neural controller
            {
                derivativeError = CalculateDerivative(_coms.GetRange(2, 3)); //two point delay on the neural controller
                return _kp*error + _kd*derivativeError;
            }

            return 0f; //return zero when the condition above isn't fulfilled
        }

        private float MechanicalController(float verticalCOMAng) //calculate torque based on mechanical properties
        {
            var velocity = 0.0f;

            if (!_coms.GetRange(0, 3).Any(v => v == 0.0f)) //make sure that the vector of com is filled (ie. nothing is zero)
                velocity = CalculateDerivative(_coms.GetRange(0, 3));

            return _k*verticalCOMAng + 5.0f*velocity;
        }

        private Dictionary<string, Dictionary<string, float>> Slopes(float qsTorque, float losBTorque, float losFTorque) //calculate slopes for neural and mech controllers
        {
            var slopes = new Dictionary<string, Dictionary<string, float>>()
            {
                ["Mech"] = new Dictionary<string, float>(),
                ["Neural"] = new Dictionary<string, float>()
            };

            foreach (var control in slopes)
            {
                foreach (var muscles in _stimMax) //we just wanna iterate through this to get the names of the muscle groups
                {
                    if (control.Key == "Mech") //mechanical controller slopes
                    {
                        switch (muscles.Key)
                        {
                            case "RPF":
                            case "LPF":
                                slopes[control.Key].Add(muscles.Key, _stimMax[muscles.Key] / (losFTorque / 2));
                                break;
                            case "RDF":
                            case "LDF":
                                slopes[control.Key].Add(muscles.Key, _stimMax[muscles.Key] / ((losBTorque - qsTorque) / 2));
                                break;
                        }
                    }
                    else //neural controller slopes
                    {
                        switch (muscles.Key)
                        {
                            case "RPF":
                            case "LPF":
                                slopes[control.Key].Add(muscles.Key, _stimMax[muscles.Key] / ((losFTorque - losBTorque)/ 2));

                                break;
                            case "RDF":
                            case "LDF":
                                slopes[control.Key].Add(muscles.Key, _stimMax[muscles.Key] / ((losBTorque - losFTorque) / 2));
                                break;
                        }
                    }
                }
            }

            return slopes;
        }

        public float CalculateDerivative(List<float> comsVector) => (3.0f*comsVector[0] - 4.0f*comsVector[1] + comsVector[2])/(2*Time.fixedDeltaTime);

        private Dictionary<string, Dictionary<string, float>> UnbiasedStimulationOutput(Dictionary<string, Dictionary<string, float>> slopes, 
                                                                                        float neuralTorque, float mechanicalTorque, 
                                                                                        float qsTorque, float comVertAng, float qsVertAng) //calculate stimulation output based on calculated torques from the controller
        {
            var stimulation = new Dictionary<string, Dictionary<string, float>>
            {
                ["Mech"] = new Dictionary<string, float>() {["RPF"] = 0f, ["RDF"] = 0f, ["LPF"] = 0f, ["LDF"] = 0f}, 
                ["Neural"] = new Dictionary<string, float>() {["RPF"] = 0f, ["RDF"] = 0f, ["LPF"] = 0f, ["LDF"] = 0f}
            };

            foreach (var control in slopes)
            {
                foreach (var stim in control.Value) 
                {
                    if (control.Key == "Mech")
                    {
                        if (0.5f*mechanicalTorque > 0 && stim.Key.Contains("PF")) //only calculate stim for pf if 0.5*mechanicalTorque > 0
                            stimulation[control.Key][stim.Key] = slopes[control.Key][stim.Key]*0.5f*mechanicalTorque;
                        if (0.5f*qsTorque > 0.5f*mechanicalTorque && stim.Key.Contains("DF")) //only calculate stim for df if 0.5f*qsTorque > 0.5f*mechanicalTorque > 0
                            stimulation[control.Key][stim.Key] = slopes[control.Key][stim.Key]*0.5f*mechanicalTorque;
                    }
                    else
                    {
                        if (comVertAng > 0 && stim.Key.Contains("PF")) //only calculate stim for pf if comVertAng > 0
                            stimulation[control.Key][stim.Key] = slopes[control.Key][stim.Key]*0.5f*neuralTorque;
                        if (comVertAng < qsVertAng && stim.Key.Contains("DF")) //only calculate stim for df if comVertAng < qsVertAng
                            stimulation[control.Key][stim.Key] = slopes[control.Key][stim.Key]*0.5f*neuralTorque;
                    }
                    
                }
            }

            return stimulation;
        }

        private Dictionary<string, float> CalculateNeuralBiases(WiiBoardData data, Vector2 targetCoords) //calculate ML bias for neural torque
        {
            var biases = new Dictionary<string, float>();
            var x = data.fCopX - targetCoords.x;
            var y = data.fCopY - targetCoords.y;
            var biasAng = -Mathf.Atan2(y, x);

            BiasFunction(biases, biasAng);

            return biases;
        }

        private Dictionary<string, float> CalculateMechBiases(WiiBoardData data, float shiftedCOMy) //calculate ML bias for mechanical torque
        {
            var biases = new Dictionary<string, float>();
            var biasAng = -Mathf.Atan2(shiftedCOMy, data.fCopX);

            BiasFunction(biases, biasAng);

            return biases;
        }

        private void BiasFunction(Dictionary<string, float> biases, float biasAng) //calculate bias using a polynomial fit
        {
            foreach (var item in _biasCoeffs) 
            {
                var bias = 0f;

                if (item.Key == "LPF" || item.Key == "RPF")
                {
                    for (var i = 7; i >= 0; i--)
                        bias += item.Value[i] * Mathf.Pow(biasAng, i);
                }
                else
                {
                    for (var i = 5; i >= 0; i--)
                        bias += item.Value[i] * Mathf.Pow(biasAng, i);
                }

                biases.Add(item.Key, bias);
            }
        }

        private Dictionary<string, float> AdjustedCombinedStimulation(Dictionary<string, float> neuralBiases, Dictionary<string, float> mechBiases, 
                                                                      Dictionary<string, Dictionary<string, float>> rawStimOutput) //applies ML biases and ramping to raw stimulation
        {
            var adjustedStimOutput = new Dictionary<string, Dictionary<string, float>>()
            {
                ["Neural"] = new Dictionary<string, float>(),
                ["Mech"] = new Dictionary<string, float>()
            };
            var adjustedCombinedStimOutput = new Dictionary<string, float>(4);
            var biasesCombined = new Dictionary<string, Dictionary<string, float>>()
            {
                ["Neural"] = neuralBiases,
                ["Mech"] = mechBiases
            };

            RampPercentage = _ramping.CalculateRamp();

            foreach (var control in rawStimOutput)
            {
                foreach (var stim in control.Value)
                {
                    adjustedStimOutput[control.Key].Add(stim.Key, RampPercentage*stim.Value/100f*biasesCombined[control.Key][stim.Key]); //divide by 100 to conver to decimal

                    if (stim.Key.Contains("PF")) //divide the maximum possible stim for pf and df stim
                        adjustedStimOutput[control.Key][stim.Key] /= _MaxPFStim;
                    else
                        adjustedStimOutput[control.Key][stim.Key] /= _MaxDFStim;
                }
            }

            foreach(var muscle in _stimMax) //this is just to add the total stimulation output, don't actually care about _stimMax at this point
                adjustedCombinedStimOutput.Add(muscle.Key, adjustedStimOutput["Mech"][muscle.Key] + adjustedStimOutput["Neural"][muscle.Key]);

            return adjustedCombinedStimOutput;
        }

        private Dictionary<string, float> CheckLimits(Dictionary<string, float> adjustedStimOutput) //makes sure stim doesn't go above max and below 0
        {
            var trueStimOutput = new Dictionary<string, float>(4);

            foreach (var stim in adjustedStimOutput)
            {
                if (stim.Value > _stimMax[stim.Key]) //make sure stimulation never goes above max
                    trueStimOutput.Add(stim.Key, _stimMax[stim.Key]);
                else if (stim.Value < 0) //if stim is for some reason negative, always set it back to zero
                    trueStimOutput.Add(stim.Key, 0f);
                else
                    trueStimOutput.Add(stim.Key, stim.Value);
            }

            return trueStimOutput;
        }
    }
}