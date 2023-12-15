using MBT;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomNodes
{
    // Empty Menu attribute prevents Node to show up in "Add Component" menu.
    [AddComponentMenu("")]
    [MBTNode(name = "CustomNodes/Croc_Comer")]

    public class Croc_Comer : Leaf
    {

        // This is called every tick as long as node is executed
        public override NodeResult Execute()
        {
            // AQUI LA EJECUCI�N DE QUE EL COCODRILO SE MUEVA A LA CAZA
            return NodeResult.success;
        }
    }
}