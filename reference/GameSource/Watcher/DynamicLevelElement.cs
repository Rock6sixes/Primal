using UnityEngine;

namespace Watcher;

public class DynamicLevelElement : UpdatableAndDeletable, IDrawable, INotifyWhenRoomUnloaded
{
	public Vector2 pos;

	public Vector2 scale;

	public float rotation;

	public MaskSource element;

	public Mesh mesh;

	public Texture texture;

	public float depthOffset;

	public float paramZ;

	public float paramW;

	public bool propsChanged;

	public string shaderOverride;

	public static bool levelCombinerAdded;

	public bool dontSetProps;

	public bool dontDrawUpdate;

	public bool dontApplyColors;

	public int setDepthOffset
	{
		get
		{
			return (int)(depthOffset * 60f - 30f);
		}
		set
		{
			depthOffset = Mathf.Clamp01(((float)value + 30f) / 60f);
			propsChanged = true;
		}
	}

	public float setParamZ
	{
		set
		{
			if (paramZ != value)
			{
				paramZ = value;
				propsChanged = true;
			}
		}
	}

	public float setParamW
	{
		set
		{
			if (paramW != value)
			{
				paramW = value;
				propsChanged = true;
			}
		}
	}

	public bool isVisible => element?.isVisible ?? false;

	public RoomCamera camera => room.game.cameras[0];

	public DynamicLevelElement(Vector2 pos, Vector2 scale, Texture texture = null, int depthOffset = 0, Mesh mesh = null, string shaderOverride = null)
	{
		this.pos = pos;
		this.scale = scale;
		this.texture = texture;
		this.mesh = mesh;
		setDepthOffset = depthOffset;
		this.shaderOverride = shaderOverride;
		if (Camera.main.depthTextureMode == DepthTextureMode.None)
		{
			Camera.main.depthTextureMode = DepthTextureMode.Depth;
		}
	}

	public override void Destroy()
	{
		base.Destroy();
		mesh = null;
		if (element != null)
		{
			element.DeleteMe();
		}
	}

	public void RoomUnloaded()
	{
		Destroy();
	}

	public void SetProps()
	{
		if (element != null && propsChanged && !dontSetProps)
		{
			element.SetProperty(1, depthOffset);
			element.SetProperty(2, paramZ);
			element.SetProperty(3, paramW);
			propsChanged = false;
		}
	}

	public override void Update(bool eu)
	{
		if (room.BeingViewed)
		{
			if (element == null)
			{
				element = MaskMaker.MakeSource("DynamicLevelElementGrab", shaderOverride ?? "DynamicLevelElement", isSecondLayer: false, mesh, pos, Vector3.back * rotation, scale);
				if (dontApplyColors)
				{
					element.applyColor = false;
				}
				Texture texture = this.texture;
				if ((object)texture != null)
				{
					element.SetTexture(texture);
				}
				SetProps();
			}
			else
			{
				SetProps();
				element.SetPos(pos);
				element.SetScale(scale);
				element.SetRotation(Vector3.back * rotation);
			}
			AddLevelCombiner(camera);
		}
		else if (element != null)
		{
			element = null;
		}
	}

	public static void AddLevelCombiner(RoomCamera camera)
	{
		if (!levelCombinerAdded && !camera.levelTexCombiner.bufferIDs.Contains("DynamicLevelElement"))
		{
			camera.levelTexCombiner.AddPass(RenderTexture.GetTemporary(1, 1), new Material(Shader.Find("Futile/DynamicLevelElementCombiner")), "DynamicLevelElement");
			levelCombinerAdded = true;
		}
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		if (!dontDrawUpdate)
		{
			element?.DrawUpdate(timeStacker, rCam, camPos);
		}
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
	}
}
