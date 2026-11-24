using AudioSystem;
using UnityEditor;
using UnityEngine;

namespace AudioSystemEditor
{
    internal static class AudioSystemSettingsProvider
    {
        private const string SettingsPath = "Project/Audio System";
        private const string UnityAudioPath = "Project/Audio";
        private const string ProjectFolder = "Assets/Resources";
        private const string FoldoutKey = "AudioSystem.Settings.Foldout.";

        private static readonly string[] PresetNames = System.Enum.GetNames(typeof(SoundConfiguration));
        private static readonly string[] ChannelNames = System.Enum.GetNames(typeof(SoundChannel));

        private static GUIStyle _padding;
        private static GUIStyle _boldFoldout;

        [MenuItem("Tools/Audio System/Settings")]
        private static void Open()
        {
            SettingsService.OpenProjectSettings(SettingsPath);
        }

        [SettingsProvider]
        private static SettingsProvider Create()
        {
            SettingsProvider provider = new SettingsProvider(SettingsPath, SettingsScope.Project);
            provider.label = "Audio System";
            provider.guiHandler = Draw;
            provider.keywords = new[] { "audio", "sound", "pool", "preset", "mixer", "voice" };
            return provider;
        }

        private static void Draw(string searchContext)
        {
            if (_padding == null)
            {
                _padding = new GUIStyle();
                _padding.padding = new RectOffset(10, 10, 10, 10);
                _boldFoldout = new GUIStyle(EditorStyles.foldout);
                _boldFoldout.fontStyle = FontStyle.Bold;
            }

            float labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 220f;
            EditorGUILayout.BeginVertical(_padding);

            AudioSystemSettings project = Resources.Load<AudioSystemSettings>(AudioSystemSettings.ProjectResource);
            AudioSystemSettings settings = project != null ? project : Resources.Load<AudioSystemSettings>(AudioSystemSettings.DefaultResource);
            if (settings == null)
            {
                EditorGUILayout.HelpBox("Settings asset is missing: Resources/" + AudioSystemSettings.DefaultResource, MessageType.Error);
            }
            else
            {
                DrawSource(settings, project == null);

                using (new EditorGUI.DisabledScope(!IsEditable(settings)))
                {
                    DrawSettings(settings);
                    DrawPresets(settings);
                }
            }

            DrawUnityLimits();

            EditorGUILayout.EndVertical();
            EditorGUIUtility.labelWidth = labelWidth;
        }

        private static void DrawSource(AudioSystemSettings settings, bool isDefault)
        {
            using (new EditorGUI.DisabledScope(true))
                EditorGUILayout.ObjectField("Settings Asset", settings, typeof(AudioSystemSettings), false);

            if (isDefault)
            {
                EditorGUILayout.HelpBox("Package defaults are in use. Create project settings to change them for this project only.", MessageType.Info);
                if (GUILayout.Button("Create Project Settings", GUILayout.Width(220f)))
                {
                    CreateProjectSettings(settings);
                    GUIUtility.ExitGUI();
                }
            }

            if (Application.isPlaying)
                EditorGUILayout.HelpBox("Volume applies at once, presets apply to the next sound, pool values apply on the next start.", MessageType.None);
        }

        private static void DrawSettings(AudioSystemSettings settings)
        {
            using (SerializedObject serialized = new SerializedObject(settings))
            {
                DrawPool(serialized);
                DrawVolume(serialized);
                serialized.ApplyModifiedProperties();
            }
        }

        private static void DrawPool(SerializedObject serialized)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Pool", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serialized.FindProperty(nameof(AudioSystemSettings.prewarmCount)));
            EditorGUILayout.PropertyField(serialized.FindProperty(nameof(AudioSystemSettings.maxPoolSize)));
            EditorGUILayout.PropertyField(serialized.FindProperty(nameof(AudioSystemSettings.maxFrequentSounds)));
        }

        private static void DrawVolume(SerializedObject serialized)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Volume", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serialized.FindProperty(nameof(AudioSystemSettings.mixer)));

            SerializedProperty volumes = serialized.FindProperty(nameof(AudioSystemSettings.volumes));
            if (volumes.arraySize != ChannelNames.Length) volumes.arraySize = ChannelNames.Length;

