using System;
using System.Globalization;
using System.IO;
using System.Linq;
using HUD;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

namespace Watcher;

public class PearlContent : CosmeticSprite
{
	public class ElementState
	{
		public float relativeTimeMultiplier = 1f;

		public bool useLocalTime;

		public bool keepAspect = true;

		public Vector2 pos = Vector2.zero;

		public Vector2 size = Vector2.one;

		public Color col = Color.white;

		public int frameCount;

		public float level = 1f;

		public float rotation;

		public float localPos;

		public float localTime;

		public float lastLocalTime;

		public float randomOffset;

		public float animationFrame;

		public void Reset(float position)
		{
			localTime = position;
			col = Color.white;
			pos = Vector2.zero;
			size = Vector2.one;
			level = 1f;
			rotation = 0f;
		}

		public void FullReset()
		{
			Reset(0f);
			localPos = 0f;
			localTime = 0f;
			lastLocalTime = 0f;
			randomOffset = 0f;
			animationFrame = 0f;
		}
	}

	public class Image : Element, INotifyWhenRoomUnloaded
	{
		public string name;

		public string shader = "PearlReaderContent";

		public FAtlasElement element;

		public Vector2 lastPosition;

		public Vector2 lastSize;

		public float lastLevel;

		public float lastRotation;

		public Mesh imageMesh;

		public Color lastCol;

		public Image(string name, bool pointFilter = true)
		{
			this.name = name;
			element = GetOrLoadFAtlasElement(name, pointFilter);
		}

		public override void Destroy()
		{
			base.Destroy();
			if (imageMesh != null)
			{
				UnityEngine.Object.Destroy(imageMesh);
				imageMesh = null;
			}
		}

		public void RoomUnloaded()
		{
			Destroy();
		}

		public override void MovePlayhead(float position)
		{
			pos = owner.pos;
			SaveLast();
			base.MovePlayhead(position);
			if (position == 0f || state.localTime == 0f || state.localTime < state.lastLocalTime)
			{
				SaveLast();
			}
			state.lastLocalTime = state.localTime;
		}

		public void SaveLast()
		{
			lastPosition = state.pos;
			lastSize = state.size;
			lastLevel = state.level;
			lastCol = state.col;
			lastRotation = state.rotation;
		}

		public FAtlasElement GetOrLoadFAtlasElement(string name, bool crisp = true)
		{
			if (Futile.atlasManager.DoesContainElementWithName(name))
			{
				return Futile.atlasManager.GetElementWithName(name);
			}
			string text = AssetManager.ResolveFilePath("pearlslides" + Path.DirectorySeparatorChar + name + ".png");
			if (!File.Exists(text))
			{
				return null;
			}
			Texture2D texture2D = new Texture2D(1, 1, TextureFormat.ARGB32, mipChain: false);
			AssetManager.SafeWWWLoadTexture(ref texture2D, "file:///" + text, clampWrapMode: true, crisp);
			Futile.atlasManager.LoadAtlasFromTexture(name, texture2D, textureFromAsset: false);
			return Futile.atlasManager.GetElementWithName(name);
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			if (element != null)
			{
				imageMesh = CreateMesh(element);
				sLeaser.maskSources = new MaskSource[1];
				sLeaser.maskSources[0] = MaskMaker.MakeSource("PearlReaderGrab", shader, isSecondLayer: false, imageMesh, GetPosition(rCam.pos, 0f), GetRotation(0f), GetScale(0f));
				UpdateMaskSourceTranslation(sLeaser.maskSources[0], rCam.pos, 0f);
				UpdateMaskSourceColor(sLeaser.maskSources[0], 0f);
				if (element != null)
				{
					sLeaser.maskSources[0].SetTexture(element.atlas.texture);
				}
				sLeaser.maskSources[0].SetProperty("_CropRect", owner.cropRectScreen);
			}
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			if (base.slatedForDeletetion)
			{
				if (sLeaser.maskSources.Length != 0)
				{
					sLeaser.maskSources = new MaskSource[0];
				}
			}
			else if (element != null && sLeaser.maskSources.Length != 0 && sLeaser.maskSources[0] != null && !(sLeaser.maskSources[0].obj == null))
			{
				bool flag = Element.IsVisible(owner.cropRect, GetPosition(camPos, timeStacker), GetScale(timeStacker) / magicScale, state.rotation, state);
				sLeaser.maskSources[0].obj.SetActive(flag);
				if (flag)
				{
					UpdateMaskSourceTranslation(sLeaser.maskSources[0], camPos, timeStacker);
					UpdateMaskSourceColor(sLeaser.maskSources[0], timeStacker);
					sLeaser.maskSources[0].SetProperty("_CropRect", owner.cropRectScreen);
					base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
				}
			}
		}

		public virtual void UpdateMaskSourceTranslation(MaskSource ms, Vector2 camPos, float timeStacker)
		{
			ms.setGameObjectPos = GetPosition(camPos, timeStacker);
			ms.setGameObjectScale = GetScale(timeStacker);
			ms.setGameObjectRotation = GetRotation(timeStacker);
		}

		public virtual Vector2 GetPosition(Vector2 camPos, float timeStacker)
		{
			Vector2 vector = Vector2.Lerp(lastPosition, state.pos, timeStacker);
			return owner.pos + owner.size * vector - camPos;
		}

		public virtual Vector3 GetScale(float timeStacker)
		{
			Vector2 vector = Vector2.Lerp(lastSize, state.size, timeStacker);
			Vector2 size = element.sourceRect.size;
			return vector * magicScale * (state.keepAspect ? (Vector2.one * Mathf.Min(owner.size.x, owner.size.y) * (size / Mathf.Max(size.x, size.y))) : owner.size);
		}

		public virtual Vector3 GetRotation(float timeStacker)
		{
			return Vector3.back * Mathf.LerpAngle(lastRotation, state.rotation, timeStacker);
		}

		public virtual void UpdateMaskSourceColor(MaskSource ms, float timeStacker)
		{
			ms.SetProperty(0, Mathf.Lerp(lastCol.r, state.col.r, timeStacker));
			ms.SetProperty(1, Mathf.Lerp(lastCol.g, state.col.g, timeStacker));
			ms.SetProperty(2, Mathf.Lerp(lastCol.b, state.col.b, timeStacker));
			ms.SetProperty(3, Mathf.Lerp(lastLevel, state.level, timeStacker));
			ms.ApplyVertexColor();
		}
	}

	public class Atlas : Image
	{
		public float widthFrames = 1f;

		public float widthStep;

		public float heightFrames = 1f;

		public float heightStep;

		public float numberOfFrames = 1f;

		public Atlas(string[] props)
			: base(props[0], props.Length <= 4)
		{
			widthFrames = ((props.Length > 1) ? ParseFloat(props[1]) : widthFrames);
			heightFrames = ((props.Length > 2) ? ParseFloat(props[2]) : heightFrames);
			numberOfFrames = ((props.Length > 2) ? ParseFloat(props[3]) : numberOfFrames);
			widthStep = 1f / widthFrames;
			heightStep = 1f / heightFrames;
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			base.InitiateSprites(sLeaser, rCam);
			if (sLeaser.maskSources.Length != 0)
			{
				SetUv(sLeaser.maskSources[0]);
				sLeaser.maskSources[0].SetProperty("_CropRect", owner.cropRectScreen);
			}
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
			if (element != null && sLeaser.maskSources.Length != 0 && sLeaser.maskSources[0] != null && !(sLeaser.maskSources[0].obj == null))
			{
				SetUv(sLeaser.maskSources[0]);
				sLeaser.maskSources[0].SetProperty("_CropRect", owner.cropRectScreen);
			}
		}

		public override Vector3 GetScale(float timeStacker)
		{
			Vector2 vector = Vector2.Lerp(lastSize, state.size, timeStacker);
			Vector2 vector2 = new Vector2(widthStep, heightStep) * element.sourceRect.size;
			vector2 /= Mathf.Max(vector2.x, vector2.y);
			return vector * magicScale * (state.keepAspect ? (Vector2.one * Mathf.Min(owner.size.x, owner.size.y) * (vector2 / Mathf.Max(vector2.x, vector2.y))) : owner.size);
		}

