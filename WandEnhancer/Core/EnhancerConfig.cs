using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using WandEnhancer.Core.Js;
using WandEnhancer.Models;

namespace WandEnhancer.Core
{
	internal static class EnhancerConfig
	{
		public delegate JsEdit[] PatchLocator(JsCursor js);

		public sealed class PatchEntry
		{
			public string Name { get; set; }

			public PatchLocator Locate { get; set; }

			public string[] CandidateFileNames { get; set; }

			public string[] SearchHints { get; set; }

			public string[] CapabilityHints { get; set; }

			public bool Applied { get; set; }

			public bool CapabilityDetected { get; set; }

			public bool IsOptional
			{
				get
				{
					if (CapabilityHints != null)
					{
						return CapabilityHints.Length != 0;
					}
					return false;
				}
			}

			public bool IsResolved
			{
				get
				{
					if (!Applied)
					{
						if (IsOptional)
						{
							return !CapabilityDetected;
						}
						return false;
					}
					return true;
				}
			}
		}

		private static readonly Regex WhenReady = new Regex("(?<app>[\\w$]+)\\.whenReady\\(\\)\\.then\\(");

		private static readonly Regex WhenReadyThenRun = new Regex("(?<app>[\\w$]+)\\.whenReady\\(\\)\\.then\\(run\\)");

		private static readonly Regex RemoteSetValue = new Regex("this\\.(?<trainer>#[\\w$]+)\\.setValue\\(\\s*(?<change>[\\w$]+)\\.name\\s*,\\s*\\k<change>\\.value\\s*,\\s*(?<source>[^,]+?)\\s*,");

		public static Dictionary<EPatchType, PatchEntry[]> GetInstance()
		{
			Dictionary<EPatchType, PatchEntry[]> dictionary = new Dictionary<EPatchType, PatchEntry[]>();
			dictionary.Add(EPatchType.ActivatePro, new PatchEntry[5]
			{
				new PatchEntry
				{
					Name = "getUserAccount",
					SearchHints = new string[1] { "getUserAccount(" },
					Locate = (JsCursor js) => ForceProSubscription(js, "getUserAccount")
				},
				new PatchEntry
				{
					Name = "setAccountWandBrandExperience",
					SearchHints = new string[1] { "setAccountWandBrandExperience(" },
					CapabilityHints = new string[1] { "/v3/account/brand_experience_wand" },
					Locate = (JsCursor js) => ForceProSubscription(js, "setAccountWandBrandExperience")
				},
				new PatchEntry
				{
					Name = "setAccountLanguage",
					SearchHints = new string[1] { "setAccountLanguage(" },
					Locate = (JsCursor js) => ForceProSubscription(js, "setAccountLanguage")
				},
				new PatchEntry
				{
					Name = "setAccountReducer",
					SearchHints = new string[1] { "ACTION_SET_ACCOUNT" },
					Locate = LocateAccountReducer
				},
				new PatchEntry
				{
					Name = "disableNativeRemotePairing",
					SearchHints = new string[1] { "requestRemoteAuthCode" },
					Locate = (JsCursor js) => Edits(js.FindFunction("requestRemoteAuthCode")?.ReplaceBody(PatchPayload.Load("disable-native-pairing")))
				}
			});
			dictionary.Add(EPatchType.DisableUpdates, new PatchEntry[1]
			{
				new PatchEntry
				{
					Name = "disableUpdateCheck",
					CandidateFileNames = new string[1] { "index.js" },
					SearchHints = new string[1] { "ACTION_CHECK_FOR_UPDATE" },
					Locate = LocateUpdateHandler
				}
			});
			dictionary.Add(EPatchType.DevToolsOnF12, new PatchEntry[1]
			{
				new PatchEntry
				{
					Name = "devToolsBeforeInputEvent",
					CandidateFileNames = new string[1] { "index.js" },
					SearchHints = new string[1] { "whenReady().then(" },
					Locate = LocateDevToolsHook
				}
			});
			dictionary.Add(EPatchType.RemoteWebPanelPreview, new PatchEntry[5]
			{
				new PatchEntry
				{
					Name = "remoteBridgeMainBoot",
					CandidateFileNames = new string[1] { "index.js" },
					SearchHints = new string[1] { "whenReady().then(run)" },
					Locate = LocateBridgeBoot
				},
				new PatchEntry
				{
					Name = "remoteBridgeReset",
					SearchHints = new string[1] { "client-state" },
					Locate = LocateBridgeReset
				},
				new PatchEntry
				{
					Name = "remoteBridgeSyncSnapshot",
					SearchHints = new string[1] { "client-state" },
					Locate = LocateBridgeSync
				},
				new PatchEntry
				{
					Name = "remoteBridgeBindHandler",
					SearchHints = new string[1] { "setCurrentTrainer(" },
					Locate = LocateBridgeBindHandler
				},
				new PatchEntry
				{
					Name = "remoteBridgeValueDelta",
					SearchHints = new string[1] { "client-value-changed" },
					Locate = LocateBridgeValueDelta
				}
			});
			return dictionary;
		}

