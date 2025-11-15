

using HarmonyLib;
using MGSC;

namespace ThrowRangeWorkOnGrenade
{

    [HarmonyPatch(typeof(SelectGrenadeTarget), nameof(SelectGrenadeTarget.ProceedTarget))]
    public class ProceedTargetFix
    {
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
                __instance._ballisticPath = TrajectoryCalculator.CalculateWayWithRicochets(__instance._mapGrid, __instance._mapRenderer, __instance._ballisticPath, position, cellPosition, false, 0f, num2, -1, false);
            }
            else
            {
                __instance._ballisticPath = TrajectoryCalculator.CalculateWayWithRicochets(__instance._mapGrid, __instance._mapRenderer, __instance._ballisticPath, position, cellPosition, false, 0f, num2, 0, false);
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

    }
}
