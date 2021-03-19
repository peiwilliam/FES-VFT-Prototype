using UnityEngine;

namespace FilterManager
{
    public class FilterStage
    {
        public float[] A { get; private set; }
        public float[] B { get; private set; } // only care about the first order term since all other terms have coeff of one
        public FIRComponent FIRComponent2nd { get; private set;} //2nd order
        public IIRComponent IIRComponent2nd { get; private set;}
        public FIRComponent FIRComponent1st { get; private set;} //1st order
        public IIRComponent IIRComponent1st { get; private set;}
        
        public FilterStage(float zeta, float wc, int order, float sampleHz, bool high, bool firstStage = false) //first stage only for odd orders
        {
            if (!high) //low pass
                LowPassFilter(zeta, wc, order, sampleHz, firstStage);
            else //high pass
                HighPassFilter(zeta, wc, order, sampleHz, firstStage);

            if (order > 1) 
            {
                FIRComponent2nd = new FIRComponent(3);
                IIRComponent2nd = new IIRComponent(2); //one less than fir because we're solving for filtered point
            }

            if (order % 2 != 0)
            {
                FIRComponent1st = new FIRComponent(2);
                IIRComponent1st = new IIRComponent(1); //one less than fir because we're solving for filtered point
            }
        }

        private void LowPassFilter(float zeta, float wc, int order, float sampleHz, bool firstStage)
        {
            if (order % 2 != 0 && firstStage == true) //first order filter for odd filters
            {
                var b0 = wc + 2.0f * sampleHz;

                A = new float[]
                {
                    wc / b0,
                    wc / b0
                };

                B = new float[]
                {
                    (wc - 2.0f * sampleHz) / b0
                };
            }
            else //2nd order filter
            {
                var b0 = 4.0f * Mathf.Pow(sampleHz, 2) + 4.0f * sampleHz * zeta * wc + Mathf.Pow(wc, 2);

                A = new float[]
                {
                    Mathf.Pow(wc, 2) / b0,
                    2.0f * Mathf.Pow(wc, 2) / b0,
                    Mathf.Pow(wc, 2) / b0
                };

                B = new float[]
                {
                    (2.0f * Mathf.Pow(wc, 2) - 8.0f * sampleHz * sampleHz) / b0,
                    (4.0f * sampleHz * sampleHz - 4.0f * sampleHz * zeta * wc + Mathf.Pow(wc, 2)) / b0
                };
            }
        }

        private void HighPassFilter(float zeta, float wc, int order, float sampleHz, bool firstStage)
        {
            if (order % 2 != 0 && firstStage == true) //first order filter for odd filters
            {
                var b0 = 1.0f + 2.0f * sampleHz * wc;
                
                A = new float[]
                {
                    2.0f * sampleHz * wc / b0,
                    2.0f * sampleHz * wc / b0
                };

                B = new float[]
                {
                    (1.0f - 2.0f * sampleHz * wc) / b0,
                };
            }
            else //2nd order filter
            {
                var b0 = 4.0f * Mathf.Pow(sampleHz, 2) + 4.0f * sampleHz * zeta / wc + 1 / (Mathf.Pow(wc, 2));
                
                A = new float[]
                {
                    4.0f * Mathf.Pow(sampleHz, 2) / b0,
                    -8.0f * Mathf.Pow(sampleHz, 2) / b0,
                    4.0f * Mathf.Pow(sampleHz, 2) / b0
                };

                B = new float[]
                {
                    (2 / (wc * wc) - 8.0f * Mathf.Pow(sampleHz, 2)) / b0,
                    (4.0f * Mathf.Pow(sampleHz, 2) - 4.0f * sampleHz * zeta / wc + 1 / (Mathf.Pow(wc, 2))) / b0
                };
            }
        }
    }
}