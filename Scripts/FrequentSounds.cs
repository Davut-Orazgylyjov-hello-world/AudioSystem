namespace AudioSystem
{
    internal sealed class FrequentSounds
    {
        internal int Count;

        private SoundEmitter _oldest;
        private SoundEmitter _newest;

        internal SoundEmitter Oldest => _oldest;

        internal void Add(SoundEmitter emitter)
        {
            emitter.Frequent = true;
            emitter.FrequentPrev = _newest;

            if (ReferenceEquals(_newest, null))
                _oldest = emitter;
            else
                _newest.FrequentNext = emitter;

            _newest = emitter;
            Count++;
        }

        internal void Remove(SoundEmitter emitter)
        {
            SoundEmitter prev = emitter.FrequentPrev;
            SoundEmitter next = emitter.FrequentNext;

            if (ReferenceEquals(prev, null))
                _oldest = next;
            else
                prev.FrequentNext = next;

            if (ReferenceEquals(next, null))
                _newest = prev;
            else
                next.FrequentPrev = prev;

            emitter.FrequentPrev = null;
            emitter.FrequentNext = null;
            emitter.Frequent = false;
            Count--;
        }
    }
}
