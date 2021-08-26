using System.Runtime.InteropServices;
using ManagedBass;
using ManagedBass.Fx;
using ManagedManagedBass.Exceptions;

namespace ManagedManagedBass.Helpers {
	public class AudioStream {
		private int      rawAudioHandle;
		private int      fxAudioHandle;
		private int      reverseFxAudioHandle;
		private GCHandle memoryHandle;

		public AudioStream() {}

		public AudioStream(byte[] audioData, BassFlags extraFlags = BassFlags.Default) {
			this.Load(audioData, extraFlags);
		}

		public bool IsValidHandle {
			get {
				try {
					Bass.ChannelGetInfo(this.fxAudioHandle);
				}
				catch {
					// ignored
				}

				return Bass.LastError != Errors.Handle;
			}
		}

		public PlaybackState PlaybackState => Bass.ChannelIsActive(this.fxAudioHandle);

		public float Volume {
			get {
				bool success = Bass.ChannelGetAttribute(this.fxAudioHandle, ChannelAttribute.Volume, out float volume);

				if (success)
					return volume;

				throw Bass.LastError switch {
					Errors.Handle => new BassHandleException(),
					Errors.Type   => new BassAttributeNotValidException(),
					_             => new BassUnknownException()
				};
			}
			set {
				bool success = Bass.ChannelSetAttribute(this.fxAudioHandle, ChannelAttribute.Volume, value);

				if (success) return;

				throw Bass.LastError switch {
					Errors.Handle    => new BassHandleException(),
					Errors.Type      => new BassAttributeNotValidException(),
					Errors.Parameter => new BassParameterException(),
					_                => new BassUnknownException()
				};
			}
		}

		public double CurrentTime {
			get {
				long audioTimeRaw = Bass.ChannelGetPosition(this.fxAudioHandle);

				if (audioTimeRaw != -1)
					return Bass.ChannelBytes2Seconds(this.fxAudioHandle, audioTimeRaw);

				throw Bass.LastError switch {
					Errors.Handle       => new BassHandleException(),
					Errors.NotAvailable => new BassNotAvailableException(),
					_                   => new BassUnknownException()
				};
			}
		}

		public bool Loop {
			get {
				BassFlags flags = Bass.ChannelFlags(this.fxAudioHandle, 0, 0);

				if ((int)flags == -1 && Bass.LastError == Errors.Handle) throw new BassHandleException();

				return (flags & BassFlags.Loop) > 0;
			}
			set => this.SetFlag(value, BassFlags.Loop);
		}

		// public float Pitch {
		// 	get {
		// 		bool success = Bass.ChannelGetAttribute(this.audioHandle, ChannelAttribute.Pitch, out float pitch);
		//
		// 		if (success) return pitch;
		// 		
		// 		throw Bass.LastError switch {
		// 			Errors.Handle => new BassHandleException(),
		// 			Errors.Type   => new BassAttributeNotValidException(),
		// 			_             => new BassUnknownException()
		// 		};
		// 	}
		// 	set {
		// 		bool success = Bass.ChannelSetAttribute(this.audioHandle, ChannelAttribute.Pitch, value);
		//
		// 		if (success) return;
		// 		
		// 		throw Bass.LastError switch {
		// 			Errors.Handle    => new BassHandleException(),
		// 			Errors.Type      => new BassAttributeNotValidException(),
		// 			Errors.Parameter => new BassParameterException(),
		// 			_                => new BassUnknownException()
		// 		};
		// 	}
		// }
		public float Frequency {
			get {
				bool success = Bass.ChannelGetAttribute(this.fxAudioHandle, ChannelAttribute.Frequency, out float frequency);

				if (success) return frequency;

				throw Bass.LastError switch {
					Errors.Handle => new BassHandleException(),
					Errors.Type   => new BassAttributeNotValidException(),
					_             => new BassUnknownException()
				};
			}
			set {
				bool success = Bass.ChannelSetAttribute(this.fxAudioHandle, ChannelAttribute.Frequency, value);

				if (success) return;

				throw Bass.LastError switch {
					Errors.Handle    => new BassHandleException(),
					Errors.Type      => new BassAttributeNotValidException(),
					Errors.Parameter => new BassParameterException(),
					_                => new BassUnknownException()
				};
			}
		}

