using UnityEngine;

namespace AudioSystem
{
    internal sealed class EmitterPool
    {
        internal readonly SoundSettings Settings;

        private readonly Transform _parent;
        private SoundEmitter _template;
        private SoundEmitter[] _items = new SoundEmitter[32];
        private int _count;
        private int _disabledCount;
        private int _lowMark;

        internal EmitterPool(SoundSettings settings, Transform parent)
        {
            Settings = settings;
            _parent = parent;
        }

        internal int Count => _count;
        internal int DisabledCount => _disabledCount;

        internal SoundEmitter Pop()
        {
            int index = --_count;
            SoundEmitter emitter = _items[index];
            _items[index] = null;

            if (index < _lowMark) _lowMark = index;
            if (index < _disabledCount)
            {
                _disabledCount = index;
                emitter.Wake();
            }

            return emitter;
        }

        internal void Push(SoundEmitter emitter)
        {
            if (_count == _items.Length)
                System.Array.Resize(ref _items, _count * 2);

            _items[_count++] = emitter;
        }

        internal SoundEmitter Spawn()
        {
            SoundEmitter emitter = Create();
            emitter.Wake();
            return emitter;
        }

        internal void Prewarm()
        {
            Push(Create());
            _disabledCount = _count;
            _lowMark = _count;
        }

        internal void DisableIdle()
        {
            int from = _disabledCount;
            int to = _lowMark;
            _lowMark = _count;
            if (to <= from) return;

            for (int i = from; i < to; i++)
                _items[i].Sleep();

            _disabledCount = to;
        }

        internal void DestroyColdest()
        {
            int index = _disabledCount - 1;
            int last = _count - 1;
            Object.Destroy(_items[index].gameObject);
            _items[index] = _items[last];
            _items[last] = null;
            _count = last;
            _disabledCount = index;
            if (_lowMark > last) _lowMark = last;
        }

        private SoundEmitter Create()
        {
            int hash = Settings.ComputeHash();
            if (_template == null)
                _template = SoundEmitter.CreateTemplate(Settings, hash, _parent);
            else if (_template.SettingsHash != hash)
                _template.ApplySettings(Settings, hash);

            SoundEmitter emitter = _template.Clone(_parent);
            emitter.Pool = this;
            return emitter;
        }
    }
}
