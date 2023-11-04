using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Com.A9.Progression;

public class ProgressionTester : MonoBehaviour
{
    [ContextMenu("Item")]
    public void Reward()
    {
        Progression.instance.Reward("ProgressionRewards, Assembly-CSharp<tp>K 1");
    }
}
