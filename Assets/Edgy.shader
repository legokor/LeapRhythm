Shader "Custom/Edgy" {
	Properties {
		_MainTex("Texture", 2D) = "white" {}
		_Color("Main color", Color) = (1, 1, 1, 1)
		_Edge("Edge color", Color) = (0, 0, 1, 1)
	}
	SubShader {
		Tags { "RenderType" = "Opaque" }
		LOD 100

		Pass {
			CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma multi_compile_fog
#include "UnityCG.cginc"

			struct appdata {
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f {
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _Color;
			float4 _Edge;

			v2f vert(appdata v) {
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target {
				fixed edge = pow(max(abs(i.uv.x - 0.5), abs(i.uv.y - 0.5)) * 2.0, 5.0);
				fixed4 col = tex2D(_MainTex, i.uv) * (edge * _Edge + (1.0 - edge) * _Color);
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}
}