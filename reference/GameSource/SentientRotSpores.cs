using System;
using System.Collections.Generic;
using UnityEngine;
using Watcher;

public class SentientRotSpores : UpdatableAndDeletable, IDrawable, INotifyWhenRoomUnloaded
{
	public struct Particle
	{
		public Vector2 position;

		public Vector2 previousPosition;

		public float rotation;

		public float previousRotation;

		public float yRotation;

		public float previousYRotation;

		public int lifetime;

		public Particle(Vector2 position)
		{
			this.position = position;
			previousPosition = this.position;
			rotation = UnityEngine.Random.value * (float)Math.PI * 2f;
			previousRotation = rotation;
			yRotation = UnityEngine.Random.value * (float)Math.PI * 2f;
			previousYRotation = yRotation;
			lifetime = UnityEngine.Random.Range(0, 200);
		}
	}

	public struct CritterChunk
	{
		public Vector2 position;

		public float repulsionRadius;

		public CritterChunk(Vector2 position, float repulsionRadius)
		{
			this.position = position;
			this.repulsionRadius = repulsionRadius;
		}
	}

	public const int particlesPerGroup = 256;

	public float initialEffectAmount;

	public float lastEffectAmount;

	public int hideAmount;

	public CritterChunk[] trackedChunks;

	public ComputeShader rotParticleCompute;

	public static MaterialPropertyBlock mpb;

	public static Material particleMaterial;

	public GameObject renderer;

	public FGameObjectNode rendererNode;

	public FContainer rendererNodeContainer;

	public ComputeBuffer particleBuffer;

	public Mesh mesh;

	public SentientRotSpores(Room room)
	{
		base.room = room;
		rotParticleCompute = base.room.game.rainWorld.particleComputeShader;
		initialEffectAmount = room.roomSettings.GetEffect(WatcherEnums.RoomEffectType.SentientRotParticles)?.amount ?? 0f;
		initialEffectAmount *= 1f - global::Watcher.Watcher.cfgReduceFallingParticles.Value;
		lastEffectAmount = initialEffectAmount;
		float num = initialEffectAmount / 250f;
		Particle[] array = new Particle[Mathf.Clamp((int)(base.room.PixelWidth * base.room.PixelHeight * num), 1, 8192)];
		for (int i = 0; i < array.Length; i++)
		{
			Vector2 position = base.room.RandomPos();
			array[i] = new Particle(position);
		}
		particleBuffer = new ComputeBuffer(array.Length, 36);
		particleBuffer.SetData(array);
		if (mpb == null)
		{
			mpb = new MaterialPropertyBlock();
		}
	}

	public void runComputeShader()
	{
		rotParticleCompute.SetBuffer(0, "particleMap", particleBuffer);
		rotParticleCompute.SetVector("roomSize", new Vector2(room.PixelWidth, room.PixelHeight));
		rotParticleCompute.SetFloat("time", room.game.clock);
		int threadGroupsX = (particleBuffer.count + 256 - 1) / 256;
		rotParticleCompute.Dispatch(0, threadGroupsX, 1, 1);
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (!room.BeingViewed || base.slatedForDeletetion)
		{
			return;
		}
		RoomSettings.RoomEffect effect = room.roomSettings.GetEffect(WatcherEnums.RoomEffectType.SentientRotParticles);
		if (effect != null)
		{
			float num = effect.amount * (1f - global::Watcher.Watcher.cfgReduceFallingParticles.Value);
			if (num != lastEffectAmount)
			{
				hideAmount = (int)(Mathf.InverseLerp(initialEffectAmount, 0f, num) * (float)particleBuffer.count);
			}
			lastEffectAmount = effect.amount;
		}
		Shader.SetGlobalInt(RainWorld.ShadPropRPMHideBeforeIndex, hideAmount);
		runComputeShader();
	}

