using HarmonyLib;
using UnityEngine;

namespace RocketPatcher
{
    [HarmonyPatch(typeof(Grenade))]
    internal class GrenadePatcher
    {
        private static Vector3 CurrentPlayerPos { 
            get => MonoSingleton<NewMovement>.Instance.transform.position; 
            set => MonoSingleton<NewMovement>.Instance.transform.position = value; } 

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Grenade), nameof(Grenade.LateUpdate))]
        static private bool LateUpdatePatch(Grenade __instance)
        {
            if (!__instance.playerRiding)
            {
                return false;
            }

            if (Vector3.Distance(__instance.transform.position, CurrentPlayerPos) > 5f + __instance.rb.velocity.magnitude * Time.deltaTime)
            {
                __instance.PlayerRideEnd();
                return false;
            }

            Vector2 rocketControlInput = MonoSingleton<InputManager>.Instance.InputSource.Move.ReadValue<Vector2>();
            __instance.transform.Rotate(rocketControlInput.y * Time.deltaTime * 165f, rocketControlInput.x * Time.deltaTime * 165f, 0f, Space.Self);

            Vector3 expectedPlayerPos;
            if (Physics.Raycast(__instance.transform.position + __instance.transform.forward, __instance.transform.up, 4f, LayerMaskDefaults.Get(LMD.Environment)))
            {
                if (Physics.Raycast(__instance.transform.position + __instance.transform.forward, Vector3.up, out var hitInfo, 2f, LayerMaskDefaults.Get(LMD.Environment)))
                {
                    expectedPlayerPos = __instance.transform.position + __instance.transform.forward - Vector3.up * hitInfo.distance;
                }
                else
                {
                    expectedPlayerPos = __instance.transform.position + __instance.transform.forward;
                }
            }
            else
            {
                expectedPlayerPos = __instance.transform.position + __instance.transform.up * 2f + __instance.transform.forward;
            }

            if (CapsuleOverlapChecks(expectedPlayerPos, out Collider collision))
            {
                __instance.PlayerRideEnd();
                __instance.Collision(collision);
                Plugin.Logger.LogInfo("Invalid rocket ride");
# if DEBUG
                var playerCapsule = MonoSingleton<NewMovement>.Instance.playerCollider;
                DebugDrawing.DrawCapsule(CurrentPlayerPos, playerCapsule.height, playerCapsule.radius, Color.blue);
                Vector3 playerHeadLocal = Vector3.up * (playerCapsule.height / 2 - playerCapsule.radius);
                Vector3 playerFootLocal = Vector3.down * (playerCapsule.height / 2 - playerCapsule.radius);
                DebugDrawing.DrawCapsule(expectedPlayerPos, playerCapsule.height, playerCapsule.radius, Color.red);
                DebugDrawing.DrawCapsule(playerHeadLocal + CurrentPlayerPos, expectedPlayerPos + playerHeadLocal, playerCapsule.radius, Color.gray);
                DebugDrawing.DrawCapsule(playerFootLocal + CurrentPlayerPos, expectedPlayerPos + playerFootLocal, playerCapsule.radius, Color.gray);
#endif
                return false;
            }

            CurrentPlayerPos = expectedPlayerPos;
            
            MonoSingleton<CameraController>.Instance.CameraShake(0.1f);
            return false;
        }

        private static bool CapsuleOverlapChecks(Vector3 expectedPlayerPos, out Collider collision)
        {
            // generate 3 new capsules to check collision

            CapsuleCollider playerCapsule = MonoSingleton<NewMovement>.Instance.playerCollider;
            Vector3 playerHeadLocal = Vector3.up * (playerCapsule.height / 2 - playerCapsule.radius);
            Vector3 playerFootLocal = Vector3.down * (playerCapsule.height / 2 - playerCapsule.radius);

            Collider[] CollidedObjects = new Collider[1];

            if (Physics.OverlapCapsuleNonAlloc(CurrentPlayerPos + playerHeadLocal, expectedPlayerPos + playerHeadLocal, playerCapsule.radius, CollidedObjects, LayerMaskDefaults.Get(LMD.Environment)) > 0 ||
                Physics.OverlapCapsuleNonAlloc(CurrentPlayerPos + playerFootLocal, expectedPlayerPos + playerFootLocal, playerCapsule.radius, CollidedObjects, LayerMaskDefaults.Get(LMD.Environment)) > 0 ||
                Physics.OverlapCapsuleNonAlloc(expectedPlayerPos + playerHeadLocal, expectedPlayerPos + playerFootLocal, playerCapsule.radius, CollidedObjects, LayerMaskDefaults.Get(LMD.Environment)) > 0
                )
            {
                collision = CollidedObjects[0];
                return true;
            }

            collision = null;
            return false;
        }
    }
}
