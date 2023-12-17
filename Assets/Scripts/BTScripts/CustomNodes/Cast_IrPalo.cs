using MBT;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomNodes
{
// Empty Menu attribute prevents Node to show up in "Add Component" menu.
[AddComponentMenu("")]
[MBTNode(name = "CustomNodes/Cast_IrPalo")]
 

public class Cast_IrPalo : Leaf
    {
        private Castor castor;
        private bool hasEnteredRunningState = false;

        private void Awake()
        {
            castor = GetComponentInParent<Castor>();
            if (castor == null) { Debug.Log("no hay castor en irPalo"); }
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

            if (castor == null)
            {
                castor = GetComponentInParent<Castor>();
                if (castor == null)
                {
                    Debug.LogError("castorScript is still null!");
                    return NodeResult.failure;
                }
            }

            Castor.ChaseState estadoHuida = castor.irPalo();
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