using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using HarmonyLib;
using MonoMod.RuntimeDetour;
using ULTRAKILL.Cheats;
using UnityEngine;

namespace RocketPatcher
{
    [HarmonyPatch(typeof(Grenade))]
    internal class GrenadePatcher
    {
        private static bool wasRocketRideStartedThisFrame = false;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Grenade), nameof(Grenade.PlayerRideStart))]
        static void WasRocketStartedThisFrame()
        {
            wasRocketRideStartedThisFrame = true;
        }


        [HarmonyPrefix]
        [HarmonyPatch(typeof(Grenade), nameof(Grenade.LateUpdate))]
        static private bool LateUpdatePatch(Grenade __instance)
        {
            if (!__instance.playerRiding)
            {
                return false;
            }

            if (!wasRocketRideStartedThisFrame)
            {
                return true;
            }
            wasRocketRideStartedThisFrame = false;

            if (Vector3.Distance(__instance.transform.position, MonoSingleton<NewMovement>.Instance.transform.position) > 5f + __instance.rb.velocity.magnitude * Time.deltaTime)
            {
                __instance.PlayerRideEnd();
                return false;
            }

            Vector2 rocketControlInput = MonoSingleton<InputManager>.Instance.InputSource.Move.ReadValue<Vector2>(); 
            __instance.transform.Rotate(rocketControlInput.y * Time.deltaTime * 165f, rocketControlInput.x * Time.deltaTime * 165f, 0f, Space.Self); 
            if (Physics.Raycast(__instance.transform.position + __instance.transform.forward, __instance.transform.up, 4f, LayerMaskDefaults.Get(LMD.Environment)))
            {
                if (Physics.Raycast(__instance.transform.position + __instance.transform.forward, Vector3.up, out var hitInfo, 2f, LayerMaskDefaults.Get(LMD.Environment)))
                {
                    // MonoSingleton<NewMovement>.Instance.transform.position = __instance.transform.position + __instance.transform.forward - Vector3.up * hitInfo.distance;
                    __instance.transform.position = MonoSingleton<NewMovement>.Instance.transform.position + (Vector3.up * hitInfo.distance) - __instance.transform.forward;
                }
                else
                {
                    // MonoSingleton<NewMovement>.Instance.transform.position = __instance.transform.position + __instance.transform.forward;
                    __instance.transform.position = MonoSingleton<NewMovement>.Instance.transform.position - __instance.transform.forward;
                }
            }
            else
            {
                // MonoSingleton<NewMovement>.Instance.transform.position = __instance.transform.position + __instance.transform.up * 2f + __instance.transform.forward;
                __instance.transform.position= MonoSingleton<NewMovement>.Instance.transform.position - (__instance.transform.forward + __instance.transform.up * 2f);
            }

            MonoSingleton<CameraController>.Instance.CameraShake(0.1f);
            return false;
        }
    }
}
