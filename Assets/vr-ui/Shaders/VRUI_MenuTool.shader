Shader "Unlit/VRUI_MenuTool"
{
	Properties
	{
		_Color("Color", Color) = (1.0, 1.0, 1.0, 1.0)
		_MainTex ("Texture", 2D) = "white" {}
		_CursorPosition("Cursor Position", Vector) = (0.0, 0.0, 0.0, 0.0)
		_CursorState("Cursor State", Int) = 0
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
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

			float4 _Color;

			float4 _CursorPosition;
			int _CursorState;
			
			v2f vert (appdata v)
			{
				//Calculate cursor pos
				float4 cp = _CursorPosition;

				//Calculate falloff amount
				float4 p = v.vertex - float4(cp.x, cp.y, cp.z, 1.0);
				
				//Calculate falloff
				float falloff = pow(1.0 / length(p), 2.25) * 0.27;
				falloff = clamp(falloff, 0.0, 1.0);

				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex + (cp * falloff/* ((_CursorState == 0) ? falloff : 1)*/));
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv) * _Color;
				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}
}
