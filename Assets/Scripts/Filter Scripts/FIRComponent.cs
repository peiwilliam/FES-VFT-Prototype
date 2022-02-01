using System.Linq;

namespace FilterManager
{
    public class FIRComponent
    {
        private float[] _x;

        public FIRComponent(int n)
        {
            _x = new float[n];
        }

        public float Solve(float input, float[] a)
        {
            var output = 0.0f;
            
            //we only need the for loop for fir because iir won't ever need to go above 3 terms
            for (var i = _x.Length - 1; i >= 0; i--) 
            {
                if (i == 0)
                    _x[i] = input;
                else
                    _x[i] = _x[i - 1];
            }

            if (_x.Any(value => value == 0.0f)) //if any values in array are 0, just directly set output as the input.
                output = input;
            else
            {
                for (var i = 0; i < _x.Length; i++)
                    output += _x[i] * a[i];
            }

            return output;
        }
    }
}