using System.Collections.Generic;
using System.Linq;
using RWCustom;
using UnityEngine;
using UnityEngine.Rendering;

namespace Watcher;

public class LevelTexCombiner
{
	public RenderTexture combinedLevelTex;

	public RenderTexture intermediateTex;

	public const CameraEvent first = CameraEvent.BeforeForwardOpaque;

	public const CameraEvent second = CameraEvent.AfterForwardOpaque;

	public const CameraEvent last = CameraEvent.BeforeForwardAlpha;

	public const string firstPass = "FirstPass";

	public bool isActive => bufferIDs.Count != 0;

	public List<string> bufferIDs { get; set; }

	public LevelTexCombiner()
	{
		bufferIDs = new List<string>();
	}

	public void Initialize()
	{
		if (combinedLevelTex == null)
		{
			combinedLevelTex = new RenderTexture(1400, 800, 0, RenderTextureFormat.ARGB32);
			combinedLevelTex.filterMode = FilterMode.Point;
			intermediateTex = new RenderTexture(1400, 800, 0, RenderTextureFormat.ARGB32);
			intermediateTex.filterMode = FilterMode.Point;
			Shader.SetGlobalTexture("_LevelTex", combinedLevelTex);
		}
		Shader.EnableKeyword("COMBINEDLEVEL");
		CreateBuffer("FirstPass", Custom.rainWorld.persistentData.cameraTextures[0, 0], null, CameraEvent.BeforeForwardOpaque);
	}

	public void AddPass(RenderTargetIdentifier texture, Shader shader, string id, CameraEvent evt = CameraEvent.AfterForwardOpaque)
	{
		AddPass(texture, new Material(shader), id, evt);
	}

	public void AddPass(Shader shader, string id, CameraEvent evt = CameraEvent.AfterForwardOpaque)
	{
		AddPass(RenderTexture.GetTemporary(1, 1), new Material(shader), id, evt);
	}

	public void AddPass(RenderTargetIdentifier texture, Material material, string id, CameraEvent evt = CameraEvent.AfterForwardOpaque)
	{
		if (!isActive)
		{
			Initialize();
		}
		else
		{
			RemovePass(id);
		}
		CreateBuffer(id, texture, material, evt);
	}

	public void CreateBuffer(string id, RenderTargetIdentifier texture, Material material, CameraEvent evt = CameraEvent.AfterForwardOpaque)
	{
		CommandBuffer commandBuffer = new CommandBuffer();
		commandBuffer.name = id;
		if (evt == CameraEvent.BeforeForwardOpaque)
		{
			commandBuffer.Blit(Custom.rainWorld.persistentData.cameraTextures[0, 0], intermediateTex);
			AddBuffer(commandBuffer, evt);
			return;
		}
		if (material != null)
		{
			commandBuffer.Blit(texture, intermediateTex, material);
		}
		else
		{
			commandBuffer.Blit(texture, intermediateTex);
		}
		AddBuffer(commandBuffer, evt);
	}

	public void AddBuffer(CommandBuffer buffer, CameraEvent cameraEvent)
	{
		buffer.CopyTexture(intermediateTex, combinedLevelTex);
		buffer.SetGlobalTexture("_LevelTex", combinedLevelTex);
		Camera.main.AddCommandBuffer(cameraEvent, buffer);
		bufferIDs.Add(buffer.name);
		SetGlobals();
	}

	public void RemovePass(string id)
	{
		foreach (CommandBuffer item in from b in Camera.main.GetCommandBuffers(CameraEvent.AfterForwardOpaque)
			where b.name == id
			select b)
		{
			Camera.main.RemoveCommandBuffer(CameraEvent.AfterForwardOpaque, item);
		}
		foreach (CommandBuffer item2 in from b in Camera.main.GetCommandBuffers(CameraEvent.BeforeForwardAlpha)
			where b.name == id
			select b)
		{
			Camera.main.RemoveCommandBuffer(CameraEvent.BeforeForwardAlpha, item2);
		}
		bufferIDs.Remove(id);
	}

	public void RemoveAllBuffers()
	{
		foreach (CommandBuffer item in from b in Camera.main.GetCommandBuffers(CameraEvent.BeforeForwardOpaque)
			where bufferIDs.Any((string name) => b.name == name)
			select b)
		{
			Camera.main.RemoveCommandBuffer(CameraEvent.BeforeForwardOpaque, item);
		}
		foreach (CommandBuffer item2 in from b in Camera.main.GetCommandBuffers(CameraEvent.AfterForwardOpaque)
			where bufferIDs.Any((string name) => b.name == name)
			select b)
		{
			Camera.main.RemoveCommandBuffer(CameraEvent.AfterForwardOpaque, item2);
		}
		foreach (CommandBuffer item3 in from b in Camera.main.GetCommandBuffers(CameraEvent.BeforeForwardAlpha)
			where bufferIDs.Any((string name) => b.name == name)
			select b)
		{
			Camera.main.RemoveCommandBuffer(CameraEvent.BeforeForwardAlpha, item3);
		}
		bufferIDs.Clear();
		Shader.DisableKeyword("COMBINEDLEVEL");
		UnSetGlobals();
		DisposeRenderTextures();
	}

	public void DisposeRenderTextures()
	{
		if (combinedLevelTex != null)
		{
			Object.Destroy(combinedLevelTex);
			combinedLevelTex = null;
		}
		if (intermediateTex != null)
		{
			Object.Destroy(intermediateTex);
			intermediateTex = null;
		}
	}

	public void SetGlobals()
	{
		Shader.SetGlobalTexture("_OrigLevelTex", Custom.rainWorld.persistentData.cameraTextures[0, 0]);
		Shader.SetGlobalTexture("_LevelTex", combinedLevelTex);
	}

	public void UnSetGlobals()
	{
		Shader.SetGlobalTexture("_LevelTex", Custom.rainWorld.persistentData.cameraTextures[0, 0]);
	}
}
