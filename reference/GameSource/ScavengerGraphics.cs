using System;
using System.Collections.Generic;
using MoreSlugcats;
using RWCustom;
using ScavengerCosmetic;
using Unity.Mathematics;
using UnityEngine;

public class ScavengerGraphics : ComplexGraphicsModule, IMuddableGraphics
{
	public struct IndividualVariations
	{
		public float headSize;

		public float eartlerWidth;

		public float neckThickness;

		public float handsHeadColor;

		public float eyeSize;

		public float narrowEyes;

		public float eyesAngle;

		public float fatness;

		public float narrowWaist;

		public float legsSize;

		public float armThickness;

		public float wideTeeth;

		public float pupilSize;

		public float scruffy;

		public bool coloredEartlerTips;

		public bool deepPupils;

		public int coloredPupils;

		public int tailSegs;

		public float generalMelanin;

		public float WaistWidth => fatness * (1f - narrowWaist);

		public IndividualVariations(Scavenger scavenger)
		{
			generalMelanin = Custom.PushFromHalf(UnityEngine.Random.value, 2f);
			headSize = Custom.ClampedRandomVariation(0.5f, 0.5f, 0.1f);
			eartlerWidth = UnityEngine.Random.value;
			eyeSize = Mathf.Pow(Mathf.Max(0f, Mathf.Lerp(UnityEngine.Random.value, Mathf.Pow(headSize, 0.5f), UnityEngine.Random.value * 0.4f)), Mathf.Lerp(0.95f, 0.55f, scavenger.abstractCreature.personality.sympathy));
			narrowEyes = ((UnityEngine.Random.value < Mathf.Lerp(0.3f, 0.7f, scavenger.abstractCreature.personality.sympathy)) ? 0f : Mathf.Pow(UnityEngine.Random.value, Mathf.Lerp(0.5f, 1.5f, scavenger.abstractCreature.personality.sympathy)));
			if (scavenger.Elite)
			{
				narrowEyes = 1f;
			}
			eyesAngle = Mathf.Pow(UnityEngine.Random.value, Mathf.Lerp(2.5f, 0.5f, Mathf.Pow(scavenger.abstractCreature.personality.energy, 0.03f)));
			fatness = Mathf.Lerp(UnityEngine.Random.value, scavenger.abstractCreature.personality.dominance, UnityEngine.Random.value * 0.2f);
			if (ModManager.MSC)
			{
				fatness = Mathf.Lerp(fatness, 0.09f, (float)scavenger.abstractCreature.RemovedKarma / 5f);
			}
			if (scavenger.abstractCreature.personality.energy < 0.5f)
			{
				fatness = Mathf.Lerp(fatness, 1f, UnityEngine.Random.value * Mathf.InverseLerp(0.5f, 0f, scavenger.abstractCreature.personality.energy));
			}
			else
			{
				fatness = Mathf.Lerp(fatness, 0f, UnityEngine.Random.value * Mathf.InverseLerp(0.5f, 1f, scavenger.abstractCreature.personality.energy));
			}
			narrowWaist = Mathf.Lerp(Mathf.Lerp(UnityEngine.Random.value, 1f - fatness, UnityEngine.Random.value), 1f - scavenger.abstractCreature.personality.energy, UnityEngine.Random.value);
			neckThickness = Mathf.Lerp(Mathf.Pow(UnityEngine.Random.value, 1.5f - scavenger.abstractCreature.personality.aggression), 1f - fatness, UnityEngine.Random.value * 0.5f);
			pupilSize = 0f;
			deepPupils = false;
			coloredPupils = 0;
			if (UnityEngine.Random.value < 0.65f && eyeSize > 0.4f && narrowEyes < 0.3f)
			{
				if (UnityEngine.Random.value < Mathf.Pow(scavenger.abstractCreature.personality.sympathy, 1.5f) * 0.8f)
				{
					pupilSize = Mathf.Lerp(0.4f, 0.8f, Mathf.Pow(UnityEngine.Random.value, 0.5f));
					if (UnityEngine.Random.value < 2f / 3f)
					{
						coloredPupils = UnityEngine.Random.Range(1, 4);
					}
				}
				else
				{
					pupilSize = 0.7f;
					deepPupils = true;
				}
			}
			if (scavenger.Elite)
			{
				coloredPupils = UnityEngine.Random.Range(1, 4);
			}
			if (UnityEngine.Random.value < generalMelanin)
			{
				handsHeadColor = ((UnityEngine.Random.value < 0.3f) ? UnityEngine.Random.value : ((UnityEngine.Random.value < 0.6f) ? 1f : 0f));
			}
			else
			{
				handsHeadColor = ((UnityEngine.Random.value < 0.2f) ? UnityEngine.Random.value : ((UnityEngine.Random.value < 0.8f) ? 1f : 0f));
			}
			legsSize = UnityEngine.Random.value;
			armThickness = Mathf.Lerp(UnityEngine.Random.value, Mathf.Lerp(scavenger.abstractCreature.personality.dominance, fatness, 0.5f), UnityEngine.Random.value);
			coloredEartlerTips = scavenger.Elite || UnityEngine.Random.value < 1f / Mathf.Lerp(1.2f, 10f, generalMelanin);
			wideTeeth = UnityEngine.Random.value;
			tailSegs = ((!(UnityEngine.Random.value < 0.5f)) ? UnityEngine.Random.Range(1, 5) : 0);
			scruffy = 0f;
			if (UnityEngine.Random.value < 0.25f)
			{
				scruffy = Mathf.Pow(UnityEngine.Random.value, 0.3f);
			}
			scruffy = 1f;
		}
	}

	public class ScavengerHand : Limb
	{
		public int firstSprite;

		public float armLength = 50f;

		public float2 spearPosAdd;

		public float2 lastSpearPosAdd;

		public float2? grabPos;

		public ScavengerGraphics graphics => owner as ScavengerGraphics;

		public Scavenger scavenger => graphics.scavenger;

		public float MyFlip(float timeStacker)
		{
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_0072: Unknown result type (might be due to invalid IL or missing references)
			//IL_008e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0094: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
			return Mathf.Lerp(Custom.LerpMap(Mathf.Lerp(graphics.lastFlip, graphics.flip, timeStacker), -1f + (float)limbNumber, limbNumber, -1f, 1f), Custom.LerpMap(Custom.DistanceToLine(math.lerp(float2.op_Implicit(lastPos), float2.op_Implicit(pos), timeStacker), math.lerp(graphics.drawPositions[graphics.hipsDrawPos, 1], graphics.drawPositions[graphics.hipsDrawPos, 0], timeStacker), math.lerp(graphics.drawPositions[graphics.chestDrawPos, 1], graphics.drawPositions[graphics.chestDrawPos, 0], timeStacker)), 10f, -10f, -1f, 1f), Mathf.Lerp(graphics.lastSwimArms, graphics.swimArms, timeStacker));
		}

		public ScavengerHand(ScavengerGraphics owner, int num, int firstSprite)
			: base(owner, owner.scavenger.mainBodyChunk, num, 3f, 0.5f, 0.99f, 24f, 0.95f)
		{
			this.firstSprite = firstSprite;
		}

		public override void Update()
		{
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_008a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0094: Unknown result type (might be due to invalid IL or missing references)
			//IL_0099: Unknown result type (might be due to invalid IL or missing references)
			//IL_0a28: Unknown result type (might be due to invalid IL or missing references)
			//IL_0a2d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0a2e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0a2f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0a47: Unknown result type (might be due to invalid IL or missing references)
			//IL_0a4c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0a51: Unknown result type (might be due to invalid IL or missing references)
			//IL_0a56: Unknown result type (might be due to invalid IL or missing references)
			//IL_0a71: Unknown result type (might be due to invalid IL or missing references)
			//IL_0a7b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0072: Unknown result type (might be due to invalid IL or missing references)
			//IL_007c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0081: Unknown result type (might be due to invalid IL or missing references)
			//IL_0a9a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0a9f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0aa4: Unknown result type (might be due to invalid IL or missing references)
			//IL_0aa5: Unknown result type (might be due to invalid IL or missing references)
			//IL_0abd: Unknown result type (might be due to invalid IL or missing references)
			//IL_0ad9: Unknown result type (might be due to invalid IL or missing references)
			//IL_0ade: Unknown result type (might be due to invalid IL or missing references)
			//IL_0ae8: Unknown result type (might be due to invalid IL or missing references)
			//IL_0aed: Unknown result type (might be due to invalid IL or missing references)
			//IL_0af2: Unknown result type (might be due to invalid IL or missing references)
			//IL_0af9: Unknown result type (might be due to invalid IL or missing references)
			//IL_0480: Unknown result type (might be due to invalid IL or missing references)
			//IL_048a: Unknown result type (might be due to invalid IL or missing references)
			//IL_048f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0b0b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0b21: Unknown result type (might be due to invalid IL or missing references)
			//IL_0b28: Unknown result type (might be due to invalid IL or missing references)
			//IL_04e0: Unknown result type (might be due to invalid IL or missing references)
			//IL_04ea: Unknown result type (might be due to invalid IL or missing references)
			//IL_04ef: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
			//IL_0201: Unknown result type (might be due to invalid IL or missing references)
			//IL_0206: Unknown result type (might be due to invalid IL or missing references)
			//IL_0228: Unknown result type (might be due to invalid IL or missing references)
			//IL_0242: Unknown result type (might be due to invalid IL or missing references)
			//IL_025e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0263: Unknown result type (might be due to invalid IL or missing references)
			//IL_0269: Unknown result type (might be due to invalid IL or missing references)
			//IL_028f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0299: Unknown result type (might be due to invalid IL or missing references)
			//IL_029e: Unknown result type (might be due to invalid IL or missing references)
			//IL_02a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_02b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_02ec: Unknown result type (might be due to invalid IL or missing references)
			//IL_0b9c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0323: Unknown result type (might be due to invalid IL or missing references)
			//IL_034f: Unknown result type (might be due to invalid IL or missing references)
			//IL_035a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0761: Unknown result type (might be due to invalid IL or missing references)
			//IL_06b3: Unknown result type (might be due to invalid IL or missing references)
			//IL_0626: Unknown result type (might be due to invalid IL or missing references)
			//IL_0819: Unknown result type (might be due to invalid IL or missing references)
			//IL_078b: Unknown result type (might be due to invalid IL or missing references)
			//IL_07d2: Unknown result type (might be due to invalid IL or missing references)
			//IL_07d9: Unknown result type (might be due to invalid IL or missing references)
			//IL_07f3: Unknown result type (might be due to invalid IL or missing references)
			//IL_07fe: Unknown result type (might be due to invalid IL or missing references)
			base.Update();
			lastSpearPosAdd = spearPosAdd;
			if (limbNumber == 0 && (scavenger.movMode == Scavenger.MovementMode.Run || scavenger.movMode == Scavenger.MovementMode.StandStill) && scavenger.grasps[0] != null && scavenger.grasps[0].grabbed is Spear)
			{
				spearPosAdd = WeaponDir() * 20f;
			}
			else
			{
				spearPosAdd *= 0f;
			}
			if (!scavenger.Consious)
			{
				base.mode = Mode.Dangle;
			}
			else if (limbNumber == 0 && scavenger.animation != null && scavenger.animation.id == Scavenger.ScavengerAnimation.ID.Throw)
			{
				base.mode = Mode.HuntAbsolutePosition;
				absoluteHuntPos = (scavenger.animation as Scavenger.ThrowAnimation).thrownObject.firstChunk.pos;
				if (scavenger.animation.age < 5)
				{
					pos = absoluteHuntPos;
				}
			}
			else if (limbNumber == 0 && scavenger.animation != null && scavenger.animation.id == Scavenger.ScavengerAnimation.ID.ThrowCharge && scavenger.animation.Active)
			{
				base.mode = Mode.HuntAbsolutePosition;
				float2 val = Vector3Ext.Slerp2(-ExtraExtentions.ToF2((scavenger.animation as Scavenger.ThrowChargeAnimation).Direction), Custom.DirVec(graphics.drawPositions[graphics.hipsDrawPos, 1], graphics.drawPositions[graphics.chestDrawPos, 1]), 0.6f);
				float num = Mathf.Sin((float)Math.PI / 20f * (scavenger.animation as Scavenger.ThrowChargeAnimation).cycle);
				val += Custom.DirVec(graphics.drawPositions[graphics.headDrawPos + 1, 1], graphics.drawPositions[graphics.headDrawPos, 1]) * num * (0.1f + 0.9f * (scavenger.animation as Scavenger.ThrowChargeAnimation).shake) * 0.2f;
				absoluteHuntPos = scavenger.mainBodyChunk.pos + ExtraExtentions.ToV2(val) * 35f;
				if (Custom.DistLess(absoluteHuntPos, float2.op_Implicit(graphics.drawPositions[graphics.headDrawPos, 1]), 10f))
				{
					absoluteHuntPos += Custom.DirVec(graphics.drawPositions[graphics.headDrawPos, 1], absoluteHuntPos) * (10f - math.distance(graphics.drawPositions[graphics.headDrawPos, 1], float2.op_Implicit(absoluteHuntPos)));
				}
			}
			else if (scavenger.Pointing && limbNumber == (scavenger.animation as Scavenger.PointingAnimation).PointingArm)
			{
				base.mode = Mode.HuntAbsolutePosition;
				absoluteHuntPos = scavenger.mainBodyChunk.pos;
				absoluteHuntPos += Custom.DirVec(scavenger.mainBodyChunk.pos, (scavenger.animation as Scavenger.PointingAnimation).LookPoint) * (50f + 10f * Mathf.Sin((scavenger.animation as Scavenger.PointingAnimation).cycle * 0.3f));
				absoluteHuntPos += Custom.RNV() * (UnityEngine.Random.value * 2f * (scavenger.AI.ActNervous + scavenger.AI.agitation));
				spearPosAdd *= 0f;
			}
			else if (scavenger.Communicating && limbNumber == (scavenger.animation as Scavenger.CommunicationAnimation).GestureArm)
			{
				absoluteHuntPos = (scavenger.animation as Scavenger.CommunicationAnimation).GestureArmPos();
				spearPosAdd *= 0f;
			}
			else if (scavenger.movMode == Scavenger.MovementMode.Swim)
			{
				base.mode = Mode.Dangle;
				if (scavenger.moving)
				{
					float t = Mathf.Sin((graphics.cycle * 0.3f + (float)limbNumber * 0.5f) * (float)Math.PI * 2f);
					float num2 = scavenger.mainBodyChunk.pos.x + ((limbNumber == 0) ? (-1f) : 1f) * 5f + scavenger.flip * Mathf.Lerp(10f, 50f, t);
					absoluteHuntPos = float2.op_Implicit(new float2(num2, scavenger.room.FloatWaterLevel(new Vector2(num2, scavenger.mainBodyChunk.pos.y)) - 12f + 20f * Mathf.Cos((graphics.cycle * 0.3f + (float)limbNumber * 0.5f) * (float)Math.PI * 2f) * scavenger.flip));
				}
				else
				{
					float num3 = scavenger.mainBodyChunk.pos.x + ((limbNumber == 0) ? (-1f) : 1f) * 35f + scavenger.flip * 15f;
					absoluteHuntPos = float2.op_Implicit(new float2(num3, scavenger.room.FloatWaterLevel(new Vector2(num3, scavenger.mainBodyChunk.pos.y)) - 4f));
				}
				vel += Vector2.ClampMagnitude(absoluteHuntPos - pos, 4f) / 5f;
				pos = absoluteHuntPos;
			}
			else if (scavenger.movMode == Scavenger.MovementMode.Run && !scavenger.climbingUpComing)
			{
				huntSpeed = defaultHuntSpeed;
				if (grabPos.HasValue)
				{
					base.mode = Mode.HuntAbsolutePosition;
					absoluteHuntPos = float2.op_Implicit(grabPos.Value);
					if (scavenger.knucklePos.HasValue && !Custom.DistLess(float2.op_Implicit(grabPos.Value), scavenger.knucklePos.Value, 40f))
					{
						float2? val2 = CheckForGrabPos();
						if (val2.HasValue && math.distance(float2.op_Implicit(scavenger.knucklePos.Value), val2.Value) < math.distance(float2.op_Implicit(scavenger.knucklePos.Value), grabPos.Value))
						{
							grabPos = val2;
						}
					}
					if (!GrabPosLegal(grabPos.Value))
					{
						grabPos = null;
					}
				}
				else
				{
					base.mode = Mode.Dangle;
					grabPos = CheckForGrabPos();
				}
			}
			else if (scavenger.Rummaging && (limbNumber == 1 || scavenger.grasps[0] == null) && scavenger.bodyChunks[2].pos.x > scavenger.mainBodyChunk.pos.x == scavenger.flip > 0f)
			{
				base.mode = Mode.HuntAbsolutePosition;
				if (limbNumber == 1 && (scavenger.animation as Scavenger.RummageAnimation).holdUpAndLook > 0)
				{
					base.mode = Mode.Dangle;
					vel *= 0.4f;
					vel += ((scavenger.animation as Scavenger.RummageAnimation).handPositions[limbNumber] - pos) / 5f;
				}
				else if (Custom.DistLess(absoluteHuntPos, (scavenger.animation as Scavenger.RummageAnimation).handPositions[limbNumber], 20f))
				{
					huntSpeed = 4.9f;
					quickness = 0.4f;
				}
				if (UnityEngine.Random.value < 0.25f || !Custom.DistLess(absoluteHuntPos, (scavenger.animation as Scavenger.RummageAnimation).handPositions[limbNumber], 20f))
				{
					absoluteHuntPos = (scavenger.animation as Scavenger.RummageAnimation).handPositions[limbNumber];
				}
			}
			else
			{
				StandardLocomotionProcedure();
			}
			float2 val3 = graphics.drawPositions[graphics.chestDrawPos, 0];
			val3 += Custom.PerpendicularVector(ExtraExtentions.normalized(val3 - graphics.drawPositions[graphics.hipsDrawPos, 0])) * (1f - Mathf.Abs(graphics.flip)) * 10f * (((float)limbNumber == 0f) ? (-1f) : 1f);
			val3 += Custom.DirVec(graphics.drawPositions[graphics.hipsDrawPos, 0], graphics.drawPositions[graphics.chestDrawPos, 0]) * 5f;
			if (Custom.DistLess(pos, float2.op_Implicit(val3), 10f))
			{
				Vector2 vector = Custom.DirVec(ExtraExtentions.ToV2(val3), pos) * (10f - math.distance(val3, float2.op_Implicit(pos)));
				vel += vector;
				pos += vector;
			}
			if (base.mode == Mode.Dangle)
			{
				ConnectToPoint(scavenger.mainBodyChunk.pos, armLength, push: false, 0f, float2.op_Implicit(new float2(0f, 0f)), 0f, 0f);
				if (scavenger.room.PointSubmerged(pos))
				{
					vel *= 0.8f;
				}
				else
				{
					vel.y -= 0.9f;
				}
			}
			else
			{
				ConnectToPoint(scavenger.mainBodyChunk.pos, armLength, push: false, 0f, scavenger.mainBodyChunk.vel, 0.4f, 0.1f);
			}
		}

		public void StandardLocomotionProcedure()
		{
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_007d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0094: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
			//IL_0666: Unknown result type (might be due to invalid IL or missing references)
			//IL_0682: Unknown result type (might be due to invalid IL or missing references)
			//IL_06b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_06b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_06ba: Unknown result type (might be due to invalid IL or missing references)
			//IL_01de: Unknown result type (might be due to invalid IL or missing references)
			//IL_0170: Unknown result type (might be due to invalid IL or missing references)
			//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
			//IL_0105: Unknown result type (might be due to invalid IL or missing references)
			//IL_010b: Unknown result type (might be due to invalid IL or missing references)
			//IL_06c1: Unknown result type (might be due to invalid IL or missing references)
			//IL_06c7: Unknown result type (might be due to invalid IL or missing references)
			//IL_0349: Unknown result type (might be due to invalid IL or missing references)
			//IL_034e: Unknown result type (might be due to invalid IL or missing references)
			//IL_020f: Unknown result type (might be due to invalid IL or missing references)
			//IL_023d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0242: Unknown result type (might be due to invalid IL or missing references)
			//IL_0247: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
			//IL_01db: Unknown result type (might be due to invalid IL or missing references)
			//IL_06fe: Unknown result type (might be due to invalid IL or missing references)
			//IL_06da: Unknown result type (might be due to invalid IL or missing references)
			//IL_06db: Unknown result type (might be due to invalid IL or missing references)
			//IL_06dc: Unknown result type (might be due to invalid IL or missing references)
			//IL_06dd: Unknown result type (might be due to invalid IL or missing references)
			//IL_06e8: Unknown result type (might be due to invalid IL or missing references)
			//IL_06ed: Unknown result type (might be due to invalid IL or missing references)
			//IL_06f2: Unknown result type (might be due to invalid IL or missing references)
			//IL_03fa: Unknown result type (might be due to invalid IL or missing references)
			//IL_0375: Unknown result type (might be due to invalid IL or missing references)
			//IL_037c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0381: Unknown result type (might be due to invalid IL or missing references)
			//IL_038b: Unknown result type (might be due to invalid IL or missing references)
			//IL_03a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_03b9: Unknown result type (might be due to invalid IL or missing references)
			//IL_03ca: Unknown result type (might be due to invalid IL or missing references)
			//IL_03db: Unknown result type (might be due to invalid IL or missing references)
			//IL_0248: Unknown result type (might be due to invalid IL or missing references)
			//IL_071b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0721: Unknown result type (might be due to invalid IL or missing references)
			//IL_02a8: Unknown result type (might be due to invalid IL or missing references)
			//IL_02bd: Unknown result type (might be due to invalid IL or missing references)
			//IL_02c2: Unknown result type (might be due to invalid IL or missing references)
			//IL_0823: Unknown result type (might be due to invalid IL or missing references)
			//IL_082f: Unknown result type (might be due to invalid IL or missing references)
			//IL_074a: Unknown result type (might be due to invalid IL or missing references)
			//IL_060b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0610: Unknown result type (might be due to invalid IL or missing references)
			//IL_0611: Unknown result type (might be due to invalid IL or missing references)
			//IL_042b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0459: Unknown result type (might be due to invalid IL or missing references)
			//IL_045e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0463: Unknown result type (might be due to invalid IL or missing references)
			//IL_0482: Unknown result type (might be due to invalid IL or missing references)
			//IL_0547: Unknown result type (might be due to invalid IL or missing references)
			//IL_054c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0565: Unknown result type (might be due to invalid IL or missing references)
			//IL_0572: Unknown result type (might be due to invalid IL or missing references)
			//IL_07bf: Unknown result type (might be due to invalid IL or missing references)
			//IL_0761: Unknown result type (might be due to invalid IL or missing references)
			//IL_0766: Unknown result type (might be due to invalid IL or missing references)
			//IL_062b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0659: Unknown result type (might be due to invalid IL or missing references)
			//IL_065e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0663: Unknown result type (might be due to invalid IL or missing references)
			//IL_07ee: Unknown result type (might be due to invalid IL or missing references)
			//IL_07fa: Unknown result type (might be due to invalid IL or missing references)
			huntSpeed = defaultHuntSpeed;
			grabPos = null;
			bool flag = true;
			float2 val = float2.op_Implicit(pos);
			Scavenger.MovementMode movementMode = scavenger.movMode;
			if (scavenger.climbingUpComing)
			{
				movementMode = Scavenger.MovementMode.Climb;
			}
			float2 val2 = ExtraExtentions.ToF2(scavenger.mainBodyChunk.pos);
			if (movementMode == Scavenger.MovementMode.StandStill)
			{
				huntSpeed *= 0.2f;
				val = val2 + Custom.RotateAroundOrigo(new float2(MyFlip(1f) * 30f, 0f), Custom.AimFromOneVectorToAnother(scavenger.bodyChunks[1].pos, scavenger.bodyChunks[0].pos));
				for (int i = 0; i < 3; i++)
				{
					if (!scavenger.room.GetTile(float2.op_Implicit(val + new float2(0f, -20f))).Solid)
					{
						((float2)(ref val))._002Ector(val.x, val.y - 20f);
					}
				}
			}
			else if (movementMode == Scavenger.MovementMode.Crawl)
			{
				huntSpeed *= 0.6f;
				val = ((!scavenger.knucklePos.HasValue) ? (val2 + Custom.RotateAroundOrigo(new float2(Mathf.Sign(MyFlip(1f)) * 5f + ((limbNumber == 0) ? (-5f) : 5f), 0f), Custom.AimFromOneVectorToAnother(scavenger.bodyChunks[1].pos, scavenger.bodyChunks[0].pos))) : (ExtraExtentions.ToF2(scavenger.knucklePos.Value) + Custom.RotateAroundOrigo(new float2(Mathf.Sign(MyFlip(1f)) * 5f + ((limbNumber == 0) ? (-5f) : 5f), 0f), Custom.AimFromOneVectorToAnother(scavenger.bodyChunks[0].pos, scavenger.knucklePos.Value))));
				if (Custom.DistLess(float2.op_Implicit(val), graphics.hands[1 - limbNumber].absoluteHuntPos, 30f))
				{
					val = float2.op_Implicit(graphics.hands[1 - limbNumber].absoluteHuntPos + Custom.DirVec(graphics.hands[1 - limbNumber].absoluteHuntPos, val) * 30f);
				}
			}
			else if (movementMode == Scavenger.MovementMode.Climb)
			{
				flag = false;
				if (scavenger.swingPos.HasValue)
				{
					if (limbNumber == scavenger.swingArm)
					{
						base.mode = Mode.HuntAbsolutePosition;
						absoluteHuntPos = scavenger.swingPos.Value;
						pos = scavenger.swingPos.Value;
						val = float2.op_Implicit(scavenger.swingPos.Value);
					}
					else if (scavenger.nextSwingPos.HasValue)
					{
						base.mode = Mode.HuntAbsolutePosition;
						absoluteHuntPos = float2.op_Implicit(math.lerp((val2 + ExtraExtentions.ToF2(pos)) / 2f, float2.op_Implicit(scavenger.nextSwingPos.Value), 0.3f + 0.7f * Mathf.InverseLerp(100f, 50f, math.distance(val2, float2.op_Implicit(scavenger.nextSwingPos.Value)))));
					}
					else
					{
						base.mode = Mode.Dangle;
						float2 val3 = val2 + Custom.RotateAroundOrigo(new float2(math.sign(MyFlip(1f)) * 8f + ((limbNumber == 0) ? (-11f) : 11f), -21f), Custom.AimFromOneVectorToAnother(scavenger.bodyChunks[1].pos, scavenger.bodyChunks[0].pos));
						vel *= 0.4f;
						vel += (ExtraExtentions.ToV2(val3) - pos) / 5f;
					}
				}
				else
				{
					if (base.mode == Mode.HuntAbsolutePosition)
					{
						if (!Custom.DistLess(connection.pos, absoluteHuntPos, armLength))
						{
							base.mode = Mode.Dangle;
						}
					}
					else if (scavenger.connections.Count > 0)
					{
						int num = scavenger.connections.Count - 1;
						while (num >= 0 && base.mode != Mode.HuntAbsolutePosition)
						{
							float2 val4 = float2.op_Implicit(scavenger.room.MiddleOfTile(scavenger.connections[num].DestTile));
							FindGrip(scavenger.room, connection.pos, float2.op_Implicit(val4), armLength, float2.op_Implicit(val4), -2, -2, behindWalls: false);
							num--;
						}
					}
					if (base.mode == Mode.Dangle)
					{
						flag = true;
						if (scavenger.connections.Count > 0)
						{
							val = float2.op_Implicit(scavenger.room.MiddleOfTile(scavenger.connections[scavenger.connections.Count - 1].DestTile));
							val += Custom.RotateAroundOrigo(new float2((limbNumber == 0) ? (-10f) : 10f, 10f), Custom.AimFromOneVectorToAnother(scavenger.bodyChunks[1].pos, scavenger.bodyChunks[0].pos));
						}
					}
				}
			}
			else
			{
				val = val2 + Custom.RotateAroundOrigo(new float2(Mathf.Sign(MyFlip(1f)) * 30f, 0f), Custom.AimFromOneVectorToAnother(scavenger.bodyChunks[1].pos, scavenger.bodyChunks[0].pos));
			}
			if (flag)
			{
				if (!Custom.DistLess(float2.op_Implicit(val), float2.op_Implicit(val2), armLength))
				{
					val = val2 + Custom.DirVec(val2, val) * armLength;
				}
				if (scavenger.room.GetTile(float2.op_Implicit(val)).Solid)
				{
					Vector2? vector = SharedPhysics.ExactTerrainRayTracePos(scavenger.room, float2.op_Implicit(val2), float2.op_Implicit(val));
					float2? val5 = (vector.HasValue ? new float2?(float2.op_Implicit(vector.GetValueOrDefault())) : null);
					if (val5.HasValue)
					{
						val = val5.Value;
					}
				}
				if (base.mode == Mode.HuntAbsolutePosition)
				{
					if (!Custom.DistLess(connection.pos, absoluteHuntPos, armLength))
					{
						base.mode = Mode.Dangle;
					}
					if (scavenger.movMode == Scavenger.MovementMode.StandStill && !Custom.DistLess(float2.op_Implicit(val), absoluteHuntPos, 40f))
					{
						FindGrip(scavenger.room, connection.pos, float2.op_Implicit(val), armLength, float2.op_Implicit(val), -2, -2, behindWalls: false);
					}
				}
				else
				{
					FindGrip(scavenger.room, connection.pos, float2.op_Implicit(val), armLength, float2.op_Implicit(val), -2, -2, behindWalls: false);
				}
			}
			if (base.mode != Mode.HuntAbsolutePosition && scavenger.climbingUpComing && !scavenger.swingPos.HasValue)
			{
				FindGrip(scavenger.room, connection.pos, connection.pos + Custom.RNV() * (armLength * UnityEngine.Random.value), armLength, connection.pos + Custom.RNV() * (armLength * UnityEngine.Random.value), -2, -2, behindWalls: false);
			}
		}

