using UnityEngine;

public class WaterTriangleMesh : FSprite
{
	public TriangleMesh.Triangle[] triangles;

	public Vector2[] vertices;

	public Color[] verticeColors;

	public bool customColor;

	public WaterTriangleMesh(string imageName, TriangleMesh.Triangle[] tris, bool customColor)
	{
		this.customColor = customColor;
		triangles = tris;
		int num = 2;
		for (int i = 0; i < triangles.Length; i++)
		{
			if (triangles[i].a > num)
			{
				num = triangles[i].a;
			}
			if (triangles[i].b > num)
			{
				num = triangles[i].b;
			}
			if (triangles[i].c > num)
			{
				num = triangles[i].c;
			}
		}
		vertices = new Vector2[num + 1];
		for (int j = 0; j < num; j++)
		{
			vertices[j] = new Vector2(0f, 0f);
		}
		if (customColor)
		{
			verticeColors = new Color[num + 1];
			for (int k = 0; k < verticeColors.Length; k++)
			{
				verticeColors[k] = _alphaColor;
			}
		}
		Init(FFacetType.Triangle, Futile.atlasManager.GetElementWithName(imageName), triangles.Length);
		_isAlphaDirty = true;
		UpdateLocalVertices();
	}

	public void MoveVertice(int ind, Vector2 pos)
	{
		vertices[ind] = pos;
		Refresh();
	}

	public void Refresh()
	{
		_isMeshDirty = true;
	}

	public override void PopulateRenderLayer()
	{
		if (!_isOnStage || _firstFacetIndex == -1)
		{
			return;
		}
		_isMeshDirty = false;
		int num = _firstFacetIndex * 3;
		Vector3[] array = _renderLayer.vertices;
		Vector2[] uvs = _renderLayer.uvs;
		Vector2[] array2 = new Vector2[6];
		Color[] colors = _renderLayer.colors;
		Vector2 uvBottomLeft = _element.uvBottomLeft;
		Vector2 vector = (array2[3] = _element.uvTopRight);
		array2[4] = uvBottomLeft;
		array2[5] = vector;
		array2[0] = uvBottomLeft;
		array2[1] = vector;
		array2[2] = uvBottomLeft;
		if (customColor)
		{
			for (int i = 0; i < triangles.Length; i++)
			{
				for (int j = 0; j < 3; j++)
				{
					colors[num + i * 3 + j] = verticeColors[triangles[i].GetAt(j)];
				}
			}
		}
		else
		{
			for (int k = 0; k < triangles.Length * 3; k++)
			{
				colors[num + k] = _alphaColor;
			}
		}
		int num2 = 0;
		for (int l = 0; l < triangles.Length; l++)
		{
			_concatenatedMatrix.ApplyVector3FromLocalVector2(ref array[num + num2], vertices[triangles[l].a], 0f);
			uvs[num + num2] = array2[l % 2 * 3];
			num2++;
			_concatenatedMatrix.ApplyVector3FromLocalVector2(ref array[num + num2], vertices[triangles[l].b], 0f);
			uvs[num + num2] = array2[1 + l % 2 * 3];
			num2++;
			_concatenatedMatrix.ApplyVector3FromLocalVector2(ref array[num + num2], vertices[triangles[l].c], 0f);
			uvs[num + num2] = array2[2 + l % 2 * 3];
			num2++;
		}
		_renderLayer.HandleVertsChange();
	}
}
