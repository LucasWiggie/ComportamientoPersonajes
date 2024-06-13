using MBT;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomNodes
{
    // Empty Menu attribute prevents Node to show up in "Add Component" menu.
    [AddComponentMenu("")]
    [MBTNode(name = "CustomNodes/Sal_PonerHuevos")]

    public class Sal_PonerHuevos : Leaf
    {
        public override NodeResult Execute()
        {
            throw new System.NotImplementedException();
        }
    }
}