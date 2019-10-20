using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using Timer = System.Timers.Timer;

namespace IDx3DSharp.DemoApp.Demos
{
    public class TestComponent2Old : BaseDemo
    {
        public override void PopulateScene(Scene scene)
        {
            scene.addMaterial("Stone1", new Material(new Texture("stone1.jpg")));
            scene.addMaterial("Stone2", new Material(new Texture("stone2.jpg")));
            scene.addMaterial("Stone3", new Material(new Texture("stone3.jpg")));
            scene.addMaterial("Stone4", new Material(new Texture("stone4.jpg")));

            scene.addLight("Light1", new Light(new Vector(0.2f, 0.2f, 1f), 0xFFFFFF, 144, 120));
            scene.addLight("Light2", new Light(new Vector(-1f, -1f, 1f), 0x332211, 100, 40));
            scene.addLight("Light3", new Light(new Vector(-1f, -1f, 1f), 0x666666, 200, 120));

            try
            {
                new Importer3ds().importFromStream(File.OpenRead("wobble.3ds"), scene);
            }
            catch (Exception e) { Console.WriteLine(e + ""); }

            scene.rebuild();
            for (var i = 0; i < scene.objects; i++)
                TextureProjector.ProjectFrontal(scene._object[i]);

            scene.Object("Sphere1").setMaterial(scene.material("Stone1"));
            scene.Object("Wobble1").setMaterial(scene.material("Stone2"));
            scene.Object("Wobble2").setMaterial(scene.material("Stone3"));
            scene.Object("Wobble3").setMaterial(scene.material("Stone4"));
            scene.normalize();
        }
    }

    public class TestComponent2 : BaseDemo
    {
        
        
        public override void PopulateScene(Scene scene)
        {
            void transformDemo(SceneObject sceneObject)
            {
                Timer tmr = new Timer(1);

                tmr.Elapsed += (sender, args) =>
                {
                    sceneObject.rotate(0, 0f, 0.1f);
                    sceneObject.shift(0, 0, 5);
                };
                tmr.Start();
            }

            scene.addMaterial("Stone1", new Material(new Texture("stone1.jpg")));
            scene.addMaterial("Stone2", new Material(new Texture("stone2.jpg")));
            scene.addMaterial("Stone3", new Material(new Texture("stone3.jpg")));
            scene.addMaterial("Stone4", new Material(new Texture("stone4.jpg")));
            var texture = new Texture(2,2)[Color.Red,Color.Blue,Color.Yellow,Color.Green];
            scene.addMaterial("tiny",new Material(texture).setWireframe(true).setColor(Color.DeepPink));
            scene.addLight("Light1", new Light(new Vector(0.2f, 0.2f, 1f), 0xFFFFFF, 144, 120));
            scene.addLight("Light2", new Light(new Vector(-1f, -1f, 1f), 0x332211, 100, 40));
            scene.addLight("Light3", new Light(new Vector(-1f, -1f, 1f), 0x666666, 200, 120));
            var box = Square(scene);
            //new Importer3ds().importFromStream(File.OpenRead("wobble.3ds"), scene);

            //scene.addObject("tri", cube);
            scene.rebuild();
            for (var i = 0; i < scene.objects; i++)
                TextureProjector.ProjectFrontal(scene._object[i]);
            scene.Object("Sphere1").setMaterial(scene.material("tiny"));
            //scene.Object("Wobble1").setMaterial(scene.material("Stone2"));
            //scene.Object("Wobble2").setMaterial(scene.material("Stone3"));
            //scene.Object("Wobble3").setMaterial(scene.material("Stone4"));
            //scene.Object("tri").setMaterial(scene.material("Stone4"));
            //scene.normalize();
        }

