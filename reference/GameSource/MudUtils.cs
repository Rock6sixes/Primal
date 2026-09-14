using UnityEngine;

public static class MudUtils
{
	public class MudOverlaySprite : CustomFSprite
	{
		public Vector2 seed;

		public bool flipX;

		public bool flipY;

		public float packedColor;

		public MudOverlaySprite(string imageName)
			: base(imageName)
		{
			seed = new Vector2(Random.value, Random.value);
		}

		public override void PopulateRenderLayer()
		{
			ref Color reference = ref verticeColors[0];
			ref Color reference2 = ref verticeColors[1];
			ref Color reference3 = ref verticeColors[2];
			ref Color reference4 = ref verticeColors[3];
			Vector2 vector = base.element.sourceSize / 120f;
			if (flipX)
			{
				vector.x = 0f - vector.x;
			}
			if (flipY)
			{
				vector.y = 0f - vector.y;
			}
			reference = new Color(seed.x - vector.x, seed.y + vector.y, packedColor, base.concatenatedAlpha * color.a);
			reference2 = new Color(seed.x + vector.x, seed.y + vector.y, packedColor, base.concatenatedAlpha * color.a);
			reference3 = new Color(seed.x + vector.x, seed.y - vector.y, packedColor, base.concatenatedAlpha * color.a);
			reference4 = new Color(seed.x - vector.x, seed.y - vector.y, packedColor, base.concatenatedAlpha * color.a);
			base.PopulateRenderLayer();
		}
	}

	public static void MakeMudSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, int firstMudSprite, params int[] sourceSprites)
	{
		for (int i = 0; i < sourceSprites.Length; i++)
		{
			sLeaser.sprites[firstMudSprite + i] = MakeMudSprite(rCam, sLeaser.sprites[sourceSprites[i]]);
		}
	}

	public static FSprite MakeMudSprite(RoomCamera rCam, FSprite source)
	{
		FSprite fSprite;
		if (source is TriangleMesh triangleMesh)
		{
			TriangleMesh triangleMesh2 = new TriangleMesh(triangleMesh.element.name, triangleMesh.triangles, customColor: true);
			triangleMesh2.vertices = triangleMesh.vertices;
			triangleMesh2.UVvertices = triangleMesh.UVvertices;
			for (int i = 0; i < triangleMesh2.verticeColors.Length; i++)
			{
				Vector2 vector = triangleMesh2.UVvertices[i];
				triangleMesh2.verticeColors[i] = new Color(vector.x, vector.y, 0f);
			}
			fSprite = triangleMesh2;
		}
		else
		{
			fSprite = new MudOverlaySprite(source.element.name);
		}
		fSprite.shader = rCam.room.game.rainWorld.Shaders["MudOverlay"];
		return fSprite;
	}

	public static void SetUpTail(FSprite sprite, float length, float width)
	{
		Color color = new Color(Random.value, Random.value, 0f, 0f);
		float num = 1f / 120f;
		if (!(sprite is TriangleMesh triangleMesh))
		{
			return;
		}
		for (int i = 0; i < triangleMesh.verticeColors.Length; i += 2)
		{
			float num2 = (float)i / (float)(triangleMesh.verticeColors.Length - 2);
			triangleMesh.verticeColors[i] = new Color(num2 * length * num, (0f - num2) * width * num, 0f) + color;
			if (i < triangleMesh.verticeColors.Length - 1)
			{
				triangleMesh.verticeColors[i + 1] = new Color(num2 * length * num, num2 * width * num, 0f) + color;
			}
		}
	}

	public static void SetUpTube(FSprite sprite, float length, float width)
	{
		Color color = new Color(Random.value, Random.value, 0f, 0f);
		float num = 1f / 120f;
		if (!(sprite is TriangleMesh triangleMesh))
		{
			return;
		}
		for (int i = 0; i < triangleMesh.verticeColors.Length; i += 2)
		{
			float num2 = (float)i / (float)(triangleMesh.verticeColors.Length - 2);
			triangleMesh.verticeColors[i] = new Color(num2 * length * num, (0f - width) * num, 0f) + color;
			if (i < triangleMesh.verticeColors.Length - 1)
			{
				triangleMesh.verticeColors[i + 1] = new Color(num2 * length * num, width * num, 0f) + color;
			}
		}
	}

	public static void UpdateMudSprite(Creature crit, FSprite dst, FSprite src)
	{
		dst.isVisible = crit.muddy > 0 && src.isVisible;
		if (!dst.isVisible)
		{
			return;
		}
		dst.SetPosition(src.GetPosition());
		dst.SetAnchor(src.GetAnchor());
		dst.element = src.element;
		dst.scaleX = src.scaleX;
		dst.scaleY = src.scaleY;
		dst.rotation = src.rotation;
		float num = PackColor(crit.mudColor);
		float num2 = (float)crit.muddy / 2400f;
		if (src is TriangleMesh triangleMesh && dst is TriangleMesh triangleMesh2)
		{
			triangleMesh2.vertices = triangleMesh.vertices;
			for (int i = 0; i < triangleMesh2.verticeColors.Length; i++)
			{
				triangleMesh2.verticeColors[i].b = num;
				triangleMesh2.verticeColors[i].a = num2;
			}
			triangleMesh2.Refresh();
		}
		else if (dst is MudOverlaySprite mudOverlaySprite)
		{
			FAtlasElement element = src.element;
			Rect rect = default(Rect);
			rect.width = element.sourceSize.x;
			rect.height = element.sourceSize.y;
			rect.x = (0f - src.anchorX) * rect.width;
			rect.y = (0f - src.anchorY) * rect.height;
			float width = element.sourceRect.width;
			float height = element.sourceRect.height;
			float num3 = rect.x + element.sourceRect.x;
			float num4 = rect.y + (rect.height - element.sourceRect.y - element.sourceRect.height);
			Vector2[] vertices = mudOverlaySprite.vertices;
			vertices[0] = new Vector2(num3, num4 + height);
			vertices[1] = new Vector2(num3 + width, num4 + height);
			vertices[2] = new Vector2(num3 + width, num4);
			vertices[3] = new Vector2(num3, num4);
			mudOverlaySprite.flipX = src.scaleX < 0f;
			mudOverlaySprite.flipY = src.scaleY < 0f;
			mudOverlaySprite.packedColor = num;
			mudOverlaySprite.alpha = num2;
		}
	}

	public static void UpdateAndLayerMudSprite(Creature crit, FSprite dst, FSprite src)
	{
		if (dst.container != src.container)
		{
			dst.RemoveFromContainer();
			src.container?.AddChildAtIndex(dst, src.container.GetChildIndex(src) + 1);
		}
		UpdateMudSprite(crit, dst, src);
	}

	public static float PackColor(Color color)
	{
		Color32 color2 = color;
		for (int i = 0; i < 3; i++)
		{
			if (color2[i] == byte.MaxValue)
			{
				color2[i] = 254;
			}
		}
		return (float)((double)(int)color2.r / 255.0 + (double)(int)color2.g / 65025.0 + (double)(int)color2.b / 16581375.0);
	}
}
