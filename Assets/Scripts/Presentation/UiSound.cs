using UnityEngine;

namespace CyberUnderground.Presentation
{
    public static class UiSound
    {
        static AudioSource _source;
        static AudioClip _click;
        static AudioClip[] _keys;
        static AudioClip _ok;

        public static void Click()
        {
            Play(ref _click, Blip(1680f, 0.04f, 46f), 0.42f);
        }

        public static void Key()
        {
            Ensure();
            if (_keys == null)
            {
                _keys = new AudioClip[6];
                for (int i = 0; i < _keys.Length; i++)
                    _keys[i] = Switch(i + 3);
            }
            int pick = Random.Range(0, _keys.Length);
            _source.PlayOneShot(_keys[pick], Random.Range(0.62f, 0.82f));
        }

        public static void Confirm()
        {
            Play(ref _ok, Chord(), 0.4f);
        }

        static void Play(ref AudioClip clip, AudioClip created, float volume)
        {
            Ensure();
            if (clip == null)
                clip = created;
            _source.PlayOneShot(clip, volume);
        }

        static void Ensure()
        {
            if (_source != null)
                return;
            var host = new GameObject("UiSound");
            Object.DontDestroyOnLoad(host);
            _source = host.AddComponent<AudioSource>();
            _source.playOnAwake = false;
            _source.spatialBlend = 0f;
            if (Object.FindAnyObjectByType<AudioListener>() == null)
                host.AddComponent<AudioListener>();
        }

        static AudioClip Switch(int seed)
        {
            const int rate = 44100;
            int count = (int)(rate * 0.06f);
            var data = new float[count];
            var rng = new System.Random(seed * 97);
            float low = 0f;
            float pitch = 0.9f + (seed % 6) * 0.045f;
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)rate;
                float white = (float)(rng.NextDouble() * 2.0 - 1.0);
                low += (white - low) * 0.22f;
                float click = (white - low) * Mathf.Exp(-t * 520f);
                float thock = Mathf.Sin(t * 210f * pitch * 6.2831853f) * Mathf.Exp(-t * 48f);
                float stem = 0f;
                float down = t - 0.011f;
                if (down > 0f)
                    stem = low * Mathf.Exp(-down * 160f);
                data[i] = click * 0.85f + thock * 0.42f + stem * 0.55f;
            }
            float peak = 0.001f;
            for (int i = 0; i < count; i++)
            {
                float a = Mathf.Abs(data[i]);
                if (a > peak)
                    peak = a;
            }
            float gain = 0.9f / peak;
            for (int i = 0; i < count; i++)
                data[i] *= gain;
            var clip = AudioClip.Create("key-" + seed, count, 1, rate, false);
            clip.SetData(data, 0);
            return clip;
        }

        static AudioClip Blip(float freq, float seconds, float decay)
        {
            const int rate = 22050;
            int count = Mathf.Max(1, (int)(rate * seconds));
            var data = new float[count];
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)rate;
                data[i] = Mathf.Sin(t * freq * 6.2831853f) * Mathf.Exp(-t * decay);
            }
            var clip = AudioClip.Create("ui-blip", count, 1, rate, false);
            clip.SetData(data, 0);
            return clip;
        }

        static AudioClip Chord()
        {
            const int rate = 22050;
            int count = (int)(rate * 0.22f);
            var data = new float[count];
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)rate;
                float a = t < 0.09f ? Mathf.Sin(t * 523f * 6.2831853f) * Mathf.Exp(-t * 18f) : 0f;
                float u = t - 0.08f;
                float b = u > 0f ? Mathf.Sin(u * 784f * 6.2831853f) * Mathf.Exp(-u * 14f) : 0f;
                data[i] = (a + b) * 0.7f;
            }
            var clip = AudioClip.Create("ui-ok", count, 1, rate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
