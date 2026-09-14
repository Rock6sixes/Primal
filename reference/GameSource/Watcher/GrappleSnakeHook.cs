using RWCustom;
using UnityEngine;

namespace Watcher;

public class GrappleSnakeHook
{
	public GrappleSnake snake;

	public GrappleSnakeBodyChunk body;

	public float maxDistance;

	public float angularOffset;

	public float lastAttachmentFactor;

	public float attachmentFactor;

	public Vector2 attachmentFactorPoint = Vector2.zero;

	public bool retracting;

	public bool fullyAttached;

	public Vector2 direction => Custom.DegToVec(Custom.VecToDeg(body.Rotation) + angularOffset);

	public Vector2 position => body.pos + direction * body.rad;

	public Vector2? attachmentPoint { get; set; }

	public Vector2? attachmentDirection
	{
		get
		{
			if (attachmentPoint.HasValue)
			{
				return (attachmentPoint.Value - position).normalized;
			}
			return null;
		}
	}

	public bool hooked { get; set; }

	public GrappleSnakeHook(GrappleSnake snake, GrappleSnakeBodyChunk body, float maxDistance, float angularOffset)
	{
		this.snake = snake;
		this.body = body;
		this.maxDistance = maxDistance;
		this.angularOffset = angularOffset;
	}

	public Vector2? Raycast()
	{
		return SharedPhysics.ExactTerrainRayTracePos(snake.room, position, position + direction * maxDistance);
	}

	public virtual void Attach(Vector2 position)
	{
		attachmentPoint = position;
		hooked = true;
		body.hookedCount++;
	}

	public void Detach()
	{
		attachmentPoint = null;
		hooked = false;
		body.hookedCount--;
		retracting = true;
	}

	public virtual Vector2 GetForce()
	{
		if (!attachmentPoint.HasValue)
		{
			return Vector2.zero;
		}
		Vector2 result = (attachmentPoint.Value - position).normalized * snake.grappleSnakeHookStrength;
		float num = Vector2.Dot(snake.facingDir, Vector2.up);
		if ((double)Mathf.Abs(num) > 0.5)
		{
			float num2 = (((double)num > 0.0) ? snake.grappleSnakeGravityCounterStrengthUp : snake.grappleSnakeGravityCounterStrengthDown);
			float num3 = (float)snake.bodyChunks.Length * snake.gravity * (1f / (float)body.hooks.Count) * num2;
			result += Vector2.up * num3;
		}
		return result;
	}

	public virtual void Update()
	{
		float num = 0.4f;
		int num2 = 5;
		if (body.hookedCount > num2)
		{
			num = 0.7f;
		}
		if (hooked)
		{
			body.vel += GetForce();
			if (Vector2.Dot(snake.facingDir, attachmentDirection.Value) < num)
			{
				Detach();
			}
		}
		else
		{
			if (body.hookedCount > num2 || Vector2.Dot(snake.facingDir, direction) < num)
			{
				return;
			}
			Vector2? vector = Raycast();
			if (vector.HasValue)
			{
				Attach(vector.Value);
			}
		}
		updateAttachmentFactor();
	}

	public void updateAttachmentFactor()
	{
		float num = 0.2f;
		lastAttachmentFactor = attachmentFactor;
		if (!retracting)
		{
			if (hooked)
			{
				attachmentFactor += num;
				if (attachmentFactor >= 1f && !fullyAttached)
				{
					fullyAttached = true;
				}
			}
			if (attachmentPoint.HasValue)
			{
				attachmentFactorPoint = attachmentPoint.Value;
			}
		}
		else
		{
			attachmentFactor -= num;
			fullyAttached = false;
			if (attachmentFactor <= 0f)
			{
				retracting = false;
			}
		}
		attachmentFactor = Mathf.Clamp(attachmentFactor, 0f, 1f);
	}

	public Vector2 GetSmoothedPosition(float partialTick)
	{
		return Vector2.Lerp(body.lastPos, body.pos, partialTick) + direction * body.rad;
	}

	public float GetAttachmentFactor(float partialTick)
	{
		return Mathf.Lerp(lastAttachmentFactor, attachmentFactor, partialTick);
	}
}