		public void SetUv(MaskSource ms)
		{
			if (!base.slatedForDeletetion)
			{
				float num = state.animationFrame % numberOfFrames;
				Vector2 vector = new Vector2(num % widthFrames * widthStep, Mathf.Floor(num / widthFrames) * heightStep);
				Vector2[] uv = new Vector2[4]
				{
					new Vector2(vector.x, 1f - vector.y - heightStep),
					new Vector2(vector.x, 1f - vector.y),
					new Vector2(vector.x + widthStep, 1f - vector.y - heightStep),
					new Vector2(vector.x + widthStep, 1f - vector.y)
				};
				ms.meshFilter.sharedMesh.uv = uv;
			}
		}
	}

	public class SoundVisual : Sound, INotifyWhenRoomUnloaded
	{
		public string shader = "PearlReaderContent";

		public Vector2 lastPosition;

		public Vector2 lastSize;

		public float lastLevel;

		public float lastRotation;

		public float[] spectrum;

		public Vector3[] lastVerts;

		public int segments = 31;

		public Mesh soundMesh;

		public Color lastCol;

		public SoundVisual(string[] props)
			: base(props)
		{
			spectrum = new float[(segments + 1) * 8];
		}

		public override void Destroy()
		{
			base.Destroy();
			if (soundMesh != null)
			{
				UnityEngine.Object.Destroy(soundMesh);
				soundMesh = null;
			}
		}

		public void RoomUnloaded()
		{
			Destroy();
		}

		public Mesh CreateMesh(FAtlasElement element)
		{
			Mesh mesh = new Mesh
			{
				name = "PearlSoundVisual Mesh"
			};
			mesh.vertices = new Vector3[(1 + segments) * 2];
			lastVerts = new Vector3[mesh.vertices.Length];
			mesh.triangles = MaskSource.GridTriangles(segments, 1);
			Vector2[] array = new Vector2[mesh.vertices.Length];
			int num = mesh.vertices.Length / 2;
			for (int i = 0; i < num; i++)
			{
				int num2 = i * 2;
				float t = Mathf.InverseLerp(0f, num, i);
				array[num2] = Vector2.Lerp(element.uvTopLeft, element.uvBottomLeft, t);
				array[num2 + 1] = Vector2.Lerp(element.uvBottomRight, element.uvTopRight, t);
			}
			mesh.uv = array;
			return mesh;
		}

		public override void MovePlayhead(float position)
		{
			pos = owner.pos;
			SaveLast();
			base.MovePlayhead(position);
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			if (se != null && se.currentSoundObject != null && se.currentSoundObject.audioSource != null)
			{
				AudioClip clip = se.currentSoundObject.audioSource.clip;
				clip.GetData(spectrum, Mathf.Min(clip.samples - spectrum.Length, se.currentSoundObject.audioSource.timeSamples));
			}
		}

		public void SaveLast()
		{
			lastPosition = state.pos;
			lastSize = state.size;
			lastLevel = state.level;
			lastCol = state.col;
			lastRotation = state.rotation;
		}

		public virtual Vector2 GetPosition(Vector2 camPos, float timeStacker)
		{
			Vector2 vector = Vector2.Lerp(lastPosition, state.pos, timeStacker);
			return owner.pos + owner.size * vector - camPos;
		}

		public virtual Vector3 GetScale(float timeStacker)
		{
			return Vector2.Lerp(lastSize, state.size, timeStacker) * magicScale * (state.keepAspect ? (Vector2.one * Mathf.Min(owner.size.x, owner.size.y)) : owner.size);
		}

		public virtual Vector3 GetRotation(float timeStacker)
		{
			return Vector3.back * Mathf.LerpAngle(lastRotation, state.rotation, timeStacker);
		}

		public virtual void UpdateMesh(MaskSource ms, float timeStacker)
		{
			if (!base.slatedForDeletetion)
			{
				Vector3[] vertices = ms.meshFilter.sharedMesh.vertices;
				int num = vertices.Length / 2;
				for (int i = 0; i < num; i++)
				{
					int num2 = i * 2;
					lastVerts[num2] = vertices[num2];
					lastVerts[num2 + 1] = vertices[num2 + 1];
					float num3 = Mathf.InverseLerp(0f, num, i);
					float num4 = 1f - Mathf.Abs(num3 - 0.5f) * 2f;
					Vector3 vector = Vector3.up * (spectrum[i * 8] * 0.5f * num4) + Vector3.right * Mathf.Lerp(-0.5f, 0.5f, num3);
					vertices[num2] = Vector3.Lerp(lastVerts[num2], Vector3.up * 0.04f + vector, timeStacker);
					vertices[num2 + 1] = Vector3.Lerp(lastVerts[num2 + 1], Vector3.down * 0.04f + vector, timeStacker);
				}
				ms.meshFilter.sharedMesh.vertices = vertices;
				ms.meshFilter.sharedMesh.RecalculateBounds();
			}
		}

		public virtual void UpdateMaskSourceTranslation(MaskSource ms, Vector2 camPos, float timeStacker)
		{
			ms.setGameObjectPos = GetPosition(camPos, timeStacker);
			ms.setGameObjectScale = GetScale(timeStacker);
			ms.setGameObjectRotation = GetRotation(timeStacker);
		}

		public virtual void UpdateMaskSourceColor(MaskSource ms, float timeStacker)
		{
			ms.SetProperty(0, Mathf.Lerp(lastCol.r, state.col.r, timeStacker));
			ms.SetProperty(1, Mathf.Lerp(lastCol.g, state.col.g, timeStacker));
			ms.SetProperty(2, Mathf.Lerp(lastCol.b, state.col.b, timeStacker));
			ms.SetProperty(3, Mathf.Lerp(lastLevel, state.level, timeStacker));
			ms.ApplyVertexColor();
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			FAtlasElement elementWithName = Futile.atlasManager.GetElementWithName("listDivider");
			soundMesh = CreateMesh(elementWithName);
			sLeaser.maskSources = new MaskSource[1];
			sLeaser.maskSources[0] = MaskMaker.MakeSource("PearlReaderGrab", shader, isSecondLayer: false, soundMesh, GetPosition(rCam.pos, 0f), GetRotation(0f), GetScale(0f));
			UpdateMaskSourceTranslation(sLeaser.maskSources[0], rCam.pos, 0f);
			UpdateMaskSourceColor(sLeaser.maskSources[0], 0f);
			sLeaser.maskSources[0].SetTexture(elementWithName.atlas.texture);
			sLeaser.maskSources[0].SetProperty("_CropRect", owner.cropRectScreen);
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			if (base.slatedForDeletetion)
			{
				if (sLeaser.maskSources.Length != 0)
				{
					sLeaser.maskSources = new MaskSource[0];
				}
			}
			else if (sLeaser.maskSources.Length != 0 && sLeaser.maskSources[0] != null && !(sLeaser.maskSources[0].obj == null))
			{
				bool flag = Element.IsVisible(owner.cropRect, GetPosition(camPos, timeStacker), GetScale(timeStacker) / magicScale, state.rotation, state);
				sLeaser.maskSources[0].obj.SetActive(flag);
				if (flag)
				{
					UpdateMaskSourceTranslation(sLeaser.maskSources[0], camPos, timeStacker);
					UpdateMaskSourceColor(sLeaser.maskSources[0], timeStacker);
					UpdateMesh(sLeaser.maskSources[0], timeStacker);
					sLeaser.maskSources[0].SetProperty("_CropRect", owner.cropRectScreen);
					base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
				}
			}
		}
	}

	public class Dialog : Element
	{
		public class PearlConversation : Conversation
		{
			public PearlConversation(int id)
				: base(null, null, null)
			{
				LoadEventsFromFile(id);
			}
		}

		public bool dialogStarted;

		public string[] text;

		public int line = -1;

		public int forcedLinger = -1;

		public DialogBox.Message lastMessage;

		public Dialog(string[] props, bool isConvo = false)
		{
			if (isConvo)
			{
				PearlConversation pearlConversation = new PearlConversation((int)ParseFloat(props[0]));
				for (int i = 0; i < pearlConversation.events.Count; i++)
				{
					if (this.text == null)
					{
						this.text = new string[pearlConversation.events.Count];
					}
					this.text[i] = (pearlConversation.events[i] as Conversation.TextEvent).text;
				}
			}
			else
			{
				string text = ResolveFilePath(props[0]);
				if (text != null)
				{
					this.text = File.ReadAllLines(text);
				}
			}
			if (props.Length > 1)
			{
				line = (int)ParseFloat(props[1]);
			}
			if (props.Length > 2)
			{
				forcedLinger = (int)ParseFloat(props[1]);
			}
		}