		private static JsEdit[] ForceProSubscription(JsCursor js, string methodName)
		{
			return Edits(js.FindFunction(methodName)?.WrapReturn(PatchPayload.Load("pro-subscription")));
		}

		private static JsEdit[] LocateAccountReducer(JsCursor js)
		{
			int num = js.IndexOf("\"ACTION_SET_ACCOUNT\"");
			JsFunction jsFunction = ((num < 0) ? null : js.FindFunctionAfter(num));
			if (jsFunction == null)
			{
				return null;
			}
			return Edits(jsFunction.ReplaceInBody("account:\\s*(?<account>[\\w$]+)", PatchPayload.Load("pro-account-reducer")));
		}

		private static JsEdit[] LocateUpdateHandler(JsCursor js)
		{
			int num = js.FindCall("registerHandler", "\"ACTION_CHECK_FOR_UPDATE\"");
			if (num < 0)
			{
				return null;
			}
			return Edits(new JsEdit(num + 1, js.MatchClose(num), PatchPayload.Load("disable-updates")));
		}

		private static JsEdit[] LocateDevToolsHook(JsCursor js)
		{
			Match match = WhenReady.Match(js.Text);
			if (!match.Success)
			{
				return null;
			}
			string text = PatchPayload.Load("devtools-f12", "app", match.Groups["app"].Value);
			return Edits(new JsEdit(match.Index, match.Index, text));
		}

		private static JsEdit[] LocateBridgeBoot(JsCursor js)
		{
			Match match = WhenReadyThenRun.Match(js.Text);
			if (!match.Success)
			{
				return null;
			}
			string text = PatchPayload.Load("remote-bridge-boot", "app", match.Groups["app"].Value);
			return Edits(new JsEdit(match.Index, match.Index + match.Length, text));
		}

		private static JsEdit[] LocateBridgeReset(JsCursor js)
		{
			JsFunction jsFunction = FindClientStateMethod(js);
			JsFunction jsFunction2 = ((jsFunction == null) ? null : js.FunctionEndingAt(js.SkipWhitespaceBack(jsFunction.Start - 1)));
			if (jsFunction2 == null || jsFunction2.Body.IndexOf("Date.now()", StringComparison.Ordinal) < 0)
			{
				return null;
			}
			return Edits(jsFunction2.InsertAtEnd(PatchPayload.Load("remote-bridge-reset")));
		}

		private static JsEdit[] LocateBridgeSync(JsCursor js)
		{
			int num = js.FindCall("send", "\"client-state\"");
			if (num < 0)
			{
				return null;
			}
			JsFunction jsFunction = js.EnclosingFunction(num);
			int num2 = js.IndexOf("{", num);
			int num3 = js.MatchClose(num2);
			if (jsFunction == null || num2 < 0 || num3 < 0)
			{
				throw new Exception("client-state payload object could not be located");
			}
			string text = js.Text.Substring(num2 + 1, num3 - num2 - 1).Trim().TrimEnd(',');
			string text2 = PatchPayload.Load("remote-bridge-sync", "snapshot", text, "trainer", jsFunction.Resolve("this\\.(?<trainer>#[\\w$]+)\\s*\\?\\.\\s*getMetadata", "trainer"), "metadata", jsFunction.Resolve("getMetadata\\(\\s*(?<metadata>[\\w$]+\\.[\\w$]+)\\s*\\)", "metadata"));
			List<JsEdit> list = new List<JsEdit>();
			list.Add(new JsEdit(js.MatchClose(num) + 1, text2));
			list.AddRange(HoistConnectedGuard(js, num));
			return list.ToArray();
		}

		private static IEnumerable<JsEdit> HoistConnectedGuard(JsCursor js, int sendOpen)
		{
			int blockOpen = js.EnclosingOpener(sendOpen, '{');
			int num = ((blockOpen < 0) ? (-1) : js.SkipWhitespaceBack(blockOpen - 1));
			if (num < 0 || js.Text[num] != ')')
			{
				yield break;
			}
			List<int> list = js.OpenerStack(num);
			if (list.Count == 0 || js.NameBefore(list[0]) != "if")
			{
				yield break;
			}
			int num2 = list[0];
			string text = js.Text.Substring(num2 + 1, num - num2 - 1);
			if (text.IndexOf("this.status", StringComparison.Ordinal) >= 0 && !HasElseBranch(js, blockOpen))
			{
				int guardStart = js.SkipWhitespaceBack(num2 - 1) - 1;
				int num3 = sendOpen;
				while (num3 > 0 && IsCalleeChar(js.Text[num3 - 1]))
				{
					num3--;
				}
				yield return new JsEdit(num3, num3, "(" + text + ")&&");
				yield return new JsEdit(guardStart, blockOpen, string.Empty);
			}
		}

