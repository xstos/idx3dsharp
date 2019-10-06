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
using System.Collections.Generic;
using System.Text;

namespace IDx3DSharp
{
public class SceneObject : CoreObject
{
	// F I E L D S
	
		public object userData=null;	// Can be freely used
		public string user=null; 	// Can be freely used

        public List<Vertex> vertexData = new List<Vertex>();
        public List<Triangle> triangleData = new List<Triangle>();
		
		public uint id;  // This object's index
		public string name="";  // This object's name
		public bool visible=true; // Visibility tag
		
		public Scene parent=null;
        bool dirty=true;  // Flag for dirty handling
		
		public Vertex[] vertices;
		public Triangle[] triangles;
		
		public int numVertices;
		public uint numTriangles;
		
		public Material material; 

	// C O N S T R U C T O R S

		public SceneObject()
		{
		}

	// D A T A  S T R U C T U R E S
	
		public Vertex Vertex(int id)
		{
			return vertexData[id];
		}
		
		public Triangle Triangle(int id)
		{
			return triangleData[id];
		}		

		public void addVertex(Vertex newVertex)
		{
			newVertex.parent=this;
			vertexData.Add(newVertex);
			dirty=true;
		}

		public void addTriangle(Triangle newTriangle)
		{
			newTriangle.parent=this;
			triangleData.Add(newTriangle);
			dirty=true;
		}
		
		public void addTriangle(int v1, int v2, int v3)
		{
			addTriangle(Vertex(v1),Vertex(v2),Vertex(v3));
		}
		
		public void removeVertex(Vertex v)
		{
			vertexData.Remove(v);
		}
		
		public void removeTriangle(Triangle t)
		{
			triangleData.Remove(t);
		}
		
		public void removeVertexAt(int pos)
		{
			vertexData.RemoveAt(pos);
		}
		
		public void removeTriangleAt(int pos)
		{
			triangleData.RemoveAt(pos);
		}
		
		
		public void setMaterial(Material m)
		{
			material=m;
		}
		
		public void rebuild()
		{
			if (!dirty) return;
			dirty=false;
			
			// Generate faster structure for vertices
			numVertices=vertexData.Count;
			vertices=new Vertex[numVertices];
			vertexData.CopyTo(vertices);
			
			// Generate faster structure for triangles
			numTriangles=(uint) triangleData.Count;
			triangles=new Triangle[numTriangles];
			triangleData.CopyTo(triangles);
			for (uint i=0;i<numTriangles;i++)
			{
				triangles[i].id=i;
			}

            for (int i = 0; i < numVertices; i++)
            {
                vertices[i].id=i;
                vertices[i].resetNeighbors();
            }
			
			Triangle tri;
			for (uint i=0;i<numTriangles;i++)
			{
				tri=triangles[i];
				tri.p1.registerNeighbor(tri);
				tri.p2.registerNeighbor(tri);
				tri.p3.registerNeighbor(tri);
			}

			regenerate();
		}

		public void addVertex(float x, float y, float z)
		{
			addVertex(new Vertex(x,y,z));
		}
		
		
		public void addVertex(float x, float y, float z, float u, float v)
		{
			var vert=new Vertex(x,y,z);
			vert.setUV(u,v);
			addVertex(vert);
		}

		public void addTriangle(Vertex a, Vertex b, Vertex c)
		{
			addTriangle(new Triangle(a,b,c));
		}

		public void regenerate()
		// Regenerates the vertex normals
		{
			for (var i=0;i<numTriangles;i++)
            {
                triangles[i].regenerateNormal();
            }

            for (var i=0;i<numVertices;i++)
            {
                vertices[i].regenerateNormal();
            }
        }

        public override string ToString()
        {
            var buffer = new StringBuilder();
            buffer.Append("<object id=" + name + ">\r\n");
            for (var i = 0; i < numVertices; i++) buffer.Append(vertices[i]);
            return buffer.ToString();
        }
		
		public void scaleTextureCoordinates(float fu, float fv)
		{
			rebuild();
			for (var i=0;i<numVertices;i++) vertices[i].scaleTextureCoordinates(fu,fv);
		}
		
		public void tilt(float fact)
		{
			rebuild();
			for (var i=0;i<numVertices;i++)
				vertices[i].pos=Vector.Add(vertices[i].pos,Vector.Random(fact));
			regenerate();
		}
			
		public Vector Min()
		{
			if (numVertices==0) return new Vector(0f,0f,0f);
			var minX=vertices[0].pos.X;
			var minY=vertices[0].pos.Y;
			var minZ=vertices[0].pos.Z;
			for (var i=1; i<numVertices; i++) 
			{
				if(vertices[i].pos.X<minX) minX=vertices[i].pos.X;
				if(vertices[i].pos.Y<minY) minY=vertices[i].pos.Y;
				if(vertices[i].pos.Z<minZ) minZ=vertices[i].pos.Z;
			}
			return new Vector(minX,minY,minZ);
		}
		
		public Vector Max()
		{
			if (numVertices==0) return new Vector(0f,0f,0f);
			var maxX=vertices[0].pos.X;
			var maxY=vertices[0].pos.Y;
			var maxZ=vertices[0].pos.Z;
			for (var i=1; i<numVertices; i++) 
			{
				if(vertices[i].pos.X>maxX) maxX=vertices[i].pos.X;
				if(vertices[i].pos.Y>maxY) maxY=vertices[i].pos.Y;
				if(vertices[i].pos.Z>maxZ) maxZ=vertices[i].pos.Z;
			}
			return new Vector(maxX,maxY,maxZ);
		}
		
		
		public void detach()
		// Centers the object in its coordinate system
		// The offset from origin to object center will be transfered to the matrix,
		// so your object actually does not move.
		// Usefull if you want prepare objects for self rotation.
		{
			var center=getCenter();
			
			for (var i=0;i<numVertices;i++)
			{
				vertices[i].pos.X-=center.X;	
				vertices[i].pos.Y-=center.Y;	
				vertices[i].pos.Z-=center.Z;	
			}
			shift(center);
		}
		
