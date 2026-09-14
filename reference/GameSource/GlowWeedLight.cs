using UnityEngine;

public class GlowWeedLight : LightFixture
{
	public LightSource lightSource;

	public Color color = new Color(0.8f, 1f, 0.4f);

	public GlowWeedLight(Room placedInRoom, PlacedObject placedObject, PlacedObject.LightFixtureData lightData)
		: base(placedInRoom, placedObject, lightData)
	{
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (lightSource == null || lightSource.room != room)
		{
			lightSource = new LightSource(placedObject.pos, environmentalLight: true, color, this);
			room.AddObject(lightSource);
			lightSource.colorFromEnvironment = false;
			lightSource.stayAlive = true;
			lightSource.requireUpKeep = true;
			lightSource.submersible = true;
		}
		float num = (float)lightData.randomSeed / 100f;
		lightSource.setPos = placedObject.pos;
		lightSource.setAlpha = 0.8f;
		lightSource.color = color;
		lightSource.setRad = num * 500f;
		lightSource.stayAlive = true;
	}
}
