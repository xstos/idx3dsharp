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
using System.IO;
using System.Net;

namespace IDx3DSharp
{

	public class Texture
	// defines a texture
	{
		// F I E L D S

		public int width;
		public int height;
		public int bitWidth;
		public int bitHeight;
		public uint[] pixel;

		public string path;

		// C O N S T R U C T O R S

		public Texture(int w, int h)
		{
			height = h;
			width = w;
			pixel = new uint[w * h];
			cls();
		}

		public Texture(int w, int h, uint[] data)
		{
			height = h;
			width = w;
			pixel = new uint[width * height];
			Array.Copy(data, pixel, pixel.Length);
		}

		public Texture(string path)
		{
			Bitmap bitmap;
			this.path = null;
			if (path.StartsWith("http"))
			{
				bitmap = (Bitmap) Image.FromStream(WebRequest.Create(path).GetResponse().GetResponseStream());
			}
			else
			{
				bitmap = new Bitmap(path, false);
			}
			loadTexture(bitmap);
		}

		// P U B L I C   M E T H O D S

		public void Resize()
		{
			var log2inv = 1 / Math.Log(2);
			var w = (int) Math.Pow(2, bitWidth = (int) (Math.Log(width) * log2inv));
			var h = (int) Math.Pow(2, bitHeight = (int) (Math.Log(height) * log2inv));
			resize(w, h);
		}

		public void resize(int w, int h)
		{
			setSize(w, h);
		}

		/// <summary>
		/// Assigns new data for the texture.
		/// </summary>
		/// <param name="newData"></param>
		/// <returns></returns>
		public Texture put(Texture newData)
		{
			Array.Copy(newData.pixel, 0, pixel, 0, width * height);
			return this;
		}

		public Texture mix(Texture newData)
		// mixes the texture with another one
		{
			for (var i = width * height - 1; i >= 0; i--)
				pixel[i] = ColorUtility.mix(pixel[i], newData.pixel[i]);
			return this;
		}

		public Texture add(Texture additive)
		// additive blends another texture with this
		{
			for (var i = width * height - 1; i >= 0; i--)
				pixel[i] = ColorUtility.add(pixel[i], additive.pixel[i]);
			return this;
		}

		public Texture sub(Texture subtractive)
		// subtractive blends another texture with this
		{
			for (var i = width * height - 1; i >= 0; i--)
				pixel[i] = ColorUtility.sub(pixel[i], subtractive.pixel[i]);
			return this;
		}

		public Texture inv()
		// inverts the texture
		{
			for (var i = width * height - 1; i >= 0; i--)
				pixel[i] = ColorUtility.inv(pixel[i]);
			return this;
		}

		public Texture multiply(Texture multiplicative)
		// inverts the texture
		{
			for (var i = width * height - 1; i >= 0; i--)
				pixel[i] = ColorUtility.multiply(pixel[i], multiplicative.pixel[i]);
			return this;
		}


		public void cls()
		// clears the texture
		{
			MathUtility.clearBuffer(pixel, 0);
		}

		public Texture toAverage()
		// builds the averidge of the channels
		{
			for (var i = width * height - 1; i >= 0; i--)
				pixel[i] = ColorUtility.getAverage(pixel[i]);
			return this;
		}

		public Texture toGray()
		// converts this texture to gray
		{
			for (var i = width * height - 1; i >= 0; i--)
				pixel[i] = ColorUtility.getGray(pixel[i]);
			return this;
		}

		public Texture valToGray()
		{
			uint intensity;
			for (var i = width * height - 1; i >= 0; i--)
			{
				intensity = MathUtility.Crop(pixel[i], 0, 255);
				pixel[i] = ColorUtility.getColor(intensity, intensity, intensity);
			}

			return this;
		}

		public Texture colorize(uint[] pal)
		{
			var range = (uint) pal.Length - 1;
			for (var i = width * height - 1; i >= 0; i--)
				pixel[i] = pal[MathUtility.Crop(pixel[i], 0, range)];
			return this;
		}

		public static Texture blendTopDown(Texture top, Texture down)
		{
			down.resize(top.width, top.height);
			var t = new Texture(top.width, top.height);
			var pos = 0;
			uint alpha;
			for (var y = 0; y < top.height; y++)
			{
				alpha = (uint) (255 * y / (top.height - 1));
				for (var x = 0; x < top.width; x++)
				{
					t.pixel[pos] = ColorUtility.transparency(down.pixel[pos], top.pixel[pos], alpha);
					pos++;
				}
			}
			return t;
		}

		// P R I V A T E   M E T H O D S


		/// <summary>
		/// Grabs the pixels out of an image.
		/// </summary>
		/// <param name="map"></param>
        unsafe void loadTexture(Bitmap map)
        {
            width = map.Width;
            height = map.Height;
            pixel = new uint[width * height];
            var bitmapdata = map.LockBits(new Rectangle(0, 0, map.Width, map.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            var stride = bitmapdata.Stride;
            var ptr = bitmapdata.Scan0;
            var numPtr = (byte*) ptr;
            var num2 = bitmapdata.Stride - (map.Width * 3);
            var num3 = 0;
            for (var i = 0; i < map.Height; i++)
            {
                for (var j = 0; j < map.Width; j++)
                {
                    int num6 = numPtr[0];
                    int num7 = numPtr[1];
                    int num8 = numPtr[2];
						        pixel[num3++] = (uint) (((ColorUtility.ALPHA | (num8 << 0x10)) | (num7 << 8)) | num6);
                    numPtr += 3;
                }
                numPtr += num2;
            }
            map.UnlockBits(bitmapdata);
            Resize();
        }


        void setSize(int w, int h)
		// resizes the texture
		{
			var offset = w * h;
			int offset2;
			if (w * h != 0)
			{
				var newpixels = new uint[w * h];
				for (var j = h - 1; j >= 0; j--)
				{
					offset -= w;
					offset2 = (j * height / h) * width;
					for (var i = w - 1; i >= 0; i--)
						newpixels[i + offset] = pixel[(i * width / w) + offset2];
				}
				width = w; height = h; pixel = newpixels;
			}
		}

        bool InRange(int a, int b, int c)
		{
			return (a >= b) & (a < c);
		}

		public Texture Clone()
		{
			var t = new Texture(width, height);
			MathUtility.copyBuffer(pixel, t.pixel);
			return t;
		}
	}
}