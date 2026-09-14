using RWCustom;
using UnityEngine;

namespace Watcher;

public class PrinceSpasmer : UpdatableAndDeletable
{
	public Creature crit;

	public bool leftSpasmStarted;

	public bool rightSpasmStarted;

	public int leftBound;

	public int rightBound;

	public bool useLeftBound;

	public bool useRightBound;

	public PrinceSpasmer(Creature crit, bool useLeftBound, int leftBound, bool useRightBound, int rightBound)
	{
		this.crit = crit;
		this.useLeftBound = useLeftBound;
		this.useRightBound = useRightBound;
		this.leftBound = leftBound;
		this.rightBound = rightBound;
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (crit is Player)
		{
			Player player = crit as Player;
			player.warpExhausionTime = Mathf.Max(200, player.warpExhausionTime);
		}
		Vector2? vector = null;
		if (useLeftBound && crit.mainBodyChunk.pos.x < (float)leftBound)
		{
			vector = Custom.RNV();
			vector = new Vector2(Mathf.Min(0f - Mathf.Abs(vector.Value.x), -0.5f), vector.Value.y);
			leftSpasmStarted = true;
			crit.Stun(40);
		}
		else if (useRightBound && crit.mainBodyChunk.pos.x > (float)rightBound)
		{
			vector = Custom.RNV();
			vector = new Vector2(Mathf.Max(Mathf.Abs(vector.Value.x), 0.5f), vector.Value.y);
			rightSpasmStarted = true;
			crit.Stun(40);
		}
		else if (leftSpasmStarted)
		{
			vector = Custom.RNV();
			vector = new Vector2(Mathf.Min(0f - Mathf.Abs(vector.Value.x), -0.5f), vector.Value.y);
			crit.Stun(40);
			if (Random.value < 0.025f)
			{
				leftSpasmStarted = false;
			}
		}
		else if (rightSpasmStarted)
		{
			vector = Custom.RNV();
			vector = new Vector2(Mathf.Max(Mathf.Abs(vector.Value.x), 0.5f), vector.Value.y);
			crit.Stun(40);
			if (Random.value < 0.025f)
			{
				rightSpasmStarted = false;
			}
		}
		if (!vector.HasValue)
		{
			return;
		}
		for (int i = 0; i < crit.bodyChunks.Length; i++)
		{
			vector = Vector3.Slerp(-vector.Value.normalized, Custom.RNV(), Random.value);
			vector *= Mathf.Min(3f, Random.value * 3f / Mathf.Lerp(crit.bodyChunks[i].mass, 1f, 0.5f));
			crit.bodyChunks[i].pos += vector.Value;
			crit.bodyChunks[i].vel += vector.Value * 0.5f;
		}
		if (crit.graphicsModule == null || crit.graphicsModule.bodyParts == null)
		{
			return;
		}
		for (int j = 0; j < crit.graphicsModule.bodyParts.Length; j++)
		{
			vector = Vector3.Slerp(-vector.Value.normalized, Custom.RNV(), Random.value);
			vector *= Random.value * 2f;
			crit.graphicsModule.bodyParts[j].pos += vector.Value;
			crit.graphicsModule.bodyParts[j].vel += vector.Value;
			if (crit.graphicsModule.bodyParts[j] is Limb)
			{
				(crit.graphicsModule.bodyParts[j] as Limb).mode = Limb.Mode.Dangle;
			}
		}
	}
}
