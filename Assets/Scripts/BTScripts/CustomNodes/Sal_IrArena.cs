using MBT;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomNodes
{
    // Empty Menu attribute prevents Node to show up in "Add Component" menu.
    [AddComponentMenu("")]
    [MBTNode(name = "CustomNodes/Sal_IrArena")]

    public class Sal_IrArena : Leaf
    {
        public override NodeResult Execute()
        {
            throw new System.NotImplementedException();
        }
    }
}