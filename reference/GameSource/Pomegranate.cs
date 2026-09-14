using System;
using System.Collections.Generic;
using System.Globalization;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class Pomegranate : PlayerCarryableItem, IDrawable
{
	public class Stalk : UpdatableAndDeletable, IDrawable
	{
		public Pomegranate fruit;

		public Vector2 stuckPos;

		public float ropeLength;

		public int attachIndex;

		public Vector2 wallStickDirection;

		public Vector2 generateDirection;

		public SharedPhysics.TerrainCollisionData scratchTerrainCollisionData;

		public Color shellColor;

		public Vector2[,] segs;

		public float[,] darkness;

		public float[,] protrusions;

		public float[] protoPomegranates;

		public List<SubStalk> subStalks;

		public bool initialized;

		public float connRad;

		public PlacedObject.PomegranateData data => fruit.data;

		public bool isCosmetic => this is CosmeticStalk;

		public Stalk(Pomegranate fruit, Room room, Vector2 fruitPos, int index)
		{
			this.fruit = fruit;
			if (fruit != null)
			{
				shellColor = fruit.shellColor;
			}
			ropeLength = -1f;
			attachIndex = index;
		}

		public void Initialize(Room placeRoom)
		{
			if (this is CosmeticStalk)
			{
				return;
			}
			UnityEngine.Random.State state = UnityEngine.Random.state;
			generateDirection = Vector2.zero;
			UnityEngine.Random.InitState((attachIndex == 1) ? fruit.stalkSeeds[0] : fruit.stalkSeeds[1]);
			Vector2 pos = fruit.firstChunk.pos;
			if (fruit.AbstrPomegranate.placedObjectIndex >= 0 && fruit.AbstrPomegranate.placedObjectIndex < placeRoom.roomSettings.placedObjects.Count && data != null)
			{
				pos = placeRoom.roomSettings.placedObjects[fruit.AbstrPomegranate.placedObjectIndex].pos;
				generateDirection = Custom.DirVec(pos, pos + data.handlePos);
			}
			else if (fruit.AbstrPomegranate.placedObjectIndex == -1 && placeRoom.game.IsArenaSession && placeRoom.game.GetArenaGameSession.counter < 10)
			{
				IntVector2 pos2 = fruit.AbstrPomegranate.pos.Tile;
				int num = 0;
				int num2 = 0;
				while (0 <= pos2.x && pos2.x < placeRoom.TileWidth)
				{
					if (0 <= pos2.y && pos2.y < placeRoom.TileHeight && placeRoom.GetTile(pos2).IsSolid())
					{
						generateDirection = Custom.DirVec(placeRoom.MiddleOfTile(fruit.AbstrPomegranate.pos.Tile), placeRoom.MiddleOfTile(pos2)) * attachIndex;
						break;
					}
					num2++;
					if (num2 > num)
					{
						num++;
						num2 = -num;
					}
					pos2 = fruit.AbstrPomegranate.pos.Tile + new IntVector2(num, num2) * attachIndex;
				}
			}
			if (generateDirection != Vector2.zero)
			{
				Vector2 pos3 = pos;
				while (pos3.x > 0f && pos3.y > 0f && pos3.x < placeRoom.PixelWidth && pos3.y < placeRoom.PixelHeight)
				{
					IntVector2 tilePosition = placeRoom.GetTilePosition(pos3);
					if (placeRoom.GetTile(tilePosition.x, tilePosition.y).Solid)
					{
						stuckPos = pos3;
						break;
					}
					pos3 += generateDirection * (attachIndex * 5);
				}
				Vector2 vector = stuckPos - generateDirection * (attachIndex * 5);
				IntVector2 tilePosition2 = placeRoom.GetTilePosition(vector + new Vector2(10f, 0f));
				IntVector2 tilePosition3 = placeRoom.GetTilePosition(vector + new Vector2(0f, 10f));
				IntVector2 tilePosition4 = placeRoom.GetTilePosition(vector + new Vector2(0f, -10f));
				IntVector2 tilePosition5 = placeRoom.GetTilePosition(vector + new Vector2(-10f, 0f));
				Room.Tile tile = placeRoom.GetTile(tilePosition2.x, tilePosition2.y);
				Room.Tile tile2 = placeRoom.GetTile(tilePosition3.x, tilePosition3.y);
				Room.Tile tile3 = placeRoom.GetTile(tilePosition4.x, tilePosition4.y);
				Room.Tile tile4 = placeRoom.GetTile(tilePosition5.x, tilePosition5.y);
				if (tile.Solid && !tile4.Solid)
				{
					wallStickDirection = new Vector2(0f, 1f);
					stuckPos.x = placeRoom.MiddleOfTile(tilePosition2).x - 10f;
				}
				else if (tile2.Solid && !tile3.Solid)
				{
					wallStickDirection = new Vector2(1f, 0f);
					stuckPos.y = placeRoom.MiddleOfTile(tilePosition3).y - 10f;
				}
				else if (tile3.Solid && !tile2.Solid)
				{
					wallStickDirection = new Vector2(-1f, 0f);
					stuckPos.y = placeRoom.MiddleOfTile(tilePosition4).y + 10f;
				}
				else if (tile4.Solid && !tile.Solid)
				{
					wallStickDirection = new Vector2(0f, -1f);
					stuckPos.x = placeRoom.MiddleOfTile(tilePosition5).x + 10f;
				}
				ropeLength = Mathf.Abs((stuckPos - pos).magnitude) * 0.75f - 10f;
			}
			else
			{
				generateDirection = new Vector2(1f, 0f);
				ropeLength = UnityEngine.Random.Range(100f, 150f);
			}
			if (placeRoom.game.IsArenaSession && ropeLength > 250f)
			{
				ropeLength = 0f;
				generateDirection = Vector2.zero;
			}
			if (ropeLength == 0f)
			{
				return;
			}
			ropeLength = Mathf.Max(ropeLength, 30f);
			segs = new Vector2[Mathf.Max(6, (int)(ropeLength / 5f)), 3];
			darkness = new float[segs.GetLength(0), 2];
			for (int i = 0; i < segs.GetLength(0); i++)
			{
				float t = (float)i / (float)(segs.GetLength(0) - 1);
				if (!fruit.disconnected)
				{
					segs[i, 0] = Vector2.Lerp(stuckPos, pos, t);
				}
				else
				{
					segs[i, 0] = pos + Custom.RNV() * UnityEngine.Random.Range(0f, 10f);
				}
				segs[i, 1] = segs[i, 0];
			}
			connRad = ropeLength / Mathf.Pow(segs.GetLength(0), 1.1f);
			protrusions = new float[segs.GetLength(0) / 2 - 2, 2];
			for (int j = 0; j < protrusions.GetLength(0); j++)
			{
				protrusions[j, 0] = UnityEngine.Random.Range(-120, 120);
				protrusions[j, 1] = Mathf.Pow(UnityEngine.Random.value, 0.25f);
			}
			protoPomegranates = new float[segs.GetLength(0) / 2 - 2];
			if (UnityEngine.Random.value < 0.66f && segs.GetLength(0) > 10)
			{
				for (int k = 0; k < UnityEngine.Random.Range(1, segs.GetLength(0) / 10); k++)
				{
					int num3 = UnityEngine.Random.Range(0, protoPomegranates.Length);
					protoPomegranates[num3] = UnityEngine.Random.Range(0.7f, 1f);
					if (UnityEngine.Random.value < 0.4f && num3 > 0 && num3 < protoPomegranates.Length - 1)
					{
						protoPomegranates[num3 + (int)Mathf.Sign(UnityEngine.Random.Range(-1f, 1f))] = protoPomegranates[num3] * UnityEngine.Random.Range(0.75f, 0.9f);
					}
				}
			}
			subStalks = new List<SubStalk>();
			int num4 = 1;
			int num5 = 0;
			for (int l = 0; l < num4; l++)
			{
				int num6 = 0;
				while (num6 < 40)
				{
					num6++;
					int num7 = UnityEngine.Random.Range(Mathf.Min(2, segs.GetLength(0) / 8), Mathf.Max(segs.GetLength(0) / 2, Mathf.Min(segs.GetLength(0), 4)));
					Vector2 vector2 = Vector2.Lerp(stuckPos, pos, (float)num7 / (float)segs.GetLength(0));
					Vector2 normalized = (Custom.RNV() + generateDirection * 2f).normalized;
					Vector2 vector3 = vector2;
					Vector2 vector4 = new Vector2(0f, 0f);
					IntVector2 tilePosition6 = placeRoom.GetTilePosition(vector3);
					while (!placeRoom.GetTile(tilePosition6.x, tilePosition6.y).Solid && vector3.x > 0f && vector3.x < placeRoom.PixelWidth && vector3.y > 0f && vector3.y < placeRoom.PixelHeight)
					{
						vector3 += normalized * 5f;
						tilePosition6 = placeRoom.GetTilePosition(vector3);
					}
					IntVector2 tilePosition7 = placeRoom.GetTilePosition(vector3 - normalized * 5f + new Vector2(10f, 0f));
					IntVector2 tilePosition8 = placeRoom.GetTilePosition(vector3 - normalized * 5f + new Vector2(0f, 10f));
					IntVector2 tilePosition9 = placeRoom.GetTilePosition(vector3 - normalized * 5f + new Vector2(0f, -10f));
					IntVector2 tilePosition10 = placeRoom.GetTilePosition(vector3 - normalized * 5f + new Vector2(-10f, 0f));
					Room.Tile tile5 = placeRoom.GetTile(tilePosition7.x, tilePosition7.y);
					Room.Tile tile6 = placeRoom.GetTile(tilePosition8.x, tilePosition8.y);
					Room.Tile tile7 = placeRoom.GetTile(tilePosition9.x, tilePosition9.y);
					Room.Tile tile8 = placeRoom.GetTile(tilePosition10.x, tilePosition10.y);
					if (tile5.Solid && !tile8.Solid)
					{
						vector4 = new Vector2(0f, 1f);
						vector3.x = placeRoom.MiddleOfTile(tilePosition7).x - 10f;
					}
					else if (tile6.Solid && !tile7.Solid)
					{
						vector4 = new Vector2(1f, 0f);
						vector3.y = placeRoom.MiddleOfTile(tilePosition8).y - 10f;
					}
					else if (tile7.Solid && !tile6.Solid)
					{
						vector4 = new Vector2(-1f, 0f);
						vector3.y = placeRoom.MiddleOfTile(tilePosition9).y + 10f;
					}
					else if (tile8.Solid && !tile5.Solid)
					{
						vector4 = new Vector2(0f, -1f);
						vector3.x = placeRoom.MiddleOfTile(tilePosition10).x + 10f;
					}
					float num8 = Mathf.Abs((vector3 - vector2).magnitude) * 0.75f - 10f;
					if (num8 < Mathf.Max(ropeLength / 2f, 20f) && num8 > 10f && Mathf.Abs((vector3 - stuckPos).magnitude) > 15f && vector4 != new Vector2(0f, 0f))
					{
						SubStalk subStalk = new SubStalk(this, num7, vector4, UnityEngine.Random.Range(-10000, 10000), SubStalkSprite(num5));
						num5++;
						subStalk.stuckPos = vector3;
						subStalks.Add(subStalk);
						break;
					}
				}
			}
			for (int m = 0; m < subStalks.Count; m++)
			{
				subStalks[m].Initialize();
			}
			UnityEngine.Random.state = state;
			initialized = true;
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			if (!initialized)
			{
				return;
			}
			if (ropeLength == -1f)
			{
				Destroy();
				return;
			}
			ConnectSegments(dir: true);
			ConnectSegments(dir: false);
			for (int i = 0; i < segs.GetLength(0); i++)
			{
				float num = ((i % 2 == 1 || i / 2 > protoPomegranates.Length - 1) ? 0f : protoPomegranates[i / 2]);
				segs[i, 1] = segs[i, 0];
				segs[i, 0] += segs[i, 2];
				segs[i, 2] *= (room.PointSubmerged(segs[i, 0]) ? 0.9f : 0.98f);
				segs[i, 2].y -= (0.9f + num / 4f) * room.gravity;
			}
			ConnectSegments(dir: false);
			ConnectSegments(dir: true);
			for (int j = 0; j < subStalks.Count; j++)
			{
				subStalks[j].Update();
			}
			for (int k = ((fruit == null || !fruit.disconnected) ? 1 : 0); k < (isCosmetic ? segs.GetLength(0) : (segs.GetLength(0) - 1)); k++)
			{
				SharedPhysics.TerrainCollisionData cd = scratchTerrainCollisionData.Set(segs[k, 0], segs[k, 1], segs[k, 2], 3f, new IntVector2(0, 0), goThroughFloors: true);
				cd = SharedPhysics.VerticalCollision(room, cd);
				cd = SharedPhysics.HorizontalCollision(room, cd);
				segs[k, 0] = cd.pos;
				segs[k, 2] = cd.vel;
			}
		}

		public Vector2 GetAttachmentPosition(float timeStacker)
		{
			Vector2 vector = Vector2.Lerp(fruit.firstChunk.lastPos, fruit.firstChunk.pos, timeStacker);
			if (fruit.smashed)
			{
				int num = ((attachIndex == 1) ? 1 : 0);
				if (fruit.shellsInverted)
				{
					num = ((num != 1) ? 1 : 0);
				}
				Vector2 vector2 = Vector2.Lerp(fruit.smashedBits[num, 1], fruit.smashedBits[num, 0], timeStacker);
				Vector2 vector3 = Custom.DegToVec(Custom.AimFromOneVectorToAnother(vector, vector2));
				return vector2 + vector3 * (8f * fruit.shellHalves[num]);
			}
			return vector + Custom.PerpendicularVector(fruit.rotation) * ((float)attachIndex * -18f * fruit.overallSize);
		}

		public Color GetColorAtLength(int index, int cap, int coloredPoints, bool isShade, RoomPalette palette, Vector2 position, RoomCamera rCam)
		{
			float num = 0f;
			if (isCosmetic)
			{
				float num2 = cap / segs.GetLength(0);
				float num3 = (float)index / num2;
				float num4 = (float)coloredPoints / num2;
				for (int i = 0; i < protoPomegranates.Length; i++)
				{
					if (protoPomegranates[i] > 1f)
					{
						float val = Mathf.Abs(num3 - (float)((i + 1) * 2)) / num4;
						num = Mathf.Clamp(Mathf.Max(num, Custom.LerpMap(val, 0.5f, 1.5f, 0.25f, 0f)), 0f, 0.25f);
					}
				}
			}
			else
			{
				num = (float)(Mathf.Clamp(index, cap - coloredPoints, cap) - (cap - coloredPoints)) / ((float)coloredPoints - 1f);
			}
			float t = rCam.room.Darkness(position) * (1f - rCam.room.LightSourceExposure(position));
			Color color = shellColor;
			if (isCosmetic)
			{
				Color.RGBToHSV(color, out var H, out var S, out var V);
				H += 0.05f;
				color = Color.HSVToRGB(H, S, V);
			}
			if (isShade)
			{
				color = Color.Lerp(color, Color.Lerp(palette.fogColor, new Color(1f, 1f, 1f), 0.9f), 0.25f);
			}
			return Color.Lerp(Color.Lerp(palette.blackColor, color, Mathf.Lerp(0.05f, 1f, num)), palette.blackColor, t);
		}

		public void ConnectSegments(bool dir)
		{
			int num = ((!dir) ? (segs.GetLength(0) - 1) : 0);
			float num2 = 1f;
			num2 = ((!isCosmetic) ? ((!fruit.disconnected) ? 1f : 1.75f) : ((this as CosmeticStalk).wallStickDirection2.HasValue ? 1f : 1.75f));
			bool flag = false;
			while (!flag)
			{
				if (num == 0 && (isCosmetic || !fruit.disconnected))
				{
					if (!Custom.DistLess(segs[num, 0], stuckPos, connRad * num2))
					{
						Vector2 vector = Custom.DirVec(segs[num, 0], stuckPos) * (Vector2.Distance(segs[num, 0], stuckPos) - connRad * num2);
						segs[num, 0] += vector;
						segs[num, 2] += vector;
					}
				}
				else
				{
					if (num > 0)
					{
						if (!Custom.DistLess(segs[num, 0], segs[num - 1, 0], connRad * num2))
						{
							Vector2 vector2 = Custom.DirVec(segs[num, 0], segs[num - 1, 0]) * (Vector2.Distance(segs[num, 0], segs[num - 1, 0]) - connRad * num2);
							segs[num, 0] += vector2 * 0.75f;
							segs[num, 2] += vector2 * 0.75f;
							segs[num - 1, 0] -= vector2 * 0.75f;
							segs[num - 1, 2] -= vector2 * 0.75f;
						}
						else
						{
							Vector2 vector3 = Custom.DirVec(segs[num - 1, 0], segs[num, 0]) * (Vector2.Distance(segs[num - 1, 0], segs[num, 0]) - connRad * num2);
							segs[num - 1, 0] += vector3 * 0.75f;
							segs[num - 1, 2] += vector3 * 0.75f;
							segs[num, 0] -= vector3 * 0.75f;
							segs[num, 2] -= vector3 * 0.75f;
						}
					}
					if (!isCosmetic)
					{
						Vector2 attachmentPosition = GetAttachmentPosition(0.5f);
						if (num == segs.GetLength(0) - 1 && fruit != null && !Custom.DistLess(segs[num, 0], fruit.firstChunk.pos + attachmentPosition, connRad * num2))
						{
							Vector2 vector4 = Custom.DirVec(segs[num, 0], attachmentPosition) * (Vector2.Distance(segs[num, 0], attachmentPosition) - connRad * num2);
							segs[num, 0] += vector4 * 0.875f;
							segs[num, 2] += vector4 * 0.875f;
							if (!fruit.disconnected)
							{
								fruit.firstChunk.vel -= vector4 * 0.25f;
							}
						}
					}
					else if (num == segs.GetLength(0) - 1 && (this as CosmeticStalk).wallStickDirection2.HasValue && !Custom.DistLess(segs[num, 0], (this as CosmeticStalk).stuckPos2, connRad * num2))
					{
						Vector2 vector5 = Custom.DirVec(segs[num, 0], (this as CosmeticStalk).stuckPos2) * (Vector2.Distance(segs[num, 0], (this as CosmeticStalk).stuckPos2) - connRad * num2);
						segs[num, 0] += vector5;
						segs[num, 2] += vector5;
					}
				}
				num += (dir ? 1 : (-1));
				if (dir && num >= segs.GetLength(0))
				{
					flag = true;
				}
				else if (!dir && num < 0)
				{
					flag = true;
				}
			}
		}

		public int ProtrusionSprite(int index)
		{
			return 2 + index;
		}

		public int ProtoSprite(int index, int shade)
		{
			return 2 + protrusions.GetLength(0) + index * 3 + shade;
		}

		public int SubStalkSprite(int index)
		{
			return 2 + protrusions.GetLength(0) + protoPomegranates.Length * 3 + index;
		}

		public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			if (ropeLength != 0f)
			{
				sLeaser.sprites = new FSprite[2 + protrusions.GetLength(0) + protoPomegranates.Length * 3 + subStalks.Count];
				sLeaser.sprites[0] = TriangleMesh.MakeLongMesh(segs.GetLength(0), pointyTip: false, customColor: true);
				sLeaser.sprites[1] = TriangleMesh.MakeLongMesh(segs.GetLength(0), pointyTip: false, customColor: true);
				for (int i = 0; i < protrusions.GetLength(0); i++)
				{
					FSprite fSprite = new FSprite("DangleFruit0A");
					fSprite.scaleX = 0.1f + 0.4f * protrusions[i, 1];
					fSprite.scaleY = 0.5f * protrusions[i, 1];
					fSprite.anchorY = 0f;
					sLeaser.sprites[ProtrusionSprite(i)] = fSprite;
				}
				for (int j = 0; j < protoPomegranates.Length; j++)
				{
					FSprite fSprite2 = new FSprite("PomegranateA");
					fSprite2.scaleX = 0.225f * Mathf.Pow(protoPomegranates[j] * 2f, 1.5f);
					fSprite2.scaleY = 0.6f * protoPomegranates[j];
					sLeaser.sprites[ProtoSprite(j, 0)] = fSprite2;
					fSprite2 = new FSprite("PomegranateC");
					fSprite2.scaleX = sLeaser.sprites[ProtoSprite(j, 0)].scaleX;
					fSprite2.scaleY = sLeaser.sprites[ProtoSprite(j, 0)].scaleY;
					sLeaser.sprites[ProtoSprite(j, 1)] = fSprite2;
					fSprite2 = new FSprite("PomegranateB");
					fSprite2.scaleX = sLeaser.sprites[ProtoSprite(j, 0)].scaleX;
					fSprite2.scaleY = sLeaser.sprites[ProtoSprite(j, 0)].scaleY;
					sLeaser.sprites[ProtoSprite(j, 2)] = fSprite2;
				}
				for (int k = 0; k < subStalks.Count; k++)
				{
					subStalks[k].InitiateSprite(sLeaser, rCam);
				}
				AddToContainer(sLeaser, rCam, null);
			}
		}

		public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			if (ropeLength == 0f)
			{
				return;
			}
			Vector2 vector = stuckPos;
			if (fruit != null && fruit.disconnected)
			{
				vector = Vector2.Lerp(segs[0, 1], segs[0, 0], timeStacker);
			}
			float num = 4f;
			bool flag = false;
			for (int i = 0; i < segs.GetLength(0); i++)
			{
				float num2 = (float)i / (float)(segs.GetLength(0) - 1);
				float num3 = Mathf.Lerp(4f, 2f, (num2 == 1f) ? 0f : Mathf.Pow(Mathf.Sin(num2 * (float)Math.PI), 0.125f));
				if (num2 == 1f)
				{
					num3 = (isCosmetic ? 3f : 6f);
				}
				Vector2 vector2 = Vector2.Lerp(segs[i, 1], segs[i, 0], timeStacker);
				if (i == segs.GetLength(0) - 1)
				{
					if (isCosmetic)
					{
						if ((this as CosmeticStalk).wallStickDirection2.HasValue)
						{
							vector2 = (this as CosmeticStalk).stuckPos2;
						}
					}
					else
					{
						vector2 = GetAttachmentPosition(timeStacker);
					}
				}
				Vector2 vector3;
				Vector2 vector4 = (vector3 = Custom.PerpendicularVector((vector - vector2).normalized));
				if (i == 0 && (isCosmetic || !fruit.disconnected))
				{
					vector3 = wallStickDirection;
				}
				if (i == segs.GetLength(0) - 1)
				{
					if (isCosmetic)
					{
						if ((this as CosmeticStalk).wallStickDirection2.HasValue)
						{
							vector4 = (this as CosmeticStalk).wallStickDirection2.Value;
						}
					}
					else if (fruit != null)
					{
						if (fruit.smashed)
						{
							Vector2 p = Vector2.Lerp(fruit.firstChunk.lastPos, fruit.firstChunk.pos, timeStacker);
							int num4 = ((attachIndex == 1) ? 1 : 0);
							if (fruit.shellsInverted)
							{
								num4 = ((num4 != 1) ? 1 : 0);
							}
							vector4 = Custom.PerpendicularVector(Custom.DegToVec(Custom.AimFromOneVectorToAnother(Vector2.Lerp(fruit.smashedBits[num4, 0], fruit.smashedBits[num4, 1], timeStacker), p) - 180f));
						}
						else
						{
							vector4 = fruit.rotation * attachIndex;
						}
					}
				}
				vector2 = new Vector2(Mathf.Floor(vector2.x) + 0.5f, Mathf.Floor(vector2.y) + 0.5f);
				TriangleMesh obj = sLeaser.sprites[0] as TriangleMesh;
				obj.MoveVertice(i * 4, vector - vector3 * num - camPos);
				obj.MoveVertice(i * 4 + 1, vector + vector3 * num - camPos);
				obj.MoveVertice(i * 4 + 2, vector2 - vector4 * num3 - camPos);
				obj.MoveVertice(i * 4 + 3, vector2 + vector4 * num3 - camPos);
				TriangleMesh obj2 = sLeaser.sprites[1] as TriangleMesh;
				obj2.MoveVertice(i * 4, vector - vector3 / 2f - camPos);
				obj2.MoveVertice(i * 4 + 1, vector + vector3 / 2f - camPos);
				obj2.MoveVertice(i * 4 + 2, vector2 - vector4 / 2f - camPos);
				obj2.MoveVertice(i * 4 + 3, vector2 + vector4 / 2f - camPos);
				darkness[i, 1] = darkness[i, 0];
				darkness[i, 0] = rCam.room.Darkness(vector2) * (1f - rCam.room.LightSourceExposure(vector2));
				if (darkness[i, 0] != darkness[i, 1])
				{
					flag = true;
				}
				vector = vector2;
				num = num3;
			}
			if (flag)
			{
				ApplyPalette(sLeaser, rCam, rCam.currentPalette);
			}
			vector = Vector2.Lerp(segs[0, 1], segs[0, 0], timeStacker);
			for (int j = 0; j < protrusions.GetLength(0); j++)
			{
				FSprite obj3 = sLeaser.sprites[ProtrusionSprite(j)];
				Vector2 vector5 = Vector2.Lerp(segs[j * 2 + 1, 1], segs[j * 2 + 1, 0], timeStacker);
				float ang = (obj3.rotation = Custom.VecToDeg((vector5 - vector).normalized) + protrusions[j, 0]);
				Vector2 vector6 = vector5 + Custom.DegToVec(ang);
				obj3.x = vector6.x - camPos.x;
				obj3.y = vector6.y - camPos.y;
				vector = vector5;
			}
			for (int k = 0; k < protoPomegranates.GetLength(0); k++)
			{
				FSprite fSprite = sLeaser.sprites[ProtoSprite(k, 0)];
				Vector2 vector7 = Vector2.Lerp(segs[k * 2 + 1, 1], segs[k * 2 + 1, 0], timeStacker);
				float rotation = Custom.AimFromOneVectorToAnother(Vector2.Lerp(segs[k * 2, 1], segs[k * 2, 0], timeStacker), Vector2.Lerp(segs[(k + 1) * 2, 1], segs[(k + 1) * 2, 0], timeStacker)) - 90f;
				fSprite.rotation = rotation;
				fSprite.x = vector7.x - camPos.x;
				fSprite.y = vector7.y - camPos.y;
				FSprite obj4 = sLeaser.sprites[ProtoSprite(k, 1)];
				obj4.rotation = fSprite.rotation;
				obj4.x = fSprite.x;
				obj4.y = fSprite.y;
				FSprite obj5 = sLeaser.sprites[ProtoSprite(k, 2)];
				obj5.rotation = fSprite.rotation;
				obj5.x = fSprite.x;
				obj5.y = fSprite.y;
			}
			for (int l = 0; l < subStalks.Count; l++)
			{
				subStalks[l].DrawSprite(sLeaser, rCam, timeStacker, camPos);
			}
			if (base.slatedForDeletetion || room != rCam.room)
			{
				sLeaser.CleanSpritesAndRemove();
			}
		}

		public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
		{
			if (ropeLength != 0f)
			{
				TriangleMesh triangleMesh = sLeaser.sprites[0] as TriangleMesh;
				for (int i = 0; i < triangleMesh.verticeColors.Length; i++)
				{
					triangleMesh.verticeColors[i] = GetColorAtLength(i, triangleMesh.verticeColors.Length, 20, isShade: false, palette, triangleMesh.vertices[i], rCam);
				}
				triangleMesh = sLeaser.sprites[1] as TriangleMesh;
				for (int j = 0; j < triangleMesh.verticeColors.Length; j++)
				{
					triangleMesh.verticeColors[j] = GetColorAtLength(j, triangleMesh.verticeColors.Length, 20, isShade: true, palette, triangleMesh.vertices[j], rCam);
				}
				for (int k = 0; k < protrusions.GetLength(0); k++)
				{
					sLeaser.sprites[ProtrusionSprite(k)].color = GetColorAtLength((k + 1) * 2, segs.GetLength(0), 5, isShade: false, palette, Vector2.Lerp(segs[(k + 1) * 2, 0], segs[(k + 1) * 2, 1], 0.5f), rCam);
				}
				for (int l = 0; l < protoPomegranates.Length; l++)
				{
					sLeaser.sprites[ProtoSprite(l, 0)].color = GetColorAtLength(l * 2 + 4, segs.GetLength(0), 5, isShade: false, palette, Vector2.Lerp(segs[l * 2 + 4, 0], segs[l * 2 + 4, 1], 0.5f), rCam);
					sLeaser.sprites[ProtoSprite(l, 2)].color = GetColorAtLength(l * 2 + 2, segs.GetLength(0), 5, isShade: true, palette, Vector2.Lerp(segs[l * 2 + 2, 0], segs[l * 2 + 2, 1], 0.5f), rCam);
					sLeaser.sprites[ProtoSprite(l, 1)].color = GetColorAtLength(l * 2, segs.GetLength(0), 5, isShade: false, palette, Vector2.Lerp(segs[l * 2, 0], segs[l * 2, 1], 0.5f), rCam);
				}
				for (int m = 0; m < subStalks.Count; m++)
				{
					subStalks[m].ApplyPalette(sLeaser, rCam, palette);
				}
			}
		}

		public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
		{
			if (ropeLength != 0f)
			{
				if (newContatiner == null)
				{
					newContatiner = rCam.ReturnFContainer("Background");
				}
				for (int i = 0; i < sLeaser.sprites.Length; i++)
				{
					sLeaser.sprites[i].RemoveFromContainer();
				}
				for (int j = 0; j < subStalks.Count; j++)
				{
					newContatiner.AddChild(sLeaser.sprites[subStalks[j].spriteIndex]);
				}
				for (int k = 0; k < protrusions.GetLength(0); k++)
				{
					newContatiner.AddChild(sLeaser.sprites[ProtrusionSprite(k)]);
				}
				newContatiner.AddChild(sLeaser.sprites[0]);
				newContatiner.AddChild(sLeaser.sprites[1]);
				for (int l = 0; l < protoPomegranates.GetLength(0); l++)
				{
					newContatiner.AddChild(sLeaser.sprites[ProtoSprite(l, 0)]);
					newContatiner.AddChild(sLeaser.sprites[ProtoSprite(l, 2)]);
					newContatiner.AddChild(sLeaser.sprites[ProtoSprite(l, 1)]);
				}
			}
		}
	}

	public class CosmeticStalk : Stalk
	{
		public Vector2 stuckPos2;

		public Vector2? wallStickDirection2;

		public PlacedObject placedObject;

		public CosmeticStalk(PlacedObject placedObject, Room room)
			: base(null, room, placedObject.pos, -1)
		{
			this.placedObject = placedObject;
			shellColor = new Color(0.18f, 0.55f, 0.11f);
			Initialize(room);
		}

		public new void Initialize(Room placeRoom)
		{
			UnityEngine.Random.State state = UnityEngine.Random.state;
			UnityEngine.Random.InitState((int)(((float)placeRoom.abstractRoom.index + placedObject.pos.x) * ((float)placeRoom.abstractRoom.index + placedObject.pos.y)));
			stuckPos = placedObject.pos;
			stuckPos2 = placedObject.pos + (placedObject.data as PlacedObject.ResizableObjectData).handlePos;
			generateDirection = Custom.DirVec(stuckPos, stuckPos2);
			IntVector2 tilePosition = placeRoom.GetTilePosition(stuckPos + new Vector2(10f, 0f));
			IntVector2 tilePosition2 = placeRoom.GetTilePosition(stuckPos + new Vector2(0f, 10f));
			IntVector2 tilePosition3 = placeRoom.GetTilePosition(stuckPos + new Vector2(0f, -10f));
			IntVector2 tilePosition4 = placeRoom.GetTilePosition(stuckPos + new Vector2(-10f, 0f));
			Room.Tile tile = placeRoom.GetTile(tilePosition.x, tilePosition.y);
			Room.Tile tile2 = placeRoom.GetTile(tilePosition2.x, tilePosition2.y);
			Room.Tile tile3 = placeRoom.GetTile(tilePosition3.x, tilePosition3.y);
			Room.Tile tile4 = placeRoom.GetTile(tilePosition4.x, tilePosition4.y);
			if (tile.Solid && !tile4.Solid)
			{
				wallStickDirection = new Vector2(0f, 1f);
				stuckPos.x = placeRoom.MiddleOfTile(tilePosition).x - 10f;
			}
			else if (tile2.Solid && !tile3.Solid)
			{
				wallStickDirection = new Vector2(-1f, 0f);
				stuckPos.y = placeRoom.MiddleOfTile(tilePosition2).y - 10f;
			}
			else if (tile3.Solid && !tile2.Solid)
			{
				wallStickDirection = new Vector2(1f, 0f);
				stuckPos.y = placeRoom.MiddleOfTile(tilePosition3).y + 10f;
			}
			else if (tile4.Solid && !tile.Solid)
			{
				wallStickDirection = new Vector2(0f, -1f);
				stuckPos.x = placeRoom.MiddleOfTile(tilePosition4).x + 10f;
			}
			tilePosition = placeRoom.GetTilePosition(stuckPos2 + new Vector2(10f, 0f));
			tilePosition2 = placeRoom.GetTilePosition(stuckPos2 + new Vector2(0f, 10f));
			tilePosition3 = placeRoom.GetTilePosition(stuckPos2 + new Vector2(0f, -10f));
			tilePosition4 = placeRoom.GetTilePosition(stuckPos2 + new Vector2(-10f, 0f));
			tile = placeRoom.GetTile(tilePosition.x, tilePosition.y);
			tile2 = placeRoom.GetTile(tilePosition2.x, tilePosition2.y);
			tile3 = placeRoom.GetTile(tilePosition3.x, tilePosition3.y);
			tile4 = placeRoom.GetTile(tilePosition4.x, tilePosition4.y);
			if (tile.Solid && !tile4.Solid)
			{
				wallStickDirection2 = new Vector2(0f, -1f);
				stuckPos2.x = placeRoom.MiddleOfTile(tilePosition).x - 10f;
			}
			else if (tile2.Solid && !tile3.Solid)
			{
				wallStickDirection2 = new Vector2(1f, 0f);
				stuckPos2.y = placeRoom.MiddleOfTile(tilePosition2).y - 10f;
			}
			else if (tile3.Solid && !tile2.Solid)
			{
				wallStickDirection2 = new Vector2(-1f, 0f);
				stuckPos2.y = placeRoom.MiddleOfTile(tilePosition3).y + 10f;
			}
			else if (tile4.Solid && !tile.Solid)
			{
				wallStickDirection2 = new Vector2(0f, 1f);
				stuckPos2.x = placeRoom.MiddleOfTile(tilePosition4).x + 10f;
			}
			ropeLength = Mathf.Abs((stuckPos - stuckPos2).magnitude) * 0.75f - 10f;
			if (ropeLength == 0f)
			{
				return;
			}
			ropeLength = Mathf.Max(ropeLength, 30f);
			segs = new Vector2[Mathf.Max(6, (int)(ropeLength / 5f)), 3];
			darkness = new float[segs.GetLength(0), 2];
			for (int i = 0; i < segs.GetLength(0); i++)
			{
				float t = (float)i / (float)(segs.GetLength(0) - 1);
				segs[i, 0] = Vector2.Lerp(stuckPos, stuckPos2, t);
				segs[i, 1] = segs[i, 0];
			}
			connRad = ropeLength / Mathf.Pow(segs.GetLength(0), 1.1f);
			protrusions = new float[segs.GetLength(0) / 2 - 2, 2];
			for (int j = 0; j < protrusions.GetLength(0); j++)
			{
				protrusions[j, 0] = UnityEngine.Random.Range(-120, 120);
				protrusions[j, 1] = Mathf.Pow(UnityEngine.Random.value, 0.125f);
			}
			protoPomegranates = new float[segs.GetLength(0) / 2 - 2];
			int num = UnityEngine.Random.Range(1, segs.GetLength(0) / 12);
			for (int k = 0; k < num; k++)
			{
				int num2 = UnityEngine.Random.Range(0, protoPomegranates.Length);
				protoPomegranates[num2] = ((k <= num / 6 && wallStickDirection2.HasValue) ? 1.1f : UnityEngine.Random.Range(0.7f, 1f));
				if (UnityEngine.Random.value < 0.4f && num2 > 0 && num2 < protoPomegranates.Length - 1)
				{
					protoPomegranates[num2 + (int)Mathf.Sign(UnityEngine.Random.Range(-1f, 1f))] = protoPomegranates[num2] * UnityEngine.Random.Range(0.75f, 0.9f);
				}
			}
			subStalks = new List<SubStalk>();
			if (wallStickDirection2.HasValue)
			{
				int num3 = 1;
				int num4 = 0;
				for (int l = 0; l < num3; l++)
				{
					int num5 = 0;
					while (num5 < 40)
					{
						num5++;
						int num6 = UnityEngine.Random.Range(0, 2);
						int stuckIndex = UnityEngine.Random.Range(Mathf.Min(2, segs.GetLength(0) / 8), Mathf.Max(segs.GetLength(0) / 2, Mathf.Min(segs.GetLength(0), 4)));
						Vector2 vector = Vector2.Lerp(stuckPos, stuckPos2, (num6 == 0) ? UnityEngine.Random.Range(0f, 0.25f) : (1f - UnityEngine.Random.Range(0f, 0.25f)));
						Vector2 normalized = (Custom.RNV() + generateDirection * (2 * ((num6 == 0) ? 1 : (-1)))).normalized;
						Vector2 vector2 = vector;
						Vector2 vector3 = new Vector2(0f, 0f);
						IntVector2 tilePosition5 = placeRoom.GetTilePosition(vector2);
						while (!placeRoom.GetTile(tilePosition5.x, tilePosition5.y).Solid && vector2.x > 0f && vector2.x < placeRoom.PixelWidth && vector2.y > 0f && vector2.y < placeRoom.PixelHeight)
						{
							vector2 += normalized * 5f;
							tilePosition5 = placeRoom.GetTilePosition(vector2);
						}
						tilePosition = placeRoom.GetTilePosition(vector2 + new Vector2(10f, 0f));
						tilePosition2 = placeRoom.GetTilePosition(vector2 + new Vector2(0f, 10f));
						tilePosition3 = placeRoom.GetTilePosition(vector2 + new Vector2(0f, -10f));
						tilePosition4 = placeRoom.GetTilePosition(vector2 + new Vector2(-10f, 0f));
						if (placeRoom.GetTile(tilePosition.x, tilePosition.y).Solid && !placeRoom.GetTile(tilePosition4.x, tilePosition4.y).Solid)
						{
							vector3 = new Vector2(1f, 0f);
						}
						else if (placeRoom.GetTile(tilePosition2.x, tilePosition2.y).Solid && !placeRoom.GetTile(tilePosition3.x, tilePosition3.y).Solid)
						{
							vector3 = new Vector2(0f, 1f);
						}
						else if (placeRoom.GetTile(tilePosition3.x, tilePosition3.y).Solid && !placeRoom.GetTile(tilePosition2.x, tilePosition2.y).Solid)
						{
							vector3 = new Vector2(0f, -1f);
						}
						else if (placeRoom.GetTile(tilePosition4.x, tilePosition4.y).Solid && !placeRoom.GetTile(tilePosition.x, tilePosition.y).Solid)
						{
							vector3 = new Vector2(-1f, 0f);
						}
						float num7 = Mathf.Abs((vector2 - vector).magnitude) * 0.75f - 10f;
						if (num7 < Mathf.Max(ropeLength / 2f, 20f) && num7 > 10f && Mathf.Abs((vector2 - stuckPos).magnitude) > 15f && vector3 != new Vector2(0f, 0f))
						{
							SubStalk subStalk = new SubStalk(this, stuckIndex, vector3, UnityEngine.Random.Range(-10000, 10000), SubStalkSprite(num4));
							num4++;
							subStalk.stuckPos = vector2;
							subStalks.Add(subStalk);
							break;
						}
					}
				}
				for (int m = 0; m < subStalks.Count; m++)
				{
					subStalks[m].Initialize();
				}
			}
			UnityEngine.Random.state = state;
			initialized = true;
		}
	}

	public class SubStalk
	{
		public Stalk owner;

		public int ownerIndex;

		public Vector2 stuckPos;

		public Vector2 attachDirection;

		public float ropeLength;

		public int seed;

		public int spriteIndex;

		public Vector2[,] segs;

		public float[,] darkness;

		public bool initialized;

		public float connRad;

		public bool attachedToWall
		{
			get
			{
				if (!(owner is CosmeticStalk))
				{
					return !owner.fruit.disconnected;
				}
				return true;
			}
		}

		public SubStalk(Stalk owner, int stuckIndex, Vector2 attachDirection, int seed, int spriteIndex)
		{
			ropeLength = -1f;
			this.owner = owner;
			this.seed = seed;
			ownerIndex = stuckIndex;
			this.spriteIndex = spriteIndex;
			this.attachDirection = attachDirection;
		}

		public void Initialize()
		{
			UnityEngine.Random.State state = UnityEngine.Random.state;
			UnityEngine.Random.InitState(seed);
			Vector2 vector = owner.segs[ownerIndex, 0];
			if (attachedToWall)
			{
				ropeLength = Mathf.Abs((stuckPos - vector).magnitude) * 0.75f - 10f;
			}
			else
			{
				ropeLength = UnityEngine.Random.Range(25f, 50f);
			}
			segs = new Vector2[Math.Max(1, (int)(ropeLength / 5f)), 3];
			darkness = new float[segs.GetLength(0), 2];
			for (int i = 0; i < segs.GetLength(0); i++)
			{
				if (attachedToWall)
				{
					float t = (float)i / (float)(segs.GetLength(0) - 1);
					segs[i, 0] = Vector2.Lerp(stuckPos, owner.segs[ownerIndex, 0], t);
				}
				else
				{
					segs[i, 0] = owner.segs[ownerIndex, 0] + Custom.RNV() * UnityEngine.Random.Range(0f, 10f);
				}
				segs[i, 1] = segs[i, 0];
			}
			connRad = ropeLength / Mathf.Pow(segs.GetLength(0), 1.1f);
			UnityEngine.Random.state = state;
			initialized = true;
		}

		public void Update()
		{
			if (initialized && ropeLength != -1f)
			{
				ConnectSegments(dir: true);
				ConnectSegments(dir: false);
				for (int i = 0; i < segs.GetLength(0); i++)
				{
					segs[i, 1] = segs[i, 0];
					segs[i, 0] += segs[i, 2];
					segs[i, 2] *= (owner.room.PointSubmerged(segs[i, 0]) ? 0.9f : 0.98f);
					segs[i, 2].y -= 0.9f * owner.room.gravity;
				}
				ConnectSegments(dir: false);
				ConnectSegments(dir: true);
				for (int j = (attachedToWall ? 1 : 0); j < segs.GetLength(0); j++)
				{
					SharedPhysics.TerrainCollisionData cd = owner.scratchTerrainCollisionData.Set(segs[j, 0], segs[j, 1], segs[j, 2], 3f, new IntVector2(0, 0), goThroughFloors: true);
					cd = SharedPhysics.VerticalCollision(owner.room, cd);
					cd = SharedPhysics.HorizontalCollision(owner.room, cd);
					segs[j, 0] = cd.pos;
					segs[j, 2] = cd.vel;
				}
			}
		}

		public Vector2 GetAttachmentPosition(float timeStacker)
		{
			return Vector2.Lerp(owner.segs[ownerIndex, 1], owner.segs[ownerIndex, 0], timeStacker);
		}

		public Color GetColorAtLength(int index, int cap, int coloredPoints, RoomPalette palette, Vector2 position, RoomCamera rCam)
		{
			float t = rCam.room.Darkness(position) * (1f - rCam.room.LightSourceExposure(position));
			Color colorAtLength = owner.GetColorAtLength(index / 4, owner.segs.GetLength(0), 6, isShade: false, palette, position, rCam);
			return Color.Lerp(Color.Lerp(palette.blackColor, colorAtLength, Mathf.Lerp(0.05f, 1f, (float)(Mathf.Clamp(index, cap - coloredPoints, cap) - (cap - coloredPoints)) / ((float)coloredPoints - 1f))), palette.blackColor, t);
		}

		public void ConnectSegments(bool dir)
		{
			int num = ((!dir) ? (segs.GetLength(0) - 1) : 0);
			float num2 = (attachedToWall ? 1f : 1.75f);
			bool flag = false;
			while (!flag)
			{
				if (num == 0 && attachedToWall)
				{
					if (!Custom.DistLess(segs[num, 0], stuckPos, connRad * num2))
					{
						Vector2 vector = Custom.DirVec(segs[num, 0], stuckPos) * (Vector2.Distance(segs[num, 0], stuckPos) - connRad * num2);
						segs[num, 0] += vector;
						segs[num, 2] += vector;
					}
				}
				else
				{
					if (num > 0)
					{
						if (!Custom.DistLess(segs[num, 0], segs[num - 1, 0], connRad * num2))
						{
							Vector2 vector2 = Custom.DirVec(segs[num, 0], segs[num - 1, 0]) * (Vector2.Distance(segs[num, 0], segs[num - 1, 0]) - connRad * num2);
							segs[num, 0] += vector2 * 0.5f;
							segs[num, 2] += vector2 * 0.5f;
							segs[num - 1, 0] -= vector2 * 0.5f;
							segs[num - 1, 2] -= vector2 * 0.5f;
						}
						else
						{
							Vector2 vector3 = Custom.DirVec(segs[num - 1, 0], segs[num, 0]) * (Vector2.Distance(segs[num - 1, 0], segs[num, 0]) - connRad * num2);
							segs[num - 1, 0] += vector3 * 0.5f;
							segs[num - 1, 2] += vector3 * 0.5f;
							segs[num, 0] -= vector3 * 0.5f;
							segs[num, 2] -= vector3 * 0.5f;
						}
					}
					Vector2 attachmentPosition = GetAttachmentPosition(0.5f);
					if (num == segs.GetLength(0) - 1 && !Custom.DistLess(segs[num, 0], attachmentPosition, connRad * num2))
					{
						Vector2 vector4 = Custom.DirVec(segs[num, 0], attachmentPosition) * (Vector2.Distance(segs[num, 0], attachmentPosition) - connRad * num2);
						segs[num, 0] += vector4 * 0.75f;
						segs[num, 2] += vector4 * 0.75f;
						owner.segs[ownerIndex, 2] -= vector4 * 0.25f;
					}
				}
				num += (dir ? 1 : (-1));
				if (dir && num >= segs.GetLength(0))
				{
					flag = true;
				}
				else if (!dir && num < 0)
				{
					flag = true;
				}
			}
		}

		public void InitiateSprite(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites[spriteIndex] = TriangleMesh.MakeLongMesh(segs.GetLength(0), pointyTip: false, customColor: true);
		}

		public void DrawSprite(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			Vector2 vector = stuckPos;
			if (!attachedToWall)
			{
				vector = Vector2.Lerp(segs[0, 1], segs[0, 0], timeStacker);
			}
			float num = 2.5f;
			bool flag = false;
			for (int i = 0; i < segs.GetLength(0); i++)
			{
				float num2 = (float)i / (float)(segs.GetLength(0) - 1);
				float num3 = Mathf.Lerp(3f, 2f, (num2 == 1f) ? 0f : Mathf.Pow(Mathf.Sin(num2 * (float)Math.PI), 0.5f));
				if (num2 == 1f)
				{
					num3 = 3f;
				}
				Vector2 vector2 = Vector2.Lerp(segs[i, 1], segs[i, 0], timeStacker);
				if (i == segs.GetLength(0) - 1)
				{
					vector2 = GetAttachmentPosition(timeStacker);
				}
				Vector2 vector3;
				Vector2 vector4 = (vector3 = Custom.PerpendicularVector((vector - vector2).normalized));
				if (i == 0 && attachedToWall)
				{
					vector3 = Custom.PerpendicularVector(attachDirection);
				}
				vector2 = new Vector2(Mathf.Floor(vector2.x) + 0.5f, Mathf.Floor(vector2.y) + 0.5f);
				TriangleMesh obj = sLeaser.sprites[spriteIndex] as TriangleMesh;
				obj.MoveVertice(i * 4, vector - vector3 * num - camPos);
				obj.MoveVertice(i * 4 + 1, vector + vector3 * num - camPos);
				obj.MoveVertice(i * 4 + 2, vector2 - vector4 * num3 - camPos);
				obj.MoveVertice(i * 4 + 3, vector2 + vector4 * num3 - camPos);
				darkness[i, 1] = darkness[i, 0];
				darkness[i, 0] = rCam.room.Darkness(vector2) * (1f - rCam.room.LightSourceExposure(vector2));
				if (darkness[i, 0] != darkness[i, 1])
				{
					flag = true;
				}
				vector = vector2;
				num = num3;
			}
			if (flag)
			{
				ApplyPalette(sLeaser, rCam, rCam.currentPalette);
			}
		}

		public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
		{
			TriangleMesh triangleMesh = sLeaser.sprites[spriteIndex] as TriangleMesh;
			for (int i = 0; i < triangleMesh.verticeColors.Length; i++)
			{
				triangleMesh.verticeColors[i] = GetColorAtLength(i, triangleMesh.verticeColors.Length, 5, palette, triangleMesh.vertices[i], rCam);
			}
		}
	}

	public class PomegranateSeed : CosmeticSprite
	{
		public Color color;

		public float darkness;

		public float lastDarkness;

		public float size;

		public int lifeTime;

		public PomegranateSeed(Vector2 pos, Vector2 vel, Color color)
		{
			base.pos = pos;
			lastPos = pos;
			base.vel = vel;
			this.color = color;
			size = UnityEngine.Random.Range(0.9f, 1.25f);
			lifeTime = UnityEngine.Random.Range(300, 600);
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			vel.y -= 0.9f * room.gravity;
			vel *= (room.PointSubmerged(pos) ? 0.9f : 0.98f);
			lifeTime--;
			if (room.GetTile(lastPos).Solid || lifeTime <= 0)
			{
				Destroy();
			}
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[3];
			ISSeed(sLeaser, rCam, 0, size, color);
			AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("Midground"));
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			Vector2 p = Vector2.Lerp(lastPos, pos, timeStacker);
			float value = Mathf.Clamp01(Mathf.InverseLerp(0f, 120f, lifeTime));
			float num = 1f - Mathf.Clamp01(Mathf.InverseLerp(0.5f, 0f, value));
			float t = Mathf.Clamp01(Mathf.InverseLerp(1f, 0f, value));
			lastDarkness = darkness;
			darkness = Mathf.Lerp(rCam.room.Darkness(p) * (1f - rCam.room.LightSourceExposure(p)), 1f, t);
			if (darkness != lastDarkness)
			{
				ApplyPalette(sLeaser, rCam, rCam.currentPalette);
			}
			DSSeed(sLeaser, rCam, timeStacker, camPos, 0, size * num, color, p, visible: true);
			base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		}

		public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
		{
			APSeed(sLeaser, rCam, rCam.currentPalette, 0, size, color, darkness);
		}

		public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
		{
			base.AddToContainer(sLeaser, rCam, newContatiner);
		}
	}

	public class AbstractPomegranate : AbstractConsumable
	{
		public bool smashed;

		public bool disconnected;

		public bool spearmasterStabbed;

		public AbstractPomegranate(World world, PhysicalObject realizedObject, WorldCoordinate pos, EntityID ID, int originRoom, int consumableIndex, PlacedObject.ConsumableObjectData consumableData, bool smashed, bool disconnected, bool spearmasterStabbed)
			: base(world, AbstractObjectType.Pomegranate, realizedObject, pos, ID, originRoom, consumableIndex, consumableData)
		{
			this.smashed = smashed;
			this.disconnected = disconnected;
			this.spearmasterStabbed = spearmasterStabbed;
		}

		public override string ToString()
		{
			string baseString = string.Format(CultureInfo.InvariantCulture, "{0}<oA>{1}<oA>{2}<oA>{3}<oA>{4}<oA>{5}<oA>{6}", ID.ToString(), type.ToString(), pos.SaveToString(), originRoom, placedObjectIndex, smashed ? "1" : "0", disconnected ? "1" : "0", spearmasterStabbed ? "1" : "0");
			baseString = SaveState.SetCustomData(this, baseString);
			return SaveUtils.AppendUnrecognizedStringAttrs(baseString, "<oA>", unrecognizedAttributes);
		}
	}

	public Stalk[] stalks;

	public int[] stalkSeeds;

	public bool disconnected;

	public int releaseCounter;

	public bool smashed;

	public int eatDelayCounter;

	public int notMovingTimer;

	public float attachDirection;

	public Vector2 idleRotationOffset;

	public Player killTagHolder;

	public SharedPhysics.TerrainCollisionData scratchTerrainCollisionData;

	public PlacedObject.PomegranateData data;

	public Color baseColor = new Color(0.27f, 0.71f, 0.19f);

	public Color shellColor = new Color(0.18f, 0.55f, 0.11f);

	public Vector2 rotation;

	public Vector2 lastRotation;

	public Vector2? setRotation;

	public bool refreshSprites;

	public Vector2[,] smashedBits;

	public float[,] bitSizes;

	public float[] shellHalves;

	public bool shellsInverted;

	public Vector2[] seeds;

	public float[] seedSizes;

	public float overallSize;

	public float stabbedFade;

	public float darkness;

	public float lastDarkness;

	public bool spearmasterStabbed
	{
		get
		{
			if (ModManager.MSC)
			{
				return AbstrPomegranate.spearmasterStabbed;
			}
			return false;
		}
		set
		{
			AbstrPomegranate.spearmasterStabbed = value;
		}
	}

	public bool currentlyEdible
	{
		get
		{
			if (smashed)
			{
				return eatDelayCounter <= 0;
			}
			return false;
		}
	}

	public bool falling
	{
		get
		{
			if (disconnected || releaseCounter > 0)
			{
				return !smashed;
			}
			return false;
		}
	}

	public AbstractPomegranate AbstrPomegranate => abstractPhysicalObject as AbstractPomegranate;

	public Pomegranate(AbstractPhysicalObject abstractPhysicalObject)
		: base(abstractPhysicalObject)
	{
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.InitState(base.abstractPhysicalObject.ID.RandomSeed);
		stalkSeeds = new int[2];
		stalkSeeds[0] = UnityEngine.Random.Range(-100000, 100000);
		stalkSeeds[1] = UnityEngine.Random.Range(-100000, 100000);
		overallSize = UnityEngine.Random.Range(0.9f, 1.1f);
		UnityEngine.Random.state = state;
		base.bodyChunks = new BodyChunk[1];
		base.bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0f, 0f), 20f * overallSize, 2f);
		bodyChunkConnections = new BodyChunkConnection[0];
		rotation = new Vector2(0f, 1f);
		idleRotationOffset = Custom.RNV();
		base.airFriction = 0.999f;
		base.gravity = 1f;
		bounce = 0f;
		surfaceFriction = 0.7f;
		collisionLayer = 1;
		base.waterFriction = 0.9f;
		base.buoyancy = 0.25f;
		base.GoThroughFloors = true;
		if (AbstrPomegranate.disconnected)
		{
			disconnected = true;
			rotation = Custom.RNV();
		}
		if (AbstrPomegranate.smashed)
		{
			EnterSmashedMode();
		}
		if (spearmasterStabbed)
		{
			stabbedFade = 1f;
		}
		lastRotation = rotation;
	}

	public void Disconnect()
	{
		if (!disconnected)
		{
			AbstrPomegranate.Consume();
			AbstrPomegranate.disconnected = true;
			disconnected = true;
			room.PlaySound(SoundID.Spear_Stick_In_Creature, base.firstChunk.pos, 0.5f, 2f);
		}
	}

	public override void Grabbed(Creature.Grasp grasp)
	{
		base.Grabbed(grasp);
		if (grasp.grabber.firstChunk.vel.magnitude > 15f && !disconnected)
		{
			if (grasp.grabber is Player)
			{
				killTagHolder = grasp.grabber as Player;
			}
			releaseCounter = UnityEngine.Random.Range(10, 20);
			base.firstChunk.vel += grasp.grabber.firstChunk.vel / 2f;
			if (room != null)
			{
				room.PlaySound(SoundID.King_Vulture_Tusk_Out_Of_Terrain, base.firstChunk.pos, 0.35f, 2f);
			}
		}
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (smashed)
		{
			SmashedUpdate(eu);
		}
		if (grabbedBy.Count > 0 && notMovingTimer < 40)
		{
			base.firstChunk.collideWithObjects = true;
		}
		if (falling)
		{
			if (base.firstChunk.vel.magnitude < 5f || notMovingTimer > 40)
			{
				notMovingTimer++;
				if (notMovingTimer > 40)
				{
					base.firstChunk.collideWithObjects = false;
				}
				if (base.firstChunk.vel.magnitude > 15f)
				{
					base.firstChunk.collideWithObjects = true;
					notMovingTimer = 0;
				}
			}
			else
			{
				notMovingTimer = 0;
			}
		}
		if (releaseCounter > 0)
		{
			releaseCounter--;
		}
		if (eatDelayCounter > 0)
		{
			eatDelayCounter--;
		}
		if (room.game.devToolsActive && Input.GetKey("b"))
		{
			base.firstChunk.vel += Custom.DirVec(base.firstChunk.pos, (Vector2)Futile.mousePosition) * 3f;
		}
		if ((falling && base.firstChunk.vel.magnitude > 5f) || smashed)
		{
			Forbid();
		}
		if (!disconnected)
		{
			bool flag = false;
			for (int i = 0; i < stalks.Length; i++)
			{
				if (stalks[i].ropeLength == 0f || !Custom.DistLess(base.firstChunk.pos, stalks[i].stuckPos, stalks[i].ropeLength * ((releaseCounter > 0) ? 1.75f : 3f) + 10f))
				{
					flag = true;
					break;
				}
			}
			if (flag || base.slatedForDeletetion || releaseCounter == 1)
			{
				Disconnect();
			}
		}
		lastRotation = rotation;
		if (setRotation.HasValue)
		{
			rotation = setRotation.Value;
			setRotation = null;
		}
		if (base.firstChunk.ContactPoint.y < 0)
		{
			rotation = (rotation - Custom.PerpendicularVector(rotation) * (0.1f * base.firstChunk.vel.x)).normalized;
			base.firstChunk.vel.x *= 0.8f;
		}
		else if (!disconnected && stalks[0].ropeLength > 0f && stalks[1].ropeLength > 0f)
		{
			float num = (stalks[0].ropeLength - stalks[1].ropeLength) / (stalks[0].ropeLength + stalks[1].ropeLength);
			if (grabbedBy.Count > 0)
			{
				rotation = Vector2.Lerp(rotation, Custom.PerpendicularVector(Custom.DirVec(stalks[0].segs[0, 0], stalks[1].segs[0, 0])) + new Vector2(Mathf.Clamp((grabbedBy[0].grabber.mainBodyChunk.pos.x - base.firstChunk.pos.x) * 0.05f * Mathf.Sign(rotation.y) * Mathf.Sign(grabbedBy[0].grabber.mainBodyChunk.pos.y - base.firstChunk.pos.y), -1f, 1f), 0f), 0.4f).normalized;
			}
			else
			{
				rotation = Vector2.Lerp(rotation, Vector2.Lerp(Custom.PerpendicularVector(Custom.DirVec(stalks[0].segs[0, 0], stalks[1].segs[0, 0])), idleRotationOffset - new Vector2(num * 7.5f, 0f), 0.05f), 0.2f).normalized;
			}
		}
		else if (grabbedBy.Count > 0)
		{
			rotation = Vector2.Lerp(rotation, rotation + Custom.DirVec(grabbedBy[0].grabber.mainBodyChunk.pos, base.firstChunk.pos), 0.4f).normalized;
			if (disconnected)
			{
				rotation.y = Mathf.Abs(rotation.y);
			}
		}
		if (smashed && ModManager.MSC && !spearmasterStabbed)
		{
			for (int j = 0; j < room.physicalObjects[0].Count; j++)
			{
				if (!(room.physicalObjects[0][j] is Spear) || !(room.physicalObjects[0][j] as Spear).Spear_NeedleCanFeed() || !((room.physicalObjects[0][j] as Spear).mode == Weapon.Mode.Thrown))
				{
					continue;
				}
				Spear spear = room.physicalObjects[0][j] as Spear;
				for (int k = 0; k < base.bodyChunks.Length; k++)
				{
					BodyChunk bodyChunk = base.bodyChunks[k];
					float num2 = Custom.CirclesCollisionTime(spear.firstChunk.lastPos.x, spear.firstChunk.lastPos.y, bodyChunk.pos.x, bodyChunk.pos.y, spear.firstChunk.pos.x - spear.firstChunk.lastPos.x, spear.firstChunk.pos.y - spear.firstChunk.lastPos.y, spear.firstChunk.rad + ((spear.thrownBy != null && spear.thrownBy is Player) ? 5f : 0f), bodyChunk.rad);
					if (num2 > 0f && num2 < 1f)
					{
						spear.meleeHitChunk = base.firstChunk;
					}
				}
			}
		}
		if (currentlyEdible)
		{
			for (int l = 0; l < (ModManager.MSC ? room.abstractRoom.creatures.Count : room.game.Players.Count); l++)
			{
				Player player;
				if (ModManager.MSC)
				{
					Creature realizedCreature = room.abstractRoom.creatures[l].realizedCreature;
					if (realizedCreature == null || !(realizedCreature is Player) || !realizedCreature.Consious)
					{
						continue;
					}
					player = realizedCreature as Player;
				}
				else
				{
					if (room.game.Players[l].realizedCreature == null || !room.game.Players[l].realizedCreature.Consious)
					{
						continue;
					}
					player = room.game.Players[l].realizedCreature as Player;
				}
				if (player.room != room || player.handOnExternalFoodSource.HasValue || player.eatExternalFoodSourceCounter >= 1 || player.dontEatExternalFoodSourceCounter >= 1 || player.FoodInStomach >= player.MaxFoodInStomach || (player.touchedNoInputCounter <= 5 && !player.input[0].pckp && (!ModManager.MSC || !(player.abstractCreature.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.SlugNPC))) || (ModManager.MSC && !(player.SlugCatClass != MoreSlugcatsEnums.SlugcatStatsName.Spear)) || player.FreeHand() <= -1)
				{
					continue;
				}
				Vector2 pos = player.mainBodyChunk.pos;
				if (Custom.DistLess(pos, base.firstChunk.pos, 35f))
				{
					player.handOnExternalFoodSource = base.firstChunk.pos + Custom.DirVec(pos, base.firstChunk.pos) * 5f;
					player.eatExternalFoodSourceCounter = 15;
					if (UnityEngine.Random.value < 0.5f)
					{
						room.AddObject(new PomegranateSeed(base.firstChunk.pos + Custom.RNV() * UnityEngine.Random.Range(0f, 10f), Custom.RNV() * 4f + new Vector2(0f, 3f), baseColor));
					}
					for (int m = 0; m < UnityEngine.Random.Range(1, 3); m++)
					{
						room.AddObject(new WaterDrip(base.firstChunk.pos + Custom.RNV() * UnityEngine.Random.Range(0f, 10f), Custom.RNV() * UnityEngine.Random.Range(0f, 2f), waterColor: false));
					}
					for (int n = 2; n < smashedBits.GetLength(0); n++)
					{
						smashedBits[n, 2] += new Vector2(UnityEngine.Random.Range(-2f, 2f), UnityEngine.Random.Range(-2f, 2f));
					}
					if (UnityEngine.Random.value < 0.5f)
					{
						room.PlaySound(SoundID.Slime_Mold_Terrain_Impact, base.firstChunk.pos, 0.5f, 1f);
					}
					if (room.game.IsStorySession && player.abstractCreature.creatureTemplate.type == CreatureTemplate.Type.Slugcat && room.game.GetStorySession.playerSessionRecords != null)
					{
						room.game.GetStorySession.playerSessionRecords[(player.abstractCreature.state as PlayerState).playerNumber].AddEat(this);
					}
					if (player.graphicsModule != null)
					{
						(player.graphicsModule as PlayerGraphics).LookAtPoint(base.firstChunk.pos, 100f);
					}
				}
			}
		}
		for (int num3 = grabbedBy.Count - 1; num3 >= 0; num3--)
		{
			if (ModManager.MSC && grabbedBy[num3].grabber is Player)
			{
				(grabbedBy[num3].grabber as Player).pyroJumpped = false;
			}
			if (grabbedBy[num3].grabber.enteringShortCut.HasValue)
			{
				grabbedBy[num3].Release();
			}
		}
	}

	public void SmashedUpdate(bool eu)
	{
		for (int i = 0; i < smashedBits.GetLength(0); i++)
		{
			smashedBits[i, 1] = smashedBits[i, 0];
			smashedBits[i, 0] += smashedBits[i, 2];
			smashedBits[i, 2] *= (room.PointSubmerged(smashedBits[i, 0]) ? 0.9f : 0.98f);
			smashedBits[i, 2].y -= 0.9f * room.gravity;
			if (i > 2)
			{
				smashedBits[i, 2] += new Vector2(Mathf.Lerp(-0.5f, 0.5f, ((float)i - 2f) / (float)(smashedBits.GetLength(0) - 2)), 0f);
			}
			float num = ((i < 2) ? 12.5f : (8f * bitSizes[i - 2, 3]));
			if (!Custom.DistLess(smashedBits[i, 0], base.firstChunk.pos, num))
			{
				Vector2 vector = Custom.DirVec(smashedBits[i, 0], base.firstChunk.pos) * (Vector2.Distance(smashedBits[i, 0], base.firstChunk.pos) - num);
				smashedBits[i, 0] += vector * 0.5f;
				smashedBits[i, 2] += vector * 0.5f;
			}
			else if (i < 2)
			{
				Vector2 vector2 = Custom.DirVec(base.firstChunk.pos, smashedBits[i, 0]) * (Vector2.Distance(base.firstChunk.pos, smashedBits[i, 0]) - num);
				smashedBits[i, 0] -= vector2 * 0.5f;
				smashedBits[i, 2] -= vector2 * 0.5f;
			}
		}
		smashedBits[0, 2] += new Vector2(-0.5f, 0f);
		if (smashedBits[0, 0].x > base.firstChunk.pos.x - 1f)
		{
			if (smashedBits[0, 2].x > 0f)
			{
				smashedBits[0, 2] *= new Vector2(-0.5f, 1f);
			}
			smashedBits[0, 0].x = base.firstChunk.pos.x - 1f;
		}
		smashedBits[1, 2] += new Vector2(0.5f, 0f);
		if (smashedBits[1, 0].x < base.firstChunk.pos.x + 1f)
		{
			if (smashedBits[1, 2].x < 0f)
			{
				smashedBits[1, 2] *= new Vector2(-0.5f, 1f);
			}
			smashedBits[1, 0].x = base.firstChunk.pos.x + 1f;
		}
		for (int j = 0; j < smashedBits.GetLength(0); j++)
		{
			SharedPhysics.TerrainCollisionData cd = scratchTerrainCollisionData.Set(smashedBits[j, 0], smashedBits[j, 1], smashedBits[j, 2], (j < 2) ? 15f : (6f * bitSizes[j - 2, 0]), new IntVector2(0, 0), goThroughFloors: true);
			cd = SharedPhysics.VerticalCollision(room, cd);
			cd = SharedPhysics.HorizontalCollision(room, cd);
			smashedBits[j, 0] = cd.pos;
			smashedBits[j, 2] = cd.vel;
		}
	}

	public override void PlaceInRoom(Room placeRoom)
	{
		base.PlaceInRoom(placeRoom);
		if (!AbstrPomegranate.isConsumed && AbstrPomegranate.placedObjectIndex >= 0 && AbstrPomegranate.placedObjectIndex < placeRoom.roomSettings.placedObjects.Count)
		{
			base.firstChunk.HardSetPosition(placeRoom.roomSettings.placedObjects[AbstrPomegranate.placedObjectIndex].pos);
			data = placeRoom.roomSettings.placedObjects[AbstrPomegranate.placedObjectIndex].data as PlacedObject.PomegranateData;
		}
		else if (AbstrPomegranate.placedObjectIndex == -1 && room.game.IsArenaSession && placeRoom.game.GetArenaGameSession.counter < 10)
		{
			base.firstChunk.HardSetPosition(placeRoom.MiddleOfTile(abstractPhysicalObject.pos.Tile));
		}
		else
		{
			base.firstChunk.HardSetPosition(placeRoom.MiddleOfTile(abstractPhysicalObject.pos));
			rotation = Custom.RNV();
			lastRotation = rotation;
			disconnected = true;
		}
		if (smashedBits != null)
		{
			for (int i = 0; i < smashedBits.GetLength(0); i++)
			{
				if (i < 2)
				{
					smashedBits[i, 0] = base.firstChunk.pos + new Vector2((i == 0) ? (-5) : 5, -5f);
				}
				else
				{
					smashedBits[i, 0] = (base.firstChunk.pos += Custom.RNV());
				}
				smashedBits[i, 1] = smashedBits[i, 0];
			}
		}
		stalks = new Stalk[2];
		stalks[0] = new Stalk(this, room, base.firstChunk.pos, -1);
		stalks[1] = new Stalk(this, room, base.firstChunk.pos, 1);
		stalks[0].Initialize(placeRoom);
		stalks[1].Initialize(placeRoom);
		room.AddObject(stalks[0]);
		room.AddObject(stalks[1]);
	}

	public override void TerrainImpact(int chunk, IntVector2 direction, float speed, bool firstContact)
	{
		base.TerrainImpact(chunk, direction, speed, firstContact);
		if (disconnected && !smashed && ((room.gravity > 0.5f && direction == new IntVector2(0, -1)) || base.firstChunk.vel.magnitude > 25f))
		{
			Smash();
		}
	}

	public void EnterSmashedMode()
	{
		smashed = true;
		AbstrPomegranate.smashed = true;
		AbstrPomegranate.disconnected = true;
		disconnected = true;
		refreshSprites = true;
		base.firstChunk.collideWithObjects = false;
		ChangeCollisionLayer(0);
		canBeHitByWeapons = false;
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.InitState(abstractPhysicalObject.ID.RandomSeed);
		smashedBits = new Vector2[2 + UnityEngine.Random.Range(6, 12), 3];
		bitSizes = new float[smashedBits.GetLength(0) - 2, 4];
		shellsInverted = rotation.x < 0f && rotation.y < 0f;
		shellHalves = new float[2];
		shellHalves[0] = UnityEngine.Random.Range(1f, 1.5f) * overallSize;
		shellHalves[1] = UnityEngine.Random.Range(1f, 1.5f) * overallSize;
		for (int i = 0; i < bitSizes.GetLength(0); i++)
		{
			bitSizes[i, 0] = UnityEngine.Random.Range(0.75f, 1.5f) * overallSize;
			bitSizes[i, 1] = UnityEngine.Random.Range(0f, 360f);
			bitSizes[i, 2] = UnityEngine.Random.Range(1, 6);
			bitSizes[i, 3] = UnityEngine.Random.Range(0.5f, 4f) * overallSize;
		}
		bitSizes[2, 3] = 0.25f;
		for (int j = 0; j < smashedBits.GetLength(0); j++)
		{
			if (j < 2)
			{
				smashedBits[j, 0] = base.firstChunk.pos + new Vector2((j == 0) ? (-5) : 5, -5f);
			}
			else
			{
				smashedBits[j, 0] = (base.firstChunk.pos += Custom.RNV());
			}
			smashedBits[j, 1] = smashedBits[j, 0];
		}
		seeds = new Vector2[18];
		seedSizes = new float[seeds.Length];
		for (int k = 0; k < seeds.Length; k++)
		{
			float num = (float)k / (float)seeds.Length;
			seeds[k] = Custom.DegToVec(num * 10f * 360f) * Mathf.Lerp(0f, 10f, num) * new Vector2(1f, 1.25f) + new Vector2(UnityEngine.Random.Range(-2f, 2f), UnityEngine.Random.Range(-2f, 2f)) + new Vector2(0f, -2f);
			seedSizes[k] = UnityEngine.Random.Range(0.75f, 1.25f);
		}
		UnityEngine.Random.state = state;
	}

	public void Smash()
	{
		room.PlaySound(SoundID.Spear_Stick_In_Creature, base.firstChunk.pos, 1f, 0.75f);
		room.ScreenMovement(base.firstChunk.pos, new Vector2(0f, -1f), 0.5f);
		EnterSmashedMode();
		eatDelayCounter = UnityEngine.Random.Range(45, 90);
		for (int i = 0; i < UnityEngine.Random.Range(3, 6); i++)
		{
			room.AddObject(new PomegranateSeed(base.firstChunk.pos + Custom.RNV() * UnityEngine.Random.Range(0f, 10f), Custom.RNV() * UnityEngine.Random.Range(2f, 6f) + new Vector2(0f, 3f), baseColor));
		}
		for (int j = 0; j < UnityEngine.Random.Range(4, 8); j++)
		{
			room.AddObject(new WaterDrip(base.firstChunk.pos + Custom.RNV() * UnityEngine.Random.Range(0f, 10f), Custom.RNV() * UnityEngine.Random.Range(2f, 6f), waterColor: false));
		}
		for (int k = 0; k < grabbedBy.Count; k++)
		{
			if (grabbedBy[k].grabber is Player)
			{
				Player player = grabbedBy[k].grabber as Player;
				for (int l = 0; l < player.bodyChunks.Length; l++)
				{
					Vector2 normalized = player.bodyChunks[l].vel.normalized;
					player.bodyChunks[l].vel = normalized * Mathf.Clamp(player.bodyChunks[l].vel.magnitude, 0f, 50f);
				}
			}
		}
		AllGraspsLetGoOfThisObject(evenNonExlusive: true);
	}

	public override void Collide(PhysicalObject otherObject, int myChunk, int otherChunk)
	{
		base.Collide(otherObject, myChunk, otherChunk);
		if (otherObject is Creature && falling && base.firstChunk.vel.magnitude > 15f)
		{
			otherObject.bodyChunks[otherChunk].vel = base.firstChunk.vel;
			Vector2 vector = base.firstChunk.vel * -0.75f;
			base.firstChunk.vel = base.firstChunk.vel.normalized * Mathf.Clamp(base.firstChunk.vel.magnitude, 0f, 14f);
			base.firstChunk.vel += vector;
			if (killTagHolder != null)
			{
				(otherObject as Creature).SetKillTag(killTagHolder.abstractCreature);
			}
			if (UnityEngine.Random.value < 0.5f)
			{
				room.AddObject(new PomegranateSeed(base.firstChunk.pos + Custom.RNV() * UnityEngine.Random.Range(0f, 10f), Custom.RNV() * UnityEngine.Random.Range(2f, 6f) + new Vector2(0f, 2f), baseColor));
			}
			for (int i = 0; i < UnityEngine.Random.Range(1, 4); i++)
			{
				room.AddObject(new WaterDrip(base.firstChunk.pos + Custom.RNV() * UnityEngine.Random.Range(0f, 4f), Custom.RNV() * UnityEngine.Random.Range(4f, 10f) + new Vector2(0f, 5f), waterColor: false));
			}
			room.PlaySound(SoundID.Spear_Stick_In_Creature, base.firstChunk.pos, 1f, 0.9f);
			(otherObject as Creature).Violence(base.firstChunk, base.firstChunk.vel, otherObject.firstChunk, null, Creature.DamageType.Blunt, 10f, 120f);
			room.ScreenMovement(base.firstChunk.pos, new Vector2(0f, -1f), 0.3f);
		}
	}

	public static void ISSeed(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, int firstSprite, float size, Color color)
	{
		sLeaser.sprites[firstSprite] = new FSprite("Circle20");
		sLeaser.sprites[firstSprite].scale = 0.3f * size;
		sLeaser.sprites[firstSprite + 1] = new FSprite("Circle20");
		sLeaser.sprites[firstSprite + 1].scale = 0.175f * size;
		sLeaser.sprites[firstSprite + 2] = new FSprite("Circle20");
		sLeaser.sprites[firstSprite + 2].scale = 0.05f * size;
	}

	public static void DSSeed(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos, int firstSprite, float size, Color color, Vector2 p, bool visible)
	{
		sLeaser.sprites[firstSprite].x = p.x - camPos.x;
		sLeaser.sprites[firstSprite].y = p.y - camPos.y;
		sLeaser.sprites[firstSprite + 1].x = p.x - camPos.x;
		sLeaser.sprites[firstSprite + 1].y = p.y - camPos.y;
		sLeaser.sprites[firstSprite + 2].x = p.x - camPos.x + 1f;
		sLeaser.sprites[firstSprite + 2].y = p.y - camPos.y + 1f;
		sLeaser.sprites[firstSprite].scale = 0.3f * size;
		sLeaser.sprites[firstSprite + 1].scale = 0.175f * size;
		sLeaser.sprites[firstSprite + 2].scale = 0.05f * size;
		for (int i = 0; i < 3; i++)
		{
			sLeaser.sprites[firstSprite + i].isVisible = visible;
		}
	}

	public static void APSeed(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette, int firstSprite, float size, Color color, float darkness)
	{
		sLeaser.sprites[firstSprite].color = Color.Lerp(Color.Lerp(color, Color.white, 0.5f), palette.blackColor, darkness);
		sLeaser.sprites[firstSprite + 1].color = Color.Lerp(Color.Lerp(color, palette.blackColor, 0.125f), palette.blackColor, darkness);
		sLeaser.sprites[firstSprite + 2].color = Color.Lerp(Color.white, palette.blackColor, Mathf.Pow(darkness, 2f));
	}

	public int SmashedBitSprite(int bit, int shade)
	{
		return 2 + bit * 3 + shade;
	}

	public int FirstSeedSprite(int seed)
	{
		return 2 + smashedBits.GetLength(0) * 3 + seed * 3;
	}

	public int BackingSprite(int shade)
	{
		return shade;
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		if (smashed)
		{
			sLeaser.sprites = new FSprite[2 + smashedBits.GetLength(0) * 3 + seeds.Length * 3];
			sLeaser.sprites[BackingSprite(0)] = new FSprite("PomegranateA");
			sLeaser.sprites[BackingSprite(0)].scale = 0.8f * overallSize;
			sLeaser.sprites[BackingSprite(1)] = new FSprite("PomegranateB");
			sLeaser.sprites[BackingSprite(1)].scale = 0.8f * overallSize;
			for (int i = 0; i < smashedBits.GetLength(0); i++)
			{
				if (i < 2)
				{
					sLeaser.sprites[SmashedBitSprite(i, 0)] = new FSprite("PomegranateShell" + (i + 1) + "A");
					sLeaser.sprites[SmashedBitSprite(i, 1)] = new FSprite("PomegranateShell" + (i + 1) + "B");
					sLeaser.sprites[SmashedBitSprite(i, 2)] = new FSprite("PomegranateShell" + (i + 1) + "C");
					sLeaser.sprites[SmashedBitSprite(i, 0)].scaleX = shellHalves[i];
					sLeaser.sprites[SmashedBitSprite(i, 1)].scaleX = shellHalves[i];
					sLeaser.sprites[SmashedBitSprite(i, 2)].scaleX = shellHalves[i];
					sLeaser.sprites[SmashedBitSprite(i, 2)].alpha = 0.75f;
				}
				else
				{
					sLeaser.sprites[SmashedBitSprite(i, 0)] = new FSprite("PomegranateBit" + bitSizes[i - 2, 2] + "A");
					sLeaser.sprites[SmashedBitSprite(i, 1)] = new FSprite("PomegranateBit" + bitSizes[i - 2, 2] + "B");
					sLeaser.sprites[SmashedBitSprite(i, 2)] = new FSprite("PomegranateBit" + bitSizes[i - 2, 2] + "C");
					sLeaser.sprites[SmashedBitSprite(i, 0)].scale = bitSizes[i - 2, 0];
					sLeaser.sprites[SmashedBitSprite(i, 1)].scale = bitSizes[i - 2, 0];
					sLeaser.sprites[SmashedBitSprite(i, 2)].scale = bitSizes[i - 2, 0];
					sLeaser.sprites[SmashedBitSprite(i, 2)].alpha = 0.75f;
				}
			}
			for (int j = 0; j < seeds.Length; j++)
			{
				ISSeed(sLeaser, rCam, FirstSeedSprite(j), seedSizes[j], baseColor);
			}
		}
		else
		{
			sLeaser.sprites = new FSprite[2];
			sLeaser.sprites[0] = new FSprite("PomegranateA");
			sLeaser.sprites[1] = new FSprite("PomegranateB");
			sLeaser.sprites[0].scale *= overallSize;
			sLeaser.sprites[1].scale *= overallSize;
		}
		AddToContainer(sLeaser, rCam, null);
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		if (refreshSprites)
		{
			sLeaser.RemoveAllSpritesFromContainer();
			InitiateSprites(sLeaser, rCam);
			ApplyPalette(sLeaser, rCam, rCam.currentPalette);
			refreshSprites = false;
		}
		if (spearmasterStabbed && stabbedFade < 1f)
		{
			stabbedFade += 0.025f;
			ApplyPalette(sLeaser, rCam, rCam.currentPalette);
		}
		Vector2 vector = Vector2.Lerp(base.firstChunk.lastPos, base.firstChunk.pos, timeStacker);
		lastDarkness = darkness;
		darkness = rCam.room.Darkness(vector) * (1f - rCam.room.LightSourceExposure(vector));
		if (darkness != lastDarkness)
		{
			ApplyPalette(sLeaser, rCam, rCam.currentPalette);
		}
		if (smashed)
		{
			Vector2 vector2 = Vector2.Lerp(Vector2.Lerp(smashedBits[0, 1], smashedBits[0, 0], timeStacker), Vector2.Lerp(smashedBits[1, 1], smashedBits[1, 0], timeStacker), 0.5f);
			sLeaser.sprites[BackingSprite(0)].x = vector2.x - camPos.x;
			sLeaser.sprites[BackingSprite(0)].y = vector2.y - camPos.y;
			sLeaser.sprites[BackingSprite(0)].rotation = Custom.AimFromOneVectorToAnother(Vector2.Lerp(smashedBits[0, 1], smashedBits[0, 0], timeStacker), Vector2.Lerp(smashedBits[1, 1], smashedBits[1, 0], timeStacker)) - 90f;
			sLeaser.sprites[BackingSprite(1)].x = sLeaser.sprites[BackingSprite(0)].x;
			sLeaser.sprites[BackingSprite(1)].y = sLeaser.sprites[BackingSprite(0)].y;
			sLeaser.sprites[BackingSprite(1)].rotation = sLeaser.sprites[BackingSprite(0)].rotation;
			for (int i = 0; i < smashedBits.GetLength(0); i++)
			{
				sLeaser.sprites[SmashedBitSprite(i, 0)].x = Vector2.Lerp(smashedBits[i, 1], smashedBits[i, 0], timeStacker).x - camPos.x;
				sLeaser.sprites[SmashedBitSprite(i, 0)].y = Vector2.Lerp(smashedBits[i, 1], smashedBits[i, 0], timeStacker).y - camPos.y;
				sLeaser.sprites[SmashedBitSprite(i, 1)].x = sLeaser.sprites[SmashedBitSprite(i, 0)].x;
				sLeaser.sprites[SmashedBitSprite(i, 1)].y = sLeaser.sprites[SmashedBitSprite(i, 0)].y;
				sLeaser.sprites[SmashedBitSprite(i, 2)].x = sLeaser.sprites[SmashedBitSprite(i, 0)].x;
				sLeaser.sprites[SmashedBitSprite(i, 2)].y = sLeaser.sprites[SmashedBitSprite(i, 0)].y;
				float num = 0f;
				if (i > 2)
				{
					num = bitSizes[i - 2, 1];
				}
				sLeaser.sprites[SmashedBitSprite(i, 0)].rotation = Custom.AimFromOneVectorToAnother(Vector2.Lerp(smashedBits[i, 1], smashedBits[i, 0], timeStacker), base.firstChunk.pos) + num;
				if (i == 1)
				{
					sLeaser.sprites[SmashedBitSprite(i, 0)].rotation = Custom.AimFromOneVectorToAnother(base.firstChunk.pos, Vector2.Lerp(smashedBits[i, 1], smashedBits[i, 0], timeStacker)) + num;
				}
				if (i < 2)
				{
					sLeaser.sprites[SmashedBitSprite(i, 0)].rotation -= 90f;
					sLeaser.sprites[SmashedBitSprite(i, 0)].rotation /= 2f;
				}
				sLeaser.sprites[SmashedBitSprite(i, 1)].rotation = sLeaser.sprites[SmashedBitSprite(i, 0)].rotation;
				sLeaser.sprites[SmashedBitSprite(i, 2)].rotation = sLeaser.sprites[SmashedBitSprite(i, 0)].rotation;
			}
			for (int j = 0; j < seeds.Length; j++)
			{
				DSSeed(sLeaser, rCam, timeStacker, camPos, FirstSeedSprite(j), seedSizes[j], baseColor, vector + seeds[j], !spearmasterStabbed || j % 6 == 1);
			}
		}
		else
		{
			Vector2 v = Vector3.Slerp(lastRotation, rotation, timeStacker);
			for (int k = 0; k < 2; k++)
			{
				sLeaser.sprites[k].x = vector.x - camPos.x;
				sLeaser.sprites[k].y = vector.y - camPos.y;
				sLeaser.sprites[k].rotation = Custom.VecToDeg(v);
			}
			Color a = Color.Lerp(shellColor, Color.Lerp(rCam.currentPalette.fogColor, new Color(1f, 1f, 1f), 0.9f), 0.25f);
			sLeaser.sprites[0].color = Color.Lerp(shellColor, rCam.currentPalette.blackColor, darkness);
			sLeaser.sprites[1].color = Color.Lerp(a, rCam.currentPalette.blackColor, darkness);
			if (blink > 0 && UnityEngine.Random.value < 0.5f)
			{
				sLeaser.sprites[0].color = Color.Lerp(sLeaser.sprites[0].color, new Color(1f, 1f, 1f), 0.25f);
				sLeaser.sprites[1].color = Color.Lerp(sLeaser.sprites[1].color, new Color(1f, 1f, 1f), 0.5f);
			}
		}
		if (base.slatedForDeletetion || room != rCam.room)
		{
			sLeaser.CleanSpritesAndRemove();
		}
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		Color color = baseColor;
		if (ModManager.MSC)
		{
			color = Color.Lerp(baseColor, palette.blackColor, stabbedFade * 0.5f);
		}
		Color a = Color.Lerp(shellColor, Color.Lerp(palette.fogColor, new Color(1f, 1f, 1f), 0.9f), 0.25f);
		if (smashed)
		{
			Color a2 = color;
			Color a3 = Color.Lerp(a2, Color.Lerp(palette.fogColor, new Color(1f, 1f, 1f), 0.9f), 0.4f);
			sLeaser.sprites[BackingSprite(1)].color = Color.Lerp(a2, palette.blackColor, darkness);
			sLeaser.sprites[BackingSprite(0)].color = Color.Lerp(a3, palette.blackColor, darkness);
			Color a4 = Color.Lerp(color, palette.blackColor, 0.75f);
			for (int i = 0; i < smashedBits.GetLength(0); i++)
			{
				sLeaser.sprites[SmashedBitSprite(i, 0)].color = Color.Lerp(shellColor, palette.blackColor, darkness);
				sLeaser.sprites[SmashedBitSprite(i, 1)].color = Color.Lerp(a, palette.blackColor, darkness);
				sLeaser.sprites[SmashedBitSprite(i, 2)].color = Color.Lerp(a4, palette.blackColor, darkness);
			}
			for (int j = 0; j < seeds.Length; j++)
			{
				APSeed(sLeaser, rCam, palette, FirstSeedSprite(j), seedSizes[j], color, darkness);
			}
		}
		else
		{
			sLeaser.sprites[0].color = Color.Lerp(shellColor, palette.blackColor, darkness);
			sLeaser.sprites[1].color = Color.Lerp(a, palette.blackColor, darkness);
		}
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		if (newContatiner == null)
		{
			newContatiner = rCam.ReturnFContainer(smashed ? "Background" : "Items");
		}
		for (int i = 0; i < sLeaser.sprites.Length; i++)
		{
			sLeaser.sprites[i].RemoveFromContainer();
		}
		if (smashed)
		{
			newContatiner.AddChild(sLeaser.sprites[BackingSprite(0)]);
			newContatiner.AddChild(sLeaser.sprites[BackingSprite(1)]);
			bool flag = false;
			for (int j = 0; j < smashedBits.GetLength(0); j++)
			{
				newContatiner.AddChild(sLeaser.sprites[SmashedBitSprite(j, 0)]);
				newContatiner.AddChild(sLeaser.sprites[SmashedBitSprite(j, 2)]);
				newContatiner.AddChild(sLeaser.sprites[SmashedBitSprite(j, 1)]);
				if (j > smashedBits.GetLength(0) / 2 - 1 && !flag)
				{
					for (int k = 0; k < seeds.Length; k++)
					{
						newContatiner.AddChild(sLeaser.sprites[FirstSeedSprite(k)]);
						newContatiner.AddChild(sLeaser.sprites[FirstSeedSprite(k) + 1]);
						newContatiner.AddChild(sLeaser.sprites[FirstSeedSprite(k) + 2]);
					}
					flag = true;
					newContatiner.AddChild(sLeaser.sprites[SmashedBitSprite(0, 0)]);
					newContatiner.AddChild(sLeaser.sprites[SmashedBitSprite(0, 2)]);
					newContatiner.AddChild(sLeaser.sprites[SmashedBitSprite(0, 1)]);
					newContatiner.AddChild(sLeaser.sprites[SmashedBitSprite(1, 0)]);
					newContatiner.AddChild(sLeaser.sprites[SmashedBitSprite(1, 2)]);
					newContatiner.AddChild(sLeaser.sprites[SmashedBitSprite(1, 1)]);
				}
			}
		}
		else
		{
			newContatiner.AddChild(sLeaser.sprites[0]);
			newContatiner.AddChild(sLeaser.sprites[1]);
		}
	}
}
