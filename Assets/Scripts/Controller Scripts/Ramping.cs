using UnityEngine;

namespace ControllerManager
{
    public class Ramping
    {
        private float _rampDuration;
        private float _frequency;
        private float _rampIterations;
        private float _percentStim; //default zero
        private bool _reset;
        
        public Ramping()
        {
            _rampDuration = PlayerPrefs.GetFloat("Ramp Duration");
            _frequency = 1/Time.fixedDeltaTime;
            _rampIterations = 100f/_rampDuration*_frequency; //how much to ramp per iteration to get to 100% stim output in 1 second

            GameSession.TargetChangeEvent += TargetChanged; //subscribe to targetchangeevent
        }

        public float RampStimulation(float stimulation)
        {
            if (_percentStim != 100f)
                _percentStim += _rampIterations;

            if (_reset)
            {
                _percentStim = 0f;
                _reset = false;
            }

            return stimulation*_percentStim/100; //divide by 100 to convert from percent to decimal
        }

        public void TargetChanged() => _reset = true;

        ~Ramping() //make sure that the event is unsubscribed when object is destroyed to prevent memory leaks
        {
            GameSession.TargetChangeEvent -= TargetChanged;
        }
    }
}