

using HarmonyLib;
using MGSC;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace ThrowRangeWorkOnGrenade
{

    [HarmonyPatch(typeof(FirearmSystem), nameof(FirearmSystem.ThrowGrenade))]
    public class ThrowGrenadeFix
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            bool found = false;

            for (int i = 0; i < codes.Count; i++)
            {
                if (
                    codes[i].opcode == OpCodes.Callvirt && codes[i].operand.ToString().Contains("get_Range") &&
                    codes[i + 1].opcode == OpCodes.Ldc_I4_1 &&
                    codes[i + 2].opcode == OpCodes.Add)
                {
                    // We inject AFTER the 'add' (Range + 1)
                    // We need 'user' to call GetMeleeThrowRangeBonus(). 
                    // 'user' is the 4th argument (index 3) in the original method.

                    var injection = new List<CodeInstruction>
                {
                    new CodeInstruction(OpCodes.Ldarg_3), // Load 'user' (Creature)
                    new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Creature), "CreatureData")), // Access CreatureData
                    new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(CreatureData), "GetMeleeThrowRangeBonus")), // Call bonus method
                    new CodeInstruction(OpCodes.Add) // Add the bonus to the existing (Range + 1)
                };

                    codes.InsertRange(i + 3, injection);
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                Plugin.Logger.LogError("Could not find patch location in FirearmSystem.ThrowGrenade!");
            }

            return codes.AsEnumerable();
        }


        /*
        static bool Prefix(MapGrid mapGrid, MapRenderer mapRenderer, MapObstacles mapObstacles, Creature user, BallisticPath pathContainer, BasePickupItem grenadeItem, CellPosition desiredTarget)
        {
            pathContainer.Clear();
            CellPosition position = user.CreatureData.Position;
            GrenadeRecord grenadeRecord = grenadeItem.Record<GrenadeRecord>();
            int num = grenadeRecord.Range + 1 + user.CreatureData.GetMeleeThrowRangeBonus();
            int num2 = num + grenadeRecord.MaxOverthrowDistance;
            if (grenadeRecord.RicochetTrajectory)
            {
                pathContainer = TrajectoryCalculator.CalculateWayWithRicochets(mapGrid, mapRenderer, mapObstacles, pathContainer, position, desiredTarget, false, 0f, num2, -1, false, true);
            }
            else
            {
                pathContainer = TrajectoryCalculator.CalculateWayWithRicochets(mapGrid, mapRenderer, mapObstacles, pathContainer, position, desiredTarget, false, 0f, num2, 0, false, true);
            }
            int range = FirearmSystem.CalculateGrenadeRange(desiredTarget, pathContainer, num, num2, grenadeRecord);
            pathContainer = pathContainer.GetWithRange(range);
            pathContainer.CutWalls(mapGrid);
            if (position.Equals(desiredTarget))
            {
                pathContainer.Clear();
                pathContainer.Path.Add(desiredTarget);
            }
            user.ChangeDirection(desiredTarget, true, true, false);
            user.CreatureData.EffectsController.PropagateAction(PlayerActionHappened.HandAction);
            user.ThrowGrenade(pathContainer, grenadeItem, false);
            return false;
        }
        */


    }
}
