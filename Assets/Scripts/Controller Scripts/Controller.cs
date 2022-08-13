using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ControllerManager
{
    public class Controller
    {
        //whether we're using the cursor or board
        private bool _foundWiiBoard;
        private bool _isTargetGame;
        private string _sceneName;
        //neural controller
        private float _kdc;
        private float _kpc;
        //mechanical controller
        private float _kc;
        //initial ramping
        private float _rampIterations;
        private float _rampPercentage; //starts at zero
        //phyiological constants
        private float _ankleDisplacement; //to shift everything to the reference point of the ankle (ie. ankle is at y = 0)
        private float _lengthOffset;
        private float _m;
        private float _hCOM;
        private float _kp;
        private float _kd;
        private float _k;
        //counter for keeping track of when the derivative should be turned on for the controller, always 0 for ellipse
        private int _neuralCounter;
        //controller parameters
        private float _qsTorque;
        private float _losFTorque;
        private float _losBTorque;
        private float _qsVertAng;
        private float _heelPosition; // m, measured manually from centre of board to bottom of indicated feet area
        private Vector2 _previousTarget;
        private float[] _limits;
        private List<float> _comAngles;
        private List<float> _comAngleErrors;
        private Dictionary<string, float> _stimMax;
        private Dictionary<string, float> _motorThresh;
        private Dictionary<string, float> _qsMechStim; //used to calculate the baseline for pf stimulation
        private Dictionary<string, float> _stimBaseline; //baseline stim levels
        private Dictionary<string, float[]> _biasCoeffs;

        //constants
        private const float _G = 9.81f; //m/s^2
        private const float _XWidth = 433f; // mm
        private const float _YLength = 238f; // mm
        private const float _MaxX = 2f*5f*16f/9f; //2*height*aspect ratio
        private const float _MaxY = 5f*2f; //2*camera size

        //properties
        public float RampPercentage { get; private set; }
        public float NeuralTorque { get; private set; }
        public float MechanicalTorque { get; private set; }
        public List<float> Angles { get; private set; } //float[] not advised by official msdn, so we use list instead
        public List<float> ShiftedPos { get; private set; } //get com and target positions in real coordinates and shifted to ankle perspective
        public List<float> MlAngles { get; private set; }
        public Dictionary<string, float> CalculatedConstants { get; private set; }
        public Dictionary<string, float> Intercepts { get; private set; } // only need intercepts for mechanical controller stim calculation
        public Dictionary<string, Dictionary<string, float>> CalculatedStimBaselines { get; private set; }
        public Dictionary<string, Dictionary<string, float>> Slopes { get; private set; }
        public Dictionary<string, Dictionary<string, float>> Biases { get; private set; }

        public Controller(Cursor cursor, bool foundWiiBoard)
        {
            //define various constants
            _kdc = PlayerPrefs.GetFloat("Kd Coefficient");
            _kpc = PlayerPrefs.GetFloat("Kp Coefficient");
            _kc = PlayerPrefs.GetFloat("K Coefficient");
            //patient parameters
            var height = PlayerPrefs.GetInt("Height")/100f; //convert from cm to m
            var mass = PlayerPrefs.GetFloat("Mass");
            var ankleMassFraction = PlayerPrefs.GetFloat("Ankle Mass Fraction");
            var comFraction = PlayerPrefs.GetFloat("CoM Height");
            var inertiaCoeff = PlayerPrefs.GetFloat("Inertia Coefficient"); //can make as a parameter in settings
            var ankleLength = PlayerPrefs.GetFloat("Ankle Fraction")*height/100f; //convert percent to fraction, cm to m
            _m = mass*ankleMassFraction; //mass without foot
            _hCOM = height*comFraction; //height of COM
            _lengthOffset = PlayerPrefs.GetFloat("Length Offset")/100f; //keep in fraction form since it's used in different contexts, is also negative!!!
            _heelPosition = PlayerPrefs.GetFloat("Heel Position");
            _ankleDisplacement = _heelPosition - ankleLength; //to change everything to ankle reference frame
            //if true, we use the board, if false, we use cursor.
            _foundWiiBoard = foundWiiBoard;
            _sceneName = SceneManager.GetActiveScene().name;
            _isTargetGame = _sceneName == "Target" ? true : false;
            //define ramping parameters
            _rampIterations = 100f/(PlayerPrefs.GetFloat("Ramp Duration")/Time.fixedDeltaTime); //how much to ramp per iteration to get to 100% stim output in 1 second
            
            //initialize MlAngles property
            MlAngles = new List<float>() {0f, 0f};

            // need to convert from percent to fraction
            // los is in qs frame of reference but need to remove shift that's inherent to los
            _limits = new float[] //front, back, left, right
            {
                PlayerPrefs.GetFloat("Limit of Stability Front")/100f - cursor.LOSShift*2f/_MaxY,
                PlayerPrefs.GetFloat("Limit of Stability Back")/100f - cursor.LOSShift*2f/_MaxY,
                PlayerPrefs.GetFloat("Limit of Stability Left")/100f,
                PlayerPrefs.GetFloat("Limit of Stability Right")/100f
            };

            _motorThresh = new Dictionary<string, float>()
            {
                ["RPF"] = PlayerPrefs.GetInt("RPF Motor Threshold"),
                ["RDF"] = PlayerPrefs.GetInt("RDF Motor Threshold"),
                ["LPF"] = PlayerPrefs.GetInt("LPF Motor Threshold"),
                ["LDF"] = PlayerPrefs.GetInt("LDF Motor Threshold")
            };

            _stimMax = new Dictionary<string, float>()
            {
                ["RPF"] = PlayerPrefs.GetInt("RPF Max"),
                ["RDF"] = PlayerPrefs.GetInt("RDF Max"),
                ["LPF"] = PlayerPrefs.GetInt("LPF Max"),
                ["LDF"] = PlayerPrefs.GetInt("LDF Max")
            };

            _biasCoeffs = new Dictionary<string, float[]>() //obtained from fitting
            {
                ["RPF"] = new float[] 
                {
                    0.99526806515243f, -0.408976667987885f,
                    -0.281770983806191f, 0.134197918133647f,
                    0.0401008570179150f, -0.014741825821627f,
                    -0.001834382543371510f, 0.000541387568634961f
                },
                ["RDF"] = new float[]
                {
                    0.1724382284725f, -0.004599540776172f,
                    0.170648963603272f, -0.039721561143300f,
                    -0.008618639088500f, 0.004059400329855f
                },
                ["LPF"] = new float[] 
                {
                    0.99526806515243f, -0.408976667987885f,
                    -0.281770983806191f, 0.134197918133647f,
                    0.0401008570179150f, -0.014741825821627f,
                    -0.001834382543371510f, 0.000541387568634961f
                },
                ["LDF"] = new float[] 
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

            _comAngles = new List<float> {0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f}; //for mechanical controller only, mechanical uses 0-2 and absolute neural uses 5-7
            _comAngleErrors = new List<float> {0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f}; //neural controller requires 5-7 for derivative, delay of 100ms

            //shift los to ankle reference frame for calculating torque
            var losF = (_limits[0] + _lengthOffset)*_YLength/1000f/2f + _ankleDisplacement; //adjust front and back limits to ankle reference frame
            var losB = _ankleDisplacement + (_lengthOffset - _limits[1])*_YLength/1000f/2f; //since torque is negative, sign of losB should also be negative

            //calculating mechanical torques
            _qsTorque = _m*_G*(_ankleDisplacement + _lengthOffset*_YLength/1000f/2f);
            _losFTorque = _m*_G*losF;
            _losBTorque = _m*_G*losB;

            Slopes = GetSlopes(); //calculate controller slopes
            Intercepts = new Dictionary<string, float> {["RDF"] = -Slopes["Mech"]["RDF"]*_qsTorque, 
                                                         ["LDF"] = -Slopes["Mech"]["LDF"]*_qsTorque};
            _qsVertAng = Mathf.Atan2(_ankleDisplacement + _lengthOffset*_YLength/1000f/2f, _hCOM);

            //calculate the qs stim (no derivative) for pf rescaling
            _qsMechStim = UnbiasedStimulationOutput(0, _qsTorque, 0)["Mech"];
            //calculating the baseline stim level, pf is a special calculation for the baseline and df is from motor threshold
            _stimBaseline = new Dictionary<string, float>();
            foreach (var baseline in _qsMechStim)
            {
                if (baseline.Key.Contains("PF")) // baseline = (QS stim - MT stim)/(QS stim/max stim - 1)
                    _stimBaseline[baseline.Key] = (_qsMechStim[baseline.Key] - _motorThresh[baseline.Key])
                                                  /(_qsMechStim[baseline.Key]/_stimMax[baseline.Key] - 1);
                else
                    _stimBaseline[baseline.Key] = _motorThresh[baseline.Key]; //baseline is just motor threshold
            }

            CalculatedConstants = new Dictionary<string, float>() //fill property
            {
                ["Kp"] = _kp, 
                ["Kd"] = _kd, 
                ["K"] = _k, 
                ["LOSF"] = losF, 
                ["LOSB"] = losB, 
                ["QSTorque"] = _qsTorque, 
                ["LOSFTorque"] = _losFTorque, 
                ["LOSBTorque"] = _losBTorque
            };

            CalculatedStimBaselines = new Dictionary<string, Dictionary<string, float>>()
            {
                ["QS Stim"] = _qsMechStim,
                ["Stim Baselines"] = _stimBaseline
            };
        }

        public Dictionary<string, Dictionary<string, float>> Stimulate(WiiBoardData data, Vector2 targetCoords)
        {
            //Shift target and com then calculate the respective vertical angles
            var (shiftedComY, comX, targetCoordsShifted) = ShiftComAndTarget(data, targetCoords);
            var (targetVertAng, comVertAng) = GetAngles(shiftedComY, targetCoordsShifted);
            IncrementNeuralCounter(targetCoordsShifted);

            //controller calculations
            NeuralTorque = NeuralDifferentialController(); //calculate neural torque from controller output
            MechanicalTorque = MechanicalController() + NeuralAbsoluteController(); //calculate passive torque from controller output
            var rawStimOutput = UnbiasedStimulationOutput(NeuralTorque, MechanicalTorque, comVertAng); //calculate unbiased output
            var neuralBiases = CalculateNeuralBiases(comX, shiftedComY, targetCoordsShifted); //calculate neural biases
            var mechBiases = CalculateMechBiases(comX, shiftedComY); //calculate mech biases
            var limitedStim = CheckLimits(AdjustedCombinedStimulation(neuralBiases, mechBiases, rawStimOutput));
            var actualStimOutput = RescaleStimulation(limitedStim); //this was added because if stim starts from 0, most of stim is below MT, which we don't want
            var unbiasedStimOutput = new Dictionary<string, float>();

            //properties for store data and write to csv
            Biases = new Dictionary<string, Dictionary<string, float>>()
            {
                ["Neural"] = neuralBiases,
                ["Mech"] = mechBiases
            };
            ShiftedPos = new List<float>() { comX, shiftedComY, targetCoordsShifted.x, targetCoordsShifted.y };
            Angles = new List<float>() { targetVertAng, comVertAng, _comAngleErrors[5] }; //only start saving the errors once there is a value in the 5th spots of the vector

            //want to get unbiased stimulation as well for documentation in data file
            foreach (var muscle in _stimMax) //used for iteration and adding together total unbiased stimulation output
                unbiasedStimOutput[muscle.Key] = rawStimOutput["Mech"][muscle.Key] + rawStimOutput["Neural"][muscle.Key];

            return new Dictionary<string, Dictionary<string, float>>() { ["Unbiased"] = unbiasedStimOutput, ["Actual"] = actualStimOutput };
        }

        private (float, float, Vector2) ShiftComAndTarget(WiiBoardData data, Vector2 targetCoords)
        {
            var shiftedComY = 0.0f;
            var comX = 0.0f;

            if (!_foundWiiBoard) //if we're using the cursor to debug
            {
                shiftedComY = data.fCopY; //conversion to ankle reference frame done in cursor.cs
                comX = data.fCopX;
            }
            else
            {
                //shift everything to the perspective of the ankles
                //note that the cop stored in wiiboarddata has been shifted to the reference frame of the qs cop
                shiftedComY = (data.fCopY + _lengthOffset) * _YLength / 1000f / 2f + _ankleDisplacement; //convert percent to actual length
                comX = data.fCopX * _XWidth / 1000f / 2f;
            }

            //conversion of target game coords to board coords
            var targetCoordsShifted = new Vector2();

            if (_sceneName != "Target")
            {
                var yLimit = 0f;
                var xLimit = 0f;

                if (targetCoords.x >= _MaxX / 2) //when the target is beyond half way forward in ap direction
                    xLimit = _limits[3];
                else //if it's not greater, it has to be smaller
                    xLimit = _limits[2];

                if (targetCoords.y >= _MaxY / 2) //when the target is beyond half way forward in ap direction
                    yLimit = _limits[0];
                else //if it's not greater, it has to be smaller
                    yLimit = _limits[1];

                //need to account for _lengthOffset since targets in game are with respect to the cop shifted to the quiet standing centre of pressure.
                targetCoordsShifted.x = (targetCoords.x - Camera.main.transform.position.x) * xLimit * _XWidth / 1000f / _MaxX;
                targetCoordsShifted.y = (targetCoords.y - Camera.main.transform.position.y) * yLimit * _YLength / 1000f / _MaxY + _ankleDisplacement + _lengthOffset * _YLength / 1000f / 2f;
            }
            else
            {
                //target game assumes that target is at 0,0 wrt length offset
                //we just need to do the shift factor and no additional conversions or shifts are required unlike the other games.
                //need to add length offset so that we get the proper shift to the target wrt ankle position
                targetCoordsShifted.y = _ankleDisplacement + _lengthOffset * _YLength / 1000f / 2f;
            }

            return (shiftedComY, comX, targetCoordsShifted);
        }

        private (float, float) GetAngles(float shiftedComY, Vector2 targetCoordsShifted)
        {
            //angle calculations
            var targetVertAng = Mathf.Atan2(targetCoordsShifted.y, _hCOM);
            var comVertAng = Mathf.Atan2(shiftedComY, _hCOM);
            var angErr = targetVertAng - comVertAng;

            //store the angles in an array, most recent data point is index 0
            for (var i = _comAngles.Count - 1; i >= 0; i--)
            {
                if (i == 0)
                    _comAngles[i] = comVertAng;
                else
                    _comAngles[i] = _comAngles[i - 1];
            }
            
            //store the angle errors in an array, most recent data point is index 0
            for (var i = _comAngleErrors.Count - 1; i >= 0; i--)
            {
                if (i == 0)
                    _comAngleErrors[i] = angErr;
                else
                    _comAngleErrors[i] = _comAngleErrors[i - 1];
            }

            return (targetVertAng, comVertAng);
        }

        private void IncrementNeuralCounter(Vector2 targetCoordsShifted)
        {
            if (_sceneName == "Hunting" || _sceneName == "Colour Matching")
            {
                if (targetCoordsShifted == _previousTarget)
                    _neuralCounter++;
                else
                {
                    _neuralCounter = 0;
                    _previousTarget = targetCoordsShifted;
                }
            }
        }

        private float NeuralDifferentialController() //calculate torque based on neural command
        {
            var derivativeError = 0.0f;
            
            if ((_neuralCounter >= 7 || _isTargetGame) && !_comAngleErrors.GetRange(5, 3).Any(v => v == 0.0f)) //7 correspond to delay of 140ms, original labview delay was 150ms  
                derivativeError = CalculateDerivative(_comAngleErrors.GetRange(5, 3));
            
            return _kp*_comAngleErrors.GetRange(5, 3)[0] + _kd*derivativeError; //only p portion when derivative error vec isn't filled
        }

        private float NeuralAbsoluteController()
        {
            var derivativeError = 0.0f;

            if ((_neuralCounter >= 7 || _isTargetGame) && !_comAngles.GetRange(5, 3).Any(v => v == 0.0f)) //7 correspond to delay of 140ms, original labview delay was 150ms  
                derivativeError = CalculateDerivative(_comAngles.GetRange(5, 3));

            return _kp*_comAngles.GetRange(5, 3)[0] + _kd*derivativeError;
        }

        private float MechanicalController() //calculate torque based on mechanical properties
        {
            var velocity = 0.0f;

            if (!_comAngles.GetRange(0, 3).Any(v => v == 0.0f)) //make sure that the vector of com is filled (ie. nothing is zero), only really initially
                velocity = CalculateDerivative(_comAngles.GetRange(0, 3));

            return _k*_comAngles[0] + 5.0f*velocity;
        }

        private Dictionary<string, Dictionary<string, float>> GetSlopes() //calculate slopes for neural and mech controllers
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
                                slopes[control.Key][muscles.Key] = _stimMax[muscles.Key] / _losFTorque;
                                break;
                            case "RDF":
                            case "LDF":
                                slopes[control.Key][muscles.Key] = -_stimMax[muscles.Key] / (_qsTorque - _losBTorque);
                                break;
                        }
                    }
                    else //neural controller slopes
                    {
                        switch (muscles.Key)
                        {
                            case "RPF":
                            case "LPF":
                                slopes[control.Key][muscles.Key] = _stimMax[muscles.Key] / (_losFTorque - _losBTorque);
                                break;
                            case "RDF":
                            case "LDF":
                                slopes[control.Key][muscles.Key] = _stimMax[muscles.Key] / (_losBTorque - _losFTorque);
                                break;
                        }
                    }
                }
            }

            return slopes;
        }

        private float CalculateDerivative(List<float> comsVector) => (3.0f*comsVector[0] - 4.0f*comsVector[1] + comsVector[2])/(2*Time.fixedDeltaTime);

        //calculate stimulation output based on calculated torques from the controller
        private Dictionary<string, Dictionary<string, float>> UnbiasedStimulationOutput(float neuralTorque, float mechanicalTorque, float comVertAng) 
        {
            var stimulation = new Dictionary<string, Dictionary<string, float>>
            {
                ["Mech"] = new Dictionary<string, float>() {["RPF"] = 0f, ["RDF"] = 0f, ["LPF"] = 0f, ["LDF"] = 0f}, 
                ["Neural"] = new Dictionary<string, float>() {["RPF"] = 0f, ["RDF"] = 0f, ["LPF"] = 0f, ["LDF"] = 0f}
            };

            foreach (var control in Slopes)
            {
                foreach (var stim in control.Value) 
                {
                    if (control.Key == "Mech")
                    {
                        if (0.5f*mechanicalTorque > 0 && stim.Key.Contains("PF")) //only calculate stim for pf if 0.5*mechanicalTorque > 0
                            stimulation[control.Key][stim.Key] = Slopes[control.Key][stim.Key]*mechanicalTorque;
                        else if (0.5f*_qsTorque > 0.5f*mechanicalTorque && stim.Key.Contains("DF")) //only calculate stim for df if 0.5f*qsTorque > 0.5f*mechanicalTorque > 0
                            stimulation[control.Key][stim.Key] = Slopes[control.Key][stim.Key]*mechanicalTorque + Intercepts[stim.Key];
                    }
                    else
                    {
                        if (comVertAng > 0 && stim.Key.Contains("PF")) //only calculate stim for pf if comVertAng > 0
                            stimulation[control.Key][stim.Key] = Slopes[control.Key][stim.Key]*neuralTorque;
                        else if (comVertAng < _qsVertAng && stim.Key.Contains("DF")) //only calculate stim for df if comVertAng < qsVertAng
                            stimulation[control.Key][stim.Key] = Slopes[control.Key][stim.Key]*neuralTorque;
                    }
                }
            }

            return stimulation;
        }

        private Dictionary<string, float> CalculateNeuralBiases(float comX, float shiftedCOMy, Vector2 shiftedTargetCoords) //calculate ML bias for neural torque
        {
            var x = shiftedTargetCoords.x - comX;
            var y = shiftedTargetCoords.y - shiftedCOMy;
            MlAngles[0] = Mathf.Atan2(x, y);
            var biases = BiasFunction(MlAngles[0]);

            return biases;
        }

        private Dictionary<string, float> CalculateMechBiases(float comX, float shiftedCOMy) //calculate ML bias for mechanical torque
        {
            MlAngles[1] = Mathf.Atan2(comX, shiftedCOMy);
            var biases = BiasFunction(MlAngles[1]);

            return biases;
        }

        private Dictionary<string, float> BiasFunction(float biasAng) //calculate bias using a polynomial fit
        {
            var biases = new Dictionary<string, float>();

            foreach (var coeffs in _biasCoeffs) 
            {
                var bias = 0f;
                if (coeffs.Key.Contains("PF"))
                {
                    for (var i = coeffs.Value.Length - 1; i >= 0; i--)
                    {
                        if (coeffs.Key.Contains("R"))
                            bias += coeffs.Value[i] * Mathf.Pow(-biasAng, i);
                        else
                            bias += coeffs.Value[i] * Mathf.Pow(biasAng, i);
                    }
                }
                else
                {
                    for (var i = coeffs.Value.Length - 1; i >= 0; i--)
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

            foreach (var control in rawStimOutput)
            {
                foreach (var stim in control.Value)
                    adjustedStimOutput[control.Key][stim.Key] = stim.Value*biasesCombined[control.Key][stim.Key]; //apply lateral biases
            }

            foreach (var muscle in _stimMax) //this is just to add the total stimulation output
                adjustedCombinedStimOutput[muscle.Key] = adjustedStimOutput["Mech"][muscle.Key] + adjustedStimOutput["Neural"][muscle.Key];

            return adjustedCombinedStimOutput;
        }

        private Dictionary<string, float> CheckLimits(Dictionary<string, float> adjustedStimOutput) //makes sure stim doesn't go above max and below 0
        {
            var trueStimOutput = new Dictionary<string, float>();

            foreach (var stim in adjustedStimOutput)
            {
                if (stim.Value > _stimMax[stim.Key]) //make sure stimulation never goes above max
                    trueStimOutput[stim.Key] = _stimMax[stim.Key];
                else if (stim.Value < 0) //if stim is for some reason negative, always set it back to zero
                    trueStimOutput[stim.Key] = 0f;
                else
                    trueStimOutput[stim.Key] = stim.Value;
            }

            return trueStimOutput;
        }

        private Dictionary<string, float> RescaleStimulation(Dictionary<string, float> limitedStimOutput) //rescale the stimulation so that stimulation doesn't start from 0 and more of the stimulation is above the motor threshold
        {
            var actualStimOutput = new Dictionary<string, float>();

            if (RampPercentage < 100f)
                RampPercentage = CalculateRamp();

            foreach (var stim in limitedStimOutput)
            {
                // new stim = old stim*(max stim - baseline stim)/max stim + baseline stim
                actualStimOutput[stim.Key] = stim.Value*(_stimMax[stim.Key] - _stimBaseline[stim.Key]) / _stimMax[stim.Key] 
                                             + _stimBaseline[stim.Key]; //calculate rescaled stim, 
                actualStimOutput[stim.Key] *= RampPercentage / 100f; //apply ramping
            }

            return actualStimOutput;
        }

        private float CalculateRamp()
        {
            if (_rampPercentage < 100f)
                _rampPercentage += _rampIterations;

            return _rampPercentage;
        }
    }
}