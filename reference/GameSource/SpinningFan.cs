using System.IO;
using RWCustom;
using UnityEngine;
using Watcher;

public class SpinningFan : UpdatableAndDeletable
{
	public PlacedObject pObj;

	public PlacedObject.FanData data;

	public float speed;

	public DynamicLevelElement FanElement;

	public SpinningFan(PlacedObject placedObject, Room currentRoom, string assetName, float scale)
	{
		pObj = placedObject;
		data = pObj.data as PlacedObject.FanData;
		room = currentRoom;
		if (!Futile.atlasManager.DoesContainAtlas(assetName))
		{
			Texture2D texture2D = new Texture2D(1, 1, TextureFormat.ARGB32, mipChain: false);
			string text = AssetManager.ResolveFilePath("Illustrations" + Path.DirectorySeparatorChar + assetName + ".png");
			AssetManager.SafeWWWLoadTexture(ref texture2D, "file:///" + text, clampWrapMode: true, crispPixels: true);
			HeavyTexturesCache.LoadAndCacheAtlasFromTexture(assetName, texture2D, textureFromAsset: false);
		}
		FanElement = new DynamicLevelElement(pObj.pos, new Vector2(scale, scale), Futile.atlasManager.GetAtlasWithName(assetName).texture, Mathf.FloorToInt(data.depth * 30f));
		FanElement.rotation = Random.value * 360f;
		room.AddObject(FanElement);
	}

	public override void Update(bool eu)
	{
		float num = Mathf.Lerp(-10f, 10f, data.speed);
		if (room.world.rainCycle.brokenAntiGrav != null)
		{
			num = ((room.world.rainCycle.brokenAntiGrav.CurrentLightsOn > 0f) ? num : 0f);
		}
		speed = Custom.LerpAndTick(speed, num, 0.035f, 0.0008f);
		FanElement.pos = pObj.pos;
		FanElement.rotation += speed * 0.1f;
		FanElement.setDepthOffset = Mathf.FloorToInt(data.depth * 30f);
		base.Update(eu);
	}
}
