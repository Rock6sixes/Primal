using System.IO;
using RWCustom;
using UnityEngine;

namespace DevInterface;

public class WeaverSpotRepresentation : PlacedObjectRepresentation
{
	public int lineSprite;

	public int previewSprite;

	public Handle directionHandle;

	public const float bodyLength = 460f;

	public PlacedObject.ResizableObjectData Data => pObj.data as PlacedObject.ResizableObjectData;

	public WeaverSpotRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, string name)
		: base(owner, IDstring, parentNode, pObj, name)
	{
		directionHandle = new Handle(owner, "Direction_Handle", this, new Vector2(0f, 100f));
		directionHandle.pos = Data.handlePos;
		subNodes.Add(directionHandle);
		if (!Futile.atlasManager.DoesContainAtlas("weaver-outline"))
		{
			Texture2D texture2D = new Texture2D(1, 1, TextureFormat.ARGB32, mipChain: false)
			{
				filterMode = FilterMode.Point
			};
			string text = AssetManager.ResolveFilePath("Illustrations" + Path.DirectorySeparatorChar + "weaver-outline.png");
			AssetManager.SafeWWWLoadTexture(ref texture2D, "file:///" + text, clampWrapMode: false, crispPixels: false);
			HeavyTexturesCache.LoadAndCacheAtlasFromTexture("weaver-outline", texture2D, textureFromAsset: false);
		}
		previewSprite = fSprites.Count;
		fSprites.Add(new FSprite("weaver-outline")
		{
			color = RainWorld.SaturatedGold,
			anchorX = 0.54330707f,
			anchorY = 0.9241517f
		});
		owner.placedObjectsContainer.AddChild(fSprites[previewSprite]);
		lineSprite = fSprites.Count;
		fSprites.Add(new FSprite("pixel")
		{
			anchorY = 0f
		});
		owner.placedObjectsContainer.AddChild(fSprites[lineSprite]);
	}

	public override void Refresh()
	{
		base.Refresh();
		directionHandle.pos = directionHandle.pos.normalized * 460f;
		Data.handlePos = directionHandle.pos;
		MoveSprite(lineSprite, absPos);
		fSprites[lineSprite].scaleY = directionHandle.pos.magnitude;
		fSprites[lineSprite].rotation = Custom.VecToDeg(directionHandle.pos);
		MoveSprite(previewSprite, absPos);
		fSprites[previewSprite].rotation = Custom.VecToDeg(-directionHandle.pos);
	}
}
