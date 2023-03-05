using System;
using UnityEngine;

namespace ControllerManager
{
    /// <summary>
    /// This internal class is used for just removing any anomalous spikes in the stimulation caused by sudden icnreases in velocity 
    /// in the gravity compensation part of the controller. It does not completely remove the spikes, but just reduces their height
    /// so that they're not as high.
    /// </summary>
    public class MedianFilter
    {
        private int _numOfPts;
        private int _counter;
        private float[] _unsortedPts;

        public MedianFilter(int numOfPts)
        {
            _numOfPts = numOfPts;
            _unsortedPts = new float[_numOfPts];
            _counter = 0;
        }

        public float Solve(float input)
        {
            var output = 0.0f;

            for (var i = _numOfPts - 1; i >= 0; i--)
            {
                if (i == 0)
                    _unsortedPts[i] = input;
                else
                    _unsortedPts[i] = _unsortedPts[i - 1];
            }

            if (_counter < _numOfPts) //use a counter to keep track of how many points we'v4e filled up to this point
            {
                output = input;
                _counter++;
            }
            else
            {
                var sortedPts = new float[_numOfPts];
                Array.Copy(_unsortedPts, sortedPts, _numOfPts);
                Array.Sort<float>(sortedPts);

                if (_numOfPts % 2 != 0) //if odd
                {
                    var idx = (_numOfPts + 1)/2 - 1; //minus one because we start indexing from 0
                    output = sortedPts[idx];
                }
                else //if even
                {
                    var firstIdx = _numOfPts/2 - 1; //minus one because we start indexing from 0
                    var secondIdx = _numOfPts/2;
                    output = (sortedPts[firstIdx] + sortedPts[secondIdx])/2;
                }
            }

            return output;
        }
    }
}