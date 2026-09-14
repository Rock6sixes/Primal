using Watcher;

public struct AbstractRoomNode
{
	public class Type : ExtEnum<Type>
	{
		public static Type Exit = new Type("Exit", register: true);

		public static Type Den = new Type("Den", register: true);

		public static Type RegionTransportation = new Type("RegionTransportation", register: true);

		public static Type SideExit = new Type("SideExit", register: true);

		public static Type SkyExit = new Type("SkyExit", register: true);

		public static Type SeaExit = new Type("SeaExit", register: true);

		public static Type BatHive = new Type("BatHive", register: true);

		public static Type GarbageHoles = new Type("GarbageHoles", register: true);

		public Type(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public Type type;

	public bool borderExit;

	public int shortCutLength;

	public int[,,] connectivity;

	public bool submerged;

	public int viewedByCamera;

	public int entranceWidth;

	public AbstractRoomNode(Type type, int shortCutLength, int totalNumberOfNodes, bool submerged, int viewedByCamera, int entranceWidth)
	{
		this.type = type;
		this.shortCutLength = shortCutLength;
		this.submerged = submerged;
		this.viewedByCamera = viewedByCamera;
		this.entranceWidth = entranceWidth;
		connectivity = new int[StaticWorld.preBakedPathingCreatures.Length, totalNumberOfNodes, 2];
		borderExit = type == Type.SideExit || type == Type.SkyExit || type == Type.SeaExit;
	}

	public int ConnectionCost(int otherNode, CreatureTemplate creatureType)
	{
		if (creatureType.PreBakedPathingIndex == -1 || otherNode < 0)
		{
			return -1;
		}
		int num = connectivity[creatureType.PreBakedPathingIndex, otherNode, 0];
		if (num == -1 && creatureType.type == WatcherEnums.CreatureTemplateType.RippleSpider)
		{
			num = connectivity[StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Salamander).PreBakedPathingIndex, otherNode, 0];
		}
		return num;
	}

	public int ConnectionLength(int otherNode, CreatureTemplate creatureType)
	{
		if (creatureType.PreBakedPathingIndex == -1 || otherNode < 0)
		{
			return -1;
		}
		int num = connectivity[creatureType.PreBakedPathingIndex, otherNode, 1];
		if (num == -1 && creatureType.type == WatcherEnums.CreatureTemplateType.RippleSpider)
		{
			num = connectivity[StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Salamander).PreBakedPathingIndex, otherNode, 1];
		}
		return num;
	}
}