        public static SceneObject Square(Scene scene)
        {
            var sceneObject = new SceneObject();
            scene.addObject("Sphere1",sceneObject);
            
            
            PlusWrapper<(float,float,float)> list = new List<(float, float, float)>();
            list += (0f, 0f, 100f);
            list += (-50f,50f,100f);
            list += (-50f,0f,100f);
            list += (0f,50f,100f);

            //list = sphereVerts(list);

            foreach (var v in list.inner)
            {
                sceneObject.addVertex(v.Item1,v.Item2,v.Item3);
            }
            PlusWrapper<(int,int,int)> ptNums = new List<(int,int,int)>();
            //var ptNums = new (int, int, int)[224];

            ptNums += (0, 1, 2);
            ptNums += (0, 3, 1);
            //ptNums = spherePoints(ptNums);
            foreach (var item in ptNums.inner)
            {
                sceneObject.addTriangle(
                    sceneObject.vertexData[item.Item1],
                    sceneObject.vertexData[item.Item2],
                    sceneObject.vertexData[item.Item3]);
            }

            return sceneObject;
        }
        public SceneObject MakeCube()
        {
            //Create a 'Cube' mesh...

            //2) Define the cube's dimensions
            float length = 10f;
            float width = 10f;
            float height = 10f;


            //3) Define the co-ordinates of each Corner of the cube 
            Vertex[] c = new Vertex[8];

            c[0] = (-length * .5f, -width * .5f, height * .5f);
            c[1] = (length * .5f, -width * .5f, height * .5f);
            c[2] = (length * .5f, -width * .5f, -height * .5f);
            c[3] = (-length * .5f, -width * .5f, -height * .5f);
            c[4] = (-length * .5f, width * .5f, height * .5f);
            c[5] = (length * .5f, width * .5f, height * .5f);
            c[6] = (length * .5f, width * .5f, -height * .5f);
            c[7] = (-length * .5f, width * .5f, -height * .5f);


            //4) Define the vertices that the cube is composed of:
            //I have used 16 vertices (4 vertices per side). 
            //This is because I want the vertices of each side to have separate normals.
            //(so the object renders light/shade correctly) 
            Vertex[] vertices = new Vertex[]
            {
            c[0], c[1], c[2], c[3], // Bottom
	        c[7], c[4], c[0], c[3], // Left
	        c[4], c[5], c[1], c[0], // Front
	        c[6], c[7], c[3], c[2], // Back
	        c[5], c[6], c[2], c[1], // Right
	        c[7], c[6], c[5], c[4]  // Top
            };


            //5) Define each vertex's Normal
            Vertex up = (0, 1, 0);
            Vertex down = (0, -1, 0);
            Vertex forward = (0, 0, 1);
            Vertex back = (0, 0, -1);
            Vertex left = (-1, 0, 0);
            Vertex right = (1, 0, 0);


            Vertex[] normals = new Vertex[]
            {
            down, down, down, down,             // Bottom
	        left, left, left, left,             // Left
	        forward, forward, forward, forward,	// Front
	        back, back, back, back,             // Back
	        right, right, right, right,         // Right
	        up, up, up, up                      // Top
            };


            //6) Define each vertex's UV co-ordinates
            var uv00 = (0f, 0f);
            var uv10 = (1f, 0f);
            var uv01 = (0f, 1f);
            var uv11 = (1f, 1f);

            var uvs = new (float, float)[]
            {
            uv11, uv01, uv00, uv10, // Bottom
	        uv11, uv01, uv00, uv10, // Left
	        uv11, uv01, uv00, uv10, // Front
	        uv11, uv01, uv00, uv10, // Back	        
	        uv11, uv01, uv00, uv10, // Right 
	        uv11, uv01, uv00, uv10  // Top
            };


            //7) Define the Polygons (triangles) that make up the our Mesh (cube)
            //IMPORTANT: Unity uses a 'Clockwise Winding Order' for determining front-facing polygons.
            //This means that a polygon's vertices must be defined in 
            //a clockwise order (relative to the camera) in order to be rendered/visible.
            int[] triangles = new int[]
            {
            3, 1, 0,        3, 2, 1,        // Bottom	
	        7, 5, 4,        7, 6, 5,        // Left
	        11, 9, 8,       11, 10, 9,      // Front
	        15, 13, 12,     15, 14, 13,     // Back
	        19, 17, 16,     19, 18, 17,	    // Right
	        23, 21, 20,     23, 22, 21,     // Top
            };

            var obj = new SceneObject();
            foreach (var vertex in vertices)
            {
                obj.addVertex(vertex.X, vertex.Y, vertex.Z);

            }

            for (int i = 0; i < vertices.Length; i++)
            {
                obj.vertexData[i].setUV(uvs[i].Item1, uvs[i].Item2);
            }
            for (int i = 0; i < triangles.Length; i += 3)
            {
                obj.addTriangle(
                    obj.vertexData[triangles[i]],
                    obj.vertexData[triangles[i + 1]],
                    obj.vertexData[triangles[i + 2]]);
            }
            //8) Build the Mesh
            //mesh.Clear();
            //mesh.vertices = vertices;
            //mesh.triangles = triangles;
            //mesh.normals = normals;
            //mesh.uv = uvs;
            //mesh.Optimize();
            return obj;
        }

