using UnityEngine;

public class IKControl : MonoBehaviour
{
   public ActorController actorController;

   private void OnAnimatorIK(int layerIndex)
   {
      actorController.OnAnimatorIK(layerIndex);
   }
}
