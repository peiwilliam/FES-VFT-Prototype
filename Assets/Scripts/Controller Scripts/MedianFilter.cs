using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.Statistics;

namespace ControllerManager
{
    /// <summary>
    /// This internal class is used for just removing any anomalous spikes in the stimulation caused by sudden icnreases in velocity 
    /// in the gravity compensation part of the controller. It does not completely remove the spikes, but just reduces their height
    /// so that they're not as high.
    /// </summary>
    public class MedianFilter
    {
        private int _numOfPts; //the number of points being used for the filter
        private int _counter; //counter to keep track of when the list of values to be filtered has been fully filled at the beginning
        private List<float> _unsortedPts; //the list of unosrted points, here just to keep a record of which points are getting removed, this could be a stack instead but list works just fine

        /// <summary>
        /// Create an instance of the MedianFilter class. Requires the number of points to be used in the filter.
        /// </summary>
        public MedianFilter(int numOfPts)
        {
            _numOfPts = numOfPts;
            _unsortedPts = new List<float>(Enumerable.Repeat<float>(0.0f, _numOfPts)); //create a list with numOfPts of zeros
            _counter = 0;
        }

        /// <summary>
        /// Solves for the filtered point given the stored number of points.
        /// </summary>
        public float Solve(float input)
        {
            for (var i = _numOfPts - 1; i >= 0; i--)
            {
                if (i == 0)
                    _unsortedPts[i] = input;
                else
                    _unsortedPts[i] = _unsortedPts[i - 1];
            }

            if (_counter < _numOfPts - 1) //use a counter to keep track of how many points we'v4e filled up to this point
            {
                _counter++;
                return input; //when we haven't filled the list yet, just set the output as input
            }
            else
                return _unsortedPts.Median(); 
        }
    }
}