        static PlusWrapper<(float, float, float)> sphereVerts(PlusWrapper<(float, float, float)> list)
        {
            list += (6.508263f, -22.09559f, 44.21677f);
            list += (6.508263f, -34.52529f, 41.74435f);
            list += (1.751622f, -33.57914f, 41.74435f);
            list += (-2.280863f, -30.88471f, 41.74435f);
            list += (-4.975283f, -26.85223f, 41.74435f);
            list += (-5.921438f, -22.09559f, 41.74435f);
            list += (-4.975282f, -17.33895f, 41.74435f);
            list += (-2.280861f, -13.30646f, 41.74435f);
            list += (1.751624f, -10.61204f, 41.74435f);
            list += (6.508265f, -9.665888f, 41.74435f);
            list += (11.26491f, -10.61204f, 41.74435f);
            list += (15.29739f, -13.30646f, 41.74435f);
            list += (17.99181f, -17.33895f, 41.74435f);
            list += (18.93796f, -22.09559f, 41.74435f);
            list += (17.99181f, -26.85223f, 41.74435f);
            list += (15.29739f, -30.88472f, 41.74435f);
            list += (11.2649f, -33.57914f, 41.74435f);
            list += (6.508262f, -45.06268f, 34.70349f);
            list += (-2.280864f, -43.31441f, 34.70349f);
            list += (-9.731926f, -38.33578f, 34.70349f);
            list += (-14.71057f, -30.88471f, 34.70349f);
            list += (-16.45883f, -22.09559f, 34.70349f);
            list += (-14.71056f, -13.30646f, 34.70349f);
            list += (-9.731922f, -5.8554f, 34.70349f);
            list += (-2.28086f, -0.8767605f, 34.70349f);
            list += (6.508266f, 0.8715038f, 34.70349f);
            list += (15.29739f, -0.8767624f, 34.70349f);
            list += (22.74845f, -5.855404f, 34.70349f);
            list += (27.72709f, -13.30647f, 34.70349f);
            list += (29.47536f, -22.09559f, 34.70349f);
            list += (27.72709f, -30.88472f, 34.70349f);
            list += (22.74845f, -38.33578f, 34.70349f);
            list += (15.29738f, -43.31441f, 34.70349f);
            list += (6.508262f, -52.10354f, 24.1661f);
            list += (-4.975284f, -49.81932f, 24.1661f);
            list += (-14.71057f, -43.31441f, 24.1661f);
            list += (-21.21547f, -33.57913f, 24.1661f);
            list += (-23.49969f, -22.09559f, 24.1661f);
            list += (-21.21547f, -10.61204f, 24.1661f);
            list += (-14.71056f, -0.8767605f, 24.1661f);
            list += (-4.975279f, 5.628145f, 24.1661f);
            list += (6.508267f, 7.912363f, 24.1661f);
            list += (17.99181f, 5.628141f, 24.1661f);
            list += (27.72709f, -0.8767662f, 24.1661f);
            list += (34.232f, -10.61205f, 24.1661f);
            list += (36.51622f, -22.09559f, 24.1661f);
            list += (34.23199f, -33.57914f, 24.1661f);
            list += (27.72709f, -43.31442f, 24.1661f);
            list += (17.9918f, -49.81932f, 24.1661f);
            list += (6.508262f, -54.57596f, 11.7364f);
            list += (-5.92144f, -52.10354f, 11.7364f);
            list += (-16.45883f, -45.06268f, 11.7364f);
            list += (-23.49969f, -34.52529f, 11.7364f);
            list += (-25.97211f, -22.09559f, 11.7364f);
            list += (-23.49969f, -9.665885f, 11.7364f);
            list += (-16.45883f, 0.8715057f, 11.7364f);
            list += (-5.921434f, 7.912365f, 11.7364f);
            list += (6.508267f, 10.38478f, 11.7364f);
            list += (18.93797f, 7.912361f, 11.7364f);
            list += (29.47536f, 0.8715f, 11.7364f);
            list += (36.51622f, -9.665893f, 11.7364f);
            list += (38.98864f, -22.09559f, 11.7364f);
            list += (36.51621f, -34.5253f, 11.7364f);
            list += (29.47535f, -45.06268f, 11.7364f);
            list += (18.93796f, -52.10355f, 11.7364f);
            list += (6.508262f, -52.10354f, -0.693305f);
            list += (-4.975284f, -49.81932f, -0.693305f);
            list += (-14.71057f, -43.31441f, -0.693305f);
            list += (-21.21547f, -33.57913f, -0.693305f);
            list += (-23.49969f, -22.09559f, -0.693305f);
            list += (-21.21547f, -10.61204f, -0.693305f);
            list += (-14.71056f, -0.8767605f, -0.693305f);
            list += (-4.975279f, 5.628145f, -0.693305f);
            list += (6.508267f, 7.912363f, -0.693305f);
            list += (17.99181f, 5.628141f, -0.693305f);
            list += (27.72709f, -0.8767662f, -0.693305f);
            list += (34.232f, -10.61205f, -0.693305f);
            list += (36.51622f, -22.09559f, -0.693305f);
            list += (34.23199f, -33.57914f, -0.693305f);
            list += (27.72709f, -43.31442f, -0.693305f);
            list += (17.9918f, -49.81932f, -0.693305f);
            list += (6.508262f, -45.06268f, -11.2307f);
            list += (-2.280863f, -43.31441f, -11.2307f);
            list += (-9.731924f, -38.33577f, -11.2307f);
            list += (-14.71056f, -30.88471f, -11.2307f);
            list += (-16.45883f, -22.09559f, -11.2307f);
            list += (-14.71056f, -13.30646f, -11.2307f);
            list += (-9.73192f, -5.855402f, -11.2307f);
            list += (-2.280859f, -0.8767624f, -11.2307f);
            list += (6.508266f, 0.8715019f, -11.2307f);
            list += (15.29739f, -0.8767643f, -11.2307f);
            list += (22.74845f, -5.855406f, -11.2307f);
            list += (27.72709f, -13.30647f, -11.2307f);
            list += (29.47535f, -22.09559f, -11.2307f);
            list += (27.72709f, -30.88472f, -11.2307f);
            list += (22.74845f, -38.33578f, -11.2307f);
            list += (15.29738f, -43.31441f, -11.2307f);
            list += (6.508263f, -34.52529f, -18.27156f);
            list += (1.751623f, -33.57913f, -18.27156f);
            list += (-2.280862f, -30.88471f, -18.27156f);
            list += (-4.975281f, -26.85223f, -18.27156f);
            list += (-5.921436f, -22.09559f, -18.27156f);
            list += (-4.975281f, -17.33895f, -18.27156f);
            list += (-2.28086f, -13.30646f, -18.27156f);
            list += (1.751625f, -10.61204f, -18.27156f);
            list += (6.508265f, -9.66589f, -18.27156f);
            list += (11.2649f, -10.61205f, -18.27156f);
            list += (15.29739f, -13.30647f, -18.27156f);
            list += (17.99181f, -17.33895f, -18.27156f);
            list += (18.93796f, -22.09559f, -18.27156f);
            list += (17.99181f, -26.85223f, -18.27156f);
            list += (15.29739f, -30.88472f, -18.27156f);
            list += (11.2649f, -33.57913f, -18.27156f);
            list += (6.508263f, -22.09559f, -20.74398f);
            return list;
        }

