using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Security.Permissions;
using BepInEx;
using Menu;
using Microsoft.CodeAnalysis;
using MoreSlugcats;
using On.Menu;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.Default | DebuggableAttribute.DebuggingModes.DisableOptimizations | DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints | DebuggableAttribute.DebuggingModes.EnableEditAndContinue)]
[assembly: TargetFramework(".NETFramework,Version=v4.8", FrameworkDisplayName = ".NET Framework 4.8")]
[assembly: AssemblyCompany("Invadd")]
[assembly: AssemblyConfiguration("Debug")]
[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: AssemblyInformationalVersion("1.0.0+fa339b0373bc272de0ffc24220e2afbe683401f0")]
[assembly: AssemblyProduct("Invadd")]
[assembly: AssemblyTitle("Invadd")]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
[assembly: AssemblyVersion("1.0.0.0")]
[module: RefSafetyRules(11)]
namespace Microsoft.CodeAnalysis
{
	[CompilerGenerated]
	[Embedded]
	internal sealed class EmbeddedAttribute : Attribute
	{
	}
}
namespace System.Runtime.CompilerServices
{
	[CompilerGenerated]
	[Embedded]
	[AttributeUsage(AttributeTargets.Module, AllowMultiple = false, Inherited = false)]
	internal sealed class RefSafetyRulesAttribute : Attribute
	{
		public readonly int Version;

		public RefSafetyRulesAttribute(int P_0)
		{
			Version = P_0;
		}
	}
}
namespace ScugRando
{
	[BepInPlugin("invadd", "InvAdd", "0.1.0")]
	internal sealed class Plugin : BaseUnityPlugin
	{
		[CompilerGenerated]
		private static class <>O
		{
			public static hook_SetSlugcatColorOrder <0>__SlugcatSelectMenu_SetSlugcatColorOrder;
		}

		public void OnEnable()
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Expected O, but got Unknown
			object obj = <>O.<0>__SlugcatSelectMenu_SetSlugcatColorOrder;
			if (obj == null)
			{
				hook_SetSlugcatColorOrder val = SlugcatSelectMenu_SetSlugcatColorOrder;
				<>O.<0>__SlugcatSelectMenu_SetSlugcatColorOrder = val;
				obj = (object)val;
			}
			SlugcatSelectMenu.SetSlugcatColorOrder += (hook_SetSlugcatColorOrder)obj;
		}

		private static void SlugcatSelectMenu_SetSlugcatColorOrder(orig_SetSlugcatColorOrder orig, SlugcatSelectMenu self)
		{
			orig.Invoke(self);
			if (ModManager.MSC)
			{
				self.slugcatColorOrder.Add(SlugcatStatsName.Sofanthiel);
			}
		}
	}
}
