using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Com.A9.Progression
{
    public class ProgressionEvents : MonoBehaviour
    {
        public static bool FINISHED_QUEST(string nm)
        {
            return Progression.instance.IsFinished(nm);
        }
    }
}