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

namespace IDx3DSharp
{
	public static class TextureProjector
	{
		public static void ProjectFrontal(SceneObject obj)
		{
			obj.rebuild();
			var min = obj.Min();
			var max = obj.Max();
			var du = 1 / (max.X - min.X);
			var dv = 1 / (max.Y - min.Y);
			for (var i = 0; i < obj.numVertices; i++)
			{
				obj.vertices[i].Tu = (obj.vertices[i].pos.X - min.X) * du;
				obj.vertices[i].Tv = 1 - (obj.vertices[i].pos.Y - min.Y) * dv;
			}
		}

		public static void ProjectTop(SceneObject obj)
		{
			obj.rebuild();
			var min = obj.Min();
			var max = obj.Max();
			var du = 1 / (max.X - min.X);
			var dv = 1 / (max.Z - min.Z);
			for (var i = 0; i < obj.numVertices; i++)
			{
				obj.vertices[i].Tu = (obj.vertices[i].pos.X - min.X) * du;
				obj.vertices[i].Tv = (obj.vertices[i].pos.Z - min.Z) * dv;
			}
		}

		public static void ProjectCylindric(SceneObject obj)
		{
			obj.rebuild();
			var min = obj.Min();
			var max = obj.Max();
			var dz = 1 / (max.Z - min.Z);
			for (var i = 0; i < obj.numVertices; i++)
			{
				obj.vertices[i].pos.BuildCylindric();
				obj.vertices[i].Tu = obj.vertices[i].pos.Theta / (2 * 3.14159265f);
				obj.vertices[i].Tv = (obj.vertices[i].pos.Z - min.Z) * dz;
			}
		}
	}
}