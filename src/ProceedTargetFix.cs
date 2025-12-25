

using HarmonyLib;
using MGSC;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Linq;

namespace ThrowRangeWorkOnGrenade
{

    [HarmonyPatch(typeof(SelectGrenadeTarget), nameof(SelectGrenadeTarget.ProceedTarget))]
    public class ProceedTargetFix
    {

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            bool found = false;

            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Callvirt && codes[i].operand.ToString().Contains("get_Range") &&
                    codes[i + 1].opcode == OpCodes.Ldc_I4_1 &&
                    codes[i + 2].opcode == OpCodes.Add)
                {
                    // The stack currently has (Range + 1). 
                    // We will now load 'this' and navigate to the bonus method.

                    var injection = new List<CodeInstruction>
                {
                    new CodeInstruction(OpCodes.Ldarg_0), // Load 'this'
                    // Access this._creatures
                    new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(SelectGrenadeTarget), "_creatures")), 
                    // Access .Player
                    new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(MGSC.Creatures), "Player")), 
                    // Access .CreatureData
                    new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(MGSC.Creature), "CreatureData")),
                    // Call the bonus method
                    new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(MGSC.CreatureData), "GetMeleeThrowRangeBonus")),
                    // Add bonus to (Range + 1)
                    new CodeInstruction(OpCodes.Add)
                };

                    codes.InsertRange(i + 3, injection);
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                Plugin.Logger.LogError("Could not find patch location in SelectGrenadeTarget.ProceedTarget!");
            }

            return codes.AsEnumerable();
        }

        /*
        static bool Prefix(ref SelectGrenadeTarget __instance, CellPosition cellPosition)
        {
            if (__instance._ballisticPath.Path.Count == 0)
            {
                SingletonMonoBehaviour<SoundController>.Instance.PlayUiSound(SingletonMonoBehaviour<SoundsStorage>.Instance.EmptyAttack, false, 0f);
                return false;
            }
            CellPosition position = __instance._creatures.Player.CreatureData.Position;
            BasePickupItem itemByIndex = __instance._creatures.Player.CreatureData.Inventory.VestStore.GetItemByIndex(__instance._slotIndex - 1);
            GrenadeRecord grenadeRecord = itemByIndex.Record<GrenadeRecord>();
            int num = grenadeRecord.Range + 1 + __instance._creatures.Player.CreatureData.GetMeleeThrowRangeBonus();
            int num2 = num + grenadeRecord.MaxOverthrowDistance;
            if (grenadeRecord.RicochetTrajectory)
            {
                __instance._ballisticPath = TrajectoryCalculator.CalculateWayWithRicochets(__instance._mapGrid, __instance._mapRenderer, __instance._mapObstacles, __instance._ballisticPath, position, cellPosition, false, 0f, num2, -1, false, true);
            }
            else
            {
                __instance._ballisticPath = TrajectoryCalculator.CalculateWayWithRicochets(__instance._mapGrid, __instance._mapRenderer, __instance._mapObstacles, __instance._ballisticPath, position, cellPosition, false, 0f, num2, 0, false, true);
            }
            int range = __instance.CalculateGrenadeRange(cellPosition, __instance._ballisticPath, num, num2, grenadeRecord);
            __instance._ballisticPath = __instance._ballisticPath.GetWithRange(range);
            __instance._ballisticPath.CutWalls(__instance._mapGrid);
            if (position.Equals(cellPosition))
            {
                __instance._ballisticPath.Clear();
                __instance._ballisticPath.Path.Add(cellPosition);
            }
            __instance._creatures.Player.RaisePerkAction(PerkLevelUpActionType.ThrowGrenade, -1);
            __instance._creatures.Player.ChangeDirection(cellPosition, true, true, false);
            __instance._creatures.Player.CreatureData.EffectsController.PropagateAction(PlayerActionHappened.HandAction);
            __instance._creatures.Player.ThrowGrenade(__instance._ballisticPath, itemByIndex, false);
            UI.Hide<SelectGrenadeTarget>();
            return false;
        }
        */
    }
}
