using MBT;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomNodes
{
    [AddComponentMenu("")]
    [MBTNode(name = "CustomNodes/Croc_HayHuevos")]
    public class Croc_HayHuevos : Condition
    {
        public Abort abort;
        public BoolReference somePropertyRef = new BoolReference(VarRefMode.DisableConstant);
        private Cocodrilo cocodriloScript; // Referencia al script Cocodrilo

        private void Start()
        {
            // Obtener el componente Cocodrilo adjunto al mismo objeto
            cocodriloScript = GetComponent<Cocodrilo>();
        }

        public override bool Check()
        {
            // AQU� LA COMPROBACI�N DE SI HAY HUEVOS
            return cocodriloScript.HayHuevos();
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
        }
    }
}
