using RWCustom;
using UnityEngine;

namespace Watcher;

public class DaemonEye : UpdatableAndDeletable, IDrawable
{
	public PlacedObject placedObj;

	public Vector2 position;

	public float size;

	public float depth;

	public bool active;

	public float lastEyeOpen;

	public float eyeOpen;

	public bool activationRangeReached;

	public bool randomShutOff;

	public float activationRange = 450f;

	public float variation;

	public Counter activationCounter;

	public Vector2 randomLookPosition;

	public Counter targetSwitchCounter = new Counter(200);

	public Counter lockOntoTargetCounter;

	public Vector2 direction;

	public Vector2 lastDirection;

	public bool eyeShouldBeOn
	{
		get
		{
			if (active && !randomShutOff)
			{
				return activationRangeReached;
			}
			return false;
		}
	}

	public DaemonEye(PlacedObject placedObj, Room room, Vector2 position, float size, float depth)
	{
		this.placedObj = placedObj;
		base.room = room;
		this.position = position;
		this.size = size;
		this.depth = depth;
		direction = (lastDirection = Vector2.zero);
		variation = Random.value;
		activationCounter = ActivationCounter();
		active = true;
	}

	public DaemonEye(Room room, Vector2 position, float size, float depth)
	{
		DaemonEyeData daemonEyeData = new DaemonEyeData(null);
		placedObj = new PlacedObject(WatcherEnums.PlacedObjectType.DaemonEye, daemonEyeData);
		daemonEyeData.owner = placedObj;
		base.room = room;
		placedObj.pos = position;
		daemonEyeData.handlePos = Vector2.left * size;
		daemonEyeData.depth = (int)depth;
		this.position = position;
		this.size = size;
		this.depth = depth;
		variation = Random.value;
		activationCounter = ActivationCounter();
		direction = (lastDirection = Vector2.zero);
		active = true;
	}

	public override void Update(bool eu)
	{
		if (lockOntoTargetCounter == null)
		{
			lockOntoTargetCounter = new Counter(Random.Range(100, 250));
			lockOntoTargetCounter.Finish();
		}
		lockOntoTargetCounter.Tick();
		lastDirection = direction;
		DaemonEyeData daemonEyeData = placedObj.data as DaemonEyeData;
		position = placedObj.pos;
		depth = daemonEyeData.depth;
		size = daemonEyeData.Rad;
		if (room.PlayersInRoom.Count != 0 && room.PlayersInRoom[0]?.mainBodyChunk != null)
		{
			CheckIfNeighboursActivated();
			LookAtTarget();
			AnimateEyeActivation();
		}
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[1];
		sLeaser.sprites[0] = new FSprite("Futile_White");
		FSprite obj = sLeaser.sprites[0];
		float width = (sLeaser.sprites[0].height = size * 2f);
		obj.width = width;
		sLeaser.sprites[0].shader = rCam.room.game.rainWorld.Shaders["DaemonEye"];
		AddToContainer(sLeaser, rCam, null);
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		sLeaser.sprites[0].SetPosition(position - camPos);
		Vector2 vector = Vector2.Lerp(lastDirection, direction, timeStacker);
		sLeaser.sprites[0].color = new Color(vector.x * 0.5f + 0.5f, vector.y * 0.5f + 0.5f, Mathf.Lerp(lastEyeOpen, eyeOpen, timeStacker), depth / 30f);
		FSprite obj = sLeaser.sprites[0];
		float width = (sLeaser.sprites[0].height = size * 4f);
		obj.width = width;
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		if (newContatiner == null)
		{
			newContatiner = rCam.ReturnFContainer("GrabShaders");
		}
		for (int i = 0; i < sLeaser.sprites.Length; i++)
		{
			sLeaser.sprites[i].RemoveFromContainer();
			newContatiner.AddChild(sLeaser.sprites[i]);
		}
	}

	public Counter ActivationCounter()
	{
		Counter counter = new Counter((int)(40f * (1f + variation * 2f)));
		counter.Finish();
		return counter;
	}

	public void CheckIfNeighboursActivated()
	{
		if (!active || activationRangeReached)
		{
			return;
		}
		for (int i = 0; i < room.updateList.Count; i++)
		{
			if (room.updateList[i] is DaemonEye daemonEye && !(Vector2.Distance(daemonEye.position, position) > activationRange * 3f) && daemonEye.activationRangeReached && (int)daemonEye.activationCounter >= daemonEye.activationCounter.max / 2)
			{
				activationRangeReached = true;
				break;
			}
		}
	}

	public void AnimateEyeActivation()
	{
		if ((eyeOpen > 0.95f || eyeOpen < 0.05f) && Random.value > (randomShutOff ? 0.93f : 0.995f))
		{
			randomShutOff = !randomShutOff;
		}
		activationCounter.Tick();
		activationCounter.countsUp = eyeShouldBeOn;
		lastEyeOpen = eyeOpen;
		eyeOpen = Mathf.Lerp(eyeOpen, Custom.LerpQuadEaseInOut(0f, 1f, activationCounter.normalized), 0.09f);
	}

	public void LookAtTarget()
	{
		targetSwitchCounter.Tick();
		if (eyeShouldBeOn && targetSwitchCounter.isFinished && Random.value > 0.95f)
		{
			randomLookPosition = position + Custom.RNV() * Random.Range(50, 500);
			targetSwitchCounter.Reset();
		}
		Vector2 vector = randomLookPosition;
		float num = 5000f;
		foreach (Player item in room.PlayersInRoom)
		{
			Vector2 vector2 = item.lastPositions[(int)Mathf.Lerp(0f, item.lastPositions.Length / 3, variation)];
			float magnitude = (vector2 - position).magnitude;
			if (Vector2.Distance(item.mainBodyChunk.pos, position) > activationRange)
			{
				if (lockOntoTargetCounter.isFinished)
				{
					continue;
				}
			}
			else
			{
				lockOntoTargetCounter.Reset();
			}
			activationRangeReached = true;
			if (magnitude < num)
			{
				vector = vector2;
				num = magnitude;
			}
		}
		if (!lockOntoTargetCounter.isFinished)
		{
			vector = Vector2.Lerp(randomLookPosition, vector, lockOntoTargetCounter.normalized);
		}
		Vector2 b = (vector - position) * (1f / (size * 5f));
		if (b.magnitude > 1f)
		{
			b.Normalize();
		}
		direction = Vector2.Lerp(direction, b, 0.005f * (1f + variation * 4f));
	}
}
