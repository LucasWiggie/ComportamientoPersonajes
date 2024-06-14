using MBT;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomNodes
{
    [AddComponentMenu("")]
    [MBTNode(name = "CustomNodes/Sal_HayMosca")]
    public class Sal_HayMosca : Leaf
    {
        public Abort abort;
        private Salamandra salamandraScript;

        private void Start()
        {

        }
        public override NodeResult Execute()
        {
            salamandraScript = GetComponentInParent<Salamandra>();

            if (salamandraScript == null)
            {
                Debug.LogError("salamandraScript is still null!");
                return NodeResult.failure;
            }

            Salamandra.ChaseState estadoHuida = salamandraScript.HayMosca();
            switch (estadoHuida)
            {
                case Salamandra.ChaseState.Finished:
                    return NodeResult.success;
                case Salamandra.ChaseState.Failed:
                    return NodeResult.failure;
                default:
                    return NodeResult.failure;
            }
        }
    }
}