		public void SeekTo(int milliseconds) {
			bool success = Bass.ChannelSetPosition(this.fxAudioHandle, milliseconds / 1000);

			if (success) return;

			throw Bass.LastError switch {
				Errors.Handle       => new BassHandleException(),
				Errors.NotFile      => new BassNotFileException(),
				Errors.Position     => new BassPositionException(),
				Errors.NotAvailable => new BassNotAvailableException(),
				_                   => new BassUnknownException()
			};
		}

		public void Pause() {
			bool success = Bass.ChannelPause(this.fxAudioHandle);

			if (success) return;

			throw Bass.LastError switch {
				Errors.Handle     => new BassHandleException(),
				Errors.NotPlaying => new BassNotPlayingException(),
				Errors.Decode     => new BassDecodeException(),
				Errors.Already    => new BassAlreadyPausedException(),
				_                 => new BassUnknownException()
			};
		}

		public void Play() {
			bool success = Bass.ChannelPlay(this.fxAudioHandle);

			if (success) return;

			throw Bass.LastError switch {
				Errors.Handle => new BassHandleException(),
				Errors.Decode => new BassDecodeException(),
				_             => new BassUnknownException()
			};
		}

		public void Stop(bool reset = true) {
			bool success = Bass.ChannelStop(this.fxAudioHandle);

			if (!success)
				throw Bass.LastError switch {
					Errors.Handle => new BassHandleException(),
					_             => new BassUnknownException()
				};

			if (reset) this.SeekTo(0);
		}

		private void SetFlag(bool status, BassFlags flag) {
			BassFlags flags = Bass.ChannelFlags(this.fxAudioHandle, status ? flag : BassFlags.Default, flag);

			if ((int)flags == -1 && Bass.LastError == Errors.Handle) throw new BassHandleException();

			bool isSet = (flags & flag) == flag;

			switch (isSet) {
				case true when status:
					break;
				case false when !status:
					break;
				default:
					throw new BassFlagSetException();
			}
		}

		public void Load(byte[] audioData, BassFlags extraFlags = BassFlags.Default) {
			this.memoryHandle = GCHandle.Alloc(audioData, GCHandleType.Pinned);

			int tempAudioHandle = Bass.CreateStream(
				this.memoryHandle.AddrOfPinnedObject(),
				0,
				audioData.Length,
				BassFlags.Prescan | BassFlags.Decode | extraFlags
			);

			if (tempAudioHandle == 0)
				throw Bass.LastError switch {
					Errors.Init         => new BassInitException(),
					Errors.NotAvailable => new BassNotAvailableException(),
					Errors.Parameter    => new BassParameterException(),
					Errors.FileOpen     => new BassFileOpenException(),
					Errors.FileFormat   => new BassFileFormatException(),
					Errors.Codec        => new BassCodecException(),
					Errors.SampleFormat => new BassSampleFormatException(),
					Errors.Speaker      => new BassSpeakerException(),
					Errors.Memory       => new BassMemoryException(),
					Errors.No3D         => new BassNo3DException(),
					_                   => new BassUnknownException()
				};

			this.rawAudioHandle = tempAudioHandle;

			tempAudioHandle = BassFx.TempoCreate(tempAudioHandle, BassFlags.Prescan | extraFlags);

			if (tempAudioHandle == 0)
				throw Bass.LastError switch {
					Errors.Handle       => new BassHandleException(),
					Errors.Decode       => new BassDecodeException(),
					Errors.SampleFormat => new BassSampleFormatException(),
					_                   => new BassUnknownException()
				};

			this.fxAudioHandle = tempAudioHandle;
			
			tempAudioHandle = BassFx.ReverseCreate(tempAudioHandle, 5f, BassFlags.Prescan | extraFlags);

			if (tempAudioHandle == 0)
				throw Bass.LastError switch {
					Errors.Handle       => new BassHandleException(),
					Errors.Decode       => new BassDecodeException(),
					Errors.SampleFormat => new BassSampleFormatException(),
					_                   => new BassUnknownException()
				};

			this.reverseFxAudioHandle = tempAudioHandle;
		}
	}
}
