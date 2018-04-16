Shader "ProBuilder/Diffuse Texture Blend"
{
	Properties
	{
		_FirstTex ("Texture", 2D) = "white" {}
		_SecondTex ("Texture", 2D) = "white" {}
		_ThirdTex ("Texture", 2D) = "white" {}
		_FourthTex ("Texture", 2D) = "white" {}
	}

	SubShader
	{
		Tags { "RenderType" = "Opaque" }

		ColorMask RGB

		CGPROGRAM
		#pragma surface surf Lambert

		sampler2D _FirstTex;
		sampler2D _SecondTex;
		sampler2D _ThirdTex;
		sampler2D _FourthTex;

		struct Input
		{
			float4 color : COLOR;
			float2 uv_FirstTex;
			float2 uv_SecondTex;
			float2 uv_ThirdTex;
			float2 uv_FourthTex;
		};

		void surf (Input IN, inout SurfaceOutput o)
		{
			fixed4 c0 = tex2D(_FirstTex, IN.uv_FirstTex);
			fixed4 c1 = tex2D(_SecondTex, IN.uv_SecondTex);
			fixed4 c2 = tex2D(_ThirdTex, IN.uv_ThirdTex);
			fixed4 c3 = tex2D(_FourthTex, IN.uv_FourthTex);

			fixed4 blend = normalize(IN.color);

			fixed4 rgba = c0 * blend.r;
			rgba = lerp(rgba, c1, blend.g);
			rgba = lerp(rgba, c2, blend.b);
			rgba = lerp(rgba, c3, blend.a);

			o.Albedo = rgba.rgb;
		}
		ENDCG
	}

	Fallback "Diffuse"
}
