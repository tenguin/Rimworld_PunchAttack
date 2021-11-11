using UnityEngine;
using Verse;

namespace PunchAttack
{
    [StaticConstructorOnStartup]
    public static class Assets
	{
		public static readonly Texture2D PunchAttack = ContentFinder<Texture2D>.Get("UI/Commands/PunchAttack");

		public static readonly string DefaultLabel = "PunchToggle_ButtonLabel".Translate();
		public static readonly string DefaultDesc = "PunchToggle_ButtonDesc".Translate();
	}
}
