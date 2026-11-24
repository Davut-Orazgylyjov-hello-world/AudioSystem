namespace AudioSystem
{
    public readonly struct SoundHandle
    {
        private readonly SoundEmitter _emitter;
        private readonly int _generation;

        internal SoundHandle(SoundEmitter emitter)
        {
            _emitter = emitter;
            _generation = emitter.Generation;
        }

        public bool IsPlaying => !ReferenceEquals(_emitter, null) && _emitter.Generation == _generation;

        public void Stop()
        {
            if (IsPlaying) AudioController.Stop(_emitter);
        }

        public void Stop(float fadeSeconds)
        {
            if (!IsPlaying) return;

            if (fadeSeconds > 0f)
                _emitter.FadeOut(fadeSeconds);
            else
                AudioController.Stop(_emitter);
        }

        public void SetVolume(float volume)
        {
            if (IsPlaying) _emitter.SetVolume(volume);
        }

        public void SetPitch(float pitch)
        {
            if (IsPlaying) _emitter.SetPitch(pitch);
        }
    }
}
