﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;
using TranspilerForPawnCapacityDefReporting;
using Androids;

namespace RoboticEqaulity
{
	public class MechanicalCheck : DefModExtension
	{
		public bool isMechanical = false;

		public MechanicalCheck()
		{
		}
	}

	[StaticConstructorOnStartup]
    public static class RobotPatch  
    {
		static RobotPatch()
        {
            Harmony harmony = new Harmony(id: "Grey.Robotic.Equality");
			//HarmonyInstance.DEBUG = true;
            harmony.Patch(original: AccessTools.Method(type: typeof(PawnCapacityDef), name: nameof(PawnCapacityDef.GetLabelFor), parameters: new Type[] { typeof(Pawn) }), 
                prefix: null, postfix: new HarmonyMethod(methodType: typeof(RobotPatch), methodName: nameof(MakeRobotic)));
			harmony.Patch(original: AccessTools.Method(type: typeof(PawnCapacityDef), name: nameof(PawnCapacityDef.GetLabelFor), parameters: new Type[] { typeof(bool), typeof(bool) }), 
				prefix: null, postfix: new HarmonyMethod(methodType: typeof(RobotPatch), methodName: nameof(ISaidMakeRoboticDammit)));
			harmony.Patch(original: AccessTools.Method(type: typeof(KidnapAIUtility), name: nameof(KidnapAIUtility.TryFindGoodKidnapVictim)/*, parameters: new Type[] { typeof(Pawn), typeof(float), typeof(Pawn), typeof(List<Thing>) }*/),
				prefix: null, postfix: null, transpiler: new HarmonyMethod(methodType: typeof(RobotPatch), methodName: nameof(PlzDontKidnapRobots)));
		}

        private static void MakeRobotic(PawnCapacityDef __instance, Pawn pawn, ref string __result)
        {
			if (pawn.RaceProps.FleshType.GetModExtension<MechanicalCheck>()?.isMechanical ?? false)
			{
				if (!__instance.labelMechanoids.NullOrEmpty())
				{
					__result = __instance.labelMechanoids;
				}
			}
		}
		private static void ISaidMakeRoboticDammit(PawnCapacityDef __instance, bool isFlesh , bool isHumanlike, ref string __result)
		{
			if (!isFlesh && isHumanlike)
			{
				if (!__instance.labelMechanoids.NullOrEmpty())
				{
					__result = __instance.labelMechanoids;
				}
			}
		}

		private static IEnumerable<CodeInstruction> PlzDontKidnapRobots(IEnumerable<CodeInstruction> instructions, ILGenerator ilg)
		{
			foreach (var instruction in instructions)
				if (instruction.operand == AccessTools.Method(typeof(GenClosest), nameof(GenClosest.ClosestThingReachable)))
				{
					//Log.Message("It did a thing");
					yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(RobotPatch), nameof(RobotPatch.ClosestThingReachable)));
				}
				else
				{
					yield return instruction;
				}
		}

		public static Thing ClosestThingReachable(IntVec3 root, Map map, ThingRequest thingReq, PathEndMode peMode, TraverseParms traverseParams, 
			float maxDistance = 9999f, Predicate<Thing> validator = null, IEnumerable<Thing> customGlobalSearchSet = null, int searchRegionsMin = 0, int searchRegionMax = -1, 
			bool forceGlobalSearch = false, RegionType traversableRegionTypes = RegionType.Set_Passable, bool ignoreEntirelyForbiddenRegions = false)
		{
			Predicate<Thing> noRobotValidator = delegate (Thing t)
			{
				Pawn pawn = t as Pawn;
				//Log.Message(pawn.ToString() + " returned as " + (!(pawn.RaceProps.FleshType.GetModExtension<MechanicalCheck>()?.isMechanical ?? false)).ToString());
				return !(pawn.RaceProps.FleshType.GetModExtension<MechanicalCheck>()?.isMechanical ?? false) && validator(t);
			};
			return GenClosest.ClosestThingReachable(root, map, thingReq, peMode, traverseParams, maxDistance, noRobotValidator, null, 0, -1, false, RegionType.Set_Passable, false);
		}
	}
}
