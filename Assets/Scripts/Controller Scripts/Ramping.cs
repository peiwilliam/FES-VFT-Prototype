using UnityEngine;

namespace ControllerManager
{
    public class Ramping
    {
        private float _rampDuration;
        private float _frequency;
        private float _rampIterations;
        private float _rampPercentage; //default zero
        private bool _reset;
        
        public Ramping()
        {
            _rampDuration = PlayerPrefs.GetFloat("Ramp Duration");
            _frequency = 1/Time.fixedDeltaTime;
            _rampIterations = 100f/_rampDuration*_frequency; //how much to ramp per iteration to get to 100% stim output in 1 second

            GameSession.TargetChangeEvent += TargetChanged; //subscribe to targetchangeevent
        }

        public float CalculateRamp()
        {
            if (_rampPercentage != 100f)
                _rampPercentage += _rampIterations;

            if (_reset)
            {
                _rampPercentage = 0f;
                _reset = false;
            }

            return _rampPercentage; //divide by 100 to convert from percent to decimal
        }

        public void TargetChanged() => _reset = true;

        ~Ramping() //make sure that the event is unsubscribed when object is destroyed to prevent memory leaks
        {
            GameSession.TargetChangeEvent -= TargetChanged;
        }
    }
}