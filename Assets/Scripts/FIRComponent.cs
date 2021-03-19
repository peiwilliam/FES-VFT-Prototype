using System.Linq;
using System;

namespace FilterManager
{
    public class FIRComponent
    {
        private float[] _x;

        public FIRComponent(int order)
        {
            _x = new float[order];
        }

        public float Solve(float input, float[] a)
        {
            float output = 0.0f;

            if (_x.Length == 3) //2nd order
            {
                _x[2] = _x[1];
                _x[1] = _x[0];
                _x[0] = input;
            }
            else //1st order
            {
                _x[1] = _x[0];
                _x[0] = input;
            }

            if (_x.Any(value => value == 0.0f)) //if any values in array are 0, just directly set output as the input.
                output = input;
            else
            {
                for (var i = 0; i < _x.Length; i++)
                {
                    output += _x[i] * a[i];
                }
            }
            
            return output;
        }
    }
}