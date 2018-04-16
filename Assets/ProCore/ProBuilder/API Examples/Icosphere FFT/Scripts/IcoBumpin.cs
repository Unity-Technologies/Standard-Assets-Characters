#if UNITY_EDITOR || UNITY_STANDALONE
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ProBuilder2.Common;
using ProBuilder2.MeshOperations;

namespace ProBuilder2.Examples
{

	[RequireComponent(typeof(AudioSource))]
	public class IcoBumpin : MonoBehaviour
	{
		pb_Object ico;			// A reference to the icosphere pb_Object component
		Mesh icoMesh;			// A reference to the icosphere mesh (cached because we access the vertex array every frame)
		Transform icoTransform;	// A reference to the icosphere transform component.  Cached because I can't remember if GameObject.transform is still a performance drain :|
		AudioSource audioSource;// Cached reference to the audiosource.

		/**
		 * Holds a pb_Face, the normal of that face, and the index of every vertex that touches it (sharedIndices).
		 */
		struct FaceRef
		{
			public pb_Face face;
			public Vector3 nrm;		// face normal
			public int[] indices;	// all vertex indices (including shared connected vertices)

			public FaceRef(pb_Face f, Vector3 n, int[] i)
			{
				face = f;
				nrm = n;
				indices = i;
			}
		}

		// All faces that have been extruded
		FaceRef[] outsides;

		// Keep a copy of the original vertex array to calculate the distance from origin.
		Vector3[] original_vertices, displaced_vertices;

		// The radius of the mesh icosphere on instantiation.
		[Range(1f, 10f)]
		public float icoRadius = 2f;

		// The number of subdivisions to give the icosphere.
		[Range(0, 3)]
		public int icoSubdivisions = 2;

		// How far along the normal should each face be extruded when at idle (no audio input).
		[Range(0f, 1f)]
		public float startingExtrusion = .1f;

		// The material to apply to the icosphere.
		public Material material;

		// The max distance a frequency range will extrude a face.
		[Range(1f, 50f)]
		public float extrusion = 30f;

		// An FFT returns a spectrum including frequencies that are out of human hearing range -
		// this restricts the number of bins used from the spectrum to the lower @fftBounds.
		[Range(8, 128)]
		public int fftBounds = 32;

		// How high the icosphere transform will bounce (sample volume determines height).
		[Range(0f, 10f)]
		public float verticalBounce = 4f;

		// Optionally weights the frequency amplitude when calculating extrude distance.
		public AnimationCurve frequencyCurve;

		// A reference to the line renderer that will be used to render the raw waveform.
		public LineRenderer waveform;

		// The y size of the waveform.
		public float waveformHeight	= 2f;

		// How far from the icosphere should the waveform be.
		public float waveformRadius	= 20f;

		// If @rotateWaveformRing is true, this is the speed it will travel.
		public float waveformSpeed = .1f;

		// If true, the waveform ring will randomly orbit the icosphere.
		public bool rotateWaveformRing = false;

		// If true, the waveform will bounce up and down with the icosphere.
		public bool bounceWaveform = false;

		public GameObject missingClipWarning;

		// Icosphere's starting position.
		Vector3 icoPosition = Vector3.zero;
		float faces_length;

		const float TWOPI = 6.283185f;		// 2 * PI
		const int WAVEFORM_SAMPLES = 1024;	// How many samples make up the waveform ring.
		const int FFT_SAMPLES = 4096;		// How many samples are used in the FFT.  More means higher resolution.

		// Keep copy of the last frame's sample data to average with the current when calculating
		// deformation amounts.  Smoothes the visual effect.
		float[] fft = new float[FFT_SAMPLES],
				fft_history = new float[FFT_SAMPLES],
				data = new float[WAVEFORM_SAMPLES],
				data_history = new float[WAVEFORM_SAMPLES];

		// Root mean square of raw data (volume, but not in dB).
		float rms = 0f, rms_history = 0f;

