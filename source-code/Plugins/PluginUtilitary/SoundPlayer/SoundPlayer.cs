using NAudio.Wave;
using System;
using System.IO;

namespace SoundPlayerLib
{
    public class SoundPlayer : IDisposable {
        WaveStream audioFileReader;
        IWavePlayer waveOutDevice;
        string FilePath { get; }
        public string Id { get; }

        public event EventHandler PlaybackFinished = delegate { };

        public SoundPlayer(string id, string fileName) {           
            if (!File.Exists(fileName)){                
                throw new InvalidOperationException("File does not exist in the provided path");
            }

            FilePath = Path.ChangeExtension(fileName, ".mp3");
            audioFileReader = new Mp3FileReader(fileName);

            Id = id;
            Prepare();
        }

        public void Play() {
            waveOutDevice.Play();    
        }

        public void Stop() {
            if (waveOutDevice != null) {
                waveOutDevice.Stop();
                Dispose();
            }
        }

        public void Prepare() {
            audioFileReader = new Mp3FileReader(FilePath);
            waveOutDevice = new WaveOutEvent(); // or WaveOutEvent()
            waveOutDevice.PlaybackStopped += PlaybackStopped;
            waveOutDevice.Init(audioFileReader);
        }

        void PlaybackStopped(object sender, StoppedEventArgs e) {
            Dispose();
            PlaybackFinished(this, null);
        }

        public void Dispose() {

            if (waveOutDevice != null) {
                waveOutDevice.PlaybackStopped -= PlaybackStopped;
                waveOutDevice.Stop();
            }

            if (audioFileReader != null) {
                audioFileReader.Dispose();
                audioFileReader = null;
            }
            if (waveOutDevice != null) {
                waveOutDevice.Dispose();
                waveOutDevice = null;
            }
        }
    }
}
