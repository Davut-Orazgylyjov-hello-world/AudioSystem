## [1.0.0] - 17 Jule 2024
 - Initial Version
## [1.0.1] - 21 September 2024
 - Upgrarded Random Sound script and object
## [1.0.2] - 21 September 2024
 - Fixed runtime scripts bug
## [2.0.0] - 24 November 2026
 - New short API: Sound.Play, Sound.Voice, Sound.Music, Sound.InPause, Sound.Clip(clip)...Play()
 - Play returns SoundHandle: IsPlaying, Stop, Stop(fadeSeconds), SetVolume, SetPitch
 - Builder options: Voice, At, Follow, Volume, Pitch, RandomPitch, Duration, FadeIn, Loop, Frequent, Config, Settings
 - Music: one looped track with crossfade, Sound.Music(clip) and Sound.StopMusic()
 - Fades do not jump after a long frame such as a scene load
 - Pause preset: Sound.InPause(clip) plays while the game is paused
 - Channel volume: Sound.SetVolume(channel, 0..1) and Sound.GetVolume(channel)
 - One AudioSystemMixer with groups Master > Effects, Voice, Music, Pause instead of four separate mixers
 - Settings window: Tools > Audio System > Settings (Project Settings > Audio System) with pool values, volume and presets
 - No prefabs: the controller and sound sources are created from code
 - No coroutines, no SetActive and no allocations per sound
 - Sound source pools per SoundSettings with prewarm, limit on sounds playing at once
 - Fixed: pitch was ignored, SoundSettings volume was ignored, StopAll exception, time limit leaking into the next sound
 - RandomSound moved to the AudioSystem namespace
 - Removed the 1.x API: CreateSoundBuilder, With* methods, SoundData, GetOrAdd extension; AudioController.GetSoundSettings is now Sound.GetSettings
 - Removed AudioController and SoundEmitter prefabs, SoundData assets, RandomSound variant prefabs, demo scene, TestDemo and Third Party Notices
 - Projects on 1.x stay on the main branch
