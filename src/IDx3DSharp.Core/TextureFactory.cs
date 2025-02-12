// | -----------------------------------------------------------------
// | idx3d III is (c)1999/2000 by Peter Walser
// | -----------------------------------------------------------------
// | idx3d is a 3d engine written in 100% pure Java (1.1 compatible)
// | and provides a fast and flexible API for software 3d rendering
// | on the Java platform.
// |
// | Feel free to use the idx3d API / classes / source code for
// | non-commercial purposes (of course on your own risk).
// | If you intend to use idx3d for commercial purposes, please
// | contact me with an e-mail [proxima@active.ch].
// |
// | Thanx & greetinx go to:
// | * Wilfred L. Guerin, 	for testing, bug report, and tons 
// |			of brilliant suggestions
// | * Sandy McArthur,	for reverse loops
// | * Dr. Douglas Lyons,	for mentioning idx3d1 in his book
// | * Hugo Elias,		for maintaining his great page
// | * the comp.graphics.algorithms people, 
// | 			for scientific concerns
// | * Tobias Hill,		for inspiration and awakening my
// |			interest in java gfx coding
// | * Kai Krause,		for inspiration and hope
// | * Incarom & Parisienne,	for keeping me awake during the 
// |			long coding nights
// | * Doris Langhard,	for being the sweetest girl on earth
// | * Etnica, Infinity Project, X-Dream and "Space Night"@BR3
// | 			for great sound while coding
// | and all coderz & scenerz out there (keep up the good work, ppl :)
// |
// | Peter Walser
// | proxima@active.ch
// | http://www2.active.ch/~proxima
// | "On the eigth day, God started debugging"
// | -----------------------------------------------------------------

using System;

