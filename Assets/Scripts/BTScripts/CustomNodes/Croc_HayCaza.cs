using MBT;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomNodes
{
    [AddComponentMenu("")]
    [MBTNode(name = "CustomNodes/Croc_HayCaza")]
    public class Croc_HayCaza : Condition
    {
        public Abort abort;
        public BoolReference somePropertyRef = new BoolReference(VarRefMode.DisableConstant);
        public LayerMask targetMask; // Nueva variable para la capa del objetivo
        private Cocodrilo cocodriloScript; // Referencia al script Cocodrilo

        private void Start()
        {
            // Obtener el componente Cocodrilo adjunto al mismo objeto
            cocodriloScript = GetComponent<Cocodrilo>();
        }

        public override bool Check()
        {
            // AQUÍ LA COMPROBACIÓN DE SI HAY CAZA Y QUE ESTA NO ESTÁ A SALVO
            //return somePropertyRef.Value == true;
            return HayCaza();
        }


        private bool HayCaza() //DE MOMENTO NO TENGO EN CUENTA SI EL ENEMIGO ESTA A SALVO O NO, MAÑANA LO VEO, NO CREO QUE SEA DIFICIL BESOS MUAK MUAK MUAK 
        {
            // Verificar si el script Cocodrilo está adjunto
            if (cocodriloScript == null)
            {
                Debug.LogError("El script Cocodrilo no está adjunto.");
                return somePropertyRef.Value == false;
            }

            Collider[] enemigosCercanos = Physics.OverlapSphere(transform.position, cocodriloScript.radio, targetMask);

            foreach (var enemigo in enemigosCercanos)
            {
                if (enemigo.gameObject.layer == LayerMask.NameToLayer("targetCocodrilo"))
                {
                    return somePropertyRef.Value == true; // Hay un enemigo con la capa "targetCocodrilo" cerca
                }
            }

            return somePropertyRef.Value == false; // No hay enemigo con la capa "targetCocodrilo" cerca
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


