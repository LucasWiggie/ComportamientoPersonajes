using MBT;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomNodes
{
    // Empty Menu attribute prevents Node to show up in "Add Component" menu.
    [AddComponentMenu("")]
    [MBTNode(name = "CustomNodes/MPato_IrNenufares")]

    public class MPato_IrNenufares : Leaf
    {

        private Pato pato;
        // This is called every tick as long as node is executed
        private void Awake()
        {
            pato = GetComponentInParent<Pato>();
        }
        public override NodeResult Execute()
        {
            if (pato == null)
            {
                pato = GetComponentInParent<Pato>();
                if (pato == null)
                {
                    Debug.LogError("pato is still null!");
                    return NodeResult.failure;
                }
            }
            // AQUI LA EJECUCIÓN DE QUE EL COCODRILO SE MUEVA A LA ARENA
            Pato.ChaseState estadoCamino = pato.IrNenufares();
            switch (estadoCamino)
            {
                case Pato.ChaseState.Finished:
                    return NodeResult.success;
                case Pato.ChaseState.Failed:
                    return NodeResult.failure;
                default:
                    return NodeResult.failure;
            }
        }
    }
}