using System;
using UnityEngine;

public class CircularSprite : FSprite
{
	public int segmentCount;

	public CircularSprite(string elementName, int segments = 64)
	{
		_facetTypeQuad = false;
		segmentCount = segments;
		Init(FFacetType.Triangle, Futile.atlasManager.GetElementWithName(elementName), segmentCount);
		_isAlphaDirty = true;
		UpdateLocalVertices();
	}

	public override void PopulateRenderLayer()
	{
		if (_isOnStage && _firstFacetIndex != -1)
		{
			_isMeshDirty = false;
			Vector3[] vertices = _renderLayer.vertices;
			Vector2[] uvs = _renderLayer.uvs;
			Color[] colors = _renderLayer.colors;
			int num = _firstFacetIndex * 3;
			int num2 = segmentCount * 3;
			_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[num], _localRect.center, _meshZ);
			uvs[num] = _element.uvRect.center;
			for (int i = 0; i < segmentCount; i++)
			{
				float f = (float)(i * 2) * (float)Math.PI / (float)segmentCount;
				Vector2 uv = new Vector2(Mathf.Cos(f) + 1f, Mathf.Sin(f) + 1f) * 0.5f;
				int num3 = num + i * 3 + 1;
				_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[num3], UnwrapUV(uv, _localRect), _meshZ);
				uvs[num3] = UnwrapUV(uv, _element.uvRect);
			}
			for (int j = 0; j < num2; j += 3)
			{
				vertices[num + j] = vertices[num];
				uvs[num + j] = uvs[num];
				vertices[num + j + 2] = vertices[num + (j + 4) % num2];
				uvs[num + j + 2] = uvs[num + (j + 4) % num2];
			}
			for (int k = 0; k < num2; k++)
			{
				colors[num + k] = _alphaColor;
			}
			_renderLayer.HandleVertsChange();
		}
	}

	public static Vector2 UnwrapUV(Vector2 uv, Rect rect)
	{
		return new Vector2(Mathf.Lerp(rect.xMin, rect.xMax, uv.x), Mathf.Lerp(rect.yMin, rect.yMax, uv.y));
	}
}
