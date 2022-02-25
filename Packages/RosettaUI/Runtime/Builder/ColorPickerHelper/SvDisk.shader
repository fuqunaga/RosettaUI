Shader "Rosettaui/ColorPicker/SvDisk"
{
	Properties
	{
		_BlendWidthNormalized("BlendWIdthNormalized", Range(0,1)) = 0.1
		_Hue("Hue", Range(0,1)) = 0
	}

	SubShader
	{
		Tags { "PreviewType"="Plane" }
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			float2 _TargetSize;
			float _BlendWidthNormalized;
			float _Hue;


			inline float2 circleToSquare(float2 uv)
			{
				const float two_sqrt2 = 2.0 * sqrt(2.0);

				const float u = uv.x;
				float v = uv.y;

				float u2 = u * u;
				float v2 = v * v;

				float subTermX = 2.0 + u2 - v2;
				float subTermY = 2.0 - u2 + v2;
				float termX1 = subTermX + u * two_sqrt2;
				float termX2 = subTermX - u * two_sqrt2;
				float termY1 = subTermY + v * two_sqrt2;
				float termY2 = subTermY - v * two_sqrt2;

				return float2(
					0.5 * sqrt(termX1) - 0.5 * sqrt(termX2),
					0.5 * sqrt(termY1) - 0.5 * sqrt(termY2)
				);
			}

			inline float3 HUEtoRGB(in float H)
			{
				float R = abs(H * 6 - 3) - 1;
				float G = 2 - abs(H * 6 - 2);
				float B = 2 - abs(H * 6 - 4);
				return saturate(float3(R,G,B));
			}

			inline float3 HSVtoRGB(in float3 HSV)
			{
				float3 RGB = HUEtoRGB(HSV.x);
				return ((RGB - 1) * HSV.y + 1) * HSV.z;
			}

			inline float inverseLerp(float a, float b, float value)
			{
				return (value - a) / (b - a);
			}

			fixed4 frag (v2f_img i) : SV_Target
			{
				fixed4 color = (0).xxxx;

				float2 pos = (i.uv - 0.5) * 2.0; // map 0~1 > -1~1
				if ( dot(pos, pos) <= 1.0)
				{
					float2 posOnSquare = circleToSquare(pos);

					float2 sv = (posOnSquare + (1.0).xx) * 0.5f; // map -1~1 > 0~1

					color.rgb = HSVtoRGB(float3(_Hue, sv.x, sv.y));
					color.a = inverseLerp(1.0, 1.0 - _BlendWidthNormalized, length(pos));
				}
				
				return color;
			}
			ENDCG
		}
	}
}