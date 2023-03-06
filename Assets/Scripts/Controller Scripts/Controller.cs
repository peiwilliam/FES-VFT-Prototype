/*
Initial code written by William Pei 2022 for his Master's thesis in MASL
*/

using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ControllerManager
{
    /// <summary>
    /// This class contains all of the logic involved in calculating the stimulation required for the left and right plantar and
    /// dorsi flexors.
    /// </summary>
    public class Controller
    {
        private bool _foundWiiBoard; //this is positive when the wii board is found and uses the board for the cursor
        private bool _isTargetGame; //is the current game target game?
        private string _sceneName; //current scene name
        //neural controller
        private float _kdc; //factor for calculating Kd for the PD controller
        private float _kpc; //factor for calculating Kp for the PD controller
        //mechanical controller
        private float _kc; //factor for calculating K for the mechanical controller
        //initial ramping so that the stimulation slowly ramp up to the target stimulation
        private float _rampIterations; //how many iterations to ramp up to target stimulation
        private float _rampPercentage; //the percentage at the current stimulation for ramping
        //phyiological constants
        private float _ankleDisplacement; //to shift everything to the reference point of the ankle (ie. ankle is at y = 0)
        private float _lengthOffset; //the AP displacement for the participant in the natural standing (calculated from qs assessment)
        private float _m; //participant mass
        private float _hCOM; //height of the the participant's com
        private float _kp; //Kp for neural PD controller
        private float _kd; //Kd for neural PD controller
        private float _k; //K for mechanical controller
        private int _neuralCounter; //counter for keeping track of when the derivative should be turned on for the controller, always 0 for ellipse
        //controller parameters
        private float _qsTorque; //torque experienced by the body during quiet standing
        private float _losFTorque; //torque experienced by the body at their front limit
        private float _losBTorque; //torque experienced by the body at their back limit
        private float _qsVertAng; //angle of the body during quiet standing
        private float _heelPosition; //measured manually from centre of board to bottom of indicated heel location
        private Vector2 _previousTarget; //location of the previous target, used for keeping a counter for neural controller
        private float[] _limits; //used for storing the LOS limits
        private List<float> _comAngles; //used for storing the com angles, in ankle reference frame
        private List<float> _comAngleErrors; //used for storing the comangle errors, in ankle reference frame
        private Dictionary<string, float> _stimMax; //stores the maximum stimulation the participant could tolerate
        private Dictionary<string, float> _motorThresh; //stores the motor threshold of the participant
        private Dictionary<string, float> _qsMechStim; //used to calculate the baseline for pf stimulation
        private Dictionary<string, float> _stimBaseline; //stores the baseline stim values
        private Dictionary<string, float[]> _biasCoeffs; //stores the bias coefficients to calculate the bias of the stimulation
        //stim filtering
        private MedianFilter _lpfFilter;
        private MedianFilter _ldfFilter;
        private MedianFilter _rpfFilter;
        private MedianFilter _rdfFilter;

        //constants
        private const float _G = 9.81f; //m/s^2
        private const float _XWidth = 433f; // mm
        private const float _YLength = 238f; // mm
        private const float _MaxX = 2f*5f*16f/9f; //2*height*aspect ratio, ideally not hard coded but will remain this way for now
        private const float _MaxY = 5f*2f; //2*camera size

        //properties
        /// <summary>
        /// Property for getting stimulation ramp scaling percentage, only really used for the ouput csv file.
        /// </summary>
        public float RampPercentage { get; private set; }
        /// <summary>
        /// Property for getting the calculated neural torque, only used for the output csv file.
        /// </summary>
        public float NeuralTorque { get; private set; }
        /// <summary>
        /// Property for getting the calculated mechanical torque, only used for the output csv file.
        /// </summary>
        public float MechanicalTorque { get; private set; }
        /// <summary>
        /// Property for getting the calculated standing angles in ankle perspective, only used for the output csv file.
        /// </summary>
        public List<float> Angles { get; private set; }
        /// <summary>
        /// Property for getting the calculated shifted cop and target positions in real coordinates and shifted to ankle perpective.
        /// Only used for the output csv file.
        /// </summary>
        public List<float> ShiftedPos { get; private set; }
        /// <summary>
        /// Property for getting the current medial-lateral angles of the person's standing posture in the mechanical and 
        /// neural controllers.
        /// </summary>
        public List<float> MlAngles { get; private set; }
        /// <summary>
        /// Property for getting various calculated constants for the controller: Kp, Kd, K, LOS F, LOS B, QS torque, LOS F torque,
        /// and LOS B torque.
        /// </summary>
        public Dictionary<string, float> CalculatedConstants { get; private set; }
        /// <summary>
        /// Property for getting the intercept values calculated and used for determining the mechanical stimulation.
        /// </summary>
        public Dictionary<string, float> Intercepts { get; private set; }
        /// <summary>
        /// Property for getting the stimulation baseline levels.
        /// </summary>
        public Dictionary<string, Dictionary<string, float>> CalculatedStimBaselines { get; private set; }
        /// <summary>
        /// Property for getting the slopes used to calculate the stimulation levels based on the calculated torque.
        /// </summary>
        public Dictionary<string, Dictionary<string, float>> Slopes { get; private set; }
        /// <summary>
        /// Property for getting the biases useds to adjust the stimulation intensity depending on the individual's medial lateral
        /// leaning angle
        /// </summary>
        public Dictionary<string, Dictionary<string, float>> Biases { get; private set; }

        /// <summary>
        /// Constructor to create an instance of the Controller class.
        /// </summary>
        public Controller(Cursor cursor, bool foundWiiBoard)
        {
            //Controller coefficients needed to calculate the controller constants
            _kdc = PlayerPrefs.GetFloat("Kd Coefficient");
            _kpc = PlayerPrefs.GetFloat("Kp Coefficient");
            _kc = PlayerPrefs.GetFloat("K Coefficient");
            //Patient parameters
            var height = PlayerPrefs.GetInt("Height")/100f; //Get the height and convert from cm to m
            var mass = PlayerPrefs.GetFloat("Mass"); //Get the mass in kg
            var ankleMassFraction = PlayerPrefs.GetFloat("Ankle Mass Fraction"); //Get the mass fraction of the body without the foot
            var comFraction = PlayerPrefs.GetFloat("CoM Height"); //Get the approximate height of the CoM
            var inertiaCoeff = PlayerPrefs.GetFloat("Inertia Coefficient"); //Inertia coefficient for calculating the inertia of the body, currently unused
            var ankleLength = PlayerPrefs.GetFloat("Ankle Fraction")*height/100f; //Calculate the ankle length as a fraction of the height converted from cm to m
            _m = mass*ankleMassFraction; //Mass without foot
            _hCOM = height*comFraction; //Height of COM
            _lengthOffset = PlayerPrefs.GetFloat("Length Offset")/100f; //Length offset calculated from QS assessment converted from percent to fraction. This is negative as well!!!!
            _heelPosition = PlayerPrefs.GetFloat("Heel Position"); //approximate position of the heel in m
            _ankleDisplacement = _heelPosition - ankleLength; //Calculated to shift everything to the ankle perspective
            _foundWiiBoard = foundWiiBoard; //Boolean for if a wii board object was found. If found, we use the wii board, if not we use the cursor
            _sceneName = SceneManager.GetActiveScene().name; //Get the name fo the current active scene for reference later
            _isTargetGame = _sceneName == "Target" ? true : false; //Boolean for determining if certain calculations are done in the controller
            _rampIterations = 100f/(PlayerPrefs.GetFloat("Ramp Duration")/Time.fixedDeltaTime); //How much to ramp per iteration to get to 100% stim output in 1 second
            _lpfFilter = new MedianFilter(21);
            _ldfFilter = new MedianFilter(21);
            _rpfFilter = new MedianFilter(21);
            _rdfFilter = new MedianFilter(21);

            MlAngles = new List<float>() {0f, 0f}; //Initialize MlAngles property

            //Need to convert from percent to fraction
            //LOS is in qs frame of reference but need to remove shift that was used to centre cursor for LOS
            _limits = new float[] //front, back, left, right
            {
                PlayerPrefs.GetFloat("Limit of Stability Front")/100f - cursor.LOSShift*2f/_MaxY,
                PlayerPrefs.GetFloat("Limit of Stability Back")/100f - cursor.LOSShift*2f/_MaxY,
                PlayerPrefs.GetFloat("Limit of Stability Left")/100f,
                PlayerPrefs.GetFloat("Limit of Stability Right")/100f
            };

            var myndSearchToStepsConv = 1f;
            //If stimulator being used is MyndSearch, we want to convert the mA to the 0-63 steps that the controller expects
            //We don't need to do for compex because each step is 1 mA
            if (PlayerPrefs.GetInt("MyndSearch", 1) == 1) 
            {
                myndSearchToStepsConv = 64f/40f;
            }

            _motorThresh = new Dictionary<string, float>()
            {
                ["RPF"] = PlayerPrefs.GetFloat("RPF Motor Threshold")*myndSearchToStepsConv,
                ["RDF"] = PlayerPrefs.GetFloat("RDF Motor Threshold")*myndSearchToStepsConv,
                ["LPF"] = PlayerPrefs.GetFloat("LPF Motor Threshold")*myndSearchToStepsConv,
                ["LDF"] = PlayerPrefs.GetFloat("LDF Motor Threshold")*myndSearchToStepsConv
            };

            _stimMax = new Dictionary<string, float>()
            {
                ["RPF"] = PlayerPrefs.GetFloat("RPF Max")*myndSearchToStepsConv,
                ["RDF"] = PlayerPrefs.GetFloat("RDF Max")*myndSearchToStepsConv,
                ["LPF"] = PlayerPrefs.GetFloat("LPF Max")*myndSearchToStepsConv,
                ["LDF"] = PlayerPrefs.GetFloat("LDF Max")*myndSearchToStepsConv
            };

            _biasCoeffs = new Dictionary<string, float[]>() //Bias coefficients obtained from fitting.
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

            //Controller constants
            _kp = _kpc*_m*_G*_hCOM; //Active/neural controller kp and kd
            _kd = _kdc*_m*_G*_hCOM; 
            _k = _kc*_m*_G*_hCOM; //Mechanical/passive controller

            _comAngles = new List<float> {0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f}; //For mechanical controller only, mechanical uses 0-2 and absolute neural uses 5-7
            _comAngleErrors = new List<float> {0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f}; //Neural controller requires 5-7 for derivative, delay of 100ms

            //Shift LOS to ankle reference frame for calculating torque
            var losF = (_limits[0] + _lengthOffset)*_YLength/1000f/2f + _ankleDisplacement; //Adjust front and back limits to ankle reference frame
            var losB = _ankleDisplacement + (_lengthOffset - _limits[1])*_YLength/1000f/2f; //Since torque is negative, sign of losB should also be negative

            //Calculating mechanical torques
            _qsTorque = _m*_G*(_ankleDisplacement + _lengthOffset*_YLength/1000f/2f); //QS torque is also adjusted to ankle reference frame.
            _losFTorque = _m*_G*losF;
            _losBTorque = _m*_G*losB;

            Slopes = GetSlopes();
            Intercepts = new Dictionary<string, float> {["RDF"] = -Slopes["Mech"]["RDF"]*_qsTorque, 
                                                         ["LDF"] = -Slopes["Mech"]["LDF"]*_qsTorque};
            _qsVertAng = Mathf.Atan2(_ankleDisplacement + _lengthOffset*_YLength/1000f/2f, _hCOM);

            _qsMechStim = UnbiasedStimulationOutput(0, _qsTorque, 0)["Mech"]; //Calculate just the mechanical portion of stimulation at QS for PF rescaling
            _stimBaseline = new Dictionary<string, float>();

            foreach (var baseline in _qsMechStim)
            {
                if (baseline.Key.Contains("PF")) //Baseline = (QS stim - MT stim)/(QS stim/max stim - 1)
                    _stimBaseline[baseline.Key] = (_qsMechStim[baseline.Key] - _motorThresh[baseline.Key])
                                                  /(_qsMechStim[baseline.Key]/_stimMax[baseline.Key] - 1);
                else
                    _stimBaseline[baseline.Key] = _motorThresh[baseline.Key]; //Baseline is just motor threshold
            }

            CalculatedConstants = new Dictionary<string, float>()
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

        /// <summary>
        /// Method for calculating the current stimulation required based on the current standing standing posture.
        /// Returns a dictionary with two dictionaries in it: Unbiased contains the raw stimulation values without any adjustments 
        /// for the left and right plantar and dorsi flexors and Actual contains the actual stimulation values for the left and right
        /// plantar and dorsi flexors to be sent to the Arduino.
        /// </summary>
        public Dictionary<string, Dictionary<string, float>> Stimulate(WiiBoardData data, Vector2 targetCoords)
        {
            //Shift target and com then calculate the respective vertical angles
            var (shiftedComY, comX, targetCoordsShifted) = ShiftComAndTarget(data, targetCoords);
            var (targetVertAng, comVertAng) = GetAngles(shiftedComY, targetCoordsShifted);
            IncrementNeuralCounter(targetCoordsShifted);

            //Controller calculations
            NeuralTorque = NeuralDifferentialController(); //Calculate neural torque from controller output
            MechanicalTorque = MechanicalController() + NeuralAbsoluteController(); //Calculate passive torque from controller output
            var rawStimOutput = UnbiasedStimulationOutput(NeuralTorque, MechanicalTorque, comVertAng); //Calculate unbiased output
            var neuralBiases = CalculateNeuralBiases(comX, shiftedComY, targetCoordsShifted); //Calculate neural biases
            var mechBiases = CalculateMechBiases(comX, shiftedComY); //Calculate mechanical biases
            var limitedStim = AdjustedCombinedStimulation(neuralBiases, mechBiases, rawStimOutput);
            var actualStimOutput = RemoveStimOutliers(CheckLimits(RescaleStimulation(limitedStim))); //Rescale the stimulation so that stimulation is contained between the baseline and maximums, and then filter to remove outliers
            var unbiasedStimOutput = new Dictionary<string, float>(); //Want to get unbiased stimulation as well for documentation in data file

            //properties for store data and write to csv
            Biases = new Dictionary<string, Dictionary<string, float>>()
            {
                ["Neural"] = neuralBiases,
                ["Mech"] = mechBiases
            };
            ShiftedPos = new List<float>() { comX, shiftedComY, targetCoordsShifted.x, targetCoordsShifted.y };
            Angles = new List<float>() { targetVertAng, comVertAng, _comAngleErrors[5] }; //only start saving the errors once there is a value in the 5th spots of the vector

            foreach (var muscle in _stimMax) //_stimMax used for iteration and adding together total unbiased stimulation output
                unbiasedStimOutput[muscle.Key] = rawStimOutput["Mech"][muscle.Key] + rawStimOutput["Neural"][muscle.Key];

            return new Dictionary<string, Dictionary<string, float>>() { ["Unbiased"] = unbiasedStimOutput, ["Actual"] = actualStimOutput };
        }
        
        //changes from game coordinates from game coordintaes to real cooridnates and shifts to the ankle perspective
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

        //gets the standing angle of the person, target and angle error between target and person
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

        //neural counter used to determine when the derivative error should be used for the neural controller
        //counter is iterated per fixed update and si reset when a new target is chosen
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

        private float NeuralDifferentialController() //calculate torque based on neural command, uses angle errors
        {
            var derivativeError = 0.0f;
            
            if ((_neuralCounter >= 7 || _isTargetGame) && !_comAngleErrors.GetRange(5, 3).Any(v => v == 0.0f)) //7 correspond to delay of 140ms, original labview delay was 150ms  
                derivativeError = CalculateDerivative(_comAngleErrors.GetRange(5, 3));
            
            return _kp*_comAngleErrors.GetRange(5, 3)[0] + _kd*derivativeError; //only p portion when derivative error vec isn't filled
        }

        private float NeuralAbsoluteController() //calculate torque based on neural command, uses actual angle
        {
            var derivativeError = 0.0f;

            if (!_comAngles.GetRange(5, 3).Any(v => v == 0.0f)) //7 correspond to delay of 140ms, original labview delay was 150ms  
                derivativeError = CalculateDerivative(_comAngles.GetRange(5, 3));

            return _kp*_comAngles.GetRange(5, 3)[0] + _kd*derivativeError;
        }

        private float MechanicalController() //calculate torque based on mechanical properties, uses actual angle
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

        //calculates the derivative, uses the backward derivative formula
        private float CalculateDerivative(List<float> comsVector) => (3.0f*comsVector[0] - 4.0f*comsVector[1] + comsVector[2])/(2*Time.fixedDeltaTime);

        //calculate raw stimulation output based on calculated torques from the controller
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

        //calculates the bias for the neural component
        private Dictionary<string, float> CalculateNeuralBiases(float comX, float shiftedCOMy, Vector2 shiftedTargetCoords) //calculate ML bias for neural torque
        {
            var x = shiftedTargetCoords.x - comX;
            var y = shiftedTargetCoords.y - shiftedCOMy;
            MlAngles[0] = Mathf.Atan2(x, y);
            var biases = BiasFunction(MlAngles[0]);

            return biases;
        }

        //calcultes the bias for the mechanical component
        private Dictionary<string, float> CalculateMechBiases(float comX, float shiftedCOMy) //calculate ML bias for mechanical torque
        {
            MlAngles[1] = Mathf.Atan2(comX, shiftedCOMy);
            var biases = BiasFunction(MlAngles[1]);

            return biases;
        }

        //this method calcualtes the bias given an angle
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

        //takes the raw stimulation and adjusts it based on biases and combines the mechanical and neural components
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

        //double checks that the stimulation is between 0 and the maximum tolerable stimulation
        private Dictionary<string, float> CheckLimits(Dictionary<string, float> adjustedStimOutput) //makes sure stim doesn't go above max and below 0
        {
            var trueStimOutput = new Dictionary<string, float>();

            foreach (var stim in adjustedStimOutput) //make sure stimulation never goes above max and never goes below zero
                trueStimOutput[stim.Key] = Mathf.Min(Mathf.Max(0f, stim.Value), _stimMax[stim.Key]);

            return trueStimOutput;
        }
        
        //rescales the stimulation to be between the baseline and maximum tolerable stimulation
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

        //applies a moving median filter to the final calculated stimulation to remove any stim outliers caused by sudden velocity
        //increases in the gravity compensation part of the controller
        private Dictionary<string, float> RemoveStimOutliers(Dictionary<string, float> unFiltStim)
        {
            return new Dictionary<string, float>()
            {
                ["LPF"] = _lpfFilter.Solve(unFiltStim["LPF"]),
                ["LDF"] = _ldfFilter.Solve(unFiltStim["LDF"]),
                ["RPF"] = _rpfFilter.Solve(unFiltStim["RPF"]),
                ["RDF"] = _rdfFilter.Solve(unFiltStim["RDF"])
            };
        }

        //calculates the initial ramp of the stimulation, ensures that the stimulation isn't too sudden at the beginning
        private float CalculateRamp()
        {
            if (_rampPercentage < 100f)
                _rampPercentage += _rampIterations;

            return _rampPercentage;
        }
    }
}