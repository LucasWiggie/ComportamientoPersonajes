using MBT;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomNodes
{
    [AddComponentMenu("")]
    [MBTNode(name = "CustomNodes/Sal_HayArena")]
    public class Sal_HayArena : Leaf
    {
        public Abort abort;
        private Salamandra salamandraScript;

        public override NodeResult Execute()
        {
            salamandraScript = GetComponentInParent<Salamandra>();
            if (salamandraScript == null)
            {
                Debug.LogError("salamandraScript is still null!");
                return NodeResult.failure;
            }

            Salamandra.ChaseState estadoHuida = salamandraScript.HayArena();
            switch (estadoHuida)
            {
                case Salamandra.ChaseState.Finished:
                    Debug.Log("ChaseEstate.Finished");
                    return NodeResult.success;
                case Salamandra.ChaseState.Failed:
                    Debug.Log("ChaseEstate.Failed");
                    return NodeResult.failure;
                default:
                    Debug.Log("Defauly: failure");
                    return NodeResult.failure;
            }
        }
    }
}
