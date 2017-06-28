// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/GlowMask" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_TileX ("X Tiling", Float) = 1
		_TileY ("Y Tiling", Float) = 1
		_GlowStrength ("Glow Intesity", Range(0.0, 1)) = 1
		_GlowRange ("Glow Range", float) = 100
	}
	SubShader {
        Blend SrcAlpha One
        Tags { "Queue"="Geometry" "IgnoreProjector"="True" "RenderType"="Opaque"}
        Lighting Off
        Cull Off
        Fog { Mode Off }
        Offset 0, -10
        ZWrite Off
		
		Pass {
			Tags {"LightMode"="Always"}
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag 
				#pragma target 3.0
				#include "UnityCG.cginc"

				sampler2D _MainTex;
				half _TileX;
				half _TileY;
				half _GlowStrength;
				half _GlowRange;
			
				struct appdata {
	    			half4 vertex : POSITION;
	    			half4 texcoord : TEXCOORD0;
	    			half4 projPos : TEXCOORD1;
				};
	 
				struct v2f {
	    			half4 pos : SV_POSITION;
	    			half4 uv : TEXCOORD0;
	    			half4 projPos : TEXCOORD1;
				};
				
				v2f vert (appdata v) {
	    			v2f o;
	    			o.pos = UnityObjectToClipPos( v.vertex );
	    			o.uv = half4( v.texcoord.xy, 0, 0 );
	    			o.projPos = ComputeScreenPos (v.vertex);
	    			UNITY_TRANSFER_DEPTH(o.projPos.z);
	    			//COMPUTE_EYEDEPTH(o.projPos.z);
	    			return o;
				}
	    		
	    		half4 frag( v2f v ) : COLOR {
					half2 tiling = half2(_TileX, _TileY);
					half4 result = tex2D(_MainTex, v.uv.xy * tiling.xy );
					half depth = v.projPos.z / _GlowRange;
					depth = 1.0 - depth;
					result.rgb *= _GlowStrength;
					result.a = depth;
					return result;
				}
			ENDCG
		}
	} 
	FallBack off
}
