using System;

namespace MoreSlugcats;

public interface ISpecialWarp
{
	Room getSourceRoom();

	[Obsolete("Use room parameter function instead.")]
	void NewWorldLoaded();

	void NewWorldLoaded(Room newRoom);
}