		private static bool HasElseBranch(JsCursor js, int blockOpen)
		{
			int indexA = js.SkipWhitespaceForward(js.MatchClose(blockOpen) + 1);
			return string.CompareOrdinal(js.Text, indexA, "else", 0, 4) == 0;
		}

		private static JsEdit[] LocateBridgeBindHandler(JsCursor js)
		{
			JsFunction jsFunction = js.FindFunction("setCurrentTrainer");
			if (jsFunction == null)
			{
				return null;
			}
			int num = js.FindCall("listen", "\"client-value-changed\"");
			string name = (((num < 0) ? null : js.EnclosingFunction(num)) ?? throw new Exception("Remote value listener could not be located")).Resolve("listen\\(\\s*\"client-value-changed\"\\s*,\\s*\\(?\\s*[\\w$]+\\s*\\)?\\s*=>\\s*this\\.(?<handler>#[\\w$]+)\\(", "handler");
			JsFunction jsFunction2 = js.FindFunction(name);
			if (jsFunction2 == null)
			{
				throw new Exception("Remote value handler could not be located");
			}
			Match match = MatchExactlyOnce(RemoteSetValue, jsFunction2.Body, "Remote setValue call");
			Match match2 = MatchExactlyOnce(new Regex("setCurrentTrainer\\(\\s*(?<info>[\\w$]+)"), js.Text.Substring(jsFunction.Start, jsFunction.BodyOpen - jsFunction.Start), "Trainer info parameter");
			return Edits(jsFunction.InsertAtStart(PatchPayload.Load("remote-bridge-renderer", "trainer", match.Groups["trainer"].Value, "trainerInfo", match2.Groups["info"].Value, "remoteSource", match.Groups["source"].Value)));
		}

		private static JsEdit[] LocateBridgeValueDelta(JsCursor js)
		{
			int num = js.FindCall("send", "\"client-value-changed\"");
			if (num < 0)
			{
				return null;
			}
			JsFunction jsFunction = js.EnclosingFunction(num);
			JsFunction jsFunction2 = FindClientStateMethod(js);
			if (jsFunction == null || jsFunction2 == null || jsFunction.Body.IndexOf(".onValueSet(", StringComparison.Ordinal) < 0)
			{
				throw new Exception("Trainer value subscription could not be located");
			}
			string text = jsFunction2.Resolve("trainerId:\\s*(?<id>this\\.#[\\w$]+)", "id");
			string text2 = jsFunction.Resolve("name:\\s*(?<change>[\\w$]+)\\.name", "change");
			Match match = Regex.Match(jsFunction.Body, "\\.onValueSet\\(\\s*\\(?\\s*" + Regex.Escape(text2) + "\\s*\\)?\\s*=>\\s*\\{");
			if (!match.Success)
			{
				throw new Exception("Trainer value callback could not be located");
			}
			return new JsEdit[2]
			{
				jsFunction.InsertAtStart(PatchPayload.Load("remote-bridge-value-subscription", "trainerId", text)),
				new JsEdit(jsFunction.BodyOpen + 1 + match.Index + match.Length, PatchPayload.Load("remote-bridge-value-delta", "change", text2))
			};
		}

		private static JsFunction FindClientStateMethod(JsCursor js)
		{
			int num = js.FindCall("send", "\"client-state\"");
			if (num >= 0)
			{
				return js.EnclosingFunction(num);
			}
			return null;
		}

		private static JsEdit[] Edits(JsEdit edit)
		{
			if (edit != null)
			{
				return new JsEdit[1] { edit };
			}
			return null;
		}

		private static bool IsCalleeChar(char value)
		{
			if (!char.IsLetterOrDigit(value) && value != '_' && value != '$' && value != '#' && value != '.')
			{
				return value == '?';
			}
			return true;
		}

		private static Match MatchExactlyOnce(Regex pattern, string text, string what)
		{
			Match match = pattern.Match(text);
			if (!match.Success)
			{
				throw new Exception(what + " could not be located");
			}
			if (match.NextMatch().Success)
			{
				throw new Exception(what + " matched more than once; cannot tell which call site is the right one");
			}
			return match;
		}
	}
}
