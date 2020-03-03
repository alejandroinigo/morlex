Shader "Custom/Terrain" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("TerrainTextureArray", 2DArray) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Specular ("Specular", Color) = (0.2, 0.2, 0.2)
		_BackgroundColor ("Background Color", Color) = (0,0,0)
		[Toggle(SHOW_MAP_DATA)] _ShowMapData ("Show Map Data", Float) = 0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Standard fullforwardshadows vertex:vert
		#pragma target 3.5

        #pragma shader_feature SHOW_MAP_DATA

        #include "./HexCellData.cginc"

		UNITY_DECLARE_TEX2DARRAY(_MainTex);

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;

		struct Input {
			float4 color : COLOR;
			float3 worldPos;
			float3 terrain;

            #if defined(SHOW_MAP_DATA)
				float mapData;
			#endif
		};

		void vert (inout appdata_full v, out Input data) {
			UNITY_INITIALIZE_OUTPUT(Input, data);
			
            float4 cell0 = GetCellData(v, 0);
			float4 cell1 = GetCellData(v, 1);
			float4 cell2 = GetCellData(v, 2);

			data.terrain.x = cell0.w;
			data.terrain.y = cell1.w;
			data.terrain.z = cell2.w;

            #if defined(SHOW_MAP_DATA)
				data.mapData = cell0.z * v.color.x + cell1.z * v.color.y +
					cell2.z * v.color.z;
			#endif
		}

		float4 GetTerrainColor (Input IN, int index) {
			float3 uvw = float3(IN.worldPos.xz * 0.02, IN.terrain[index]);
			float4 c = UNITY_SAMPLE_TEX2DARRAY(_MainTex, uvw);
			return c * IN.color[index];
		}

		void surf (Input IN, inout SurfaceOutputStandard o) {
			fixed4 c =
				GetTerrainColor(IN, 0) +
				GetTerrainColor(IN, 1) +
				GetTerrainColor(IN, 2);
			o.Albedo = c.rgb * _Color;
            #if defined(SHOW_MAP_DATA)
				o.Albedo = IN.mapData;
			#endif
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}