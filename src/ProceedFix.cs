

using HarmonyLib;
using MGSC;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace ThrowRangeWorkOnGrenade
{

    [HarmonyPatch(typeof(SelectGrenadeTarget), nameof(SelectGrenadeTarget.Process))]
    public class ProcessFix
    {

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            bool found = false;

            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Callvirt && codes[i].operand.ToString().Contains("get_Range"))
                {
                    // We inject immediately after get_Range returns the base range to the stack
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

                    codes.InsertRange(i + 1, injection);
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                Plugin.Logger.LogError("Could not find patch location in SelectGrenadeTarget.Process!");
            }

            return codes.AsEnumerable();
        }

        /*
        static bool Prefix(ref SelectGrenadeTarget __instance, out bool interruptProcessing)
        {
            interruptProcessing = true;
            __instance._view.FreeBorders();
            __instance._view.FreeHitHints();
            __instance._view.HideSingleCell();
            InputController instance = SingletonMonoBehaviour<InputController>.Instance;
            CellPosition cellUnderCursor = __instance._mapRenderer.GetCellUnderCursor();
            if (instance.Mode != InputController.InputMode.KeyboardAndMouse)
            {
                int num;
                CellPosition.ChangePositionByDir(InputHelper.GetDirectionFromAxis(instance.GetAxis("UI", "Movement", out num, SingletonMonoBehaviour<GameSettings>.Instance.KeyThreshold, __instance._keyThresholdTimer)), ref __instance._x, ref __instance._y);
                cellUnderCursor = new CellPosition(__instance._x, __instance._y);
                PlayerInteractionSystem.ProcessCameraInput(instance, __instance._gameCamera);
            }
            else
            {
                __instance._x = cellUnderCursor.X;
                __instance._y = cellUnderCursor.Y;
            }
            if (instance.IsKeyDown("PrimaryCursorAction", null, false) || instance.IsKeyDown("Confirm", "UI", false))
            {
                __instance.ProceedTarget(new CellPosition(__instance._x, __instance._y));
                return false;
            }
            if (instance.IsKeyDown("SecondaryCursorAction", null, false) || instance.IsKeyDown("Back", "UI", false) || instance.IsKeyDown("Menu", "UI", false))
            {
                UI.Hide<SelectGrenadeTarget>();
                return false;
            }
            __instance._creatures.Player.ChangeDirection(cellUnderCursor, true, true, false);
            CellPosition position = __instance._creatures.Player.CreatureData.Position;
            GrenadeRecord grenadeRecord = __instance._creatures.Player.CreatureData.Inventory.VestStore.GetItemByIndex(__instance._slotIndex - 1).Record<GrenadeRecord>();
            ExplosionRecord record = Data.Explosions.GetRecord(grenadeRecord.Explosion, true);
            int range = grenadeRecord.Range + __instance._creatures.Player.CreatureData.GetMeleeThrowRangeBonus();
            if (grenadeRecord.RicochetTrajectory)
            {
                __instance._ballisticPath = TrajectoryCalculator.CalculateWayWithRicochets(__instance._mapGrid, __instance._mapRenderer, __instance._mapObstacles, __instance._ballisticPath, position, cellUnderCursor, false, 0f, range, -1, false, true);
            }
            else
            {
                __instance._ballisticPath = TrajectoryCalculator.CalculateWayWithRicochets(__instance._mapGrid, __instance._mapRenderer, __instance._mapObstacles, __instance._ballisticPath, position, cellUnderCursor, false, 0f, range, 0, false, true);
            }
            __instance._ballisticPath = __instance._ballisticPath.GetStoppedAtCellWithoutRicochet(cellUnderCursor);
            __instance._view.ShowGrenadeTrajectory(__instance._ballisticPath);
            SelectTargetView view = __instance._view;
            CellPosition center;
            if (__instance._ballisticPath.Path.Count <= 0)
            {
                center = cellUnderCursor;
            }
            else
            {
                List<CellPosition> path = __instance._ballisticPath.Path;
                center = path[path.Count - 1];
            }
            view.ShowExplosionArea(center, record.Parameters.Radius);

            return false;
        }
        */
    }
}
