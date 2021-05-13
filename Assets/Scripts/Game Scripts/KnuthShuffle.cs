using System.Collections.Generic;
using UnityEngine;

namespace KnuthShuffle
{
    public class KnuthShuffler
    {
        //taken from here: https://stackoverflow.com/questions/2450954/how-to-randomize-shuffle-a-javascript-array
        
        public static List<T> Shuffle<T>(List<T> list)
        {
            for (var i = list.Count - 1; i > 0; i--)
            {
                var randomValue = Random.value;

                if (Random.value == 1f) //very unlikely to happen, but want to make sure that it never happens
                    randomValue = Random.value; //will almost certainly not return 1 again if it happens

                var j = Mathf.FloorToInt(randomValue * (i + 1));
                var temp = list[i];
                list[i] = list[j];
                list[j] = temp;
            }

            return list;
        }

        public static T[] Shuffle<T>(T[] array)
        {
            for (var i = array.Length - 1; i > 0; i--)
            {
                var randomValue = Random.value;

                if (Random.value == 1f) //very unlikely to happen, but want to make sure that it never happens
                    randomValue = Random.value; //will almost certainly not return 1 again if it happens

                var j = Mathf.FloorToInt(randomValue * (i + 1));
                var temp = array[i];
                array[i] = array[j];
                array[j] = temp;
            }

            return array;
        }
    }
}