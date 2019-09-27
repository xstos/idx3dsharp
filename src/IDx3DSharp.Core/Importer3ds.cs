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
using System.IO;
using System.Net;

namespace IDx3DSharp
{
	public class Importer3ds
	// Imports a scene from a 3ds (3d Studio Max) Ressource
	{
		// F I E L D S

        int currentJunkId;
        int nextJunkOffset;

        Scene scene;
        string currentObjectName;
        SceneObject currentObject;
        bool endOfStream;


		// P U B L I C   M E T H O D S

		public void importFromURL(Uri url, Scene targetscene)
        {
            importFromStream(
                url.Scheme == "http"
                    ? WebRequest.Create(url).GetResponse().GetResponseStream()
                    : File.OpenRead(url.ToString()), targetscene);
        }

		public void importFromStream(Stream inStream, Scene targetscene)
		{
			Console.WriteLine(">> Importing scene from 3ds stream ...");
			scene = targetscene;
			var input = new BinaryReader(inStream);
			readJunkHeader(input);
			if (currentJunkId != 0x4D4D)
			{
				Console.WriteLine("Error: This is no valid 3ds file.");
				return;
			}
			while (!endOfStream) readNextJunk(input);
			inStream.Close();
		}


		// P R I V A T E   M E T H O D S

        string readString(BinaryReader inStream)
		{
			byte num;
			var str = "";
			while ((num = inStream.ReadByte()) != 0)
				str = str + ((char) num);
			return str;
		}

        int readInt(BinaryReader inStream)
		{
			return (((inStream.ReadByte() | (inStream.ReadByte() << 8)) | (inStream.ReadByte() << 0x10)) | (inStream.ReadByte() << 0x18));
		}

        int readShort(BinaryReader inStream)
		{
			return (inStream.ReadByte() | (inStream.ReadByte() << 8));
		}

        float readFloat(BinaryReader input)
		{
			var num = readInt(input);
			var num2 = ((num >> 0x1f) == 0) ? 1 : -1;
			var num3 = (num >> 0x17) & 0xff;
			var num4 = (num3 == 0) ? ((num & 0x7fffff) << 1) : ((num & 0x7fffff) | 0x800000);
			var num5 = (num2 * num4) * Math.Pow(2.0, num3 - 150);
			return (float) num5;
		}


        void readJunkHeader(BinaryReader input)
		{
			currentJunkId = readShort(input);
			nextJunkOffset = readInt(input);
			endOfStream = currentJunkId < 0;
		}

        void readNextJunk(BinaryReader input)
		{
			readJunkHeader(input);

			if (currentJunkId == 0x3D3D) return; // Mesh block
			if (currentJunkId == 0x4000) // Object block
			{
				currentObjectName = readString(input);
				Console.WriteLine(">> Importing object: " + currentObjectName);
				return;
			}
			if (currentJunkId == 0x4100)  // Triangular polygon object
			{
				currentObject = new SceneObject();
				scene.addObject(currentObjectName, currentObject);
				return;
			}
			if (currentJunkId == 0x4110) // Vertex list
			{
				readVertexList(input);
				return;
			}
			if (currentJunkId == 0x4120) // Point list
			{
				readPointList(input);
				return;
			}
			if (currentJunkId == 0x4140) // Mapping coordinates
			{
				readMappingCoordinates(input);
				return;
			}

			skipJunk(input);
		}

        void skipJunk(BinaryReader inStream)
		{
			try
			{
				for (var i = 0; (i < (nextJunkOffset - 6)) && !endOfStream; i++)
				{
					endOfStream = inStream.ReadByte() < 0;
				}
			}
			catch (Exception)
			{
				endOfStream = true;
			}
		}

        void readVertexList(BinaryReader input)
		{
			float x, y, z;
			var vertices = readShort(input);
			for (var i = 0; i < vertices; i++)
			{
				x = readFloat(input);
				y = readFloat(input);
				z = readFloat(input);
				currentObject.addVertex(x, -y, z);
			}
		}

        void readPointList(BinaryReader input)
		{
			int v1, v2, v3;
			var triangles = readShort(input);
			for (var i = 0; i < triangles; i++)
			{
				v1 = readShort(input);
				v2 = readShort(input);
				v3 = readShort(input);
				readShort(input);
				currentObject.addTriangle(
					currentObject.Vertex(v1),
					currentObject.Vertex(v2),
					currentObject.Vertex(v3));
			}
		}

        void readMappingCoordinates(BinaryReader input)
		{
			var vertices = readShort(input);
			for (var i = 0; i < vertices; i++)
			{
				currentObject.Vertex(i).Tu = readFloat(input);
				currentObject.Vertex(i).Tv = readFloat(input);
			}
		}
	}
}