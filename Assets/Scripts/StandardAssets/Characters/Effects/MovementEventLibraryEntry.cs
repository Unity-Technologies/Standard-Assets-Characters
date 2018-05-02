using System;
using Boo.Lang;

namespace StandardAssets.Characters.Effects
{
	[Serializable]
	public class MovementEventLibraryEntry
	{
		public string id;

		public List<MovementEventPlayer> movementEventPlayers;
	}
}