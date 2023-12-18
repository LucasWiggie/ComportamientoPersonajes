using MBT;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomNodes
{
    [AddComponentMenu("")]
    [MBTNode(name = "CustomNodes/Cast_HayPresa")]
    public class Cast_HayPresa : Leaf
    {
        public Abort abort;
        private Castor castorScript;


        public override NodeResult Execute()
        {
            castorScript = GetComponentInParent<Castor>();
            if (castorScript == null)
            {
                Debug.LogError("castorScript is still null!");
                return NodeResult.failure;
            }

            // AQUI LA EJECUCIÓN DE QUE EL COCODRILO SE MUEVA A LA ARENA
            Castor.ChaseState estadoHuida = castorScript.HayPresa();
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
