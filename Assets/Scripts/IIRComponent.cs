using System.Collections;
using System.Collections.Generic;
using FilterManager;

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
            return 0.0f;
        }
    }
}
