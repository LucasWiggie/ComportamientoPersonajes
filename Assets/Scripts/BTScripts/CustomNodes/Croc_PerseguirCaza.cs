using MBT;
using UnityEngine;

namespace CustomNodes
{
    [AddComponentMenu("")]
    [MBTNode(name = "CustomNodes/Croc_PerseguirCaza")]
    public class Croc_PerseguirCaza : Leaf
    {
        private Cocodrilo crocodile;

        private void Awake()
        {
            crocodile = GetComponentInParent<Cocodrilo>();
        }

        public override NodeResult Execute()
        {
            if (crocodile == null)
            {
                crocodile = GetComponentInParent<Cocodrilo>();
                if (crocodile == null)
                {
                    Debug.LogError("Crocodile is still null!");
                    return NodeResult.failure;
                }
            }

            Cocodrilo.ChaseState chaseState = crocodile.Chase();
            switch (chaseState)
            {
                case Cocodrilo.ChaseState.Finished:
                    Debug.Log("Persecution finished: moving to eat.");
                    return NodeResult.success; // La persecución ha terminado
                case Cocodrilo.ChaseState.Failed:
                    Debug.Log("Persecution failed.");
                    return NodeResult.failure; // La persecución falló
                case Cocodrilo.ChaseState.Enproceso:
                    Debug.Log("Persecution in process.");
                    return NodeResult.running; // La persecución está en curso
                default:
                    Debug.LogError("Unexpected chase state.");
                    return NodeResult.failure; // Estado inesperado
            }
        }
    }
}