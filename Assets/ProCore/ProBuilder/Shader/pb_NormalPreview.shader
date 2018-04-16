Shader "Hidden/ProBuilder/NormalPreview"
{
	SubShader
	{
		Tags { "Queue"="AlphaTest" "IgnoreProjector"="True" "RenderType"="Geometry" }
		Lighting Off
		ZTest LEqual
		Blend SrcAlpha OneMinusSrcAlpha
		ZWrite On
		Cull Off

		Pass
		{
			AlphaTest Greater .25

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			float _Scale;

			struct appdata
			{
				float4 vertex : POSITION;
				// secretely this is the normal
				float4 tangent : TANGENT;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				float4 color : COLOR;
			};

			v2f vert (appdata v)
			{
				v2f o;

				float4 world = mul(unity_ObjectToWorld, v.vertex);
				float3 nrm = UnityObjectToWorldNormal(v.tangent.xyz);
				float4 extruded = world + float4((nrm * v.tangent.w * _Scale), 0);
				o.pos = mul(UNITY_MATRIX_VP, extruded);
				o.color = float4(abs(v.tangent.xyz), 1);

				return o;
			}

			half4 frag (v2f i) : COLOR
			{
				return i.color;
			}

			ENDCG
		}
	}
}
