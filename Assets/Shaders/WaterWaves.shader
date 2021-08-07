Shader "Custom/WaterWaves" {
    Properties {
        _Color ("Color", Color) = (1,1,1,1)
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0

        _WaveLength("Wave length", Range(2, 50)) = 5.0
        _WaveHeight("Wave height", Range(0, 50)) = 2.0
        _WaveSpeed("Wave speed", Range(0, 50)) = 1.0
        // _RandomHeight("Random height", Range(0, 25)) = 0.5
        // _RandomSpeed("Random Speed", Range(0, 25)) = 0.5
    }

    SubShader {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        // Tags { "RenderType"="Opaque" }

        LOD 200
        Cull Off

        CGPROGRAM
        #pragma surface surf Standard alpha fullforwardshadows vertex:vert
        // #pragma surface surf Standard fullforwardshadows vertex:vert
        #pragma target 3.0 // Use shader model 3.0 target, to get nicer looking lighting

        struct Input {
            float3 worldPos;
            float3 worldNormal;
            INTERNAL_DATA
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        float _WaveLength;
        float _WaveHeight;
        float _WaveSpeed;
        float _RandomHeight;
        float _RandomSpeed;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        // https://www.shadertoy.com/view/4djSRW
        // float hash13(float3 p3)
        // {
        //     p3  = frac(p3 * .1031);
        //     p3 += dot(p3, p3.zyx + 31.32);
        //     return frac((p3.x + p3.y) * p3.z) * 2 - 1;
        // }


        void vert(inout appdata_full v, out Input o) {
            UNITY_INITIALIZE_OUTPUT(Input, o);

            float4 v0 = mul(unity_ObjectToWorld, v.vertex);

            // Calculate Gerstner wave movements 
            float p = (v0.x + v0.z) / 16;
            half k = 2 * UNITY_PI / _WaveLength;
            float f = k * (p - _WaveSpeed * _Time.y);
            v0.x += _WaveHeight * cos(f);
            v0.y += _WaveHeight * sin(f);

            v.vertex = mul(unity_WorldToObject, v0);

            o.worldPos = v0;
        }

        void surf (Input i, inout SurfaceOutputStandard o) {
            // Albedo comes from a texture tinted by color
            fixed4 c = _Color;
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;

            // Construct world to tangent matrix
            half3 worldT = WorldNormalVector(i, half3(1,0,0));
            half3 worldB = WorldNormalVector(i, half3(0,1,0));
            half3 worldN = WorldNormalVector(i, half3(0,0,1));
            half3x3 tbn = half3x3(worldT, worldB, worldN);

            half3 worldNormal = -Unity_SafeNormalize(cross(ddx(i.worldPos), ddy(i.worldPos)));
            o.Normal = mul(tbn, worldNormal);
        }

        ENDCG
    }
    FallBack "Diffuse"
}

