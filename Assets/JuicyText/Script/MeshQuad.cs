using UnityEngine;

namespace Com.GitHub.Knose1.JuicyText
{
	public struct MeshQuad
	{
		public UIVertex a;
		public UIVertex b;
		public UIVertex c;
		public UIVertex d;

		public Color color
		{
			get => a.color;
			set
			{
				a.color = value;
				b.color = value;
				c.color = value;
				d.color = value;
			}
		}



		public MeshQuad(MeshQuad copy)
		{
			a = copy.a;
			b = copy.b;
			c = copy.c;
			d = copy.d;
		}

		public UIVertex this[int index]
		{
			get
			{
				switch (index)
				{
					case 0: return a;
					case 1: return b;
					case 2: return c;
					case 3: return d;
					default:
						Debug.LogWarning("Index out of bounds");
						return d;
				}
			}

			set
			{
				switch (index)
				{
					case 0: a = value;
						break;
					case 1: b = value;
						break;
					case 2: c = value;
						break;
					case 3: d = value;
						break;
					default:
						Debug.LogWarning("Index out of bounds");
						break;
				}
			}
		}

		public Vector3 Position
		{
			get => Center();
			set
			{
				Translate(value-Position);
			}
		}

		/*----------------------------------------------*/
		/*-          Translate, Rotate, Scale          -*/
		/*----------------------------------------------*/
		public void Translate(Vector3 value, int from = 0, int to = -1)
		{
			if (to == -1) to = 3;

			//- Deplacez le triangle sur les trois axes
			for (int i = to; i >= from; i--)
			{
				UIVertex uIVertex = this[i];
				uIVertex.position = uIVertex.position + value;
				this[i] = uIVertex;
			}
		}

		public void Scale(Vector3 value, int from = 0, int to = -1)
		{
			if (to == -1) to = 3;

			//- Redimensionner le triangle sur les trois axes
			Matrix4x4 scale = Matrix4x4.Scale(value);
			for (int i = to; i >= from; i--)
			{
				//vertices[i] = scale.MultiplyPoint(vertices[i]);
				UIVertex uIVertex = this[i];
				Vector3 vertice = uIVertex.position;
				uIVertex.position = new Vector3(vertice.x * value.x, vertice.y * value.y, vertice.z * value.z);
				this[i] = uIVertex;
			}

		}
		public void Scale(Vector3 value, Vector3 scaleOrigine, int from = 0, int to = -1)
		{
			Translate(-scaleOrigine, from, to);
			Scale(value, from, to);
			Translate(scaleOrigine, from, to);
		}

		public void Rotate(Quaternion value, int from = 0, int to = -1)
		{
			if (to == -1) to = 3;

			//- Redimensionner le triangle sur les trois axes
			Matrix4x4 rot = Matrix4x4.Rotate(value);
			for (int i = to; i >= from; i--)
			{
				//vertices[i] = rot.MultiplyPoint(vertices[i]);

				UIVertex uIVertex = this[i];
				Vector3 vertice = uIVertex.position;
				uIVertex.position = value * vertice;
				this[i] = uIVertex;
			}

		}
		public void Rotate(Quaternion value, Vector3 rotationOrigine, int from = 0, int to = -1)
		{
			Translate(-rotationOrigine, from, to);
			Rotate(value, from, to);
			Translate(rotationOrigine, from, to);
		}
		
		
		public void VectorSymetry(Vector3 directorVector, int from = 0, int to = -1)
		{
			Rotate(Quaternion.AngleAxis(180, directorVector), from, to);
		}
		public void VectorSymetry(Vector3 directorVector, Vector3 symetryOrigine, int from = 0, int to = -1)
		{
			Rotate(Quaternion.AngleAxis(180, directorVector), symetryOrigine, from, to);
		}
		
		public Vector3 Center()
		{
			Vector3 toReturn = default;
			for (int i = 3; i >= 0; i--)
			{
				toReturn += this[i].position;
			}

			return toReturn / 4;
		}

		public MeshQuad Copy() => new MeshQuad(this);
		public static implicit operator UIVertex[](MeshQuad m) => new UIVertex[] { m.a, m.b, m.c, m.d };
	}
}
