

using HarmonyLib;
using MGSC;
using System.Collections.Generic;

namespace ThrowRangeWorkOnGrenade
{

    [HarmonyPatch(typeof(FirearmSystem), nameof(FirearmSystem.ThrowGrenade))]
    public class ThrowGrenadeFix
    {
        static bool Prefix(MapGrid mapGrid, MapRenderer mapRenderer, Creature user, BallisticPath pathContainer, BasePickupItem grenadeItem, CellPosition desiredTarget)
        {
            pathContainer.Clear();
            CellPosition position = user.CreatureData.Position;
            GrenadeRecord grenadeRecord = grenadeItem.Record<GrenadeRecord>();
            int num = grenadeRecord.Range + 1;
            int num2 = num + grenadeRecord.MaxOverthrowDistance;
            if (grenadeRecord.RicochetTrajectory)
            {
                pathContainer = TrajectoryCalculator.CalculateWayWithRicochets(mapGrid, mapRenderer, pathContainer, position, desiredTarget, false, 0f, num2, -1, false);
            }
            else
            {
                pathContainer = TrajectoryCalculator.CalculateWayWithRicochets(mapGrid, mapRenderer, pathContainer, position, desiredTarget, false, 0f, num2, 0, false);
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



    }
}