		public override void GrabbedTerrain()
		{
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_0171: Unknown result type (might be due to invalid IL or missing references)
			//IL_0176: Unknown result type (might be due to invalid IL or missing references)
			//IL_0187: Unknown result type (might be due to invalid IL or missing references)
			//IL_018c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
			base.GrabbedTerrain();
			if (!Custom.DistLess(absoluteHuntPos, float2.op_Implicit(graphics.lastKnuckleSoundPos), 16f) && !Custom.DistLess(absoluteHuntPos, float2.op_Implicit(graphics.lastLastKnuckleSoundPos), 16f))
			{
				if (scavenger.room.GetTile(absoluteHuntPos).AnyBeam)
				{
					scavenger.room.PlaySound(SoundID.Scavenger_Grab_Pole, absoluteHuntPos, scavenger.abstractCreature);
				}
				else if (scavenger.movMode == Scavenger.MovementMode.Run && scavenger.room.GetTile(absoluteHuntPos + new Vector2(0f, -10f)).Solid)
				{
					scavenger.room.PlaySound((ExtraExtentions.magnitude(spearPosAdd) > 1f) ? SoundID.Scavenger_Spear_Blunt_Hit_Ground : SoundID.Scavenger_Knuckle_Hit_Ground, absoluteHuntPos, scavenger.abstractCreature);
				}
				else if (scavenger.room.aimap.getTerrainProximity(absoluteHuntPos) < 2)
				{
					scavenger.room.PlaySound(SoundID.Scavenger_Grab_Terrain, absoluteHuntPos, scavenger.abstractCreature);
				}
				graphics.lastLastKnuckleSoundPos = graphics.lastKnuckleSoundPos;
				graphics.lastKnuckleSoundPos = float2.op_Implicit(absoluteHuntPos);
			}
		}

		public float2? CheckForGrabPos()
		{
			//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
			if (scavenger.knucklePos.HasValue)
			{
				float num = Custom.AimFromOneVectorToAnother(scavenger.mainBodyChunk.pos, scavenger.knucklePos.Value);
				for (float num2 = 0f; num2 < 45f; num2 += 5f)
				{
					for (int i = -1; i <= 1; i += 2)
					{
						Vector2? vector = SharedPhysics.ExactTerrainRayTracePos(scavenger.room, scavenger.mainBodyChunk.pos, scavenger.mainBodyChunk.pos + Custom.DegToVec(num + num2 * (float)i + ((limbNumber == 0) ? (-4f) : 4f)) * (armLength * 10.5f));
						float2? result = (vector.HasValue ? new float2?(float2.op_Implicit(vector.GetValueOrDefault())) : null);
						if (result.HasValue && GrabPosLegal(result.Value))
						{
							return result;
						}
					}
				}
			}
			return null;
		}

		public bool GrabPosLegal(float2 pos)
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			float num = 1f;
			if (!Custom.DistLess(scavenger.mainBodyChunk.pos, float2.op_Implicit(pos), armLength * 1.5f * num))
			{
				return Custom.DistLess(scavenger.mainBodyChunk.pos + scavenger.mainBodyChunk.vel * 5f, float2.op_Implicit(pos), armLength * 1.5f * num);
			}
			return true;
		}

