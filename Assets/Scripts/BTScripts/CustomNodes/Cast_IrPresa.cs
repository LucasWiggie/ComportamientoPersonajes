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

        // This is called every tick as long as node is executed
        public override NodeResult Execute()
        {
            // AQUI LA EJECUCI�N DE QUE EL CASTOR VAYA A LA PRESA
            return NodeResult.success;
        }
    }
}