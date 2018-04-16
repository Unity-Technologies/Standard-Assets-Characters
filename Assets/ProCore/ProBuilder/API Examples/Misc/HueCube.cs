#if UNITY_EDITOR || UNITY_STANDALONE
using UnityEngine;
using System.Collections;
using ProBuilder2.Common;
using System.Collections.Generic;

namespace ProBuilder2.Examples
{
	/**
	 * Creates a cube on start and colors it's vertices programmitically.
	 */
	public class HueCube : MonoBehaviour
	{
		pb_Object pb;

		void Start()
		{
			// Create a new ProBuilder cube to work with.
			pb = pb_ShapeGenerator.CubeGenerator(Vector3.one);

			// Cycle through each unique vertex in the cube (8 total), and assign a color
			// to the index in the sharedIndices array.
			int si_len = pb.sharedIndices.Length;
			Color[] vertexColors = new Color[si_len];
			for(int i = 0; i < si_len; i++)
			{
				vertexColors[i] = HSVtoRGB( (i/(float)si_len) * 360f, 1f, 1f);
			}

			// Now go through each face (vertex colors are stored the pb_Face class) and
			// assign the pre-calculated index color to each index in the triangles array.
			Color[] colors = pb.colors;

			for(int CurSharedIndex = 0; CurSharedIndex < pb.sharedIndices.Length; CurSharedIndex++)
			{
				foreach(int CurIndex in pb.sharedIndices[CurSharedIndex].array)
				{
					colors[CurIndex] = vertexColors[CurSharedIndex];
				}
			}

			pb.SetColors(colors);

			// In order for these changes to take effect, you must refresh the mesh
			// object.
			pb.Refresh();
		}

		/**
		 * Convert HSV to RGB.
		 *  http://www.cs.rit.edu/~ncs/color/t_convert.html
		 *	r,g,b values are from 0 to 1
		 *	h = [0,360], s = [0,1], v = [0,1]
		 *	if s == 0, then h = -1 (undefined)
		 */
		static Color HSVtoRGB(float h, float s, float v )
		{
			float r, g, b;
			int i;
			float f, p, q, t;
			if( s == 0 ) {
				// achromatic (grey)
				return new Color(v, v, v, 1f);
			}
			h /= 60;			// sector 0 to 5
			i = (int)Mathf.Floor( h );
			f = h - i;			// factorial part of h
			p = v * ( 1 - s );
			q = v * ( 1 - s * f );
			t = v * ( 1 - s * ( 1 - f ) );

			switch( i )
			{
				case 0:
					r = v;
					g = t;
					b = p;
					break;
				case 1:
					r = q;
					g = v;
					b = p;
					break;
				case 2:
					r = p;
					g = v;
					b = t;
					break;
				case 3:
					r = p;
					g = q;
					b = v;
					break;
				case 4:
					r = t;
					g = p;
					b = v;
					break;
				default:		// case 5:
					r = v;
					g = p;
					b = q;
					break;
			}
			
			return new Color(r, g, b, 1f);
		}
	}
}
#endif
