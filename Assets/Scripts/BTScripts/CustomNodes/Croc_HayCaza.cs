using MBT;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomNodes
{
    [AddComponentMenu("")]
    [MBTNode(name = "CustomNodes/Croc_HayCaza")]
    public class Croc_HayCaza : Leaf
    {
        public Abort abort;
        private Cocodrilo cocodriloScript; // Referencia al script Cocodrilo

        private void Start()
        {
            // Obtener el componente Cocodrilo adjunto al mismo objeto
            cocodriloScript = GetComponentInParent<Cocodrilo>();
        }

        public override NodeResult Execute()
        {
            if (cocodriloScript == null)
            {
                cocodriloScript = GetComponentInParent<Cocodrilo>();
                if (cocodriloScript == null)
                {
                    Debug.LogError("crocodile is still null!");
                    return NodeResult.failure;
                }
            }
            
            Cocodrilo.ChaseState estadoCaza = cocodriloScript.ComprobarVision();
            switch (estadoCaza)
            {
                case Cocodrilo.ChaseState.Finished:
                    Debug.Log("bien");
                    return NodeResult.success;
                case Cocodrilo.ChaseState.Failed:
                    Debug.Log("mal");
                    return NodeResult.failure;
                default:
                    Debug.Log("maldefault");
                    return NodeResult.failure;
            }
            /*if(cocodriloScript.puedeVer == true)
            {
                return somePropertyRef.Value == true; // Hay un enemigo con la capa "targetCocodrilo" cerca
            }
            else
            {
                return somePropertyRef.Value == false;
            }*/
        }


        /*public override void OnAllowInterrupt()
        {
            // Do not listen any changes if abort is disabled
            if (abort != Abort.None)
            {
                // This method cache current tree state used later by abort system
                ObtainTreeSnapshot();
                // If somePropertyRef is constant, then null exception will be thrown.
                // Use somePropertyRef.isConstant in case you need constant enabled.
                // Constant variable is disabled here, so it is safe to do this.
                somePropertyRef.GetVariable().AddListener(OnVariableChange);
            }
        }

        public override void OnDisallowInterrupt()
        {
            if (abort != Abort.None)
            {
                // Remove listener
                somePropertyRef.GetVariable().RemoveListener(OnVariableChange);
            }
        }

        private void OnVariableChange(bool oldValue, bool newValue)
        {
            // Reevaluate Check() and abort tree when needed
            EvaluateConditionAndTryAbort(abort);
        }*/
    }
}


