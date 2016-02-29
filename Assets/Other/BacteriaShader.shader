Shader "Unlit/BacteriaOutline"
{
	Properties
	{
		_Thickness ("Thickness", Float) = 0.5
	}
	SubShader
	{
		Tags { "RenderType"="Transparent" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float4 color : COLOR0;
			};

			struct v2f
			{
				float alpha : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};
			
			float _Thickness;
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.alpha = v.color.a;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = i.alpha >  _Thickness ? fixed4(1,1,1,i.alpha) : fixed4(0,0,0,0);
				return col;
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
}
