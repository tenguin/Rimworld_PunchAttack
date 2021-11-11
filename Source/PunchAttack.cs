using HarmonyLib;
using Verse;

namespace PunchAttack
{
	public class PunchAttack : Mod
	{
		public PunchAttack(ModContentPack content) : base(content)
		{
			Harmony harmony = new Harmony(content.PackageId);
			harmony.PatchAll();
		}
	}
}
