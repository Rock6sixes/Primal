using System.Collections.Generic;
using UnityEngine;

namespace Watcher;

public class GrappleSnakeBodyChunk : BodyChunk
{
	public float idealDistanceFromHead;

	public float distanceFactor = 1f;

	public int hookedCount;

	public GrappleSnake snake => base.owner as GrappleSnake;

	public List<GrappleSnakeHook> hooks { get; set; }

	public GrappleSnakeBodyChunk(GrappleSnake owner, int index, Vector2 pos, float rad, float mass, float distanceFactor)
		: base(owner, index, pos, rad, mass)
	{
		hooks = new List<GrappleSnakeHook>();
		this.distanceFactor = distanceFactor;
	}

	public void AddHook(GrappleSnakeHook hook)
	{
		hooks.Add(hook);
		snake.hooks.Add(hook);
	}
}
