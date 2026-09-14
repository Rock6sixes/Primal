using System.Collections.Generic;
using System.Linq;
using HUD;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class OracleBehavior : Conversation.IOwnAConversation
{
	public Oracle oracle;

	public Vector2 lookPoint;

	public Player player;

	public Player playerWithGreenNeuron;

	public Player firstAlivePlayerInRoomNotInShortcut;

	public Player firstAlivePlayerInRoom;

	public Player firstAnyPlayerInRoom;

	public int allStillCounter;

	public int allPlayersOutOfRoomCounter;

	public int consistentBasePosCounter;

	public int inActionCounter;

	public DebugSprite dbspr;

	public ChunkSoundEmitter voice;

	public Creature safariCreature;

	public bool isRepeatedDiscussion;

	public List<Player> PlayersInRoom => (from x in oracle?.room?.game?.NonPermaDeadPlayers?.Where((AbstractCreature x) => x.Room == oracle.room.abstractRoom)
		select x?.realizedCreature as Player into x
		orderby x.dead
		select x).ToList();

	public virtual Vector2 OracleGetToPos => default(Vector2);

	public virtual Vector2 BaseGetToPos => default(Vector2);

	public virtual Vector2 GetToDir => default(Vector2);

	public virtual bool EyesClosed => false;

	public virtual DialogBox dialogBox => null;

	public void StunCoopPlayers(int st)
	{
		foreach (Player item in PlayersInRoom)
		{
			item.Stun(st);
			if (item.slugOnBack != null && item.slugOnBack.HasASlug && !item.slugOnBack.slugcat.dead)
			{
				item.slugOnBack.DropSlug();
			}
		}
	}

	public OracleBehavior(Oracle oracle)
	{
		this.oracle = oracle;
		FindPlayer();
		safariCreature = null;
	}

	public void FindPlayer()
	{
		if (oracle.room != null && !oracle.room.game.rainWorld.safariMode)
		{
			SlugcatStats.Name name = SlugcatStats.Name.White;
			if (oracle.room.game.IsStorySession)
			{
				name = oracle.room.game.GetStorySession.saveStateNumber;
			}
			playerWithGreenNeuron = null;
			firstAlivePlayerInRoomNotInShortcut = null;
			firstAlivePlayerInRoom = null;
			firstAnyPlayerInRoom = null;
			for (int i = 0; i < oracle.room.game.Players.Count; i++)
			{
				Player player = null;
				if (oracle.room.game.Players[i].realizedCreature != null && oracle.room.game.Players[i].realizedCreature.room == oracle.room)
				{
					player = oracle.room.game.Players[i].realizedCreature as Player;
				}
				if (player == null)
				{
					continue;
				}
				if (playerWithGreenNeuron == null && player.objectInStomach != null && player.objectInStomach.type == AbstractPhysicalObject.AbstractObjectType.NSHSwarmer)
				{
					playerWithGreenNeuron = player;
				}
				if (playerWithGreenNeuron == null)
				{
					for (int j = 0; j < player.grasps.Length; j++)
					{
						if (player.grasps[j] != null && player.grasps[j].grabbed is NSHSwarmer)
						{
							playerWithGreenNeuron = player;
							break;
						}
					}
				}
				if ((firstAlivePlayerInRoomNotInShortcut == null || (oracle.room.game.IsStorySession && firstAlivePlayerInRoomNotInShortcut.SlugCatClass != name && player.SlugCatClass == name)) && !player.dead && !player.inShortcut)
				{
					firstAlivePlayerInRoomNotInShortcut = player;
				}
				if ((firstAlivePlayerInRoom == null || (oracle.room.game.IsStorySession && firstAlivePlayerInRoom.SlugCatClass != name && player.SlugCatClass == name)) && !player.dead)
				{
					firstAlivePlayerInRoom = player;
				}
				if (firstAnyPlayerInRoom == null || (oracle.room.game.IsStorySession && firstAnyPlayerInRoom.SlugCatClass != name && player.SlugCatClass == name))
				{
					firstAnyPlayerInRoom = player;
				}
			}
			if (oracle.room.game.IsStorySession && oracle.ID == Oracle.OracleID.SS && oracle.room.game.StoryCharacter == SlugcatStats.Name.Red && !oracle.room.game.GetStorySession.saveState.miscWorldSaveData.pebblesSeenGreenNeuron && playerWithGreenNeuron != null)
			{
				this.player = playerWithGreenNeuron;
			}
			else if (firstAlivePlayerInRoomNotInShortcut != null)
			{
				this.player = firstAlivePlayerInRoomNotInShortcut;
			}
			else if (firstAlivePlayerInRoom != null)
			{
				this.player = firstAlivePlayerInRoom;
			}
			else if (firstAnyPlayerInRoom != null)
			{
				this.player = firstAnyPlayerInRoom;
			}
			else
			{
				this.player = null;
			}
			if (this.player != null)
			{
				oracle.room.game.cameras[0].EnterCutsceneMode(this.player.abstractCreature, RoomCamera.CameraCutsceneType.Oracle);
			}
		}
		else
		{
			this.player = null;
		}
	}

	public virtual void Update(bool eu)
	{
		if (voice != null)
		{
			voice.alive = true;
			if (voice.slatedForDeletetion)
			{
				voice = null;
			}
		}
		if (ModManager.MSC && oracle.room != null && oracle.room.game.rainWorld.safariMode)
		{
			safariCreature = null;
			float num = float.MaxValue;
			for (int i = 0; i < oracle.room.abstractRoom.creatures.Count; i++)
			{
				if (oracle.room.abstractRoom.creatures[i].realizedCreature != null)
				{
					Creature realizedCreature = oracle.room.abstractRoom.creatures[i].realizedCreature;
					float num2 = Custom.Dist(oracle.firstChunk.pos, realizedCreature.mainBodyChunk.pos);
					if (num2 < num)
					{
						num = num2;
						safariCreature = realizedCreature;
					}
				}
			}
		}
		FindPlayer();
	}

	public virtual string Translate(string s)
	{
		return oracle.room.game.rainWorld.inGameTranslator.Translate(s);
	}

	public string AlreadyDiscussedItemString(bool pearl)
	{
		string empty = string.Empty;
		if (pearl)
		{
			if (!ModManager.MSC || oracle.ID != Oracle.OracleID.SS)
			{
				return Random.Range(0, 3) switch
				{
					1 => Translate("This one I've already read to you, <PlayerName>."), 
					0 => Translate("Oh, I have already read this one to you, <PlayerName>."), 
					_ => Translate("This one again, <PlayerName>?"), 
				};
			}
			return Random.Range(0, 3) switch
			{
				1 => Translate("We have read this one, little creature."), 
				0 => Translate("We've read this, little creature. As previously stated:"), 
				_ => Translate("I have read this. As I have stated before:"), 
			};
		}
		return Random.Range(0, 3) switch
		{
			1 => Translate("I've told you about this one, <PlayerName>."), 
			0 => Translate("I think we have already talked about this one, <PlayerName>."), 
			_ => Translate("<CapPlayerName>, this one again?"), 
		};
	}

	public bool CheckSlugpupsInRoom()
	{
		if (!ModManager.MSC)
		{
			return false;
		}
		foreach (AbstractCreature creature in oracle.room.abstractRoom.creatures)
		{
			if (creature.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.SlugNPC && creature.state.alive)
			{
				return true;
			}
		}
		return false;
	}

	public CreatureTemplate.Type CheckStrayCreatureInRoom()
	{
		foreach (AbstractCreature creature in oracle.room.abstractRoom.creatures)
		{
			if (creature.creatureTemplate.type != CreatureTemplate.Type.Slugcat && (!ModManager.MSC || creature.creatureTemplate.type != MoreSlugcatsEnums.CreatureTemplateType.SlugNPC) && creature.creatureTemplate.type != CreatureTemplate.Type.TubeWorm && creature.creatureTemplate.type != CreatureTemplate.Type.VultureGrub && creature.creatureTemplate.type != CreatureTemplate.Type.Hazer && creature.creatureTemplate.type != CreatureTemplate.Type.Fly && creature.creatureTemplate.type != CreatureTemplate.Type.Overseer && creature.state.alive)
			{
				return creature.creatureTemplate.type;
			}
		}
		return CreatureTemplate.Type.StandardGroundCreature;
	}

	public virtual void UnconciousUpdate()
	{
		FindPlayer();
	}

	public string ReplaceParts(string s)
	{
		return s;
	}

	public void SpecialEvent(string eventName)
	{
	}
}
