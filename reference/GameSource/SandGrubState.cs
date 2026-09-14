public class SandGrubState : HealthState
{
	public int origRoom;

	public int placedObjectIndex;

	public SandGrubState(AbstractCreature creature)
		: base(creature)
	{
		origRoom = -1;
		placedObjectIndex = -1;
	}
}
