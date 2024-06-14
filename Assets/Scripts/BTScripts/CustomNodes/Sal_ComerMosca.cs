using MBT;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomNodes
{
    // Empty Menu attribute prevents Node to show up in "Add Component" menu.
    [AddComponentMenu("")]
    [MBTNode(name = "CustomNodes/Sal_ComerMosca")]

    public class Sal_ComerMosca : Leaf
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

            Salamandra.ChaseState estadoHuida = salamandraScript.ComerMosca();
            switch (estadoHuida)
            {
                case Salamandra.ChaseState.Enproceso:
                    return NodeResult.running;
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