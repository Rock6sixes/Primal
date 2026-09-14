using HUD;

namespace Watcher;

public class VoidWeaverConversation : Conversation
{
	public SoundID Voiceline { get; set; }

	public VoidWeaverConversation(IOwnAConversation interfaceOwner, ID id, DialogBox dialogBox)
		: base(interfaceOwner, id, dialogBox)
	{
		Voiceline = GetVoiceline(id);
		AddEvents();
	}

	public override void AddEvents()
	{
		LoadEventsFromFile(GetConversationFile(id));
	}

	public static SoundID GetVoiceline(ID id)
	{
		if (id == WatcherEnums.ConversationID.Weaver_1)
		{
			return WatcherEnums.WatcherSoundID.Void_Weaver_Voice_1;
		}
		if (id == WatcherEnums.ConversationID.Weaver_2)
		{
			return WatcherEnums.WatcherSoundID.Void_Weaver_Voice_2;
		}
		if (id == WatcherEnums.ConversationID.Weaver_3)
		{
			return WatcherEnums.WatcherSoundID.Void_Weaver_Voice_3;
		}
		if (id == WatcherEnums.ConversationID.Weaver_4)
		{
			return WatcherEnums.WatcherSoundID.Void_Weaver_Voice_4;
		}
		return WatcherEnums.WatcherSoundID.Void_Weaver_Voice_1;
	}

	public static int GetConversationFile(ID id)
	{
		if (id == WatcherEnums.ConversationID.Weaver_1)
		{
			return 229;
		}
		if (id == WatcherEnums.ConversationID.Weaver_2)
		{
			return 230;
		}
		if (id == WatcherEnums.ConversationID.Weaver_3)
		{
			return 231;
		}
		if (id == WatcherEnums.ConversationID.Weaver_4)
		{
			return 232;
		}
		return 0;
	}
}
