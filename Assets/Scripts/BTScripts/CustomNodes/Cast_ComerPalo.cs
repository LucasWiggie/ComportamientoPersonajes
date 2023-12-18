using BehaviourAPI.UtilitySystems;
using MBT;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomNodes
{
    // Empty Menu attribute prevents Node to show up in "Add Component" menu.
    [AddComponentMenu("")]
    [MBTNode(name = "CustomNodes/Cast_ComerPalo")]

    public class Cast_ComerPalo : Leaf
    {
        private Castor castor;
        // This is called every tick as long as node is executed
        public override NodeResult Execute()
        {

            if (castor == null)
            {
                castor = GetComponentInParent<Castor>();
                if (castor == null)
                {
                    Debug.LogError("castorScript is still null!");
                    return NodeResult.failure;
                }
            }

            if (castor == null)
            {
                castor = GetComponentInParent<Castor>();
                if (castor == null)
                {
                    Debug.LogError("castorScript is still null!");
                    return NodeResult.failure;
                }
            }

            Castor.ChaseState estadoHuida = castor.comerPalo();
            switch (estadoHuida)
            {
                case Castor.ChaseState.Enproceso:
                    return NodeResult.running;
                case Castor.ChaseState.Finished:
                    return NodeResult.success;
                case Castor.ChaseState.Failed:
                    return NodeResult.failure;
                default:
                    return NodeResult.failure;
            }
        }
    }
}