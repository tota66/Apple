Shader "Custom/Glow2Illumin" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_Emission ("Emissive Color", Color) = (0,0,0,0)
		_Shininess ("Shininess", Range (0.1, 1)) = 0.7
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
	    _GlowTex ("Glow", 2D) = "" {}
	    _GlowColor ("Glow Color", Color)  = (1,1,1,1)
	    _GlowStrength ("Glow Strength", Float) = 1.0
	}
	SubShader {
		Tags { "Queue" = "Transparent" "RenderEffect"="Glow2" "RenderType"="Glow2" }
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows alpha

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;
		fixed4 _GlowColor;
		fixed4 _Emission;
		

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			//o.Emission = _Emission;
			//o.Shininess = _Shininess;
			o.Alpha = c.a;
		}
		ENDCG
	} 
	FallBack "VertexLit"
}
