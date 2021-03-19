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

            for (var i = 0; i < _y.Length; i++)
            {
                if (i < _y.Length)
                    _y[i + 1] = _y[i];
                else
                {
                    if (_y.Any(value => value == 0))
                        output = input;
                }
            }

            return output;
        }
    }
}
