using RWCustom;
using UnityEngine;

namespace Watcher;

public class MaskSource
{
	public float magicScale = 16f;

	public Material material;

	public bool hasCustomParameter;

	public Vector2 oldPos;

	public Vector2 pos;

	public Vector3 oldRotation;

	public Vector3 rotation;

	public Color vertexColor = Color.black;

	public Vector3 oldScale;

	public Vector3 scale;

	public bool interpolate = true;

	public int interpolateDelay = 10;

	public bool vertexColorChanged = true;

	public MeshFilter meshFilter;

	public MeshRenderer meshRenderer;

	public bool applyColor = true;

	public Mesh _mesh;

	public Mesh myCreatedMesh;

	public GameObject obj;

	public static Vector3[] quadVerts = new Vector3[4]
	{
		new Vector3(-0.5f, -0.5f, 0f),
		new Vector3(-0.5f, 0.5f, 0f),
		new Vector3(0.5f, -0.5f, 0f),
		new Vector3(0.5f, 0.5f, 0f)
	};

	public static int[] quadTris = new int[6] { 0, 2, 1, 2, 3, 1 };

	public static Vector2[] quadUV = new Vector2[4]
	{
		Vector2.zero,
		Vector2.up,
		Vector2.right,
		Vector2.one
	};

	public bool beingDeleted { get; set; }

	public bool isBaseNode { get; set; }

	public bool isVisible
	{
		get
		{
			if (!beingDeleted)
			{
				return meshRenderer?.isVisible ?? false;
			}
			return false;
		}
	}

	public Vector3 setGameObjectPos
	{
		set
		{
			if (!(obj == null))
			{
				obj.transform.localPosition = value;
			}
		}
	}

	public Vector3 setGameObjectRotation
	{
		set
		{
			if (!(obj == null))
			{
				obj.transform.localRotation = Quaternion.Euler(value);
			}
		}
	}

	public Vector3 setGameObjectScale
	{
		set
		{
			if (!(obj == null))
			{
				value *= magicScale;
				obj.transform.localScale = value;
			}
		}
	}

	public bool IsVisible
	{
		get
		{
			if (!(obj == null))
			{
				return false;
			}
			return obj.activeSelf;
		}
		set
		{
			obj?.SetActive(value);
		}
	}

	public static RoomCamera Camera => (Custom.rainWorld.processManager.currentMainLoop as RainWorldGame).cameras[0];

	public void SetPos(Vector2 position, bool initialize = false)
	{
		oldPos = (initialize ? position : pos);
		pos = position;
	}

	public void SetRotation(Vector3 rotation, bool initialize = false)
	{
		oldRotation = (initialize ? rotation : this.rotation);
		this.rotation = rotation;
	}

	public void SetScale(Vector3 scale, bool initialize = false)
	{
		oldScale = (initialize ? scale : this.scale);
		this.scale = scale;
	}

	public MaskSource(Vector2 pos, Vector3 rotation, Vector3 scale, Material material, Mesh mesh = null)
	{
		this.material = material;
		_mesh = mesh;
		CreateGameObject();
		SetPos(pos, initialize: true);
		SetScale(scale, initialize: true);
		SetRotation(rotation, initialize: true);
		interpolate = false;
	}

	public MaskSource(Material material)
	{
		this.material = material;
		CreateGameObject();
		isBaseNode = true;
		SetPos(new Vector2(Camera.sSize.x * 0.5f, Camera.sSize.y * 0.5f), initialize: true);
		SetScale(Vector3.one * (Camera.sSize.x * 0.065f + 8f), initialize: true);
	}

