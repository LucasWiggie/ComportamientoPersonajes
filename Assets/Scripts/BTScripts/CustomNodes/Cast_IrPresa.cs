using MBT;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomNodes
{
    // Empty Menu attribute prevents Node to show up in "Add Component" menu.
    [AddComponentMenu("")]
    [MBTNode(name = "CustomNodes/Cast_IrPresa")]

    public class Cast_IrPresa : Leaf
    {

        private Castor castor;

        private void Awake()
        {
            castor = GetComponentInParent<Castor>();
        }


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


            Castor.ChaseState estadoHuida = castor.irPresa();
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