		public static string ResolveFilePath(string name)
		{
			string text = AssetManager.ResolveFilePath("pearlstext" + Path.DirectorySeparatorChar + name + ".txt");
			if (!File.Exists(text))
			{
				return null;
			}
			return text;
		}

		public override void MovePlayhead(float position)
		{
			base.MovePlayhead(position);
			if (text == null || owner.room == null || dialogStarted || !(state.localTime > 0f) || !(state.localTime < owner.relativeTimeMultiplier))
			{
				return;
			}
			if (owner.room.game.cameras[0].hud.dialogBox == null)
			{
				room.game.cameras[0].hud.InitDialogBox();
			}
			if (line >= 0 && line < text.Length)
			{
				Message(text[line], interrupt: true, forcedLinger);
			}
			else
			{
				for (int i = 0; i < text.Length; i++)
				{
					Message(text[i], i == 0, forcedLinger);
				}
			}
			dialogStarted = true;
		}

		public void Message(string text, bool interrupt, int forceLinger = -1)
		{
			DialogBox dialogBox = owner.room.game.cameras[0].hud.dialogBox;
			lastMessage = new DialogBox.Message(text, dialogBox.defaultXOrientation, dialogBox.defaultYPos, 60);
			if (forceLinger >= 0)
			{
				lastMessage.linger = forceLinger;
			}
			if (interrupt)
			{
				dialogBox.Interrupt(lastMessage);
			}
			else
			{
				dialogBox.NewMessage(lastMessage);
			}
		}
	}

	public class Sound : Element
	{
		public SoundID soundID;

		public bool loop;

		public PositionedSoundEmitter se;

		public bool lastPaused;

		public float lastSoundTime;

		public Sound(string[] props)
		{
			soundID = new SoundID(props[0]);
			if (props.Length > 1)
			{
				loop = ParseFloat(props[1]) > 0.5f;
			}
			else
			{
				loop = props[0].Contains("LOOP");
			}
		}

		public override void PausedUpdate()
		{
			base.PausedUpdate();
			HandlePause();
		}

		public override void MovePlayhead(float position)
		{
			base.MovePlayhead(position);
			if (owner.room != null && soundID != null && se == null && state.localTime > 0f && state.localTime < owner.relativeTimeMultiplier)
			{
				se = new PositionedSoundEmitter(owner.pos, state.level, 1f);
				owner.room.PlaySound(soundID, se, loop, state.level, 1f, randomStartPosition: false);
			}
			state.level *= Mathf.InverseLerp(0f, 40f, (int)owner.life);
			state.level *= Mathf.InverseLerp(owner.life.max, owner.life.max - 40, (int)owner.life);
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			HandlePause();
			if (se != null)
			{
				se.volume = state.level;
				se.Update(eu);
			}
		}

		public void HandlePause()
		{
			bool gamePaused = room.game.GamePaused;
			if (se != null && se.currentSoundObject != null)
			{
				AudioSource audioSource = se.currentSoundObject.audioSource;
				if ((object)audioSource != null)
				{
					if (gamePaused)
					{
						if (lastPaused != gamePaused)
						{
							lastSoundTime = audioSource.time;
						}
						audioSource.time = lastSoundTime;
						se.volume = 0.001f;
					}
					else if (lastPaused != gamePaused)
					{
						audioSource.time = lastSoundTime;
					}
				}
			}
			lastPaused = gamePaused;
		}
	}

	public class Text : Element, INotifyWhenRoomUnloaded
	{
		public int font;

		public float seed;

		public int n;

		public int nx;

		public int ny;

		public FAtlasElement element;

		public Vector2 lastPosition;

		public Vector2 lastSize;

		public float lastLevel;

		public float lastRotation;

		public Color lastCol;

		public int[] glyphs;

		public Mesh textMesh;

		public Text(string props)
		{
			float[] floats = GetFloats(props);
			nx = (int)floats[0];
			ny = (int)floats[1];
			font = ((floats.Length > 2) ? ((int)floats[2]) : 0);
			seed = ((floats.Length > 3) ? floats[3] : 0f);
			element = Futile.atlasManager.GetElementWithName("glyphs");
		}

		public override void Destroy()
		{
			base.Destroy();
			if (textMesh != null)
			{
				UnityEngine.Object.Destroy(textMesh);
				textMesh = null;
			}
		}

		public void RoomUnloaded()
		{
			Destroy();
		}

		public void SaveLast()
		{
			lastPosition = state.pos;
			lastSize = state.size;
			lastLevel = state.level;
			lastCol = state.col;
			lastRotation = state.rotation;
		}

		public override void MovePlayhead(float position)
		{
			SaveLast();
			base.MovePlayhead(position);
			if (position == 0f || state.localTime == 0f || state.localTime < state.lastLocalTime)
			{
				SaveLast();
			}
			state.lastLocalTime = state.localTime;
			if (glyphs == null)
			{
				InitText();
			}
		}

		public void InitText()
		{
			n = nx * ny;
			int num = 0;
			glyphs = new int[n];
			glyphs = GlyphLabel.RandomString(n, (int)((float)owner.seed + seed), font > 0);
			UnityEngine.Random.State state = UnityEngine.Random.state;
			UnityEngine.Random.InitState((int)((float)owner.seed + seed));
			for (int i = 0; i < n; i++)
			{
				if ((double)UnityEngine.Random.value > 1.0 - 0.03 * (double)num)
				{
					glyphs[i] = -1;
					num = 0;
				}
				else
				{
					num++;
				}
			}
			UnityEngine.Random.state = state;
		}

		public static void GetUv(Vector2[] uvs, int quadIndex, int glyphIndex)
		{
			float num = (float)glyphIndex % 50f * 0.02f;
			int num2 = quadIndex * 4;
			uvs[num2] = new Vector2(num, 0f);
			uvs[num2 + 1] = new Vector2(num, 1f);
			uvs[num2 + 2] = new Vector2(num + 0.02f, 0f);
			uvs[num2 + 3] = new Vector2(num + 0.02f, 1f);
		}

		public static void GetVerts(Vector3[] verts, int quadIndex, Vector2 bottomLeft, Vector2 quadSize)
		{
			int num = quadIndex * 4;
			verts[num] = bottomLeft;
			verts[num + 1] = bottomLeft + new Vector2(0f, quadSize.y);
			verts[num + 2] = bottomLeft + new Vector2(quadSize.x, 0f);
			verts[num + 3] = bottomLeft + quadSize;
		}

		public static void GetTris(int[] tris, int quadIndex)
		{
			int num = MaskSource.quadTris.Length;
			int num2 = quadIndex * num;
			for (int i = num2; i < num2 + num; i++)
			{
				tris[i] = MaskSource.quadTris[i - num2] + MaskSource.quadVerts.Length * quadIndex;
			}
		}

		public static Mesh CreateMesh(int[] glyphs, int rowLength, int colLength)
		{
			Vector2 quadSize = new Vector2(1f / (float)rowLength, 1f / (float)colLength);
			Vector3[] array = new Vector3[MaskSource.quadVerts.Length * glyphs.Length];
			Vector2[] array2 = new Vector2[MaskSource.quadUV.Length * glyphs.Length];
			int[] array3 = new int[MaskSource.quadTris.Length * glyphs.Length];
			for (int i = 0; i < glyphs.Length; i++)
			{
				GetUv(array2, i, glyphs[i]);
				Vector2 bottomLeft = new Vector2(quadSize.x * (float)(i % rowLength), 0f - quadSize.y - quadSize.y * (float)(i / rowLength)) - new Vector2(0.5f, -0.5f);
				GetVerts(array, i, bottomLeft, quadSize);
				GetTris(array3, i);
			}
			return new Mesh
			{
				name = "PearlContentText Mesh",
				vertices = array,
				uv = array2,
				triangles = array3
			};
		}

		public Vector2 GetPosition(Vector2 camPos, float timeStacker)
		{
			return owner.pos - camPos + owner.size * Vector2.Lerp(lastPosition, state.pos, timeStacker);
		}

		public virtual Vector3 GetRotation(float timeStacker)
		{
			return Vector3.back * Mathf.LerpAngle(lastRotation, state.rotation, timeStacker);
		}

