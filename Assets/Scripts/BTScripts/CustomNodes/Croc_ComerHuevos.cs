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
        private Cocodrilo crocodile;
        
        public override NodeResult Execute()
        {
       
            crocodile = GetComponentInParent<Cocodrilo>();
            if (crocodile == null)
            {
                Debug.LogError("crocodile is still null!");
                return NodeResult.failure;
            }
            
            // AQUI LA EJECUCIÓN DE QUE EL COCODRILO SE COMA LOS HUEVOS
            crocodile.Eat(false,true);//come huevos no animal
            return NodeResult.success;
        }
    }
}