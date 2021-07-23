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
            _ankleLength = PlayerPrefs.GetFloat("Ankle Fraction")/100f*_height; //convert to m
            _lengthOffset = PlayerPrefs.GetFloat("Length Offset")*_YLength/1000f; //convert from percentage to length and mm to m
            _ankleQS = _lengthOffset - _ankleLength;

            _limits = new List<float>() //front, back, left, right
            {
                PlayerPrefs.GetFloat("Limit of Stability Front"),
                PlayerPrefs.GetFloat("Limit of Stability Back"),
                PlayerPrefs.GetFloat("Limit of Stability Left"),
                PlayerPrefs.GetFloat("Limit of Stability Right")
            };

            _stimMax = new List<float>() //RPF, RDF, LPF, LDF
            {
                PlayerPrefs.GetFloat("RPF Max"),
                PlayerPrefs.GetFloat("RDF Max"),
                PlayerPrefs.GetFloat("LPF Max"),
                PlayerPrefs.GetFloat("LDF Max")
            };

            //controller constants
            _kp = _kpc*_m*_G*_height; //active/neural controller kp and kd
            _kd = _kdc*_m*_G*_height; 
            _k = _kc*_m*_G*_height; //mechanical/passive controller
        }

        public void Stimulate(WiiBoardData data, Vector2 targetCoords)
        {
            var com = data.fCopY + _ankleQS;
            var targetY = targetCoords.y + _ankleQS;
            _limits[0] += _ankleQS;
            _limits[1] += _ankleQS;
            var qsTorque = _m*_G*_ankleQS;
        }
    }
}