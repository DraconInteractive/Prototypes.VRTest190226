using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Audio
{
    /// <summary>
    /// Global controller for music, VO, etc
    /// </summary>
    public class AudioController : MonoBehaviour
    {
        private static AudioController _instance;

        public static AudioController Instance
        {
            get
            {
                if (_instance == null)
                {
                    // Other controllers will assign on their awake
                    var global = Instantiate(Resources.Load<GameObject>("Global Controllers"));
                    _instance = global.GetComponentInChildren<AudioController>();
                }
                return _instance;
            }
        }
        
        public enum VOSpeaker
        {
            Player,
            Narrator,
            Enemy,
            Friend
        }
        
        [Serializable]
        public class VOState
        {
            public string ID;
            public VOSpeaker speaker;
            public AudioClip clip;
            public AudioSource source;
            
            public Action<string> OnStart;
            public Action<string> OnPause;
            public Action<string> OnResume;
            public Action<string> OnCancel;
            public Action<string> OnComplete;

            public bool IsPaused;
            public bool IsCancelled;
            
            public void Start()
            {
                source.clip = clip;
                source.Play();
                OnStart?.Invoke(ID);
            }

            public void Pause()
            {
                IsPaused = true;
                source.Pause();
                OnPause?.Invoke(ID);
            }

            public void Resume()
            {
                IsPaused = false;
                source.UnPause();
                OnResume?.Invoke(ID);
            }

            public void Cancel()
            {
                IsCancelled = true;
                source.Stop();
                OnCancel?.Invoke(ID);
            }

            public void Complete()
            {
                source.Stop();
                OnComplete?.Invoke(ID);
            }

            public void Mute()
            {
                source.mute = true;
            }

            public void UnMute()
            {
                source.mute = false;
            }
        }

        [Serializable]
        public class DefaultSource
        {
            public VOSpeaker speaker;
            public AudioSource source;
        }

        public List<DefaultSource> defaultSources = new();
        public List<AudioClip> clipRegistry = new();

        public AudioClip GetClip(string clipName)
        {
            var clip = clipRegistry.FirstOrDefault(c => c != null && c.name == clipName);
            if (clip == null)
                Debug.LogError($"AudioController: no clip registered with name '{clipName}'.");
            return clip;
        }

        private Dictionary<string, List<VOState>> activeSpeakers = new();
        private List<string> speakerDeletionSet = new();
        
        private void Awake()
        {
            _instance = this;
        }

        private void Update()
        {
            if (activeSpeakers.Count == 0) return;

            var processDeletion = false;
            speakerDeletionSet.Clear();
            
            // Check if any are done, and 
            foreach (var kvp in activeSpeakers)
            {
                var id = kvp.Key;
                var state = activeSpeakers[id][0];

                if (state.IsPaused) continue;

                var complete = !state.source.isPlaying && !state.IsPaused;

                if (complete || state.IsCancelled)
                {
                    activeSpeakers[id].RemoveAt(0);
                    if (activeSpeakers[id].Count > 0)
                    {
                        activeSpeakers[id][0].Start();
                    }
                    else
                    {
                        speakerDeletionSet.Add(id);
                        processDeletion = true;
                    }
                }
            }

            if (processDeletion)
            {
                foreach (var id in speakerDeletionSet)
                {
                    activeSpeakers.Remove(id);
                }
            }
        }

        public void GlobalPause()
        {
            if (activeSpeakers.Count == 0) return;
            
            foreach (var kvp in activeSpeakers)
            {
                activeSpeakers[kvp.Key][0].Pause();
            }
        }

        public void GlobalResume()
        {
            if (activeSpeakers.Count == 0) return;
            
            foreach (var kvp in activeSpeakers)
            {
                activeSpeakers[kvp.Key][0].Resume();
            }
        }

        public void Pause(string id)
        {
            if (activeSpeakers.ContainsKey(id))
            {
                activeSpeakers[id][0].Pause();
            }
            else
            {
                Debug.LogError($"Cannot pause speaker with ID {id}: doesn't exist");
            }
        }

        public void Resume(string id)
        {
            if (activeSpeakers.ContainsKey(id))
            {
                activeSpeakers[id][0].Resume();
            }
            else
            {
                Debug.LogError($"Cannot resume speaker with ID {id}: doesn't exist");
            }
        }

        public void GlobalMute()
        {
            if (activeSpeakers.Count == 0) return;
            
            foreach (var kvp in activeSpeakers)
            {
                activeSpeakers[kvp.Key][0].Mute();
            }
        }

        public void GlobalUnMute()
        {
            if (activeSpeakers.Count == 0) return;
            
            foreach (var kvp in activeSpeakers)
            {
                activeSpeakers[kvp.Key][0].UnMute();
            }
        }

        public void Mute(string id)
        {
            if (activeSpeakers.ContainsKey(id))
            {
                activeSpeakers[id][0].Mute();
            }
            else
            {
                Debug.LogError($"Cannot mute speaker with ID {id}: doesn't exist");
            }
        }

        public void UnMute(string id)
        {
            if (activeSpeakers.ContainsKey(id))
            {
                activeSpeakers[id][0].UnMute();
            }
            else
            {
                Debug.LogError($"Cannot unmute speaker with ID {id}: doesn't exist");
            }
        }
        
        public VOState AddVoiceOver(VOSpeaker speaker, AudioClip clip, string id = "", AudioSource source = null, bool clearQueue = false)
        {
            var stateId = string.IsNullOrEmpty(id) ? speaker.ToString() : id;

            var stateSource = source;
            if (stateSource == null)
            {
                var defaultSourceData = defaultSources.FirstOrDefault(x => x.speaker == speaker);
                if (defaultSourceData != null && defaultSourceData.source != null)
                {
                    stateSource = defaultSourceData.source;
                }
            }

            if (stateSource == null)
            {
                Debug.LogError($"No default VO source for speaker {speaker.ToString()}");
                return null;
            }
            
            var state = new VOState()
            {
                clip = clip,
                ID = stateId,
                source = stateSource,
                speaker = speaker
            };

            if (activeSpeakers.ContainsKey(stateId))
            {
                if (clearQueue)
                {
                    foreach (var queueItem in activeSpeakers[stateId])
                    {
                        queueItem.OnCancel?.Invoke(stateId);
                    }
                    activeSpeakers[stateId].Clear();
                    activeSpeakers[stateId].Add(state);
                    state.Start();
                }
                else
                {
                    activeSpeakers[stateId].Add(state);
                }
            }
            else
            {
                activeSpeakers[stateId] = new List<VOState>() { state };
                state.Start();
            }
            
            return state;
        }
    }
}