		public Vector getCenter()
		// Returns the center of this object
		{
			var max=Max();
			var min=Min();
			return new Vector((max.X+min.X)/2,(max.Y+min.Y)/2,(max.Z+min.Z)/2);
		}
		
		public Vector getDimension()
		// Returns the x,y,z - Dimension of this object
		{
			var max=Max();
			var min=Min();
			return new Vector(max.X-min.X,max.Y-min.Y,max.Z-min.Z);
		}			
		
		public void matrixMeltdown()
		// Applies the transformations in the matrix to all vertices
		// and resets the matrix to untransformed.
		{
			rebuild();
			for (var i=numVertices-1;i>=0;i--)
				vertices[i].pos=vertices[i].pos.Transform(matrix);
			regenerate();
			matrix.reset();
			normalmatrix.reset();
		}
		
		public SceneObject Clone()
		{
			var obj=new SceneObject();
			rebuild();
			for(var i=0;i<numVertices;i++) obj.addVertex(vertices[i].Clone());
			for(var i=0;i<numTriangles;i++) obj.addTriangle(triangles[i].Clone());
			obj.name=name+" [cloned]";
			obj.material=material;
			obj.matrix=matrix.Clone();
			obj.normalmatrix=normalmatrix.Clone();
			obj.rebuild();
			return obj;
		}
		
		public void removeDuplicateVertices()
		{
			rebuild();
            var edgesToCollapse = new List<Edge>();
			for (var i=0;i<numVertices;i++)
				for (var j=i+1;j<numVertices;j++)
					if (vertices[i].equals(vertices[j],0.0001f))
						edgesToCollapse.Add(new Edge(vertices[i],vertices[j]));
            foreach (var edge in edgesToCollapse)
			    edgeCollapse(edge);
		
			removeDegeneratedTriangles();
		}
		
		public void removeDegeneratedTriangles()
		{
			rebuild();
			for (var i=0;i<numTriangles;i++)
				if (triangles[i].degenerated()) removeTriangleAt(i);
			
			dirty=true;
			rebuild();			
		}
		
		public void meshSmooth()
		{				
			rebuild();
			Triangle tri;
			float u,v;
			Vertex a,b,c,d,e,f,temp;
			Vector ab,bc,ca,nab,nbc,nca,center;
			float sab,sbc,sca,rab,rbc,rca;
			float uab,vab,ubc,vbc,uca,vca;
			var sqrt3=(float)Math.Sqrt(3f);
			
			for (var i=0;i<numTriangles;i++)
			{
				tri=Triangle(i);
				a=tri.p1;
				b=tri.p2;
				c=tri.p3;
				ab=Vector.Scale(0.5f,Vector.Add(b.pos,a.pos));
                bc = Vector.Scale(0.5f, Vector.Add(c.pos, b.pos));
                ca = Vector.Scale(0.5f, Vector.Add(a.pos, c.pos));
				rab=Vector.Subtract(ab,a.pos).Length();
                rbc = Vector.Subtract(bc, b.pos).Length();
                rca = Vector.Subtract(ca, c.pos).Length();
				
				nab=Vector.Scale(0.5f,Vector.Add(a.n,b.n));
                nbc = Vector.Scale(0.5f, Vector.Add(b.n, c.n));
                nca = Vector.Scale(0.5f, Vector.Add(c.n, a.n));
				uab=0.5f*(a.Tu+b.Tu);
				vab=0.5f*(a.Tv+b.Tv);
				ubc=0.5f*(b.Tu+c.Tu);
				vbc=0.5f*(b.Tv+c.Tv);
				uca=0.5f*(c.Tu+a.Tu);
				vca=0.5f*(c.Tv+a.Tv);
				sab=1f-nab.Length();
                sbc = 1f - nbc.Length();
                sca = 1f - nca.Length();
				nab.Normalize();
                nbc.Normalize();
                nca.Normalize();
				
				d=new Vertex(Vector.Subtract(ab,Vector.Scale(rab*sab,nab)),uab,vab);
                e = new Vertex(Vector.Subtract(bc, Vector.Scale(rbc * sbc, nbc)), ubc, vbc);
                f = new Vertex(Vector.Subtract(ca, Vector.Scale(rca * sca, nca)), uca, vca);
				
				addVertex(d);
				addVertex(e);
				addVertex(f);
				tri.p2=d;
				tri.p3=f;
				addTriangle(b,e,d);
				addTriangle(c,f,e);
				addTriangle(d,e,f);
			}
			removeDuplicateVertices();			
		}
		

	// P R I V A T E   M E T H O D S

    void edgeCollapse(Edge edge)
		// Collapses the edge [u,v] by replacing v by u
		{
			var u=edge.start();
			var v=edge.end();
			if (!vertexData.Contains(u)) return;
            if (!vertexData.Contains(v)) return;
			rebuild();
			Triangle tri;
			for (var i=0; i<numTriangles; i++)
			{
				tri=Triangle(i);
				if (tri.p1==v) tri.p1=u;
				if (tri.p2==v) tri.p2=u;
				if (tri.p3==v) tri.p3=u;
			}
			vertexData.Remove(v);
		}
}
}