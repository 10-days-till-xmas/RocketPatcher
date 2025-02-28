using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;
using HarmonyLib;
using System.Reflection;

namespace RocketPatcher
{
    /// <summary>
    /// Inserts a branch before setting the player position to the rocket's position,
    /// and checks if the rocket ride is valid, else ends the ride and skips setting the player pos.
    /// </summary>
    [HarmonyPatch]
    internal static class GrenadeTranspiler
    {
        static readonly MethodInfo p_CameraController_Instance = AccessTools.PropertyGetter(typeof(MonoSingleton<CameraController>), "Instance");
        static readonly MethodInfo m_CameraController_CameraShake = AccessTools.Method(typeof(CameraController), nameof(CameraController.CameraShake));

        static readonly MethodInfo p_NewMovement_Instance = AccessTools.PropertyGetter(typeof(MonoSingleton<NewMovement>), "Instance");
        static readonly FieldInfo f_NewMovement_rb = AccessTools.Field(typeof(NewMovement), "rb");

        static readonly MethodInfo p_Component_transform = AccessTools.PropertyGetter(typeof(Component), nameof(Component.transform));
        static readonly MethodInfo p_Transform_position = AccessTools.PropertySetter(typeof(Transform), nameof(Transform.position));
        static readonly MethodInfo p_Rigidbody_position = AccessTools.PropertySetter(typeof(Rigidbody), nameof(Rigidbody.position));

        [HarmonyTranspiler]
        [HarmonyPriority(Priority.First)]
        [HarmonyPatch(typeof(Grenade), "LateUpdate", MethodType.Normal)]
        internal static IEnumerable<CodeInstruction> LateUpdateTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            return new CodeMatcher(instructions, generator)
                .Start()
                .MatchForward(false,
                    new CodeMatch(OpCodes.Call, p_CameraController_Instance),       // IL_01BD: call      !0 class MonoSingleton`1<class CameraController>::get_Instance()
                    new CodeMatch(OpCodes.Ldc_R4, 0.1f),                            // IL_01C2: ldc.r4    0.1
                    new CodeMatch(OpCodes.Callvirt, m_CameraController_CameraShake) // IL_01C7: callvirt  instance void CameraController::CameraShake(float32)
                    )
                .ThrowIfInvalid("Couldnt find a match for CameraController.CameraShake()")
                .CreateLabel(out Label l_CameraController_CameraShake)
                .Start()
                .MatchForward(false,
                    new CodeMatch(OpCodes.Call, p_NewMovement_Instance),            // IL_019D: call      !0 class MonoSingleton`1<class NewMovement>::get_Instance()
                    new CodeMatch(OpCodes.Callvirt, p_Component_transform),         // IL_01A2: callvirt  instance class [UnityEngine.CoreModule]UnityEngine.Transform [UnityEngine.CoreModule]UnityEngine.Component::get_transform()
                    new CodeMatch(OpCodes.Ldloc_1),                                 // IL_01A7: ldloc.1
                    new CodeMatch(OpCodes.Callvirt, p_Transform_position),          // IL_01A8: callvirt  instance void [UnityEngine.CoreModule]UnityEngine.Transform::set_position(valuetype [UnityEngine.CoreModule]UnityEngine.Vector3)
                    new CodeMatch(OpCodes.Call, p_NewMovement_Instance),            // IL_01AD: call      !0 class MonoSingleton`1<class NewMovement>::get_Instance()
                    new CodeMatch(OpCodes.Ldfld, f_NewMovement_rb),                 // IL_01B2: ldfld     class [UnityEngine.PhysicsModule]UnityEngine.Rigidbody NewMovement::rb
                    new CodeMatch(OpCodes.Ldloc_1),                                 // IL_01B7: ldloc.1
                    new CodeMatch(OpCodes.Callvirt, p_Rigidbody_position)           // IL_01B8: callvirt  instance void [UnityEngine.PhysicsModule]UnityEngine.Rigidbody::set_position(valuetype [UnityEngine.CoreModule]UnityEngine.Vector3)
                    )
                .ThrowIfInvalid("Couldnt find a match for setting the player position to the rocket")
                .Insert(
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldloc_1),
                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(GrenadeTranspiler), nameof(CheckIfPositionIsValid))),
                    new CodeInstruction(OpCodes.Brfalse, l_CameraController_CameraShake)
                    )
                .InstructionEnumeration();
        }

        public static bool CheckIfPositionIsValid(Grenade __instance, Vector3 targetPlayerPosition)
        {
            if (CapsuleCastCheck(targetPlayerPosition, out Collider collision))
            {
                __instance.PlayerRideEnd();
                __instance.Collision(collision);
                Plugin.Logger.LogInfo("Invalid rocket ride");
                return false;
            }
            return true;
        }

        private static bool CapsuleCastCheck(Vector3 expectedPlayerPos, out Collider collision)
        {
            Vector3 currentPlayerPos = MonoSingleton<NewMovement>.Instance.transform.position;
            CapsuleCollider playerCapsule = MonoSingleton<NewMovement>.Instance.playerCollider;
            Vector3 playerHeadLocal = Vector3.up * (playerCapsule.height / 2 - playerCapsule.radius);
            Vector3 playerFootLocal = Vector3.down * (playerCapsule.height / 2 - playerCapsule.radius);
            bool hit = Physics.CapsuleCast(
                currentPlayerPos + playerHeadLocal,
                currentPlayerPos + playerFootLocal,
                playerCapsule.radius,
                (expectedPlayerPos - currentPlayerPos).normalized,
                out RaycastHit hitInfo,
                (expectedPlayerPos - currentPlayerPos).magnitude, LayerMaskDefaults.Get(LMD.Environment));
            collision = hitInfo.collider;
            return hit;
        }
    }
}
