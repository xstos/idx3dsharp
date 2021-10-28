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
	/// <summary>
	/// Rasterizer stage of the render pipeline.
	/// </summary>
	public sealed class Rasterizer
	{
        bool materialLoaded;
        bool lightmapLoaded;
		public bool ready;

		// Current material settings
        uint color;
        uint currentColor;
        uint transparency;
        uint reflectivity;
        int refraction = 0;
        Texture texture;
        uint[] envmap;
        uint[] diffuse;
        uint[] specular;
        short[] refractionMap = null;
        int tw;
        int th;
        int tbitW;
        int tbitH;

		// Rasterizer hints

        int mode;
        const int Flat = 0;   	// FLAT
        const int Wireframe = 1;	// WIREFRAME
        const int Phong = 2;  	// PHONG
        const int EnvMap = 4;  	// ENVMAP
        const int Textured = 8; 	// TEXTURED
        const int SHADED = Phong | EnvMap | Textured;

		//  R E G I S T E R S

		Vertex p1, p2, p3, tempVertex;

        uint
			bkgrd, c, s;

        int
			lutID, r,   //lutID is position in LUT (diffuse,envmap,specular)

			x1, x2, x3, x4, y1, y2, y3, z1, z2, z3, z4,
			x, y, z, k, dx, dy, dz, offset, pos, temp,
			xL, xR, xBase, xMax, yMax, dxL, dxR, zBase, dzBase,

			nx1, nx2, nx3, nx4, ny1, ny2, ny3, ny4,
			nxBase, nyBase,
			dnx4, dny4,
			dnx, dny, nx, ny,
			dnxBase, dnyBase,

			tx1, tx2, tx3, tx4, ty1, ty2, ty3, ty4,
			txBase, tyBase,
			dtx4, dty4,
			dtx, dty, tx, ty,
			dtxBase, dtyBase;

		Screen screen;
		uint[] zBuffer;
		uint[] idBuffer;
		int width, height;
		bool useIdBuffer;
		const int zFar = 0xFFFFFFF;
		uint currentId;


		// Constructor

		public Rasterizer(RenderPipeline pipeline)
		{
			rebuildReferences(pipeline);
			loadLightmap(pipeline.lightmap);
		}

		// References

		public void rebuildReferences(RenderPipeline pipeline)
		{
			screen = pipeline.screen;
			zBuffer = pipeline.zBuffer;
			idBuffer = pipeline.idBuffer;
			width = screen.w;
			height = screen.h;
			useIdBuffer = pipeline.useIdBuffer;
		}

		// Lightmap loader

		public void loadLightmap(Lightmap lm)
		{
			if (lm == null) return;
			diffuse = lm.diffuse;
			specular = lm.specular;
			lightmapLoaded = true;
			ready = lightmapLoaded && materialLoaded;
		}

		// Material loader

		public void loadMaterial(Material material)
		{
			color = material.color;
			transparency = material.transparency;
			reflectivity = material.reflectivity;
			texture = material.texture;
			envmap = material.envmap?.pixel;

            if (texture != null)
			{
				tw = texture.width - 1;
				th = texture.height - 1;
				tbitW = texture.bitWidth;
				tbitH = texture.bitHeight;
			}

			mode = 0;
			if (!material.flat) mode |= Phong;
			if (envmap != null) mode |= EnvMap;
			if (texture != null) mode |= Textured;
			if (material.wireframe) mode |= Wireframe;
			materialLoaded = true;
			ready = lightmapLoaded && materialLoaded;
		}

		public void Render(Triangle tri)
		{
			if (!ready) return;
			if (tri.getParent() == null) return;
			if ((mode & Wireframe) != 0)
			{
				drawWireframe(tri, color);
				if ((mode & Wireframe) == 0) return;

			}

			p1 = tri.p1;
			p2 = tri.p2;
			p3 = tri.p3;

			if (p1.Y > p2.Y) { tempVertex = p1; p1 = p2; p2 = tempVertex; }
			if (p2.Y > p3.Y) { tempVertex = p2; p2 = p3; p3 = tempVertex; }
			if (p1.Y > p2.Y) { tempVertex = p1; p1 = p2; p2 = tempVertex; }

			if (p1.Y >= height) return;
			if (p3.Y < 0) return;
			if (p1.Y == p3.Y) return;

			if (mode == Flat)
			{
				lutID = (int) (tri.n2.X * 127 + 127) + ((int) (tri.n2.Y * 127 + 127) << 8);
				c = ColorUtility.multiply(color, diffuse[lutID]);
				s = ColorUtility.scale(specular[lutID], reflectivity);
				currentColor = ColorUtility.add(c, s);
			}

			currentId = (tri.getParent().id << 16) | tri.id;

			x1 = p1.X << 8;
			x2 = p2.X << 8;
			x3 = p3.X << 8;
			y1 = p1.Y;
			y2 = p2.Y;
			y3 = p3.Y;

			x4 = x1 + (x3 - x1) * (y2 - y1) / (y3 - y1);
			x1 <<= 8; x2 <<= 8; x3 <<= 8; x4 <<= 8;

			z1 = p1.Z;
			z2 = p2.Z;
			z3 = p3.Z;
			nx1 = p1.nx << 16;
			nx2 = p2.nx << 16;
			nx3 = p3.nx << 16;
			ny1 = p1.ny << 16;
			ny2 = p2.ny << 16;
			ny3 = p3.ny << 16;
			tx1 = p1.tx << 16;
			tx2 = p2.tx << 16;
			tx3 = p3.tx << 16;
			ty1 = p1.ty << 16;
			ty2 = p2.ty << 16;
			ty3 = p3.ty << 16;

			dx = (x4 - x2) >> 16;
			if (dx == 0) return;

			temp = 256 * (y2 - y1) / (y3 - y1);

			z4 = z1 + ((z3 - z1) >> 8) * temp;
			nx4 = nx1 + ((nx3 - nx1) >> 8) * temp;
			ny4 = ny1 + ((ny3 - ny1) >> 8) * temp;
			tx4 = tx1 + ((tx3 - tx1) >> 8) * temp;
			ty4 = ty1 + ((ty3 - ty1) >> 8) * temp;

			dz = (z4 - z2) / dx;
			dnx = (nx4 - nx2) / dx;
			dny = (ny4 - ny2) / dx;
			dtx = (tx4 - tx2) / dx;
			dty = (ty4 - ty2) / dx;


			if (dx < 0)
			{
				temp = x2; x2 = x4; x4 = temp;
				z2 = z4;
				tx2 = tx4;
				ty2 = ty4;
				nx2 = nx4;
				ny2 = ny4;
			}
			if (y2 >= 0)
			{
				dy = y2 - y1;
				if (dy != 0)
				{
					dxL = (x2 - x1) / dy;
					dxR = (x4 - x1) / dy;
					dzBase = (z2 - z1) / dy;
					dnxBase = (nx2 - nx1) / dy;
					dnyBase = (ny2 - ny1) / dy;
					dtxBase = (tx2 - tx1) / dy;
					dtyBase = (ty2 - ty1) / dy;
				}

				xBase = x1;
				xMax = x1;
				zBase = z1;
				nxBase = nx1;
				nyBase = ny1;
				txBase = tx1;
				tyBase = ty1;

				if (y1 < 0)
				{
					xBase -= y1 * dxL;
					xMax -= y1 * dxR;
					zBase -= y1 * dzBase;
					nxBase -= y1 * dnxBase;
					nyBase -= y1 * dnyBase;
					txBase -= y1 * dtxBase;
					tyBase -= y1 * dtyBase;
					y1 = 0;
				}

				y2 = (y2 < height) ? y2 : height;
				offset = y1 * width;
				for (y = y1; y < y2; y++) renderLine();
			}

			if (y2 < height)
			{
				dy = y3 - y2;
				if (dy != 0)
				{
					dxL = (x3 - x2) / dy;
					dxR = (x3 - x4) / dy;
					dzBase = (z3 - z2) / dy;
					dnxBase = (nx3 - nx2) / dy;
					dnyBase = (ny3 - ny2) / dy;
					dtxBase = (tx3 - tx2) / dy;
					dtyBase = (ty3 - ty2) / dy;
				}

				xBase = x2;
				xMax = x4;
				zBase = z2;
				nxBase = nx2;
				nyBase = ny2;
				txBase = tx2;
				tyBase = ty2;

				if (y2 < 0)
				{
					xBase -= y2 * dxL;
					xMax -= y2 * dxR;
					zBase -= y2 * dzBase;
					nxBase -= y2 * dnxBase;
					nyBase -= y2 * dnyBase;
					txBase -= y2 * dtxBase;
					tyBase -= y2 * dtyBase;
					y2 = 0;
				}

				y3 = (y3 < height) ? y3 : height;
				offset = y2 * width;

				for (y = y2; y < y3; y++) renderLine();
			}
		}

        void renderLine()
		{
			xL = xBase >> 16;
			xR = xMax >> 16;
			z = zBase;
			nx = nxBase;
			ny = nyBase;
			tx = txBase;
			ty = tyBase;

			if (xL < 0)
			{
				z -= xL * dz;
				nx -= xL * dnx;
				ny -= xL * dny;
				tx -= xL * dtx;
				ty -= xL * dty;
				xL = 0;
			}
			xR = (xR < width) ? xR : width;

			if (mode == Flat) renderLineF();
			else if ((mode & SHADED) == Phong) renderLineP();
			else if ((mode & SHADED) == EnvMap) renderLineE();
			else if ((mode & SHADED) == Textured) renderLineT();
			else if ((mode & SHADED) == (Phong | EnvMap)) renderLinePE();
			else if ((mode & SHADED) == (Phong | Textured)) renderLinePT();
			else if ((mode & SHADED) == (Phong | EnvMap | Textured)) renderLinePET();

			offset += width;
			xBase += dxL;
			xMax += dxR;
			zBase += dzBase;
			nxBase += dnxBase;
			nyBase += dnyBase;
			txBase += dtxBase;
			tyBase += dtyBase;
		}

		// Fast scanline rendering

        void renderLineF()
		{
			for (x = xL; x < xR; x++)
			{
				pos = x + offset;
				if (z < zBuffer[pos])
				{
					bkgrd = screen.p[pos];
					c = ColorUtility.transparency(bkgrd, currentColor, transparency);

					screen.p[pos] = 0xFF000000 | c;
					zBuffer[pos] = (uint) z;
					if (useIdBuffer) idBuffer[pos] = currentId;
				}
				z += dz;
			}

		}

        void renderLineP()
		{
			for (x = xL; x < xR; x++)
			{
				pos = x + offset;
				if (z < zBuffer[pos])
				{
					lutID = ((nx >> 16) & 255) + (((ny >> 16) & 255) << 8);
					bkgrd = screen.p[pos];
					c = ColorUtility.multiply(color, diffuse[lutID]);
					s = specular[lutID];
					s = ColorUtility.scale(s, reflectivity);
					c = ColorUtility.transparency(bkgrd, c, transparency);
					c = ColorUtility.add(c, s);

					screen.p[pos] = 0xFF000000 | c;
					zBuffer[pos] = (uint) z;
					if (useIdBuffer) idBuffer[pos] = currentId;
				}
				z += dz;
				nx += dnx;
				ny += dny;
			}

		}

        void renderLineE()
		{
			for (x = xL; x < xR; x++)
			{
				pos = x + offset;
				if (z < zBuffer[pos])
				{
					lutID = ((nx >> 16) & 255) + (((ny >> 16) & 255) << 8);
					bkgrd = screen.p[pos];
					s = ColorUtility.add(specular[lutID], envmap[lutID]);
					s = ColorUtility.scale(s, reflectivity);
					c = ColorUtility.transparency(bkgrd, s, transparency);

					screen.p[pos] = 0xFF000000 | c;
					zBuffer[pos] = (uint) z;
					if (useIdBuffer) idBuffer[pos] = currentId;
				}
				z += dz;
				nx += dnx;
				ny += dny;
			}

		}

        void renderLineT()
		{
			for (x = xL; x < xR; x++)
			{
				pos = x + offset;
				if (z < zBuffer[pos])
				{
					bkgrd = screen.p[pos];
					c = texture.pixel[((tx >> 16) & tw) + (((ty >> 16) & th) << tbitW)];
					c = ColorUtility.transparency(bkgrd, c, transparency);

					screen.p[pos] = 0xFF000000 | c;
					zBuffer[pos] = (uint) z;
					if (useIdBuffer) idBuffer[pos] = currentId;
				}
				z += dz;
				tx += dtx;
				ty += dty;
			}

		}

        void renderLinePE()
		{
			for (x = xL; x < xR; x++)
			{
				pos = x + offset;
				if (z < zBuffer[pos])
				{
					lutID = ((nx >> 16) & 255) + (((ny >> 16) & 255) << 8);
					bkgrd = screen.p[pos];
					c = ColorUtility.multiply(color, diffuse[lutID]);
					s = ColorUtility.add(specular[lutID], envmap[lutID]);
					s = ColorUtility.scale(s, reflectivity);
					c = ColorUtility.transparency(bkgrd, c, transparency);
					c = ColorUtility.add(c, s);

					screen.p[pos] = 0xFF000000 | c;
					zBuffer[pos] = (uint) z;
					if (useIdBuffer) idBuffer[pos] = currentId;
				}
				z += dz;
				nx += dnx;
				ny += dny;
			}
		}

        void renderLinePT()
		{
            var screenP = screen.p;
            var texturePixel = texture.pixel;
			for (x = xL; x < xR; x++)
			{
				pos = x + offset;
				if (z < zBuffer[pos])
				{
					lutID = ((nx >> 16) & 255) + (((ny >> 16) & 255) << 8);
                    bkgrd = screenP[pos];
                    c = texturePixel[((tx >> 16) & tw) + (((ty >> 16) & th) << tbitW)];
					c = ColorUtility.multiply(c, diffuse[lutID]);
					s = specular[lutID];
					s = ColorUtility.scale(s, reflectivity);
					c = ColorUtility.transparency(bkgrd, c, transparency);
					c = ColorUtility.add(c, s);

					screenP[pos] = 0xFF000000 | c;
					zBuffer[pos] = (uint) z;
					if (useIdBuffer) idBuffer[pos] = currentId;
				}
				z += dz;
				nx += dnx;
				ny += dny;
				tx += dtx;
				ty += dty;
			}
		}

        void renderLinePET()
		{
			for (x = xL; x < xR; x++)
			{
				pos = x + offset;
				if (z < zBuffer[pos])
				{
					lutID = ((nx >> 16) & 255) + (((ny >> 16) & 255) << 8);
					bkgrd = screen.p[pos];
					c = texture.pixel[((tx >> 16) & tw) + (((ty >> 16) & th) << tbitW)];
					c = ColorUtility.multiply(c, diffuse[lutID]);
					s = ColorUtility.add(specular[lutID], envmap[lutID]);
					s = ColorUtility.scale(s, reflectivity);
					c = ColorUtility.transparency(bkgrd, c, transparency);
					c = ColorUtility.add(c, s);

					screen.p[pos] = 0xFF000000 | c;
					zBuffer[pos] = (uint) z;
					if (useIdBuffer) idBuffer[pos] = currentId;
				}
				z += dz;
				nx += dnx;
				ny += dny;
				tx += dtx;
				ty += dty;
			}
		}

        void drawWireframe(Triangle tri, uint defaultcolor)
		{
			drawLine(tri.p1, tri.p2, defaultcolor);
			drawLine(tri.p2, tri.p3, defaultcolor);
			drawLine(tri.p3, tri.p1, defaultcolor);
		}


        void drawLine(Vertex a, Vertex b, uint color)
		{
			Vertex temp;
			if ((a.clipcode & b.clipcode) != 0) return;

			dx = Math.Abs(a.X - b.X);
			dy = Math.Abs(a.Y - b.Y);
			dz = 0;

			if (dx > dy)
			{
				if (a.X > b.X) { temp = a; a = b; b = temp; }
				if (dx > 0)
				{
					dz = (b.Z - a.Z) / dx;
					dy = ((b.Y - a.Y) << 16) / dx;
				}
				z = a.Z;
				y = a.Y << 16;
                var bX = b.X;
                int xPlusOffset;
                for (x = a.X; x <= bX; x++)
				{
					y2 = y >> 16;
					if (MathUtility.inrange(x, 0, width - 1) && MathUtility.inrange(y2, 0, height - 1))
					{
						offset = y2 * width;
                        xPlusOffset = x + offset;
                        if (z < zBuffer[xPlusOffset])
						{
							if (!screen.antialias)
							{
								screen.p[xPlusOffset] = color;
								zBuffer[xPlusOffset] = (uint) z;
							}
							else
							{
								screen.p[xPlusOffset] = color;
								screen.p[xPlusOffset + 1] = color;
								screen.p[xPlusOffset + width] = color;
								screen.p[xPlusOffset + width + 1] = color;
								zBuffer[xPlusOffset] = (uint) z;
							}
						}
						if (useIdBuffer) idBuffer[xPlusOffset] = currentId;
					}
					z += dz; y += dy;
				}
			}
			else
			{
				if (a.Y > b.Y) { temp = a; a = b; b = temp; }
				if (dy > 0)
				{
					dz = (b.Z - a.Z) / dy;
					dx = ((b.X - a.X) << 16) / dy;
				}
				z = a.Z;
				x = a.X << 16;
                int bY = b.Y;
                int x2PlusOffset;
				for (y = a.Y; y <= bY; y++)
				{
					x2 = x >> 16;
					if (MathUtility.inrange(x2, 0, width - 1) && MathUtility.inrange(y, 0, height - 1))
					{
						offset = y * width;
                        x2PlusOffset = x2 + offset;
                        if (z < zBuffer[x2PlusOffset])
						{
							if (!screen.antialias)
							{
								screen.p[x2PlusOffset] = color;
								zBuffer[x2PlusOffset] = (uint) z;
							}
							else
							{
								screen.p[x2PlusOffset] = color;
								screen.p[x2PlusOffset + 1] = color;
								screen.p[x2PlusOffset + width] = color;
								screen.p[x2PlusOffset + width + 1] = color;
								zBuffer[x2PlusOffset] = (uint) z;
							}
						}
						if (useIdBuffer) idBuffer[x2PlusOffset] = currentId;
					}
					z += dz; x += dx;
				}
			}
		}
	}
}