		/**
		 * Creates the icosphere, and loads all the cache information.
		 */
		void Start()
		{
			audioSource = GetComponent<AudioSource>();

			if( audioSource.clip == null )
				missingClipWarning.SetActive(true);

			// Create a new icosphere.
			ico = pb_ShapeGenerator.IcosahedronGenerator(icoRadius, icoSubdivisions);

			// Shell is all the faces on the new icosphere.
			pb_Face[] shell = ico.faces;

			// Materials are set per-face on pb_Object meshes.  pb_Objects will automatically
			// condense the mesh to the smallest set of subMeshes possible based on materials.
#if !PROTOTYPE
			foreach(pb_Face f in shell)
				f.material = material;
#else
			ico.gameObject.GetComponent<MeshRenderer>().sharedMaterial = material;
#endif

			// Extrude all faces on the icosphere by a small amount.  The third boolean parameter
			// specifies that extrusion should treat each face as an individual, not try to group
			// all faces together.
			ico.Extrude(shell, ExtrudeMethod.IndividualFaces, startingExtrusion);

			// ToMesh builds the mesh positions, submesh, and triangle arrays.  Call after adding
			// or deleting vertices, or changing face properties.
			ico.ToMesh();

			// Refresh builds the normals, tangents, and UVs.
			ico.Refresh();

			outsides = new FaceRef[shell.Length];
			Dictionary<int, int> lookup = ico.sharedIndices.ToDictionary();

			// Populate the outsides[] cache.  This is a reference to the tops of each extruded column, including
			// copies of the sharedIndices.
			for(int i = 0; i < shell.Length; ++i)
				outsides[i] = new FaceRef( 	shell[i],
											pb_Math.Normal(ico, shell[i]),
											ico.sharedIndices.AllIndicesWithValues(lookup, shell[i].distinctIndices).ToArray()
											);

			// Store copy of positions array un-modified
			original_vertices = new Vector3[ico.vertices.Length];
			System.Array.Copy(ico.vertices, original_vertices, ico.vertices.Length);

			// displaced_vertices should mirror icosphere mesh vertices.
			displaced_vertices = ico.vertices;

			icoMesh = ico.msh;
			icoTransform = ico.transform;

			faces_length = (float)outsides.Length;

			// Build the waveform ring.
			icoPosition = icoTransform.position;
#if UNITY_4_5 || UNITY_4_6 || UNITY_4_7 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2 || UNITY_5_3 || UNITY_5_4
			waveform.SetVertexCount(WAVEFORM_SAMPLES);
#elif UNITY_5_5
			waveform.numPositions = WAVEFORM_SAMPLES;
#else
			waveform.positionCount = WAVEFORM_SAMPLES;
#endif


			if( bounceWaveform )
				waveform.transform.parent = icoTransform;

			audioSource.Play();
		}

		void Update()
		{
			// fetch the fft spectrum
			audioSource.GetSpectrumData(fft, 0, FFTWindow.BlackmanHarris);

			// get raw data for waveform
			audioSource.GetOutputData(data, 0);

			// calculate root mean square (volume)
			rms = RMS(data);

			/**
			 * For each face, translate the vertices some distance depending on the frequency range assigned.
			 * Not using the TranslateVertices() pb_Object extension method because as a convenience, that method
			 * gathers the sharedIndices per-face on every call, which while not tremondously expensive in most
			 * contexts, is far too slow for use when dealing with audio, and especially so when the mesh is
			 * somewhat large.
			 */
			for(int i = 0; i < outsides.Length; i++)
			{
				float normalizedIndex = (i/faces_length);

				int n = (int)(normalizedIndex*fftBounds);

				Vector3 displacement = outsides[i].nrm * ( ((fft[n]+fft_history[n]) * .5f) * (frequencyCurve.Evaluate(normalizedIndex) * .5f + .5f)) * extrusion;

				foreach(int t in outsides[i].indices)
				{
					displaced_vertices[t] = original_vertices[t] + displacement;
				}
			}

			Vector3 vec = Vector3.zero;

			// Waveform ring
			for(int i = 0; i < WAVEFORM_SAMPLES; i++)
			{
				int n = i < WAVEFORM_SAMPLES-1 ? i : 0;
				vec.x = Mathf.Cos((float)n/WAVEFORM_SAMPLES * TWOPI) * (waveformRadius + (((data[n] + data_history[n]) * .5f) * waveformHeight));
				vec.z = Mathf.Sin((float)n/WAVEFORM_SAMPLES * TWOPI) * (waveformRadius + (((data[n] + data_history[n]) * .5f) * waveformHeight));

				vec.y = 0f;

				waveform.SetPosition(i, vec);
			}

			// Ring rotation
			if( rotateWaveformRing )
			{
				Vector3 rot = waveform.transform.localRotation.eulerAngles;

				rot.x = Mathf.PerlinNoise(Time.time * waveformSpeed, 0f) * 360f;
				rot.y = Mathf.PerlinNoise(0f, Time.time * waveformSpeed) * 360f;

				waveform.transform.localRotation = Quaternion.Euler(rot);
			}

			icoPosition.y = -verticalBounce + ((rms + rms_history) * verticalBounce);
			icoTransform.position = icoPosition;

			// Keep copy of last FFT samples so we can average with the current.  Smoothes the movement.
			System.Array.Copy(fft, fft_history, FFT_SAMPLES);
			System.Array.Copy(data, data_history, WAVEFORM_SAMPLES);
			rms_history = rms;

			icoMesh.vertices = displaced_vertices;
		}

		/**
		 * Root mean square is a good approximation of perceived loudness.
		 */
		float RMS(float[] arr)
		{
			float 	v = 0f,
					len = (float)arr.Length;

			for(int i = 0; i < len; i++)
				v += Mathf.Abs(arr[i]);

			return Mathf.Sqrt(v / (float)len);
		}
	}
}
#endif
