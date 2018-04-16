Shader "Hidden/ProBuilder/SelectionPicker"
{
	Properties {}

	SubShader
	{
		Tags { "ProBuilderPicker"="VertexPass" }
		Lighting Off
		ZTest LEqual
		ZWrite On
		Cull Off
		Blend Off

		UsePass "Hidden/ProBuilder/VertexPicker/VERTICES"
	}

	SubShader
	{
		Tags { "ProBuilderPicker"="Base" }
		Lighting Off
		ZTest LEqual
		ZWrite On
		Cull Back
		Blend Off

		UsePass "Hidden/ProBuilder/FacePicker/BASE"
	}
}
