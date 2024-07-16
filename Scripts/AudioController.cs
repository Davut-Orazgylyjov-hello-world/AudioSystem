using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Pool;

namespace AudioSystem
{
    public enum SoundConfiguration : byte
    {
        Effect2D,
        Effect3D,
        Voice2D,
        Voice3D
    }

    public class AudioController : MonoBehaviour
    {
        [SerializeField] private SoundSettings[] soundSettings;

        private static AudioController _instance;

        public static readonly LinkedList<SoundEmitter> FrequentSoundEmitters = new();

        [SerializeField] private SoundEmitter soundEmitterPrefab;
        [SerializeField] private bool collectionCheck = true;
        [SerializeField] private int defaultCapacity = 10;
        [SerializeField] private int maxPoolSize = 100;
        [SerializeField] private int maxSoundInstances = 30;

        private IObjectPool<SoundEmitter> _soundEmitterPool;
        private readonly List<SoundEmitter> _activeSoundEmitters = new();


        private void Awake()
        {
            if (_instance == null)
                _instance = this;
        }

        private void Start()
        {
            InitializePool();
        }

        public static Transform Transform => _instance.transform;

        public static SoundSettings GetSoundSettings(SoundConfiguration soundConfiguration)
        {
            return _instance.soundSettings[(int) soundConfiguration];
        }

        public static SoundBuilder CreateSoundBuilder() => new();

        public static bool CanPlaySound(SoundData data)
        {
            if (!data.soundSettings.frequentSound) return true;

            if (FrequentSoundEmitters.Count < _instance.maxSoundInstances) return true;

            try
            {
                FrequentSoundEmitters.First.Value.Stop();
                return true;
            }
            catch
            {
                Debug.Log("SoundEmitter is already released");
            }

            return false;
        }

        public static SoundEmitter Get()
        {
            return _instance._soundEmitterPool.Get();
        }

        public static void ReturnToPool(SoundEmitter soundEmitter)
        {
            _instance._soundEmitterPool.Release(soundEmitter);
        }

        public void StopAll()
        {
            foreach (var soundEmitter in _activeSoundEmitters)
            {
                soundEmitter.Stop();
            }

            FrequentSoundEmitters.Clear();
        }

        private void InitializePool()
        {
            _soundEmitterPool = new ObjectPool<SoundEmitter>(
                CreateSoundEmitter,
                OnTakeFromPool,
                OnReturnedToPool,
                OnDestroyPoolObject,
                collectionCheck,
                defaultCapacity,
                maxPoolSize);
        }

        private SoundEmitter CreateSoundEmitter()
        {
            var soundEmitter = Instantiate(soundEmitterPrefab);
            soundEmitter.gameObject.SetActive(false);
            return soundEmitter;
        }

        private void OnTakeFromPool(SoundEmitter soundEmitter)
        {
            soundEmitter.gameObject.SetActive(true);
            _activeSoundEmitters.Add(soundEmitter);
        }

        private void OnReturnedToPool(SoundEmitter soundEmitter)
        {
            if (soundEmitter.Node != null)
            {
                FrequentSoundEmitters.Remove(soundEmitter.Node);
                soundEmitter.Node = null;
            }

            soundEmitter.gameObject.SetActive(false);
            _activeSoundEmitters.Remove(soundEmitter);
        }

        private void OnDestroyPoolObject(SoundEmitter soundEmitter)
        {
            Destroy(soundEmitter.gameObject);
        }
    }
}