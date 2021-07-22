using System.Collections;
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
        private float _limitFront;
        private float _limitBack;
        private float _limitLeft;
        private float _limitRight;
        private float _m;
        private float _h;
        private float _i;
        private float _kp;
        private float _kd;
        private float _k;
        private float _rpfMax;
        private float _rdfMax;
        private float _lpfMax;
        private float _ldfMax;
        
        private const float _G = 9.81f; //m/s^2

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
            _ankleLength = PlayerPrefs.GetFloat("Ankle Fraction")/100f*_height;
            _lengthOffset = PlayerPrefs.GetFloat("Length Offset");
            _ankleQS = _lengthOffset - _ankleLength;
            _limitFront = PlayerPrefs.GetFloat("Limit of Stability Front");
            _limitBack = PlayerPrefs.GetFloat("Limit of Stability Back");
            _limitLeft = PlayerPrefs.GetFloat("Limit of Stability Left");
            _limitRight = PlayerPrefs.GetFloat("Limit of Stability Right");
            _rpfMax = PlayerPrefs.GetFloat("RPF Max");
            _rdfMax = PlayerPrefs.GetFloat("RDF Max");
            _lpfMax = PlayerPrefs.GetFloat("LPF Max");
            _ldfMax = PlayerPrefs.GetFloat("LDF Max");

            //controller constants
            _kp = _kpc*_m*_G*_height; //active/neural controller kp and kd
            _kd = _kdc*_m*_G*_height; 
            _k = _kc*_m*_G*_height; //mechanical/passive controller
        }

        public void Stimulate(WiiBoardData data, Vector2 targetCoords)
        {
            
        }
    }
}