		public Vector2 GetScale(float timeStacker)
		{
			return owner.size * Vector2.Lerp(lastSize, state.size, timeStacker) * magicScale;
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			base.InitiateSprites(sLeaser, rCam);
			sLeaser.maskSources = new MaskSource[1];
			textMesh = CreateMesh(glyphs, nx, ny);
			sLeaser.maskSources[0] = MaskMaker.MakeSource("PearlReaderGrab", "PearlReaderContent", isSecondLayer: false, textMesh);
			sLeaser.maskSources[0].setGameObjectPos = GetPosition(rCam.pos, 0f);
			sLeaser.maskSources[0].setGameObjectScale = GetScale(0f);
			sLeaser.maskSources[0].setGameObjectRotation = GetRotation(0f);
			sLeaser.maskSources[0].SetTexture(element.atlas.texture);
			sLeaser.maskSources[0].SetProperty("_IsText", 1f);
			sLeaser.maskSources[0].SetProperty("_CropRect", owner.cropRectScreen);
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
			if (base.slatedForDeletetion)
			{
				if (sLeaser.maskSources.Length != 0)
				{
					sLeaser.maskSources = new MaskSource[0];
				}
			}
			else if (element != null && sLeaser.maskSources.Length != 0 && sLeaser.maskSources[0] != null && !(sLeaser.maskSources[0].obj == null))
			{
				Vector2 vector = GetPosition(camPos, timeStacker);
				Vector2 scale = GetScale(timeStacker);
				bool flag = Element.IsVisible(owner.cropRect, vector, scale / magicScale, state.rotation, state);
				sLeaser.maskSources[0].obj.SetActive(flag);
				if (flag)
				{
					sLeaser.maskSources[0].setGameObjectPos = vector;
					sLeaser.maskSources[0].setGameObjectRotation = GetRotation(0f);
					sLeaser.maskSources[0].setGameObjectScale = scale;
					sLeaser.maskSources[0].SetProperty(0, Mathf.Lerp(lastCol.r, state.col.r, timeStacker));
					sLeaser.maskSources[0].SetProperty(1, Mathf.Lerp(lastCol.g, state.col.g, timeStacker));
					sLeaser.maskSources[0].SetProperty(2, Mathf.Lerp(lastCol.b, state.col.b, timeStacker));
					sLeaser.maskSources[0].SetProperty(3, Mathf.Lerp(lastLevel, state.level, timeStacker));
					sLeaser.maskSources[0].SetProperty("_CropRect", owner.cropRectScreen);
					sLeaser.maskSources[0].ApplyVertexColor();
				}
			}
		}
	}

	public abstract class Element : CosmeticSprite
	{
		public PearlContent owner;

		public IModifier[] modifiers;

		public ElementState state;

		public float position;

		public virtual void MovePlayhead(float position)
		{
			state.frameCount++;
			state.animationFrame = Mathf.Floor((float)state.frameCount / 3.3333333f);
			state.randomOffset = owner.randomOffset;
			state.Reset(position);
			for (int i = 0; i < modifiers.Length; i++)
			{
				modifiers[i]?.Apply(state, state.useLocalTime ? state.localTime : position);
			}
		}

		public static bool IsVisible(Vector4 cropRect, Vector2 center, Vector2 size, float rotation, ElementState state)
		{
			if (state.level <= 0f)
			{
				return false;
			}
			size.x = Mathf.Abs(size.x) * 0.5f;
			size.y = Mathf.Abs(size.y) * 0.5f;
			if (rotation == 0f || Custom.Mod(rotation, 180f) == 0f)
			{
				if (!(center.x + size.x < cropRect.x) && !(center.y + size.y < cropRect.y) && !(center.x - size.x > cropRect.z))
				{
					return !(center.y - size.y > cropRect.w);
				}
				return false;
			}
			if (Custom.Mod(rotation, 180f) == 90f)
			{
				if (!(center.x + size.y < cropRect.x) && !(center.y + size.x < cropRect.y) && !(center.x - size.y > cropRect.z))
				{
					return !(center.y - size.x > cropRect.w);
				}
				return false;
			}
			Vector2 vec = new Vector2(0f - size.x, 0f - size.y);
			Vector2 vec2 = new Vector2(0f - size.x, size.y);
			Vector2 vec3 = new Vector2(size.x, 0f - size.y);
			Vector2 vec4 = new Vector2(size.x, size.y);
			vec = Custom.rotateVectorDeg(vec, rotation);
			vec2 = Custom.rotateVectorDeg(vec2, rotation);
			vec3 = Custom.rotateVectorDeg(vec3, rotation);
			vec4 = Custom.rotateVectorDeg(vec4, rotation);
			size = Vector2.zero;
			size = Vector2.Max(size, vec);
			size = Vector2.Max(size, vec2);
			size = Vector2.Max(size, vec3);
			size = Vector2.Max(size, vec4);
			if (!(center.x + size.x < cropRect.x) && !(center.y + size.y < cropRect.y) && !(center.x - size.x > cropRect.z))
			{
				return !(center.y - size.y > cropRect.w);
			}
			return false;
		}

		public Element()
		{
		}
	}

	public class Rect : IModifier
	{
		public Vector2 bottomLeft;

		public Vector2 topRight;

		public float start;

		public float end;

		public int easeType;

		public Rect(Vector2 bottomLeft, Vector2 topRight, float start, float end, int easeType)
		{
			this.bottomLeft = bottomLeft;
			this.topRight = topRight;
			this.start = start;
			this.end = end;
			this.easeType = easeType;
		}

		public static IModifier FromString(string data)
		{
			float[] floats = GetFloats(data);
			return new Rect(new Vector2(floats[0], floats[1]), (floats.Length > 3) ? new Vector2(floats[2], floats[3]) : new Vector2(1f - floats[0], 1f - floats[1]), (floats.Length > 4) ? floats[4] : 0f, (floats.Length > 5) ? floats[5] : ((floats.Length > 4) ? (floats[4] + defaultOffset) : 0f), (int)((floats.Length > 6) ? floats[6] : 0f));
		}

		public void Apply(ElementState state, float position)
		{
			state.keepAspect = false;
			Vector2 vector = (topRight + bottomLeft - Vector2.one) * 0.5f;
			Vector2 vector2 = topRight - bottomLeft;
			if (start == end && start == 0f)
			{
				state.pos = vector;
				state.size = vector2;
			}
			else
			{
				float t = ILerp(start, end, position, easeType);
				state.pos = Vector2.Lerp(state.pos, vector, t);
				state.size = Vector2.Lerp(state.size, vector2, t);
			}
		}
	}

	public class Fit : IModifier
	{
		public Vector2 bottomLeft;

		public Vector2 topRight;

		public float start;

		public float end;

		public int easeType;

		public Fit(Vector2 bottomLeft, Vector2 topRight, float start, float end, int easeType)
		{
			this.bottomLeft = bottomLeft;
			this.topRight = topRight;
			this.start = start;
			this.end = end;
			this.easeType = easeType;
		}

		public static IModifier FromString(string data)
		{
			float[] floats = GetFloats(data);
			return new Fit(new Vector2(floats[0], floats[1]), (floats.Length > 3) ? new Vector2(floats[2], floats[3]) : new Vector2(1f - floats[0], 1f - floats[1]), (floats.Length > 4) ? floats[4] : 0f, (floats.Length > 5) ? floats[5] : ((floats.Length > 4) ? (floats[4] + defaultOffset) : 0f), (int)((floats.Length > 6) ? floats[6] : 0f));
		}

		public void Apply(ElementState state, float position)
		{
			state.keepAspect = false;
			topRight.x += ((topRight.x == bottomLeft.x) ? 1E-05f : 0f);
			topRight.y += ((topRight.y == bottomLeft.y) ? 1E-05f : 0f);
			_ = topRight - bottomLeft;
			Vector2 vector = (Vector2.one - topRight - bottomLeft) * 0.5f;
			Vector2 vector2 = Vector2.one / (topRight - bottomLeft);
			vector *= vector2;
			if (start == end && start == 0f)
			{
				state.pos = vector;
				state.size = vector2;
			}
			else
			{
				float t = ILerp(start, end, position, easeType);
				state.pos = Vector2.Lerp(state.pos, vector, t);
				state.size = Vector2.Lerp(state.size, vector2, t);
			}
		}
	}

