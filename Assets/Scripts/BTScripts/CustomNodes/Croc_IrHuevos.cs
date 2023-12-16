using MBT;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomNodes
{
    // Empty Menu attribute prevents Node to show up in "Add Component" menu.
    [AddComponentMenu("")]
    [MBTNode(name = "CustomNodes/Croc_IrHuevos")]

    public class Croc_IrHuevos : Leaf
    {
        private Cocodrilo crocodile;

        // This is called every tick as long as node is executed
        private void Awake()
        {
            crocodile = GetComponentInParent<Cocodrilo>();
        }
        public override NodeResult Execute()
        {
            // AQUI LA EJECUCI�N DE QUE EL COCODRILO SE MUEVA A LOS HUEVOS
            Cocodrilo.ChaseState estadoIr = crocodile.IrAHuevos();
            switch (estadoIr)
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