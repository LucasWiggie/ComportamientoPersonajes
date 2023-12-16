using MBT;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomNodes
{
    // Empty Menu attribute prevents Node to show up in "Add Component" menu.
    [AddComponentMenu("")]
    [MBTNode(name = "CustomNodes/MPato_PerseguirSal")]

    public class MPato_PerseguirSal : Leaf
    {

        // This is called every tick as long as node is executed
        public override NodeResult Execute()
        {
            // AQUI LA EJECUCIÓN DE QUE EL PATO PERSIGA A LA SALAMANDRA
            return NodeResult.success;
        }
    }
}
