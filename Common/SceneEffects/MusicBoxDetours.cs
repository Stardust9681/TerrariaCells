using System;
using System.Reflection;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;

namespace TerrariaCells.Common.SceneEffects;

public class MusicBoxDetours : ModSystem {
    const string UndergroundDesertMusicBoxOverride = "TerrariaCells/Pyramid 1.2(2)";

    private Exception thrown;
    public override void Load()
    {
        Terraria.On_Main.UpdateAudio += On_Main_UpdateAudio;
    }
    private static T GetField<T>(string field) {
        return (T)typeof(Main).GetField(field, BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
    }
    private static T GetField<T>(string field, Main instance) {
        return (T)typeof(Main).GetField(field, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(instance);
    }
    private static void SetField<T>(string field, T value) {
        typeof(Main).GetField(field, BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, value);
    }

    private void On_Main_UpdateAudio(Terraria.On_Main.orig_UpdateAudio orig, Main instance)
    {
        // Main.curMusic = 1;
        // orig(instance);
		if (!SoundEngine.IsAudioSupported || !GetField<bool>("_musicLoaded", instance))
			return;

		if (!Main.dedServ && !Main.showSplash)
			SoundEngine.Update();

		if (Main.musicVolume == 0f)
			Main.curMusic = 0;

		try {
			if (Main.dedServ)
				return;

            if (!instance.IsActive) {
				Main.audioSystem.PauseAll();
				SoundEngine.StopAmbientSounds();
			}
			else {
				Main.audioSystem.ResumeAll();
			}


			if (Main.gameMenu)
				SetField("swapMusic", false);

			// if (GetField<bool>("swapMusic")) {
            //     typeof(Main).GetMethod("UpdateAudio_DecideOnTOWMusic").Invoke(instance, []);
			// }
			// else if (!gameMenu && drunkWorld && !remixWorld) {
				// UpdateAudio_DecideOnTOWMusic();
			// }
			// else {
            // Main.NewText(Main.audioSystem);
                typeof(Main)
                    .GetMethod("UpdateAudio_DecideOnNewMusic", BindingFlags.NonPublic | BindingFlags.InvokeMethod)
                    .Invoke(instance, []);
            // }

            // Main.NewText("music?");
            SceneMetrics SceneMetrics = GetField<SceneMetrics>("SceneMetrics");

            ref int newMusic = ref Main.newMusic;

			if (Main.musicBox2 >= 0)
				SceneMetrics.ActiveMusicBox = Main.musicBox2;

			if (SceneMetrics.ActiveMusicBox >= 0) {
                if (SceneMetrics.ActiveMusicBox == 57) {

                    newMusic = MusicLoader.GetMusicSlot(UndergroundDesertMusicBoxOverride);
                }
                else if (SceneMetrics.ActiveMusicBox >= 0 && SceneMetrics.ActiveMusicBox <= 26) {
                    newMusic = SceneMetrics.ActiveMusicBox + 1;
                }
                else if (SceneMetrics.ActiveMusicBox >= 27 && SceneMetrics.ActiveMusicBox <= 31) {
                    newMusic = SceneMetrics.ActiveMusicBox + 2;
                }
				else if (SceneMetrics.ActiveMusicBox == 32)
					newMusic = 38;

				else if (SceneMetrics.ActiveMusicBox == 33)
					newMusic = 37;

				else if (SceneMetrics.ActiveMusicBox == 34)
					newMusic = 35;

				else if (SceneMetrics.ActiveMusicBox == 35)
					newMusic = 36;

				else if (SceneMetrics.ActiveMusicBox == 36)
					newMusic = 34;

				else if (SceneMetrics.ActiveMusicBox == 37)
					newMusic = 39;

				else if (SceneMetrics.ActiveMusicBox == 38)
					newMusic = 40;

				else if (SceneMetrics.ActiveMusicBox == 39)
					newMusic = 41;

				else if (SceneMetrics.ActiveMusicBox == 40)
					newMusic = 44;

				else if (SceneMetrics.ActiveMusicBox == 41)
					newMusic = 48;

				else if (SceneMetrics.ActiveMusicBox == 42)
					newMusic = 42;

				else if (SceneMetrics.ActiveMusicBox == 43)
					newMusic = 43;

				else if (SceneMetrics.ActiveMusicBox == 44)
					newMusic = 46;

				else if (SceneMetrics.ActiveMusicBox == 45)
					newMusic = 47;

				else if (SceneMetrics.ActiveMusicBox == 46)
					newMusic = 49;

                else if (SceneMetrics.ActiveMusicBox >= 47 && SceneMetrics.ActiveMusicBox <= 87) {
                    newMusic = SceneMetrics.ActiveMusicBox + 4;
                }

				if (SceneMetrics.ActiveMusicBox >= Main.maxMusic)
					newMusic = SceneMetrics.ActiveMusicBox;
			}

			if (Main.gameMenu || Main.musicVolume == 0f) {
				Main.musicBox2 = -1;
				SceneMetrics.ActiveMusicBox = -1;
			}

			if (Main.musicVolume == 0f)
				newMusic = 0;

			Main.audioSystem.Update();
			Main.audioSystem.UpdateMisc();
			Main.curMusic = newMusic;
			float num = 1f;
			if (NPC.MoonLordCountdown > 0) {
				num = (float)NPC.MoonLordCountdown / (float)NPC.MaxMoonLordCountdown;
				num *= num;
				if ((float)NPC.MoonLordCountdown > (float)NPC.MaxMoonLordCountdown * 0.2f) {
					num = MathHelper.Lerp(0f, 1f, num);
				}
				else {
					num = 0f;
					Main.curMusic = 0;
				}

				if (NPC.MoonLordCountdown == 1 && Main.curMusic >= 1 && Main.curMusic < Main.musicFade.Length)
					Main.musicFade[Main.curMusic] = 0f;
			}

			bool isMainTrackAudible = Main.musicFade[Main.curMusic] > 0.25f;
			for (int i = 1; i < Main.musicFade.Length; i++) {
				float num2 = Main.musicFade[i] * Main.musicVolume * num;
				if (i >= 62 && i <= 88) {
					num2 *= 0.9f;
				}
				else if (i == 52) {
					num2 *= 1.15f;
					if (num2 > 1f)
						num2 = 1f;
				}

				float num3 = Main.shimmerAlpha;
				switch (i) {
					case 28: { // pumpkin moon
						float num7 = 0.5f;
						float num8 = Main.cloudAlpha / 9f * 10f * num7 + (1f - num7);
						if (num3 > 0f)
							num8 *= 1f - num3;

						if (num8 > 1f)
							num8 = 1f;

						if (Main.gameMenu)
							num8 = 0f;

						num8 *= (float)Math.Pow(Main.atmo, 4.0);
						// if (Main.remixWorld) {
						// 	if (Main.cloudAlpha > 0f && (double)(Main.LocalPlayer.position.Y / 16f) > Main.rockLayer && Main.LocalPlayer.position.Y / 16f < (float)(Main.maxTilesY - 350) && !Main.LocalPlayer.ZoneSnow && !Main.LocalPlayer.ZoneDungeon) {
						// 		float trackVolume5 = Main.musicFade[i];
						// 		Main.audioSystem.UpdateAmbientCueState(i, flag, ref trackVolume5, Main.ambientVolume * num8);
						// 		Main.musicFade[i] = trackVolume5;
						// 	}
						// 	else {
						// 		float trackVolume6 = Main.musicFade[i];
						// 		Main.audioSystem.UpdateAmbientCueTowardStopping(i, 0.005f, ref trackVolume6, Main.ambientVolume * num8);
						// 		Main.musicFade[i] = trackVolume6;
						// 	}
						// }
						// else
                         if (Main.cloudAlpha > 0f && (double)Main.LocalPlayer.position.Y < Main.worldSurface * 16.0 + (double)(Main.screenHeight / 2) && !Main.LocalPlayer.ZoneSnow) {
							float trackVolume7 = Main.musicFade[i];
							Main.audioSystem.UpdateAmbientCueState(i, instance.IsActive, ref trackVolume7, Main.ambientVolume * num8);
							Main.musicFade[i] = trackVolume7;
						}
						else {
							float trackVolume8 = Main.musicFade[i];
							Main.audioSystem.UpdateAmbientCueTowardStopping(i, 0.005f, ref trackVolume8, Main.ambientVolume * num8);
							Main.musicFade[i] = trackVolume8;
						}

						break;
					}
					case 45: {
						float num4 = 0.7f;
						float num5 = Math.Abs(Main.windSpeedCurrent) * num4 + (1f - num4);
						if (num3 > 0f)
							num5 *= 1f - num3;

						if (num5 > 1f)
							num5 = 1f;

						num5 *= 0.9f;
						// float num6 = 20f;
						num5 *= (float)Math.Pow(Main.atmo, 4.0);
						// if (remixWorld) {
						// 	if (!gameMenu && Math.Abs(windSpeedCurrent) >= num6 / 50f && (double)(Main.LocalPlayer.position.Y / 16f) > rockLayer && Main.LocalPlayer.position.Y / 16f < (float)(maxTilesY - 350) && !Main.LocalPlayer.ZoneDungeon) {
						// 		float trackVolume = musicFade[i];
						// 		audioSystem.UpdateAmbientCueState(i, flag, ref trackVolume, ambientVolume * num5);
						// 		musicFade[i] = trackVolume;
						// 	}
						// 	else {
						// 		float trackVolume2 = musicFade[i];
						// 		audioSystem.UpdateAmbientCueTowardStopping(i, 0.005f, ref trackVolume2, ambientVolume * num5);
						// 		musicFade[i] = trackVolume2;
						// 	}
						// } else 
                        // if (!Main.gameMenu && Math.Abs(Main.windSpeedCurrent) >= num6 / 50f && (double)Main.LocalPlayer.position.Y < worldSurface * 16.0 + (double)(screenHeight / 2)) {
						// 	float trackVolume3 = musicFade[i];
						// 	audioSystem.UpdateAmbientCueState(i, flag, ref trackVolume3, ambientVolume * num5);
						// 	musicFade[i] = trackVolume3;
						// }
						// else {
							float trackVolume4 = Main.musicFade[i];
							Main.audioSystem.UpdateAmbientCueTowardStopping(i, 0.005f, ref trackVolume4, Main.ambientVolume * num5);
							Main.musicFade[i] = trackVolume4;
						// }

						break;
					}
					default: {
						float tempFade = Main.musicFade[i];
						if (i == Main.curMusic)
							Main.audioSystem.UpdateCommonTrack(instance.IsActive, i, num2, ref tempFade);
						else
							Main.audioSystem.UpdateCommonTrackTowardStopping(i, num2, ref tempFade, isMainTrackAudible);

						Main.musicFade[i] = tempFade;
						break;
					}
				}
			}

			Main.audioSystem.UpdateAudioEngine();
			if (Main.musicError > 0)
				Main.musicError--;
		}
		catch (Exception exception) {
            if (thrown == null || exception.Message != thrown.Message) {
                ModContent.GetInstance<TerrariaCells>().Logger.Error(exception);
                thrown = exception;
            }
			Main.musicError++;
			if (Main.musicError >= 100) {
				Main.musicError = 0;
				Main.musicVolume = 0f;
			}
	}

    }
}
