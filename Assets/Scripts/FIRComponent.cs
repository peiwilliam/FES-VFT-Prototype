using System.Collections;
using System.Collections.Generic;
using FilterManager;

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
            return 0.0f;
        }
    }
}