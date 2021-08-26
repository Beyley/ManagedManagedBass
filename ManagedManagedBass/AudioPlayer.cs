using System;
using System.Runtime.InteropServices;
using ManagedBass;
using ManagedManagedBass.Helpers;
using ManagedManagedBass.Platform.Linux;

namespace ManagedManagedBass {
	public class AudioPlayer {
		private AudioStream stream;

		/// <summary>
		///     Creates an AudioPlayer with the specified Window ID
		/// </summary>
		/// <param name="windowId">The Window ID to initialize Bass with, default (or 0) means use the desktop window</param>
		public AudioPlayer(IntPtr windowId = default) {
			if (windowId == default) windowId = IntPtr.Zero;

			if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
				Console.WriteLine("Load Linux ManagedBass Libraries");

				Library.Load("/usr/lib/libbass.so", Library.LoadFlags.RTLD_LAZY    | Library.LoadFlags.RTLD_GLOBAL);
				Library.Load("/usr/lib/libbass_fx.so", Library.LoadFlags.RTLD_LAZY | Library.LoadFlags.RTLD_GLOBAL);
			}

			Bass.Init(-1, 44100, DeviceInitFlags.Default, windowId);
			Bass.PluginLoad("/usr/lib/libbass_fx.so");
		}

		public double CurrentTime => this.stream.CurrentTime;
		public bool Loop {
			get => this.stream.Loop;
			set => this.stream.Loop = value;
		}
		// public float Pitch {
		// 	get => this.stream.Pitch;
		// 	set => this.stream.Pitch = value;
		// }
		public float Frequency {
			get => this.stream.Frequency;
			set => this.stream.Frequency = value;
		}
		public float Volume {
			get => this.stream.Volume;
			set => this.stream.Volume = value;
		}

		public void LoadData(byte[] audioData, BassFlags extraFlags = BassFlags.Default) {
			if (this.stream == null || !this.stream.IsValidHandle)
				this.stream = new AudioStream();

			this.stream.Load(audioData, extraFlags);
		}

		public void Play() {
			this.stream.Play();
		}
		public void Pause() {
			this.stream.Pause();
		}
		public void Stop() {
			this.stream.Stop();
		}
		public void SeekTo(int milis) {
			this.stream.SeekTo(milis);
		}
	}
}
