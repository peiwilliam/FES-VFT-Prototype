using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        private float _ankleDisplacement; //to shift everything to the reference point of the ankle (ie. ankle is at y = 0)
        private float _ankleMassFraction;
        private float _comFraction;
        private float _inertiaCoeff;
        private float _ankleLength;
        private float _lengthOffset;
        private float _m;
        private float _hCOM;
        private float _i;
        private float _kp;
        private float _kd;
        private float _k;
        //counter for keeping track of when the derivative should be turned on for the controller
        private int _neuralCounter;
        //game screen params
        private float _maxX = 2f*5f*16f/9f; //2*height*aspect ratio
        private float _maxY = 5f*2f; //2*camera size
        //controller parameters
        private float _losF;
        private float _losB;
        private float _qsTorque;
        private float _losFTorque;
        private float _losBTorque;
        private Vector2 _previousTarget;
        private List<float> _limits;
        private List<float> _comAngles;
        private List<float> _comAngleErrors;
        private Dictionary<string, float> _stimMax;
        private Dictionary<string, float> _intercepts; // only need intercepts for mechanical controller stim calculation
        private Dictionary<string, List<float>> _biasCoeffs;
        private Dictionary<string, Dictionary<string, float>> _slopes;
        //ramping function
        private Ramping _ramping;
        private Cursor _cursor;

        private const float _G = 9.81f; //m/s^2
        private const float _XWidth = 433f; // mm
        private const float _YLength = 238f; // mm
        private const float _HeelLocation = 90; //mm, measured manually from centre of board to bottom of indicated feet area
        private const float _MaxPFStim = 1.117055995961f; // not sure what these units are
        private const float _MaxDFStim = 1.170727515177f; // not sure what these units are either

        public float RampPercentage { get; private set; }
        public List<float> Angles { get; private set; }
        public List<float> ShiftedPos { get; private set; } //get com and target positions in real coordinates and shifted to ankle perspective

        public Controller(Cursor cursor)
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
            _ankleLength = PlayerPrefs.GetFloat("Ankle Fraction")*_height/100f; //convert percent to fraction, cm to m
            _lengthOffset = PlayerPrefs.GetFloat("Length Offset")/100f; //keep in fraction form since it's used in different contexts, is also negative!!!
            _ankleDisplacement = _HeelLocation/1000f - _ankleLength; //to change everything to ankle reference frame
            //initialize cursor object
            _cursor = cursor;
            
            _limits = new List<float>() //front, back, left, right, converted to m, convert from percentage to fraction
            {
                (PlayerPrefs.GetFloat("Limit of Stability Front")/100f - _cursor.LOSShift*2f/_maxY)*_YLength/1000f/2f, //percentage corresponds to length/2, also need to account for extra shift that's only in los
                (PlayerPrefs.GetFloat("Limit of Stability Back")/100f - _cursor.LOSShift*2f/_maxY)*_YLength/1000f/2f,
                PlayerPrefs.GetFloat("Limit of Stability Left")/100f*_XWidth/1000f/2f,
                PlayerPrefs.GetFloat("Limit of Stability Right")/100f*_XWidth/1000f/2f
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

            _comAngles = new List<float> {0f, 0f, 0f}; //for mechanical controller only
            _comAngleErrors = new List<float> {0f, 0f, 0f, 0f, 0f}; //neural controller requires 2-4 for derivative

            //initialize ramping function
            _ramping = new Ramping();
            
            // for (var i  = 0; i < _limits.Count - 1; i++)
            // {
            //     switch (i)
            //     {
            //         case 0:
            //             Debug.Log("front");
            //             break;
            //         case 1:
            //             Debug.Log("back");
            //             break;
            //         case 2:
            //             Debug.Log("left");
            //             break;
            //         case 3:
            //             Debug.Log("right");
            //             break;
            //     }
            //     Debug.Log(_limits[i]);
            // }   

            _losF = _limits[0] + _ankleDisplacement + _lengthOffset*_YLength/1000f/2f; //adjust front and back limits to ankle reference frame
            _losB = _ankleDisplacement + _lengthOffset*_YLength/1000f/2f - _limits[1]; //since torque is negative, sign of losB should also be negative
            // Debug.Log("losF");
            // Debug.Log(_losF);
            // Debug.Log("losB");
            // Debug.Log(_losB);
            // calculating mechanical torques
            _qsTorque = _m*_G*(_ankleDisplacement + _lengthOffset*_YLength/1000f/2f);
            _losFTorque = _m*_G*_losF;
            _losBTorque = _m*_G*_losB;
            // Debug.Log("qsTorque");
            // Debug.Log(_qsTorque);
            // Debug.Log("losFTorque");
            // Debug.Log(_losFTorque);
            // Debug.Log("losBTorque");
            // Debug.Log(_losBTorque);

            _slopes = Slopes(); //calculate controller slopes
            _intercepts = new Dictionary<string, float> {["RDF"] = -_slopes["Mech"]["RDF"]*0.5f*_qsTorque, ["LDF"] = -_slopes["Mech"]["LDF"]*0.5f*_qsTorque};
        }

        public Dictionary<string, float> Stimulate(WiiBoardData data, Vector2 targetCoords)
        {
            var shiftedComY = 0.0f;
            var comX = 0.0f;

            if (Wii.GetRemoteCount() == 0) //if we're using the cursor to debug
            {
                shiftedComY = data.fCopY; //conversion to ankle reference frame done in cursor.cs
                comX = data.fCopX;
            }
            else
            {
                //shift everything to the perspective of the ankles
                //note that the cop stored in wiiboarddata has been shifted to the reference frame of the qs cop
                shiftedComY = (data.fCopY + _lengthOffset)*_YLength/1000f/2f + _ankleDisplacement; //convert percent to actual length
                comX = data.fCopX*_XWidth/1000f/2f;
            }
            
            //conversion of target game coords to board coords
            var targetCoordsShifted = new Vector2(); //shifted target coords from game coords to board coords
            
            if (SceneManager.GetActiveScene().name != "Target") 
            {
                var yLimit = 0f;
                var xLimit = 0f;

                if (targetCoords.y >= _maxY / 2) //when the target is beyond half way forward in ap direction
                    yLimit = _limits[0];
                else //if it's not greater, it has to be smaller
                    yLimit = _limits[1];

                if (targetCoords.x >= _maxX / 2) //when the target is beyond half way forward in ap direction
                    xLimit = _limits[3];
                else //if it's not greater, it has to be smaller
                    xLimit = _limits[2];

                targetCoordsShifted.x = (targetCoords.x - Camera.main.transform.position.x)*xLimit*_XWidth/1000f/_maxX;
                targetCoordsShifted.y = (targetCoords.y - Camera.main.transform.position.y)*yLimit*_YLength/1000f/_maxY + _ankleDisplacement + _lengthOffset*_YLength/1000f/2f; //need to account for _lengthOffset since targets in game are with respect to the cop shifted to the quiet standing centre of pressure.
                
            }
            else
            {
                //target game assumes that target is at 0,0 wrt length offset
                //we just need to do the shift factor and no additional conversions or shifts are required unlike the other games.
                targetCoordsShifted.y = _ankleDisplacement + _lengthOffset*_YLength/1000f/2f; //need to add length offset so that we get the proper shift to the target wrt ankle position
            }
            
            // Debug.Log("fcopy");
            // Debug.Log(data.fCopY);
            // Debug.Log("shiftedcomy");
            // Debug.Log(shiftedComY);
            // Debug.Log("targetcoords");
            // Debug.Log(targetCoords.y);
            // Debug.Log("targetcoordsshifted");
            // Debug.Log(targetCoordsShifted.y);
            // Debug.Log("ankledisplacement2");
            // Debug.Log(_ankleDisplacement);
            // Debug.Log("length offset2");
            // Debug.Log(_lengthOffset);

            ShiftedPos = new List<float>() {shiftedComY, targetCoordsShifted.x, targetCoordsShifted.y};

            //angle calculations
            var targetVertAng = Mathf.Atan2(targetCoordsShifted.y, _hCOM);
            var comVertAng = Mathf.Atan2(shiftedComY, _hCOM);
            var qsVertAng = Mathf.Atan2(_ankleDisplacement + _lengthOffset*_YLength/1000f/2f, _hCOM);
            var angErr = targetVertAng - comVertAng;
            Angles = new List<float>() {targetVertAng, comVertAng, angErr}; //store in prop

            //first need to adjust the stored COM values as new values are added from cursor, max is count - 2 so that we don't get index error
            for (var i = _comAngles.Count - 1; i >= 0; i--)
            {
                if (i == 0)
                    _comAngles[i] = comVertAng;
                else
                    _comAngles[i] = _comAngles[i - 1];
            }

            for (var i = _comAngleErrors.Count - 1; i >= 0; i--)
            {
                if (i == 0)
                    _comAngleErrors[i] = angErr;
                else
                    _comAngleErrors[i] = _comAngleErrors[i - 1];
            }

            // since previoustarget is initialized as 0,0, we set it to the current target coords
            if (_previousTarget == new Vector2(0f, 0f))
                _previousTarget = targetCoordsShifted;

            if (targetCoordsShifted == _previousTarget)
                _neuralCounter++;
            else
            {
                _neuralCounter = 0;
                _previousTarget = targetCoordsShifted;
            }

            //controller calculations
            var neuralTorque = NeuralController(); //calculate neural torque from controller output

            if (_neuralCounter < 3) //only calculate neural torque after 3 iterations have passed
                neuralTorque = 0f;
                
            var mechanicalTorque = MechanicalController(); //calculate passive torque from controller output

            // Debug.Log("neuraltorque");
            // Debug.Log(neuralTorque);
            // Debug.Log("mechanicaltorque");
            // Debug.Log(mechanicalTorque);

            var rawStimOutput = UnbiasedStimulationOutput(neuralTorque, mechanicalTorque, comVertAng, qsVertAng); //calculate unbiased output

            // foreach (var i in rawStimOutput)
            // {
            //     foreach (var j in i.Value)
            //     {
            //         Debug.Log(j.Key);
            //         Debug.Log(j.Value);
            //     }
            // }
            
            var neuralBiases = CalculateNeuralBiases(comX, shiftedComY, targetCoordsShifted); //calculate neural biases
            var mechBiases = CalculateMechBiases(comX, shiftedComY); //calculate mech biases

            // foreach (var i in neuralBiases)
            // {
            //     Debug.Log(i.Key);
            //     Debug.Log(i.Value);
            // }

            // foreach (var i in mechBiases)
            // {
            //     Debug.Log(i.Key);
            //     Debug.Log(i.Value);
            // }

            var actualStimOutput = CheckLimits(AdjustedCombinedStimulation(neuralBiases, mechBiases, rawStimOutput));
            
            return actualStimOutput;
        }

        private float NeuralController() //calculate torque based on neural command
        {
            var derivativeError = 0.0f;

            if (!_comAngleErrors.GetRange(2, 3).Any(v => v == 0.0f)) //make sure that the vector of com is filled (ie. nothing is zero), 2 point delay on neural controller
            {
                derivativeError = CalculateDerivative(_comAngleErrors.GetRange(2, 3)); //two point delay on the neural controller
                return _kp*_comAngleErrors.GetRange(2, 3)[0] + _kd*derivativeError;
            }

            return 0f; //return zero when the condition above isn't fulfilled
        }

        private float MechanicalController() //calculate torque based on mechanical properties
        {
            var velocity = 0.0f;

            if (!_comAngles.Any(v => v == 0.0f)) //make sure that the vector of com is filled (ie. nothing is zero)
                velocity = CalculateDerivative(_comAngles);

            return _k*_comAngles[0] + 5.0f*velocity;
        }

        private Dictionary<string, Dictionary<string, float>> Slopes() //calculate slopes for neural and mech controllers
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
                                slopes[control.Key].Add(muscles.Key, _stimMax[muscles.Key] / (_losFTorque / 2f));
                                break;
                            case "RDF":
                            case "LDF":
                                slopes[control.Key].Add(muscles.Key, -_stimMax[muscles.Key] / ((_qsTorque - _losBTorque) / 2f));
                                break;
                        }
                    }
                    else //neural controller slopes
                    {
                        switch (muscles.Key)
                        {
                            case "RPF":
                            case "LPF":
                                slopes[control.Key].Add(muscles.Key, _stimMax[muscles.Key] / ((_losFTorque - _losBTorque) / 2f));

                                break;
                            case "RDF":
                            case "LDF":
                                slopes[control.Key].Add(muscles.Key, _stimMax[muscles.Key] / ((_losBTorque - _losFTorque) / 2f));
                                break;
                        }
                    }
                }
            }

            return slopes;
        }

        public float CalculateDerivative(List<float> comsVector) => (3.0f*comsVector[0] - 4.0f*comsVector[1] + comsVector[2])/(2*Time.fixedDeltaTime);

        private Dictionary<string, Dictionary<string, float>> UnbiasedStimulationOutput(float neuralTorque, float mechanicalTorque, 
                                                                                        float comVertAng, float qsVertAng) //calculate stimulation output based on calculated torques from the controller
        {
            var stimulation = new Dictionary<string, Dictionary<string, float>>
            {
                ["Mech"] = new Dictionary<string, float>() {["RPF"] = 0f, ["RDF"] = 0f, ["LPF"] = 0f, ["LDF"] = 0f}, 
                ["Neural"] = new Dictionary<string, float>() {["RPF"] = 0f, ["RDF"] = 0f, ["LPF"] = 0f, ["LDF"] = 0f}
            };

            foreach (var control in _slopes)
            {
                foreach (var stim in control.Value) 
                {
                    if (control.Key == "Mech")
                    {
                        if (0.5f*mechanicalTorque > 0 && stim.Key.Contains("PF")) //only calculate stim for pf if 0.5*mechanicalTorque > 0
                            stimulation[control.Key][stim.Key] = _slopes[control.Key][stim.Key]*0.5f*mechanicalTorque;
                        else if (0.5f*_qsTorque > 0.5f*mechanicalTorque && stim.Key.Contains("DF")) //only calculate stim for df if 0.5f*qsTorque > 0.5f*mechanicalTorque > 0
                        {
                            if (stim.Key == "RDF")
                                stimulation[control.Key][stim.Key] = _slopes[control.Key][stim.Key]*0.5f*mechanicalTorque + _intercepts[stim.Key];
                            else
                                stimulation[control.Key][stim.Key] = _slopes[control.Key][stim.Key]*0.5f*mechanicalTorque + _intercepts[stim.Key];
                        }
                           
                    }
                    else
                    {
                        if (comVertAng > 0 && stim.Key.Contains("PF")) //only calculate stim for pf if comVertAng > 0
                            stimulation[control.Key][stim.Key] = _slopes[control.Key][stim.Key]*0.5f*neuralTorque;
                        else if (comVertAng < qsVertAng && stim.Key.Contains("DF")) //only calculate stim for df if comVertAng < qsVertAng
                            stimulation[control.Key][stim.Key] = _slopes[control.Key][stim.Key]*0.5f*neuralTorque;
                    }
                    
                }
            }

            return stimulation;
        }

        private Dictionary<string, float> CalculateNeuralBiases(float comX, float shiftedCOMy, Vector2 shiftedTargetCoords) //calculate ML bias for neural torque
        {
            var x = shiftedTargetCoords.x - comX;
            var y = shiftedTargetCoords.y - shiftedCOMy;
            // Debug.Log("y");
            // Debug.Log(y);
            // Debug.Log("x");
            // Debug.Log(x);
            // Debug.Log("neural ratio");
            // Debug.Log(y/x);
            // Debug.Log("neural atan");
            // Debug.Log(Mathf.Atan(y/x));
            // Debug.Log("neural atan2");
            // Debug.Log(Mathf.Atan2(y, x));
            var biasAng = Mathf.Atan2(x, y);
            
            // Debug.Log("neural bias");
            // Debug.Log(biasAng);

            var biases = BiasFunction(biasAng);

            return biases;
        }

        private Dictionary<string, float> CalculateMechBiases(float comX, float shiftedCOMy) //calculate ML bias for mechanical torque
        {
            // Debug.Log("y");
            // Debug.Log(shiftedCOMy);
            // Debug.Log("x");
            // Debug.Log(comX);
            // Debug.Log("mech ratio");
            // Debug.Log(shiftedCOMy/comX);
            // Debug.Log("mech atan");
            // Debug.Log(Mathf.Atan(shiftedCOMy/comX));
            // Debug.Log("mech atan2");
            // Debug.Log(Mathf.Atan2(shiftedCOMy, comX));
            var biasAng = Mathf.Atan2(comX, shiftedCOMy);

            // Debug.Log("mech bias");
            // Debug.Log(biasAng);

            var biases = BiasFunction(biasAng);

            return biases;
        }

        private Dictionary<string, float> BiasFunction(float biasAng) //calculate bias using a polynomial fit
        {
            var biases = new Dictionary<string, float>();

            foreach (var coeffs in _biasCoeffs) 
            {
                var bias = 0f;
                if (coeffs.Key == "LPF" || coeffs.Key == "RPF")
                {
                    for (var i = 7; i >= 0; i--)
                    {
                        if (coeffs.Key.Contains("R"))
                            bias += coeffs.Value[i] * Mathf.Pow(-biasAng, i);
                        else
                            bias += coeffs.Value[i] * Mathf.Pow(biasAng, i);
                    }
                }
                else
                {
                    for (var i = 5; i >= 0; i--)
                    {
                        if (coeffs.Key.Contains("R"))
                            bias += coeffs.Value[i] * Mathf.Pow(-biasAng, i);
                        else
                            bias += coeffs.Value[i] * Mathf.Pow(biasAng, i);
                    }
                }

                biases.Add(coeffs.Key, bias);
            }

            return biases;
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
                    // Debug.Log(control.Key);
                    // Debug.Log(stim.Key);
                    // Debug.Log(biasesCombined[control.Key][stim.Key]);
                    // Debug.Log(stim.Value);
                    adjustedStimOutput[control.Key].Add(stim.Key, RampPercentage/100f*stim.Value*biasesCombined[control.Key][stim.Key]); //divide by 100 to convert to decimal
                    if (stim.Key.Contains("PF")) //divide the maximum possible stim for pf and df stim
                        adjustedStimOutput[control.Key][stim.Key] /= _MaxPFStim;
                    else
                        adjustedStimOutput[control.Key][stim.Key] /= _MaxDFStim;
                    
                    // Debug.Log(adjustedStimOutput[control.Key][stim.Key]);
                }
            }

            foreach (var muscle in _stimMax) //this is just to add the total stimulation output
            {
                // Debug.Log(muscle.Key);
                // Debug.Log(adjustedStimOutput["Mech"][muscle.Key] + adjustedStimOutput["Neural"][muscle.Key]);
                adjustedCombinedStimOutput.Add(muscle.Key, adjustedStimOutput["Mech"][muscle.Key] + adjustedStimOutput["Neural"][muscle.Key]);
            }
                
                
            // foreach (var i in adjustedCombinedStimOutput)
            // {
            //     Debug.Log(i.Key);
            //     Debug.Log(i.Value);
            // }

            return adjustedCombinedStimOutput;
        }

        private Dictionary<string, float> CheckLimits(Dictionary<string, float> adjustedStimOutput) //makes sure stim doesn't go above max and below 0
        {
            var trueStimOutput = new Dictionary<string, float>();

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