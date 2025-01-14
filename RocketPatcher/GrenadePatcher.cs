using HarmonyLib;
using UnityEngine;

namespace RocketPatcher
{
    [HarmonyPatch(typeof(Grenade))]
    internal class GrenadePatcher
    {
        private const float timeToAlign = 1f;
        private static float timeSpentAligning = 0f;
        private static Vector3 playerPosSavedRelative;
        private static Vector3 CurrentPlayerPos { 
            get => MonoSingleton<NewMovement>.Instance.transform.position; 
            set => MonoSingleton<NewMovement>.Instance.transform.position = value; } 

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Grenade), nameof(Grenade.PlayerRideStart))]
        static void WasRocketStartedThisFrame(Grenade __instance)
        {
            timeSpentAligning = 0f;
            playerPosSavedRelative = CurrentPlayerPos - __instance.transform.position;
        }


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
            AlignPlayer(__instance, expectedPlayerPos);
            MonoSingleton<CameraController>.Instance.CameraShake(0.1f);
            return false;
        }

        private static void AlignPlayer(Grenade __instance, Vector3 expectedPlayerPos)
        {
            if (!__instance.frozen)
            {
                timeSpentAligning += Time.deltaTime;
                Mathf.Clamp(timeSpentAligning, 0f, timeToAlign);
            }

            if (timeSpentAligning == 0f) // fixes weird sinking behaviour
            {
                CurrentPlayerPos = __instance.transform.position + playerPosSavedRelative;
            }
            else
            {
                CurrentPlayerPos = Vector3.Lerp(CurrentPlayerPos, expectedPlayerPos, timeSpentAligning / timeToAlign);
            }
        }
    }
}
