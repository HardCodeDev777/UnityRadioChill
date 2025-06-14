using NAudio.Wave;
using System;
using System.Threading;
using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
namespace HardCodeDev.UnityRadioChill
{
    public class UnityRadioChill : EditorWindow
    {
        private static IWavePlayer _waveOut;
        private static MediaFoundationReader _reader;
        private static Thread _streamingThread;
        private static BufferedWaveProvider _waveProvider;

        private static bool _playing = true;
        private static string _streamUrl;

        [MenuItem("HardCodeDev/UnityRadioChill")]
        public static void ShowWindow() 
        { 
            GetWindow<UnityRadioChill>("UnityRadioChill");
            StopRadio();
        }

        private void OnGUI()
        {
            _streamUrl = EditorGUILayout.TextField("Radio stream URL", _streamUrl);
            if (GUILayout.Button("Start")) StartRadio();
            if (GUILayout.Button("Stop")) StopRadio();
        }

        private static void StartRadio()
        {
            _streamingThread = null;
            _waveProvider = null;
            _waveOut = null;
            _reader = null;
            _playing = true;

            _streamingThread = new Thread(StreamRadio);
            _streamingThread.IsBackground = true;
            _streamingThread.Start();
        }

        private static void StopRadio()
        {
            _playing = false;
            _reader?.Dispose();
            _waveOut?.Stop();
            _waveOut?.Dispose();
        }

        private static void StreamRadio()
        {
            _reader = new MediaFoundationReader(_streamUrl);
            _waveProvider = new BufferedWaveProvider(_reader.WaveFormat)
            {
                BufferDuration = TimeSpan.FromSeconds(20),
                DiscardOnBufferOverflow = true
            };

            _waveOut = new WaveOutEvent();
            _waveOut.Init(_waveProvider);
            _waveOut.Play();

            var buffer = new byte[65536];
            int bytesRead;
            while (_playing && (bytesRead = _reader.Read(buffer, 0, buffer.Length)) > 0) _waveProvider.AddSamples(buffer, 0, bytesRead);
        }
    }

}
#endif
