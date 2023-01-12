using System.Collections.Generic;
using UnityEngine;

namespace KnuthShuffle
{
    /// <summary>
    /// This class is responsible for randomly shufflying the elements within an array or list.
    /// The code is adapted from here: https://stackoverflow.com/questions/2450954/how-to-randomize-shuffle-a-javascript-array
    /// </summary>
    public class KnuthShuffler
    {
        /// <summary>
        /// The list version of the shuffle. Give the method an input list and it will return a shuffled version of the list.
        /// </summary>
        public static List<T> Shuffle<T>(List<T> list)
        {
            for (var i = list.Count - 1; i > 0; i--)
            {
                var randomValue = Random.value;

                if (Random.value == 1f) //very unlikely to happen, but want to make sure that it never happens
                    randomValue = Random.value;

                var j = Mathf.FloorToInt(randomValue * (i + 1));
                var temp = list[i];
                list[i] = list[j];
                list[j] = temp;
            }

            return list;
        }

        /// <summary>
        /// The array version of the shuffle. Give the method an input array and it will return a shuffled version of the array.
        /// </summary>
        public static T[] Shuffle<T>(T[] array)
        {
            for (var i = array.Length - 1; i > 0; i--)
            {
                var randomValue = Random.value;

                if (Random.value == 1f) //very unlikely to happen, but want to make sure that it never happens
                    randomValue = Random.value;

                var j = Mathf.FloorToInt(randomValue * (i + 1));
                var temp = array[i];
                array[i] = array[j];
                array[j] = temp;
            }

            return array;
        }
    }
}