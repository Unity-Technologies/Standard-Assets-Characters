// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Shader created with Shader Forge v1.32
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.32;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,lico:1,lgpr:1,limd:0,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:2,bsrc:0,bdst:1,dpts:6,wrdp:True,dith:0,rfrpo:False,rfrpn:Refraction,coma:15,ufog:False,aust:False,igpj:False,qofs:0,qpre:1,rntp:5,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False;n:type:ShaderForge.SFN_Final,id:3138,x:33161,y:32544,varname:node_3138,prsc:2|emission-7953-OUT;n:type:ShaderForge.SFN_Color,id:7241,x:32343,y:32483,ptovrint:False,ptlb:Highlight,ptin:_Highlight,varname:node_7241,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.07843138,c2:0.3921569,c3:0.7843137,c4:1;n:type:ShaderForge.SFN_TexCoord,id:1076,x:32355,y:32830,varname:node_1076,prsc:2,uv:0;n:type:ShaderForge.SFN_Lerp,id:7953,x:32889,y:32503,varname:node_7953,prsc:2|A-7241-RGB,B-1009-RGB,T-3159-OUT;n:type:ShaderForge.SFN_Color,id:1009,x:32343,y:32656,ptovrint:False,ptlb:Base,ptin:_Base,varname:node_1009,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.5,c2:0.5,c3:0.5,c4:1;n:type:ShaderForge.SFN_Multiply,id:6573,x:32673,y:33080,varname:node_6573,prsc:2|A-9717-OUT,B-9741-OUT;n:type:ShaderForge.SFN_Slider,id:9741,x:32275,y:33210,ptovrint:False,ptlb:Speed,ptin:_Speed,varname:node_9741,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0.1,max:1;n:type:ShaderForge.SFN_Frac,id:6472,x:32865,y:33050,varname:node_6472,prsc:2|IN-6573-OUT;n:type:ShaderForge.SFN_Distance,id:7233,x:32904,y:32864,varname:node_7233,prsc:2|A-6472-OUT,B-7546-OUT;n:type:ShaderForge.SFN_Smoothstep,id:3159,x:32856,y:32677,varname:node_3159,prsc:2|A-3575-OUT,B-4893-OUT,V-7233-OUT;n:type:ShaderForge.SFN_Vector1,id:3575,x:32608,y:32677,varname:node_3575,prsc:2,v1:0;n:type:ShaderForge.SFN_Vector1,id:4893,x:32608,y:32738,varname:node_4893,prsc:2,v1:0.2;n:type:ShaderForge.SFN_ValueProperty,id:9717,x:32387,y:33067,ptovrint:False,ptlb:EditorTime,ptin:_EditorTime,varname:node_9717,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_ValueProperty,id:6146,x:32486,y:32982,ptovrint:False,ptlb:LineDistance,ptin:_LineDistance,varname:node_6146,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:18;n:type:ShaderForge.SFN_Divide,id:7546,x:32700,y:32889,varname:node_7546,prsc:2|A-1076-U,B-6146-OUT;proporder:7241-1009-9741-9717-6146;pass:END;sub:END;*/

Shader "Hidden/ProBuilder/ScrollHighlight" {
    Properties {
        _Highlight ("Highlight", Color) = (0.07843138,0.3921569,0.7843137,1)
        _Base ("Base", Color) = (0.5,0.5,0.5,1)
        _Speed ("Speed", Range(0, 1)) = 0.5
        _EditorTime ("EditorTime", Float ) = 0
        _LineDistance ("LineDistance", Float ) = 18
    }
    SubShader {
        Tags {
            "RenderType"="Overlay"
        }
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            Cull Off
            ZTest Always


            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase_fullshadows
            #pragma target 3.0
            uniform float4 _Highlight;
            uniform float4 _Base;
            uniform float _Speed;
            uniform float _EditorTime;
            uniform float _LineDistance;
            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.pos = UnityObjectToClipPos(v.vertex );
                return o;
            }
            float4 frag(VertexOutput i, float facing : VFACE) : COLOR {
                float isFrontFace = ( facing >= 0 ? 1 : 0 );
                float faceSign = ( facing >= 0 ? 1 : -1 );
////// Lighting:
////// Emissive:
                float3 emissive = lerp(_Highlight.rgb,_Base.rgb,smoothstep( 0.0, 0.2, distance(frac((_EditorTime*_Speed)),(i.uv0.r/_LineDistance)) ));
                float3 finalColor = emissive;
                return fixed4(finalColor,1);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
