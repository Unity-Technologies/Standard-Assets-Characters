Shader "Hidden/ProBuilder/HideVertices"
{
	SubShader
	{
		Tags { "IgnoreProjector"="True" "RenderType"="Geometry" }
		Lighting Off
		ZTest On
		ZWrite On
		Cull Back

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

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
				o.pos = fixed4(0,0,0,0);

				return o;
			}

			half4 frag (v2f i) : COLOR
			{
				return fixed4(0,0,0,0);
			}

			ENDCG
		}
	}
}
