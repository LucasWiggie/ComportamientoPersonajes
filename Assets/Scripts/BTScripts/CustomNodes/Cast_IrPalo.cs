using MBT;
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
        
        private Castor castorScript;
        // This is called every tick as long as node is executed
        private void Awake()
        {
            castorScript = GetComponentInParent<Castor>();
            if(castorScript == null) { Debug.Log("no hay castor en irPalo"); }
        }

        public override NodeResult Execute()
        {
            if (castorScript == null)
            {
                castorScript = GetComponentInParent<Castor>();
                if (castorScript == null)
                {
                    Debug.LogError("castorScript is still null!");
                    return NodeResult.failure;
                }
            }
            Castor.ChaseState estadoHuida = castorScript.irPalo();
            switch (estadoHuida)
            {
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