using System.Linq;

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

            for (var i = 0; i < _x.Length; i++)
            {
                if (i < _x.Length - 1)
                    _x[i + 1] = _x[i];
                else
                    _x[i] = input;
                
                if (_x.Any(value => value == 0.0f))
                    output = input;
            }

            return output;
        }
    }
}