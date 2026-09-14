namespace Watcher;

public class MillipedeLimb : Limb
{
	public int limbSide;

	public MillipedeLimb legToTrigger;

	public int triggerLegCounter;

	public MillipedeGraphics graphics => owner as MillipedeGraphics;

	public MillipedeLimb(GraphicsModule owner, BodyChunk connectionChunk, int num, float rad, float sfFric, float aFric, float huntSpeed, float quickness, int limbSide)
		: base(owner, connectionChunk, num, rad, sfFric, aFric, huntSpeed, quickness)
	{
		this.limbSide = limbSide;
	}

	public override void Update()
	{
		base.Update();
		if (triggerLegCounter > 0)
		{
			triggerLegCounter--;
			if (triggerLegCounter == 0)
			{
				graphics.FindGrip(legToTrigger.limbNumber, legToTrigger.limbSide);
			}
		}
	}

	public override void GrabbedTerrain()
	{
		if (triggerLegCounter == 0 && legToTrigger != null)
		{
			triggerLegCounter = 5;
		}
	}
}
