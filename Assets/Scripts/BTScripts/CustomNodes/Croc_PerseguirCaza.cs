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
        private NavMeshAgent crocodrile;
        private Collider collidedObject;
        public Transform animalTarget;
        

        // This is called every tick as long as node is executed
        private void Start()
        {
            crocodrile = GetComponentInParent<NavMeshAgent>();
            
        }
        public override NodeResult Execute()
        {
            // AQUI LA EJECUCIÓN DE QUE EL COCODRILO SE MUEVA A LA CAZA
            crocodrile.speed = crocodrile.speed + 1;
            crocodrile.SetDestination(animalTarget.position);
            return NodeResult.success;
        }
    }
}
