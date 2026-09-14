public class OracleProjectionScreen : ProjectionScreen
{
	public ProjectedImage desktop;

	public ProjectedImage[] horizons;

	public OracleProjectionScreen(Room room, OracleBehavior oracleBehav)
		: base(room)
	{
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
	}
}
