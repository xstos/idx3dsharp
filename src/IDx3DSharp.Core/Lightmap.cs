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
	/// Lightmap for faster rendering, assuming static light sources.
	/// </summary>
public sealed class Lightmap
{
	public uint[] diffuse=new uint[65536];
	public uint[] specular=new uint[65536];
    float[] sphere=new float[65536];
    Light[] light;
    uint lights;
    uint ambient;
    int temp,overflow,color,pos,r,g,b;
	
	public Lightmap(Scene scene)
	{
		scene.rebuild();
		light=scene._light;
		lights=scene.lights;
		ambient=scene.environment.ambient;
		buildSphereMap();
		rebuildLightmap();
	}

    const float divBy128 = 1f / 128f;
    void buildSphereMap()
	{
		float fnx,fny,fnz;
		int pos;
		for (var ny=-128;ny<128;ny++)
		{
			fny=(float)ny*divBy128;
			for (var nx=-128;nx<128;nx++)
			{
				pos=nx+128+((ny+128)<<8);
				fnx=nx*divBy128;
				fnz=(float)(1-Math.Sqrt(fnx*fnx+fny*fny));
				sphere[pos]=(fnz>0)?fnz:0;
			}
		}
	}

    const float divBy255 = 1f / 255f;
    const float divBy4096 = 1f / 4096f;
	public void rebuildLightmap()
	{
		Console.WriteLine(">> Rebuilding Light Map  ...  ["+lights+" light sources]");
		Vector l;
		float fnx,fny,angle,phongfact,sheen, spread;
		uint diffuse, specular, cos, dr, dg, db, sr, sg, sb;
		for (var ny=-128;ny<128;ny++)
		{
			fny=(float)ny*divBy128;
			for (var nx=-128;nx<128;nx++)
			{
				pos=nx+128+((ny+128)<<8);
				fnx=nx*divBy128;
				sr=sg=sb=0;
				dr = ColorUtility.getRed(ambient);
				dg = ColorUtility.getGreen(ambient);
				db = ColorUtility.getBlue(ambient);
				for (var i=0;i<lights;i++)
				{		
					l=light[i].v;
					diffuse=light[i].diffuse;
					specular=light[i].specular;
					sheen=light[i].highlightSheen*divBy255;
					spread=(float)light[i].highlightSpread*divBy4096;
					spread=(spread<0.01f)?0.01f:spread;
					cos=(uint)(255*Vector.Angle(light[i].v,new Vector(fnx,fny,sphere[pos])));
					cos=(cos>0)?cos:0;
					dr += (ColorUtility.getRed(diffuse) * cos) >> 8;
					dg += (ColorUtility.getGreen(diffuse) * cos) >> 8;
					db += (ColorUtility.getBlue(diffuse) * cos) >> 8;
					phongfact=sheen*(float)Math.Pow(cos*divBy255,1/spread);
					sr += (uint) (ColorUtility.getRed(specular) * phongfact);
					sg += (uint) (ColorUtility.getGreen(specular) * phongfact);
					sb += (uint) (ColorUtility.getBlue(specular) * phongfact);
				}
				this.diffuse[pos] = ColorUtility.getCropColor(dr, dg, db);
				this.specular[pos] = ColorUtility.getCropColor(sr, sg, sb);
			}
		}
	}
}	
}