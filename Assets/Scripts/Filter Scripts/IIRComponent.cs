using System.Linq;

namespace FilterManager
{
    /// <summary>
    /// This class is responsible for storing the previous data points for the infinite impulse component of the filter and also
    /// solving for the infinite impulse component of the filter.
    /// </summary>
    public class IIRComponent
    {
        private float[] _y;

        /// <summary>
        /// Create the finite impulse response component object with a given size stord previous points.
        /// </summary>
        public IIRComponent(int n)
        {
            _y = new float[n];
        }

        /// <summary>
        /// Compute the infinite impulse response component of the filter.
        /// </summary>
        public float Solve(float input, float[] b)
        {
            var output = input; //if any values in array are 0, just directly set output as the input.

            if (!_y.Any(value => value == 0.0f)) //once array is filled, start filtering
            {
                for (var i = 0; i < _y.Length; i++)
                    output += _y[i] * b[i];
            }
            
            if (_y.Length == 2) //2nd order
            {
                _y[1] = _y[0];
                _y[0] = output;
            }
            else //1st order
                _y[0] = output;
            
            return output;
        }
    }
}
