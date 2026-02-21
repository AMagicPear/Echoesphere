// Made with Amplify Shader Editor v1.9.2.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Glow"
{
	Properties
	{
		_ASEOutlineColor( "Outline Color", Color ) = (0,0.1942773,1,0)
		_ASEOutlineWidth( "Outline Width", Float ) = 0
		_Emission1("Emission 1", Color) = (1,0,0,0)
		_Emission1Str("Emission 1 Str", Float) = 1
		_Emission2("Emission 2", Color) = (0,1,0,0)
		_Emission2Str("Emission 2 Str", Float) = 1
		[Toggle]_FresnelEdge("Fresnel/Edge", Float) = 0
		_FresnelBias("Fresnel Bias", Float) = 0
		_FresnelScale("Fresnel Scale", Float) = 1
		_FresnelPower("Fresnel Power", Float) = 5
		_EdgeWidth("Edge Width", Float) = 0.509
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ }
		Cull Front
		CGPROGRAM
		#pragma target 3.0
		#pragma surface outlineSurf Outline nofog  keepalpha noshadow noambient novertexlights nolightmap nodynlightmap nodirlightmap nometa noforwardadd vertex:outlineVertexDataFunc 
		
		
		
		
		struct Input {
			half filler;
		};
		float4 _ASEOutlineColor;
		float _ASEOutlineWidth;
		void outlineVertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			v.vertex.xyz *= ( 1 + _ASEOutlineWidth);
		}
		inline half4 LightingOutline( SurfaceOutput s, half3 lightDir, half atten ) { return half4 ( 0,0,0, s.Alpha); }
		void outlineSurf( Input i, inout SurfaceOutput o )
		{
			o.Emission = _ASEOutlineColor.rgb;
			o.Alpha = 1;
		}
		ENDCG
		

		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Unlit keepalpha noshadow exclude_path:deferred 
		struct Input
		{
			float3 worldPos;
			float3 worldNormal;
		};

		uniform float _FresnelEdge;
		uniform float4 _Emission1;
		uniform float _Emission1Str;
		uniform float4 _Emission2;
		uniform float _Emission2Str;
		uniform float _FresnelBias;
		uniform float _FresnelScale;
		uniform float _FresnelPower;
		uniform float _EdgeWidth;

		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			float3 ase_worldPos = i.worldPos;
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float3 ase_worldNormal = i.worldNormal;
			float fresnelNdotV17 = dot( ase_worldNormal, ase_worldViewDir );
			float fresnelNode17 = ( _FresnelBias + _FresnelScale * pow( 1.0 - fresnelNdotV17, _FresnelPower ) );
			float4 lerpResult23 = lerp( ( _Emission1 * _Emission1Str ) , ( _Emission2 * _Emission2Str ) , saturate( fresnelNode17 ));
			float3 ase_vertexNormal = mul( unity_WorldToObject, float4( ase_worldNormal, 0 ) );
			ase_vertexNormal = normalize( ase_vertexNormal );
			float3 temp_output_32_0 = abs( ase_vertexNormal );
			float3 temp_cast_0 = (_EdgeWidth).xxx;
			float dotResult37 = dot( max( temp_output_32_0 , ( 1.0 - temp_output_32_0 ) ) , temp_cast_0 );
			float3 _Vector0 = float3(1.5,1,0);
			float ifLocalVar40 = 0;
			if( dotResult37 >= _Vector0.x )
				ifLocalVar40 = _Vector0.y;
			else
				ifLocalVar40 = _Vector0.z;
			float4 lerpResult43 = lerp( ( _Emission1 * _Emission1Str ) , ( _Emission2 * _Emission2Str ) , ifLocalVar40);
			o.Emission = (( _FresnelEdge )?( lerpResult43 ):( lerpResult23 )).rgb;
			o.Alpha = 1;
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=19202
Node;AmplifyShaderEditor.FresnelNode;17;-499.274,490.9836;Inherit;True;Standard;WorldNormal;ViewDir;False;False;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.NormalVertexDataNode;31;-700.4379,1140.575;Inherit;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMaxOpNode;36;189.7439,1137.606;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.OneMinusNode;35;-88.03358,1255.127;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.DotProductOpNode;37;455.6506,1153.039;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ConditionalIfNode;40;733.6237,1179.386;Inherit;False;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;43;1123.854,968.7936;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;46;271.2814,491.2436;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;48;-274.0272,53.21741;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;24;567.8964,792.4687;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;49;-401.504,200.6903;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.AbsOpNode;32;-354.505,1160.275;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;1621.192,874.6539;Float;False;True;-1;2;ASEMaterialInspector;0;0;Unlit;Glow;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;;0;False;;False;0;False;;0;False;;False;0;Opaque;0.5;True;False;0;False;Opaque;;Geometry;ForwardOnly;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;False;0;0;False;;0;False;;0;0;False;;0;False;;0;False;;0;False;;0;True;0;0,0.1942773,1,0;VertexScale;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;16;FLOAT4;0,0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
Node;AmplifyShaderEditor.LerpOp;23;279.4174,174.574;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SaturateNode;45;-57.54808,471.7674;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ToggleSwitchNode;42;1325.562,949.3626;Inherit;False;Property;_FresnelEdge;Fresnel/Edge;4;0;Create;True;0;0;0;False;0;False;0;True;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;22;-737.2172,221.7647;Inherit;False;Property;_Emission2;Emission 2;2;0;Create;True;0;0;0;False;0;False;0,1,0,0;0,0.8679245,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;1;-755.3853,-48.43875;Inherit;False;Property;_Emission1;Emission 1;0;0;Create;True;0;0;0;False;0;False;1,0,0,0;1,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;50;-827.1448,466.2357;Inherit;False;Property;_FresnelBias;Fresnel Bias;5;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;27;-824.1708,579.8298;Inherit;False;Property;_FresnelScale;Fresnel Scale;6;0;Create;True;0;0;0;False;0;False;1;2.98;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;28;-793.9357,675.502;Inherit;False;Property;_FresnelPower;Fresnel Power;7;0;Create;True;0;0;0;False;0;False;5;3.21;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;47;35.50903,747.483;Inherit;False;Property;_Emission1Str;Emission 1 Str;1;0;Create;True;0;0;0;False;0;False;1;2.44;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;25;307.1289,932.7255;Inherit;False;Property;_Emission2Str;Emission 2 Str;3;0;Create;True;0;0;0;False;0;False;1;2.17;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;38;153.018,1359.531;Inherit;False;Property;_EdgeWidth;Edge Width;8;0;Create;True;0;0;0;False;0;False;0.509;0.509;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector3Node;41;454.8986,1296.152;Inherit;False;Constant;_Vector0;Vector 0;9;0;Create;True;0;0;0;False;0;False;1.5,1,0;1.5,1,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
WireConnection;17;1;50;0
WireConnection;17;2;27;0
WireConnection;17;3;28;0
WireConnection;36;0;32;0
WireConnection;36;1;35;0
WireConnection;35;0;32;0
WireConnection;37;0;36;0
WireConnection;37;1;38;0
WireConnection;40;0;37;0
WireConnection;40;1;41;1
WireConnection;40;2;41;2
WireConnection;40;3;41;2
WireConnection;40;4;41;3
WireConnection;43;0;46;0
WireConnection;43;1;24;0
WireConnection;43;2;40;0
WireConnection;46;0;1;0
WireConnection;46;1;47;0
WireConnection;48;0;1;0
WireConnection;48;1;47;0
WireConnection;24;0;22;0
WireConnection;24;1;25;0
WireConnection;49;0;22;0
WireConnection;49;1;25;0
WireConnection;32;0;31;0
WireConnection;0;2;42;0
WireConnection;23;0;48;0
WireConnection;23;1;49;0
WireConnection;23;2;45;0
WireConnection;45;0;17;0
WireConnection;42;0;23;0
WireConnection;42;1;43;0
ASEEND*/
//CHKSM=3BB6442D2A2617B4EA44F60537039B9ADAAF4BAF