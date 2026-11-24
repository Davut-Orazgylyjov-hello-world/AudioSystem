# Audio System 2

Звук одной строкой. Пул источников, без корутин и аллокаций на каждый звук.

## Установка

Package Manager → Add package from git URL:

`https://github.com/Davut-Orazgylyjov-hello-world/AudioSystem.git#v2`

Старая версия 1.x живёт в ветке `main` (тот же URL без `#v2`).

## Использование

`using AudioSystem;`

| Задача | Код |
|---|---|
| Эффект 2D | `Sound.Play(clip);` |
| Эффект 3D в точке | `Sound.Play(clip, position);` |
| Голос 2D | `Sound.Voice(clip);` |
| Голос 3D в точке | `Sound.Voice(clip, position);` |
| Музыка | `Sound.Music(clip);` |
| Остановить музыку | `Sound.StopMusic();` |
| Звук во время паузы | `Sound.InPause(clip);` |
| С параметрами | `Sound.Clip(clip).Volume(0.5f).RandomPitch().Play();` |
| Управление звуком | `SoundHandle sound = Sound.Clip(clip).Loop().Play();` затем `sound.Stop();` |
| Остановить всё | `Sound.StopAll();` |
| Громкость канала | `Sound.SetVolume(SoundChannel.Music, 0.5f);` |

Музыка играет одной зацикленной дорожкой: новый клип плавно сменяет текущий, тот же клип повторно не перезапускается. Время перехода задаётся вторым аргументом: `Sound.Music(clip, 2f)`, `Sound.StopMusic(0.5f)`.

Звуки паузы идут в отдельную группу Pause и не замирают при `AudioListener.pause`. От `Time.timeScale` плагин не зависит.

Параметры после `Sound.Clip(clip)`:

| Параметр | Что делает |
|---|---|
| `Voice()` | канал голоса вместо эффектов |
| `At(position)` | 3D-звук в точке |
| `Follow(transform)` | 3D-звук следует за объектом |
| `Volume(v)` / `Pitch(p)` | множитель к значению из настроек |
| `RandomPitch(range)` | случайный сдвиг высоты, по умолчанию ±0.05 |
| `Duration(seconds)` | остановить через время |
| `FadeIn(seconds)` | плавное появление |
| `Loop()` | зациклить, останавливать через `SoundHandle` |
| `Frequent()` | массовый звук: сверх лимита останавливается самый старый |
| `Config(configuration)` / `Settings(soundSettings)` | явный пресет или свои настройки |

`SoundHandle`: `IsPlaying`, `Stop()`, `Stop(fadeSeconds)`, `SetVolume(v)`, `SetPitch(p)`.

Компонент `RandomSound` играет случайный клип из списка без кода.

## Настройка

Все настройки в одном окне: **Tools → Audio System → Settings** (оно же Edit → Project Settings → Audio System).

- **Pool.** `Prewarm Count`, `Max Pool Size`, `Max Frequent Sounds`.
- **Volume.** Микшер и стартовая громкость каналов Master, Effects, Voice, Music, Pause.
- **Presets.** Параметры источника для каждого типа: Effect2D, Effect3D, Voice2D, Voice3D, Music, Pause.
- **Unity Audio Limits.** Max Real Voices и Max Virtual Voices проекта: всё сверх лимита Unity не воспроизводит.

Пакет из git доступен только для чтения, поэтому в проекте нажмите **Create Project Settings**: появится `Assets/Resources/AudioSystemSettings.asset`, и плагин будет брать настройки из него.

Громкость: один микшер `Settings/AudioSystemMixer` с группами Master → Effects, Voice, Music, Pause. Из кода: `Sound.SetVolume(канал, 0..1)` и `Sound.GetVolume(канал)`, значение от 0 до 1. Сохранение выбора игрока остаётся за игрой. Напрямую в микшере доступны параметры `MasterAudio`, `EffectsAudio`, `VoiceAudio`, `MusicAudio`, `PauseAudio` и такие же с окончанием `Pitch`.

Свой пресет: Create → AudioSystem → SoundSettings, затем `Sound.Clip(clip).Settings(preset).Play();`.

Префабы и объекты на сцене не нужны: плагин создаёт всё сам при старте игры.
