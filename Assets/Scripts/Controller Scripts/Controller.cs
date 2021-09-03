using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ControllerManager
{
    public class Controller
    {
        private float _kdc;
        private float _kpc;
        private float _kc;
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
        private List<float> _limits;
        private List<float> _stimMax;
        private List<float> _coms;
        private List<float> _angles;
        private Dictionary<string, List<float>> _biasCoeffs;
        
        private const float _G = 9.81f; //m/s^2
        private const float _XWidth = 433; // mm
        private const float _YLength = 228; // mm

        public Controller()
        {
            //define various constants
            _kdc = PlayerPrefs.GetFloat("Kd Coefficient");
            _kpc = PlayerPrefs.GetFloat("Kp Coefficient");
            _kc = PlayerPrefs.GetFloat("K Coefficient");
            _height = PlayerPrefs.GetFloat("Height")/100f; //convert from cm to m
            _mass = PlayerPrefs.GetFloat("Mass");
            _ankleMassFraction = PlayerPrefs.GetFloat("Ankle Mass Fraction");
            _comFraction = PlayerPrefs.GetFloat("CoM Height");
            _inertiaCoeff = PlayerPrefs.GetFloat("Inertia Coefficient"); //can make as a parameter in settings
            _m = _mass*_ankleMassFraction; //mass without foot
            _hCOM = _height*_comFraction; //height of COM
            _i = _inertiaCoeff*_mass*Mathf.Pow(_height, 2); //inertia
            _ankleLength = PlayerPrefs.GetFloat("Ankle Fraction")*_height;
            _lengthOffset = PlayerPrefs.GetFloat("Length Offset")*_YLength/1000f; //convert from percentage to length and mm to m
            _ankleQS = _lengthOffset - _ankleLength;

            _limits = new List<float>() //front, back, left, right, converted to m
            {
                PlayerPrefs.GetFloat("Limit of Stability Front")*_YLength/1000f,
                PlayerPrefs.GetFloat("Limit of Stability Back")*_YLength/1000f,
                PlayerPrefs.GetFloat("Limit of Stability Left")*_XWidth/1000f,
                PlayerPrefs.GetFloat("Limit of Stability Right")*_XWidth/1000f
            };

            _stimMax = new List<float>() //RPF, RDF, LPF, LDF
            {
                PlayerPrefs.GetFloat("RPF Max"),
                PlayerPrefs.GetFloat("RDF Max"),
                PlayerPrefs.GetFloat("LPF Max"),
                PlayerPrefs.GetFloat("LDF Max")
            };

            _biasCoeffs = new Dictionary<string, List<float>>() //obtained from fitting
            {
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
                },
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
                }
            };

            //controller constants
            _kp = _kpc*_m*_G*_height; //active/neural controller kp and kd
            _kd = _kdc*_m*_G*_height; 
            _k = _kc*_m*_G*_height; //mechanical/passive controller

            _coms = new List<float>(5); //mechanical controller requires 0-2 and neural controller requires 2-4 for derivative
            _angles = new List<float>(5);
        }

        public void Stimulate(WiiBoardData data, Vector2 targetCoords)
        {
            //shift everything to the perspective of the ankles
            var shiftedCOMy = data.fCopY + _ankleQS;
            var targetY = targetCoords.y + _ankleQS;
            var losF = _limits[0] + _ankleQS;
            var losB = _limits[1] - _ankleQS;

            // calculating mechanical torques
            var qsTorque = _m*_G*_ankleQS;
            var losFTorque = _m*_G*_limits[0];
            var losBTorque = _m*_G*_limits[1];

            //angle calculations
            var targetVertAng = Mathf.Atan2(targetY, _hCOM);
            var comVertAng = Mathf.Atan2(shiftedCOMy, _hCOM);
            var qsVertAng = Mathf.Atan2(_ankleQS, _hCOM);
            var angErr = targetVertAng - comVertAng;

            //first need to adjust the stored COM values as new values are added from cursor
            for (var i = _coms.Count - 1; i >= 0; i--)
            {
                if (i == 0)
                    _coms[i] = shiftedCOMy;
                else
                    _coms[i] = _coms[i - 1];
            }

            //controller calculations
            var neuralTorque = NeuralController(angErr); //calculate neural torque from controller output
            var mechanicalTorque = MechanicalController(comVertAng); //calculate passive torque from controller output
            var slopes = Slopes(qsTorque, losFTorque, losBTorque); //calculate controller slopes
            var stimulationOutput = UnbiasedStimulationOutput(slopes, neuralTorque, mechanicalTorque, qsTorque, comVertAng, qsVertAng); //calculate unbiased output
            var neuralBiases = CalculateNeuralBiases(data, targetCoords); //calculate neural biases
            var mechBiases = CalculateMechBiases(data, shiftedCOMy); //calculate mech biases

            
        }

        private float NeuralController(float error)
        {
            float derivativeError;

            if (_coms.GetRange(2, 3).Any(v => v == 0.0f)) //make sure that the vector of com is filled (ie. nothing is zero)
                derivativeError = 0.0f;
            else
                derivativeError = CalculateDerivative(_coms.GetRange(2, 3)); //two point delay on the neural controller
            
            return _kp*error + _kd*derivativeError;
        }

        private float MechanicalController(float verticalCOMAng)
        {
            float velocity;

            if (_coms.GetRange(0, 3).Any(v => v == 0.0f)) //make sure that the vector of com is filled (ie. nothing is zero)
                velocity = 0.0f;
            else
                velocity = CalculateDerivative(_coms.GetRange(0, 3));

            return _kc*verticalCOMAng + 5.0f*velocity;
        }

        private Dictionary<string, List<float>> Slopes(float qsTorque, float losBTorque, float losFTorque) //calculate slopes for neural and mech controllers
        {
            var slopes = new Dictionary<string, List<float>>()
            {
                ["Mech"] = new List<float>(4), //follows order of RPF, RDF, LPF, LDF
                ["Neural"] = new List<float>(4)
            };

            //mech PF
            slopes["Mech"][0] = _stimMax[0] / (losFTorque / 2); //RPF
            slopes["Mech"][2] = _stimMax[2] / (losFTorque / 2); //LPF
            //mech DF
            slopes["Mech"][1] = _stimMax[1] / ((losBTorque - qsTorque) / 2); //RDF
            slopes["Mech"][3] = _stimMax[3] / ((losBTorque - qsTorque) / 2); //LDF
            //neural PF
            slopes["Neural"][0] = _stimMax[0] / ((losFTorque - losBTorque)/ 2); //RPF
            slopes["Neural"][2] = _stimMax[2] / ((losFTorque - losBTorque) / 2); //LPF
            //neural DF
            slopes["Neural"][1] = _stimMax[1] / ((losBTorque - losFTorque) / 2); //RDF
            slopes["Neural"][3] = _stimMax[3] / ((losBTorque - losFTorque) / 2); //LDF

            return slopes;
        }

        public float CalculateDerivative(List<float> comsVector) => (3.0f*comsVector[0] - 4.0f*comsVector[1] + comsVector[2])/2*Time.fixedDeltaTime;

        private Dictionary<string, List<float>> UnbiasedStimulationOutput(Dictionary<string, List<float>> slopes, float neuralTorque, float mechanicalTorque, float qsTorque, float comVertAng, float qsVertAng)
        {
            var stimulation = new Dictionary<string, List<float>>
            {
                ["Mech"] = new List<float>(4), //default zero, order is RPF, RDF, LPF, LDF
                ["Neural"] = new List<float>(4)
            };

            if (0.5f*mechanicalTorque > 0) //only calculate stim if 0.5*mechanicalTorque > 0
            {
                stimulation["Mech"][0] = slopes["Mech"][0]*0.5f*mechanicalTorque;
                stimulation["Mech"][2] = slopes["Mech"][2]*0.5f*mechanicalTorque;
            }
            if (0.5f*qsTorque > 0.5f*mechanicalTorque) //only calculate stim if 0.5f*qsTorque > 0.5f*mechanicalTorque > 0
            {
                stimulation["Mech"][1] = slopes["Mech"][1]*0.5f*mechanicalTorque;
                stimulation["Mech"][3] = slopes["Mech"][3]*0.5f*mechanicalTorque;
            }
            if (comVertAng > 0) //only calculate stim if comVertAng > 0
            {
                stimulation["Neural"][0] = slopes["Neural"][0]*0.5f*neuralTorque;
                stimulation["Neural"][2] = slopes["Neural"][2]*0.5f*neuralTorque;
            }
            if (comVertAng < qsVertAng) //only calculate stim if comVertAng < qsVertAng
            {
                stimulation["Neural"][1] = slopes["Neural"][1]*0.5f*neuralTorque;
                stimulation["Neural"][3] = slopes["Neural"][3]*0.5f*neuralTorque;
            }

            return stimulation;
        }

        private Dictionary<string, float> CalculateNeuralBiases(WiiBoardData data, Vector2 targetCoords)
        {
            var biases = new Dictionary<string, float>();
            var x = data.fCopX - targetCoords.x;
            var y = data.fCopY - targetCoords.y;
            var biasAng = -Mathf.Atan2(y, x);

            BiasFunction(biases, biasAng);

            return biases;
        }

        private Dictionary<string, float> CalculateMechBiases(WiiBoardData data, float shiftedCOMy)
        {
            var biases = new Dictionary<string, float>();
            var biasAng = -Mathf.Atan2(shiftedCOMy, data.fCopX);

            BiasFunction(biases, biasAng);

            return biases;
        }

        private void BiasFunction(Dictionary<string, float> biases, float biasAng)
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
    }
}