            for (int i = 0; i < ChannelNames.Length; i++)
            {
                SerializedProperty element = volumes.GetArrayElementAtIndex(i);
                EditorGUI.BeginChangeCheck();
                float value = EditorGUILayout.Slider(ChannelNames[i], element.floatValue, 0f, 1f);
                if (!EditorGUI.EndChangeCheck()) continue;

                element.floatValue = value;
                if (Application.isPlaying) Sound.SetVolume((SoundChannel)i, value);
            }
        }

        private static void DrawPresets(AudioSystemSettings settings)
        {
            EditorGUILayout.Space();
            if (!Foldout("Presets", true, _boldFoldout)) return;

            EditorGUI.indentLevel++;
            for (int i = 0; i < PresetNames.Length; i++)
            {
                if (!Foldout(PresetNames[i], false, EditorStyles.foldout)) continue;

                SoundSettings preset = settings.GetPreset((SoundConfiguration)i);
                EditorGUI.indentLevel++;
                if (preset == null)
                    EditorGUILayout.HelpBox("Preset is not assigned", MessageType.Warning);
                else
                    DrawPreset(preset);
                EditorGUI.indentLevel--;
            }

            EditorGUI.indentLevel--;
        }

        private static bool Foldout(string label, bool openByDefault, GUIStyle style)
        {
            string key = FoldoutKey + label;
            bool wasOpen = SessionState.GetBool(key, openByDefault);
            bool isOpen = EditorGUILayout.Foldout(wasOpen, label, true, style);
            if (isOpen != wasOpen) SessionState.SetBool(key, isOpen);
            return isOpen;
        }

        private static void DrawPreset(SoundSettings preset)
        {
            using (SerializedObject serialized = new SerializedObject(preset))
            {
                SerializedProperty property = serialized.GetIterator();
                bool enterChildren = true;
                while (property.NextVisible(enterChildren))
                {
                    enterChildren = false;
                    if (property.propertyPath == "m_Script") continue;

                    EditorGUILayout.PropertyField(property, true);
                }

                serialized.ApplyModifiedProperties();
            }
        }

        private static void DrawUnityLimits()
        {
            AudioConfiguration configuration = AudioSettings.GetConfiguration();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Unity Audio Limits", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Max Real Voices (audible)", configuration.numRealVoices.ToString());
            EditorGUILayout.LabelField("Max Virtual Voices (playing)", configuration.numVirtualVoices.ToString());
            if (GUILayout.Button("Open Unity Audio Settings", GUILayout.Width(220f)))
                SettingsService.OpenProjectSettings(UnityAudioPath);
        }

        private static bool IsEditable(AudioSystemSettings settings)
        {
            string path = AssetDatabase.GetAssetPath(settings);
            UnityEditor.PackageManager.PackageInfo package = UnityEditor.PackageManager.PackageInfo.FindForAssetPath(path);
            if (package != null
                && package.source != UnityEditor.PackageManager.PackageSource.Embedded
                && package.source != UnityEditor.PackageManager.PackageSource.Local)
                return false;

            return AssetDatabase.IsOpenForEdit(settings, StatusQueryOptions.UseCachedIfPossible);
        }

        private static void CreateProjectSettings(AudioSystemSettings defaults)
        {
            if (!AssetDatabase.IsValidFolder(ProjectFolder))
                AssetDatabase.CreateFolder("Assets", "Resources");

            AudioSystemSettings copy = Object.Instantiate(defaults);
            copy.name = AudioSystemSettings.ProjectResource;
            AssetDatabase.CreateAsset(copy, ProjectFolder + "/" + AudioSystemSettings.ProjectResource + ".asset");

            for (int i = 0; i < copy.presets.Length; i++)
            {
                SoundSettings source = defaults.presets[i];
                if (source == null) continue;

                SoundSettings preset = Object.Instantiate(source);
                preset.name = source.name;
                AssetDatabase.AddObjectToAsset(preset, copy);
                copy.presets[i] = preset;
            }

            EditorUtility.SetDirty(copy);
            AssetDatabase.SaveAssets();
            EditorGUIUtility.PingObject(copy);
        }
    }
}