	public GameObject CreateMeshObject()
	{
		DisposeMesh();
		renderer = new GameObject("Rot Particle Renderer", typeof(MeshFilter), typeof(MeshRenderer));
		MeshFilter component = renderer.GetComponent<MeshFilter>();
		MeshRenderer component2 = renderer.GetComponent<MeshRenderer>();
		mesh = new Mesh
		{
			name = "Rot Particle Mesh"
		};
		int count = particleBuffer.count;
		Vector3[] array = new Vector3[count * 4];
		Vector2[] array2 = new Vector2[count * 4];
		int[] array3 = new int[count * 6];
		List<FAtlasElement> list = new List<FAtlasElement>();
		for (int i = 1; i <= 14; i++)
		{
			list.Add(Futile.atlasManager.GetElementWithName("Pebble" + i));
		}
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		for (int j = 0; j < count; j++)
		{
			array3[num3++] = num;
			array3[num3++] = num + 1;
			array3[num3++] = num + 2;
			array3[num3++] = num;
			array3[num3++] = num + 2;
			array3[num3++] = num + 3;
			FAtlasElement fAtlasElement = list[UnityEngine.Random.Range(0, list.Count)];
			float num4 = fAtlasElement.sourceRect.width / 2f;
			float num5 = fAtlasElement.sourceRect.height / 2f;
			array[num++] = new Vector3(0f - num4, 0f - num5, 0f);
			array[num++] = new Vector3(0f - num4, num5, 0f);
			array[num++] = new Vector3(num4, num5, 0f);
			array[num++] = new Vector3(num4, 0f - num5, 0f);
			array2[num2++] = fAtlasElement.uvBottomLeft;
			array2[num2++] = fAtlasElement.uvTopLeft;
			array2[num2++] = fAtlasElement.uvTopRight;
			array2[num2++] = fAtlasElement.uvBottomRight;
		}
		mesh.SetVertices(array);
		mesh.SetUVs(0, array2);
		mesh.SetIndices(array3, MeshTopology.Triangles, 0);
		mesh.bounds = new Bounds(Vector3.zero, new Vector3(100000f, 100000f, 100000f));
		component.sharedMesh = mesh;
		mpb.Clear();
		mpb.SetTexture("_MainTex", list[0].atlas.texture);
		mpb.SetBuffer("_particleMap", particleBuffer);
		if (!particleMaterial)
		{
			particleMaterial = new Material(Shader.Find("Futile/RotParticleMesh"));
		}
		component2.sharedMaterial = particleMaterial;
		component2.SetPropertyBlock(mpb);
		return renderer;
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		if (!base.slatedForDeletetion)
		{
			sLeaser.sprites = new FSprite[0];
			sLeaser.containers = new FContainer[1]
			{
				new FContainer()
			};
			DisposeRendererNode();
			rendererNode = new FGameObjectNode(CreateMeshObject(), shouldLinkPosition: true, shouldLinkRotation: true, shouldLinkScale: true)
			{
				shouldDestroyOnRemoveFromStage = true
			};
			rendererNodeContainer = sLeaser.containers[0];
			rendererNodeContainer.AddChild(rendererNode);
			AddToContainer(sLeaser, rCam, null);
		}
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		if (!base.slatedForDeletetion)
		{
			FGameObjectNode fGameObjectNode = sLeaser.containers[0].GetChildAt(0) as FGameObjectNode;
			fGameObjectNode.SetPosition(-camPos);
			if ((bool)fGameObjectNode.gameObject)
			{
				MeshRenderer component = fGameObjectNode.gameObject.GetComponent<MeshRenderer>();
				component.GetPropertyBlock(mpb);
				mpb.SetFloat("_timeStacker", timeStacker);
				component.SetPropertyBlock(mpb);
			}
			if (base.slatedForDeletetion || room != rCam.room)
			{
				sLeaser.CleanSpritesAndRemove();
			}
		}
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		if (!base.slatedForDeletetion)
		{
			FGameObjectNode fGameObjectNode = sLeaser.containers[0].GetChildAt(0) as FGameObjectNode;
			if ((bool)fGameObjectNode.gameObject)
			{
				MeshRenderer component = fGameObjectNode.gameObject.GetComponent<MeshRenderer>();
				component.GetPropertyBlock(mpb);
				mpb.SetColor("_Color", palette.blackColor);
				component.SetPropertyBlock(mpb);
			}
		}
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		if (newContatiner == null)
		{
			newContatiner = rCam.ReturnFContainer("Midground");
		}
		newContatiner.AddChild(sLeaser.containers[0]);
	}

	public void DisposeRendererNode()
	{
		if (rendererNode != null)
		{
			rendererNodeContainer.RemoveChild(rendererNode);
			rendererNode = null;
			rendererNodeContainer = null;
		}
	}

	public void DisposeMesh()
	{
		DisposeRendererNode();
		if (renderer != null)
		{
			UnityEngine.Object.Destroy(renderer);
			renderer = null;
		}
		if (mesh != null)
		{
			UnityEngine.Object.Destroy(mesh);
			mesh = null;
		}
	}

	public override void Destroy()
	{
		base.Destroy();
		if (particleBuffer != null)
		{
			particleBuffer.Release();
			particleBuffer = null;
		}
		DisposeMesh();
	}

	public void RoomUnloaded()
	{
		Destroy();
	}
}
