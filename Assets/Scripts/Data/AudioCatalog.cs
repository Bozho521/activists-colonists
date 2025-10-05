using System.Collections.Generic;
using UnityEngine;

namespace Data
{
    [CreateAssetMenu(menuName = "Audio/Audio Catalog", fileName = "AudioCatalog")]
    public class AudioCatalog : ScriptableObject
    {
        [System.Serializable]
        public class ClipEntry
        {
            public string key;
            public AudioClip clip;
        }

        [System.Serializable]
        public class GroupEntry
        {
            public string key;
            public AudioClip[] clips;
        }

        public AudioClip defaultMusic;
        public ClipEntry[] clips;
        public GroupEntry[] groups;

        Dictionary<string, AudioClip> _clipMap;
        Dictionary<string, AudioClip[]> _groupMap;

        public void Build()
        {
            _clipMap = new Dictionary<string, AudioClip>(clips != null ? clips.Length : 0);
            _groupMap = new Dictionary<string, AudioClip[]>(groups != null ? groups.Length : 0);

            if (clips != null)
            {
                foreach (var e in clips)
                {
                    if (e == null || string.IsNullOrWhiteSpace(e.key) || !e.clip) continue;
                    _clipMap[e.key] = e.clip;
                }
            }

            if (groups != null)
            {
                foreach (var g in groups)
                {
                    if (g == null || string.IsNullOrWhiteSpace(g.key) || g.clips == null || g.clips.Length == 0) continue;
                    _groupMap[g.key] = g.clips;
                }
            }
        }

        public IEnumerable<KeyValuePair<string, AudioClip>> GetClipPairs()
        {
            if (_clipMap == null) Build();
            return _clipMap;
        }

        public IEnumerable<KeyValuePair<string, AudioClip[]>> GetGroupPairs()
        {
            if (_groupMap == null) Build();
            return _groupMap;
        }

        public bool TryGetClip(string key, out AudioClip clip)
        {
            if (_clipMap == null) Build();
            return _clipMap.TryGetValue(key, out clip);
        }

        public bool TryGetGroup(string key, out AudioClip[] arr)
        {
            if (_groupMap == null) Build();
            return _groupMap.TryGetValue(key, out arr);
        }
    }
}