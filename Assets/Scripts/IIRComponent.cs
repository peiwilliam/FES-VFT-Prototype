using System.Linq;

namespace FilterManager
{
    public class IIRComponent
    {
        private float[] _y;

        public IIRComponent(int n)
        {
            _y = new float[n];
        }

        public float Solve(float input, float[] b)
        {
            var output = input; //if any values in array are 0, just directly set output as the input.

            if (!_y.Any(value => value == 0.0f)) //once array is filled, start filtering
            {
                for (var i = 0; i < _y.Length; i++)
                {
                    output += _y[i] * b[i];
                }
            }

            if (_y.Length == 2) //2nd order
            {
                _y[1] = _y[0];
                _y[0] = output;
            }
            else //1st order
            {
                _y[0] = output;
            }
            
            return output;
        }
    }
}
