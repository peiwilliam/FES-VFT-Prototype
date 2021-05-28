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
        private float _m;
        private float _h;
        private float _i;
        private float _kp;
        private float _kd;
        private float _k;
        
        
        private const float _G = 9.81f; //m/s^2

        public Controller()
        {
            //various bio constants
            _kdc = PlayerPrefs.GetFloat("Kd Coefficient");
            _kpc = PlayerPrefs.GetFloat("Kp Coefficient");
            _kc = PlayerPrefs.GetFloat("K Coefficient");
            _height = PlayerPrefs.GetFloat("Height")/100f; //conver to m
            _mass = PlayerPrefs.GetFloat("Mass");
            _ankleMassFraction = PlayerPrefs.GetFloat("Ankle Mass Fraction");
            _comFraction = PlayerPrefs.GetFloat("CoM Height");
            _inertiaCoeff = PlayerPrefs.GetFloat("Inertia Coefficient"); //can make as a parameter in settings
            _m = _mass*_ankleMassFraction;
            _h = _height*_comFraction;
            _i = _inertiaCoeff*_mass*Mathf.Pow(_height, 2);
            _ankleLength = PlayerPrefs.GetFloat("Ankle Fraction")/100f*_height;

            //controller constants
            _kp = _kpc*_m*_G*_height; //active/neural controller
            _kd = _kdc*_m*_G*_height;
            _k = _kc*_m*_G*_height; //mechanical/passive controller


            
            
        }


    }
}