using System.Linq;

namespace FilterManager
{
    public class IIRComponent
    {
        private float[] _y;

        public IIRComponent(int order)
        {
            _y = new float[order];
        }

        public float Solve(float input, float[] b)
        {
            float output = 0.0f;

            if (_y.Length == 2) //2nd order
            {
                _y[1] = _y[0];
                _y[0] = input;
            }
            else //1st order
            {
                _y[0] = input;
            }

            if (_y.Any(value => value == 0.0f)) //if any values in array are 0, just directly set output as the input.
                output = input;
            else
            {
                for (var i = 0; i < _y.Length; i++)
                {
                    output += _y[i] * b[i];
                }
            }
            
            return output;
        }
    }
}
