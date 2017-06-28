Shader "Custom/MyLambertGlow2" {
    Properties {
        _MainTex("Base (RGB)", 2D) = "white" {}
        _Color("Main Color", Color) = (1, 1, 1, 1)
    }   
    SubShader {
		Tags { "RenderEffect"="Glow2" "RenderType"="Glow2" }
        LOD 200 

        CGPROGRAM
            #pragma surface surf MyLambert

            sampler2D _MainTex;
            fixed4 _Color;
            
            struct Input {
                float2 uv_MainTex;
            };  

            inline fixed4 LightingMyLambert(SurfaceOutput s, fixed3 lightDir, fixed atten) {
                fixed diff = max(0, dot(s.Normal, lightDir));
                
                fixed4 c;
                c.rgb = s.Albedo * _LightColor0.rgb * (diff * atten * 2); 
                c.a = s.Alpha;
                return c;
            }   

            void surf(Input IN, inout SurfaceOutput o) {
                _Color *= tex2D(_MainTex, IN.uv_MainTex);
            
                o.Albedo = _Color.rgb;
                o.Alpha = 1.0;
            }   
        ENDCG
    }   

    FallBack "Diffuse"
} 