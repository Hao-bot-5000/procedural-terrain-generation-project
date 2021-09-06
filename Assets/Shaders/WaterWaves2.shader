Shader "Custom/Water Waves 2" {
    Properties {
        _NoiseTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0

        _WaveLength("Wave length", Range(2, 50)) = 5.0
        _WaveHeight("Wave height", Range(0, 50)) = 2.0
        _WaveSpeed("Wave speed", Range(0, 50)) = 1.0
    }

    CGINCLUDE
        #include "UnityCG.cginc"

        float _WaveLength;
        float _WaveHeight;
        float _WaveSpeed;

        sampler2D _NoiseTex;

        float apply_noise(float4 v0) {
            return (tex2Dlod(_NoiseTex, float4(v0.xz + _Time.xz, 0, 0) / 512) * 2 - 1) * 4;
        }

        float4 displace_vert(float4 v0) {
            // Calculate Gerstner wave movements 
            float p = (v0.x + v0.z) / 16;
            half k = 2 * UNITY_PI / _WaveLength;
            float f = k * (p - _WaveSpeed * _Time.y);
            v0.x += _WaveHeight * cos(f);
            v0.y += _WaveHeight * sin(f) + apply_noise(v0);

            return v0;
        }

    ENDCG

   SubShader {
        Tags { "Queue"="Transparent" "IgnoreProjector"="true" "RenderType"="Transparent" }
        Cull Off

        // Pass {
		    Stencil {
		        Ref 2
		        Comp NotEqual
		        Pass Replace
		    }

		    Blend SrcAlpha OneMinusSrcAlpha     
	 
			// CGPROGRAM
			// #pragma vertex vert
			// #pragma fragment frag
			// #include "UnityCG.cginc"
			
			// fixed4 _Color;
            // // half _Glossiness;
            // // half _Metallic;
			 
			// float4 vert(appdata_base v) : POSITION {
			//     float4 v0 = displace_vert(mul(unity_ObjectToWorld, v.vertex));
            //     v.vertex = mul(unity_WorldToObject, v0);
            //     return UnityObjectToClipPos(v.vertex);
			// }

			// half4 frag () : COLOR {
            //     half4 color = _Color;
            //     return color;
            // }
			// ENDCG

            CGPROGRAM
            #pragma surface surf Standard alpha vertex:vert
            #pragma target 3.0 // Use shader model 3.0 target, to get nicer looking lighting
            #include "UnityCG.cginc"

            struct Input {
                float3 worldPos;
                float3 worldNormal;
                INTERNAL_DATA
            };

            fixed4 _Color;
            half _Glossiness;
            half _Metallic;

            // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
            // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
            // #pragma instancing_options assumeuniformscaling
            UNITY_INSTANCING_BUFFER_START(Props)
                // put more per-instance properties here
            UNITY_INSTANCING_BUFFER_END(Props)

            void vert(inout appdata_full v, out Input o) {
                UNITY_INITIALIZE_OUTPUT(Input, o);

                float4 v0 = displace_vert(mul(unity_ObjectToWorld, v.vertex));
                v.vertex = mul(unity_WorldToObject, v0);

                o.worldPos = v0;
            }

            void surf(Input i, inout SurfaceOutputStandard o) {
                fixed4 c = _Color;
                o.Albedo = c.rgb; // lerp(half3(1.0, 1.0, 1.0), c.rgb, c.a);
                o.Metallic = _Metallic;
                o.Smoothness = _Glossiness;
                o.Alpha = c.a;

                // TODO: Create two passes to calculate front face normal and back face normal
                // Construct world to tangent matrix
                // half3 worldT = WorldNormalVector(i, half3(1,0,0));
                // half3 worldB = WorldNormalVector(i, half3(0,1,0));
                // half3 worldN = WorldNormalVector(i, half3(0,0,1));
                // half3x3 tbn = half3x3(worldT, worldB, worldN);

                // half3 worldNormal = -Unity_SafeNormalize(cross(ddx(i.worldPos), ddy(i.worldPos)));
                // o.Normal = mul(tbn, worldNormal);
            }

            ENDCG
		// }
    }
}
