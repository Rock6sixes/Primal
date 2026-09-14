namespace Watcher;

public class GrappleSnakeAI : ArtificialIntelligence, IUseARelationshipTracker
{
	public class Behavior : ExtEnum<Behavior>
	{
		public static Behavior Idle = new Behavior("Idle", register: true);

		public static Behavior Hunt = new Behavior("Hunt", register: true);

		public static Behavior EscapeRain = new Behavior("EscapeRain", register: true);

		public Behavior(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public DebugDestinationVisualizer debugDestinationVisualizer;

	public Behavior behavior;

	public GrappleSnake snake => creature.realizedCreature as GrappleSnake;

	public GrappleSnakeAI(AbstractCreature abstractCreature, World world)
		: base(abstractCreature, world)
	{
		snake.AI = this;
		AddModule(new StandardPather(this, world, creature));
		AddModule(new Tracker(this, 10, 5, 250, 0.5f, 5, 5, 20));
		AddModule(new PreyTracker(this, 5, 1f, 5f, 150f, 0.05f));
		AddModule(new ThreatTracker(this, 3));
		AddModule(new RainTracker(this));
		AddModule(new DenFinder(this, creature));
		AddModule(new RelationshipTracker(this, base.tracker));
		AddModule(new UtilityComparer(this));
		base.utilityComparer.AddComparedModule(base.threatTracker, null, 1f, 1.1f);
		base.utilityComparer.AddComparedModule(base.preyTracker, null, 0.9f, 2f);
		base.utilityComparer.AddComparedModule(base.rainTracker, null, 1f, 1.1f);
	}

	public override void Update()
	{
	}

	public RelationshipTracker.TrackedCreatureState CreateTrackedCreatureState(RelationshipTracker.DynamicRelationship rel)
	{
		return null;
	}

	public AIModule ModuleToTrackRelationship(CreatureTemplate.Relationship relationship)
	{
		return null;
	}

	public CreatureTemplate.Relationship UpdateDynamicRelationship(RelationshipTracker.DynamicRelationship dRelation)
	{
		return StaticRelationship(dRelation.trackerRep.representedCreature);
	}
}