		[Obsolete("Use float2 parameter function instead.")]
		public bool GrabPosLegal(Vector2 pos)
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			return GrabPosLegal(new float2(pos.x, pos.y));
		}

		public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			TriangleMesh.Triangle[] tris = new TriangleMesh.Triangle[17]
			{
				new TriangleMesh.Triangle(0, 18, 1),
				new TriangleMesh.Triangle(18, 17, 1),
				new TriangleMesh.Triangle(16, 17, 1),
				new TriangleMesh.Triangle(1, 15, 16),
				new TriangleMesh.Triangle(1, 2, 15),
				new TriangleMesh.Triangle(14, 2, 15),
				new TriangleMesh.Triangle(3, 2, 14),
				new TriangleMesh.Triangle(14, 3, 13),
				new TriangleMesh.Triangle(4, 3, 13),
				new TriangleMesh.Triangle(12, 4, 13),
				new TriangleMesh.Triangle(4, 12, 5),
				new TriangleMesh.Triangle(11, 12, 5),
				new TriangleMesh.Triangle(11, 6, 5),
				new TriangleMesh.Triangle(11, 6, 10),
				new TriangleMesh.Triangle(7, 6, 10),
				new TriangleMesh.Triangle(7, 9, 10),
				new TriangleMesh.Triangle(7, 9, 8)
			};
			sLeaser.sprites[firstSprite] = new TriangleMesh("Futile_White", tris, customColor: true);
			sLeaser.sprites[firstSprite + 1] = new FSprite("pixel");
			sLeaser.sprites[firstSprite + 1].anchorY = 0f;
			sLeaser.sprites[firstSprite + 2] = new FSprite("pixel");
		}

		public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, float2 camPos)
		{
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			//IL_007f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0084: Unknown result type (might be due to invalid IL or missing references)
			//IL_0089: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
			//IL_00de: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
			//IL_0101: Unknown result type (might be due to invalid IL or missing references)
			//IL_011d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0123: Unknown result type (might be due to invalid IL or missing references)
			//IL_013f: Unknown result type (might be due to invalid IL or missing references)
			//IL_015b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0161: Unknown result type (might be due to invalid IL or missing references)
			//IL_0166: Unknown result type (might be due to invalid IL or missing references)
			//IL_0170: Unknown result type (might be due to invalid IL or missing references)
			//IL_0175: Unknown result type (might be due to invalid IL or missing references)
			//IL_017a: Unknown result type (might be due to invalid IL or missing references)
			//IL_020d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0218: Unknown result type (might be due to invalid IL or missing references)
			//IL_021e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0223: Unknown result type (might be due to invalid IL or missing references)
			//IL_0224: Unknown result type (might be due to invalid IL or missing references)
			//IL_0226: Unknown result type (might be due to invalid IL or missing references)
			//IL_022c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0232: Unknown result type (might be due to invalid IL or missing references)
			//IL_0237: Unknown result type (might be due to invalid IL or missing references)
			//IL_023c: Unknown result type (might be due to invalid IL or missing references)
			//IL_02c2: Unknown result type (might be due to invalid IL or missing references)
			//IL_02c3: Unknown result type (might be due to invalid IL or missing references)
			//IL_02c4: Unknown result type (might be due to invalid IL or missing references)
			//IL_02c5: Unknown result type (might be due to invalid IL or missing references)
			//IL_02cf: Unknown result type (might be due to invalid IL or missing references)
			//IL_02d4: Unknown result type (might be due to invalid IL or missing references)
			//IL_02d9: Unknown result type (might be due to invalid IL or missing references)
			//IL_02da: Unknown result type (might be due to invalid IL or missing references)
			//IL_02e0: Unknown result type (might be due to invalid IL or missing references)
			//IL_0332: Unknown result type (might be due to invalid IL or missing references)
			//IL_0338: Unknown result type (might be due to invalid IL or missing references)
			//IL_0354: Unknown result type (might be due to invalid IL or missing references)
			//IL_0359: Unknown result type (might be due to invalid IL or missing references)
			//IL_0372: Unknown result type (might be due to invalid IL or missing references)
			//IL_038e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0394: Unknown result type (might be due to invalid IL or missing references)
			//IL_0399: Unknown result type (might be due to invalid IL or missing references)
			//IL_039b: Unknown result type (might be due to invalid IL or missing references)
			//IL_039c: Unknown result type (might be due to invalid IL or missing references)
			//IL_039e: Unknown result type (might be due to invalid IL or missing references)
			//IL_03a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_03a8: Unknown result type (might be due to invalid IL or missing references)
			//IL_03ad: Unknown result type (might be due to invalid IL or missing references)
			//IL_02f2: Unknown result type (might be due to invalid IL or missing references)
			//IL_02f3: Unknown result type (might be due to invalid IL or missing references)
			//IL_02f4: Unknown result type (might be due to invalid IL or missing references)
			//IL_02f5: Unknown result type (might be due to invalid IL or missing references)
			//IL_02ff: Unknown result type (might be due to invalid IL or missing references)
			//IL_0300: Unknown result type (might be due to invalid IL or missing references)
			//IL_0307: Unknown result type (might be due to invalid IL or missing references)
			//IL_030c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0311: Unknown result type (might be due to invalid IL or missing references)
			//IL_0312: Unknown result type (might be due to invalid IL or missing references)
			//IL_0313: Unknown result type (might be due to invalid IL or missing references)
			//IL_0314: Unknown result type (might be due to invalid IL or missing references)
			//IL_0315: Unknown result type (might be due to invalid IL or missing references)
			//IL_031f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0320: Unknown result type (might be due to invalid IL or missing references)
			//IL_0327: Unknown result type (might be due to invalid IL or missing references)
			//IL_032c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0331: Unknown result type (might be due to invalid IL or missing references)
			//IL_0272: Unknown result type (might be due to invalid IL or missing references)
			//IL_0277: Unknown result type (might be due to invalid IL or missing references)
			//IL_03ec: Unknown result type (might be due to invalid IL or missing references)
			//IL_03ed: Unknown result type (might be due to invalid IL or missing references)
			//IL_0a2d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0a33: Unknown result type (might be due to invalid IL or missing references)
			//IL_0a4f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0a55: Unknown result type (might be due to invalid IL or missing references)
			//IL_02a1: Unknown result type (might be due to invalid IL or missing references)
			//IL_0408: Unknown result type (might be due to invalid IL or missing references)
			//IL_0409: Unknown result type (might be due to invalid IL or missing references)
			//IL_040d: Unknown result type (might be due to invalid IL or missing references)
			//IL_040f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0413: Unknown result type (might be due to invalid IL or missing references)
			//IL_0414: Unknown result type (might be due to invalid IL or missing references)
			//IL_0418: Unknown result type (might be due to invalid IL or missing references)
			//IL_0419: Unknown result type (might be due to invalid IL or missing references)
			//IL_041b: Unknown result type (might be due to invalid IL or missing references)
			//IL_041d: Unknown result type (might be due to invalid IL or missing references)
			//IL_041f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0424: Unknown result type (might be due to invalid IL or missing references)
			//IL_0429: Unknown result type (might be due to invalid IL or missing references)
			//IL_042e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0430: Unknown result type (might be due to invalid IL or missing references)
			//IL_0432: Unknown result type (might be due to invalid IL or missing references)
			//IL_0437: Unknown result type (might be due to invalid IL or missing references)
			//IL_0439: Unknown result type (might be due to invalid IL or missing references)
			//IL_043b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0a9e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0aa4: Unknown result type (might be due to invalid IL or missing references)
			//IL_0473: Unknown result type (might be due to invalid IL or missing references)
			//IL_0475: Unknown result type (might be due to invalid IL or missing references)
			//IL_0477: Unknown result type (might be due to invalid IL or missing references)
			//IL_049a: Unknown result type (might be due to invalid IL or missing references)
			//IL_049c: Unknown result type (might be due to invalid IL or missing references)
			//IL_04a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_04aa: Unknown result type (might be due to invalid IL or missing references)
			//IL_04af: Unknown result type (might be due to invalid IL or missing references)
			//IL_04b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_04b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_04c2: Unknown result type (might be due to invalid IL or missing references)
			//IL_04c7: Unknown result type (might be due to invalid IL or missing references)
			//IL_04cc: Unknown result type (might be due to invalid IL or missing references)
			//IL_04ce: Unknown result type (might be due to invalid IL or missing references)
			//IL_04f0: Unknown result type (might be due to invalid IL or missing references)
			//IL_04f2: Unknown result type (might be due to invalid IL or missing references)
			//IL_04f4: Unknown result type (might be due to invalid IL or missing references)
			//IL_04f6: Unknown result type (might be due to invalid IL or missing references)
			//IL_04fb: Unknown result type (might be due to invalid IL or missing references)
			//IL_0502: Unknown result type (might be due to invalid IL or missing references)
			//IL_050c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0511: Unknown result type (might be due to invalid IL or missing references)
			//IL_0516: Unknown result type (might be due to invalid IL or missing references)
			//IL_0518: Unknown result type (might be due to invalid IL or missing references)
			//IL_051d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0524: Unknown result type (might be due to invalid IL or missing references)
			//IL_0529: Unknown result type (might be due to invalid IL or missing references)
			//IL_052b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0553: Unknown result type (might be due to invalid IL or missing references)
			//IL_0555: Unknown result type (might be due to invalid IL or missing references)
			//IL_055c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0561: Unknown result type (might be due to invalid IL or missing references)
			//IL_0563: Unknown result type (might be due to invalid IL or missing references)
			//IL_0568: Unknown result type (might be due to invalid IL or missing references)
			//IL_056f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0579: Unknown result type (might be due to invalid IL or missing references)
			//IL_057e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0583: Unknown result type (might be due to invalid IL or missing references)
			//IL_0585: Unknown result type (might be due to invalid IL or missing references)
			//IL_05a8: Unknown result type (might be due to invalid IL or missing references)
			//IL_05aa: Unknown result type (might be due to invalid IL or missing references)
			//IL_05ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_05b3: Unknown result type (might be due to invalid IL or missing references)
			//IL_05b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_05bc: Unknown result type (might be due to invalid IL or missing references)
			//IL_05c6: Unknown result type (might be due to invalid IL or missing references)
			//IL_05cb: Unknown result type (might be due to invalid IL or missing references)
			//IL_05d0: Unknown result type (might be due to invalid IL or missing references)
			//IL_05d2: Unknown result type (might be due to invalid IL or missing references)
			//IL_05f5: Unknown result type (might be due to invalid IL or missing references)
			//IL_05f7: Unknown result type (might be due to invalid IL or missing references)
			//IL_05fb: Unknown result type (might be due to invalid IL or missing references)
			//IL_0605: Unknown result type (might be due to invalid IL or missing references)
			//IL_060a: Unknown result type (might be due to invalid IL or missing references)
			//IL_060f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0613: Unknown result type (might be due to invalid IL or missing references)
			//IL_061d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0622: Unknown result type (might be due to invalid IL or missing references)
			//IL_0627: Unknown result type (might be due to invalid IL or missing references)
			//IL_0629: Unknown result type (might be due to invalid IL or missing references)
			//IL_064b: Unknown result type (might be due to invalid IL or missing references)
			//IL_064d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0651: Unknown result type (might be due to invalid IL or missing references)
			//IL_065b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0660: Unknown result type (might be due to invalid IL or missing references)
			//IL_0665: Unknown result type (might be due to invalid IL or missing references)
			//IL_0669: Unknown result type (might be due to invalid IL or missing references)
			//IL_0673: Unknown result type (might be due to invalid IL or missing references)
			//IL_0678: Unknown result type (might be due to invalid IL or missing references)
			//IL_067d: Unknown result type (might be due to invalid IL or missing references)
			//IL_067f: Unknown result type (might be due to invalid IL or missing references)
			//IL_06a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_06a4: Unknown result type (might be due to invalid IL or missing references)
			//IL_06a8: Unknown result type (might be due to invalid IL or missing references)
			//IL_06ad: Unknown result type (might be due to invalid IL or missing references)
			//IL_06b2: Unknown result type (might be due to invalid IL or missing references)
			//IL_06b6: Unknown result type (might be due to invalid IL or missing references)
			//IL_06c0: Unknown result type (might be due to invalid IL or missing references)
			//IL_06c5: Unknown result type (might be due to invalid IL or missing references)
			//IL_06ca: Unknown result type (might be due to invalid IL or missing references)
			//IL_06cc: Unknown result type (might be due to invalid IL or missing references)
			//IL_06ee: Unknown result type (might be due to invalid IL or missing references)
			//IL_06f0: Unknown result type (might be due to invalid IL or missing references)
			//IL_06f4: Unknown result type (might be due to invalid IL or missing references)
			//IL_06fe: Unknown result type (might be due to invalid IL or missing references)
			//IL_0703: Unknown result type (might be due to invalid IL or missing references)
			//IL_0708: Unknown result type (might be due to invalid IL or missing references)
			//IL_070c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0716: Unknown result type (might be due to invalid IL or missing references)
			//IL_071b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0720: Unknown result type (might be due to invalid IL or missing references)
			//IL_0722: Unknown result type (might be due to invalid IL or missing references)
			//IL_074a: Unknown result type (might be due to invalid IL or missing references)
			//IL_074c: Unknown result type (might be due to invalid IL or missing references)
			//IL_074e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0750: Unknown result type (might be due to invalid IL or missing references)
			//IL_0755: Unknown result type (might be due to invalid IL or missing references)
			//IL_075c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0766: Unknown result type (might be due to invalid IL or missing references)
			//IL_076b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0770: Unknown result type (might be due to invalid IL or missing references)
			//IL_0772: Unknown result type (might be due to invalid IL or missing references)
			//IL_0794: Unknown result type (might be due to invalid IL or missing references)
			//IL_0796: Unknown result type (might be due to invalid IL or missing references)
			//IL_0798: Unknown result type (might be due to invalid IL or missing references)
			//IL_079f: Unknown result type (might be due to invalid IL or missing references)
			//IL_07a6: Unknown result type (might be due to invalid IL or missing references)
			//IL_07b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_07b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_07ba: Unknown result type (might be due to invalid IL or missing references)
			//IL_07bc: Unknown result type (might be due to invalid IL or missing references)
			//IL_07df: Unknown result type (might be due to invalid IL or missing references)
			//IL_07e1: Unknown result type (might be due to invalid IL or missing references)
			//IL_07e5: Unknown result type (might be due to invalid IL or missing references)
			//IL_07ef: Unknown result type (might be due to invalid IL or missing references)
			//IL_07f4: Unknown result type (might be due to invalid IL or missing references)
			//IL_07f9: Unknown result type (might be due to invalid IL or missing references)
			//IL_07fd: Unknown result type (might be due to invalid IL or missing references)
			//IL_0807: Unknown result type (might be due to invalid IL or missing references)
			//IL_080c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0811: Unknown result type (might be due to invalid IL or missing references)
			//IL_0813: Unknown result type (might be due to invalid IL or missing references)
			//IL_0835: Unknown result type (might be due to invalid IL or missing references)
			//IL_0837: Unknown result type (might be due to invalid IL or missing references)
			//IL_0839: Unknown result type (might be due to invalid IL or missing references)
			//IL_0840: Unknown result type (might be due to invalid IL or missing references)
			//IL_0847: Unknown result type (might be due to invalid IL or missing references)
			//IL_0851: Unknown result type (might be due to invalid IL or missing references)
			//IL_0856: Unknown result type (might be due to invalid IL or missing references)
			//IL_085b: Unknown result type (might be due to invalid IL or missing references)
			//IL_085d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0880: Unknown result type (might be due to invalid IL or missing references)
			//IL_0882: Unknown result type (might be due to invalid IL or missing references)
			//IL_0886: Unknown result type (might be due to invalid IL or missing references)
			//IL_0890: Unknown result type (might be due to invalid IL or missing references)
			//IL_0895: Unknown result type (might be due to invalid IL or missing references)
			//IL_089a: Unknown result type (might be due to invalid IL or missing references)
			//IL_089e: Unknown result type (might be due to invalid IL or missing references)
			//IL_08a8: Unknown result type (might be due to invalid IL or missing references)
			//IL_08ad: Unknown result type (might be due to invalid IL or missing references)
			//IL_08b2: Unknown result type (might be due to invalid IL or missing references)
			//IL_08b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_08d6: Unknown result type (might be due to invalid IL or missing references)
			//IL_08d8: Unknown result type (might be due to invalid IL or missing references)
			//IL_08dc: Unknown result type (might be due to invalid IL or missing references)
			//IL_08e6: Unknown result type (might be due to invalid IL or missing references)
			//IL_08eb: Unknown result type (might be due to invalid IL or missing references)
			//IL_08f0: Unknown result type (might be due to invalid IL or missing references)
			//IL_08f4: Unknown result type (might be due to invalid IL or missing references)
			//IL_08fe: Unknown result type (might be due to invalid IL or missing references)
			//IL_0903: Unknown result type (might be due to invalid IL or missing references)
			//IL_0908: Unknown result type (might be due to invalid IL or missing references)
			//IL_090a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0932: Unknown result type (might be due to invalid IL or missing references)
			//IL_0934: Unknown result type (might be due to invalid IL or missing references)
			//IL_0936: Unknown result type (might be due to invalid IL or missing references)
			//IL_0938: Unknown result type (might be due to invalid IL or missing references)
			//IL_093d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0944: Unknown result type (might be due to invalid IL or missing references)
			//IL_0949: Unknown result type (might be due to invalid IL or missing references)
			//IL_094e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0950: Unknown result type (might be due to invalid IL or missing references)
			//IL_0972: Unknown result type (might be due to invalid IL or missing references)
			//IL_0974: Unknown result type (might be due to invalid IL or missing references)
			//IL_0976: Unknown result type (might be due to invalid IL or missing references)
			//IL_0978: Unknown result type (might be due to invalid IL or missing references)
			//IL_097d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0984: Unknown result type (might be due to invalid IL or missing references)
			//IL_0989: Unknown result type (might be due to invalid IL or missing references)
			//IL_098e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0990: Unknown result type (might be due to invalid IL or missing references)
			//IL_09b3: Unknown result type (might be due to invalid IL or missing references)
			//IL_09b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_09b9: Unknown result type (might be due to invalid IL or missing references)
			//IL_09be: Unknown result type (might be due to invalid IL or missing references)
			//IL_09c3: Unknown result type (might be due to invalid IL or missing references)
			//IL_09c5: Unknown result type (might be due to invalid IL or missing references)
			//IL_09e7: Unknown result type (might be due to invalid IL or missing references)
			//IL_09e9: Unknown result type (might be due to invalid IL or missing references)
			//IL_09ed: Unknown result type (might be due to invalid IL or missing references)
			//IL_09f2: Unknown result type (might be due to invalid IL or missing references)
			//IL_09f7: Unknown result type (might be due to invalid IL or missing references)
			//IL_09f9: Unknown result type (might be due to invalid IL or missing references)
			//IL_0a08: Unknown result type (might be due to invalid IL or missing references)
			//IL_0a0a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0a0c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0a0e: Unknown result type (might be due to invalid IL or missing references)
			float2 val = math.lerp(graphics.drawPositions[graphics.chestDrawPos, 1], graphics.drawPositions[graphics.chestDrawPos, 0], timeStacker);
			val += Custom.PerpendicularVector(ExtraExtentions.normalized(val - math.lerp(graphics.drawPositions[graphics.hipsDrawPos, 1], graphics.drawPositions[graphics.hipsDrawPos, 0], timeStacker))) * (1f - Mathf.Abs(Mathf.Lerp(graphics.lastFlip, graphics.flip, timeStacker))) * 10f * (((float)limbNumber == 0f) ? (-1f) : 1f);
			val += Custom.DirVec(math.lerp(graphics.drawPositions[graphics.hipsDrawPos, 1], graphics.drawPositions[graphics.hipsDrawPos, 0], timeStacker), math.lerp(graphics.drawPositions[graphics.chestDrawPos, 1], graphics.drawPositions[graphics.chestDrawPos, 0], timeStacker)) * 5f;
			bool flag = reachedSnapPosition;
			if (scavenger.Pointing && limbNumber == (scavenger.animation as Scavenger.PointingAnimation).PointingArm)
			{
				flag = false;
			}
			else if (scavenger.animation != null && scavenger.animation is Scavenger.CommunicationAnimation && limbNumber == (scavenger.animation as Scavenger.CommunicationAnimation).GestureArm)
			{
				flag = false;
			}
			if (limbNumber == 0 && scavenger.grasps[0] != null)
			{
				flag = true;
			}
			float2 val2 = math.lerp(float2.op_Implicit(lastPos), float2.op_Implicit(pos), timeStacker);
			val2 += math.lerp(lastSpearPosAdd, spearPosAdd, timeStacker);
			if (scavenger.swingPos.HasValue)
			{
				if (limbNumber == scavenger.swingArm)
				{
					val2 = float2.op_Implicit(scavenger.swingPos.Value);
					flag = true;
				}
				else if (scavenger.nextSwingPos.HasValue && limbNumber != scavenger.swingArm)
				{
					flag = Custom.DistLess(float2.op_Implicit(val2), scavenger.nextSwingPos.Value, 5f);
				}
			}
			float2 val3 = val2 + Custom.DirVec(val2, val) * 6f;
			if (Custom.DistLess(float2.op_Implicit(val3), float2.op_Implicit(val), 5f))
			{
				val2 += Custom.DirVec(val, val3) * (5f - math.distance(val, val3));
				val3 += Custom.DirVec(val, val3) * (5f - math.distance(val, val3));
			}
			float2 val4 = float2.op_Implicit(Custom.InverseKinematic(float2.op_Implicit(val), float2.op_Implicit(val3), 18f, 18f, MyFlip(timeStacker)));
			float2 val5 = math.lerp(graphics.drawPositions[graphics.chestDrawPos, 1], graphics.drawPositions[graphics.chestDrawPos, 0], timeStacker);
			float2 val6 = Custom.PerpendicularVector(ExtraExtentions.normalized(val - val5));
			float num = (0f - (1.75f + Mathf.Lerp(-1f, 1f, graphics.iVars.armThickness))) * Mathf.Sign(MyFlip(timeStacker));
			for (int i = 0; i < 4; i++)
			{
				float2 val7 = val;
				switch (i)
				{
				case 0:
					val7 = val;
					break;
				case 1:
					val7 = val4;
					break;
				case 2:
					val7 = val3;
					break;
				case 3:
					val7 = val2;
					break;
				}
				float2 val8 = -ExtraExtentions.normalized(val5 - val7);
				float2 val9 = Custom.PerpendicularVector(val8);
				float num2 = math.distance(val7, val5);
				switch (i)
				{
				case 0:
					(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(0, float2.op_Implicit(val5 - camPos));
					(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(18, float2.op_Implicit(val7 - val6 * num * 2f - val8 * num2 * 0.2f - camPos));
					(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(1, float2.op_Implicit(math.lerp(val7 + ExtraExtentions.normalized(val9 + val6) * num * 2f - val8, val5, 0.5f) - camPos));
					break;
				case 1:
					(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(17, float2.op_Implicit(val5 - ExtraExtentions.normalized(val9 * 2f + val6) * num * 2f - camPos));
					(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(16, float2.op_Implicit(val7 - val9 * num - val8 * num2 * 0.8f - camPos));
					(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(15, float2.op_Implicit(val7 - val9 * num * 2.5f - val8 * num2 * 0.5f - camPos));
					(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(2, float2.op_Implicit(val7 + val9 * num * 2.5f - val8 * num2 * 0.5f - camPos));
					(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(14, float2.op_Implicit(val7 - val9 * num - val8 * num2 * 0.2f - camPos));
					(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(3, float2.op_Implicit(val7 + val9 * num * 1.6f - val8 * num2 * 0.2f - camPos));
					break;
				case 2:
					(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(13, float2.op_Implicit(val5 - ExtraExtentions.normalized(val9 + val6) * num * 2f - camPos));
					(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(4, float2.op_Implicit(val5 + Vector3Ext.Slerp2(val9, val6, 0.7f) * num * 2f - camPos));
					(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(12, float2.op_Implicit(val7 - val9 * num * 1.3f - val8 * num2 * 0.8f - camPos));
					(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(5, float2.op_Implicit(val5 + Vector3Ext.Slerp2(val9, val6, 0.3f) * num * 2f - camPos));
					(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(11, float2.op_Implicit(val7 - val9 * num * 1.5f - val8 * num2 * 0.65f - camPos));
					(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(6, float2.op_Implicit(val7 + val9 * num * 1.5f - val8 * num2 * 0.65f - camPos));
					break;
				case 3:
					(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(10, float2.op_Implicit(val5 - ExtraExtentions.normalized(val9 + val6) * num - camPos));
					(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(7, float2.op_Implicit(val5 + ExtraExtentions.normalized(val9 + val6) * num - camPos));
					(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(9, float2.op_Implicit(val7 - val9 * num - camPos));
					(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(8, float2.op_Implicit(val7 + val9 * num - camPos));
					break;
				}
				val5 = val7;
				val6 = val9;
			}
			sLeaser.sprites[firstSprite + 1].x = val3.x - camPos.x;
			sLeaser.sprites[firstSprite + 1].y = val3.y - camPos.y;
			sLeaser.sprites[firstSprite + 1].element = Futile.atlasManager.GetElementWithName(flag ? "ScavengerHandB" : "ScavengerHandA");
			sLeaser.sprites[firstSprite + 1].rotation = Custom.AimFromOneVectorToAnother(float2.op_Implicit(val3), float2.op_Implicit(val2));
			sLeaser.sprites[firstSprite + 1].scaleX = (0f - num * 2f) * 3f / 18f;
		}

		[Obsolete("Use float2 parameter function instead.")]
		public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			DrawSprites(sLeaser, rCam, timeStacker, new float2(camPos.x, camPos.y));
		}

		public float2 WeaponDir()
		{
			//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
			Vector2 vector = Custom.PerpendicularVector(scavenger.mainBodyChunk.pos, pos) * (0f - MyFlip(1f));
			vector += Custom.DirVec(scavenger.bodyChunks[1].pos, scavenger.bodyChunks[0].pos) * ((!scavenger.swingPos.HasValue) ? 1f : 0.1f);
			if (scavenger.animation != null && scavenger.animation.id == Scavenger.ScavengerAnimation.ID.ThrowCharge)
			{
				vector += Custom.DirVec(pos, (scavenger.animation as Scavenger.ThrowChargeAnimation).UseTarget) * 2f;
			}
			else if (scavenger.Pointing && limbNumber == (scavenger.animation as Scavenger.PointingAnimation).PointingArm)
			{
				vector += Custom.DirVec(pos, (scavenger.animation as Scavenger.PointingAnimation).LookPoint) * 7f;
			}
			else if (scavenger.animation != null && scavenger.animation is Scavenger.CommunicationAnimation && (scavenger.animation as Scavenger.CommunicationAnimation).pointWithSpears && limbNumber == (scavenger.animation as Scavenger.CommunicationAnimation).GestureArm)
			{
				vector += Custom.DirVec(scavenger.mainBodyChunk.pos, (scavenger.animation as Scavenger.CommunicationAnimation).GestureArmPos()) * 7f;
			}
			return ExtraExtentions.ToF2(vector.normalized);
		}
	}

	public class ScavengerLeg : Limb
	{
		public int firstSprite;

		public float legLength;

		public ScavengerGraphics graphics => owner as ScavengerGraphics;

		public Scavenger scavenger => graphics.scavenger;

		public float2 IdealPos
		{
			get
			{
				//IL_0058: Unknown result type (might be due to invalid IL or missing references)
				//IL_0047: Unknown result type (might be due to invalid IL or missing references)
				if (scavenger.movMode == Scavenger.MovementMode.Run)
				{
					return float2.op_Implicit(connection.pos + Vector2.ClampMagnitude(connection.vel * 10f, legLength));
				}
				return float2.op_Implicit(connection.pos);
			}
		}

		public ScavengerLeg(ScavengerGraphics owner, int num, int firstSprite)
			: base(owner, owner.owner.bodyChunks[1], num, 1f, 0.5f, 0.99f, 12f, 0.95f)
		{
			this.firstSprite = firstSprite;
			legLength = Mathf.Lerp(20f, 40f, owner.iVars.legsSize);
		}

		public override void Update()
		{
			//IL_0233: Unknown result type (might be due to invalid IL or missing references)
			//IL_0121: Unknown result type (might be due to invalid IL or missing references)
			//IL_014b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0150: Unknown result type (might be due to invalid IL or missing references)
			//IL_016f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0174: Unknown result type (might be due to invalid IL or missing references)
			//IL_0175: Unknown result type (might be due to invalid IL or missing references)
			//IL_03aa: Unknown result type (might be due to invalid IL or missing references)
			//IL_03d8: Unknown result type (might be due to invalid IL or missing references)
			//IL_0447: Unknown result type (might be due to invalid IL or missing references)
			//IL_0452: Unknown result type (might be due to invalid IL or missing references)
			//IL_04aa: Unknown result type (might be due to invalid IL or missing references)
			//IL_04b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_0290: Unknown result type (might be due to invalid IL or missing references)
			//IL_02ac: Unknown result type (might be due to invalid IL or missing references)
			//IL_02b7: Unknown result type (might be due to invalid IL or missing references)
			//IL_02c8: Unknown result type (might be due to invalid IL or missing references)
			//IL_02e6: Unknown result type (might be due to invalid IL or missing references)
			//IL_02f0: Unknown result type (might be due to invalid IL or missing references)
			//IL_031c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0321: Unknown result type (might be due to invalid IL or missing references)
			base.Update();
			ConnectToPoint(connection.pos, legLength, push: false, 0f, connection.vel, 0.8f, 0.5f);
			if (scavenger.Consious)
			{
				if (scavenger.movMode == Scavenger.MovementMode.Swim)
				{
					base.mode = Mode.Dangle;
					float2 val = default(float2);
					((float2)(ref val))._002Ector(((limbNumber == 0) ? (-10f) : 10f) - scavenger.flip * 7f + Mathf.Sin((graphics.cycle + (float)limbNumber * 0.5f) * (float)Math.PI * 2f) * 12.5f * scavenger.flip, (0f - legLength) * 0.6f + Mathf.Cos((graphics.cycle + (float)limbNumber * 0.5f) * (float)Math.PI * 2f) * 3.5f * scavenger.flip);
					val = Custom.RotateAroundOrigo(val, Custom.AimFromOneVectorToAnother(scavenger.bodyChunks[1].pos, scavenger.bodyChunks[0].pos));
					vel += Vector2.ClampMagnitude(ExtraExtentions.ToV2(graphics.drawPositions[graphics.hipsDrawPos, 0] + val) - pos, 8f) / 2f;
				}
				else if (scavenger.movMode != Scavenger.MovementMode.Climb || !scavenger.swingPos.HasValue || scavenger.swingClimbCounter < 30)
				{
					vel += Custom.DirVec(scavenger.mainBodyChunk.pos, scavenger.bodyChunks[1].pos);
					if (base.mode == Mode.HuntAbsolutePosition)
					{
						if (!Custom.DistLess(float2.op_Implicit(IdealPos), absoluteHuntPos, legLength))
						{
							base.mode = Mode.HuntRelativePosition;
						}
					}
					else
					{
						relativeHuntPos = float2.op_Implicit(new float2((0f - scavenger.flip) * 10f + ((limbNumber == 0) ? (-4f) : 4f), 10f));
						FindGrip(scavenger.room, float2.op_Implicit(IdealPos), float2.op_Implicit(IdealPos), legLength, float2.op_Implicit(IdealPos + Custom.RotateAroundOrigo(new float2((limbNumber == 0) ? (-1f) : 1f, 0f) * 5f, Custom.AimFromOneVectorToAnother(scavenger.bodyChunks[1].pos, scavenger.mainBodyChunk.pos))), -2, -2, behindWalls: false);
					}
				}
				else
				{
					base.mode = Mode.Dangle;
				}
			}
			else
			{
				base.mode = Mode.Dangle;
			}
			if (base.mode == Mode.Dangle)
			{
				vel.y -= 0.9f;
				vel += ExtraExtentions.ToV2(Custom.RotateAroundOrigo(new float2(0.1f * scavenger.flip + ((limbNumber == 0) ? (-0.15f) : 0.15f), -0.1f), Custom.AimFromOneVectorToAnother(scavenger.bodyChunks[1].pos, scavenger.bodyChunks[0].pos)));
				if (!Custom.DistLess(pos, connection.pos, legLength / 3f))
				{
					vel += Custom.DirVec(connection.pos, pos) * (legLength / 3f - math.distance(float2.op_Implicit(connection.pos), float2.op_Implicit(pos))) / 15f;
					pos += Custom.DirVec(connection.pos, pos) * (legLength / 3f - math.distance(float2.op_Implicit(connection.pos), float2.op_Implicit(pos))) / 15f;
				}
			}
		}

		public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			TriangleMesh.Triangle[] tris = new TriangleMesh.Triangle[10]
			{
				new TriangleMesh.Triangle(0, 1, 2),
				new TriangleMesh.Triangle(1, 2, 3),
				new TriangleMesh.Triangle(3, 4, 2),
				new TriangleMesh.Triangle(4, 2, 5),
				new TriangleMesh.Triangle(2, 5, 6),
				new TriangleMesh.Triangle(5, 6, 7),
				new TriangleMesh.Triangle(5, 7, 8),
				new TriangleMesh.Triangle(8, 7, 9),
				new TriangleMesh.Triangle(8, 9, 10),
				new TriangleMesh.Triangle(8, 9, 11)
			};
			sLeaser.sprites[firstSprite] = new TriangleMesh("Futile_White", tris, customColor: true);
			sLeaser.sprites[firstSprite + 1] = new FSprite("pixel");
		}

		public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, float2 camPos)
		{
			//IL_007d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0099: Unknown result type (might be due to invalid IL or missing references)
			//IL_009f: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00da: Unknown result type (might be due to invalid IL or missing references)
			//IL_00db: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
			//IL_0102: Unknown result type (might be due to invalid IL or missing references)
			//IL_0107: Unknown result type (might be due to invalid IL or missing references)
			//IL_010c: Unknown result type (might be due to invalid IL or missing references)
			//IL_010d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0113: Unknown result type (might be due to invalid IL or missing references)
			//IL_013d: Unknown result type (might be due to invalid IL or missing references)
			//IL_013e: Unknown result type (might be due to invalid IL or missing references)
			//IL_013f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0140: Unknown result type (might be due to invalid IL or missing references)
			//IL_014a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0150: Unknown result type (might be due to invalid IL or missing references)
			//IL_0155: Unknown result type (might be due to invalid IL or missing references)
			//IL_015a: Unknown result type (might be due to invalid IL or missing references)
			//IL_015c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0162: Unknown result type (might be due to invalid IL or missing references)
			//IL_017e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0183: Unknown result type (might be due to invalid IL or missing references)
			//IL_0185: Unknown result type (might be due to invalid IL or missing references)
			//IL_018c: Unknown result type (might be due to invalid IL or missing references)
			//IL_018d: Unknown result type (might be due to invalid IL or missing references)
			//IL_018f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0199: Unknown result type (might be due to invalid IL or missing references)
			//IL_0125: Unknown result type (might be due to invalid IL or missing references)
			//IL_0126: Unknown result type (might be due to invalid IL or missing references)
			//IL_0127: Unknown result type (might be due to invalid IL or missing references)
			//IL_0128: Unknown result type (might be due to invalid IL or missing references)
			//IL_0132: Unknown result type (might be due to invalid IL or missing references)
			//IL_0137: Unknown result type (might be due to invalid IL or missing references)
			//IL_013c: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f5: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
			//IL_0201: Unknown result type (might be due to invalid IL or missing references)
			//IL_0207: Unknown result type (might be due to invalid IL or missing references)
			//IL_020c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0211: Unknown result type (might be due to invalid IL or missing references)
			//IL_0212: Unknown result type (might be due to invalid IL or missing references)
			//IL_0213: Unknown result type (might be due to invalid IL or missing references)
			//IL_0215: Unknown result type (might be due to invalid IL or missing references)
			//IL_0217: Unknown result type (might be due to invalid IL or missing references)
			//IL_0221: Unknown result type (might be due to invalid IL or missing references)
			//IL_0227: Unknown result type (might be due to invalid IL or missing references)
			//IL_022c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0231: Unknown result type (might be due to invalid IL or missing references)
			//IL_0232: Unknown result type (might be due to invalid IL or missing references)
			//IL_0233: Unknown result type (might be due to invalid IL or missing references)
			//IL_0235: Unknown result type (might be due to invalid IL or missing references)
			//IL_023a: Unknown result type (might be due to invalid IL or missing references)
			//IL_023c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0246: Unknown result type (might be due to invalid IL or missing references)
			//IL_024b: Unknown result type (might be due to invalid IL or missing references)
			//IL_024c: Unknown result type (might be due to invalid IL or missing references)
			//IL_024e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0258: Unknown result type (might be due to invalid IL or missing references)
			//IL_0262: Unknown result type (might be due to invalid IL or missing references)
			//IL_0267: Unknown result type (might be due to invalid IL or missing references)
			//IL_0269: Unknown result type (might be due to invalid IL or missing references)
			//IL_026b: Unknown result type (might be due to invalid IL or missing references)
			//IL_026d: Unknown result type (might be due to invalid IL or missing references)
			//IL_026e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0273: Unknown result type (might be due to invalid IL or missing references)
			//IL_0278: Unknown result type (might be due to invalid IL or missing references)
			//IL_027e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0283: Unknown result type (might be due to invalid IL or missing references)
			//IL_0285: Unknown result type (might be due to invalid IL or missing references)
			//IL_0286: Unknown result type (might be due to invalid IL or missing references)
			//IL_02ab: Unknown result type (might be due to invalid IL or missing references)
			//IL_02b6: Unknown result type (might be due to invalid IL or missing references)
			//IL_02bb: Unknown result type (might be due to invalid IL or missing references)
			//IL_02c0: Unknown result type (might be due to invalid IL or missing references)
			//IL_02d5: Unknown result type (might be due to invalid IL or missing references)
			//IL_02d6: Unknown result type (might be due to invalid IL or missing references)
			//IL_02d7: Unknown result type (might be due to invalid IL or missing references)
			//IL_02d9: Unknown result type (might be due to invalid IL or missing references)
			//IL_02de: Unknown result type (might be due to invalid IL or missing references)
			//IL_02e3: Unknown result type (might be due to invalid IL or missing references)
			//IL_02ed: Unknown result type (might be due to invalid IL or missing references)
			//IL_02f3: Unknown result type (might be due to invalid IL or missing references)
			//IL_02ff: Unknown result type (might be due to invalid IL or missing references)
			//IL_0304: Unknown result type (might be due to invalid IL or missing references)
			//IL_0309: Unknown result type (might be due to invalid IL or missing references)
			//IL_030b: Unknown result type (might be due to invalid IL or missing references)
			//IL_032d: Unknown result type (might be due to invalid IL or missing references)
			//IL_032e: Unknown result type (might be due to invalid IL or missing references)
			//IL_032f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0331: Unknown result type (might be due to invalid IL or missing references)
			//IL_0336: Unknown result type (might be due to invalid IL or missing references)
			//IL_033b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0345: Unknown result type (might be due to invalid IL or missing references)
			//IL_034b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0357: Unknown result type (might be due to invalid IL or missing references)
			//IL_035c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0361: Unknown result type (might be due to invalid IL or missing references)
			//IL_0363: Unknown result type (might be due to invalid IL or missing references)
			//IL_0385: Unknown result type (might be due to invalid IL or missing references)
			//IL_0387: Unknown result type (might be due to invalid IL or missing references)
			//IL_0389: Unknown result type (might be due to invalid IL or missing references)
			//IL_03ab: Unknown result type (might be due to invalid IL or missing references)
			//IL_03ad: Unknown result type (might be due to invalid IL or missing references)
			//IL_03af: Unknown result type (might be due to invalid IL or missing references)
			//IL_03b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_03b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_03bf: Unknown result type (might be due to invalid IL or missing references)
			//IL_03c5: Unknown result type (might be due to invalid IL or missing references)
			//IL_03ca: Unknown result type (might be due to invalid IL or missing references)
			//IL_03cf: Unknown result type (might be due to invalid IL or missing references)
			//IL_03d1: Unknown result type (might be due to invalid IL or missing references)
			//IL_03f3: Unknown result type (might be due to invalid IL or missing references)
			//IL_03f5: Unknown result type (might be due to invalid IL or missing references)
			//IL_03f7: Unknown result type (might be due to invalid IL or missing references)
			//IL_03f8: Unknown result type (might be due to invalid IL or missing references)
			//IL_03fd: Unknown result type (might be due to invalid IL or missing references)
			//IL_0407: Unknown result type (might be due to invalid IL or missing references)
			//IL_040d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0412: Unknown result type (might be due to invalid IL or missing references)
			//IL_0417: Unknown result type (might be due to invalid IL or missing references)
			//IL_0419: Unknown result type (might be due to invalid IL or missing references)
			//IL_043b: Unknown result type (might be due to invalid IL or missing references)
			//IL_043d: Unknown result type (might be due to invalid IL or missing references)
			//IL_043f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0440: Unknown result type (might be due to invalid IL or missing references)
			//IL_0445: Unknown result type (might be due to invalid IL or missing references)
			//IL_044f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0455: Unknown result type (might be due to invalid IL or missing references)
			//IL_045a: Unknown result type (might be due to invalid IL or missing references)
			//IL_045f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0461: Unknown result type (might be due to invalid IL or missing references)
			//IL_0483: Unknown result type (might be due to invalid IL or missing references)
			//IL_0485: Unknown result type (might be due to invalid IL or missing references)
			//IL_0487: Unknown result type (might be due to invalid IL or missing references)
			//IL_0489: Unknown result type (might be due to invalid IL or missing references)
			//IL_048e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0493: Unknown result type (might be due to invalid IL or missing references)
			//IL_0494: Unknown result type (might be due to invalid IL or missing references)
			//IL_0496: Unknown result type (might be due to invalid IL or missing references)
			//IL_049b: Unknown result type (might be due to invalid IL or missing references)
			//IL_04a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_04a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_04aa: Unknown result type (might be due to invalid IL or missing references)
			//IL_04ab: Unknown result type (might be due to invalid IL or missing references)
			//IL_04cf: Unknown result type (might be due to invalid IL or missing references)
			//IL_04d5: Unknown result type (might be due to invalid IL or missing references)
			//IL_04da: Unknown result type (might be due to invalid IL or missing references)
			//IL_04df: Unknown result type (might be due to invalid IL or missing references)
			//IL_04e1: Unknown result type (might be due to invalid IL or missing references)
			//IL_0503: Unknown result type (might be due to invalid IL or missing references)
			//IL_0505: Unknown result type (might be due to invalid IL or missing references)
			//IL_0506: Unknown result type (might be due to invalid IL or missing references)
			//IL_0507: Unknown result type (might be due to invalid IL or missing references)
			//IL_050c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0516: Unknown result type (might be due to invalid IL or missing references)
			//IL_051c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0521: Unknown result type (might be due to invalid IL or missing references)
			//IL_0526: Unknown result type (might be due to invalid IL or missing references)
			//IL_0528: Unknown result type (might be due to invalid IL or missing references)
			//IL_054a: Unknown result type (might be due to invalid IL or missing references)
			//IL_054c: Unknown result type (might be due to invalid IL or missing references)
			//IL_054d: Unknown result type (might be due to invalid IL or missing references)
			//IL_054e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0553: Unknown result type (might be due to invalid IL or missing references)
			//IL_055d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0563: Unknown result type (might be due to invalid IL or missing references)
			//IL_0568: Unknown result type (might be due to invalid IL or missing references)
			//IL_056d: Unknown result type (might be due to invalid IL or missing references)
			//IL_056f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0591: Unknown result type (might be due to invalid IL or missing references)
			//IL_0592: Unknown result type (might be due to invalid IL or missing references)
			//IL_0594: Unknown result type (might be due to invalid IL or missing references)
			//IL_0595: Unknown result type (might be due to invalid IL or missing references)
			//IL_059a: Unknown result type (might be due to invalid IL or missing references)
			//IL_059f: Unknown result type (might be due to invalid IL or missing references)
			//IL_05a9: Unknown result type (might be due to invalid IL or missing references)
			//IL_05af: Unknown result type (might be due to invalid IL or missing references)
			//IL_05bb: Unknown result type (might be due to invalid IL or missing references)
			//IL_05c0: Unknown result type (might be due to invalid IL or missing references)
			//IL_05c5: Unknown result type (might be due to invalid IL or missing references)
			//IL_05c7: Unknown result type (might be due to invalid IL or missing references)
			//IL_05ea: Unknown result type (might be due to invalid IL or missing references)
			//IL_05eb: Unknown result type (might be due to invalid IL or missing references)
			//IL_05ed: Unknown result type (might be due to invalid IL or missing references)
			//IL_05ee: Unknown result type (might be due to invalid IL or missing references)
			//IL_05f3: Unknown result type (might be due to invalid IL or missing references)
			//IL_05f8: Unknown result type (might be due to invalid IL or missing references)
			//IL_0602: Unknown result type (might be due to invalid IL or missing references)
			//IL_0608: Unknown result type (might be due to invalid IL or missing references)
			//IL_0614: Unknown result type (might be due to invalid IL or missing references)
			//IL_0619: Unknown result type (might be due to invalid IL or missing references)
			//IL_061e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0620: Unknown result type (might be due to invalid IL or missing references)
			//IL_0643: Unknown result type (might be due to invalid IL or missing references)
			//IL_0644: Unknown result type (might be due to invalid IL or missing references)
			//IL_0646: Unknown result type (might be due to invalid IL or missing references)
			//IL_0647: Unknown result type (might be due to invalid IL or missing references)
			//IL_064c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0651: Unknown result type (might be due to invalid IL or missing references)
			//IL_065b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0661: Unknown result type (might be due to invalid IL or missing references)
			//IL_066d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0672: Unknown result type (might be due to invalid IL or missing references)
			//IL_0677: Unknown result type (might be due to invalid IL or missing references)
			//IL_0679: Unknown result type (might be due to invalid IL or missing references)
			//IL_067a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0680: Unknown result type (might be due to invalid IL or missing references)
			//IL_068a: Unknown result type (might be due to invalid IL or missing references)
			//IL_068f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0694: Unknown result type (might be due to invalid IL or missing references)
			//IL_0696: Unknown result type (might be due to invalid IL or missing references)
			//IL_06b9: Unknown result type (might be due to invalid IL or missing references)
			//IL_06ba: Unknown result type (might be due to invalid IL or missing references)
			//IL_06bc: Unknown result type (might be due to invalid IL or missing references)
			//IL_06bd: Unknown result type (might be due to invalid IL or missing references)
			//IL_06c2: Unknown result type (might be due to invalid IL or missing references)
			//IL_06c7: Unknown result type (might be due to invalid IL or missing references)
			//IL_06d1: Unknown result type (might be due to invalid IL or missing references)
			//IL_06d7: Unknown result type (might be due to invalid IL or missing references)
			//IL_06e3: Unknown result type (might be due to invalid IL or missing references)
			//IL_06e8: Unknown result type (might be due to invalid IL or missing references)
			//IL_06ed: Unknown result type (might be due to invalid IL or missing references)
			//IL_06ef: Unknown result type (might be due to invalid IL or missing references)
			//IL_06f0: Unknown result type (might be due to invalid IL or missing references)
			//IL_06f6: Unknown result type (might be due to invalid IL or missing references)
			//IL_0700: Unknown result type (might be due to invalid IL or missing references)
			//IL_0705: Unknown result type (might be due to invalid IL or missing references)
			//IL_070a: Unknown result type (might be due to invalid IL or missing references)
			//IL_070c: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_01be: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
			float num = Custom.LerpMap(Mathf.Lerp(graphics.lastFlip, graphics.flip, timeStacker), 0f - (float)limbNumber, 1f - (float)limbNumber, -1f, 1f);
			float num2 = Mathf.Lerp(1f, 1.4f, graphics.iVars.legsSize);
			float2 val = math.lerp(graphics.drawPositions[graphics.hipsDrawPos, 1], graphics.drawPositions[graphics.hipsDrawPos, 0], timeStacker);
			float2 val2 = math.lerp(float2.op_Implicit(lastPos), float2.op_Implicit(pos), timeStacker);
			val2 -= Custom.PerpendicularVector(ExtraExtentions.normalized(val2 - val)) * num2 * Custom.LerpMap(math.distance(val, val2) / num2, 2f, 20f, 6f, -1f) * num;
			if (Custom.DistLess(float2.op_Implicit(val2), float2.op_Implicit(val), 3f))
			{
				val2 = val + Custom.DirVec(val, val2) * 3f;
			}
			float2 val3 = val2 + Custom.DirVec(val2, val) * 1.5f * num2;
			float2 val4 = float2.op_Implicit(Custom.InverseKinematic(float2.op_Implicit(val), float2.op_Implicit(val3), 10f * num2, 10f * num2, 0f - num));
			if (Custom.DistLess(float2.op_Implicit(val4), float2.op_Implicit((val + val3) / 2f), 3f * num2))
			{
				val4 = (val + val3) / 2f + Custom.DirVec((val + val3) / 2f, val4) * 3f * num2;
			}
			val2 += Custom.DirVec(val, val4) * 3f * num2;
			val2 += Custom.DirVec(val3, val4) * 2f * num2;
			float2 val5 = math.lerp((val + val4 + val3) / 3f, (val + val3) / 2f, 0.35f);
			val4 += Custom.PerpendicularVector(ExtraExtentions.normalized(val3 - val)) * num2 * Custom.LerpMap(Mathf.Abs(Custom.DistanceToLine(val4, val, val3)), 0f, 20f, 2f, 0f) * Mathf.Sign(num);
			(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(0, float2.op_Implicit(val - Custom.PerpendicularVector(ExtraExtentions.normalized(val - val4)) * 2f * num2 * (0f - Mathf.Sign(num)) - camPos));
			(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(1, float2.op_Implicit(val + Custom.PerpendicularVector(ExtraExtentions.normalized(val - val4)) * 2f * num2 * (0f - Mathf.Sign(num)) - camPos));
			(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(2, float2.op_Implicit(val5 - camPos));
			(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(3, float2.op_Implicit(val4 - ExtraExtentions.normalized(val3 - val) * 3f * num2 - camPos));
			(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(4, float2.op_Implicit(val4 + ExtraExtentions.normalized(val3 - val) * 3f * num2 - camPos));
			(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(4, float2.op_Implicit(val4 + ExtraExtentions.normalized(val3 - val) * 3f * num2 - camPos));
			(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(5, float2.op_Implicit(val3 - ExtraExtentions.normalized(ExtraExtentions.normalized(val3 - val4) + ExtraExtentions.normalized(val - val4)) * Custom.LerpMap(math.distance(val, val3), 10f * num2, 30f * num2, 3f, 1f) * num2 - camPos));
			(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(6, float2.op_Implicit(val3 + ExtraExtentions.normalized(val - val2) * 0.8f * num2 - camPos));
			(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(7, float2.op_Implicit(val3 - ExtraExtentions.normalized(val - val2) * 0.8f * num2 - camPos));
			(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(8, float2.op_Implicit(val2 + Custom.PerpendicularVector(ExtraExtentions.normalized(val3 - val2)) * 0.8f * num2 * (0f - Mathf.Sign(num)) - camPos));
			(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(9, float2.op_Implicit(val2 - Custom.PerpendicularVector(ExtraExtentions.normalized(val3 - val2)) * 0.8f * num2 * (0f - Mathf.Sign(num)) - camPos));
			(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(10, float2.op_Implicit(val2 + Custom.PerpendicularVector(ExtraExtentions.normalized(val3 - val2)) * 0.7f * num2 * (0f - Mathf.Sign(num)) + Custom.DirVec(val3, val2) * num2 * 2f - camPos));
			(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(11, float2.op_Implicit(val2 - Custom.PerpendicularVector(ExtraExtentions.normalized(val3 - val2)) * 1.2f * num2 * (0f - Mathf.Sign(num)) + Custom.DirVec(val3, val2) * num2 * 2f - camPos));
		}

		[Obsolete("Use float2 parameter function instead.")]
		public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			DrawSprites(sLeaser, rCam, timeStacker, new float2(camPos.x, camPos.y));
		}
	}

	public class Eartlers
	{
		public struct Vertex
		{
			public float2 pos;

			public float rad;

			public Vertex(float2 pos, float rad)
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_0002: Unknown result type (might be due to invalid IL or missing references)
				this.pos = pos;
				this.rad = rad;
			}

			[Obsolete("Use float2 parameter constructor instead.")]
			public Vertex(Vector2 pos, float rad)
				: this(new float2(pos.x, pos.y), rad)
			{
			}//IL_000d: Unknown result type (might be due to invalid IL or missing references)

		}

		public ScavengerGraphics owner;

		public List<Vertex[]> points;

		public int firstSprite;

		public int TotalSprites => points.Count;

		public Eartlers(int firstSprite, ScavengerGraphics owner)
		{
			this.firstSprite = firstSprite;
			this.owner = owner;
			GenerateSegments();
		}

		public void GenerateSegments()
		{
			//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
			//IL_0104: Unknown result type (might be due to invalid IL or missing references)
			//IL_010a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0121: Unknown result type (might be due to invalid IL or missing references)
			//IL_0127: Unknown result type (might be due to invalid IL or missing references)
			//IL_0139: Unknown result type (might be due to invalid IL or missing references)
			//IL_013e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0140: Unknown result type (might be due to invalid IL or missing references)
			//IL_0156: Unknown result type (might be due to invalid IL or missing references)
			//IL_0160: Unknown result type (might be due to invalid IL or missing references)
			//IL_0166: Unknown result type (might be due to invalid IL or missing references)
			//IL_016b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0170: Unknown result type (might be due to invalid IL or missing references)
			//IL_0172: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01be: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f5: Unknown result type (might be due to invalid IL or missing references)
			//IL_021b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0232: Unknown result type (might be due to invalid IL or missing references)
			//IL_0182: Unknown result type (might be due to invalid IL or missing references)
			//IL_0189: Unknown result type (might be due to invalid IL or missing references)
			//IL_019a: Unknown result type (might be due to invalid IL or missing references)
			//IL_026c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0283: Unknown result type (might be due to invalid IL or missing references)
			//IL_02a8: Unknown result type (might be due to invalid IL or missing references)
			//IL_02ad: Unknown result type (might be due to invalid IL or missing references)
			//IL_02b1: Unknown result type (might be due to invalid IL or missing references)
			//IL_02c4: Unknown result type (might be due to invalid IL or missing references)
			//IL_02d8: Unknown result type (might be due to invalid IL or missing references)
			//IL_02dd: Unknown result type (might be due to invalid IL or missing references)
			//IL_02f4: Unknown result type (might be due to invalid IL or missing references)
			//IL_02f9: Unknown result type (might be due to invalid IL or missing references)
			//IL_0309: Unknown result type (might be due to invalid IL or missing references)
			//IL_0313: Unknown result type (might be due to invalid IL or missing references)
			//IL_0318: Unknown result type (might be due to invalid IL or missing references)
			//IL_0355: Unknown result type (might be due to invalid IL or missing references)
			//IL_036c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0385: Unknown result type (might be due to invalid IL or missing references)
			//IL_038a: Unknown result type (might be due to invalid IL or missing references)
			//IL_038e: Unknown result type (might be due to invalid IL or missing references)
			//IL_03a1: Unknown result type (might be due to invalid IL or missing references)
			//IL_03b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_03ba: Unknown result type (might be due to invalid IL or missing references)
			//IL_03d1: Unknown result type (might be due to invalid IL or missing references)
			//IL_03d6: Unknown result type (might be due to invalid IL or missing references)
			//IL_03e6: Unknown result type (might be due to invalid IL or missing references)
			//IL_03f0: Unknown result type (might be due to invalid IL or missing references)
			//IL_03f5: Unknown result type (might be due to invalid IL or missing references)
			//IL_044e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0467: Unknown result type (might be due to invalid IL or missing references)
			//IL_046c: Unknown result type (might be due to invalid IL or missing references)
			//IL_046e: Unknown result type (might be due to invalid IL or missing references)
			//IL_049b: Unknown result type (might be due to invalid IL or missing references)
			//IL_04a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_054f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0565: Unknown result type (might be due to invalid IL or missing references)
			//IL_0581: Unknown result type (might be due to invalid IL or missing references)
			//IL_0597: Unknown result type (might be due to invalid IL or missing references)
			//IL_05b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_05b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_04d6: Unknown result type (might be due to invalid IL or missing references)
			//IL_04ec: Unknown result type (might be due to invalid IL or missing references)
			//IL_0505: Unknown result type (might be due to invalid IL or missing references)
			//IL_050a: Unknown result type (might be due to invalid IL or missing references)
			//IL_050f: Unknown result type (might be due to invalid IL or missing references)
			//IL_051d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0530: Unknown result type (might be due to invalid IL or missing references)
			//IL_061d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0664: Unknown result type (might be due to invalid IL or missing references)
			//IL_066b: Unknown result type (might be due to invalid IL or missing references)
			//IL_06b2: Unknown result type (might be due to invalid IL or missing references)
			//IL_06d8: Unknown result type (might be due to invalid IL or missing references)
			//IL_0715: Unknown result type (might be due to invalid IL or missing references)
			//IL_071a: Unknown result type (might be due to invalid IL or missing references)
			//IL_072f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0751: Unknown result type (might be due to invalid IL or missing references)
			bool elite = owner.scavenger.Elite;
			float num = (elite ? 1.75f : 1f);
			float2 val = default(float2);
			((float2)(ref val))._002Ector(elite ? 45f : 15f, elite ? 90f : 45f);
			float num2 = (elite ? 1.5f : 1f);
			float num3 = (elite ? 2f : 1f);
			float num4 = (elite ? 1f : 1f);
			float num5 = (elite ? 0f : 1f);
			float num6 = (elite ? 2f : 1f);
			float num7 = (elite ? 0f : 1f);
			points = new List<Vertex[]>();
			List<Vertex> list = new List<Vertex>();
			list.Clear();
			list.Add(new Vertex(new float2(0f, 0f), 1f));
			list.Add(new Vertex(Custom.DegToFloat2(Mathf.Lerp(40f, 90f, UnityEngine.Random.value)) * 0.4f * num2, 1f * num3));
			float2 val2 = Custom.DegToFloat2(Mathf.Lerp(val.x, val.y, UnityEngine.Random.value) * num);
			float2 val3 = val2 - Custom.DegToFloat2(Mathf.Lerp(40f, 90f, UnityEngine.Random.value)) * 0.4f * num2;
			if (val3.x < 0.2f)
			{
				((float2)(ref val3))._002Ector(Mathf.Lerp(val3.x, val2.x, 0.4f), val3.y);
			}
			list.Add(new Vertex(val3, 1.5f * num4));
			list.Add(new Vertex(val2, 2f * num5));
			DefineBranch(list);
			list.Clear();
			list.Add(new Vertex(points[0][1].pos, 1f));
			int num8 = ((!((double)math.distance(points[0][1].pos, points[0][2].pos) > 0.6) || !(UnityEngine.Random.value < 0.5f)) ? 1 : 2);
			float2 val4 = math.lerp(points[0][1].pos, points[0][2].pos, Mathf.Lerp(0f, (num8 == 1) ? 0.7f : 0.25f, UnityEngine.Random.value));
			list.Add(new Vertex(val4, 1.2f));
			list.Add(new Vertex(val4 + points[0][3].pos - points[0][2].pos + Custom.DegToFloat2(UnityEngine.Random.value * 360f) * 0.1f, 1.75f));
			DefineBranch(list);
			if (num8 == 2)
			{
				list.Clear();
				val4 = math.lerp(points[0][1].pos, points[0][2].pos, Mathf.Lerp(0.45f, 0.7f, UnityEngine.Random.value));
				list.Add(new Vertex(val4, 1.2f));
				list.Add(new Vertex(val4 + points[0][3].pos - points[0][2].pos + Custom.DegToFloat2(UnityEngine.Random.value * 360f) * 0.1f, 1.75f));
				DefineBranch(list);
			}
			bool flag = UnityEngine.Random.value < 0.5f && !elite;
			if (flag)
			{
				list.Clear();
				float2 val5 = Custom.DegToFloat2(90f + Mathf.Lerp(-20f, 20f, UnityEngine.Random.value)) * Mathf.Lerp(0.2f, 0.5f, UnityEngine.Random.value);
				if (val5.y > points[0][1].pos.y - 0.1f)
				{
					((float2)(ref val5))._002Ector(val5.x, val5.y - 0.2f);
				}
				float num9 = Mathf.Lerp(0.8f, 2f, UnityEngine.Random.value);
				if (UnityEngine.Random.value < 0.5f)
				{
					val5 += Custom.DegToFloat2(Mathf.Lerp(120f, 170f, UnityEngine.Random.value)) * Mathf.Lerp(0.1f, 0.3f, UnityEngine.Random.value);
					list.Add(new Vertex(new float2(0f, 0f), num9));
					list.Add(new Vertex(val5, num9));
				}
				else
				{
					list.Add(new Vertex(new float2(0f, 0f), 1f));
					list.Add(new Vertex(val5, (1f + num9) / 2f));
					list.Add(new Vertex(val5 + Custom.DegToFloat2(Mathf.Lerp(95f, 170f, UnityEngine.Random.value)) * Mathf.Lerp(0.1f, 0.2f, UnityEngine.Random.value), num9));
				}
				DefineBranch(list);
			}
			if (UnityEngine.Random.value > 0.25f || !flag || elite)
			{
				list.Clear();
				float num10 = 1f + UnityEngine.Random.value * 1.5f;
				bool flag2 = UnityEngine.Random.value < 0.5f;
				list.Add(new Vertex(new float2(0f, 0f), 1f));
				float num11 = Mathf.Lerp(95f, 135f, UnityEngine.Random.value);
				float num12 = Mathf.Lerp(0.25f, 0.4f, UnityEngine.Random.value) * num6;
				list.Add(new Vertex(Custom.DegToFloat2(num11) * num12, (flag2 ? 0.8f : Mathf.Lerp(1f, num10, 0.3f)) * num6));
				list.Add(new Vertex(Custom.DegToFloat2(num11 + Mathf.Lerp(5f, 35f, UnityEngine.Random.value)) * Mathf.Max(num12 + 0.1f, Mathf.Lerp(0.3f, 0.6f, UnityEngine.Random.value)), flag2 ? 0.8f : Mathf.Lerp(1f, num10, 0.6f)));
				list.Add(new Vertex(ExtraExtentions.normalized(list[list.Count - 1].pos) * (ExtraExtentions.magnitude(list[list.Count - 1].pos) + Mathf.Lerp(0.15f, 0.25f, UnityEngine.Random.value) * num6), num10 * num7));
				DefineBranch(list);
			}
		}

		public void DefineBranch(List<Vertex> vList)
		{
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			points.Add(vList.ToArray());
			for (int i = 0; i < vList.Count; i++)
			{
				vList[i] = new Vertex(new float2(0f - vList[i].pos.x, vList[i].pos.y), vList[i].rad);
			}
			points.Add(vList.ToArray());
		}

		public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			int num = firstSprite;
			for (int i = 0; i < points.Count; i++)
			{
				sLeaser.sprites[num] = TriangleMesh.MakeLongMesh(points[i].Length, pointyTip: false, owner.iVars.coloredEartlerTips);
				num++;
			}
		}

		public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette, Color headColor, Color decorationColor)
		{
			for (int i = 0; i < points.Count; i++)
			{
				sLeaser.sprites[firstSprite + i].color = headColor;
			}
			if (!owner.iVars.coloredEartlerTips)
			{
				return;
			}
			for (int j = 0; j < points.Count; j++)
			{
				for (int k = 0; k < 2; k++)
				{
					(sLeaser.sprites[firstSprite + j] as TriangleMesh).verticeColors[(points[j].Length - 1) * 4 + 3 - k] = decorationColor;
				}
			}
		}

		public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, float2 camPos, float2 headPos, float2 headDir, float lookUpFac)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			//IL_007c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0081: Unknown result type (might be due to invalid IL or missing references)
			//IL_0096: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
			//IL_0101: Unknown result type (might be due to invalid IL or missing references)
			//IL_0103: Unknown result type (might be due to invalid IL or missing references)
			//IL_0105: Unknown result type (might be due to invalid IL or missing references)
			//IL_0107: Unknown result type (might be due to invalid IL or missing references)
			//IL_010c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0111: Unknown result type (might be due to invalid IL or missing references)
			//IL_0113: Unknown result type (might be due to invalid IL or missing references)
			//IL_0115: Unknown result type (might be due to invalid IL or missing references)
			//IL_011a: Unknown result type (might be due to invalid IL or missing references)
			//IL_011c: Unknown result type (might be due to invalid IL or missing references)
			//IL_011e: Unknown result type (might be due to invalid IL or missing references)
			//IL_018a: Unknown result type (might be due to invalid IL or missing references)
			//IL_018c: Unknown result type (might be due to invalid IL or missing references)
			//IL_018e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0195: Unknown result type (might be due to invalid IL or missing references)
			//IL_019f: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
			//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
			//IL_01fa: Unknown result type (might be due to invalid IL or missing references)
			//IL_0204: Unknown result type (might be due to invalid IL or missing references)
			//IL_0209: Unknown result type (might be due to invalid IL or missing references)
			//IL_020e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0212: Unknown result type (might be due to invalid IL or missing references)
			//IL_0217: Unknown result type (might be due to invalid IL or missing references)
			//IL_021c: Unknown result type (might be due to invalid IL or missing references)
			//IL_021e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0251: Unknown result type (might be due to invalid IL or missing references)
			//IL_0253: Unknown result type (might be due to invalid IL or missing references)
			//IL_0257: Unknown result type (might be due to invalid IL or missing references)
			//IL_025c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0261: Unknown result type (might be due to invalid IL or missing references)
			//IL_0265: Unknown result type (might be due to invalid IL or missing references)
			//IL_026a: Unknown result type (might be due to invalid IL or missing references)
			//IL_026f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0271: Unknown result type (might be due to invalid IL or missing references)
			//IL_0293: Unknown result type (might be due to invalid IL or missing references)
			//IL_0295: Unknown result type (might be due to invalid IL or missing references)
			//IL_0299: Unknown result type (might be due to invalid IL or missing references)
			//IL_029e: Unknown result type (might be due to invalid IL or missing references)
			//IL_02a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_02a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_02ac: Unknown result type (might be due to invalid IL or missing references)
			//IL_02b1: Unknown result type (might be due to invalid IL or missing references)
			//IL_02b3: Unknown result type (might be due to invalid IL or missing references)
			//IL_02c2: Unknown result type (might be due to invalid IL or missing references)
			//IL_02c4: Unknown result type (might be due to invalid IL or missing references)
			//IL_02c6: Unknown result type (might be due to invalid IL or missing references)
			//IL_02c8: Unknown result type (might be due to invalid IL or missing references)
			float rotat = Custom.VecToDeg(float2.op_Implicit(-headDir));
			float num = 1f - Mathf.Pow(Mathf.Abs(Custom.RotateAroundOrigo(headDir, owner.BodyAxis(timeStacker)).x), 1.5f) * Mathf.Lerp(1f, 0.5f, lookUpFac);
			float ySqueeze = Mathf.Lerp(1f, -0.25f, lookUpFac);
			int num2 = firstSprite;
			for (int i = 0; i < points.Count; i++)
			{
				float2 val = RotatedPosOfVertex(i, 0, headPos, rotat, num, ySqueeze);
				float2 val2 = Custom.PerpendicularVector(points[i][1].pos, points[i][0].pos);
				float num3 = points[i][0].rad;
				for (int j = 0; j < points[i].Length; j++)
				{
					float num4 = (float)j / (float)(points[i].Length - 1);
					float2 val3 = RotatedPosOfVertex(i, j, headPos, rotat, num, ySqueeze);
					float2 val4 = ExtraExtentions.normalized(val - val3);
					float2 val5 = Custom.PerpendicularVector(val4);
					float num5 = math.distance(val3, val) / 10f;
					float num6 = Mathf.Lerp(1f, 2f, owner.iVars.eartlerWidth) * points[i][j].rad * Mathf.Lerp(0.5f, 1f, num);
					(sLeaser.sprites[num2] as TriangleMesh).MoveVertice(j * 4, float2.op_Implicit(val - Vector3Ext.Slerp2(val2, val5, 0.5f) * (num6 + num3) * 0.5f - val4 * num5 - camPos));
					(sLeaser.sprites[num2] as TriangleMesh).MoveVertice(j * 4 + 1, float2.op_Implicit(val + Vector3Ext.Slerp2(val2, val5, 0.5f) * (num6 + num3) * 0.5f - val4 * num5 - camPos));
					if (num4 == 1f)
					{
						num6 /= 4f;
					}
					(sLeaser.sprites[num2] as TriangleMesh).MoveVertice(j * 4 + 2, float2.op_Implicit(val3 - val5 * num6 + val4 * num5 - camPos));
					(sLeaser.sprites[num2] as TriangleMesh).MoveVertice(j * 4 + 3, float2.op_Implicit(val3 + val5 * num6 + val4 * num5 - camPos));
					val = val3;
					val2 = val5;
					num3 = num6;
				}
				num2++;
			}
		}

		[Obsolete("Use float2 parameter function instead.")]
		public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos, Vector2 headPos, Vector2 headDir, float lookUpFac)
		{
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			DrawSprites(sLeaser, rCam, timeStacker, new float2(camPos.x, camPos.y), new float2(headPos.x, headPos.y), new float2(headDir.x, headDir.y), lookUpFac);
		}

		public float2 RotatedPosOfVertex(int segment, int vert, float2 headPos, float rotat, float xSqueeze, float ySqueeze)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			return headPos + Custom.RotateAroundOrigo(new float2(points[segment][vert].pos.x * xSqueeze, points[segment][vert].pos.y * ySqueeze), rotat) * Mathf.Lerp(15f, 35f, owner.scavenger.abstractCreature.personality.dominance);
		}

		[Obsolete("Use float2 parameter function instead.")]
		public Vector2 RotatedPosOfVertex(int segment, int vert, Vector2 headPos, float rotat, float xSqueeze, float ySqueeze)
		{
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			float2 val = RotatedPosOfVertex(segment, vert, new float2(headPos.x, headPos.y), rotat, xSqueeze, ySqueeze);
			return new Vector2(val.x, val.y);
		}
	}

	public Scavenger scavenger;

	public ScavengerHand[] hands;

	public ScavengerLeg[] legs;

	public float2[] chestPatchShape;

	public float2[,] drawPositions;

	public int headDrawPos;

	public int chestDrawPos = 2;

	public int hipsDrawPos = 4;

	public float[] spineLengths;

	public int totBckCosSprs;

	public int totFrntCosSprs;

	public float cycle;

	public float lastCycle;

	public float flip;

	public float lastFlip;

	public float2 bodyAxis;

	public float2 lastBodyAxis;

	public float riseBody;

	public float lookUp;

	public float lastLookUp;

	public float neutralFace;

	public float lastNeutralFace;

	public float shiftingNeutralFace;

	public float shiftingNeutralFaceGoal;

	public Eartlers eartlers;

	public HSLColor bodyColor;

	public HSLColor headColor;

	public HSLColor decorationColor;

	public HSLColor bellyColor;

	public HSLColor eyeColor;

	public float bodyColorBlack;

	public float headColorBlack;

	public float bellyColorBlack;

	public Color blackColor;

	public float[,] teeth;

	public float2 lastKnuckleSoundPos;

	public float2 lastLastKnuckleSoundPos;

	public bool spearSound;

	public TailSegment[] tail;

	public float swimArms;

	public float lastSwimArms;

	public float eyesOpen;

	public float lastEyesOpen;

	public int blink;

	public float2 lookPoint;

	public float2 lastLookPoint;

	public float shake;

	public float lastShake;

	public float eyesPop;

	public float lastEyesPop;

	public float bristle;

	public float lastBristle;

	public float eyesPopGoBack;

	public VultureMaskGraphics maskGfx;

	public CentipedeShellCosmetic[] shells;

	public float markAlpha;

	public float lastMarkAlpha;

	public TemplarBlindfold blindfold;

	public TemplarCloak cloak;

	public IndividualVariations iVars;

	public float SAVFLIP;

	public bool objectsInRightContainers;

	public float darkness;

	public int FirstBehindLimbSprite { get; }

	public int FirstBckCosmeticSprite { get; }

	public int ChestSprite { get; }

	public int HipSprite { get; }

	public int WaistSprite { get; }

	public int ChestPatchSprite { get; }

	public int FirstInFrontLimbSprite { get; }

	public int NeckSprite { get; }

	public int FirstFrntCosmeticSprite { get; }

	public int FirstEartlerSprite { get; }

	public int HeadSprite { get; }

	public int TeethSprite { get; }

	public int FirstEyeSprite { get; }

	public int MaskSprite { get; }

	public int ShellSprite { get; }

	public int MarkSprite { get; }

	public int TotalSprites { get; }

	[Obsolete("IMuddableGraphics implemented - this is no longer used")]
	public Color MudOverlayColor => Color.Lerp(blackColor, scavenger.mudColor, 0.8f * (1f - darkness * 0.5f));

	public Color BlendedBodyColor => Color.Lerp(bodyColor.rgb, blackColor, Mathf.Lerp(bodyColorBlack, 1f, darkness));

	public Color BlendedHeadColor => Color.Lerp(headColor.rgb, blackColor, Mathf.Lerp(headColorBlack, 1f, darkness));

	public Color BlendedDecorationColor => Color.Lerp(decorationColor.rgb, blackColor, darkness * 0.9f);

	public Color BlendedEyeColor
	{
		get
		{
			if (darkness < 0.5f)
			{
				return eyeColor.rgb;
			}
			return Custom.Saturate(eyeColor.rgb, Mathf.InverseLerp(0.5f, 1f, darkness));
		}
	}

	public int EyeSprite(int eye, int part)
	{
		return FirstEyeSprite + eye + part * 2;
	}

	public float Cycle(float timeStacker)
	{
		return Mathf.Sin(Mathf.Lerp(lastCycle, cycle, timeStacker) * (float)Math.PI * 2f);
	}

	public float2 HeadDir(float t)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		return ExtraExtentions.normalized(float2Ext.ClampMagnitude(math.lerp(lastLookPoint, lookPoint, t) - math.lerp(float2.op_Implicit(scavenger.mainBodyChunk.lastPos), float2.op_Implicit(scavenger.mainBodyChunk.pos), t), 300f) / 300f * 0.2f + float2Ext.ClampMagnitude(math.lerp(drawPositions[headDrawPos, 1], drawPositions[headDrawPos, 0], t) - math.lerp(drawPositions[chestDrawPos, 1], drawPositions[chestDrawPos, 0], t), 100f) / 100f);
	}

	public float BodyAxis(float timeStacker)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		return 0f - Custom.VecToDeg(float2.op_Implicit(ExtraExtentions.normalized(Vector3Ext.Slerp2(lastBodyAxis, bodyAxis, timeStacker))));
	}

	public ScavengerGraphics(PhysicalObject ow)
		: base(ow, internalContainers: true)
	{
		//IL_05d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_05d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_05f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0611: Unknown result type (might be due to invalid IL or missing references)
		//IL_0616: Unknown result type (might be due to invalid IL or missing references)
		//IL_0632: Unknown result type (might be due to invalid IL or missing references)
		//IL_0637: Unknown result type (might be due to invalid IL or missing references)
		//IL_0654: Unknown result type (might be due to invalid IL or missing references)
		//IL_0659: Unknown result type (might be due to invalid IL or missing references)
		//IL_0675: Unknown result type (might be due to invalid IL or missing references)
		//IL_067a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0691: Unknown result type (might be due to invalid IL or missing references)
		//IL_0696: Unknown result type (might be due to invalid IL or missing references)
		//IL_06ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_06b1: Unknown result type (might be due to invalid IL or missing references)
		scavenger = ow as Scavenger;
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.InitState(scavenger.abstractCreature.ID.RandomSeed);
		iVars = new IndividualVariations(scavenger);
		GenerateColors();
		int num = 0;
		if (scavenger.Templar)
		{
			cloak = new TemplarCloak(this, num);
			AddSubModule(cloak);
			num += cloak.totalSprites;
		}
		FirstBehindLimbSprite = num;
		num += 2;
		num = (FirstBckCosmeticSprite = num + 3);
		tail = new TailSegment[iVars.tailSegs];
		if (iVars.tailSegs > 0)
		{
			for (int i = 0; i < tail.Length; i++)
			{
				tail[i] = new TailSegment(this, Mathf.Lerp(2f, 1f, (float)i / (float)(iVars.tailSegs - 1)), 10f, (i > 0) ? tail[i - 1] : null, 0.5f, 0.9f, 0.5f, pullInPreviousPosition: true);
			}
			AddSubModule(new Tail(this, num));
			totBckCosSprs += subModules[subModules.Count - 1].totalSprites;
			num += subModules[subModules.Count - 1].totalSprites;
		}
		if (UnityEngine.Random.value < 0.1f || scavenger.Elite)
		{
			AddSubModule(new HardBackSpikes(this, num));
			totBckCosSprs += subModules[subModules.Count - 1].totalSprites;
			num += subModules[subModules.Count - 1].totalSprites;
		}
		else
		{
			AddSubModule(new WobblyBackTufts(this, num));
			totBckCosSprs += subModules[subModules.Count - 1].totalSprites;
			num += subModules[subModules.Count - 1].totalSprites;
		}
		ChestSprite = num++;
		HipSprite = num++;
		WaistSprite = num++;
		ChestPatchSprite = num++;
		hands = new ScavengerHand[2];
		legs = new ScavengerLeg[2];
		legs[1] = new ScavengerLeg(this, 1, FirstBehindLimbSprite);
		hands[1] = new ScavengerHand(this, 1, FirstBehindLimbSprite + 2);
		FirstInFrontLimbSprite = num;
		legs[0] = new ScavengerLeg(this, 0, num);
		num += 2;
		hands[0] = new ScavengerHand(this, 0, num);
		num += 3;
		bodyParts = new BodyPart[4 + tail.Length];
		for (int j = 0; j < 2; j++)
		{
			bodyParts[j] = hands[j];
			bodyParts[j + 2] = legs[j];
		}
		for (int k = 0; k < tail.Length; k++)
		{
			bodyParts[4 + k] = tail[k];
		}
		drawPositions = new float2[5, 2];
		spineLengths = new float[2];
		NeckSprite = num++;
		FirstFrntCosmeticSprite = num;
		num += totFrntCosSprs;
		eartlers = new Eartlers(num, this);
		FirstEartlerSprite = num;
		num += eartlers.TotalSprites;
		HeadSprite = num++;
		TeethSprite = num++;
		teeth = new float[UnityEngine.Random.Range(2, 5) * 2, 2];
		float num2 = Mathf.Lerp(0.5f, 1.5f, Mathf.Pow(UnityEngine.Random.value, 1.5f - scavenger.abstractCreature.personality.aggression));
		num2 = Mathf.Lerp(num2, num2 * Custom.LerpMap(teeth.GetLength(0), 4f, 8f, 1f, 0.5f), 0.3f);
		float a = Mathf.Lerp(num2 + 0.2f, Mathf.Lerp(0.7f, 1.2f, UnityEngine.Random.value), UnityEngine.Random.value);
		a = Mathf.Lerp(a, Custom.LerpMap(teeth.GetLength(0), 4f, 8f, 1.5f, 0.2f), 0.4f);
		float a2 = 0.3f + 0.7f * UnityEngine.Random.value;
		for (int l = 0; l < teeth.GetLength(0); l++)
		{
			float num3 = (float)l / (float)(teeth.GetLength(0) - 1);
			teeth[l, 0] = Mathf.Lerp(a2, 1f, Mathf.Sin(num3 * (float)Math.PI)) * num2;
			if (UnityEngine.Random.value < iVars.scruffy && UnityEngine.Random.value < 0.2f)
			{
				teeth[l, 0] = 0f;
			}
			teeth[l, 1] = Mathf.Lerp(0.5f, 1f, Mathf.Sin(num3 * (float)Math.PI)) * a;
		}
		FirstEyeSprite = num;
		num += ((iVars.pupilSize <= 0f) ? 2 : 4);
		chestPatchShape = (float2[])(object)new float2[8];
		float num4 = 0.4f;
		float num5 = 0.95f;
		float num6 = 1f;
		chestPatchShape[0] = new float2((0f - num6) / 4f, num4);
		chestPatchShape[1] = new float2(num6 / 4f, num4);
		chestPatchShape[2] = new float2(0f - num6, Mathf.Lerp(num4, num5, 1f / 3f));
		chestPatchShape[3] = new float2(num6, Mathf.Lerp(num4, num5, 1f / 3f));
		chestPatchShape[4] = new float2(0f - num6, Mathf.Lerp(num4, num5, 2f / 3f));
		chestPatchShape[5] = new float2(num6, Mathf.Lerp(num4, num5, 2f / 3f));
		chestPatchShape[6] = new float2((0f - num6) / 3f, num5);
		chestPatchShape[7] = new float2(num6 / 3f, num5);
		if (scavenger.King)
		{
			maskGfx = new VultureMaskGraphics(scavenger, VultureMask.MaskType.SCAVKING, num, "KingMask");
			maskGfx.GenerateColor(scavenger.abstractCreature.ID.RandomSeed);
		}
		else if (scavenger.Elite)
		{
			string overrideSprite = "KrakenMask";
			switch (UnityEngine.Random.Range(0, 4))
			{
			case 1:
				overrideSprite = "SpikeMask";
				break;
			case 2:
				overrideSprite = "HornedMask";
				break;
			case 3:
				overrideSprite = "SadMask";
				break;
			}
			maskGfx = new VultureMaskGraphics(scavenger, VultureMask.MaskType.NORMAL, num, overrideSprite);
			maskGfx.GenerateColor(scavenger.abstractCreature.ID.RandomSeed);
		}
		if (scavenger.Templar)
		{
			maskGfx = new VultureMaskGraphics(scavenger, VultureMask.MaskType.SCAVTEMPLAR, num, null);
			maskGfx.GenerateColor(scavenger.abstractCreature.ID.RandomSeed);
			maskGfx.ignoreDarkness = true;
			maskGfx.glimmer = true;
		}
		MaskSprite = num;
		if (maskGfx != null)
		{
			num += maskGfx.TotalSprites;
		}
		if (scavenger.King)
		{
			shells = new CentipedeShellCosmetic[3];
		}
		else
		{
			shells = new CentipedeShellCosmetic[0];
		}
		ShellSprite = num;
		for (int m = 0; m < shells.Length; m++)
		{
			shells[m] = new CentipedeShellCosmetic(num, scavenger.firstChunk.pos, 0f, 1f, 1f, 1f);
			num += shells[m].TotalSprites;
		}
		MarkSprite = num;
		if (scavenger.King)
		{
			num += 2;
		}
		if (scavenger.Disciple)
		{
			blindfold = new TemplarBlindfold(this, num);
			num += blindfold.totalSprites;
		}
		TotalSprites = num;
		UnityEngine.Random.state = state;
	}

	public override void Reset()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		base.Reset();
		drawPositions[0, 0] = float2.op_Implicit(scavenger.bodyChunks[2].pos);
		drawPositions[2, 0] = float2.op_Implicit(scavenger.bodyChunks[0].pos);
		drawPositions[4, 0] = float2.op_Implicit(scavenger.bodyChunks[1].pos);
		drawPositions[1, 0] = math.lerp(drawPositions[0, 0], drawPositions[2, 0], 0.5f);
		drawPositions[3, 0] = math.lerp(drawPositions[2, 0], drawPositions[4, 0], 0.5f);
		for (int i = 0; i < drawPositions.GetLength(0); i++)
		{
			drawPositions[i, 1] = drawPositions[i, 0];
		}
	}

	public override void Update()
	{
		//IL_03de: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0404: Unknown result type (might be due to invalid IL or missing references)
		//IL_0414: Unknown result type (might be due to invalid IL or missing references)
		//IL_0419: Unknown result type (might be due to invalid IL or missing references)
		//IL_0552: Unknown result type (might be due to invalid IL or missing references)
		//IL_056d: Unknown result type (might be due to invalid IL or missing references)
		//IL_057c: Unknown result type (might be due to invalid IL or missing references)
		//IL_059f: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_05af: Unknown result type (might be due to invalid IL or missing references)
		//IL_05b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_06c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_06cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_06d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_06e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_06f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_06fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_070d: Unknown result type (might be due to invalid IL or missing references)
		//IL_071f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0724: Unknown result type (might be due to invalid IL or missing references)
		//IL_072e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0739: Unknown result type (might be due to invalid IL or missing references)
		//IL_073e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0743: Unknown result type (might be due to invalid IL or missing references)
		//IL_074d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0752: Unknown result type (might be due to invalid IL or missing references)
		//IL_07bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_07cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0787: Unknown result type (might be due to invalid IL or missing references)
		//IL_0796: Unknown result type (might be due to invalid IL or missing references)
		//IL_07a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_07a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0845: Unknown result type (might be due to invalid IL or missing references)
		//IL_084a: Unknown result type (might be due to invalid IL or missing references)
		//IL_085b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0860: Unknown result type (might be due to invalid IL or missing references)
		//IL_09a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_09ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_0cf4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0cf9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d2e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d33: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d52: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d57: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d76: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d7b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d91: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d9e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0da3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0da9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0db6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0dde: Unknown result type (might be due to invalid IL or missing references)
		//IL_0de3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0dff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e19: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e28: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e2d: Unknown result type (might be due to invalid IL or missing references)
		//IL_108a: Unknown result type (might be due to invalid IL or missing references)
		//IL_1097: Unknown result type (might be due to invalid IL or missing references)
		//IL_109c: Unknown result type (might be due to invalid IL or missing references)
		//IL_10a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_10ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_10be: Unknown result type (might be due to invalid IL or missing references)
		//IL_10cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_10d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_10dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_10fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_110c: Unknown result type (might be due to invalid IL or missing references)
		//IL_1111: Unknown result type (might be due to invalid IL or missing references)
		//IL_1116: Unknown result type (might be due to invalid IL or missing references)
		//IL_112b: Unknown result type (might be due to invalid IL or missing references)
		//IL_1138: Unknown result type (might be due to invalid IL or missing references)
		//IL_1142: Unknown result type (might be due to invalid IL or missing references)
		//IL_1147: Unknown result type (might be due to invalid IL or missing references)
		//IL_1166: Unknown result type (might be due to invalid IL or missing references)
		//IL_1173: Unknown result type (might be due to invalid IL or missing references)
		//IL_1185: Unknown result type (might be due to invalid IL or missing references)
		//IL_118a: Unknown result type (might be due to invalid IL or missing references)
		//IL_1194: Unknown result type (might be due to invalid IL or missing references)
		//IL_1199: Unknown result type (might be due to invalid IL or missing references)
		//IL_119e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e69: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e76: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e8a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e8f: Unknown result type (might be due to invalid IL or missing references)
		//IL_11b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_11c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_11d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_11db: Unknown result type (might be due to invalid IL or missing references)
		//IL_11e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_11e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_1029: Unknown result type (might be due to invalid IL or missing references)
		//IL_102e: Unknown result type (might be due to invalid IL or missing references)
		//IL_1057: Unknown result type (might be due to invalid IL or missing references)
		//IL_1061: Unknown result type (might be due to invalid IL or missing references)
		//IL_106b: Unknown result type (might be due to invalid IL or missing references)
		//IL_1070: Unknown result type (might be due to invalid IL or missing references)
		//IL_1075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ea7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ecf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ee4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ef7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f1a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f1f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f24: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f47: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f54: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f61: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f6e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f73: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f7a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f7f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f89: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f8e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0fa1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0fa6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0fb0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0fb5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0fba: Unknown result type (might be due to invalid IL or missing references)
		//IL_0fbf: Unknown result type (might be due to invalid IL or missing references)
		//IL_12ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_12ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_121c: Unknown result type (might be due to invalid IL or missing references)
		//IL_1221: Unknown result type (might be due to invalid IL or missing references)
		//IL_122b: Unknown result type (might be due to invalid IL or missing references)
		//IL_1236: Unknown result type (might be due to invalid IL or missing references)
		//IL_1240: Unknown result type (might be due to invalid IL or missing references)
		//IL_1255: Unknown result type (might be due to invalid IL or missing references)
		//IL_126a: Unknown result type (might be due to invalid IL or missing references)
		//IL_128d: Unknown result type (might be due to invalid IL or missing references)
		//IL_1292: Unknown result type (might be due to invalid IL or missing references)
		//IL_1297: Unknown result type (might be due to invalid IL or missing references)
		//IL_134d: Unknown result type (might be due to invalid IL or missing references)
		//IL_139c: Unknown result type (might be due to invalid IL or missing references)
		//IL_13a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_13a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_13d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_13d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_13da: Unknown result type (might be due to invalid IL or missing references)
		//IL_13ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_13f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_13f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_13f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_1403: Unknown result type (might be due to invalid IL or missing references)
		//IL_1422: Unknown result type (might be due to invalid IL or missing references)
		if (!objectsInRightContainers)
		{
			scavenger.PlaceAllGrabbedObjectsInCorrectContainers();
			objectsInRightContainers = true;
		}
		base.Update();
		lastMarkAlpha = markAlpha;
		if (scavenger.Stunned)
		{
			markAlpha = Mathf.Lerp(markAlpha, UnityEngine.Random.Range(0f, 0.5f), 0.25f);
		}
		else if (!scavenger.dead)
		{
			markAlpha = Mathf.Lerp(markAlpha, 1f, 0.2f);
		}
		else
		{
			markAlpha = Mathf.Lerp(markAlpha, 0f, 0.1f);
		}
		if (DEBUGLABELS != null)
		{
			DEBUGLABELS[0].label.text = scavenger.movMode.ToString() + " " + scavenger.AI.behavior.ToString() + " " + ((scavenger.animation != null) ? scavenger.animation.id.ToString() : "none");
			DEBUGLABELS[1].label.text = scavenger.commitedToMove.type.ToString() + " " + scavenger.commitToMoveCounter + " " + scavenger.drop + " idl: " + scavenger.AI.idleCounter;
			DEBUGLABELS[3].label.text = "AL: " + (float.IsNaN(scavenger.AI.filteredLikeB) ? "~" : ((int)(Mathf.Lerp(-1f, 1f, scavenger.AI.filteredLikeB) * 100f)).ToString()) + "  TL: " + (int)(scavenger.AI.tempLikeB * 100f) + "  L: " + (int)(scavenger.AI.likeB * 100f);
			DEBUGLABELS[2].label.text = "agitation: " + (int)(scavenger.AI.agitation * 100f) + " Scared: " + (int)(scavenger.AI.scared * 100f);
		}
		if (scavenger.knucklePos.HasValue && scavenger.movMode == Scavenger.MovementMode.Run)
		{
			float num = Custom.LerpMap(Mathf.Abs(scavenger.mainBodyChunk.pos.x - scavenger.knucklePos.Value.x), 20f, 0f, 0f, 10f);
			if (riseBody < num)
			{
				riseBody = Mathf.Min(num, riseBody + 2f);
			}
			else
			{
				riseBody = Mathf.Lerp(riseBody, num, 0.2f);
			}
		}
		else
		{
			riseBody = Mathf.Max(0f, riseBody - 0.5f);
		}
		if (tail.Length != 0)
		{
			tail[0].connectedPoint = float2.op_Implicit(drawPositions[hipsDrawPos, 0]);
			float2 val = drawPositions[hipsDrawPos, 0];
			float2 val2 = drawPositions[hipsDrawPos - 1, 0];
			float num2 = 28f;
			for (int i = 0; i < tail.Length; i++)
			{
				tail[i].Update();
				tail[i].vel *= Mathf.Lerp(1f, 0.5f, base.owner.bodyChunks[1].submersion);
				tail[i].vel.y -= 0.9f * (1f - base.owner.bodyChunks[1].submersion) * base.owner.room.gravity;
				if (!Custom.DistLess(tail[i].pos, base.owner.bodyChunks[1].pos, 9f * (float)(i + 1)))
				{
					tail[i].pos = base.owner.bodyChunks[1].pos + Custom.DirVec(base.owner.bodyChunks[1].pos, tail[i].pos) * (9f * (float)(i + 1));
				}
				tail[i].vel += Custom.DirVec(val2, tail[i].pos) * num2 / math.distance(val2, float2.op_Implicit(tail[i].pos));
				num2 *= 0.25f;
				val2 = val;
				val = float2.op_Implicit(tail[i].pos);
			}
		}
		for (int j = 0; j < 2; j++)
		{
			hands[j].Update();
		}
		for (int k = 0; k < 2; k++)
		{
			legs[k].Update();
		}
		lastFlip = flip;
		flip = scavenger.flip;
		lastCycle = cycle;
		if (scavenger.moving)
		{
			cycle += Mathf.Sign(flip) * 0.1f;
		}
		lastSwimArms = swimArms;
		if (scavenger.movMode == Scavenger.MovementMode.Swim || scavenger.Pointing)
		{
			swimArms = Mathf.Min(swimArms + 0.1f, 1f);
		}
		else
		{
			swimArms = Mathf.Max(swimArms - 0.1f, 0f);
		}
		lastBodyAxis = bodyAxis;
		bodyAxis = Vector3Ext.Slerp2(bodyAxis, ExtraExtentions.normalized(drawPositions[chestDrawPos, 0] - drawPositions[hipsDrawPos, 0] + Custom.PerpendicularVector(drawPositions[chestDrawPos, 0], drawPositions[hipsDrawPos, 0]) * 10f * flip), 0.1f);
		if (scavenger.movMode != Scavenger.MovementMode.Crawl && scavenger.movMode != Scavenger.MovementMode.Climb)
		{
			bodyAxis = Vector3Ext.Slerp2(bodyAxis, new float2(0f, 1f), 0.5f);
		}
		lastLookUp = lookUp;
		if (Custom.RotateAroundOrigo(HeadDir(1f), BodyAxis(1f)).y > 0.8f)
		{
			lookUp = Mathf.Min(1f, lookUp + 1f / Mathf.Lerp(5f, 20f, shiftingNeutralFace));
		}
		else
		{
			lookUp = Mathf.Max(0f, lookUp - 1f / Mathf.Lerp(20f, 5f, shiftingNeutralFace));
		}
		lastLookPoint = lookPoint;
		lookPoint = float2.op_Implicit(scavenger.EyesLookPoint);
		lastEyesOpen = eyesOpen;
		if (scavenger.Consious)
		{
			blink--;
			if (blink < 0)
			{
				eyesOpen = Mathf.Max(0f, eyesOpen - 0.5f);
				if (blink < -UnityEngine.Random.Range(2, 6))
				{
					blink = UnityEngine.Random.Range(4, (UnityEngine.Random.value < 0.5f) ? 12 : ((int)Mathf.Lerp(800f, 200f, scavenger.abstractCreature.personality.nervous)));
				}
			}
			else
			{
				eyesOpen = Mathf.Min(1f, eyesOpen + 0.05f + 0.95f * scavenger.abstractCreature.personality.energy);
				if (scavenger.Blinded && UnityEngine.Random.value < 0.1f)
				{
					blink = 0;
				}
			}
			lastNeutralFace = neutralFace;
			float num3 = Mathf.Lerp(200f, 600f, Mathf.Pow(shiftingNeutralFace, 1.7f));
			float num4 = Mathf.InverseLerp(num3, 0f, math.distance(float2.op_Implicit(scavenger.mainBodyChunk.pos), lookPoint));
			neutralFace = Mathf.Lerp(shiftingNeutralFace, num4, Custom.LerpMap(Mathf.Abs(num4 - num3 * 0.5f), 0f, num3 * 0.5f, 0.05f, 0.55f));
			if (shiftingNeutralFace < shiftingNeutralFaceGoal)
			{
				shiftingNeutralFace = Mathf.Min(shiftingNeutralFace + 0.025f, shiftingNeutralFaceGoal);
			}
			else
			{
				shiftingNeutralFace = Mathf.Max(shiftingNeutralFace - 0.025f, shiftingNeutralFaceGoal);
			}
			shiftingNeutralFace = Mathf.Lerp(shiftingNeutralFace, shiftingNeutralFaceGoal, 0.15f);
			if (UnityEngine.Random.value < 0.0125f)
			{
				shiftingNeutralFaceGoal = UnityEngine.Random.value;
			}
			if (shiftingNeutralFaceGoal > 0.2f && (scavenger.Rummaging || (scavenger.animation != null && scavenger.animation is Scavenger.AttentiveAnimation)))
			{
				shiftingNeutralFaceGoal *= 0.25f;
			}
		}
		else
		{
			eyesOpen = Mathf.Lerp(eyesOpen, 0.5f, 0.1f);
		}
		lastEyesPop = eyesPop;
		if (eyesPop > 0f)
		{
			eyesPop = Mathf.Max(0f, eyesPop - 1f / ((eyesPop > 0.5f || scavenger.animation == null || scavenger.animation.id != Scavenger.ScavengerAnimation.ID.Look) ? eyesPopGoBack : (eyesPopGoBack * 2f)));
			eyesOpen = 1f;
		}
		if (scavenger.AI.scared > 0.8f && scavenger.abstractCreature.personality.bravery < 0.3f)
		{
			eyesPop = Mathf.Lerp(eyesPop, UnityEngine.Random.value * Mathf.InverseLerp(0.8f, 1f, scavenger.AI.scared), 0.1f);
		}
		lastShake = shake;
		if (shake > 0f)
		{
			shake = Mathf.Max(Mathf.InverseLerp(0.6f, 1f, scavenger.AI.scared * (1f - scavenger.abstractCreature.personality.bravery)) * 0.3f, shake - 0.1f);
		}
		lastBristle = bristle;
		if (bristle > 0f)
		{
			bristle = Mathf.Max(0f, bristle - (1f - scavenger.AI.agitation) / 30f);
		}
		if (scavenger.dead)
		{
			eyesPop = 0f;
			bristle = 0f;
			shake = 0f;
		}
		if (scavenger.Blinded)
		{
			eyesPop = 0f;
		}
		for (int l = 0; l < drawPositions.GetLength(0); l++)
		{
			drawPositions[l, 1] = drawPositions[l, 0];
		}
		drawPositions[0, 0] = float2.op_Implicit(scavenger.bodyChunks[2].pos);
		drawPositions[2, 0] = float2.op_Implicit(scavenger.bodyChunks[0].pos);
		drawPositions[4, 0] = float2.op_Implicit(scavenger.bodyChunks[1].pos);
		if (scavenger.Consious)
		{
			float2 val3 = Custom.DirVec(lookPoint, drawPositions[2, 0]) * Custom.LerpMap(math.distance(lookPoint, drawPositions[2, 0]), 50f, 400f, 11f, 0f, 0.5f);
			drawPositions[2, 0] = new float2(drawPositions[2, 0].x + val3.x, drawPositions[2, 0].y + val3.y + riseBody);
		}
		if (scavenger.movMode == Scavenger.MovementMode.Climb && !scavenger.swingPos.HasValue)
		{
			float num5 = math.distance(drawPositions[4, 0], drawPositions[2, 0]);
			float2 val4 = drawPositions[2, 0];
			for (int m = 0; m < 2; m++)
			{
				ref float2 reference = ref drawPositions[2, 0];
				reference += ExtraExtentions.ToF2(Custom.DirVec(scavenger.mainBodyChunk.pos, hands[m].pos)) * Custom.LerpMap(math.distance(float2.op_Implicit(scavenger.mainBodyChunk.pos), float2.op_Implicit(hands[m].pos)), 5f, 40f, 5f, -7.5f);
			}
			drawPositions[2, 0] = math.lerp(drawPositions[2, 0], drawPositions[4, 0] + Custom.DirVec(drawPositions[4, 0], drawPositions[2, 0]) * num5, 0.5f);
			ref float2 reference2 = ref drawPositions[1, 0];
			reference2 += val4 - drawPositions[2, 0];
		}
		if (scavenger.Consious && scavenger.abstractCreature.personality.nervous > 0.8f && UnityEngine.Random.value < 0.5f + 0.5f * scavenger.abstractCreature.personality.nervous)
		{
			ref float2 reference3 = ref drawPositions[2, 0];
			reference3 += Custom.RNVf2() * Mathf.InverseLerp(0.8f, 1f, scavenger.abstractCreature.personality.nervous) * 1.5f * UnityEngine.Random.value;
		}
		drawPositions[3, 0] = (drawPositions[2, 0] + drawPositions[4, 0]) / 2f;
		ref float2 reference4 = ref drawPositions[3, 0];
		reference4 -= Custom.PerpendicularVector(drawPositions[4, 0], drawPositions[2, 0]) * Mathf.Lerp(5f, 15f, iVars.narrowWaist) * scavenger.flip;
		drawPositions[3, 0] = math.lerp(drawPositions[3, 0], drawPositions[4, 0], 0.25f);
		drawPositions[1, 0] = ExtraExtentions.ToF2(scavenger.bodyChunks[0].pos) + Custom.DirVec(drawPositions[3, 0], drawPositions[chestDrawPos, 0]) * 8f;
		for (int n = 0; n < 2; n++)
		{
			ref float2 reference5 = ref drawPositions[4, 0];
			reference5 += Custom.DirVec(drawPositions[4, 0], ExtraExtentions.ToF2(legs[n].pos));
		}
		if (shake > 0f)
		{
			for (int num6 = 0; num6 < drawPositions.GetLength(0); num6++)
			{
				ref float2 reference6 = ref drawPositions[num6, 0];
				reference6 += Custom.RNVf2() * UnityEngine.Random.value * shake * 7f * Custom.LerpMap(math.distance(float2.op_Implicit(scavenger.mainBodyChunk.lastPos), float2.op_Implicit(scavenger.mainBodyChunk.pos)), 2f, 8f, 1f, 4f);
			}
		}
		spineLengths[1] = spineLengths[0];
		spineLengths[0] = 0f;
		for (int num7 = 0; num7 < drawPositions.GetLength(0) - 1; num7++)
		{
			spineLengths[0] = Mathf.Clamp(spineLengths[0] + math.distance(drawPositions[num7, 0], drawPositions[num7 + 1, 0]), 0f, 200f);
		}
		if (ModManager.DLCShared)
		{
			if (maskGfx != null)
			{
				float2 val5 = HeadDir(0f);
				float num8 = BodyAxis(0f);
				float num9 = Mathf.Lerp(lastLookUp, lookUp, 0f);
				float num10 = Mathf.Lerp(lastNeutralFace, neutralFace, 0f);
				num9 *= 1f - num10;
				float2 val6 = ExtraExtentions.normalized(math.lerp(ExtraExtentions.normalized(val5), -Custom.DegToFloat2(0f - num8), Mathf.Lerp(0.5f, 1f, Mathf.Max(Mathf.Pow(num9, 1.1f), num10))));
				maskGfx.rotationA = float2.op_Implicit(Vector3Ext.Slerp2(float2.op_Implicit(maskGfx.rotationA), -ExtraExtentions.normalized(val6), 0.5f));
				maskGfx.rotationB = float2.op_Implicit(new float2(0f, 1f));
				maskGfx.Update();
			}
			for (int num11 = 0; num11 < shells.Length; num11++)
			{
				shells[num11].Update();
			}
		}
		blindfold?.Update();
	}

	public int ContainerForHeldItem(PhysicalObject item, int grasp)
	{
		if (grasp == 0)
		{
			return 2;
		}
		if (!(item is Spear))
		{
			return 1;
		}
		return 0;
	}

	public void ShockReaction(float intensity)
	{
		if (!scavenger.dead && scavenger.stun <= 20)
		{
			eyesPop = Mathf.Max(eyesPop, Mathf.Pow(intensity, 0.5f));
			eyesPopGoBack = Mathf.Max(eyesPopGoBack, Mathf.Lerp(2f, 20f, Mathf.Pow(intensity, Mathf.Lerp(0.8f, 2f, scavenger.abstractCreature.personality.bravery))));
			shake = Mathf.Max(shake, Mathf.Max(0f, intensity - Custom.LerpMap(scavenger.abstractCreature.personality.bravery, 0.75f, 1f, 0f, 0.5f)));
			blink = UnityEngine.Random.Range(10, 40);
			bristle = Mathf.Max(bristle, scavenger.AI.agitation, intensity);
		}
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[TotalSprites];
		sLeaser.sprites[ChestSprite] = new FSprite("Circle20");
		sLeaser.sprites[ChestSprite].scaleX = base.owner.bodyChunks[0].rad * Mathf.Lerp(0.7f, 1.3f, iVars.fatness) / 10f;
		sLeaser.sprites[ChestSprite].scaleY = (base.owner.bodyChunks[0].rad + Mathf.Lerp(2f, 1.5f, iVars.narrowWaist)) / 10f;
		sLeaser.sprites[ChestSprite].anchorY = 0.4f + 0.05f * iVars.narrowWaist;
		sLeaser.sprites[HipSprite] = new FSprite("Circle20");
		sLeaser.sprites[HipSprite].scale = base.owner.bodyChunks[1].rad / 15f;
		sLeaser.sprites[HeadSprite] = new FSprite("Circle20");
		for (int i = 0; i < 2; i++)
		{
			for (int j = 0; j < ((!(iVars.pupilSize > 0f)) ? 1 : 2); j++)
			{
				sLeaser.sprites[EyeSprite(i, j)] = new FSprite("Circle20");
			}
		}
		TriangleMesh.Triangle[] array = new TriangleMesh.Triangle[6];
		for (int k = 0; k < 6; k++)
		{
			array[k] = new TriangleMesh.Triangle(k, k + 1, k + 2);
		}
		sLeaser.sprites[WaistSprite] = new TriangleMesh("Futile_White", array, customColor: false);
		sLeaser.sprites[NeckSprite] = TriangleMesh.MakeLongMesh(4, pointyTip: false, customColor: true);
		for (int l = 0; l < 2; l++)
		{
			hands[l].InitiateSprites(sLeaser, rCam);
		}
		for (int m = 0; m < 2; m++)
		{
			legs[m].InitiateSprites(sLeaser, rCam);
		}
		eartlers.InitiateSprites(sLeaser, rCam);
		array = new TriangleMesh.Triangle[teeth.GetLength(0) * 3];
		for (int n = 0; n < teeth.GetLength(0); n++)
		{
			array[n * 3] = new TriangleMesh.Triangle(n * 5, n * 5 + 1, n * 5 + 2);
			array[n * 3 + 1] = new TriangleMesh.Triangle(n * 5 + 1, n * 5 + 2, n * 5 + 3);
			array[n * 3 + 2] = new TriangleMesh.Triangle(n * 5 + 2, n * 5 + 3, n * 5 + 4);
		}
		sLeaser.sprites[TeethSprite] = new TriangleMesh("Futile_White", array, customColor: false);
		array = new TriangleMesh.Triangle[6];
		for (int num = 0; num < 6; num++)
		{
			array[num] = new TriangleMesh.Triangle(num, num + 1, num + 2);
		}
		sLeaser.sprites[ChestPatchSprite] = new TriangleMesh("Futile_White", array, customColor: false);
		if (ModManager.DLCShared)
		{
			if (maskGfx != null)
			{
				maskGfx.InitiateSprites(sLeaser, rCam);
			}
			for (int num2 = 0; num2 < shells.Length; num2++)
			{
				shells[num2].InitiateSprites(sLeaser, rCam);
			}
			if (scavenger.King)
			{
				sLeaser.sprites[MarkSprite] = new FSprite("pixel");
				sLeaser.sprites[MarkSprite].scale = 5f;
				sLeaser.sprites[MarkSprite + 1] = new FSprite("Futile_White");
				sLeaser.sprites[MarkSprite + 1].shader = rCam.game.rainWorld.Shaders["FlatLight"];
			}
		}
		blindfold?.InitiateSprites(sLeaser, rCam);
		sLeaser.containers = new FContainer[3];
		for (int num3 = 0; num3 < sLeaser.containers.Length; num3++)
		{
			sLeaser.containers[num3] = new FContainer();
		}
		base.InitiateSprites(sLeaser, rCam);
		AddToContainer(sLeaser, rCam, null);
		scavenger.PlaceAllGrabbedObjectsInCorrectContainers();
	}

	public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		sLeaser.RemoveAllSpritesFromContainer();
		if (newContatiner == null)
		{
			newContatiner = rCam.ReturnFContainer("Midground");
		}
		newContatiner.AddChild(sLeaser.containers[0]);
		for (int i = 0; i < FirstInFrontLimbSprite; i++)
		{
			newContatiner.AddChild(sLeaser.sprites[i]);
		}
		if (ModManager.DLCShared)
		{
			if (scavenger.King)
			{
				FContainer fContainer = rCam.ReturnFContainer("Foreground");
				for (int j = ShellSprite; j < MarkSprite; j++)
				{
					newContatiner.AddChild(sLeaser.sprites[j]);
				}
				for (int k = MarkSprite; k < MarkSprite + 2; k++)
				{
					fContainer.AddChild(sLeaser.sprites[k]);
				}
			}
			else
			{
				for (int l = ShellSprite; l < TotalSprites; l++)
				{
					newContatiner.AddChild(sLeaser.sprites[l]);
				}
			}
		}
		for (int m = FirstInFrontLimbSprite; m < FirstInFrontLimbSprite + 2; m++)
		{
			newContatiner.AddChild(sLeaser.sprites[m]);
		}
		newContatiner.AddChild(sLeaser.containers[1]);
		for (int n = FirstInFrontLimbSprite + 2; n < TotalSprites; n++)
		{
			if (n < ShellSprite || n >= MarkSprite + 2)
			{
				newContatiner.AddChild(sLeaser.sprites[n]);
			}
		}
		newContatiner.AddChild(sLeaser.containers[2]);
		if (ModManager.DLCShared && maskGfx != null)
		{
			maskGfx.AddToContainer(sLeaser, rCam, null);
		}
		blindfold?.AddToContainer(sLeaser, rCam, null);
		cloak?.AddToContainer(sLeaser, rCam, null);
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPosV2)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0205: Unknown result type (might be due to invalid IL or missing references)
		//IL_0211: Unknown result type (might be due to invalid IL or missing references)
		//IL_0213: Unknown result type (might be due to invalid IL or missing references)
		//IL_0215: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_022f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0231: Unknown result type (might be due to invalid IL or missing references)
		//IL_0233: Unknown result type (might be due to invalid IL or missing references)
		//IL_0235: Unknown result type (might be due to invalid IL or missing references)
		//IL_023a: Unknown result type (might be due to invalid IL or missing references)
		//IL_023f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0256: Unknown result type (might be due to invalid IL or missing references)
		//IL_0275: Unknown result type (might be due to invalid IL or missing references)
		//IL_027a: Unknown result type (might be due to invalid IL or missing references)
		//IL_027f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0280: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0315: Unknown result type (might be due to invalid IL or missing references)
		//IL_0317: Unknown result type (might be due to invalid IL or missing references)
		//IL_0319: Unknown result type (might be due to invalid IL or missing references)
		//IL_031b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0325: Unknown result type (might be due to invalid IL or missing references)
		//IL_032f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0334: Unknown result type (might be due to invalid IL or missing references)
		//IL_033b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0340: Unknown result type (might be due to invalid IL or missing references)
		//IL_0345: Unknown result type (might be due to invalid IL or missing references)
		//IL_0347: Unknown result type (might be due to invalid IL or missing references)
		//IL_034c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0361: Unknown result type (might be due to invalid IL or missing references)
		//IL_038a: Unknown result type (might be due to invalid IL or missing references)
		//IL_038f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0394: Unknown result type (might be due to invalid IL or missing references)
		//IL_0395: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_0403: Unknown result type (might be due to invalid IL or missing references)
		//IL_042c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0431: Unknown result type (might be due to invalid IL or missing references)
		//IL_0436: Unknown result type (might be due to invalid IL or missing references)
		//IL_0437: Unknown result type (might be due to invalid IL or missing references)
		//IL_0459: Unknown result type (might be due to invalid IL or missing references)
		//IL_045b: Unknown result type (might be due to invalid IL or missing references)
		//IL_045d: Unknown result type (might be due to invalid IL or missing references)
		//IL_045f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0469: Unknown result type (might be due to invalid IL or missing references)
		//IL_0473: Unknown result type (might be due to invalid IL or missing references)
		//IL_0478: Unknown result type (might be due to invalid IL or missing references)
		//IL_047f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0484: Unknown result type (might be due to invalid IL or missing references)
		//IL_0489: Unknown result type (might be due to invalid IL or missing references)
		//IL_048b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0490: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_04fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_04fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0501: Unknown result type (might be due to invalid IL or missing references)
		//IL_050b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0515: Unknown result type (might be due to invalid IL or missing references)
		//IL_051a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0521: Unknown result type (might be due to invalid IL or missing references)
		//IL_0526: Unknown result type (might be due to invalid IL or missing references)
		//IL_052b: Unknown result type (might be due to invalid IL or missing references)
		//IL_052d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0532: Unknown result type (might be due to invalid IL or missing references)
		//IL_0547: Unknown result type (might be due to invalid IL or missing references)
		//IL_0570: Unknown result type (might be due to invalid IL or missing references)
		//IL_0575: Unknown result type (might be due to invalid IL or missing references)
		//IL_057a: Unknown result type (might be due to invalid IL or missing references)
		//IL_057b: Unknown result type (might be due to invalid IL or missing references)
		//IL_059d: Unknown result type (might be due to invalid IL or missing references)
		//IL_059f: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_05c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_05eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_060e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0610: Unknown result type (might be due to invalid IL or missing references)
		//IL_0612: Unknown result type (might be due to invalid IL or missing references)
		//IL_0614: Unknown result type (might be due to invalid IL or missing references)
		//IL_0619: Unknown result type (might be due to invalid IL or missing references)
		//IL_061e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0633: Unknown result type (might be due to invalid IL or missing references)
		//IL_0652: Unknown result type (might be due to invalid IL or missing references)
		//IL_0657: Unknown result type (might be due to invalid IL or missing references)
		//IL_065c: Unknown result type (might be due to invalid IL or missing references)
		//IL_065d: Unknown result type (might be due to invalid IL or missing references)
		//IL_066c: Unknown result type (might be due to invalid IL or missing references)
		//IL_066e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0670: Unknown result type (might be due to invalid IL or missing references)
		//IL_0672: Unknown result type (might be due to invalid IL or missing references)
		//IL_067c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0681: Unknown result type (might be due to invalid IL or missing references)
		//IL_0686: Unknown result type (might be due to invalid IL or missing references)
		//IL_0688: Unknown result type (might be due to invalid IL or missing references)
		//IL_068a: Unknown result type (might be due to invalid IL or missing references)
		//IL_06a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_06a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_06ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_06cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_06d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_06d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_06d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_06db: Unknown result type (might be due to invalid IL or missing references)
		//IL_06dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_06df: Unknown result type (might be due to invalid IL or missing references)
		//IL_06e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_06e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_06ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_06fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0706: Unknown result type (might be due to invalid IL or missing references)
		//IL_070d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0713: Unknown result type (might be due to invalid IL or missing references)
		//IL_0718: Unknown result type (might be due to invalid IL or missing references)
		//IL_071d: Unknown result type (might be due to invalid IL or missing references)
		//IL_071f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0721: Unknown result type (might be due to invalid IL or missing references)
		//IL_0723: Unknown result type (might be due to invalid IL or missing references)
		//IL_0728: Unknown result type (might be due to invalid IL or missing references)
		//IL_072d: Unknown result type (might be due to invalid IL or missing references)
		//IL_072f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0731: Unknown result type (might be due to invalid IL or missing references)
		//IL_0736: Unknown result type (might be due to invalid IL or missing references)
		//IL_0738: Unknown result type (might be due to invalid IL or missing references)
		//IL_073a: Unknown result type (might be due to invalid IL or missing references)
		//IL_07a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_07a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_07aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_07b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_07b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_07be: Unknown result type (might be due to invalid IL or missing references)
		//IL_07c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_07c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_07cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_07cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_07f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_07f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_07fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0807: Unknown result type (might be due to invalid IL or missing references)
		//IL_080c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0811: Unknown result type (might be due to invalid IL or missing references)
		//IL_0815: Unknown result type (might be due to invalid IL or missing references)
		//IL_081a: Unknown result type (might be due to invalid IL or missing references)
		//IL_081f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0820: Unknown result type (might be due to invalid IL or missing references)
		//IL_0847: Unknown result type (might be due to invalid IL or missing references)
		//IL_0849: Unknown result type (might be due to invalid IL or missing references)
		//IL_084d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0852: Unknown result type (might be due to invalid IL or missing references)
		//IL_0857: Unknown result type (might be due to invalid IL or missing references)
		//IL_085b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0860: Unknown result type (might be due to invalid IL or missing references)
		//IL_0865: Unknown result type (might be due to invalid IL or missing references)
		//IL_0866: Unknown result type (might be due to invalid IL or missing references)
		//IL_088d: Unknown result type (might be due to invalid IL or missing references)
		//IL_088f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0893: Unknown result type (might be due to invalid IL or missing references)
		//IL_0898: Unknown result type (might be due to invalid IL or missing references)
		//IL_089d: Unknown result type (might be due to invalid IL or missing references)
		//IL_08a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_08a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_08ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_08ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_08bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_08bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_08de: Unknown result type (might be due to invalid IL or missing references)
		//IL_08e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_08fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0903: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a29: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a2e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a6e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a70: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a78: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a7d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0aa4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0aa9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0aae: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ab0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ab2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ab9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0abe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ac0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ac2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ac7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ac9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0acb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ad0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0afb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0afd: Unknown result type (might be due to invalid IL or missing references)
		//IL_092c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0932: Unknown result type (might be due to invalid IL or missing references)
		//IL_094b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0951: Unknown result type (might be due to invalid IL or missing references)
		//IL_0996: Unknown result type (might be due to invalid IL or missing references)
		//IL_099c: Unknown result type (might be due to invalid IL or missing references)
		//IL_09b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_09bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c76: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c78: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c7d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c7f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c84: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c8b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c90: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c92: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c93: Unknown result type (might be due to invalid IL or missing references)
		//IL_0cba: Unknown result type (might be due to invalid IL or missing references)
		//IL_0cc1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0cc6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ccb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ccd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ccf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ceb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d02: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d07: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d0c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b36: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c0f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c11: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c16: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c30: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b5c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b5e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b63: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b7d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b83: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b8f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e7e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ea4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ea6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ea8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0eb2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0eb9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ec0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ec5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0eca: Unknown result type (might be due to invalid IL or missing references)
		//IL_0edc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ee3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0eff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f06: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f12: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f19: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c49: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bc0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bc2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bc7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0be1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0be7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bf3: Unknown result type (might be due to invalid IL or missing references)
		//IL_1373: Unknown result type (might be due to invalid IL or missing references)
		//IL_1375: Unknown result type (might be due to invalid IL or missing references)
		//IL_137c: Unknown result type (might be due to invalid IL or missing references)
		//IL_1381: Unknown result type (might be due to invalid IL or missing references)
		//IL_1386: Unknown result type (might be due to invalid IL or missing references)
		//IL_1388: Unknown result type (might be due to invalid IL or missing references)
		//IL_139e: Unknown result type (might be due to invalid IL or missing references)
		//IL_13a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_13aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_13af: Unknown result type (might be due to invalid IL or missing references)
		//IL_13b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_13b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_13b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_13d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_13e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_13ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_13f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_13f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_142b: Unknown result type (might be due to invalid IL or missing references)
		//IL_143d: Unknown result type (might be due to invalid IL or missing references)
		//IL_144f: Unknown result type (might be due to invalid IL or missing references)
		//IL_1462: Unknown result type (might be due to invalid IL or missing references)
		//IL_1467: Unknown result type (might be due to invalid IL or missing references)
		//IL_146c: Unknown result type (might be due to invalid IL or missing references)
		//IL_146e: Unknown result type (might be due to invalid IL or missing references)
		//IL_1470: Unknown result type (might be due to invalid IL or missing references)
		//IL_148e: Unknown result type (might be due to invalid IL or missing references)
		//IL_14a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_14a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_14ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_14ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_14e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_14f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_1507: Unknown result type (might be due to invalid IL or missing references)
		//IL_151a: Unknown result type (might be due to invalid IL or missing references)
		//IL_151f: Unknown result type (might be due to invalid IL or missing references)
		//IL_1524: Unknown result type (might be due to invalid IL or missing references)
		//IL_153c: Unknown result type (might be due to invalid IL or missing references)
		//IL_153e: Unknown result type (might be due to invalid IL or missing references)
		//IL_1540: Unknown result type (might be due to invalid IL or missing references)
		//IL_1542: Unknown result type (might be due to invalid IL or missing references)
		//IL_154c: Unknown result type (might be due to invalid IL or missing references)
		//IL_1551: Unknown result type (might be due to invalid IL or missing references)
		//IL_1556: Unknown result type (might be due to invalid IL or missing references)
		//IL_1557: Unknown result type (might be due to invalid IL or missing references)
		//IL_157e: Unknown result type (might be due to invalid IL or missing references)
		//IL_1580: Unknown result type (might be due to invalid IL or missing references)
		//IL_1582: Unknown result type (might be due to invalid IL or missing references)
		//IL_1584: Unknown result type (might be due to invalid IL or missing references)
		//IL_158e: Unknown result type (might be due to invalid IL or missing references)
		//IL_1593: Unknown result type (might be due to invalid IL or missing references)
		//IL_1598: Unknown result type (might be due to invalid IL or missing references)
		//IL_1599: Unknown result type (might be due to invalid IL or missing references)
		//IL_15c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_15c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_15c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_15c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_15d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_15de: Unknown result type (might be due to invalid IL or missing references)
		//IL_15e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_15e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_160b: Unknown result type (might be due to invalid IL or missing references)
		//IL_160d: Unknown result type (might be due to invalid IL or missing references)
		//IL_160f: Unknown result type (might be due to invalid IL or missing references)
		//IL_1611: Unknown result type (might be due to invalid IL or missing references)
		//IL_1624: Unknown result type (might be due to invalid IL or missing references)
		//IL_1629: Unknown result type (might be due to invalid IL or missing references)
		//IL_162e: Unknown result type (might be due to invalid IL or missing references)
		//IL_162f: Unknown result type (might be due to invalid IL or missing references)
		//IL_1656: Unknown result type (might be due to invalid IL or missing references)
		//IL_1658: Unknown result type (might be due to invalid IL or missing references)
		//IL_1659: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e38: Unknown result type (might be due to invalid IL or missing references)
		//IL_1072: Unknown result type (might be due to invalid IL or missing references)
		//IL_1075: Unknown result type (might be due to invalid IL or missing references)
		//IL_107a: Unknown result type (might be due to invalid IL or missing references)
		//IL_1089: Unknown result type (might be due to invalid IL or missing references)
		//IL_108c: Unknown result type (might be due to invalid IL or missing references)
		//IL_109b: Unknown result type (might be due to invalid IL or missing references)
		//IL_10c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_10c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_1067: Unknown result type (might be due to invalid IL or missing references)
		//IL_1069: Unknown result type (might be due to invalid IL or missing references)
		//IL_106e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e55: Unknown result type (might be due to invalid IL or missing references)
		//IL_10cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_10cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_10d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_10d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_10f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_1115: Unknown result type (might be due to invalid IL or missing references)
		//IL_111a: Unknown result type (might be due to invalid IL or missing references)
		//IL_111f: Unknown result type (might be due to invalid IL or missing references)
		//IL_1131: Unknown result type (might be due to invalid IL or missing references)
		//IL_1138: Unknown result type (might be due to invalid IL or missing references)
		//IL_1140: Unknown result type (might be due to invalid IL or missing references)
		//IL_115c: Unknown result type (might be due to invalid IL or missing references)
		//IL_1163: Unknown result type (might be due to invalid IL or missing references)
		//IL_116b: Unknown result type (might be due to invalid IL or missing references)
		//IL_16a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_16a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_16ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_16af: Unknown result type (might be due to invalid IL or missing references)
		//IL_16e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_1703: Unknown result type (might be due to invalid IL or missing references)
		//IL_171d: Unknown result type (might be due to invalid IL or missing references)
		//IL_171e: Unknown result type (might be due to invalid IL or missing references)
		//IL_171f: Unknown result type (might be due to invalid IL or missing references)
		//IL_1721: Unknown result type (might be due to invalid IL or missing references)
		//IL_1723: Unknown result type (might be due to invalid IL or missing references)
		//IL_172a: Unknown result type (might be due to invalid IL or missing references)
		//IL_1756: Unknown result type (might be due to invalid IL or missing references)
		//IL_17c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_188c: Unknown result type (might be due to invalid IL or missing references)
		//IL_18f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_1828: Unknown result type (might be due to invalid IL or missing references)
		base.DrawSprites(sLeaser, rCam, timeStacker, camPosV2);
		float2 val = ExtraExtentions.ToF2(camPosV2);
		float num = rCam.PaletteDarkness();
		if (num > 0.5f)
		{
			num -= rCam.room.LightSourceExposure(scavenger.mainBodyChunk.pos) * Custom.LerpMap(num, 0.5f, 1f, 0f, 0.5f);
		}
		if (num != darkness || scavenger.muddy > 0)
		{
			ApplyPalette(sLeaser, rCam, rCam.currentPalette);
		}
		float num2 = Mathf.Lerp(lastFlip, flip, timeStacker);
		float2 val2 = math.lerp(drawPositions[headDrawPos, 1], drawPositions[headDrawPos, 0], timeStacker);
		float2 val3 = math.lerp(drawPositions[chestDrawPos, 1], drawPositions[chestDrawPos, 0], timeStacker);
		float2 val4 = math.lerp(drawPositions[hipsDrawPos, 1], drawPositions[hipsDrawPos, 0], timeStacker);
		float2 val5 = (val3 + val4) / 2f;
		val5 -= Custom.PerpendicularVector(ExtraExtentions.normalized(val4 - val3)) * Mathf.Lerp(5f, 15f, iVars.narrowWaist) * num2;
		sLeaser.sprites[ChestSprite].x = val3.x - val.x;
		sLeaser.sprites[ChestSprite].y = val3.y - val.y;
		sLeaser.sprites[ChestSprite].rotation = Custom.AimFromOneVectorToAnother(float2.op_Implicit(val3), float2.op_Implicit(val5));
		sLeaser.sprites[HipSprite].x = val4.x - val.x;
		sLeaser.sprites[HipSprite].y = val4.y - val.y;
		float2 val6 = Custom.DirVec(val4, val3);
		(sLeaser.sprites[WaistSprite] as TriangleMesh).MoveVertice(0, float2.op_Implicit(val4 + Custom.PerpendicularVector(ExtraExtentions.normalized(val4 - val5)) * scavenger.bodyChunks[1].rad * Mathf.Lerp(0.65f, 0.9f, iVars.WaistWidth) - val));
		(sLeaser.sprites[WaistSprite] as TriangleMesh).MoveVertice(1, float2.op_Implicit(val4 - Custom.PerpendicularVector(ExtraExtentions.normalized(val4 - val5)) * scavenger.bodyChunks[1].rad * Mathf.Lerp(0.65f, 0.9f, iVars.WaistWidth) - val));
		(sLeaser.sprites[WaistSprite] as TriangleMesh).MoveVertice(2, float2.op_Implicit(math.lerp(val5, (val3 + val4) / 2f, 0.4f) - val6 * 4f + Custom.PerpendicularVector(-val6) * scavenger.mainBodyChunk.rad * Mathf.Lerp(0.35f, 0.9f, Mathf.Pow(iVars.WaistWidth, 1.3f)) - val));
		(sLeaser.sprites[WaistSprite] as TriangleMesh).MoveVertice(3, float2.op_Implicit(math.lerp(val5, (val3 + val4) / 2f, 0.4f) - val6 * 4f - Custom.PerpendicularVector(-val6) * scavenger.mainBodyChunk.rad * Mathf.Lerp(0.35f, 0.9f, Mathf.Pow(iVars.WaistWidth, 1.3f)) - val));
		(sLeaser.sprites[WaistSprite] as TriangleMesh).MoveVertice(4, float2.op_Implicit(math.lerp(val5, (val3 + val4) / 2f, 0.4f) + val6 * 4f + Custom.PerpendicularVector(-val6) * scavenger.mainBodyChunk.rad * Mathf.Lerp(0.25f, 0.8f, Mathf.Pow(iVars.WaistWidth, 1.3f)) - val));
		(sLeaser.sprites[WaistSprite] as TriangleMesh).MoveVertice(5, float2.op_Implicit(math.lerp(val5, (val3 + val4) / 2f, 0.4f) + val6 * 4f - Custom.PerpendicularVector(-val6) * scavenger.mainBodyChunk.rad * Mathf.Lerp(0.25f, 0.8f, Mathf.Pow(iVars.WaistWidth, 1.3f)) - val));
		(sLeaser.sprites[WaistSprite] as TriangleMesh).MoveVertice(6, float2.op_Implicit(val3 + Custom.PerpendicularVector(ExtraExtentions.normalized(val5 - val3)) * scavenger.mainBodyChunk.rad * Mathf.Lerp(0.7f, 1.3f, iVars.fatness) - val));
		(sLeaser.sprites[WaistSprite] as TriangleMesh).MoveVertice(7, float2.op_Implicit(val3 - Custom.PerpendicularVector(ExtraExtentions.normalized(val5 - val3)) * scavenger.mainBodyChunk.rad * Mathf.Lerp(0.7f, 1.3f, iVars.fatness) - val));
		float2 val7 = val3 + Custom.DirVec(val5, val3) * 5f;
		float2 val8 = val7;
		float num3 = scavenger.mainBodyChunk.rad;
		float num4 = Mathf.InverseLerp(0f, 10f, Custom.DistanceToLine(val2, val4, val3) * (0f - num2));
		for (int i = 0; i < 4; i++)
		{
			float num5 = (float)i / 3f;
			float2 val9 = math.lerp(val7, val2, num5);
			val9 += Custom.PerpendicularVector(ExtraExtentions.normalized(val7 - val2)) * Mathf.Sin(num5 * (float)Math.PI) * 3f * num4 * num2;
			float2 val10 = ExtraExtentions.normalized(val8 - val9);
			float2 val11 = Custom.PerpendicularVector(val10);
			float num6 = math.distance(val9, val8) / 4f;
			float num7 = Mathf.Lerp(7f, 3f, num5) - 2f * Mathf.Sin(num5 * (float)Math.PI) * Mathf.Lerp(0.5f, 1.5f, iVars.neckThickness);
			(sLeaser.sprites[NeckSprite] as TriangleMesh).MoveVertice(i * 4, float2.op_Implicit(val8 - val11 * (num3 + num7) * 0.5f - val10 * num6 - val));
			(sLeaser.sprites[NeckSprite] as TriangleMesh).MoveVertice(i * 4 + 1, float2.op_Implicit(val8 + val11 * (num3 + num7) * 0.5f - val10 * num6 - val));
			(sLeaser.sprites[NeckSprite] as TriangleMesh).MoveVertice(i * 4 + 2, float2.op_Implicit(val9 - val11 * num7 + val10 * num6 - val));
			(sLeaser.sprites[NeckSprite] as TriangleMesh).MoveVertice(i * 4 + 3, float2.op_Implicit(val9 + val11 * num7 + val10 * num6 - val));
			val8 = val9;
			num3 = num7;
		}
		sLeaser.sprites[HeadSprite].x = val2.x - val.x;
		sLeaser.sprites[HeadSprite].y = val2.y - val.y;
		if (scavenger.King)
		{
			sLeaser.sprites[MarkSprite].x = val2.x - val.x;
			sLeaser.sprites[MarkSprite].y = val2.y - val.y + 32f;
			sLeaser.sprites[MarkSprite].alpha = Mathf.Lerp(lastMarkAlpha, markAlpha, timeStacker);
			sLeaser.sprites[MarkSprite + 1].x = val2.x - val.x;
			sLeaser.sprites[MarkSprite + 1].y = val2.y - val.y + 32f;
			sLeaser.sprites[MarkSprite + 1].alpha = 0.2f * Mathf.Lerp(lastMarkAlpha, markAlpha, timeStacker);
			sLeaser.sprites[MarkSprite + 1].scale = 1f + Mathf.Lerp(lastMarkAlpha, markAlpha, timeStacker);
		}
		float2 val12 = HeadDir(timeStacker);
		float num8 = BodyAxis(timeStacker);
		float num9 = Mathf.Lerp(lastLookUp, lookUp, timeStacker);
		float num10 = Mathf.Lerp(lastNeutralFace, neutralFace, timeStacker);
		num9 *= 1f - num10;
		float2 val13 = ExtraExtentions.normalized(math.lerp(ExtraExtentions.normalized(val12), -Custom.DegToFloat2(0f - num8), Mathf.Lerp(0.5f, 1f, Mathf.Max(Mathf.Pow(num9, 1.1f), num10))));
		float2 val14 = Custom.RotateAroundOrigo(ExtraExtentions.normalized(val13), num8);
		val12 = ExtraExtentions.normalized(val12);
		val13 = ExtraExtentions.normalized(val13);
		float num11 = Mathf.Lerp(0.6f, 1.1f, iVars.headSize);
		sLeaser.sprites[HeadSprite].rotation = Custom.VecToDeg(float2.op_Implicit(ExtraExtentions.normalized(val13)));
		if (ModManager.DLCShared)
		{
			if (maskGfx != null && !scavenger.readyToReleaseMask)
			{
				float num12 = Custom.VecToDeg(float2.op_Implicit(val12));
				if (num12 < 30f && num12 > -30f)
				{
					maskGfx.overrideAnchorVector = float2.op_Implicit(-ExtraExtentions.normalized(val12));
					maskGfx.overrideDrawVector = float2.op_Implicit(new float2(val2.x, val2.y + 4f));
				}
				else if (num12 <= 90f && num12 >= -90f)
				{
					maskGfx.overrideAnchorVector = float2.op_Implicit(-ExtraExtentions.normalized(val13));
					maskGfx.overrideDrawVector = float2.op_Implicit(new float2(val2.x, val2.y + 1f));
				}
				else
				{
					maskGfx.overrideAnchorVector = float2.op_Implicit(-ExtraExtentions.normalized(val13));
					maskGfx.overrideDrawVector = float2.op_Implicit(val2);
				}
				maskGfx.DrawSprites(sLeaser, rCam, timeStacker, float2.op_Implicit(val));
			}
			if (scavenger.readyToReleaseMask && maskGfx != null)
			{
				maskGfx.SetVisible(sLeaser, visible: false);
			}
		}
		float2 val15 = Vector3Ext.Slerp2(ExtraExtentions.normalized(val13), -ExtraExtentions.normalized(val12), num9);
		float2 val16 = val2 + val15 * (4f - Mathf.Max(10f * Mathf.Pow(num9, 0.5f), 3f * num10)) * num11;
		float2 val17 = val16 + val15 * Mathf.Lerp(-5f, 60f, iVars.eyesAngle) * Mathf.Lerp(lastEyesOpen, eyesOpen, timeStacker);
		float num13 = Mathf.Pow(Mathf.Max(0f, Mathf.Lerp(lastEyesPop, eyesPop, timeStacker)), 0.25f);
		float num14 = Mathf.Clamp(iVars.eyeSize + Mathf.Lerp(0.5f, 0.25f, num9) * num13, 0f, 1f);
		if (blindfold != null)
		{
			blindfold.drawRotationA = maskGfx.overrideRotationVector ?? ((Vector2)Vector3.Slerp(maskGfx.lastRotationA, maskGfx.rotationA, timeStacker));
			blindfold.drawRotationB = maskGfx.overrideAnchorVector ?? ((Vector2)Vector3.Slerp(maskGfx.lastRotationB, maskGfx.rotationB, timeStacker));
			blindfold.drawPos = maskGfx.overrideDrawVector ?? float2.op_Implicit(val2);
			blindfold.DrawSprites(sLeaser, rCam, timeStacker, float2.op_Implicit(val));
		}
		for (int j = 0; j < 2; j++)
		{
			float value = ((j == 0) ? (-1f) : 1f) * 0.5f + val14.x * (1f - num10);
			value = Mathf.Clamp(value, -1f, 1f);
			float2 val18 = val16 + Custom.PerpendicularVector(val15) * 8f * num11 * value;
			sLeaser.sprites[EyeSprite(j, 0)].x = val18.x - val.x;
			sLeaser.sprites[EyeSprite(j, 0)].y = val18.y - val.y;
			float num15 = Custom.AimFromOneVectorToAnother(float2.op_Implicit(val18), float2.op_Implicit(val17));
			float num16 = Mathf.Lerp(1.5f, 2f, Mathf.Pow(num9, 1.5f)) * Mathf.Lerp(0.3f, 1.5f, Mathf.Pow(num14, 0.7f)) * (1f + 0.2f * num13 * (1f - num9)) * Mathf.InverseLerp(1f, 0.7f, Mathf.Abs(value)) * Mathf.Lerp(1f, Mathf.Lerp(0.5f, 0.25f, num14), iVars.narrowEyes * Mathf.Lerp(1f, 0.5f, num13)) * Mathf.Lerp(lastEyesOpen, eyesOpen, timeStacker);
			float num17 = Mathf.Lerp(2.5f, 1.5f, Mathf.Pow(num9, 0.5f)) * Mathf.Lerp(0.3f, 1.5f, Mathf.Pow(num14, 0.7f)) * (1f + 0.2f * num13) * Mathf.InverseLerp(0f, 0.75f, Mathf.Lerp(lastEyesOpen, eyesOpen, timeStacker));
			if (iVars.pupilSize > 0f)
			{
				float2 val19 = ((!iVars.deepPupils) ? (Custom.DirVec(val18, lookPoint) * Mathf.InverseLerp(0f, 30f, math.distance(val18, lookPoint)) * Mathf.InverseLerp(0.3f, 0.7f, scavenger.abstractCreature.personality.sympathy)) : (-val12));
				val19 = Custom.RotateAroundOrigo(val19, num15);
				((float2)(ref val19))._002Ector(val19.x * num16 * (1f - iVars.pupilSize), val19.y * num17 * (1f - iVars.pupilSize));
				val19 = Custom.RotateAroundOrigo(val19, 0f - num15);
				sLeaser.sprites[EyeSprite(j, 1)].x = val18.x + val19.x - val.x;
				sLeaser.sprites[EyeSprite(j, 1)].y = val18.y + val19.y - val.y;
				sLeaser.sprites[EyeSprite(j, 1)].scaleX = num16 * 0.1f * iVars.pupilSize * (1f - 0.5f * num13);
				sLeaser.sprites[EyeSprite(j, 1)].scaleY = num17 * 0.1f * iVars.pupilSize * (1f - 0.5f * num13);
				sLeaser.sprites[EyeSprite(j, 1)].rotation = num15;
			}
			sLeaser.sprites[EyeSprite(j, 0)].scaleX = num16 * 0.1f;
			sLeaser.sprites[EyeSprite(j, 0)].scaleY = num17 * 0.1f;
			sLeaser.sprites[EyeSprite(j, 0)].rotation = num15;
		}
		if (scavenger.blind >= 10)
		{
			if (scavenger.blind == 10)
			{
				ApplyPalette(sLeaser, rCam, rCam.currentPalette);
			}
			else
			{
				for (int k = 0; k < 2; k++)
				{
					for (int l = 0; l < ((!(iVars.pupilSize > 0f)) ? 1 : 2); l++)
					{
						sLeaser.sprites[EyeSprite(k, l)].color = new Color(0.3f, 0.3f, 0.3f);
					}
				}
			}
		}
		sLeaser.sprites[HeadSprite].scaleX = Mathf.Lerp(8f, 9f, Mathf.Pow(num9, 0.5f)) * num11 / 10f;
		sLeaser.sprites[HeadSprite].scaleY = Mathf.Lerp(11f, 8f, Mathf.Pow(num9, 0.5f)) * num11 / 10f;
		for (int m = 0; m < teeth.GetLength(0); m++)
		{
			float num18 = (float)m / (float)(teeth.GetLength(0) - 1);
			float2 val20 = val16 + val15 * 4f + Custom.PerpendicularVector(val15) * Mathf.Lerp(-3f, 3f, num18) * val14.x;
			float2 val21 = val16 + val15 * Mathf.Lerp(8f, 10f, Mathf.Sin(num18 * (float)Math.PI)) * teeth[m, 0] + Custom.PerpendicularVector(val15) * Mathf.Lerp(Mathf.Lerp(-9f, 9f, num18) * Mathf.Lerp(0.5f, 1.2f, iVars.wideTeeth), -2f * Mathf.Sign(val14.x), 1f - Mathf.Abs(val14.y)) * teeth[m, 0];
			float2 val22 = val16 + val15 * Mathf.Lerp(12f, 15f, Mathf.Sin(num18 * (float)Math.PI)) * teeth[m, 0] + Custom.PerpendicularVector(val15) * Mathf.Lerp(Mathf.Lerp(-9f, 9f, num18) * Mathf.Lerp(0.5f, 1.2f, iVars.wideTeeth), -15f * Mathf.Sign(val14.x), 1f - Mathf.Abs(val14.y)) * teeth[m, 0];
			(sLeaser.sprites[TeethSprite] as TriangleMesh).MoveVertice(m * 5, float2.op_Implicit(val20 - Custom.PerpendicularVector(val20, val21) * 1f - val));
			(sLeaser.sprites[TeethSprite] as TriangleMesh).MoveVertice(m * 5 + 1, float2.op_Implicit(val20 + Custom.PerpendicularVector(val20, val21) * 1f - val));
			(sLeaser.sprites[TeethSprite] as TriangleMesh).MoveVertice(m * 5 + 2, float2.op_Implicit(val21 - Custom.PerpendicularVector(val21, val22) * teeth[m, 1] - val));
			(sLeaser.sprites[TeethSprite] as TriangleMesh).MoveVertice(m * 5 + 3, float2.op_Implicit(val21 + Custom.PerpendicularVector(val21, val22) * teeth[m, 1] - val));
			(sLeaser.sprites[TeethSprite] as TriangleMesh).MoveVertice(m * 5 + 4, float2.op_Implicit(val22 - val));
		}
		for (int n = 0; n < chestPatchShape.Length; n++)
		{
			(sLeaser.sprites[ChestPatchSprite] as TriangleMesh).MoveVertice(n, float2.op_Implicit(OnBellySurfacePos(chestPatchShape[n], timeStacker) - val));
		}
		for (int num19 = 0; num19 < 2; num19++)
		{
			hands[num19].DrawSprites(sLeaser, rCam, timeStacker, val);
		}
		for (int num20 = 0; num20 < 2; num20++)
		{
			legs[num20].DrawSprites(sLeaser, rCam, timeStacker, val);
		}
		eartlers.DrawSprites(sLeaser, rCam, timeStacker, val, val2, Vector3Ext.Slerp2(val13, -val12, num9), num9);
		if (!ModManager.DLCShared)
		{
			return;
		}
		for (int num21 = 0; num21 < Mathf.Min(scavenger.armorPieces, shells.Length); num21++)
		{
			switch (num21)
			{
			case 2:
				shells[num21].pos = float2.op_Implicit(val3);
				shells[num21].scaleX = 1.25f;
				shells[num21].scaleY = 1f;
				shells[num21].rotation = 0f;
				shells[num21].zRotation = 90f;
				break;
			case 1:
				shells[num21].pos = float2.op_Implicit(val5);
				shells[num21].scaleX = 0.75f;
				shells[num21].scaleY = 0.75f;
				shells[num21].rotation = 0f;
				shells[num21].zRotation = 90f;
				break;
			case 0:
				shells[num21].pos = float2.op_Implicit(val4);
				shells[num21].scaleX = 1f;
				shells[num21].scaleY = 0.75f;
				shells[num21].rotation = 0f;
				shells[num21].zRotation = 90f;
				break;
			}
			shells[num21].DrawSprites(sLeaser, rCam, timeStacker, float2.op_Implicit(val));
		}
		for (int num22 = Mathf.Min(scavenger.armorPieces, shells.Length); num22 < shells.Length; num22++)
		{
			shells[num22].visible = false;
			shells[num22].DrawSprites(sLeaser, rCam, timeStacker, float2.op_Implicit(val));
		}
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		darkness = rCam.PaletteDarkness();
		if (darkness > 0.5f)
		{
			darkness -= rCam.room.LightSourceExposure(scavenger.mainBodyChunk.pos) * Custom.LerpMap(darkness, 0.5f, 1f, 0f, 0.5f);
		}
		blackColor = palette.blackColor;
		if (ModManager.DLCShared)
		{
			if (maskGfx != null)
			{
				maskGfx.ApplyPalette(sLeaser, rCam, palette);
			}
			for (int i = 0; i < shells.Length; i++)
			{
				shells[i].ApplyPalette(sLeaser, rCam, palette);
			}
		}
		blindfold?.ApplyPalette(sLeaser, rCam, palette);
		Color blendedBodyColor = BlendedBodyColor;
		Color blendedHeadColor = BlendedHeadColor;
		sLeaser.sprites[ChestSprite].color = blendedBodyColor;
		sLeaser.sprites[HipSprite].color = blendedBodyColor;
		sLeaser.sprites[WaistSprite].color = blendedBodyColor;
		for (int j = 0; j < 5; j++)
		{
			sLeaser.sprites[FirstBehindLimbSprite + j].color = blendedBodyColor;
			sLeaser.sprites[FirstInFrontLimbSprite + j].color = blendedBodyColor;
		}
		sLeaser.sprites[NeckSprite].color = blendedBodyColor;
		for (int k = 0; k < 4; k++)
		{
			(sLeaser.sprites[NeckSprite] as TriangleMesh).verticeColors[k * 4] = Color.Lerp(blendedBodyColor, blendedHeadColor, ((float)k - 0.5f) / 3f);
			(sLeaser.sprites[NeckSprite] as TriangleMesh).verticeColors[k * 4 + 1] = Color.Lerp(blendedBodyColor, blendedHeadColor, ((float)k - 0.5f) / 3f);
			(sLeaser.sprites[NeckSprite] as TriangleMesh).verticeColors[k * 4 + 2] = Color.Lerp(blendedBodyColor, blendedHeadColor, (float)k / 3f);
			(sLeaser.sprites[NeckSprite] as TriangleMesh).verticeColors[k * 4 + 3] = Color.Lerp(blendedBodyColor, blendedHeadColor, (float)k / 3f);
		}
		if (iVars.handsHeadColor > 0f)
		{
			for (int l = 0; l < 2; l++)
			{
				for (int m = 0; m < 4; m++)
				{
					for (int n = 7; n < 11; n++)
					{
						(sLeaser.sprites[hands[l].firstSprite] as TriangleMesh).verticeColors[n] = Color.Lerp(blendedBodyColor, blendedHeadColor, iVars.handsHeadColor);
					}
					(sLeaser.sprites[hands[l].firstSprite] as TriangleMesh).verticeColors[6] = Color.Lerp(blendedBodyColor, blendedHeadColor, 0.5f * iVars.handsHeadColor);
					(sLeaser.sprites[hands[l].firstSprite] as TriangleMesh).verticeColors[11] = Color.Lerp(blendedBodyColor, blendedHeadColor, 0.5f * iVars.handsHeadColor);
					(sLeaser.sprites[hands[l].firstSprite] as TriangleMesh).verticeColors[5] = Color.Lerp(blendedBodyColor, blendedHeadColor, 0.2f * iVars.handsHeadColor);
					(sLeaser.sprites[hands[l].firstSprite] as TriangleMesh).verticeColors[12] = Color.Lerp(blendedBodyColor, blendedHeadColor, 0.2f * iVars.handsHeadColor);
				}
				sLeaser.sprites[hands[l].firstSprite + 1].color = Color.Lerp(blendedBodyColor, blendedHeadColor, iVars.handsHeadColor);
			}
		}
		sLeaser.sprites[HeadSprite].color = blendedHeadColor;
		eartlers.ApplyPalette(sLeaser, rCam, palette, blendedHeadColor, scavenger.Templar ? RainWorld.GoldRGB : BlendedDecorationColor);
		for (int num = 0; num < 2; num++)
		{
			sLeaser.sprites[EyeSprite(num, 0)].color = BlendedEyeColor;
			if (!(iVars.pupilSize > 0f))
			{
				continue;
			}
			if (iVars.deepPupils)
			{
				if (headColor.lightness * (1f - headColorBlack) > 0.1f)
				{
					sLeaser.sprites[EyeSprite(num, 1)].color = Color.Lerp(BlendedEyeColor, Color.Lerp(blendedBodyColor, blendedHeadColor, 0.5f), 0.5f);
					sLeaser.sprites[EyeSprite(num, 0)].color = Color.Lerp(BlendedEyeColor, new Color(1f, 1f, 1f), 0.5f);
				}
				else
				{
					sLeaser.sprites[EyeSprite(num, 0)].color = Color.Lerp(BlendedEyeColor, Color.Lerp(blendedBodyColor, blendedHeadColor, 0.5f), 0.5f);
					sLeaser.sprites[EyeSprite(num, 1)].color = Color.Lerp(BlendedEyeColor, new Color(1f, 1f, 1f), 0.5f);
				}
				continue;
			}
			switch ((!(iVars.generalMelanin < 0.5f)) ? iVars.coloredPupils : 0)
			{
			case 1:
				sLeaser.sprites[EyeSprite(num, 1)].color = Custom.HSL2RGB(bodyColor.hue, 1f, 0.35f);
				continue;
			case 2:
				sLeaser.sprites[EyeSprite(num, 1)].color = Custom.HSL2RGB(headColor.hue, 1f, 0.35f);
				continue;
			case 3:
				sLeaser.sprites[EyeSprite(num, 1)].color = Custom.HSL2RGB(decorationColor.hue, 1f, 0.35f);
				continue;
			}
			if (headColor.lightness * (1f - headColorBlack) > 0.1f)
			{
				sLeaser.sprites[EyeSprite(num, 1)].color = Color.Lerp(Custom.HSL2RGB(headColor.hue, headColor.saturation, 0.15f), blackColor, headColorBlack);
			}
			else
			{
				sLeaser.sprites[EyeSprite(num, 1)].color = blendedHeadColor;
			}
		}
		sLeaser.sprites[TeethSprite].color = blendedHeadColor;
		sLeaser.sprites[ChestPatchSprite].color = Color.Lerp(bellyColor.rgb, palette.blackColor, Mathf.Lerp(bellyColorBlack, 1f, darkness));
		base.ApplyPalette(sLeaser, rCam, palette);
	}

	public bool MuddableSprite(RoomCamera.SpriteLeaser sLeaser, int sprite)
	{
		for (int i = 0; i < 2; i++)
		{
			for (int j = 0; j < ((!(iVars.pupilSize > 0f)) ? 1 : 2); j++)
			{
				if (sprite == EyeSprite(i, j))
				{
					return false;
				}
			}
		}
		if (ModManager.DLCShared)
		{
			if (maskGfx != null && maskGfx.firstSprite <= sprite && sprite < maskGfx.firstSprite + maskGfx.TotalSprites)
			{
				return false;
			}
			for (int k = 0; k < shells.Length; k++)
			{
				if (shells[k].firstSprite <= sprite && sprite < shells[k].firstSprite + shells[k].TotalSprites)
				{
					return false;
				}
			}
			if (scavenger.King && (sprite == MarkSprite || sprite == MarkSprite + 1))
			{
				return false;
			}
		}
		if (blindfold != null && blindfold.firstSprite <= sprite && sprite < blindfold.firstSprite + blindfold.totalSprites)
		{
			return false;
		}
		if (cloak != null && cloak.firstSprite <= sprite && sprite < cloak.firstSprite + cloak.totalSprites)
		{
			return false;
		}
		return true;
	}

	public void SetUpSpecialMudSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera.SpriteLeaser mudSleaser, MudOverlay mudOverlay)
	{
		ScavengerHand[] array = hands;
		foreach (ScavengerHand scavengerHand in array)
		{
			MudUtils.SetUpTube(mudOverlay.GetMudSprite(scavengerHand.firstSprite), 80f, 5f);
		}
		ScavengerLeg[] array2 = legs;
		foreach (ScavengerLeg scavengerLeg in array2)
		{
			MudUtils.SetUpTube(mudOverlay.GetMudSprite(scavengerLeg.firstSprite), 50f, 7f);
		}
	}

	public float2 OnSpinePos(float f, float timeStacker)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		float num = OnSpineIndex(f, timeStacker);
		return math.lerp(SpineSegmentPos(Custom.IntClamp(Mathf.FloorToInt(num), 0, drawPositions.GetLength(0) - 1), timeStacker), SpineSegmentPos(Custom.IntClamp(Mathf.FloorToInt(num) + 1, 0, drawPositions.GetLength(0) - 1), timeStacker), num - Mathf.Floor(num));
	}

	public float2 SpineSegmentPos(int seg, float timeStacker)
	{
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		if (seg == 0)
		{
			float2 val = math.lerp(drawPositions[0, 1], drawPositions[0, 0], timeStacker);
			float num = Mathf.InverseLerp(0f, 10f, Custom.DistanceToLine(val, SpineSegmentPos(hipsDrawPos, timeStacker), SpineSegmentPos(chestDrawPos, timeStacker)) * (0f - math.lerp(lastFlip, flip, timeStacker)));
			return val + Custom.PerpendicularVector(ExtraExtentions.normalized(math.lerp(drawPositions[1, 1], drawPositions[1, 0], timeStacker) - val)) * 5f * num * math.lerp(lastFlip, flip, timeStacker);
		}
		return math.lerp(drawPositions[seg, 1], drawPositions[seg, 0], timeStacker);
	}

	public float2 OnBackSurfacePos(float2 relPos, float timeStacker)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		float x = relPos.x;
		x = math.clamp(x - math.lerp(lastFlip, flip, timeStacker), -1f, 1f);
		return OnSpinePos(relPos.y, timeStacker) + OnSpinePerp(relPos.y, timeStacker) * OnSpineWidth(relPos.y, timeStacker) * 0.5f * x;
	}

	[Obsolete("Use float2 parameter function instead.")]
	public Vector2 OnBackSurfacePos(Vector2 relPos, float timeStacker)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		float2 val = OnBackSurfacePos(new float2(relPos.x, relPos.y), timeStacker);
		return new Vector2(val.x, val.y);
	}

	public float2 OnBellySurfacePos(float2 relPos, float timeStacker)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		float x = relPos.x;
		x = Mathf.Clamp(x + Mathf.Lerp(lastFlip, flip, timeStacker), -1f, 1f);
		float y = relPos.y;
		y = Mathf.Lerp(y, 1f, Custom.LerpMap(math.dot(ExtraExtentions.normalized(SpineSegmentPos(1, timeStacker) - SpineSegmentPos(0, timeStacker)), ExtraExtentions.normalized(SpineSegmentPos(2, timeStacker) - SpineSegmentPos(1, timeStacker))), 1f, -1f, 0f, 0.5f));
		return OnSpinePos(y, timeStacker) + OnSpinePerp(y, timeStacker) * OnSpineWidth(y, timeStacker) * 0.5f * x;
	}

	[Obsolete("Use float2 parameter function instead.")]
	public Vector2 OnBellySurfacePos(Vector2 relPos, float timeStacker)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		float2 val = OnBellySurfacePos(new float2(relPos.x, relPos.y), timeStacker);
		return new Vector2(val.x, val.y);
	}

	public float2 OnSpineDir(float f, float timeStacker)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		float num = OnSpineIndex(f, timeStacker);
		return Vector3Ext.Slerp2(SpineSegmentDir(Mathf.FloorToInt(num), timeStacker), SpineSegmentDir(Mathf.FloorToInt(num) + 1, timeStacker), Mathf.InverseLerp(0.5f, 1f, num - Mathf.Floor(num)));
	}

	public float2 SpineSegmentDir(int seg, float timeStacker)
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		if (seg > drawPositions.GetLength(0) - 2)
		{
			return Custom.DirVec(SpineSegmentPos(drawPositions.GetLength(0) - 2, timeStacker), SpineSegmentPos(drawPositions.GetLength(0) - 1, timeStacker));
		}
		return Custom.DirVec(SpineSegmentPos(seg, timeStacker), SpineSegmentPos(seg + 1, timeStacker));
	}

	public float2 OnSpinePerp(float f, float timeStacker)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		float num = OnSpineIndex(f, timeStacker);
		return Vector3Ext.Slerp2(SpineSegmentPerp(Mathf.FloorToInt(num), timeStacker), SpineSegmentPerp(Mathf.FloorToInt(num) + 1, timeStacker), Mathf.InverseLerp(0.5f, 1f, num - Mathf.Floor(num)));
	}

	public float2 SpineSegmentPerp(int seg, float timeStacker)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		if (seg == 0)
		{
			float f = math.dot(ExtraExtentions.normalized(SpineSegmentPos(1, timeStacker) - SpineSegmentPos(0, timeStacker)), ExtraExtentions.normalized(SpineSegmentPos(2, timeStacker) - SpineSegmentPos(1, timeStacker)));
			return Vector3Ext.Slerp2(Custom.PerpendicularVector(Custom.DirVec(SpineSegmentPos(0, timeStacker), SpineSegmentPos(1, timeStacker))) * Mathf.Sign(f), SpineSegmentPerp(1, timeStacker), Mathf.InverseLerp(0.5f, 1f, 1f - Mathf.Abs(f)));
		}
		if (seg > drawPositions.GetLength(0) - 2)
		{
			return Custom.PerpendicularVector(Custom.DirVec(SpineSegmentPos(drawPositions.GetLength(0) - 2, timeStacker), SpineSegmentPos(drawPositions.GetLength(0) - 1, timeStacker)));
		}
		return Custom.PerpendicularVector(Custom.DirVec(SpineSegmentPos(seg, timeStacker), SpineSegmentPos(seg + 1, timeStacker)));
	}

	public float2 OnSpineOutwardsDir(float2 ps, float timeStacker)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		float num = OnSpineIndex(ps.y, timeStacker);
		float dt = math.dot(ExtraExtentions.normalized(SpineSegmentPos(1, timeStacker) - SpineSegmentPos(0, timeStacker)), ExtraExtentions.normalized(SpineSegmentPos(2, timeStacker) - SpineSegmentPos(1, timeStacker)));
		float num2 = Mathf.Clamp(ps.x - Mathf.Lerp(lastFlip, flip, timeStacker), -1f, 1f);
		return Vector3Ext.Slerp2(Vector3Ext.Slerp2(SpineSegmentOutwardsDir(Mathf.FloorToInt(num), timeStacker, dt, num2), SpineSegmentOutwardsDir(Mathf.FloorToInt(num) + 1, timeStacker, dt, num2), Mathf.InverseLerp(0f, 1f, num - Mathf.Floor(num))) * math.sign(num2), new float2(ps.y, timeStacker), Mathf.Pow(1f - Mathf.Abs(num2), 3f));
	}

	[Obsolete("Use float2 parameter function instead.")]
	public Vector2 OnSpineOutwardsDir(Vector2 ps, float timeStacker)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		float2 val = OnSpineOutwardsDir(new float2(ps.x, ps.y), timeStacker);
		return new Vector2(val.x, val.y);
	}

	public float2 SpineSegmentOutwardsDir(int seg, float timeStacker, float dt, float psX)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		if (seg == 0)
		{
			float2 val = Custom.PerpendicularVector(Custom.DirVec(SpineSegmentPos(0, timeStacker), SpineSegmentPos(1, timeStacker)));
			return Vector3Ext.Slerp2(Custom.PerpendicularVector(Custom.DirVec(SpineSegmentPos(1, timeStacker), SpineSegmentPos(2, timeStacker))), val * math.sign(dt), math.pow(math.abs(dt), 3f));
		}
		if (seg > drawPositions.GetLength(0) - 2)
		{
			return Custom.PerpendicularVector(Custom.DirVec(SpineSegmentPos(drawPositions.GetLength(0) - 2, timeStacker), SpineSegmentPos(drawPositions.GetLength(0) - 1, timeStacker)));
		}
		return Custom.PerpendicularVector(Custom.DirVec(SpineSegmentPos(seg, timeStacker), SpineSegmentPos(seg + 1, timeStacker)));
	}

	public float2 OnSpineUpDir(float f, float timeStacker)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		float num = OnSpineIndex(f, timeStacker);
		float num2 = Mathf.Lerp(lastFlip, flip, timeStacker);
		float dt = math.dot(ExtraExtentions.normalized(SpineSegmentPos(1, timeStacker) - SpineSegmentPos(0, timeStacker)), ExtraExtentions.normalized(SpineSegmentPos(2, timeStacker) - SpineSegmentPos(1, timeStacker)));
		return Vector3Ext.Slerp2(Vector3Ext.Slerp2(SpineSegmentUpDir(Mathf.FloorToInt(num), timeStacker, dt, num2), SpineSegmentUpDir(Mathf.FloorToInt(num) + 1, timeStacker, dt, num2), Mathf.InverseLerp(0f, 1f, num - Mathf.Floor(num))), -OnSpineDirForUps(f, timeStacker, dt), 1f - Mathf.Abs(num2));
	}

	public float2 SpineSegmentUpDir(int seg, float timeStacker, float dt, float uFlp)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		if (seg == 0)
		{
			float2 val = Custom.PerpendicularVector(Custom.DirVec(SpineSegmentPos(0, timeStacker), SpineSegmentPos(1, timeStacker))) * (0f - Mathf.Sign(uFlp));
			float f = Custom.DistanceToLine(SpineSegmentPos(0, timeStacker), SpineSegmentPos(1, timeStacker), SpineSegmentPos(2, timeStacker));
			return Vector3Ext.Slerp2(val * Mathf.Sign(f) * Mathf.Sign(uFlp), -SpineSegmentDir(1, timeStacker), Mathf.Abs(dt));
		}
		if (seg > drawPositions.GetLength(0) - 2)
		{
			return Custom.PerpendicularVector(Custom.DirVec(SpineSegmentPos(drawPositions.GetLength(0) - 2, timeStacker), SpineSegmentPos(drawPositions.GetLength(0) - 1, timeStacker))) * (0f - Mathf.Sign(uFlp));
		}
		return Custom.PerpendicularVector(Custom.DirVec(SpineSegmentPos(seg, timeStacker), SpineSegmentPos(seg + 1, timeStacker))) * (0f - Mathf.Sign(uFlp));
	}

	public float2 OnSpineDirForUps(float f, float timeStacker, float dt)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		float num = OnSpineIndex(f, timeStacker);
		return Vector3Ext.Slerp2(SpineSegmentDirForUps(Mathf.FloorToInt(num), timeStacker, dt), SpineSegmentDirForUps(Mathf.FloorToInt(num) + 1, timeStacker, dt), Mathf.InverseLerp(0.5f, 1f, num - Mathf.Floor(num)));
	}

	public float2 SpineSegmentDirForUps(int seg, float timeStacker, float dt)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		if (seg == 0)
		{
			return SpineSegmentDir(1, timeStacker);
		}
		if (seg > drawPositions.GetLength(0) - 2)
		{
			return Custom.DirVec(SpineSegmentPos(drawPositions.GetLength(0) - 2, timeStacker), SpineSegmentPos(drawPositions.GetLength(0) - 1, timeStacker));
		}
		return Custom.DirVec(SpineSegmentPos(seg, timeStacker), SpineSegmentPos(seg + 1, timeStacker));
	}

	public float OnSpineWidth(float f, float timeStacker)
	{
		float num = OnSpineIndex(f, timeStacker);
		int num2 = Mathf.FloorToInt(num);
		return math.lerp(SpineSegmentWidth(num2, timeStacker), SpineSegmentWidth(num2 + 1, timeStacker), Mathf.InverseLerp(0.5f, 1f, num - Mathf.Floor(num)));
	}

	public float SpineSegmentWidth(int seg, float timeStacker)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		return seg switch
		{
			0 => 4f * math.lerp(0.5f, 1.5f, iVars.neckThickness) * math.abs(math.sign(math.dot(ExtraExtentions.normalized(SpineSegmentPos(1, timeStacker) - SpineSegmentPos(0, timeStacker)), ExtraExtentions.normalized(SpineSegmentPos(2, timeStacker) - SpineSegmentPos(1, timeStacker))))), 
			1 => math.lerp(8f, 18f, iVars.fatness), 
			2 => math.lerp(10f, 20f, iVars.fatness), 
			3 => math.lerp(4f, 8f, iVars.WaistWidth), 
			4 => 10f, 
			_ => 0f, 
		};
	}

	public float OnSpineIndex(float f, float timeStacker)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		float num = f * math.lerp(spineLengths[1], spineLengths[0], timeStacker);
		float a = 0f;
		float num2 = math.distance(math.lerp(drawPositions[0, 1], drawPositions[0, 0], timeStacker), math.lerp(drawPositions[1, 1], drawPositions[1, 0], timeStacker));
		int num3;
		for (num3 = 0; num2 < num; num2 += math.distance(math.lerp(drawPositions[num3, 1], drawPositions[num3, 1], timeStacker), math.lerp(drawPositions[num3 + 1, 1], drawPositions[num3 + 1, 1], timeStacker)))
		{
			a = num2;
			num3++;
			if (num3 >= drawPositions.GetLength(0) - 1)
			{
				break;
			}
		}
		return (float)num3 + Mathf.InverseLerp(a, num2, num);
	}

	public float2 ItemPosition(int grasp)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		if (grasp == 0)
		{
			return ExtraExtentions.ToF2(hands[0].pos) + hands[0].spearPosAdd;
		}
		if (scavenger.grasps[grasp].grabbed is Spear)
		{
			return OnBackSurfacePos(new float2((0f - flip) * 0.9f, 0.4f + (float)(grasp - 1) * 0.1f), 1f);
		}
		if (scavenger.grasps[grasp].grabbed is Lantern)
		{
			return float2.op_Implicit(hands[1].pos);
		}
		return math.lerp(float2.op_Implicit(scavenger.bodyChunks[1].pos), float2.op_Implicit(scavenger.bodyChunks[0].pos), 0.4f - 0.1f * (float)(grasp - 1));
	}

	public float2 ItemDirection(int grasp)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		if (grasp == 0)
		{
			return hands[0].WeaponDir();
		}
		if (scavenger.grasps[grasp].grabbed is Spear)
		{
			return Custom.RotateAroundOrigo(Custom.DegToFloat2(45f - flip * (float)(grasp - 1) * 10f + Custom.LerpMap(flip, -1f, 1f, -30f, 0f)), Custom.VecToDeg(float2.op_Implicit(Vector3Ext.Slerp2(bodyAxis, Custom.DirVec(drawPositions[hipsDrawPos, 1], drawPositions[chestDrawPos, 1]), 0.75f))));
		}
		return float2.op_Implicit(Custom.DirVec(scavenger.grasps[grasp].grabbed.firstChunk.pos, ItemPosition(grasp)));
	}

	public void GenerateColors()
	{
		//IL_021c: Unknown result type (might be due to invalid IL or missing references)
		//IL_022c: Unknown result type (might be due to invalid IL or missing references)
		//IL_022d: Unknown result type (might be due to invalid IL or missing references)
		//IL_022e: Unknown result type (might be due to invalid IL or missing references)
		//IL_023d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0248: Unknown result type (might be due to invalid IL or missing references)
		//IL_024d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0264: Unknown result type (might be due to invalid IL or missing references)
		//IL_0279: Unknown result type (might be due to invalid IL or missing references)
		float num = UnityEngine.Random.value * 0.1f;
		if (UnityEngine.Random.value < 0.025f)
		{
			num = Mathf.Pow(UnityEngine.Random.value, 0.4f);
		}
		if (scavenger.Elite)
		{
			num = Mathf.Pow(UnityEngine.Random.value, 5f);
		}
		float num2 = num + Mathf.Lerp(-1f, 1f, UnityEngine.Random.value) * 0.3f * Mathf.Pow(UnityEngine.Random.value, 2f);
		if (num2 > 1f)
		{
			num2 -= 1f;
		}
		else if (num2 < 0f)
		{
			num2 += 1f;
		}
		bodyColor = new HSLColor(num, Mathf.Lerp(0.05f, 1f, Mathf.Pow(UnityEngine.Random.value, 0.85f)), Mathf.Lerp(0.05f, 0.8f, UnityEngine.Random.value));
		bodyColor.saturation *= 1f - iVars.generalMelanin;
		bodyColor.lightness = Mathf.Lerp(bodyColor.lightness, 0.5f + 0.5f * Mathf.Pow(UnityEngine.Random.value, 0.8f), 1f - iVars.generalMelanin);
		bodyColorBlack = Custom.LerpMap((bodyColor.rgb.r + bodyColor.rgb.g + bodyColor.rgb.b) / 3f, 0.04f, 0.8f, 0.3f, 0.95f, 0.5f);
		bodyColorBlack = Mathf.Lerp(bodyColorBlack, Mathf.Lerp(0.5f, 1f, UnityEngine.Random.value), UnityEngine.Random.value * UnityEngine.Random.value * UnityEngine.Random.value);
		bodyColorBlack *= iVars.generalMelanin;
		float2 val = default(float2);
		((float2)(ref val))._002Ector(bodyColor.saturation, Mathf.Lerp(-1f, 1f, bodyColor.lightness * (1f - bodyColorBlack)));
		if (ExtraExtentions.magnitude(val) < 0.5f)
		{
			val = math.lerp(val, ExtraExtentions.normalized(val), Mathf.InverseLerp(0.5f, 0.3f, ExtraExtentions.magnitude(val)));
			bodyColor = new HSLColor(bodyColor.hue, Mathf.InverseLerp(-1f, 1f, val.x), Mathf.InverseLerp(-1f, 1f, val.y));
			bodyColorBlack = Custom.LerpMap((bodyColor.rgb.r + bodyColor.rgb.g + bodyColor.rgb.b) / 3f, 0.04f, 0.8f, 0.3f, 0.95f, 0.5f);
			bodyColorBlack = Mathf.Lerp(bodyColorBlack, Mathf.Lerp(0.5f, 1f, UnityEngine.Random.value), UnityEngine.Random.value * UnityEngine.Random.value * UnityEngine.Random.value);
			bodyColorBlack *= iVars.generalMelanin;
		}
		float a;
		if (UnityEngine.Random.value < Custom.LerpMap(bodyColorBlack, 0.5f, 0.8f, 0.9f, 0.3f))
		{
			a = num2 + Mathf.Lerp(-1f, 1f, UnityEngine.Random.value) * 0.1f * Mathf.Pow(UnityEngine.Random.value, 1.5f);
			a = Mathf.Lerp(a, 0.15f, UnityEngine.Random.value);
			if (a > 1f)
			{
				a -= 1f;
			}
			else if (a < 0f)
			{
				a += 1f;
			}
		}
		else
		{
			a = ((UnityEngine.Random.value < 0.5f) ? Custom.Decimal(num + 0.5f) : Custom.Decimal(num2 + 0.5f)) + Mathf.Lerp(-1f, 1f, UnityEngine.Random.value) * 0.25f * Mathf.Pow(UnityEngine.Random.value, 2f);
			if (UnityEngine.Random.value < Mathf.Lerp(0.8f, 0.2f, scavenger.abstractCreature.personality.energy))
			{
				a = Mathf.Lerp(a, 0.15f, UnityEngine.Random.value);
			}
			if (a > 1f)
			{
				a -= 1f;
			}
			else if (a < 0f)
			{
				a += 1f;
			}
		}
		headColor = new HSLColor((UnityEngine.Random.value < 0.75f) ? num2 : a, 1f, 0.05f + 0.15f * UnityEngine.Random.value);
		headColor.saturation *= Mathf.Pow(1f - iVars.generalMelanin, 2f);
		headColor.lightness = Mathf.Lerp(headColor.lightness, 0.5f + 0.5f * Mathf.Pow(UnityEngine.Random.value, 0.8f), 1f - iVars.generalMelanin);
		headColor.saturation *= 0.1f + 0.9f * Mathf.InverseLerp(0.1f, 0f, Custom.DistanceBetweenZeroToOneFloats(bodyColor.hue, headColor.hue) * Custom.LerpMap(Mathf.Abs(0.5f - headColor.lightness), 0f, 0.5f, 1f, 0.3f));
		if (headColor.lightness < 0.5f)
		{
			headColor.lightness *= 0.5f + 0.5f * Mathf.InverseLerp(0.2f, 0.05f, Custom.DistanceBetweenZeroToOneFloats(bodyColor.hue, headColor.hue));
		}
		headColorBlack = Custom.LerpMap((headColor.rgb.r + headColor.rgb.g + headColor.rgb.b) / 3f, 0.035f, 0.26f, 0.7f, 0.95f, 0.25f);
		headColorBlack = Mathf.Lerp(headColorBlack, Mathf.Lerp(0.8f, 1f, UnityEngine.Random.value), UnityEngine.Random.value * UnityEngine.Random.value * UnityEngine.Random.value);
		headColorBlack *= 0.2f + 0.7f * iVars.generalMelanin;
		headColorBlack = Mathf.Max(headColorBlack, bodyColorBlack);
		headColor.saturation = Custom.LerpMap(headColor.lightness * (1f - headColorBlack), 0f, 0.15f, 1f, headColor.saturation);
		if (headColor.lightness > bodyColor.lightness)
		{
			headColor = bodyColor;
		}
		if (headColor.saturation < bodyColor.saturation * 0.75f)
		{
			if (UnityEngine.Random.value < 0.5f)
			{
				headColor.hue = bodyColor.hue;
			}
			else
			{
				headColor.lightness *= 0.25f;
			}
			headColor.saturation = bodyColor.saturation * 0.75f;
		}
		decorationColor = new HSLColor((UnityEngine.Random.value < 0.65f) ? num : ((UnityEngine.Random.value < 0.5f) ? num2 : a), UnityEngine.Random.value, 0.5f + 0.5f * Mathf.Pow(UnityEngine.Random.value, 0.5f));
		decorationColor.lightness *= Mathf.Lerp(iVars.generalMelanin, UnityEngine.Random.value, 0.5f);
		eyeColor = new HSLColor(scavenger.Elite ? 0f : a, 1f, (UnityEngine.Random.value < 0.2f) ? (0.5f + UnityEngine.Random.value * 0.5f) : 0.5f);
		if (iVars.coloredPupils > 0)
		{
			eyeColor.lightness = Mathf.Lerp(eyeColor.lightness, 1f, 0.3f);
		}
		if (headColor.lightness * (1f - headColorBlack) > eyeColor.lightness / 2f && (iVars.pupilSize == 0f || iVars.deepPupils))
		{
			eyeColor.lightness *= 0.2f;
		}
		float value = UnityEngine.Random.value;
		float value2 = UnityEngine.Random.value;
		bellyColor = new HSLColor(Mathf.Lerp(bodyColor.hue, decorationColor.hue, value * 0.7f), bodyColor.saturation * Mathf.Lerp(1f, 0.5f, value), bodyColor.lightness + 0.05f + 0.3f * value2);
		bellyColorBlack = Mathf.Lerp(bodyColorBlack, 1f, 0.3f * Mathf.Pow(value2, 1.4f));
		if (UnityEngine.Random.value < 1f / 30f)
		{
			headColor.lightness = Mathf.Lerp(0.2f, 0.35f, UnityEngine.Random.value);
			headColorBlack *= Mathf.Lerp(1f, 0.8f, UnityEngine.Random.value);
			bellyColor.hue = Mathf.Lerp(bellyColor.hue, headColor.hue, Mathf.Pow(UnityEngine.Random.value, 0.5f));
		}
	}
}