	public class Osc : IModifier
	{
		public int mode;

		public int easeType;

		public float speedMult;

		public float phaseOffset;

		public Vector2 startPos;

		public Vector2 endPos;

		public Osc(Vector2 startPos, Vector2 endPos, float speedMult, float phaseOffset, int easeType)
		{
			this.startPos = startPos;
			this.endPos = endPos;
			this.speedMult = speedMult;
			this.phaseOffset = phaseOffset;
			this.easeType = easeType;
		}

		public static IModifier FromString(string data, int mode = 0)
		{
			float[] floats = GetFloats(data);
			return mode switch
			{
				3 => new Osc(new Vector2(floats[0], floats[1]), Vector2.zero, (floats.Length > 2) ? floats[2] : 1f, (floats.Length > 3) ? floats[3] : 0f, (int)((floats.Length > 4) ? floats[4] : 0f))
				{
					mode = mode
				}, 
				2 => new Osc(new Vector2(floats[0], floats[1]), Vector2.zero, (floats.Length > 2) ? floats[2] : 1f, (floats.Length > 3) ? floats[3] : 0f, (int)((floats.Length > 4) ? floats[4] : 0f))
				{
					mode = mode
				}, 
				_ => new Osc(new Vector2(floats[0], floats[1]), new Vector2(floats[2], floats[3]), (floats.Length > 4) ? floats[4] : 1f, (floats.Length > 5) ? floats[5] : 0f, (int)((floats.Length > 6) ? floats[6] : 0f))
				{
					mode = mode
				}, 
			};
		}

		public void Apply(ElementState state, float position)
		{
			float t = 1f - Mathf.Abs(Custom.Mod(position * 2f * speedMult + phaseOffset, 2f) - 1f);
			if (mode == 0)
			{
				state.pos += Vector2.Lerp(startPos, endPos, ILerp(0f, 1f, t, easeType));
			}
			else if (mode == 1)
			{
				state.size *= Vector2.Lerp(startPos, endPos, ILerp(0f, 1f, t, easeType));
			}
			else
			{
				state.rotation += Mathf.Lerp(startPos.x, startPos.y, ILerp(0f, 1f, t, easeType));
			}
		}
	}

	public class OscColor : IModifier
	{
		public int mode;

		public int easeType;

		public float speedMult;

		public float phaseOffset;

		public Color col;

		public OscColor(Color col, float speedMult, float phaseOffset, int easeType)
		{
			this.col = col;
			this.speedMult = speedMult;
			this.phaseOffset = phaseOffset;
			this.easeType = easeType;
		}

		public static IModifier FromString(string data, int mode = 0)
		{
			float[] floats = GetFloats(data);
			return new OscColor(new Color(floats[0], floats[1], floats[2]), (floats.Length > 3) ? floats[3] : 1f, (floats.Length > 4) ? floats[4] : 0f, (int)((floats.Length > 5) ? floats[5] : 0f))
			{
				mode = mode
			};
		}

		public void Apply(ElementState state, float position)
		{
			float t = 1f - Mathf.Abs(Custom.Mod(position * 2f * speedMult + phaseOffset, 2f) - 1f);
			t = ILerp(0f, 1f, t, easeType);
			if (mode == 1)
			{
				Color.RGBToHSV(state.col, out var H, out var S, out var V);
				H = Mathf.Lerp(H, col.r, t);
				S = Mathf.Lerp(S, col.g, t);
				V = Mathf.Lerp(V, col.b, t);
				state.col = Color.HSVToRGB(Custom.Mod(H, 1f), S, V);
			}
			else
			{
				state.col = Color.Lerp(state.col, col, t);
			}
		}
	}

	public class Move : IModifier
	{
		public int easeType;

		public float start;

		public float end;

		public Vector2 targetPos;

		public Move(Vector2 startPos, float start, float end, int easeType)
		{
			targetPos = startPos;
			this.start = start;
			this.end = end;
			this.easeType = easeType;
		}

		public static IModifier FromString(string data)
		{
			float[] floats = GetFloats(data);
			return new Move(new Vector2(floats[0], floats[1]), (floats.Length > 2) ? floats[2] : 0f, (floats.Length > 3) ? floats[3] : ((floats.Length > 2) ? (floats[2] + defaultOffset) : 0f), (int)((floats.Length > 4) ? floats[4] : 0f));
		}

		public void Apply(ElementState state, float position)
		{
			if (start == end && start == 0f)
			{
				state.pos += targetPos;
			}
			else
			{
				state.pos += targetPos * ILerp(start, end, position, easeType);
			}
		}
	}

	public class Level : IModifier
	{
		public int easeType;

		public float level;

		public float start;

		public float end;

		public Level(float level, float start, float end, int easeType)
		{
			this.level = level;
			this.start = start;
			this.end = end;
			this.easeType = easeType;
		}

		public static IModifier FromString(string data)
		{
			float[] floats = GetFloats(data);
			return new Level(floats[0], (floats.Length > 1) ? floats[1] : 0f, (floats.Length > 2) ? floats[2] : ((floats.Length > 1) ? (floats[1] + defaultOffset) : 0f), (int)((floats.Length > 6) ? floats[6] : 0f));
		}

		public void Apply(ElementState state, float position)
		{
			if (start == end && start == 0f)
			{
				state.level = level;
			}
			else
			{
				state.level = Mathf.Lerp(state.level, level, ILerp(start, end, position, easeType));
			}
		}
	}

	public class Size : IModifier
	{
		public int easeType;

		public Vector2 size;

		public float start;

		public float end;

		public Size(Vector2 size, float start, float end, int easeType)
		{
			this.size = size;
			this.start = start;
			this.end = end;
			this.easeType = easeType;
		}

		public static IModifier FromString(string data)
		{
			float[] floats = GetFloats(data);
			return new Size(new Vector2(floats[0], (floats.Length > 1) ? floats[1] : floats[0]), (floats.Length > 2) ? floats[2] : 0f, (floats.Length > 3) ? floats[3] : ((floats.Length > 2) ? (floats[2] + defaultOffset) : 0f), (int)((floats.Length > 4) ? floats[4] : 0f));
		}

		public void Apply(ElementState state, float position)
		{
			if (start == end && start == 0f)
			{
				state.size = size;
			}
			else
			{
				state.size = Vector2.Lerp(state.size, size, ILerp(start, end, position, easeType));
			}
		}
	}

	public class FadeIn : IModifier
	{
		public int easeType;

		public float start;

		public float end;

		public FadeIn(float start, float end, int easeType)
		{
			this.start = start;
			this.end = end;
			this.easeType = easeType;
		}

		public static IModifier FromString(string data)
		{
			float[] floats = GetFloats(data);
			return new FadeIn(floats[0], floats[1], (int)((floats.Length > 2) ? floats[2] : 0f));
		}

		public void Apply(ElementState state, float position)
		{
			state.level *= ILerp(start, end, position, easeType);
		}
	}

	public class FadeOut : IModifier
	{
		public int easeType;

		public float start;

		public float end;

		public FadeOut(float start, float end, int easeType)
		{
			this.start = start;
			this.end = end;
			this.easeType = easeType;
		}

		public static IModifier FromString(string data)
		{
			float[] floats = GetFloats(data);
			return new FadeOut(floats[0], floats[1], (int)((floats.Length > 2) ? floats[2] : 0f));
		}

		public void Apply(ElementState state, float position)
		{
			state.level *= ILerp(end, start, position, easeType);
		}
	}

	public class Play : IModifier
	{
		public bool loop;

		public float start;

		public float end;

		public float framesToPlay;

		public float startFrame;

		public Play(float start, float end, float framesToPlay, float startFrame)
		{
			this.start = start;
			this.end = end;
			this.framesToPlay = framesToPlay;
			this.startFrame = startFrame;
		}

		public static IModifier FromString(string data, int mode = 0)
		{
			float[] floats = GetFloats(data);
			return new Play(floats[0], (floats.Length > 1) ? floats[1] : (-1f), (floats.Length > 2) ? floats[2] : (-1f), (floats.Length > 3) ? floats[3] : 0f)
			{
				loop = (mode == 1)
			};
		}

