using MBT;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace CustomNodes
{
    // Empty Menu attribute prevents Node to show up in "Add Component" menu.
    [AddComponentMenu("")]
    [MBTNode(name = "CustomNodes/Croc_PerseguirCaza")]

    public class Croc_PerseguirCaza : Leaf
    {
 
        
        private Cocodrilo crocodile;

        // This is called every tick as long as node is executed
        private void Awake()
        {
            crocodile= GetComponentInParent<Cocodrilo>();
        }
        public override NodeResult Execute()
        {
            // AQUI LA EJECUCIÓN DE QUE EL COCODRILO SE MUEVA A LA CAZA
            crocodile.Chase();
            return NodeResult.success;
        }
    }
}
