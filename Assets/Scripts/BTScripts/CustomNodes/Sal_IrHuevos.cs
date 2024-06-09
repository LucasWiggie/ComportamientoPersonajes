using MBT;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomNodes
{
    // Empty Menu attribute prevents Node to show up in "Add Component" menu.
    [AddComponentMenu("")]
    [MBTNode(name = "CustomNodes/Sal_IrHuevos")]

    public class Sal_IrHuevos : Leaf
    {
        public override NodeResult Execute()
        {
            throw new System.NotImplementedException();
        }
    }
}