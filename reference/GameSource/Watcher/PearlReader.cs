using RWCustom;
using UnityEngine;

namespace Watcher;

public class PearlReader : UpdatableAndDeletable, IDrawable
{
	public class Action : ExtEnum<Action>
	{
		public static Action Idle = new Action("Idle", register: true);

		public static Action MoveToPearl = new Action("MoveToPearl", register: true);

		public static Action HaltPearl = new Action("HaltPearl", register: true);

		public static Action Retract = new Action("Retract", register: true);

		public static Action LoadPearl = new Action("LoadPearl", register: true);

		public static Action ProjectingPearl = new Action("ProjectingPearl", register: true);

		public static Action PutPearlOnGround = new Action("PutPearlOnGround", register: true);

		public Action(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public Vector2 clawPos;

	public Vector2 lastClawPos;

	public Vector2 pos;

	public DataPearl targetPearl;

	public float reach;

	public Counter haltPearl = new Counter(5, 0, countsUp: true);

	public Counter armExtended = new Counter(20, 0, countsUp: true);

	public Counter delay = new Counter(40);

	public Vector2 targetPos;

	public Vector2 targetDir;

	public Vector2 lastTargetPos;

	public Vector2 lastTargetDir;

	public Vector2 projectionPos;

	public Action action;

	public Color color = new Color(0.244f, 0.341f, 0.42f);

	public IntRect storageArea;

	public Vector2? pearlPickedUpFrom;

	public Vector2[] rayPos;

	public Vector2[] lastRayPos;

	public PearlContent currentContent;

	public PearlContent calibrationSequence;

	public StaticSoundLoop armSoundLoop;

	public bool movedPearl;

	public bool clawWasMoving;

	public float rayFade;

	public float distance;

	public float lastClawClosed;

	public float clawClosed;

	public Vector2 projectionSize;

	public int projectionRayStart;

	public int nProjectionRays = 20;

	public int normalArmSpeed = 40;

	public int nArmSegments = 20;

	public Color statusColor;

	public bool projectionInProgress;

	public float progressBar;

	public float lastProgressBar;

	public float lastArmExtended;

	public float armSoundVolume;

	public StaticSoundLoop soundLoop;

	public bool eligibleForAchievement;

	public bool gaveAchievement;

	public const int body = 0;

	public const int claw = 1;

	public const int arm = 2;

	public const int panel = 3;

	public const int capture = 4;

	public const int window = 5;

	public const int windowArm = 6;

	public const int panelHighlight = 7;

	public const int bar = 8;

	public const int status = 9;

	public Vector2 clawDockPos => pos + Vector2.up * 25f;

	public Vector2 clawDockDirection => pos + Vector2.up * 100f;

	public Vector2 sSize => Custom.rainWorld.screenSize;

	public PearlReader(Room room, Vector2 pos, Vector2 projectionPos)
	{
		base.room = room;
		this.pos = room.MiddleOfTile(room.GetTilePosition(pos));
		clawPos = clawDockPos;
		reach = 400f;
		storageArea = new IntRect(-6, -1, 4, 5);
		this.projectionPos = this.pos + projectionPos;
		rayPos = new Vector2[nProjectionRays];
		lastRayPos = new Vector2[nProjectionRays];
		action = Action.Idle;
	}

	public override void Update(bool eu)
	{
		lastClawPos = clawPos;
		lastArmExtended = armExtended.normalized;
		lastTargetPos = targetPos;
		lastTargetDir = targetDir;
		lastClawClosed = clawClosed;
		lastProgressBar = progressBar;
		base.Update(eu);
		PerformAction();
	}

	public void PerformAction()
	{
		if (!delay.isFinished)
		{
			delay.Tick();
			return;
		}
		projectionInProgress = false;
		movedPearl = false;
		progressBar = currentContent?.life.normalized ?? 0f;
		statusColor = ((currentContent == null && delay.isFinished) ? (RainWorld.SaturatedGold * (Mathf.Sin((float)Time.frameCount * 0.03f) * 0.5f + 0.5f)) : RainWorld.RippleGold);
		if (!clawWasMoving)
		{
			armSoundVolume = Mathf.Max(0f, armSoundVolume - 0.05f);
		}
		clawWasMoving = false;
		if (action == Action.Idle)
		{
			Idle();
		}
		else if (action == Action.MoveToPearl)
		{
			Extend();
			SetTargetToPearl();
			SetDirectionToPearl();
			MoveClaw();
		}
		else if (action == Action.HaltPearl)
		{
			HaltPearl();
			SetTargetToPearl();
			MoveClaw();
		}
		else if (action == Action.Retract)
		{
			Retract();
			MoveClaw();
			MovePearl();
		}
		else if (action == Action.LoadPearl)
		{
			MovePearl();
			LoadPearl();
		}
		else if (action == Action.ProjectingPearl)
		{
			MovePearl();
			PlayPearl();
			UpdateSoundLoop();
		}
		else if (action == Action.PutPearlOnGround)
		{
			PutPearlOnGround();
			MoveClaw();
			MovePearl();
		}
		clawClosed = Mathf.Lerp(clawClosed, movedPearl ? 1 : 0, 0.1f);
		if (armSoundLoop != null)
		{
			armSoundLoop.volume = armSoundVolume;
			armSoundLoop.pitch = 0.4f + armSoundVolume * 0.2f;
			armSoundLoop.Update();
		}
	}

	public void SetPearlCollision(bool canCollide)
	{
		if (targetPearl != null)
		{
			targetPearl.CollideWithTerrain = canCollide;
		}
	}

	public void SetAction(Action action)
	{
		SetPearlCollision(canCollide: true);
		if (action == Action.MoveToPearl || action == Action.PutPearlOnGround)
		{
			armExtended.countsUp = true;
		}
		if (action == Action.Retract)
		{
			armExtended.countsUp = false;
			if (eligibleForAchievement && !gaveAchievement)
			{
				Custom.rainWorld.processManager.CueAchievement(RainWorld.AchievementID.PearlReader, 3f);
				gaveAchievement = true;
			}
		}
		if (action == Action.HaltPearl)
		{
			armExtended.SetMax(normalArmSpeed);
			armExtended.SetToMax();
		}
		if (action == Action.PutPearlOnGround)
		{
			FindStorageTile();
		}
		if (action == Action.Idle)
		{
			Reset();
		}
		this.action = action;
	}

	public void NextAction()
	{
		if (action == Action.Idle)
		{
			SetAction(Action.MoveToPearl);
		}
		else if (action == Action.MoveToPearl)
		{
			room.PlaySound(WatcherEnums.WatcherSoundID.PearlReaderPearl, clawPos);
			SetAction(Action.HaltPearl);
		}
		else if (action == Action.HaltPearl)
		{
			SetAction(Action.Retract);
		}
		else if (action == Action.Retract)
		{
			room.PlaySound(WatcherEnums.WatcherSoundID.PearlReaderClick, pos, 0.7f, 1f);
			if (targetPearl != null)
			{
				SetAction(Action.LoadPearl);
			}
			else
			{
				SetAction(Action.Idle);
			}
		}
		else if (action == Action.LoadPearl)
		{
			SetAction(Action.ProjectingPearl);
		}
		else if (action == Action.ProjectingPearl)
		{
			SetAction(Action.PutPearlOnGround);
		}
		else if (action == Action.PutPearlOnGround)
		{
			room.PlaySound(WatcherEnums.WatcherSoundID.PearlReaderPearl, clawPos);
			SetAction(Action.Retract);
		}
		else
		{
			SetAction(Action.Idle);
		}
	}

	public static DataPearl FlyingPearlInRange(Room room, Vector2 grabberPos, float reach, float speedThreshold)
	{
		for (int i = 0; i < room.updateList.Count; i++)
		{
			if (room.updateList[i] is DataPearl dataPearl && !(Vector2.Distance(dataPearl.firstChunk.pos, grabberPos) > reach) && (dataPearl.grabbedBy.Count == 0 || dataPearl.grabbedBy[0] == null) && !(dataPearl.firstChunk.vel.magnitude < speedThreshold))
			{
				return dataPearl;
			}
		}
		return null;
	}

	public static DataPearl PearlPutOnGround(Room room, Vector2 grabberPos, float reach)
	{
		for (int i = 0; i < room.socialEventRecognizer.ownedItemsOnGround.Count; i++)
		{
			if (room.socialEventRecognizer.ownedItemsOnGround[i].item is DataPearl dataPearl && room.socialEventRecognizer.ownedItemsOnGround[i].age < 70 && Vector2.Distance(dataPearl.firstChunk.pos, grabberPos) <= reach)
			{
				return dataPearl;
			}
		}
		return null;
	}

	public void Idle()
	{
		if (targetPearl == null)
		{
			DataPearl dataPearl = FlyingPearlInRange(room, pos, reach, 5f);
			if (dataPearl != null)
			{
				targetPearl = dataPearl;
				armExtended.SetMax(Mathf.Max(10, (int)((Vector2.Distance(pos, targetPearl.firstChunk.pos) - targetPearl.firstChunk.vel.magnitude) / 30f)));
			}
		}
		if (targetPearl == null)
		{
			DataPearl dataPearl2 = PearlPutOnGround(room, pos, reach * 0.5f);
			if (dataPearl2 != null)
			{
				targetPearl = dataPearl2;
				pearlPickedUpFrom = targetPearl.firstChunk.pos;
				delay.Reset();
				armExtended.SetMax(normalArmSpeed);
			}
		}
		if (targetPearl != null)
		{
			eligibleForAchievement = targetPearl.AbstractPearl.dataPearlType != DataPearl.AbstractDataPearl.DataPearlType.Misc && targetPearl.AbstractPearl.dataPearlType != DataPearl.AbstractDataPearl.DataPearlType.Misc2;
			NextAction();
		}
	}

	public void FindStorageTile()
	{
		Vector2? vector = pearlPickedUpFrom;
		if (vector.HasValue)
		{
			Vector2 valueOrDefault = vector.GetValueOrDefault();
			targetPos = valueOrDefault;
			targetDir = Vector2.up * 100f;
			return;
		}
		targetPos = pos;
		IntVector2 tilePosition = room.GetTilePosition(pos);
		for (int i = 0; i < 10; i++)
		{
			IntVector2 intVector = tilePosition + storageArea.RandomPoint();
			if (!room.GetTile(intVector).IsSolid())
			{
				intVector = room.aimap.getAItile(intVector).fallRiskTile;
				if (storageArea.Includes(intVector - tilePosition))
				{
					targetPos = room.MiddleOfTile(intVector) + Vector2.down * 10f;
					break;
				}
			}
		}
		targetDir = Vector2.up * 100f;
	}

	public void Reset()
	{
		delay.Reset();
		clawPos = clawDockPos;
		haltPearl.Reset();
		targetPearl = null;
		pearlPickedUpFrom = null;
		currentContent = null;
		calibrationSequence?.Reset();
	}

	public void MoveClaw()
	{
		clawWasMoving = true;
		clawPos = Custom.Bezier(clawDockPos, clawDockDirection, targetPos, targetPos + targetDir, armExtended.normalized);
		distance = Vector2.Distance(pos, targetPos) / reach;
		if (armSoundLoop == null)
		{
			armSoundLoop = new StaticSoundLoop(WatcherEnums.WatcherSoundID.PearlReaderArm_LOOP, pos, room, 0f, 1f)
			{
				fadeOutOnDestroyFrames = 40
			};
		}
		float num = Vector2.Distance(clawPos, lastClawPos);
		if (num > 0.1f)
		{
			armSoundVolume = Mathf.Lerp(armSoundVolume, Mathf.Clamp(num * 0.05f, 0.3f, 0.9f), 0.05f);
		}
	}

	public void SetTargetToPearl()
	{
		if (targetPearl != null)
		{
			targetPos = targetPearl.firstChunk.pos + targetPearl.firstChunk.vel;
		}
	}

	public void SetDirectionToPearl()
	{
		if (targetPearl != null)
		{
			if (!pearlPickedUpFrom.HasValue)
			{
				targetDir = targetPearl.firstChunk.vel * 7f;
			}
			else
			{
				targetDir = Vector2.up * 100f;
			}
		}
	}

	public void MovePearl()
	{
		SetPearlCollision(canCollide: false);
		if (targetPearl != null)
		{
			movedPearl = true;
			if (targetPearl.grabbedBy.Count > 0)
			{
				targetPearl.AllGraspsLetGoOfThisObject(evenNonExlusive: false);
			}
			targetPearl.firstChunk.setPos = clawPos;
			targetPearl.firstChunk.vel = Vector2.up;
		}
	}

	public void HaltPearl()
	{
		if (targetPearl != null)
		{
			targetPearl.firstChunk.vel = Vector2.Lerp(targetPearl.firstChunk.vel, Vector2.up, haltPearl.normalized);
			movedPearl = true;
		}
		haltPearl.Tick();
		if (haltPearl.isFinished)
		{
			NextAction();
		}
	}

	public void Retract()
	{
		if (targetPearl != null)
		{
			targetPearl.CollideWithTerrain = false;
		}
		armExtended.Tick();
		if (armExtended.isFinished)
		{
			NextAction();
		}
	}

	public void Extend()
	{
		armExtended.Tick();
		if (armExtended.isFinished)
		{
			NextAction();
		}
	}

	public void LoadPearl()
	{
		if (calibrationSequence == null)
		{
			calibrationSequence = PearlContent.Load("calibration_sequence", 0);
		}
		if (currentContent == null)
		{
			currentContent = PearlContent.Load(targetPearl.AbstractPearl);
		}
		if (currentContent == null)
		{
			currentContent = PearlContent.Load("misc", targetPearl.AbstractPearl.ID.RandomSeed);
		}
		calibrationSequence.seed = currentContent.seed;
		NextAction();
	}

	public void PlayPearl()
	{
		projectionInProgress = true;
		UpdateRayPos();
		if (calibrationSequence != null && (int)calibrationSequence.life == 0)
		{
			calibrationSequence.PlayInRoom(room, projectionPos);
			projectionSize = calibrationSequence.size;
		}
		UpdateRayFade();
		if (calibrationSequence.life.isFinished)
		{
			if (currentContent != null && (int)currentContent.life == 0)
			{
				currentContent.PlayInRoom(room, projectionPos);
				projectionSize = currentContent.size;
				UpdateRayFade(currentContent);
			}
			if (currentContent.life.isFinished)
			{
				NextAction();
			}
		}
	}

	public void UpdateSoundLoop()
	{
		if (soundLoop == null)
		{
			soundLoop = new StaticSoundLoop(SoundID.Overseer_Image_LOOP, pos, room, 0f, 1f);
		}
		soundLoop.volume = rayFade * 0.7f;
		soundLoop.Update();
	}

	public void UpdateRayFade()
	{
		if (!calibrationSequence.life.isFinished)
		{
			UpdateRayFade(calibrationSequence);
		}
		else
		{
			UpdateRayFade(currentContent);
		}
	}

	public void UpdateRayFade(PearlContent content)
	{
		rayFade = Mathf.InverseLerp(0f, content.fadeTime, (int)content.life) * Mathf.InverseLerp(content.life.max, (float)content.life.max - content.fadeTime, (int)content.life);
	}

	public void UpdateRayPos()
	{
		for (int i = 0; i < nProjectionRays; i++)
		{
			lastRayPos[i] = rayPos[i];
			rayPos[i] = projectionPos + new Vector2(Random.Range(0f, projectionSize.x), Random.Range(0f, projectionSize.y)) - projectionSize * 0.5f;
		}
	}

	public void PutPearlOnGround()
	{
		armExtended.Tick();
		if (armExtended.isFinished)
		{
			targetPearl = null;
			NextAction();
		}
	}

	public static void SetVerts(FSprite mesh, Vector2[] verts)
	{
		if (mesh is TriangleMesh triangleMesh)
		{
			triangleMesh.vertices = verts;
		}
	}

	public static void SetUV(FSprite mesh, Vector2[] uv)
	{
		if (mesh is TriangleMesh triangleMesh)
		{
			triangleMesh.UVvertices = uv;
		}
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		projectionRayStart = 10;
		sLeaser.sprites = new FSprite[projectionRayStart + nProjectionRays];
		sLeaser.sprites[0] = new TriangleMesh("Futile_White", TriangleMesh.GridTriangles(2, 1), customColor: true);
		sLeaser.sprites[0].scale = 2f;
		sLeaser.sprites[1] = new FSprite("mouseHeadUp");
		sLeaser.sprites[1].anchorY = 0.9f;
		sLeaser.sprites[1].scale = 1f;
		sLeaser.sprites[2] = TriangleMesh.MakeLongMesh(nArmSegments, pointyTip: false, customColor: false);
		sLeaser.sprites[2].shader = Custom.rainWorld.Shaders["PearlReaderArm"];
		sLeaser.sprites[3] = new TriangleMesh("Futile_White", TriangleMesh.GridTriangles(1, 1), customColor: true);
		sLeaser.sprites[3].scale = 2f;
		sLeaser.sprites[7] = new TriangleMesh("Futile_White", TriangleMesh.GridTriangles(1, 1), customColor: true);
		sLeaser.sprites[7].scale = 2f;
		sLeaser.sprites[8] = new TriangleMesh("Futile_White", TriangleMesh.GridTriangles(1, 1), customColor: true);
		sLeaser.sprites[8].scale = 2f;
		sLeaser.sprites[9] = new FSprite("Futile_White");
		sLeaser.sprites[9].scale = 0.2f;
		sLeaser.sprites[5] = new TriangleMesh("Futile_White", TriangleMesh.GridTriangles(1, 1), customColor: true);
		sLeaser.sprites[5].scale = 2f;
		sLeaser.sprites[6] = new TriangleMesh("Futile_White", TriangleMesh.GridTriangles(2, 1), customColor: true);
		sLeaser.sprites[6].scale = 2f;
		sLeaser.sprites[6].shader = Custom.rainWorld.Shaders["PearlReaderArm"];
		sLeaser.sprites[4] = new FSprite("Futile_White");
		sLeaser.sprites[4].shader = rCam.game.rainWorld.Shaders["PearlReaderHolder"];
		sLeaser.sprites[4].scale = 2f;
		SetVerts(sLeaser.sprites[0], new Vector2[6]
		{
			new Vector2(-25f, -8f),
			new Vector2(-25f, 3f),
			new Vector2(-16f, -8f),
			new Vector2(-16f, 8f),
			new Vector2(16f, -8f),
			new Vector2(16f, 8f)
		});
		SetVerts(sLeaser.sprites[3], new Vector2[4]
		{
			new Vector2(-20.24f, 4f),
			new Vector2(-15f, 7f),
			new Vector2(-8f, 4f),
			new Vector2(-5f, 7f)
		});
		SetVerts(sLeaser.sprites[7], new Vector2[4]
		{
			new Vector2(-25f, 3f),
			new Vector2(-16f, 8f),
			new Vector2(-6.8f, 3f),
			new Vector2(-2f, 8f)
		});
		SetVerts(sLeaser.sprites[5], new Vector2[4]
		{
			new Vector2(-4f, 0f),
			new Vector2(-4f, 4f),
			new Vector2(14f, 0f),
			new Vector2(14f, 4f)
		});
		SetVerts(sLeaser.sprites[6], new Vector2[6]
		{
			new Vector2(-4f, 0f),
			new Vector2(-4f, 4f),
			new Vector2(8f, 0f),
			new Vector2(8f, 4f),
			new Vector2(12f, 0f),
			new Vector2(12f, 4f)
		});
		for (int i = projectionRayStart; i < projectionRayStart + nProjectionRays; i++)
		{
			sLeaser.sprites[i] = new FSprite("Futile_White");
			sLeaser.sprites[i].shader = Custom.rainWorld.Shaders["PearlReaderProjectionRay"];
			sLeaser.sprites[i].scale = 1f;
			sLeaser.sprites[i].anchorY = 0f;
		}
		AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("Midground"));
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		Vector2 position = Vector2.Lerp(lastClawPos, clawPos, timeStacker) - camPos;
		TriangleMesh triangleMesh = sLeaser.sprites[2] as TriangleMesh;
		Vector2 vector = Vector2.Lerp(lastTargetPos, targetPos, timeStacker);
		Vector2 vector2 = Vector2.Lerp(lastTargetDir, targetDir, timeStacker);
		float num = Mathf.Lerp(lastArmExtended, armExtended.normalized, timeStacker);
		sLeaser.sprites[0].SetPosition(pos - camPos);
		sLeaser.sprites[1].SetPosition(position);
		float num2 = Mathf.Lerp(lastProgressBar, progressBar, timeStacker) * 10f;
		SetVerts(sLeaser.sprites[8], new Vector2[4]
		{
			new Vector2(-16.25f, 4f),
			new Vector2(-16.5f, 5.57f),
			new Vector2(-16.25f + num2, 4f),
			new Vector2(-16.5f + num2, 5.57f)
		});
		if (num > 0f)
		{
			sLeaser.sprites[1].MoveInFrontOfOtherNode(sLeaser.sprites[2]);
		}
		else
		{
			sLeaser.sprites[1].MoveBehindOtherNode(sLeaser.sprites[0]);
		}
		float rotation = Custom.VecToDeg(Custom.BezierDerivative(clawDockPos, clawDockDirection, vector, vector + vector2, num).normalized);
		sLeaser.sprites[1].rotation = rotation;
		sLeaser.sprites[4].rotation = rotation;
		sLeaser.sprites[4].SetPosition(position);
		sLeaser.sprites[4].color = new Color(0f, 0f, 0f, Mathf.Lerp(lastClawClosed, clawClosed, timeStacker));
		sLeaser.sprites[3].SetPosition(pos - camPos);
		sLeaser.sprites[8].SetPosition(pos - camPos);
		sLeaser.sprites[8].color = RainWorld.SaturatedGold * rayFade * 0.75f;
		sLeaser.sprites[9].SetPosition(pos - camPos + new Vector2(-29f, 13f));
		sLeaser.sprites[9].color = statusColor;
		sLeaser.sprites[7].SetPosition(pos - camPos);
		sLeaser.sprites[5].SetPosition(pos - camPos);
		sLeaser.sprites[6].SetPosition(pos - camPos);
		float num3 = num * -0.5f;
		SetUV(sLeaser.sprites[6], new Vector2[6]
		{
			new Vector2(0f, num3),
			new Vector2(1f, num3),
			new Vector2(0f, 0.1f + num3),
			new Vector2(1f, 0.1f + num3),
			new Vector2(0f, 0.3f + num3),
			new Vector2(1f, 0.3f + num3)
		});
		for (int i = 0; i < triangleMesh.vertices.Length; i += 2)
		{
			float f = Mathf.Max(0f, (float)i / (float)(triangleMesh.vertices.Length - 2) + num - 1f);
			Vector2 vector3 = Custom.Bezier(clawDockPos + Vector2.down * 10f, clawDockDirection, vector, vector + vector2, f) - camPos;
			Vector2 vector4 = Custom.PerpendicularVector(Custom.BezierDerivative(clawDockPos + Vector2.down * 10f, clawDockDirection, vector, vector + vector2, f).normalized) * 5f;
			triangleMesh.MoveVertice(i, vector3 + vector4);
			triangleMesh.MoveVertice(i + 1, vector3 - vector4);
		}
		Color color = sLeaser.sprites[2].color;
		color.a = distance;
		sLeaser.sprites[2].color = color;
		for (int j = projectionRayStart; j < projectionRayStart + nProjectionRays; j++)
		{
			int num4 = j - projectionRayStart;
			Vector2 vector5 = Vector2.Lerp(lastRayPos[num4], rayPos[num4], timeStacker);
			sLeaser.sprites[j].SetPosition(position);
			sLeaser.sprites[j].isVisible = projectionInProgress;
			sLeaser.sprites[j].rotation = Custom.AimFromOneVectorToAnother(clawPos, vector5);
			sLeaser.sprites[j].scaleY = Vector2.Distance(clawPos, vector5) * PearlContent.magicScale;
			sLeaser.sprites[j].color = new Color((vector5.x - camPos.x) / sSize.x, (vector5.y - camPos.y) / sSize.y, 0f, rayFade * 0.25f);
		}
		if (!sLeaser.deleteMeNextFrame && (base.slatedForDeletetion || room != rCam.room))
		{
			sLeaser.CleanSpritesAndRemove();
		}
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		newContatiner.AddChild(sLeaser.sprites[0]);
		newContatiner.AddChild(sLeaser.sprites[7]);
		newContatiner.AddChild(sLeaser.sprites[3]);
		newContatiner.AddChild(sLeaser.sprites[8]);
		newContatiner.AddChild(sLeaser.sprites[9]);
		newContatiner.AddChild(sLeaser.sprites[6]);
		newContatiner.AddChild(sLeaser.sprites[5]);
		newContatiner.AddChild(sLeaser.sprites[2]);
		newContatiner.AddChild(sLeaser.sprites[1]);
		rCam.ReturnFContainer("GrabShaders").AddChild(sLeaser.sprites[4]);
		FContainer fContainer = rCam.ReturnFContainer("Bloom");
		for (int i = projectionRayStart; i < projectionRayStart + nProjectionRays; i++)
		{
			fContainer.AddChild(sLeaser.sprites[i]);
		}
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		Color a = this.color;
		a *= new Color(Mathf.Max(palette.skyColor.r, 0.5f), Mathf.Max(palette.skyColor.g, 0.5f), Mathf.Max(palette.skyColor.b, 0.5f));
		Color color = Color.Lerp(a, palette.blackColor, 0.4f);
		Color color2 = Color.Lerp(a, palette.blackColor, 0.9f);
		color2.a = 0.3f;
		sLeaser.sprites[0].color = color;
		sLeaser.sprites[1].color = a;
		sLeaser.sprites[2].color = a;
		sLeaser.sprites[3].color = color;
		sLeaser.sprites[8].color = RainWorld.SaturatedGold;
		sLeaser.sprites[7].color = a;
		sLeaser.sprites[5].color = color2;
		sLeaser.sprites[6].color = Color.Lerp(a, palette.blackColor, 0.5f);
	}
}