namespace IDx3DSharp
{
	public static class TextureFactory
	// generates Textures
	{
		public const float pi = 3.1415926535f;
		public const float deg2rad = pi / 180;
        static float[,] noiseBuffer;
        static bool noiseBufferInitialized;
		static int minx, maxx, miny, maxy;

		// E X A M P L E   M A T E R I A L S

		public static Texture SKY(int w, int h, float density)
		{
			var colors = new uint[2];
			colors[0] = 0x003399;
			colors[1] = 0xFFFFFF;
			return PERLIN(w, h, 0.5f, 2.8f * density, 8, 1024).colorize(
				ColorUtility.makeGradient(colors, 1024));
		}

		public static Texture MARBLE(int w, int h, float density)
		{
			var colors = new uint[3];
			colors[0] = 0x111111;
			colors[1] = 0x696070;
			colors[2] = 0xFFFFFF;
			return WAVE(w, h, 0.5f, 0.64f * density, 6, 1024).colorize(
				ColorUtility.makeGradient(colors, 1024));
		}

		public static Texture WOOD(int w, int h, float density)
		{
			var colors = new uint[3];
			colors[0] = 0x332211;
			colors[1] = 0x523121;
			colors[2] = 0x996633;

			return GRAIN(w, h, 0.5f, 3f * density, 3, 8, 1024).colorize(
				ColorUtility.makeGradient(colors, 1024));
		}

		public static Texture RANDOM(int w, int h)
		{
			var nc = (int) MathUtility.Random(2, 6);
			var colors = new uint[nc];
			for (var i = 0; i < nc; i++)
				colors[i] = ColorUtility.random();

			var persistency = MathUtility.Random(0.4f, 0.9f);
			var density = MathUtility.Random(0.5f, 3f);
			var samples = (int) MathUtility.Random(1, 7f);

			return PERLIN(w, h, persistency, density, samples, 1024).colorize(
				ColorUtility.makeGradient(colors, 1024));
		}

		public static Texture CHECKERBOARD(int w, int h, int cellbits, uint oddColor, uint evenColor)
		{
			var t = new Texture(w, h);

			var pos = 0;
			for (var y = 0; y < h; y++)
				for (var x = 0; x < w; x++)
					t.pixel[pos++] = (((x >> cellbits) + (y >> cellbits)) & 1) == 0 ? evenColor : oddColor;

			return t;
		}

		// B A S E  T Y P E S

		public static Texture PERLIN(int w, int h, float persistency, float density, int samples, int scale)
		{
			initNoiseBuffer();
			var t = new Texture(w, h);
			var pos = 0;
			var wavelength = ((w > h) ? w : h) / density;

			for (var y = 0; y < h; y++)
				for (var x = 0; x < w; x++)
					t.pixel[pos++] = (uint) (scale * perlin2d(x, y, wavelength, persistency, samples));
			return t;
		}

		public static Texture WAVE(int w, int h, float persistency, float density, int samples, int scale)
		{
			initNoiseBuffer();
			var t = new Texture(w, h);
			var pos = 0;
			var wavelength = ((w > h) ? w : h) / density;

			for (var y = 0; y < h; y++)
				for (var x = 0; x < w; x++)
					t.pixel[pos++] = (uint) (scale * (Math.Sin(32 * perlin2d(x, y, wavelength, persistency, samples)) * 0.5 + 0.5));
			return t;
		}

		public static Texture GRAIN(int w, int h, float persistency, float density, int samples, int levels, int scale)
		// TIP: For wooden textures
		{
			initNoiseBuffer();
			var t = new Texture(w, h);
			var pos = 0;
			var wavelength = ((w > h) ? w : h) / density;
			float perlin;

			for (var y = 0; y < h; y++)
				for (var x = 0; x < w; x++)
				{
					perlin = levels * perlin2d(x, y, wavelength, persistency, samples);
					t.pixel[pos++] = (uint) (scale * (perlin - (int) perlin));
				}
			return t;
		}

		// Perlin noise functions

        static float perlin2d(float x, float y, float wavelength, float persistence, int samples)
		{
			float sum = 0;
			var freq = 1f / wavelength;
			var amp = persistence;
			float range = 0;

			for (var i = 0; i < samples; i++)
			{
				sum += amp * interpolatedNoise(x * freq, y * freq, i);
				range += amp;
				amp *= persistence;
				freq *= 2;
			}
			return MathUtility.Crop(sum / persistence * 0.5f + 0.5f, 0, 1);
		}

		// Helper methods

        static float interpolatedNoise(float x, float y, int octave)
		{
			var intx = (int) x;
			var inty = (int) y;
			var fracx = x - intx;
			var fracy = y - inty;

			var i1 = MathUtility.Interpolate(noise(intx, inty, octave), noise(intx + 1, inty, octave), fracx);
			var i2 = MathUtility.Interpolate(noise(intx, inty + 1, octave), noise(intx + 1, inty + 1, octave), fracx);

			return MathUtility.Interpolate(i1, i2, fracy);
		}

        static float smoothNoise(int x, int y, int o)
		{
			return (noise(x - 1, y - 1, o) + noise(x + 1, y - 1, o) + noise(x - 1, y + 1, o) + noise(x + 1, y + 1, o)) / 16
				+ (noise(x - 1, y, o) + noise(x + 1, y, o) + noise(x, y - 1, o) + noise(x, y + 1, o)) / 8
				+ noise(x, y, o) / 4;
		}

        static float noise(int x, int y, int octave)
		{
			return noiseBuffer[octave & 3, (x + y * 57) & 8191];
		}

        static float noise(int seed, int octave)
		{
			var id = octave & 3;
			var n = (seed << 13) ^ seed;

			if (id == 0) return 1f - ((n * (n * n * 15731 + 789221) + 1376312589) & 0x7FFFFFFF) * 0.000000000931322574615478515625f;
			if (id == 1) return 1f - ((n * (n * n * 12497 + 604727) + 1345679039) & 0x7FFFFFFF) * 0.000000000931322574615478515625f;
			if (id == 2) return 1f - ((n * (n * n * 19087 + 659047) + 1345679627) & 0x7FFFFFFF) * 0.000000000931322574615478515625f;
			return 1f - ((n * (n * n * 16267 + 694541) + 1345679501) & 0x7FFFFFFF) * 0.000000000931322574615478515625f;
		}

        static void initNoiseBuffer()
		{
			if (noiseBufferInitialized) return;
			noiseBuffer = new float[4, 8192];
			for (var octave = 0; octave < 4; octave++)
				for (var i = 0; i < 8192; i++)
					noiseBuffer[octave, i] = noise(i, octave);
			noiseBufferInitialized = true;
		}
	}
}