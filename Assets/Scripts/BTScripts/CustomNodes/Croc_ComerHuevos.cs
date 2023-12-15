using MBT;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomNodes
{
    // Empty Menu attribute prevents Node to show up in "Add Component" menu.
    [AddComponentMenu("")]
    [MBTNode(name = "CustomNodes/Croc_ComerHuevos")]

    public class Croc_ComerHuevos : Leaf
    {

        // This is called every tick as long as node is executed
        public override NodeResult Execute()
        {
            // AQUI LA EJECUCIÓN DE QUE EL COCODRILO SE COMA LOS HUEVOS
            return NodeResult.success;
        }
    }
}