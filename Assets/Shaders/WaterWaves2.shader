// NOTE: based on code from -> https://forum.unity.com/threads/accessing-depth-buffer-from-a-surface-shader.404380/#post-2640297

Shader "Custom/Water Waves v2" {
    Properties {
        _Color("Water Color", Color) = (1, 1, 1, 1)
        _Foam("Foam Color", Color) = (1, 1, 1, 1)
        _Glossiness("Smoothness", Range(0, 1)) = 0.0
        _Metallic("Metallic", Range(0, 1)) = 0.0
        _InvFade("Soft Factor", Range(0.01, 3.0)) = 0.05

        _WaveLength("Wave Length", Range(1, 50)) = 12.0
        _WaveHeight("Wave Height", Range(1, 50)) = 1.0
        _WaveSpeed("Wave Speed", Range(1, 50)) = 1.0

        [Space(10)]
        _NoiseTex("Noise Texture", 2D) = "white" {}
        _NoiseScale("Noise Scale", Range(1, 50)) = 2.0
        _NoiseHeight("Noise Height", Range(1, 50)) = 2.0
    }

    CGINCLUDE
        #include "UnityCG.cginc"

        float _WaveLength;
        float _WaveHeight;
        float _WaveSpeed;

        sampler2D _NoiseTex;
        float4 _NoiseTex_TexelSize;
        float _NoiseScale;
        float _NoiseHeight;

        float apply_noise(float4 v0) {
            return (tex2Dlod(_NoiseTex, float4((v0.xz + _Time.xz) / (_NoiseTex_TexelSize.z * _NoiseScale), 0, 0)) * 2 - 1) * _NoiseHeight;
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

    // FIXME: Looking through the backside of one mesh removes 'foam' on the frontside of another mesh -- http://prntscr.com/1rli8ni
    SubShader {
        Tags { "Queue"="Transparent" "IgnoreProjector"="true" "RenderType"="Transparent" }

        Blend SrcAlpha OneMinusSrcAlpha

        // Backface rendering
        Cull Front

        CGPROGRAM
        #pragma surface surf Standard alpha vertex:vert
        #pragma target 3.0 // Use shader model 3.0 target, to get nicer looking lighting
        #include "UnityCG.cginc"

        struct Input {
            float3 worldPos;
            float3 worldNormal;
            float4 screenPos;
            float eyeDepth;
            INTERNAL_DATA
        };

        fixed4 _Color;
        fixed4 _Foam;
        half _Glossiness;
        half _Metallic;

        sampler2D_float _CameraDepthTexture;
        float _InvFade;

        void vert(inout appdata_full v, out Input o) {
            UNITY_INITIALIZE_OUTPUT(Input, o);

            float4 v0 = displace_vert(mul(unity_ObjectToWorld, v.vertex));
            v.vertex = mul(unity_WorldToObject, v0);

            o.worldPos = v0;
            COMPUTE_EYEDEPTH(o.eyeDepth);
        }

        void surf(Input i, inout SurfaceOutputStandard o) {
            fixed4 c = _Color;
            o.Albedo = c.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;

            // Equivalent to SAMPLE_DEPTH_TEXTURE_PROJ(tex, pos) but handles divide-by-zero scenarios
            float rawZ = tex2D(_CameraDepthTexture, i.screenPos.xy / max(0.001, i.screenPos.w));
            float sceneZ = LinearEyeDepth(rawZ);
            float partZ = i.eyeDepth;
            float fade = 1.0;
            if (rawZ > 0.0) // Make sure the depth texture exists
                fade = saturate(_InvFade * (sceneZ - partZ));
            o.Albedo = c.rgb + (_Foam.rgb * (1 - fade));
            o.Alpha = c.a * fade;
            

            half3 worldT = WorldNormalVector(i, half3(1,0,0));
            half3 worldB = WorldNormalVector(i, half3(0,1,0));
            half3 worldN = WorldNormalVector(i, half3(0,0,1));
            half3x3 tbn = half3x3(worldT, worldB, worldN);

            half3 worldNormal = Unity_SafeNormalize(cross(ddx(i.worldPos), ddy(i.worldPos)));
            o.Normal = mul(tbn, worldNormal);
        }
        ENDCG

        // Frontface rendering
        Cull Back

        CGPROGRAM
        #pragma surface surf Standard alpha vertex:vert
        #pragma target 3.0 // Use shader model 3.0 target, to get nicer looking lighting
        #include "UnityCG.cginc"

        struct Input {
            float3 worldPos;
            float3 worldNormal;
            float4 screenPos;
            float eyeDepth;
            INTERNAL_DATA
        };

        fixed4 _Color;
        fixed4 _Foam;
        half _Glossiness;
        half _Metallic;

        sampler2D_float _CameraDepthTexture;
        float _InvFade;

        void vert(inout appdata_full v, out Input o) {
            UNITY_INITIALIZE_OUTPUT(Input, o);

            float4 v0 = displace_vert(mul(unity_ObjectToWorld, v.vertex));
            v.vertex = mul(unity_WorldToObject, v0);

            o.worldPos = v0;
            COMPUTE_EYEDEPTH(o.eyeDepth);
        }

        void surf(Input i, inout SurfaceOutputStandard o) {
            fixed4 c = _Color;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;

            // Equivalent to SAMPLE_DEPTH_TEXTURE_PROJ(tex, pos) but handles divide-by-zero scenarios
            float rawZ = tex2D(_CameraDepthTexture, i.screenPos.xy / max(0.001, i.screenPos.w));
            float sceneZ = LinearEyeDepth(rawZ);
            float partZ = i.eyeDepth;
            float fade = 1.0;
            if (rawZ > 0.0) // Make sure the depth texture exists
                fade = saturate(_InvFade * (sceneZ - partZ));
            o.Albedo = c.rgb + (_Foam.rgb * (1 - fade));
            o.Alpha = c.a * fade;

            half3 worldT = WorldNormalVector(i, half3(1,0,0));
            half3 worldB = WorldNormalVector(i, half3(0,1,0));
            half3 worldN = WorldNormalVector(i, half3(0,0,1));
            half3x3 tbn = half3x3(worldT, worldB, worldN);

            half3 worldNormal = -Unity_SafeNormalize(cross(ddx(i.worldPos), ddy(i.worldPos)));
            o.Normal = mul(tbn, worldNormal);
        }
        ENDCG
    }
}
