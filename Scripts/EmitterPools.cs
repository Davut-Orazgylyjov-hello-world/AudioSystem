using UnityEngine;

namespace AudioSystem
{
    internal sealed class EmitterPools
    {
        private readonly Transform _parent;
        private readonly int _maxIdle;
        private EmitterPool[] _pools = new EmitterPool[8];
        private EmitterPool _last;
        private int _count;
        private int _idleCount;

        internal EmitterPools(Transform parent, int maxIdle)
        {
            _parent = parent;
            _maxIdle = maxIdle;
        }

        internal SoundEmitter Take(SoundSettings settings)
        {
            EmitterPool pool = Get(settings);
            if (pool.Count == 0) return pool.Spawn();

            _idleCount--;
            return pool.Pop();
        }

        internal void Release(SoundEmitter emitter)
        {
            if (_idleCount < _maxIdle)
            {
                _idleCount++;
            }
            else if (!DestroyColdest())
            {
                Object.Destroy(emitter.gameObject);
                return;
            }

            emitter.Pool.Push(emitter);
        }

        internal void Prewarm(SoundSettings[] presets, int presetCount, int count)
        {
            if (presets == null) return;

            if (presetCount > presets.Length) presetCount = presets.Length;
            if (presetCount == 0) return;

            if (count > _maxIdle) count = _maxIdle;
            for (int i = 0; i < count; i++)
            {
                SoundSettings settings = presets[i % presetCount];
                if (settings == null) continue;

                Get(settings).Prewarm();
                _idleCount++;
            }
        }

        internal void DisableIdle()
        {
            for (int i = 0; i < _count; i++)
                _pools[i].DisableIdle();
        }

        private EmitterPool Get(SoundSettings settings)
        {
            EmitterPool pool = _last;
            if (!ReferenceEquals(pool, null) && ReferenceEquals(pool.Settings, settings)) return pool;

            for (int i = 0; i < _count; i++)
            {
                pool = _pools[i];
                if (!ReferenceEquals(pool.Settings, settings)) continue;

                _last = pool;
                return pool;
            }

            if (_count == _pools.Length)
                System.Array.Resize(ref _pools, _count * 2);

            pool = new EmitterPool(settings, _parent);
            _pools[_count++] = pool;
            _last = pool;
            return pool;
        }

        private bool DestroyColdest()
        {
            EmitterPool coldest = null;
            int disabled = 0;
            for (int i = 0; i < _count; i++)
            {
                EmitterPool pool = _pools[i];
                if (pool.DisabledCount <= disabled) continue;

                disabled = pool.DisabledCount;
                coldest = pool;
            }

            if (ReferenceEquals(coldest, null)) return false;

            coldest.DestroyColdest();
            return true;
        }
    }
}
