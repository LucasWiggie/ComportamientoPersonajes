using MBT;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomNodes
{
    [AddComponentMenu("")]
    [MBTNode(name = "CustomNodes/Croc_HayHuevos")]
    public class Croc_HayHuevos : Leaf
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
            
            // AQUÍ LA COMPROBACIÓN DE SI HAY HUEVOS
            Cocodrilo.ChaseState estadoCaminar = cocodriloScript.HayHuevos();
            Debug.Log("SE INICIO ARBOL DEL CROCCCC");
            switch (estadoCaminar)
            {
                case Cocodrilo.ChaseState.Finished:
                    return NodeResult.success;
                case Cocodrilo.ChaseState.Failed:
                    return NodeResult.failure;
                default:
                    return NodeResult.failure;
            }
        }
    }
}
