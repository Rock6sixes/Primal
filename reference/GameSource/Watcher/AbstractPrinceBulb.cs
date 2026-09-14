using System.Globalization;
using UnityEngine;

namespace Watcher;

public class AbstractPrinceBulb : AbstractPhysicalObject
{
	public static int maxGrowthStage = 3;

	public int growthStage;

	public Vector2 direction;

	public float radius;

	public AbstractPrinceBulb(World world, PhysicalObject realizedObject, WorldCoordinate pos, EntityID ID, int growthStage, Vector2 direction, float radius)
		: base(world, WatcherEnums.AbstractObjectType.PrinceBulb, realizedObject, pos, ID)
	{
		this.growthStage = growthStage;
		this.direction = direction;
		this.radius = radius;
	}

	public static AbstractPrinceBulb FromStrings(World world, PhysicalObject realizedObject, WorldCoordinate pos, EntityID ID, string[] s)
	{
		int num = int.Parse(s[3], NumberStyles.Any, CultureInfo.InvariantCulture);
		Vector2 vector = default(Vector2);
		vector.x = float.Parse(s[4], NumberStyles.Any, CultureInfo.InvariantCulture);
		vector.y = float.Parse(s[5], NumberStyles.Any, CultureInfo.InvariantCulture);
		float num2 = float.Parse(s[6], NumberStyles.Any, CultureInfo.InvariantCulture);
		return new AbstractPrinceBulb(world, realizedObject, pos, ID, num, vector, num2)
		{
			unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(s, 7)
		};
	}

	public override string ToString()
	{
		string baseString = string.Format(CultureInfo.InvariantCulture, "{0}<oA>{1}<oA>{2}<oA>{3}<oA>{4}<oA>{5}<oA>{6}", base.IDAndRippleLayerString, type.ToString(), pos.SaveToString(), growthStage, direction.x, direction.y, radius);
		baseString = SaveState.SetCustomData(this, baseString);
		return SaveUtils.AppendUnrecognizedStringAttrs(baseString, "<oA>", unrecognizedAttributes);
	}
}
