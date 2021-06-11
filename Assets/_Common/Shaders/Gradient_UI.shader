// Based on : Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "UI/Gradient"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		
		[Gradient(_GradientMode, _GradientWidth)] _Gradient("Gradient", 2D) = "white" {}
		[HideInInspector] _GradientMode("Gradient Mode", float) = 0
		[HideInInspector] _GradientWidth("Gradient width", float) = 2
		[KeywordEnum(Linear, Angular, Radial)] _GradientType("Gradient Type", Float) = 0
		_StartAxis("Start Axis", Range(0, 360)) = 0
		[Toggle] _Invert("Invert", int) = 0

		[Header(Other)] _Color("Tint", Color) = (1,1,1,1)

		_StencilComp("Stencil Comparison", Float) = 8
		_Stencil("Stencil ID", Float) = 0
		_StencilOp("Stencil Operation", Float) = 0
		_StencilWriteMask("Stencil Write Mask", Float) = 255
		_StencilReadMask("Stencil Read Mask", Float) = 255

		_ColorMask("Color Mask", Float) = 15

		[Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip("Use Alpha Clip", Float) = 0
	}

		SubShader
		{
			Tags
			{
				"Queue" = "Transparent"
				"IgnoreProjector" = "True"
				"RenderType" = "Transparent"
				"PreviewType" = "Plane"
				"CanUseSpriteAtlas" = "True"
			}

			Stencil
			{
				Ref[_Stencil]
				Comp[_StencilComp]
				Pass[_StencilOp]
				ReadMask[_StencilReadMask]
				WriteMask[_StencilWriteMask]
			}

			Cull Off
			Lighting Off
			ZWrite Off
			ZTest[unity_GUIZTestMode]
			Blend SrcAlpha OneMinusSrcAlpha
			ColorMask[_ColorMask]

			Pass
			{
				Name "Default"
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma target 2.0

				#include "UnityCG.cginc"
				#include "UnityUI.cginc"

				#pragma multi_compile_local _ UNITY_UI_CLIP_RECT
				#pragma multi_compile_local _ UNITY_UI_ALPHACLIP

				struct appdata_t
				{
					float4 vertex   : POSITION;
					float4 color    : COLOR;
					float2 texcoord : TEXCOORD0;
					UNITY_VERTEX_INPUT_INSTANCE_ID
				};

				struct v2f
				{
					float4 vertex   : SV_POSITION;
					fixed4 color : COLOR;
					float2 texcoord  : TEXCOORD0;
					float4 worldPosition : TEXCOORD1;
					UNITY_VERTEX_OUTPUT_STEREO
				};

				sampler2D _MainTex;
				sampler2D _Gradient;
				float _GradientMode;
				float _GradientWidth;
				float _GradientType;
				float _StartAxis;
				int _Invert;
				
				fixed4 _Color;
				fixed4 _TextureSampleAdd;
				float4 _ClipRect;
				float4 _MainTex_ST;

				half1 invLerp(half1 from, half1 to, half1 value) { return (value - from) / (to - from); }
				half2 invLerp(half2 from, half2 to, half2 value) { return (value - from) / (to - from); }
				half3 invLerp(half3 from, half3 to, half3 value) { return (value - from) / (to - from); }
				half4 invLerp(half4 from, half4 to, half4 value) { return (value - from) / (to - from); }

				float1 invLerp(float1 from, float1 to, float1 value) { return (value - from) / (to - from); }
				float2 invLerp(float2 from, float2 to, float2 value) { return (value - from) / (to - from); }
				float3 invLerp(float3 from, float3 to, float3 value) { return (value - from) / (to - from); }
				float4 invLerp(float4 from, float4 to, float4 value) { return (value - from) / (to - from); }

				half4 getGradientColor(float gradientT) 
				{
					gradientT = clamp(gradientT, 0, 1);

					if (_GradientWidth <= 1) return tex2D(_Gradient, half2(0,0));

					half4 from = half4(0,0,0,1);
					half4 to = half4(0,0,0,1);

					bool isIFixed = false;
					bool isIBeforeCurve = false;

					half pixelToRatio = 1/(_GradientWidth + 1);

					for(int i = 0; i < _GradientWidth; i++) 
					{
						if (!isIFixed) {
							half x1 = i;
							half x2 = i + 1;
							from = tex2D(_Gradient, half2(x1 * pixelToRatio, 0));
							to =   tex2D(_Gradient, half2(x2 * pixelToRatio, 0));
						}

						if (gradientT < from.a)
						{
							to = from; 
							isIFixed = true;
							isIBeforeCurve = true;
						}
						if (gradientT < to.a) isIFixed = true;
					}

					if (isIBeforeCurve) return from;
					if (_GradientMode == 1) return to;

					float lerpVal = invLerp(from.a, to.a, gradientT);
					return lerp(from, to, lerpVal);
				}

				half4 getGradient(v2f IN) 
				{
					float2 coords = IN.texcoord;
					float t = 0;
					float2 offsetedCoords = coords - float2(0.5,0.5);
					//Remember enum : Linear, Angular, Radial
					if (_GradientType == 0 || _GradientType == 1) 
					{
						half rad = radians(_StartAxis);
						float2 direction = float2(cos(rad), sin(rad));

						if (_GradientType == 0)
							t = dot(direction, offsetedCoords) + 0.5;
						
						else if (_GradientType == 1)
							t = (invLerp(-180, 180, (degrees(atan2(offsetedCoords.y, offsetedCoords.x)))) + invLerp(0, 360, _StartAxis)) % 1;
					}
					else 
						t = length(offsetedCoords);

					if (_Invert) t = 1-t;

					//return half4(t,t,t,1);
					return getGradientColor(t);
				}

				v2f vert(appdata_t v)
				{
					v2f OUT;
					UNITY_SETUP_INSTANCE_ID(v);
					UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
					OUT.worldPosition = v.vertex;
					OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);

					OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);

					OUT.color = v.color * _Color;
					return OUT;
				}

				fixed4 frag(v2f IN) : SV_Target
				{
					half4 gradientColor = getGradient(IN);
					gradientColor.a = 1;

					half4 color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color * gradientColor;
					//half4 color = gradientColor;

					#ifdef UNITY_UI_CLIP_RECT
					color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
					#endif

					#ifdef UNITY_UI_ALPHACLIP
					clip(color.a - 0.001);
					#endif

					return color;
				}
			ENDCG
			}
		}
}