        static PlusWrapper<(int, int, int)> spherePoints(PlusWrapper<(int, int, int)> ptNums)
        {
            ptNums += (0, 2, 3);
            ptNums += (0, 3, 4);
            ptNums += (0, 4, 5);
            ptNums += (0, 5, 6);
            ptNums += (0, 6, 7);
            ptNums += (0, 7, 8);
            ptNums += (0, 8, 9);
            ptNums += (0, 9, 10);
            ptNums += (0, 10, 11);
            ptNums += (0, 11, 12);
            ptNums += (0, 12, 13);
            ptNums += (0, 13, 14);
            ptNums += (0, 14, 15);
            ptNums += (0, 15, 16);
            ptNums += (0, 16, 1);
            ptNums += (1, 17, 18);
            ptNums += (1, 18, 2);
            ptNums += (2, 18, 19);
            ptNums += (2, 19, 3);
            ptNums += (3, 19, 20);
            ptNums += (3, 20, 4);
            ptNums += (4, 20, 21);
            ptNums += (4, 21, 5);
            ptNums += (5, 21, 22);
            ptNums += (5, 22, 6);
            ptNums += (6, 22, 23);
            ptNums += (6, 23, 7);
            ptNums += (7, 23, 24);
            ptNums += (7, 24, 8);
            ptNums += (8, 24, 25);
            ptNums += (8, 25, 9);
            ptNums += (9, 25, 26);
            ptNums += (9, 26, 10);
            ptNums += (10, 26, 27);
            ptNums += (10, 27, 11);
            ptNums += (11, 27, 28);
            ptNums += (11, 28, 12);
            ptNums += (12, 28, 29);
            ptNums += (12, 29, 13);
            ptNums += (13, 29, 30);
            ptNums += (13, 30, 14);
            ptNums += (14, 30, 31);
            ptNums += (14, 31, 15);
            ptNums += (15, 31, 32);
            ptNums += (15, 32, 16);
            ptNums += (16, 32, 17);
            ptNums += (16, 17, 1);
            ptNums += (17, 33, 34);
            ptNums += (17, 34, 18);
            ptNums += (18, 34, 35);
            ptNums += (18, 35, 19);
            ptNums += (19, 35, 36);
            ptNums += (19, 36, 20);
            ptNums += (20, 36, 37);
            ptNums += (20, 37, 21);
            ptNums += (21, 37, 38);
            ptNums += (21, 38, 22);
            ptNums += (22, 38, 39);
            ptNums += (22, 39, 23);
            ptNums += (23, 39, 40);
            ptNums += (23, 40, 24);
            ptNums += (24, 40, 41);
            ptNums += (24, 41, 25);
            ptNums += (25, 41, 42);
            ptNums += (25, 42, 26);
            ptNums += (26, 42, 43);
            ptNums += (26, 43, 27);
            ptNums += (27, 43, 44);
            ptNums += (27, 44, 28);
            ptNums += (28, 44, 45);
            ptNums += (28, 45, 29);
            ptNums += (29, 45, 46);
            ptNums += (29, 46, 30);
            ptNums += (30, 46, 47);
            ptNums += (30, 47, 31);
            ptNums += (31, 47, 48);
            ptNums += (31, 48, 32);
            ptNums += (32, 48, 33);
            ptNums += (32, 33, 17);
            ptNums += (33, 49, 50);
            ptNums += (33, 50, 34);
            ptNums += (34, 50, 51);
            ptNums += (34, 51, 35);
            ptNums += (35, 51, 52);
            ptNums += (35, 52, 36);
            ptNums += (36, 52, 53);
            ptNums += (36, 53, 37);
            ptNums += (37, 53, 54);
            ptNums += (37, 54, 38);
            ptNums += (38, 54, 55);
            ptNums += (38, 55, 39);
            ptNums += (39, 55, 56);
            ptNums += (39, 56, 40);
            ptNums += (40, 56, 57);
            ptNums += (40, 57, 41);
            ptNums += (41, 57, 58);
            ptNums += (41, 58, 42);
            ptNums += (42, 58, 59);
            ptNums += (42, 59, 43);
            ptNums += (43, 59, 60);
            ptNums += (43, 60, 44);
            ptNums += (44, 60, 61);
            ptNums += (44, 61, 45);
            ptNums += (45, 61, 62);
            ptNums += (45, 62, 46);
            ptNums += (46, 62, 63);
            ptNums += (46, 63, 47);
            ptNums += (47, 63, 64);
            ptNums += (47, 64, 48);
            ptNums += (48, 64, 49);
            ptNums += (48, 49, 33);
            ptNums += (49, 65, 66);
            ptNums += (49, 66, 50);
            ptNums += (50, 66, 67);
            ptNums += (50, 67, 51);
            ptNums += (51, 67, 68);
            ptNums += (51, 68, 52);
            ptNums += (52, 68, 69);
            ptNums += (52, 69, 53);
            ptNums += (53, 69, 70);
            ptNums += (53, 70, 54);
            ptNums += (54, 70, 71);
            ptNums += (54, 71, 55);
            ptNums += (55, 71, 72);
            ptNums += (55, 72, 56);
            ptNums += (56, 72, 73);
            ptNums += (56, 73, 57);
            ptNums += (57, 73, 74);
            ptNums += (57, 74, 58);
            ptNums += (58, 74, 75);
            ptNums += (58, 75, 59);
            ptNums += (59, 75, 76);
            ptNums += (59, 76, 60);
            ptNums += (60, 76, 77);
            ptNums += (60, 77, 61);
            ptNums += (61, 77, 78);
            ptNums += (61, 78, 62);
            ptNums += (62, 78, 79);
            ptNums += (62, 79, 63);
            ptNums += (63, 79, 80);
            ptNums += (63, 80, 64);
            ptNums += (64, 80, 65);
            ptNums += (64, 65, 49);
            ptNums += (65, 81, 82);
            ptNums += (65, 82, 66);
            ptNums += (66, 82, 83);
            ptNums += (66, 83, 67);
            ptNums += (67, 83, 84);
            ptNums += (67, 84, 68);
            ptNums += (68, 84, 85);
            ptNums += (68, 85, 69);
            ptNums += (69, 85, 86);
            ptNums += (69, 86, 70);
            ptNums += (70, 86, 87);
            ptNums += (70, 87, 71);
            ptNums += (71, 87, 88);
            ptNums += (71, 88, 72);
            ptNums += (72, 88, 89);
            ptNums += (72, 89, 73);
            ptNums += (73, 89, 90);
            ptNums += (73, 90, 74);
            ptNums += (74, 90, 91);
            ptNums += (74, 91, 75);
            ptNums += (75, 91, 92);
            ptNums += (75, 92, 76);
            ptNums += (76, 92, 93);
            ptNums += (76, 93, 77);
            ptNums += (77, 93, 94);
            ptNums += (77, 94, 78);
            ptNums += (78, 94, 95);
            ptNums += (78, 95, 79);
            ptNums += (79, 95, 96);
            ptNums += (79, 96, 80);
            ptNums += (80, 96, 81);
            ptNums += (80, 81, 65);
            ptNums += (81, 97, 98);
            ptNums += (81, 98, 82);
            ptNums += (82, 98, 99);
            ptNums += (82, 99, 83);
            ptNums += (83, 99, 100);
            ptNums += (83, 100, 84);
            ptNums += (84, 100, 101);
            ptNums += (84, 101, 85);
            ptNums += (85, 101, 102);
            ptNums += (85, 102, 86);
            ptNums += (86, 102, 103);
            ptNums += (86, 103, 87);
            ptNums += (87, 103, 104);
            ptNums += (87, 104, 88);
            ptNums += (88, 104, 105);
            ptNums += (88, 105, 89);
            ptNums += (89, 105, 106);
            ptNums += (89, 106, 90);
            ptNums += (90, 106, 107);
            ptNums += (90, 107, 91);
            ptNums += (91, 107, 108);
            ptNums += (91, 108, 92);
            ptNums += (92, 108, 109);
            ptNums += (92, 109, 93);
            ptNums += (93, 109, 110);
            ptNums += (93, 110, 94);
            ptNums += (94, 110, 111);
            ptNums += (94, 111, 95);
            ptNums += (95, 111, 112);
            ptNums += (95, 112, 96);
            ptNums += (96, 112, 97);
            ptNums += (96, 97, 81);
            ptNums += (113, 98, 97);
            ptNums += (113, 99, 98);
            ptNums += (113, 100, 99);
            ptNums += (113, 101, 100);
            ptNums += (113, 102, 101);
            ptNums += (113, 103, 102);
            ptNums += (113, 104, 103);
            ptNums += (113, 105, 104);
            ptNums += (113, 106, 105);
            ptNums += (113, 107, 106);
            ptNums += (113, 108, 107);
            ptNums += (113, 109, 108);
            ptNums += (113, 110, 109);
            ptNums += (113, 111, 110);
            ptNums += (113, 112, 111);
            ptNums += (113, 97, 112);
            return ptNums;
        }

    }
}