using System.Collections.Generic;
using UnityEngine;

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
        private float _h;
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
            _h = _height*_comFraction; //height of COM
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

            _coms = new List<float>(3);
            _angles = new List<float>(3);
        }

        public void Stimulate(WiiBoardData data, Vector2 targetCoords)
        {
            //shift everything to the perspective of the ankles
            var shiftedCOMy = data.fCopY + _ankleQS;
            var targetY = targetCoords.y + _ankleQS;
            _limits[0] += _ankleQS;
            _limits[1] += _ankleQS;

            // calculating mechanical torques
            var qsTorque = _m*_G*_ankleQS;
            var losfTorque = _m*_G*_limits[0];
            var losbTorque = _m*_G*_limits[1];

            //angle calculations
            var targetVertAng = Mathf.Atan2(targetY, _h);
            var comvertAng = Mathf.Atan2(shiftedCOMy, _h);
            var qsVertAng = Mathf.Atan2(_ankleQS, _h);
            var angErr = qsVertAng - comvertAng;
            
        }

        public void NeuralController()
        {

        }

        public void MechanicalController()
        {

        }

        public float CalculateDerivative(float input)
        {
            _coms[2] = _coms[1];
            _coms[1] = _coms[0];
            _coms[0] = input;
            
            var derivative = (3.0f*_coms[0] - 4.0f*_coms[1] + _coms[2])/2*Time.fixedDeltaTime;

            return derivative;
        }

        public Dictionary<string, float> CalculateBiases(WiiBoardData data, float shiftedCOMy)
        {
            var biases = new Dictionary<string, float>();
            var biasAng = -Mathf.Atan2(data.fCopX, shiftedCOMy);

            foreach (var item in _biasCoeffs)
            {
                var bias = 0f;

                if (item.Key == "LPF" || item.Key == "RPF")
                {
                    for (var i = 7; i >= 0; i--)
                        bias += item.Value[i]*Mathf.Pow(biasAng, i);
                }
                else
                {
                    for (var i = 5; i >= 0; i--)
                        bias += item.Value[i]*Mathf.Pow(biasAng, i);
                }
                   
                biases.Add(item.Key, bias);
            }

            return biases;
        }
    }
}