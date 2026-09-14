using UnityEngine;

public class VertexColorSprite : FSprite
{
	public Color[] verticeColors;

	public VertexColorSprite(string elementName, bool quadType = true)
		: base(elementName, quadType)
	{
		verticeColors = new Color[4];
	}

	public override void PopulateRenderLayer()
	{
		if (_isOnStage && _firstFacetIndex != -1)
		{
			_isMeshDirty = false;
			Vector3[] vertices = _renderLayer.vertices;
			Vector2[] uvs = _renderLayer.uvs;
			Color[] colors = _renderLayer.colors;
			if (_facetTypeQuad)
			{
				int num = _firstFacetIndex * 4;
				int num2 = num + 1;
				int num3 = num + 2;
				int num4 = num + 3;
				_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[num], _localVertices[0], _meshZ);
				_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[num2], _localVertices[1], _meshZ);
				_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[num3], _localVertices[2], _meshZ);
				_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[num4], _localVertices[3], _meshZ);
				uvs[num] = _element.uvTopLeft;
				uvs[num2] = _element.uvTopRight;
				uvs[num3] = _element.uvBottomRight;
				uvs[num4] = _element.uvBottomLeft;
				colors[num] = verticeColors[0];
				colors[num2] = verticeColors[1];
				colors[num3] = verticeColors[2];
				colors[num4] = verticeColors[3];
			}
			else
			{
				int num5 = _firstFacetIndex * 3;
				int num6 = num5 + 1;
				int num7 = num5 + 2;
				int num8 = num5 + 3;
				int num9 = num5 + 4;
				int num10 = num5 + 5;
				_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[num5], _localVertices[0], _meshZ);
				_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[num6], _localVertices[1], _meshZ);
				_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[num7], _localVertices[2], _meshZ);
				_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[num8], _localVertices[0], _meshZ);
				_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[num9], _localVertices[2], _meshZ);
				_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[num10], _localVertices[3], _meshZ);
				uvs[num5] = _element.uvTopLeft;
				uvs[num6] = _element.uvTopRight;
				uvs[num7] = _element.uvBottomRight;
				uvs[num8] = _element.uvTopLeft;
				uvs[num9] = _element.uvBottomRight;
				uvs[num10] = _element.uvBottomLeft;
				colors[num5] = verticeColors[0];
				colors[num6] = verticeColors[1];
				colors[num7] = verticeColors[2];
				colors[num8] = verticeColors[0];
				colors[num9] = verticeColors[2];
				colors[num10] = verticeColors[3];
			}
			_renderLayer.HandleVertsChange();
		}
	}
}