		public void Apply(ElementState state, float position)
		{
			state.useLocalTime = true;
			if (end == -1f)
			{
				end = state.relativeTimeMultiplier + start;
			}
			if (loop)
			{
				state.localTime = Custom.Mod((position - start) / (end - start) * state.relativeTimeMultiplier, state.relativeTimeMultiplier);
			}
			else
			{
				state.localTime = Mathf.InverseLerp(start, end, position) * state.relativeTimeMultiplier;
				state.level *= ((state.localTime > 0f && state.localTime < state.relativeTimeMultiplier) ? 1 : 0);
			}
			if (framesToPlay != -1f)
			{
				state.animationFrame = Mathf.Floor(startFrame + state.localTime * framesToPlay / state.relativeTimeMultiplier);
			}
		}
	}

	public class Rotate : IModifier
	{
		public int easeType;

		public float angle;

		public float start;

		public float end;

		public Rotate(float angle, float start, float end, int easeType)
		{
			this.angle = angle;
			this.start = start;
			this.end = end;
			this.easeType = easeType;
		}

		public static IModifier FromString(string data)
		{
			float[] floats = GetFloats(data);
			return new Rotate(floats[0], (floats.Length > 1) ? floats[1] : 0f, (floats.Length > 2) ? floats[2] : ((floats.Length > 1) ? (floats[1] + defaultOffset) : 0f), (int)((floats.Length > 3) ? floats[3] : 0f));
		}

		public void Apply(ElementState state, float position)
		{
			if (start == end && start == 0f)
			{
				state.rotation += angle;
			}
			else
			{
				state.rotation += angle * ILerp(start, end, position, easeType);
			}
		}
	}

	public class Rand : IModifier
	{
		public float speed;

		public float amountX;

		public float amountY;

		public float timeOffset;

		public int mode;

		public Rand(float speed, float amountX, float amountY, float timeOffset)
		{
			this.speed = speed;
			this.amountX = amountX;
			this.amountY = amountY;
			this.timeOffset = timeOffset;
		}

		public static IModifier FromString(string data, int mode = 0)
		{
			float[] floats = GetFloats(data);
			return new Rand(floats[0], (floats.Length > 1) ? floats[1] : 0.1f, (floats.Length > 2) ? floats[2] : floats[1], (floats.Length > 3) ? floats[3] : 0f)
			{
				mode = mode
			};
		}

		public void Apply(ElementState state, float position)
		{
			if (mode < 2)
			{
				Vector2 vector = new Vector2(FlameJet.AnimatePerlin(position * speed + state.randomOffset * 10000f + timeOffset), FlameJet.AnimatePerlin(position * speed + state.randomOffset * 10000f + 100f + timeOffset));
				if (mode == 1)
				{
					vector.x *= amountX;
					vector.y *= amountY;
					state.size *= vector + Vector2.one;
				}
				else
				{
					vector = vector * 2f - Vector2.one;
					vector.x *= amountX;
					vector.y *= amountY;
					state.pos += vector;
				}
				return;
			}
			float num = FlameJet.AnimatePerlin(position * speed + state.randomOffset * 10000f + timeOffset);
			if (mode == 2)
			{
				state.rotation += Mathf.Lerp(amountX, amountY, num);
			}
			else if (mode == 3)
			{
				float num2 = 1f;
				if (amountX > 0f)
				{
					num2 += num * amountY;
				}
				else if (amountX < 0f)
				{
					num2 /= Mathf.Max(0f, 1f + num * amountY);
				}
				else
				{
					num2 = num * 2f - 1f;
					num2 = ((!(num2 > 0f)) ? (1f + Mathf.Abs(num2 * amountY)) : (1f / Mathf.Max(0f, 1f + num2 * amountY)));
				}
				state.size *= num2;
			}
		}
	}

	public class Rgb : IModifier
	{
		public int easeType;

		public float start;

		public float end;

		public Color col;

		public Rgb(float r1, float g1, float b1, float start, float end, int easeType)
		{
			col = new Color(r1, g1, b1);
			this.start = start;
			this.end = end;
			this.easeType = easeType;
		}

		public static IModifier FromString(string data)
		{
			float[] floats = GetFloats(data);
			return new Rgb(floats[0], floats[1], floats[2], (floats.Length > 3) ? floats[3] : 0f, (floats.Length > 4) ? floats[4] : ((floats.Length > 3) ? (floats[3] + defaultOffset) : 0f), (int)((floats.Length > 5) ? floats[5] : 0f));
		}

		public void Apply(ElementState state, float position)
		{
			float t = ((start == 0f && start == end) ? 1f : ILerp(start, end, position, easeType));
			state.col = Color.Lerp(state.col, col, t);
		}
	}

	public class RgbBlink : IModifier
	{
		public float mult;

		public Color col;

		public RgbBlink(float r, float g, float b, float mult)
		{
			col = new Color(r, g, b);
			this.mult = mult;
		}

		public static IModifier FromString(string data)
		{
			float[] floats = GetFloats(data);
			return new RgbBlink(floats[0], floats[1], floats[2], (floats.Length > 3) ? floats[3] : 1f);
		}

		public void Apply(ElementState state, float position)
		{
			float t = Mathf.Sin((float)state.frameCount * 0.2f * mult) * 0.5f + 0.5f;
			state.col = Color.Lerp(state.col, col, t);
		}
	}

	public class HsvBlink : IModifier
	{
		public float h1;

		public float s1;

		public float v1;

		public float mult;

		public HsvBlink(float h, float s, float v, float mult)
		{
			h1 = h;
			s1 = s;
			v1 = v;
			this.mult = mult;
		}

		public static IModifier FromString(string data)
		{
			float[] floats = GetFloats(data);
			return new HsvBlink(floats[0], floats[1], floats[2], (floats.Length > 3) ? floats[3] : 1f);
		}

		public void Apply(ElementState state, float position)
		{
			Color.RGBToHSV(state.col, out var H, out var S, out var V);
			float t = Mathf.Sin((float)state.frameCount * 0.2f * mult) * 0.5f + 0.5f;
			H = Mathf.Lerp(H, h1, t);
			S = Mathf.Lerp(S, s1, t);
			V = Mathf.Lerp(V, v1, t);
			state.col = Color.HSVToRGB(Custom.Mod(H, 1f), S, V);
		}
	}

	public class Hsv : IModifier
	{
		public int easeType;

		public float h1;

		public float h2;

		public float s1;

		public float s2;

		public float v1;

		public float v2;

		public float start;

		public float end;

		public Hsv(float h1, float s1, float v1, float start, float end, int easeType)
		{
			this.h1 = h1;
			this.s1 = s1;
			this.v1 = v1;
			this.start = start;
			this.end = end;
			this.easeType = easeType;
		}

		public static IModifier FromString(string data)
		{
			float[] floats = GetFloats(data);
			return new Hsv(floats[0], floats[1], floats[2], (floats.Length > 3) ? floats[3] : 0f, (floats.Length > 4) ? floats[4] : ((floats.Length > 3) ? (floats[3] + defaultOffset) : 0f), (int)((floats.Length > 5) ? floats[5] : 0f));
		}

		public void Apply(ElementState state, float position)
		{
			float t = ((start == 0f && start == end) ? 1f : ILerp(start, end, position, easeType));
			Color.RGBToHSV(state.col, out var H, out var S, out var V);
			H = Mathf.Lerp(H, h1, t);
			S = Mathf.Lerp(S, s1, t);
			V = Mathf.Lerp(V, v1, t);
			state.col = Color.HSVToRGB(Custom.Mod(H, 1f), S, V);
		}
	}

	public class Spin : IModifier
	{
		public float anglePerStep;

		public Spin(float anglePerStep)
		{
			this.anglePerStep = anglePerStep;
		}

		public static IModifier FromString(string data)
		{
			return new Spin(GetFloats(data)[0]);
		}

		public void Apply(ElementState state, float position)
		{
			state.rotation += (float)state.frameCount * anglePerStep;
		}
	}

	public class Blink : IModifier
	{
		public float mult;

		public float min;

		public Blink(float frequencyMultiplier, float minimum)
		{
			mult = frequencyMultiplier;
			min = minimum;
		}

		public static IModifier FromString(string data)
		{
			float[] floats = GetFloats(data);
			return new Blink(floats[0], (floats.Length > 1) ? floats[1] : 0f);
		}

		public void Apply(ElementState state, float position)
		{
			state.level *= Mathf.Lerp(min, 1f, Mathf.Sin((float)state.frameCount * 0.2f * mult) * 0.5f + 0.5f);
		}
	}

