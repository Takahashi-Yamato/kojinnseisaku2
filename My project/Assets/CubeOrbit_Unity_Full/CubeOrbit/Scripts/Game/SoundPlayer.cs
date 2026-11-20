using System.Collections.Generic;
using UnityEngine;

namespace CubeOrbit
{
    public enum Sfx { Roll, Climb, Bump, Land, Gem, Switch, Checkpoint, Fall, Push, Crack, Crumble, Goal }

    /// <summary>
    /// 効果音。インスペクターで好きな AudioClip を入れられる。
    /// 何も入れていない音は、簡単な電子音を自動で作って鳴らす。
    /// </summary>
    public class SoundPlayer : MonoBehaviour
    {
        [System.Serializable]
        public class Entry
        {
            public Sfx type;
            public AudioClip clip;
            [Range(0f, 1f)] public float volume = 1f;
        }

        public static SoundPlayer Instance { get; private set; }
        static bool muted;

        [Range(0f, 1f)] public float masterVolume = 0.8f;
        [Tooltip("自分で用意した音（ここにないものは自動生成）")]
        public List<Entry> sounds = new List<Entry>();
        [Tooltip("AudioClip がない音を電子音で自動生成する")]
        public bool generateMissingSounds = true;

        AudioSource source;
        readonly Dictionary<Sfx, Entry> map = new Dictionary<Sfx, Entry>();
        int gemCount;

        void Awake()
        {
            Instance = this;
            muted = PlayerPrefs.GetInt("CubeOrbit_Muted", 0) == 1;
            source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            foreach (var e in sounds) if (e != null && e.clip != null) map[e.type] = e;
            if (generateMissingSounds)
                foreach (Sfx s in System.Enum.GetValues(typeof(Sfx)))
                    if (!map.ContainsKey(s)) map[s] = new Entry { type = s, clip = Synth.Make(s), volume = 1f };
        }

        void OnDestroy() { if (Instance == this) Instance = null; }

        public static bool Muted { get { return muted; } }

        public static void ToggleMute()
        {
            muted = !muted;
            PlayerPrefs.SetInt("CubeOrbit_Muted", muted ? 1 : 0);
        }

        public static void Play(Sfx s)
        {
            if (Instance == null || muted) return;
            Instance.PlayInternal(s);
        }

        void PlayInternal(Sfx s)
        {
            Entry e;
            if (!map.TryGetValue(s, out e) || e.clip == null) return;
            if (s == Sfx.Gem) { source.pitch = 1f + (gemCount++ % 5) * 0.12f; }
            else if (s == Sfx.Roll) source.pitch = Random.Range(0.92f, 1.08f);
            else source.pitch = 1f;
            source.PlayOneShot(e.clip, e.volume * masterVolume);
        }

        /// <summary>簡単な電子音を作る</summary>
        static class Synth
        {
            const int Rate = 44100;

            public static AudioClip Make(Sfx s)
            {
                switch (s)
                {
                    case Sfx.Roll: return Tone("roll", 0.07f, 160f, 0.6f, Wave.Triangle, 0.5f);
                    case Sfx.Climb: return Tone("climb", 0.12f, 200f, 1.6f, Wave.Triangle, 0.6f);
                    case Sfx.Bump: return Tone("bump", 0.08f, 90f, 0.7f, Wave.Square, 0.25f);
                    case Sfx.Land: return Tone("land", 0.09f, 110f, 0.5f, Wave.Triangle, 0.6f);
                    case Sfx.Gem: return Chord("gem", new[] { 880f, 1320f }, 0.22f, Wave.Sine, 0.06f, 0.6f);
                    case Sfx.Switch: return Chord("switch", new[] { 330f, 495f, 660f }, 0.4f, Wave.Square, 0.08f, 0.3f);
                    case Sfx.Checkpoint: return Chord("checkpoint", new[] { 520f, 780f }, 0.25f, Wave.Sine, 0.07f, 0.5f);
                    case Sfx.Fall: return Tone("fall", 0.6f, 500f, 0.15f, Wave.Saw, 0.3f);
                    case Sfx.Push: return Tone("push", 0.18f, 70f, 0.5f, Wave.Square, 0.5f);
                    case Sfx.Crack: return Noise("crack", 0.06f, 0.3f);
                    case Sfx.Crumble: return Noise("crumble", 0.35f, 0.5f);
                    case Sfx.Goal: return Chord("goal", new[] { 523f, 659f, 784f, 1047f, 1319f }, 0.7f, Wave.Triangle, 0.08f, 0.6f);
                }
                return null;
            }

            enum Wave { Sine, Square, Triangle, Saw }

            static float Osc(Wave w, float phase)
            {
                float p = phase - Mathf.Floor(phase);
                switch (w)
                {
                    case Wave.Square: return p < 0.5f ? 1f : -1f;
                    case Wave.Triangle: return 1f - 4f * Mathf.Abs(p - 0.5f);
                    case Wave.Saw: return 2f * p - 1f;
                    default: return Mathf.Sin(p * Mathf.PI * 2f);
                }
            }

            static float Env(float t, float dur) { return Mathf.Clamp01(t / 0.01f) * Mathf.Exp(-5f * t / dur); }

            static AudioClip Tone(string name, float dur, float freq, float slide, Wave w, float vol)
            {
                int n = Mathf.CeilToInt(dur * Rate);
                var data = new float[n];
                float phase = 0f;
                for (int i = 0; i < n; i++)
                {
                    float t = (float)i / Rate;
                    float f = freq * Mathf.Pow(slide, t / dur);
                    phase += f / Rate;
                    data[i] = Osc(w, phase) * Env(t, dur) * vol;
                }
                return Build(name, data);
            }

            static AudioClip Chord(string name, float[] notes, float dur, Wave w, float stagger, float vol)
            {
                float total = dur + stagger * notes.Length;
                int n = Mathf.CeilToInt(total * Rate);
                var data = new float[n];
                for (int k = 0; k < notes.Length; k++)
                {
                    int start = Mathf.FloorToInt(stagger * k * Rate);
                    float phase = 0f;
                    for (int i = start; i < n; i++)
                    {
                        float t = (float)(i - start) / Rate;
                        if (t > dur) break;
                        phase += notes[k] / Rate;
                        data[i] += Osc(w, phase) * Env(t, dur) * vol / Mathf.Sqrt(notes.Length);
                    }
                }
                return Build(name, data);
            }

            static AudioClip Noise(string name, float dur, float vol)
            {
                int n = Mathf.CeilToInt(dur * Rate);
                var data = new float[n];
                float lp = 0f;
                var rng = new System.Random(name.GetHashCode());
                for (int i = 0; i < n; i++)
                {
                    float t = (float)i / Rate;
                    float x = (float)(rng.NextDouble() * 2.0 - 1.0);
                    lp += (x - lp) * 0.25f;
                    data[i] = lp * Env(t, dur) * vol;
                }
                return Build(name, data);
            }

            static AudioClip Build(string name, float[] data)
            {
                var clip = AudioClip.Create("sfx_" + name, data.Length, 1, Rate, false);
                clip.SetData(data, 0);
                return clip;
            }
        }
    }
}
