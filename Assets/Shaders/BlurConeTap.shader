// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/BlurConeTap" {
	Properties {
		_MaskTex ("Base (RGB)", 2D) = "white" {}
	    _GlowStrength ("GlowStrength", Float) = 1.0
        _Color ("Color", Color) = (1, 1, 1, 1)
	}
	CGINCLUDE
	#include "UnityCG.cginc"
	struct v2f {
		float4 pos : POSITION;
		half2 uv : TEXCOORD0;
		half2 taps[4] : TEXCOORD1; 
	};
	sampler2D _MaskTex;
	half4 _TexelSize;
	half4 _BlurOffsets;
	uniform fixed _GlowStrength;
    fixed4 _Color;
	v2f blur(appdata_img v, half4 blurOffsets) {
		v2f o; 
		o.pos = UnityObjectToClipPos(v.vertex);
		//o.uv = v.texcoord - blurOffsets.xy * _TexelSize.xy; // hack, see BlurEffect.cs for the reason for this. let's make a new blur effect soon
		o.uv = v.texcoord;
		o.taps[0] = o.uv + _TexelSize * blurOffsets.xy;
		o.taps[1] = o.uv - _TexelSize * blurOffsets.xy;
		o.taps[2] = o.uv + _TexelSize * blurOffsets.xy * half2(1,-1);
		o.taps[3] = o.uv - _TexelSize * blurOffsets.xy * half2(1,-1);
		return o;
	}
	v2f vert( appdata_img v ) {
		return blur(v, _BlurOffsets);
	}
	v2f vert2( appdata_img v ) {
		return blur(v, _BlurOffsets * half4(-1, -1, 0, 0));
	}
	v2f vert3( appdata_img v ) {
		return blur(v, _BlurOffsets * half4(1, -1, 0, 0));
	}
	v2f vert4( appdata_img v ) {
		return blur(v, _BlurOffsets * half4(-1, 1, 0, 0));
	}
	
	half4 frag(v2f i) : COLOR {
		half4 color = tex2D(_MaskTex, i.taps[0]);
		color += tex2D(_MaskTex, i.taps[1]);
		color += tex2D(_MaskTex, i.taps[2]);
		color += tex2D(_MaskTex, i.taps[3]); 
		return color * _GlowStrength * _Color;
	}
	ENDCG
 
	SubShader {
		Pass {
            Blend One One
	        ZTest Always Cull Off ZWrite Off
	        Fog { Mode off }      

	        CGPROGRAM
		        #pragma fragmentoption ARB_precision_hint_fastest
		        #pragma vertex vert
		        #pragma fragment frag
	        ENDCG
        }
//		Pass {
//            Blend One One
//	        ZTest Always Cull Off ZWrite Off
//	        Fog { Mode off }      
//
//	        CGPROGRAM
//		        #pragma fragmentoption ARB_precision_hint_fastest
//		        #pragma vertex vert2
//		        #pragma fragment frag
//	        ENDCG
//        }
//		Pass {
//            Blend One One
//	        ZTest Always Cull Off ZWrite Off
//	        Fog { Mode off }      
//
//	        CGPROGRAM
//		        #pragma fragmentoption ARB_precision_hint_fastest
//		        #pragma vertex vert3
//		        #pragma fragment frag
//	        ENDCG
//        }
//		Pass {
//            Blend One One
//	        ZTest Always Cull Off ZWrite Off
//	        Fog { Mode off }      
//
//	        CGPROGRAM
//		        #pragma fragmentoption ARB_precision_hint_fastest
//		        #pragma vertex vert4
//		        #pragma fragment frag
//	        ENDCG
//        }
	} 
	FallBack off 
}
