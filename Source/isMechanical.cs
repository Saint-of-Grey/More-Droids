using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Harmony;
using RimWorld;
using Verse;
using TranspilerForPawnCapacityDefReporting;

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
            HarmonyInstance harmony = HarmonyInstance.Create(id: "Grey.Robotic.Equality");
            
            harmony.Patch(original: AccessTools.Method(type: typeof(PawnCapacityDef), name: nameof(PawnCapacityDef.GetLabelFor), parameters: new Type[] { typeof(Pawn) }), 
                prefix: null, postfix: new HarmonyMethod(type: typeof(RobotPatch), name: nameof(MakeRobotic)));
			harmony.Patch(original: AccessTools.Method(type: typeof(PawnCapacityDef), name: nameof(PawnCapacityDef.GetLabelFor), parameters: new Type[] { typeof(bool), typeof(bool) }), 
				prefix: null, postfix: new HarmonyMethod(type: typeof(RobotPatch), name: nameof(ISaidMakeRoboticDammit)));
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
	}
}
