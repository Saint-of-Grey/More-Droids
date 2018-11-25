using System;
using System.Collections.Generic;
using Harmony;
using RimWorld;
using Verse;
using System.Reflection;
using System.Reflection.Emit;
using RoboticEqaulity;

//would you mind writing a small transpiler for him?
//sadly not near an IDE atm

//callvirt Verse.RaceProperties::get_IsFlesh()

//replaced with

//call YourClass::IsFleshFix(RaceProperties)


//then place his code inside of

//public static bool IsFleshFix(RaceProperties raceProps)


//then he can place the mod extension on his custom flesh type def, and be done with it
//transpiler doesn't need any other conditions, just replace that call with another one

namespace TranspilerForPawnCapacityDefReporting
{
	[StaticConstructorOnStartup]
	public static class Class1
	{
		static Class1()
		{
			HarmonyInstance harmony = HarmonyInstance.Create("mehni.rimworld.TranspilerForPawnCapacityDefReporting");
			harmony.Patch(AccessTools.Method(typeof(HealthCardUtility), "DrawOverviewTab"),
				transpiler: new HarmonyMethod(typeof(Class1), nameof(TranspilerForPawnCapacityDefReporting)));
		}

		private static IEnumerable<CodeInstruction> TranspilerForPawnCapacityDefReporting(IEnumerable<CodeInstruction> instructions)
		{
			foreach (CodeInstruction codeInstruction in instructions)
			{
				if (codeInstruction.opcode == OpCodes.Callvirt && codeInstruction.operand == (AccessTools.Property(typeof(RaceProperties), nameof(RaceProperties.IsFlesh)).GetGetMethod()))
				{
					codeInstruction.opcode = OpCodes.Call;
					codeInstruction.operand = AccessTools.Method(typeof(Class1), nameof(IsFleshFix));
				}
				//Log.Message("It did a thing");
				yield return codeInstruction;
			}
		}

		public static bool IsFleshFix(RaceProperties raceProps)
		{
			if (raceProps.FleshType.HasModExtension<MechanicalCheck>())
			{
				return (raceProps.FleshType.GetModExtension<MechanicalCheck>()?.isMechanical ?? false);
			}
			else
			{
				return raceProps.FleshType != FleshTypeDefOf.Mechanoid;
			}
		}
	}
}