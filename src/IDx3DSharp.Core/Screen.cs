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
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace IDx3DSharp
{
	/// <summary>
	/// Defines a virtual screen which is a server for rendered images.
	/// </summary>
	public class Screen
	{
		// F I E L D S

		public uint[] pixel;
		public int width;
		public int height;

		public uint[] p; // pixel array before antialiasing
		public int w, h; // dimensions before antialiasing

        Image image;
		public bool antialias;

        GCHandle handle;

		// BENCHMARK STUFF
        long timestamp;
        long time;
        int probes = 32;
		public float FPS;


		// C O N S T R U C T O R S

		public Screen(int w, int h)
		{
			width = w;
			height = h;
			this.w = width;
			this.h = height;
			pixel = new uint[w * h];
			p = pixel;
			handle = GCHandle.Alloc(pixel, GCHandleType.Pinned);
			var ptr = Marshal.UnsafeAddrOfPinnedArrayElement(pixel, 0);
			image = new Bitmap(w, h, w * 4, PixelFormat.Format32bppPArgb, ptr);
		}


		// P U B L I C   M E T H O D S

		public void render()
		{
			if (antialias) performAntialiasing();
		}

		public Image getImage()
		{
			performBench();
			return image;
		}

		public void setAntialias(bool active)
		{
			if (antialias == active) return;
			antialias = active;
			if (antialias)
			{
				w = width * 2;
				h = height * 2;
				p = new uint[w * h];

			}
			else
			{
				w = width;
				h = height;
				p = pixel;
			}
		}

		public Texture asTexture()
		{
			return new Texture(width, height, pixel);
		}

		public void clear(uint bgcolor)
		{
			MathUtility.clearBuffer(p, bgcolor);
		}

		public void resize(int width, int height)
		{
			this.width = width;
			this.height = height;
			if (antialias)
			{
				w = width * 2;
				h = height * 2;
				pixel = new uint[width * height];
				p = new uint[w * h];

			}
			else
			{
				w = width;
				h = height;
				pixel = new uint[width * height];
				p = pixel;
			}
			/*producer = new idx3d_ImageProducer(width, height, cm, pixel);
			image = Toolkit.getDefaultToolkit().createImage(producer);*/
		}

		public bool Antialias()
		{
			return antialias;
		}


		// P R I V A T E   M E T H O D S
        float frac = 1f / 1000f;
        void performBench()
		{
			probes += 1;
            if (probes <= 32) return;
            time = Environment.TickCount;
            FPS = 32f / ((float) (time - timestamp) * frac);
            timestamp = time;
            probes = 0;
        }

        void performAntialiasing()
		{
			int offset;
			var pos = 0;
			for (var y = 0; y < (h >> 1); y++)
			{
				offset = (y << 1) * w;
				for (var x = 0; x < (w >> 1); x++)
				{
					pixel[pos] = ((p[offset] & 0xFCFCFC) >> 2) +
						((p[offset + 1] & 0xFCFCFC) >> 2) +
						((p[offset + w] & 0xFCFCFC) >> 2) +
						((p[offset + w + 1] & 0xFCFCFC) >> 2);
					pos += 1;
					offset += 2;
				}
			}
		}

		// IMAGE OVERLAYING

		public void draw(Texture texture, int posx, int posy, int xsize, int ysize)
		{
			draw(pixel, width, height, texture, posx, posy, xsize, ysize);
		}

		public void add(Texture texture, int posx, int posy, int xsize, int ysize)
		{
			add(pixel, width, height, texture, posx, posy, xsize, ysize);
		}

		public void drawBackground(Texture texture, int posx, int posy, int xsize, int ysize)
		{
			draw(p, w, h, texture, posx, posy, xsize, ysize);
		}

		// Private part of image overlaying

        void draw(uint[] buffer, int width, int height, Texture texture, int posx, int posy, int xsize, int ysize)
		{
			if (texture == null) return;
			var w = xsize;
			var h = ysize;
			var xBase = posx;
			var yBase = posy;
			var tx = texture.width * 255;
			var ty = texture.height * 255;
			var tw = texture.width;
			var dtx = tx / w;
			var dty = ty / h;
			var txBase = MathUtility.Crop(-xBase * dtx, 0, 255 * tx);
			var tyBase = MathUtility.Crop(-yBase * dty, 0, 255 * ty);
			var xend = MathUtility.Crop(xBase + w, 0, width);
			var yend = MathUtility.Crop(yBase + h, 0, height);
			int pos, offset1, offset2;
			xBase = MathUtility.Crop(xBase, 0, width);
			yBase = MathUtility.Crop(yBase, 0, height);

			ty = tyBase;
			for (var j = yBase; j < yend; j++)
			{
				tx = txBase;
				offset1 = j * width;
				offset2 = (ty >> 8) * tw;
				for (var i = xBase; i < xend; i++)
				{
					buffer[i + offset1] = texture.pixel[(tx >> 8) + offset2];
					tx += dtx;
				}
				ty += dty;
			}
		}


        void add(uint[] buffer, int width, int height, Texture texture, int posx, int posy, int xsize, int ysize)
		{
			var w = xsize;
			var h = ysize;
			var xBase = posx;
			var yBase = posy;
			var tx = texture.width * 255;
			var ty = texture.height * 255;
			var tw = texture.width;
			var dtx = tx / w;
			var dty = ty / h;
			var txBase = MathUtility.Crop(-xBase * dtx, 0, 255 * tx);
			var tyBase = MathUtility.Crop(-yBase * dty, 0, 255 * ty);
			var xend = MathUtility.Crop(xBase + w, 0, width);
			var yend = MathUtility.Crop(yBase + h, 0, height);
			int pos, offset1, offset2;
			xBase = MathUtility.Crop(xBase, 0, width);
			yBase = MathUtility.Crop(yBase, 0, height);

			ty = tyBase;
			for (var j = yBase; j < yend; j++)
			{
				tx = txBase;
				offset1 = j * width;
				offset2 = (ty >> 8) * tw;
				for (var i = xBase; i < xend; i++)
				{
					buffer[i + offset1] = ColorUtility.add(texture.pixel[(tx >> 8) + offset2], pixel[i + offset1]);
					tx += dtx;
				}
				ty += dty;
			}
		}
	}
}