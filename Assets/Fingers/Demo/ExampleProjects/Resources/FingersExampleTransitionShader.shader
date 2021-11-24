Shader "FingersGestures/FingersExampleTransitionShader"
{
	Properties
	{
		_MainTex1 ("Texture", 2D) = "green" {}
		_MainTex2 ("Texture", 2D) = "blue" {}
		_Fade ("Fade", Range(0.0, 1.0)) = 0.5
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
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
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			uniform sampler2D _MainTex1;
			uniform float4 _MainTex1_ST;
			uniform sampler2D _MainTex2;
			uniform float4 _MainTex2_ST;
			uniform fixed _Fade;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv.xy = TRANSFORM_TEX(v.uv, _MainTex1);
				o.uv.zw = TRANSFORM_TEX(v.uv, _MainTex2);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// this is a fade effect, but other effects are easily added
				fixed4 col1 = tex2D(_MainTex1, i.uv.xy);
				fixed4 col2 = tex2D(_MainTex2, i.uv.zw);
				fixed4 col = lerp(col1, col2, _Fade);
				return col;
			}
			ENDCG
		}
	}
}
