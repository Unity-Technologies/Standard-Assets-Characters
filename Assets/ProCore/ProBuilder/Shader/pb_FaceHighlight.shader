Shader "Hidden/ProBuilder/FaceHighlight"
{
	Properties
	{
		_Color ("Color Tint", Color) = (1,1,1,1)
	}

	SubShader
	{
		Tags { "IgnoreProjector"="True" "RenderType"="Geometry" }
		Lighting Off
		ZTest LEqual
		ZWrite On
		Cull Back
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			AlphaTest Greater .25

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			float4 _Color;

			struct appdata
			{
				float4 vertex : POSITION;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
			};

			v2f vert (appdata v)
			{
				v2f o;

				/// https://www.opengl.org/discussion_boards/showthread.php/166719-Clean-Wireframe-Over-Solid-Mesh
				o.pos = float4(UnityObjectToViewPos(v.vertex.xyz), 1);
				o.pos.xyz *= .99;
				o.pos = mul(UNITY_MATRIX_P, o.pos);

				return o;
			}

			half4 frag (v2f i) : COLOR
			{
				return _Color;
			}

			ENDCG
		}
	}
}
