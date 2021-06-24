﻿Shader "Custom/ToonShader"
{
	Properties
	{
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_ShadingQuality ("Shading Quality", float) = 10
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM

		half _ShadingQuality;

		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Toon fullforwardshadows
		half4 LightingToon(SurfaceOutput s, half3 lightDir, half atten) {

			half NdotL = dot(s.Normal, lightDir);

			half light = (NdotL * atten);
			light = max(0, light);
			light = floor(light * _ShadingQuality) / _ShadingQuality;

			half4 c;
			c.rgb = s.Albedo * _LightColor0.rgb * light;
			c.a = s.Alpha;
			return c;
		}

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;

		struct Input
		{
			float2 uv_MainTex;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)

		void surf (Input IN, inout SurfaceOutput o)
		{
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}

/*Shader "Example/Diffuse Texture" {
	Properties{
		_MainTex("Texture", 2D) = "white" {}
	}
		SubShader{
		Tags { "RenderType" = "Opaque" }
		CGPROGRAM
		  #pragma surface surf SimpleLambert

		  half4 LightingSimpleLambert(SurfaceOutput s, half3 lightDir, half atten) {
			  half NdotL = dot(s.Normal, lightDir);
			  half4 c;
			  c.rgb = s.Albedo * _LightColor0.rgb * (NdotL * atten);
			  c.a = s.Alpha;
			  return c;
		  }

		struct Input {
			float2 uv_MainTex;
		};

		sampler2D _MainTex;

		void surf(Input IN, inout SurfaceOutput o) {
			o.Albedo = tex2D(_MainTex, IN.uv_MainTex).rgb;
		}
		ENDCG
	}
		Fallback "Diffuse"
}*/