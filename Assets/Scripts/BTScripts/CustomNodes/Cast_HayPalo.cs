using MBT;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomNodes
{
    [AddComponentMenu("")]
    [MBTNode(name = "CustomNodes/Cast_HayPalo")]
    public class Cast_HayPalo : Leaf
    {
        public Abort abort;
        private Castor castorScript;


        public override NodeResult Execute()
        {
            castorScript = GetComponentInParent<Castor>();
            if (castorScript == null)
            {
                Debug.LogError("castorScript is still null!");
                return NodeResult.failure;
            }
            
            // AQUI LA EJECUCIÓN DE QUE EL COCODRILO SE MUEVA A LA ARENA
            Castor.ChaseState estadoHuida = castorScript.ComprobarVision();
            switch (estadoHuida)
            {
                case Castor.ChaseState.Finished:
                    return NodeResult.success;
                case Castor.ChaseState.Failed:
                    return NodeResult.failure;
                default:
                    return NodeResult.failure;
            }
        }

        /* public override bool Check()
         {
             // AQUÍ LA COMPROBACIÓN DE SI HAY PRESA Y DE DÓNDE ESTÁ
             return castorScript.ComprobarVision();
         }
        
         public override void OnAllowInterrupt()
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
