using System;
using UnityEngine;

namespace Watcher;

public class StabilizerGrappleSnakeHook : GrappleSnakeHook
{
	public float attachDistance;

	public Vector2 desiredPosition => snake.firstChunk.pos + snake.smoothedFacingDir * (0f - body.idealDistanceFromHead) + base.direction * body.rad;

	public StabilizerGrappleSnakeHook(GrappleSnake snake, GrappleSnakeBodyChunk body, float maxDistance, float angularOffset)
		: base(snake, body, maxDistance, angularOffset)
	{
	}

	public override Vector2 GetForce()
	{
		if (!base.attachmentPoint.HasValue)
		{
			return Vector2.zero;
		}
		Vector2 vector = (desiredPosition - base.position) * snake.grappleSnakeStablizerHookStrength * Mathf.Pow(body.distanceFactor, 2f);
		if (base.attachmentDirection.HasValue)
		{
			vector *= Mathf.Max(Vector2.Dot(vector.normalized, base.attachmentDirection.Value) * 2f, 1f);
		}
		Vector2 vector2 = Vector2.up * snake.gravity * (1 / body.hooks.Count);
		Vector2 vector3 = body.vel * -0.05f;
		return vector + vector2 + vector3;
	}

	public override void Attach(Vector2 position)
	{
		base.Attach(position);
		attachDistance = Vector2.Distance(position, base.position);
	}

	public override void Update()
	{
		Vector2 vector = desiredPosition - base.position;
		float num = Vector2.Dot(vector.normalized, base.direction);
		if (base.hooked)
		{
			body.vel += GetForce();
			if ((double)Vector2.Distance(base.attachmentPoint.Value, base.position) > (double)attachDistance * 1.5)
			{
				Detach();
			}
			if (Math.Abs(Vector2.Dot(base.direction, snake.facingDir)) > 0.5f)
			{
				return;
			}
			if (base.attachmentDirection.HasValue && Vector2.Dot(vector.normalized, base.attachmentDirection.Value) < -0.9f)
			{
				Detach();
			}
		}
		else
		{
			if (!(Vector2.Dot(base.direction, Vector2.up) > 0.5f) && !(Math.Abs(Vector2.Dot(base.direction, snake.facingDir)) > 0.3f) && (num < 0f || vector.magnitude < 60f))
			{
				return;
			}
			Vector2? vector2 = Raycast();
			if (vector2.HasValue)
			{
				Attach(vector2.Value);
			}
		}
		updateAttachmentFactor();
	}
}