	public void DrawUpdate(float timeStacker, RoomCamera rCam, Vector2 camPos)
	{
		if (applyColor)
		{
			ApplyVertexColor();
		}
		if (interpolateDelay > 0)
		{
			interpolate = false;
			interpolateDelay--;
		}
		else
		{
			interpolate = true;
		}
		Vector2 vector = pos;
		Vector3 vector2 = scale;
		Vector3 vector3 = rotation;
		if (interpolate)
		{
			vector2 = Vector3.Lerp(oldScale, scale, timeStacker);
			vector.x = Mathf.Lerp(oldPos.x, pos.x, timeStacker);
			vector.y = Mathf.Lerp(oldPos.y, pos.y, timeStacker);
			vector3 = new Vector3(Mathf.LerpAngle(oldRotation.x, rotation.x, timeStacker), Mathf.LerpAngle(oldRotation.y, rotation.y, timeStacker), Mathf.LerpAngle(oldRotation.z, rotation.z, timeStacker));
		}
		if (!isBaseNode)
		{
			vector -= camPos;
		}
		setGameObjectRotation = vector3;
		setGameObjectPos = vector;
		setGameObjectScale = vector2;
	}

	public void CreateGameObject()
	{
		obj = new GameObject();
		obj.name = material.name;
		meshRenderer = obj.AddComponent<MeshRenderer>();
		meshRenderer.material = material;
		meshFilter = obj.AddComponent<MeshFilter>();
		if (_mesh == null)
		{
			myCreatedMesh = CreateQuadMesh();
			meshFilter.sharedMesh = myCreatedMesh;
		}
		else
		{
			meshFilter.sharedMesh = _mesh;
		}
	}

	public void ApplyVertexColor()
	{
		if (vertexColorChanged && obj != null)
		{
			Mesh sharedMesh = meshFilter.sharedMesh;
			Color32[] array = new Color32[sharedMesh.vertexCount];
			for (int num = sharedMesh.vertexCount - 1; num >= 0; num--)
			{
				array[num] = vertexColor;
			}
			sharedMesh.SetColors(array);
			vertexColorChanged = false;
		}
	}

	public void SetProperty(int index, float value)
	{
		if (vertexColor[index] != value)
		{
			vertexColor[index] = value;
			vertexColorChanged = true;
		}
	}

	public void SetProperty(string name, float value)
	{
		if (!beingDeleted)
		{
			if (!hasCustomParameter)
			{
				hasCustomParameter = true;
			}
			meshRenderer.material.SetFloat(name, value);
		}
	}

	public void SetProperty(string name, Vector4 value)
	{
		if (!beingDeleted)
		{
			if (!hasCustomParameter)
			{
				hasCustomParameter = true;
			}
			meshRenderer.material.SetVector(name, value);
		}
	}

	public void SetTexture(Texture texture)
	{
		if (!beingDeleted)
		{
			if (!hasCustomParameter)
			{
				hasCustomParameter = true;
			}
			meshRenderer.material.SetTexture("_MainTex", texture);
		}
	}

	public void UpdateQueue()
	{
		if (!beingDeleted && hasCustomParameter)
		{
			meshRenderer.material.renderQueue = material.renderQueue;
		}
	}

	public void DeleteMe()
	{
		beingDeleted = true;
		if (!(obj == null))
		{
			obj.SetActive(value: false);
			_mesh = null;
			meshFilter.sharedMesh = null;
			Object.Destroy(obj);
			obj = null;
			if (myCreatedMesh != null)
			{
				Object.Destroy(myCreatedMesh);
				myCreatedMesh = null;
			}
		}
	}

	public static Mesh CreateQuadMesh()
	{
		return new Mesh
		{
			name = "MaskSource Quad Mesh",
			vertices = quadVerts,
			triangles = quadTris,
			uv = quadUV
		};
	}

	public static int[] GridTriangles(int h, int w)
	{
		int[] array = new int[h * w * 2 * 3];
		for (int i = 0; i < h; i++)
		{
			for (int j = 0; j < w; j++)
			{
				int num = (i * w + j) * 6;
				array[num] = i * (w + 1) + j;
				array[num + 1] = array[num] + 1;
				array[num + 2] = array[num] + 1 + w;
				array[num + 3] = array[num] + 1;
				array[num + 4] = array[num] + 1 + w + 1;
				array[num + 5] = array[num] + 1 + w;
			}
		}
		return array;
	}
}
