using RWCustom;
using UnityEngine;

public class BezierMesh : TriangleMesh
{
	public bool pointyTip;

	public Vector2 _posB;

	public Vector2 _handleA;

	public Vector2 _handleB;

	public float _length;

	public float _thickness = 1f;

	public int segments { get; set; }

	public Vector2 posB
	{
		get
		{
			return _posB;
		}
		set
		{
			_isMatrixDirty |= value != _posB;
			_posB = value;
		}
	}

	public Vector2 handleA
	{
		get
		{
			return _handleA;
		}
		set
		{
			_isMatrixDirty |= value != _handleA;
			_handleA = value;
		}
	}

	public Vector2 handleB
	{
		get
		{
			return _handleB;
		}
		set
		{
			_isMatrixDirty |= value != _handleB;
			_handleB = value;
		}
	}

	public float length
	{
		get
		{
			if (!_isMatrixDirty)
			{
				return _length;
			}
			return Custom.BezierLength(Vector2.zero, Vector2.zero + handleA, posB, posB + handleB, segments);
		}
		set
		{
			_length = value;
		}
	}

	public float thickness
	{
		get
		{
			return _thickness;
		}
		set
		{
			_isMatrixDirty |= value != _thickness;
			_thickness = value;
		}
	}

	public BezierMesh(int segments, bool customColor, bool pointyTip, string imageName = "Futile_White", bool atlasedImage = false)
		: base(imageName, new Triangle[1], customColor, atlasedImage)
	{
		this.pointyTip = pointyTip;
		ChangeSegments(segments);
	}

	public override void PopulateRenderLayer()
	{
		length = 0f;
		Vector2 vector = Vector2.zero;
		for (int i = 0; i < segments; i++)
		{
			float f = (float)(i + 1) / (float)segments;
			Vector2 vector2 = Custom.Bezier(Vector2.zero, Vector2.zero + handleA, posB, posB + handleB, f);
			Vector2 vector3 = Custom.PerpendicularVector((vector - vector2).normalized);
			MoveVertice(i * 4, vector - vector3 * thickness);
			MoveVertice(i * 4 + 1, vector + vector3 * thickness);
			MoveVertice(i * 4 + 2, vector2 - vector3 * thickness);
			MoveVertice(i * 4 + 3, vector2 + vector3 * thickness);
			length += Vector2.Distance(vector, vector2);
			vector = vector2;
		}
		base.PopulateRenderLayer();
	}

	public void ChangeSegments(int newSegments)
	{
		if (newSegments >= 1)
		{
			segments = newSegments;
			TriangleMesh triangleMesh = TriangleMesh.MakeLongMesh(segments, pointyTip, customColor);
			triangles = triangleMesh.triangles;
			vertices = triangleMesh.vertices;
			UVvertices = triangleMesh.UVvertices;
			verticeColors = triangleMesh.verticeColors;
			_numberOfFacetsNeeded = triangles.Length;
			_isMatrixDirty = true;
			if (_isOnStage)
			{
				_stage.HandleFacetsChanged();
			}
		}
	}
}
