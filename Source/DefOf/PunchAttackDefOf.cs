using RimWorld;
using Verse;

namespace PunchAttack
{
	[DefOf]
	public static class PunchAttackDefOf
	{
		public static JobDef Fuu_PunchAttack;

		static PunchAttackDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(PunchAttackDefOf));
		}
	}

}