	public interface IModifier
	{
		void Apply(ElementState state, float position);
	}

	public Vector2 size;

	public Vector2 _size;

	public float globalScale = 1f;

	public int seed;

	public bool needReset;

	public float randomOffset;

	public float randomOffset2;

	public float totalTime;

	public Counter life;

	public int lastLife;

	public Element[] elements;

	public bool shouldStopAtHoldPoint;

	public float holdPoint = -1f;

	public float fadeTime = 20f;

	public Vector4 cropRect;

	public Vector4 cropRectScreen;

	public string shader = "PearlReaderProjection";

	public string container = "GrabShaders";

	public float relativeTimeMultiplier;

	public static float magicScale = 0.0625f;

	public bool debugMode;

	public bool noGlow;

	public static float defaultOffset = 0.0001f;

	public float time => life.normalized * relativeTimeMultiplier;

	public Vector2 sSize => Custom.rainWorld.screenSize;

	public PearlContent(float[] properties, int seed)
	{
		this.seed = seed;
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.InitState(seed);
		randomOffset = UnityEngine.Random.value;
		randomOffset2 = UnityEngine.Random.value;
		UnityEngine.Random.state = state;
		if (properties.Length >= 2)
		{
			_size = new Vector2(properties[0], properties[1]);
		}
		else
		{
			_size = Vector2.one * 100f;
		}
		size = _size * globalScale;
		if (properties.Length >= 3)
		{
			totalTime = properties[2];
		}
		else
		{
			totalTime = 100f;
		}
		relativeTimeMultiplier = ((properties.Length > 3) ? properties[3] : 1f);
		life = new Counter((int)totalTime, 0, countsUp: true);
	}

	public void Reset()
	{
		base.slatedForDeletetion = false;
		size = _size * globalScale;
		for (int i = 0; i < elements.Length; i++)
		{
			if (elements[i] != null)
			{
				elements[i].state.FullReset();
				elements[i].slatedForDeletetion = false;
			}
		}
		shouldStopAtHoldPoint = false;
		life.Reset();
		lastLife = life;
		needReset = false;
	}

	public static string[] LoadPearlProperties(string filePath)
	{
		string[] array = File.ReadAllText(filePath).Split(new string[3] { "\r\n|\r\n", "\r|\r", "\n|\n" }, StringSplitOptions.RemoveEmptyEntries);
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = string.Join("", array[i].Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
		}
		return array;
	}

	public static PearlContent Load(DataPearl.AbstractDataPearl pearl)
	{
		return Load(pearl.dataPearlType.value.ToLowerInvariant(), pearl.ID.RandomSeed);
	}

	public static PearlContent Load(string name, int seed)
	{
		PearlContent pearlContent = TryLoadSingleFile(name, seed);
		if (pearlContent != null)
		{
			return pearlContent;
		}
		return TryLoadFromDirectory(name, seed);
	}

	public static PearlContent TryLoadSingleFile(string name, int seed)
	{
		string text = ResolveFilePath(name);
		if (text == null)
		{
			return null;
		}
		return Load(LoadPearlProperties(text), seed);
	}

	public static string ResolveFilePath(string name)
	{
		string text = AssetManager.ResolveFilePath("pearls" + Path.DirectorySeparatorChar + name + ".txt");
		if (!File.Exists(text))
		{
			return null;
		}
		return text;
	}

	public static PearlContent TryLoadFromDirectory(string name, int seed)
	{
		string path = AssetManager.ResolveDirectory("pearls" + Path.DirectorySeparatorChar + name);
		if (!Directory.Exists(path))
		{
			return null;
		}
		return LoadRandom(Directory.GetFiles(path, "*.txt"), seed);
	}

	public static PearlContent LoadRandom(string[] filePaths, int seed)
	{
		if (filePaths.Length == 0)
		{
			return null;
		}
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.InitState(seed);
		string filePath = filePaths[UnityEngine.Random.Range(0, filePaths.Length)];
		UnityEngine.Random.state = state;
		return Load(LoadPearlProperties(filePath), seed);
	}

