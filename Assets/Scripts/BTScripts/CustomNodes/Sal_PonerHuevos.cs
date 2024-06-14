using MBT;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomNodes
{
    // Empty Menu attribute prevents Node to show up in "Add Component" menu.
    [AddComponentMenu("")]
    [MBTNode(name = "CustomNodes/Sal_PonerHuevos")]

    public class Sal_PonerHuevos : Leaf
    {
        private Salamandra salamandraScript;

        private void Start()
        {
            salamandraScript = GetComponentInParent<Salamandra>();
        }

        public override NodeResult Execute()
        {
            if (salamandraScript == null)
            {
                salamandraScript = GetComponentInParent<Salamandra>();
                if (salamandraScript == null)
                {
                    Debug.LogError("Salamandra is still null!");
                    return NodeResult.failure;
                }
            }

            Salamandra.ChaseState estadoHuevos = salamandraScript.PonerHuevos();
            switch (estadoHuevos)
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