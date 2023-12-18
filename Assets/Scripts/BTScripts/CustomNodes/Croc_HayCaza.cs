using MBT;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomNodes
{
    [AddComponentMenu("")]
    [MBTNode(name = "CustomNodes/Croc_HayCaza")]
    public class Croc_HayCaza : Leaf
    {
        public Abort abort;
        private Cocodrilo cocodriloScript; // Referencia al script Cocodrilo

        public override NodeResult Execute()
        {
            cocodriloScript = GetComponentInParent<Cocodrilo>();

            if (cocodriloScript == null)
            {
                Debug.LogError("crocodile is still null!");
                return NodeResult.failure;
            }


            Cocodrilo.ChaseState estadoCaza = cocodriloScript.HayCaza();
            switch (estadoCaza)
            {
                case Cocodrilo.ChaseState.Finished:
                    Debug.Log("bien");
                    return NodeResult.success;
                case Cocodrilo.ChaseState.Failed:
                    Debug.Log("mal");
                    return NodeResult.failure;
                default:
                    Debug.Log("default");
                    return NodeResult.failure;
            }
        }
    }
}


