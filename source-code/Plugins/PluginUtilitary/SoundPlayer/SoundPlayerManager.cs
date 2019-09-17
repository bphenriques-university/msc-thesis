using System;
using System.Collections.Generic;

namespace SoundPlayerLib {
    public class SoundPlayerManager {
        private Dictionary<string, SoundPlayer> ActiveSounds { get; } = new Dictionary<string, SoundPlayer>();

        public event EventHandler SoundFinished = delegate { };

        internal void RemoveActiveSound(object sender, EventArgs e) {
            SoundPlayer p = sender as SoundPlayer;
            ActiveSounds.Remove(p.Id);
        }

        public void PlaySound(string fileName, string id) {
            SoundPlayer sp = new SoundPlayer(id, fileName);
            sp.PlaybackFinished += SoundFinished;
            sp.PlaybackFinished += RemoveActiveSound;
            ActiveSounds.Add(id, sp);

            if (ActiveSounds.ContainsKey(id)) {
                ActiveSounds[id].Play();
            }
        }

        public void StopSound(string id) {
            if (ActiveSounds.ContainsKey(id)) {
                ActiveSounds[id].Stop();
            }
        }
    }
}
