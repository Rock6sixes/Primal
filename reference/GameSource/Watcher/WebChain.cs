using System.Collections.Generic;
using UnityEngine;

namespace Watcher;

public class WebChain : UpdatableAndDeletable
{
	public List<CosmicWeb> cosmicWebs = new List<CosmicWeb>();

	public override void Update(bool eu)
	{
		for (int i = 0; i < cosmicWebs.Count; i++)
		{
			CosmicWeb cosmicWeb = cosmicWebs[i];
			float num = Mathf.Sin((float)room.game.clock / 40f + cosmicWeb.randoSeed + (float)i);
			Vector2 originalStuckAtAPos = cosmicWeb.originalStuckAtAPos;
			Vector2 vector = originalStuckAtAPos + new Vector2(num, num) * (25f * Mathf.Min(1f, (float)cosmicWeb.age / 40f));
			if (cosmicWeb.room.aimap.getTerrainProximity(cosmicWeb.stuckPosA.Value) < 3)
			{
				cosmicWeb.stuckPosA = SharedPhysics.ExactTerrainRayTracePos(room, originalStuckAtAPos, vector) ?? vector;
			}
			else
			{
				cosmicWeb.stuckPosA = vector;
			}
			if (i - 1 > 0)
			{
				cosmicWeb.stuckPosB = cosmicWebs[i - 1].stuckPosA;
			}
		}
	}
}
