using UnityEngine;

namespace FilterManager 
{
    /// <summary>
    /// This is the class responsible for creating filter object that filters the COP and the target position.
    /// Currently, only the moving average is used for filtering the target position to prevent suddent changes in the position
    /// The COP use have a 2nd order Butterworth filter, but the delay introduced by that filter is almost 0.5s, so COP is
    /// currently unfiltered.
    /// The type of filter that you create is dependent on the constructor that you use to instaniate the filter object.
    /// </summary>
    public class Filter
    {
        //All filters
        private int _order;

        //BW filter
        private const float _PI = Mathf.PI;
        private float _wc;
        private FilterStage[] _filterStages;

        //MA filter
        private FIRComponent _fir;
        private float[] _coeffs;
        
        /// <summary>
        /// Create a high or low pass Butterworth filter with a specified cutoff frequency, smapling frequency, and order.
        /// The default is a low pass filter with high set to false.
        /// </summary>
        public Filter(float cutoffHz, float sampleHz, int order, bool high = false) // BW filter true is high, false is low (default)
        {
            _wc = 2.0f * sampleHz * Mathf.Tan(_PI * cutoffHz / sampleHz); //default for low pass
            _order = order;

            if (high)
                _wc = 1.0f / _wc; //high pass has the inverse of the low pass

            if (order % 2 == 0)
                _filterStages = new FilterStage[_order / 2]; //even order calls for order/2 number of stages
            else
                _filterStages = new FilterStage[_order / 2 + 1]; //odd order calls for order/2 + 1 number of stages
        
            GetStages(_order, sampleHz, high);
        }

        /// <summary>
        /// Create a moving average filter with a specified order.
        /// </summary>
        public Filter(int order) // MA filter
        {
            _order = order;
            _coeffs = new float[order + 1];
            _fir = new FIRComponent(order + 1);

            for (var i = 0; i <= _order; i++)
                _coeffs[i] = 1f/(float)(order + 1);
        }

        // The Butterworth filter is computed via a cascading stages method. The cascading stages method involves consecutive 2nd
        // order filters. For odd order filters, a 1st order stage is added as well.
        private void GetStages(int order, float sampleHz, bool high)
        {
            float zeta = 0.0f; //set to zero for now, zet not needed for first order and recalculated later for 2nd order

            if (order % 2 != 0) //odd order
            {
                _filterStages[0] = new FilterStage(zeta, _wc, order, sampleHz, high, true); //first order filter

                if (order >= 3) //want to avoid error where maximum limit of k is 0 for loop
                {
                    for (var k = 1; k <= (order - 1) / 2; k++)
                    {
                        zeta = -Mathf.Cos(_PI * (2.0f * k + order - 1) / (2.0f * order));
                        _filterStages[k] = new FilterStage(zeta, _wc, order, sampleHz, high);
                    }
                }
            }
            else //even order
            {
                for (var k = 1; k <= order / 2; k++)
                {
                    zeta = -Mathf.Cos(_PI * (2.0f * k + order - 1) / (2.0f * order));
                    _filterStages[k - 1] = new FilterStage(zeta, _wc, order, sampleHz, high);
                }
            }
        }

        /// <summary>
        /// Pass in an unfiltered point to the Butterworth filter and a filtered point is returned
        /// </summary>
        public float ComputeBW(float input)
        {
            var output = input;
            
            for (var stage = 0; stage < _filterStages.Length; stage++)
            {
                if (_order % 2 != 0 && stage == 0) //solve 1st order filter for odd ordered filters
                    output = _filterStages[stage].IIRComponent1st.Solve(_filterStages[stage].FIRComponent1st.Solve(output, 
                                                                        _filterStages[stage].A), _filterStages[stage].B);
                else
                    output = _filterStages[stage].IIRComponent2nd.Solve(_filterStages[stage].FIRComponent2nd.Solve(output, 
                                                                        _filterStages[stage].A), _filterStages[stage].B);
            }

            return output;
        }

        /// <summary>
        /// Pass in an unfiltered point to the moving average filter and a filtered point is returned
        /// </summary>
        public float ComputeMA(float input)
        {
            var output = _fir.Solve(input, _coeffs);

            return output;
        }
    }
}