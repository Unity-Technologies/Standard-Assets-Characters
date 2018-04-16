Shader "Hidden/ProBuilder/VertexPicker"
{
	Properties {}

	SubShader
	{
		Tags
		{
			"ProBuilderPicker"="VertexPass"
			"RenderType"="Transparent"
			"IgnoreProjector"="True"
			"DisableBatching"="True"
		}

		Lighting Off
		ZTest LEqual
		ZWrite On
		Cull Off
		Blend Off
		Offset -1, -1

		Pass
		{
			Name "Vertices"
			AlphaTest Greater .25

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float4 color : COLOR;
				float2 texcoord : TEXCOORD0;
				float2 texcoord1 : TEXCOORD1;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float4 color : COLOR;
			};

			v2f vert (appdata v)
			{
				v2f o;

				o.pos = float4(UnityObjectToViewPos(v.vertex.xyz), 1);
				o.pos.xyz *= .95;
				o.pos = mul(UNITY_MATRIX_P, o.pos);

				// convert vertex to screen space, add pixel-unit xy to vertex, then transform back to clip space.
				float4 clip = o.pos;

				clip.xy /= clip.w;
				clip.xy = clip.xy * .5 + .5;
				clip.xy *= _ScreenParams.xy;

				clip.xy += v.texcoord1.xy * 3.5;
				clip.z -= .0001 * (1 - UNITY_MATRIX_P[3][3]);

				clip.xy /= _ScreenParams.xy;
				clip.xy = (clip.xy - .5) / .5;
				clip.xy *= clip.w;

				o.pos = clip;
				o.uv = v.texcoord.xy;
				o.color = v.color;

				return o;
			}

			float4 frag (v2f i) : COLOR
			{
				return i.color;
			}

			ENDCG
		}
	}
}
