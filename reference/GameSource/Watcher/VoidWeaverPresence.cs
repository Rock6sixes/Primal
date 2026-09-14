using System.Collections.Generic;

namespace Watcher;

public class VoidWeaverPresence : World.IMigrationInfluence
{
	public World world;

	public AbstractRoom weaverRoom;

	public List<AbstractRoom> presenceRooms = new List<AbstractRoom>();

	public string songName = "RWTW_WeaverAura";

	public VoidWeaverPresence(World world, AbstractRoom weaverRoom)
	{
		this.world = world;
		this.weaverRoom = weaverRoom;
	}

	public float AttractionValueForCreature(AbstractRoom room, CreatureTemplate.Type tp, float defValue)
	{
		if (room == weaverRoom)
		{
			return 0f;
		}
		return defValue;
	}

	public float AttractionValueForCreature(AbstractRoom room, string namedAttr, float defValue)
	{
		if (room == weaverRoom)
		{
			return 0f;
		}
		return defValue;
	}

	public float AttractionValueForCreature(AbstractRoom room, AbstractCreature creature, float defValue)
	{
		if (room == weaverRoom)
		{
			return 0f;
		}
		return defValue;
	}
}
