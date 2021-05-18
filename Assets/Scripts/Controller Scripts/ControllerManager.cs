using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ControllerManager
{
    public class Controller
    {
        private float _kdc = 0.5f;
        private float _kpc = 0.5f;
        private float _height = PlayerPrefs.GetFloat("Height");
        private float _mass = PlayerPrefs.GetFloat("Mass");
        private float _ankleMassFraction = PlayerPrefs.GetFloat("Ankle Mass Fraction");
        private float _kp;
        private float _kd;
        private float _m;

        public Controller()
        {
            _kdc = 0.5f;
            _kpc = 0.5f;
            _height = PlayerPrefs.GetFloat("Height");
            _mass = PlayerPrefs.GetFloat("Mass");
            _ankleMassFraction = PlayerPrefs.GetFloat("Ankle Mass Fraction");
            _kp = _kpc*_height;
            _kd = _kdc*_mass;
            _m = _mass*_ankleMassFraction;

        }
    }
}