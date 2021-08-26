using System;
using System.IO;
using ManagedManagedBass;

namespace ExampleApp {
	internal class Program {
		private static void Main(string[] args) {
			byte[] bytes = File.ReadAllBytes("ringtone.mp3");

			var player = new AudioPlayer();

			player.LoadData(bytes);
			player.Loop   = true;
			player.Volume = 0.1f;

			player.Play();
			Console.ReadLine();
			player.Pause();
			Console.ReadLine();
			player.Play();
			Console.ReadLine();
			player.Frequency += 10000;
			Console.ReadLine();
			player.Frequency += -50000;
			Console.ReadLine();
		}
	}
}