	public static PearlContent Load(string[] properties, int seed)
	{
		PearlContent pearlContent = new PearlContent(GetFloats(properties[0]), seed);
		Element[] array = new Element[properties.Length - 1];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = MakeElement(properties[i + 1], pearlContent);
			array[i]?.MovePlayhead(0f);
		}
		pearlContent.elements = array;
		return pearlContent;
	}

	public void PlayInRoom(Room room, Vector2 pos)
	{
		if (needReset)
		{
			Reset();
		}
		base.pos = pos;
		base.room = room;
		for (int i = 0; i < elements.Length; i++)
		{
			if (elements[i] != null)
			{
				room.AddObject(elements[i]);
			}
		}
		room.AddObject(this);
		needReset = true;
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		size = _size * globalScale;
		if (life.isFinished)
		{
			Destroy();
		}
		lastLife = life;
		if (!shouldStopAtHoldPoint || !(time >= holdPoint))
		{
			life.Tick();
		}
		shouldStopAtHoldPoint = false;
		for (int i = 0; i < elements.Length; i++)
		{
			if (elements[i] != null)
			{
				elements[i].MovePlayhead(time);
				CheckIfShouldStopAtHoldPoint(elements[i]);
			}
		}
	}

	public override void Destroy()
	{
		for (int i = 0; i < elements.Length; i++)
		{
			if (elements[i] != null)
			{
				elements[i].RemoveFromRoom();
				elements[i].Destroy();
			}
		}
		RemoveFromRoom();
		base.Destroy();
	}

	public void CheckIfShouldStopAtHoldPoint(Element element)
	{
		if (holdPoint == -1f)
		{
			return;
		}
		if (element is Sound { se: var se } sound)
		{
			if (se != null)
			{
				shouldStopAtHoldPoint = shouldStopAtHoldPoint || (!sound.loop && se.soundStillPlaying);
			}
		}
		else if (element is Dialog dialog && room != null && dialog.lastMessage != null && room.game.cameras[0].hud != null)
		{
			DialogBox dialogBox = room.game.cameras[0].hud.dialogBox;
			if (dialogBox != null && dialogBox.messages != null && dialogBox.messages.Contains(dialog.lastMessage))
			{
				shouldStopAtHoldPoint = true;
			}
		}
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		size = _size * globalScale;
		sLeaser.sprites = new FSprite[2];
		sLeaser.sprites[0] = new FSprite("Futile_White");
		sLeaser.sprites[0].shader = Custom.rainWorld.Shaders["PearlReaderProjectionGlow"];
		sLeaser.sprites[1] = new FSprite("Futile_White");
		sLeaser.sprites[1].shader = Custom.rainWorld.Shaders[shader];
		AddToContainer(sLeaser, rCam, rCam.ReturnFContainer(container));
		cropRectScreen = GetCropRectScreen(cropRect = GetCropRect(pos, _size, rCam.pos), sSize);
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		size = _size * globalScale;
		cropRectScreen = GetCropRectScreen(cropRect = GetCropRect(pos, _size, camPos), sSize);
		Vector2 position = pos - camPos;
		float value = Mathf.Lerp(lastLife, (int)life, timeStacker);
		float a = Mathf.InverseLerp(0f, fadeTime, value) * Mathf.InverseLerp(life.max, (float)life.max - fadeTime, value);
		sLeaser.sprites[0].isVisible = !debugMode && !noGlow;
		if (!debugMode)
		{
			sLeaser.sprites[0].SetPosition(position);
			sLeaser.sprites[0].scaleX = _size.x * magicScale * 1.5f;
			sLeaser.sprites[0].scaleY = _size.y * magicScale * 1.5f;
			sLeaser.sprites[0].color = new Color(position.x / sSize.x, position.y / sSize.y, Mathf.Max(_size.x / sSize.x, _size.y / sSize.y), a);
		}
		sLeaser.sprites[1].SetPosition(position);
		sLeaser.sprites[1].scaleX = _size.x * magicScale * 1.2f;
		sLeaser.sprites[1].scaleY = _size.y * magicScale * 1.2f;
		sLeaser.sprites[1].color = new Color(randomOffset, randomOffset2, 1f, a);
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
	}

	public static Vector4 GetCropRect(Vector2 center, Vector2 size, Vector2 camPos)
	{
		return new Vector4(center.x - size.x * 0.5f - camPos.x, center.y - size.y * 0.5f - camPos.y, center.x + size.x * 0.5f - camPos.x, center.y + size.y * 0.5f - camPos.y);
	}

	public static Vector4 GetCropRectScreen(Vector4 cropRect, Vector2 screenSize)
	{
		cropRect.x /= screenSize.x;
		cropRect.y /= screenSize.y;
		cropRect.z /= screenSize.x;
		cropRect.w /= screenSize.y;
		return cropRect;
	}

	public static float[] GetFloats(string data)
	{
		return (from x in data.Split(',')
			select ParseFloat(x)).ToArray();
	}

	public static float ParseFloat(string s)
	{
		return float.Parse(s, NumberStyles.Any, CultureInfo.InvariantCulture);
	}

	public bool CanUnderstandDialog()
	{
		if (Custom.rainWorld.processManager.currentMainLoop is RainWorldGame rainWorldGame)
		{
			if (rainWorldGame.session is StoryGameSession)
			{
				if (!(rainWorldGame.session as StoryGameSession).saveState.deathPersistentSaveData.theMark)
				{
					if (ModManager.MSC)
					{
						return rainWorldGame.StoryCharacter == MoreSlugcatsEnums.SlugcatStatsName.Saint;
					}
					return false;
				}
				return true;
			}
			return false;
		}
		return false;
	}

	public static Element MakeElement(string data, PearlContent owner)
	{
		string[] array = data.Split(new string[3] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
		Element concreteElement = GetConcreteElement(array[0], owner);
		if (concreteElement == null)
		{
			return null;
		}
		concreteElement.state = new ElementState
		{
			relativeTimeMultiplier = owner.relativeTimeMultiplier
		};
		IModifier[] array2 = new IModifier[array.Length - 1];
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i] = GetConcreteModifier(array[i + 1], concreteElement);
		}
		concreteElement.modifiers = array2;
		return concreteElement;
	}

	public static Element GetConcreteElement(string data, PearlContent owner)
	{
		string[] array = data.Split(':');
		array[0] = array[0].ToLowerInvariant();
		Element element = null;
		if (array[0] == "atlas")
		{
			element = new Atlas(array[1].Split(','));
		}
		else if (array[0] == "image" || array[0] == "img")
		{
			string[] array2 = array[1].Split(',');
			element = new Image(array2[0], array2.Length <= 1);
		}
		else if (array[0] == "sound" || array[0] == "snd")
		{
			element = new Sound(array[1].Split(','));
		}
		else if (array[0] == "soundvisual" || array[0] == "sndvis")
		{
			element = new SoundVisual(array[1].Split(','));
		}
		else if (array[0] == "text" || array[0] == "txt")
		{
			element = new Text(array[1]);
		}
		else if (array[0] == "dialog")
		{
			if (owner.CanUnderstandDialog())
			{
				element = new Dialog(array[1].Split(','));
			}
		}
		else if (array[0] == "conversation" || array[0] == "convo")
		{
			if (owner.CanUnderstandDialog())
			{
				element = new Dialog(array[1].Split(','), isConvo: true);
			}
		}
		else if (array[0] == "holdpoint" || array[0] == "hold")
		{
			owner.holdPoint = ((array.Length > 1) ? ParseFloat(array[1]) : 0.5f);
		}
		if (array[0] == "globalscale")
		{
			owner.globalScale = ((array.Length > 1) ? ParseFloat(array[1]) : 1f);
		}
		else if (array[0] == "debug" || array[0] == "dbg")
		{
			owner.debugMode = true;
			owner.shader = "PearlReaderProjectionDebug";
		}
		else if (array[0] == "noglow")
		{
			owner.debugMode = true;
		}
		else if (array[0] == "opaque")
		{
			owner.shader = "PearlReaderProjectionOpaque";
		}
		else if (array[0] == "shader" && array.Length > 1)
		{
			owner.shader = array[1];
		}
		else if (array[0] == "fadetime" && array.Length > 1)
		{
			owner.fadeTime = Mathf.Clamp(ParseFloat(array[1]) * 40f, 1f, owner.totalTime / 2f);
		}
		if (array[0] == "container" && array.Length > 1)
		{
			owner.container = array[1];
		}
		else if (element != null)
		{
			element.owner = owner;
		}
		return element;
	}

	public static Mesh CreateMesh(FAtlasElement element)
	{
		if (element == null)
		{
			return null;
		}
		Mesh mesh = new Mesh
		{
			name = "PearlContent Mesh"
		};
		Vector2[] uv = new Vector2[4] { element.uvBottomLeft, element.uvTopLeft, element.uvBottomRight, element.uvTopRight };
		mesh.vertices = MaskSource.quadVerts;
		mesh.triangles = MaskSource.quadTris;
		mesh.uv = uv;
		return mesh;
	}

	public static IModifier GetConcreteModifier(string data, Element element)
	{
		string[] array = data.Split(':');
		if (array[0] == "rect")
		{
			return Rect.FromString(array[1]);
		}
		if (array[0] == "fit")
		{
			return Fit.FromString(array[1]);
		}
		if (array[0] == "move" || array[0] == "pos")
		{
			return Move.FromString(array[1]);
		}
		if (array[0] == "level" || array[0] == "vol" || array[0] == "volume" || array[0] == "alpha")
		{
			return Level.FromString(array[1]);
		}
		if (array[0] == "size" || array[0] == "scale")
		{
			return Size.FromString(array[1]);
		}
		if (array[0] == "osc")
		{
			return Osc.FromString(array[1]);
		}
		if (array[0] == "oscsize" || array[0] == "oscscale")
		{
			return Osc.FromString(array[1], 1);
		}
		if (array[0] == "oscrot")
		{
			return Osc.FromString(array[1], 2);
		}
		if (array[0] == "oscrgb")
		{
			return OscColor.FromString(array[1]);
		}
		if (array[0] == "oschsv")
		{
			return OscColor.FromString(array[1], 1);
		}
		if (array[0] == "fadein")
		{
			return FadeIn.FromString(array[1]);
		}
		if (array[0] == "fadeout")
		{
			return FadeOut.FromString(array[1]);
		}
		if (array[0] == "play")
		{
			return Play.FromString(array[1]);
		}
		if (array[0] == "loop")
		{
			return Play.FromString(array[1], 1);
		}
		if (array[0] == "rotate" || array[0] == "rot")
		{
			return Rotate.FromString(array[1]);
		}
		if (array[0] == "spin")
		{
			return Spin.FromString(array[1]);
		}
		if (array[0] == "blink")
		{
			return Blink.FromString(array[1]);
		}
		if (array[0] == "rgb")
		{
			return Rgb.FromString(array[1]);
		}
		if (array[0] == "hsv")
		{
			return Hsv.FromString(array[1]);
		}
		if (array[0] == "rgbblink" || array[0] == "blinkrgb")
		{
			return RgbBlink.FromString(array[1]);
		}
		if (array[0] == "hsvblink" || array[0] == "blinkhsv")
		{
			return HsvBlink.FromString(array[1]);
		}
		if (array[0] == "randmove" || array[0] == "rndmove")
		{
			return Rand.FromString(array[1]);
		}
		if (array[0] == "randscale" || array[0] == "rndscale" || array[0] == "randsize" || array[0] == "rndsize")
		{
			return Rand.FromString(array[1], 1);
		}
		if (array[0] == "randrot" || array[0] == "rndrot")
		{
			return Rand.FromString(array[1], 2);
		}
		if (array[0] == "randzoom" || array[0] == "rndzoom")
		{
			return Rand.FromString(array[1], 3);
		}
		if (array[0] == "shader" && array.Length > 1 && element is Image image)
		{
			image.shader = array[1];
		}
		return null;
	}

	public static float ILerp(float start, float end, float t, int easeType)
	{
		float num = Mathf.InverseLerp(start, end, t);
		switch (easeType)
		{
		case 1:
			num = Custom.LerpQuadEaseIn(0f, 1f, num);
			break;
		case 2:
			num = Custom.LerpQuadEaseOut(0f, 1f, num);
			break;
		case 3:
			num = 0.5f - Mathf.Cos(Mathf.InverseLerp(0f, 1f, num) * (float)Math.PI) * 0.5f;
			break;
		}
		return num;
	}
}
