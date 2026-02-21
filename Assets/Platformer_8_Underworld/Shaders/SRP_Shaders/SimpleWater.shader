// Made with Amplify Shader Editor v1.9.2.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "SimpleWater"
{
	Properties
	{
		_Pattern("Pattern", 2D) = "white" {}
		_Color1("Color 1", Color) = (0.1137255,0.3960785,0.6,1)
		_Color2("Color 2", Color) = (0.4453542,0.659279,0.6792453,1)
		_ScaleDistortion("Scale Distortion", Float) = 2.4
		_DistortionStr("Distortion Str", Float) = 0.24
		_Layer1Direction("Layer 1 Direction", Vector) = (0.5,0,0,0)
		_Layer1Speed("Layer 1 Speed", Float) = 0.6
		_Layer1Tiling("Layer 1 Tiling", Float) = 0.2
		_Layer2Direction("Layer 2 Direction", Vector) = (0.5,0,0,0)
		_Layer2Speed("Layer 2 Speed", Float) = 0.4
		_Layer2Tiling("Layer 2 Tiling", Float) = 0.15
		_Layer3Direction("Layer 3 Direction", Vector) = (0,-0.5,0,0)
		_Layer3Speed("Layer 3 Speed", Float) = 0.2
		_Layer3Tiling("Layer 3 Tiling", Float) = 0.2
		_Blend1("Blend 1", Float) = 0
		_Blend2("Blend 2", Float) = 0
		_FoamWidth("Foam Width", Float) = 0.4
		_FoamScale("Foam Scale", Float) = 4
		_FoamDistortionSpeed("Foam Distortion Speed", Float) = 1.5
		_FoamDistortionScale("Foam Distortion Scale", Float) = 4
		_FoamDistortionAmount("Foam Distortion Amount", Range( 0 , 0.1)) = 0.03
		_WaveTiling("Wave Tiling", Vector) = (0.2,1,0,0)
		_WaveScale("Wave Scale", Float) = 0.04
		_WaveSpeed("Wave Speed", Float) = 4
		_WaveBlend("Wave Blend", Float) = 0
		_Metallic("Metallic", Range( 0 , 1)) = 0
		_Smoothness("Smoothness", Range( 0 , 1)) = 0
		_Tessellation("Tessellation", Float) = 4
		[Toggle]_PatternInvertBW("Pattern Invert  B/W", Float) = 1
		_DisplacementAmount("Displacement Amount", Float) = 0.4
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" }
		Cull Back
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#include "UnityCG.cginc"
		#include "Tessellation.cginc"
		#pragma target 4.6
		#pragma surface surf Standard alpha:fade keepalpha vertex:vertexDataFunc tessellate:tessFunction 
		struct Input
		{
			float3 worldPos;
			float3 worldNormal;
			INTERNAL_DATA
			float4 screenPos;
		};

		uniform float _WaveSpeed;
		uniform float2 _WaveTiling;
		uniform float _WaveScale;
		uniform float _DisplacementAmount;
		uniform float _ScaleDistortion;
		uniform float _DistortionStr;
		uniform float _PatternInvertBW;
		uniform sampler2D _Pattern;
		uniform float _Layer3Speed;
		uniform float2 _Layer3Direction;
		uniform float _Layer3Tiling;
		uniform float _Layer2Speed;
		uniform float2 _Layer2Direction;
		uniform float _Layer2Tiling;
		uniform float4 _Color1;
		uniform float4 _Color2;
		uniform float _Layer1Speed;
		uniform float2 _Layer1Direction;
		uniform float _Layer1Tiling;
		uniform float _Blend1;
		uniform float _Blend2;
		uniform float _WaveBlend;
		UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
		uniform float4 _CameraDepthTexture_TexelSize;
		uniform float _FoamWidth;
		uniform float _FoamDistortionSpeed;
		uniform float _FoamDistortionScale;
		uniform float _FoamDistortionAmount;
		uniform float _FoamScale;
		uniform float _Metallic;
		uniform float _Smoothness;
		uniform float _Tessellation;


		float3 mod2D289( float3 x ) { return x - floor( x * ( 1.0 / 289.0 ) ) * 289.0; }

		float2 mod2D289( float2 x ) { return x - floor( x * ( 1.0 / 289.0 ) ) * 289.0; }

		float3 permute( float3 x ) { return mod2D289( ( ( x * 34.0 ) + 1.0 ) * x ); }

		float snoise( float2 v )
		{
			const float4 C = float4( 0.211324865405187, 0.366025403784439, -0.577350269189626, 0.024390243902439 );
			float2 i = floor( v + dot( v, C.yy ) );
			float2 x0 = v - i + dot( i, C.xx );
			float2 i1;
			i1 = ( x0.x > x0.y ) ? float2( 1.0, 0.0 ) : float2( 0.0, 1.0 );
			float4 x12 = x0.xyxy + C.xxzz;
			x12.xy -= i1;
			i = mod2D289( i );
			float3 p = permute( permute( i.y + float3( 0.0, i1.y, 1.0 ) ) + i.x + float3( 0.0, i1.x, 1.0 ) );
			float3 m = max( 0.5 - float3( dot( x0, x0 ), dot( x12.xy, x12.xy ), dot( x12.zw, x12.zw ) ), 0.0 );
			m = m * m;
			m = m * m;
			float3 x = 2.0 * frac( p * C.www ) - 1.0;
			float3 h = abs( x ) - 0.5;
			float3 ox = floor( x + 0.5 );
			float3 a0 = x - ox;
			m *= 1.79284291400159 - 0.85373472095314 * ( a0 * a0 + h * h );
			float3 g;
			g.x = a0.x * x0.x + h.x * x0.y;
			g.yz = a0.yz * x12.xz + h.yz * x12.yw;
			return 130.0 * dot( m, g );
		}


		float3 PerturbNormal107_g1( float3 surf_pos, float3 surf_norm, float height, float scale )
		{
			// "Bump Mapping Unparametrized Surfaces on the GPU" by Morten S. Mikkelsen
			float3 vSigmaS = ddx( surf_pos );
			float3 vSigmaT = ddy( surf_pos );
			float3 vN = surf_norm;
			float3 vR1 = cross( vSigmaT , vN );
			float3 vR2 = cross( vN , vSigmaS );
			float fDet = dot( vSigmaS , vR1 );
			float dBs = ddx( height );
			float dBt = ddy( height );
			float3 vSurfGrad = scale * 0.05 * sign( fDet ) * ( dBs * vR1 + dBt * vR2 );
			return normalize ( abs( fDet ) * vN - vSurfGrad );
		}


		float4 tessFunction( appdata_full v0, appdata_full v1, appdata_full v2 )
		{
			float4 temp_cast_0 = (_Tessellation).xxxx;
			return temp_cast_0;
		}

		void vertexDataFunc( inout appdata_full v )
		{
			float mulTime45 = _Time.y * _WaveSpeed;
			float3 ase_worldPos = mul( unity_ObjectToWorld, v.vertex );
			float2 temp_output_5_0 = (ase_worldPos).xz;
			float2 panner46 = ( mulTime45 * float2( 0.5,0.5 ) + ( temp_output_5_0 * _WaveTiling ));
			float simplePerlin2D39 = snoise( panner46*_WaveScale );
			simplePerlin2D39 = simplePerlin2D39*0.5 + 0.5;
			float3 ase_worldNormal = UnityObjectToWorldNormal( v.normal );
			float3 ase_vertexNormal = v.normal.xyz;
			v.vertex.xyz += ( ( ( simplePerlin2D39 * _DisplacementAmount ) * ase_worldNormal.y ) * ase_vertexNormal );
			v.vertex.w = 1;
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float3 ase_worldPos = i.worldPos;
			float3 surf_pos107_g1 = ase_worldPos;
			float3 ase_worldNormal = WorldNormalVector( i, float3( 0, 0, 1 ) );
			float3 surf_norm107_g1 = ase_worldNormal;
			float simplePerlin2D103 = snoise( (ase_worldPos).xz*_ScaleDistortion );
			simplePerlin2D103 = simplePerlin2D103*0.5 + 0.5;
			float height107_g1 = simplePerlin2D103;
			float scale107_g1 = _DistortionStr;
			float3 localPerturbNormal107_g1 = PerturbNormal107_g1( surf_pos107_g1 , surf_norm107_g1 , height107_g1 , scale107_g1 );
			float3 ase_worldTangent = WorldNormalVector( i, float3( 1, 0, 0 ) );
			float3 ase_worldBitangent = WorldNormalVector( i, float3( 0, 1, 0 ) );
			float3x3 ase_worldToTangent = float3x3( ase_worldTangent, ase_worldBitangent, ase_worldNormal );
			float3 worldToTangentDir42_g1 = mul( ase_worldToTangent, localPerturbNormal107_g1);
			float3 temp_output_107_40 = worldToTangentDir42_g1;
			float3 Normal120 = temp_output_107_40;
			o.Normal = Normal120;
			float mulTime45 = _Time.y * _WaveSpeed;
			float2 temp_output_5_0 = (ase_worldPos).xz;
			float2 panner46 = ( mulTime45 * float2( 0.5,0.5 ) + ( temp_output_5_0 * _WaveTiling ));
			float simplePerlin2D39 = snoise( panner46*_WaveScale );
			simplePerlin2D39 = simplePerlin2D39*0.5 + 0.5;
			float4 temp_cast_0 = (simplePerlin2D39).xxxx;
			float mulTime34 = _Time.y * _Layer3Speed;
			float3 temp_output_106_0 = ( float3( temp_output_5_0 ,  0.0 ) + temp_output_107_40 );
			float2 panner30 = ( mulTime34 * _Layer3Direction + temp_output_106_0.xy);
			float4 tex2DNode27 = tex2D( _Pattern, ( panner30 * _Layer3Tiling ) );
			float4 temp_cast_3 = (( (( _PatternInvertBW )?( 1.0 ):( 0.0 )) == 0.0 ? tex2DNode27.r : ( 1.0 - tex2DNode27.r ) )).xxxx;
			float mulTime125 = _Time.y * _Layer2Speed;
			float2 panner20 = ( mulTime125 * _Layer2Direction + temp_output_106_0.xy);
			float4 tex2DNode17 = tex2D( _Pattern, ( panner20 * _Layer2Tiling ) );
			float4 temp_cast_6 = (( (( _PatternInvertBW )?( 1.0 ):( 0.0 )) == 0.0 ? tex2DNode17.r : ( 1.0 - tex2DNode17.r ) )).xxxx;
			float mulTime15 = _Time.y * _Layer1Speed;
			float2 panner13 = ( mulTime15 * _Layer1Direction + temp_output_106_0.xy);
			float4 tex2DNode9 = tex2D( _Pattern, ( panner13 * _Layer1Tiling ) );
			float4 lerpResult10 = lerp( _Color1 , _Color2 , ( (( _PatternInvertBW )?( 1.0 ):( 0.0 )) == 0.0 ? tex2DNode9.r : ( 1.0 - tex2DNode9.r ) ));
			float4 blendOpSrc24 = temp_cast_6;
			float4 blendOpDest24 = lerpResult10;
			float4 lerpBlendMode24 = lerp(blendOpDest24,min( blendOpSrc24 , blendOpDest24 ),_Blend1);
			float4 blendOpSrc35 = temp_cast_3;
			float4 blendOpDest35 = ( saturate( lerpBlendMode24 ));
			float4 lerpBlendMode35 = lerp(blendOpDest35,min( blendOpSrc35 , blendOpDest35 ),_Blend2);
			float4 blendOpSrc41 = temp_cast_0;
			float4 blendOpDest41 = ( saturate( lerpBlendMode35 ));
			float4 lerpBlendMode41 = lerp(blendOpDest41,( blendOpSrc41 * blendOpDest41 ),_WaveBlend);
			float4 temp_cast_9 = (1.0).xxxx;
			float4 ase_screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
			float4 ase_screenPosNorm = ase_screenPos / ase_screenPos.w;
			ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
			float screenDepth59 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ase_screenPosNorm.xy ));
			float distanceDepth59 = abs( ( screenDepth59 - LinearEyeDepth( ase_screenPosNorm.z ) ) / ( _FoamWidth ) );
			float2 temp_output_61_0 = (ase_worldPos).xz;
			float2 temp_cast_10 = (_FoamDistortionSpeed).xx;
			float2 panner62 = ( 0.2 * _Time.y * temp_cast_10 + temp_output_61_0);
			float simplePerlin2D65 = snoise( panner62*_FoamDistortionScale );
			simplePerlin2D65 = simplePerlin2D65*0.5 + 0.5;
			float simplePerlin2D69 = snoise( ( temp_output_61_0 + ( simplePerlin2D65 * _FoamDistortionAmount ) )*_FoamScale );
			simplePerlin2D69 = simplePerlin2D69*0.5 + 0.5;
			float4 lerpResult55 = lerp( ( saturate( lerpBlendMode41 )) , temp_cast_9 , saturate( step( distanceDepth59 , simplePerlin2D69 ) ));
			float4 Base_Color118 = lerpResult55;
			o.Albedo = Base_Color118.rgb;
			o.Metallic = _Metallic;
			o.Smoothness = _Smoothness;
			float screenDepth114 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ase_screenPosNorm.xy ));
			float distanceDepth114 = abs( ( screenDepth114 - LinearEyeDepth( ase_screenPosNorm.z ) ) / ( 0.04 ) );
			o.Alpha = saturate( distanceDepth114 );
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=19202
Node;AmplifyShaderEditor.CommentaryNode;58;-524.3294,2470.803;Inherit;False;2222.684;607.4407;Foam;15;73;72;71;70;69;68;67;66;65;64;63;62;61;60;59;;0,1,0,1;0;0
Node;AmplifyShaderEditor.DepthFade;59;399.199,2520.802;Inherit;False;True;False;True;2;1;FLOAT3;0,0,0;False;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode;60;-387.0533,2554.424;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.ComponentMaskNode;61;-155.2539,2604.642;Inherit;False;True;False;True;False;1;0;FLOAT3;0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.PannerNode;62;-207.9483,2862.965;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0.5,0.5;False;1;FLOAT;0.2;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;63;489.806,2649.738;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;64;348.2563,2764.085;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;65;49.70164,2823.994;Inherit;True;Simplex2D;True;False;2;0;FLOAT2;0,0;False;1;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;66;-255.0322,2995.518;Inherit;False;Property;_FoamDistortionScale;Foam Distortion Scale;19;0;Create;True;0;0;0;False;0;False;4;4;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;67;-482.8192,2900.554;Inherit;False;Property;_FoamDistortionSpeed;Foam Distortion Speed;18;0;Create;True;0;0;0;False;0;False;1.5;2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;68;55.91456,2731.83;Inherit;False;Property;_FoamDistortionAmount;Foam Distortion Amount;20;0;Create;True;0;0;0;False;0;False;0.03;0.03;0;0.1;0;1;FLOAT;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;69;643.5131,2790.603;Inherit;True;Simplex2D;True;False;2;0;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;70;146.9301,2537.82;Inherit;False;Property;_FoamWidth;Foam Width;16;0;Create;True;0;0;0;False;0;False;0.4;0.4;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;71;374.2573,2896.761;Inherit;False;Property;_FoamScale;Foam Scale;17;0;Create;True;0;0;0;False;0;False;4;3;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;72;1529.549,2739.105;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StepOpNode;73;1176.764,2568.841;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;77;2511.287,2590.325;Inherit;False;1073.688;521.8873;Displacement;6;85;83;84;87;86;78;;0,0,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;113;2529.096,2371.271;Inherit;False;688.0833;163.4724;Edge Opacity;3;116;115;114;;1,0,0,1;0;0
Node;AmplifyShaderEditor.DepthFade;114;2784.707,2421.272;Inherit;False;True;False;True;2;1;FLOAT3;0,0,0;False;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;115;3059.234,2430.927;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;116;2559.292,2432.256;Inherit;False;Constant;_EdgeOpacity;Edge Opacity;40;0;Create;True;0;0;0;False;0;False;0.04;0.04;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;110;3462.095,2206.929;Inherit;False;120;Normal;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;119;3459.928,2124.351;Inherit;False;118;Base Color;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;111;3355.908,2322.302;Inherit;False;Property;_Metallic;Metallic;25;0;Create;True;0;0;0;False;0;False;0;0.107;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.BlendOpsNode;41;2053.707,1459.324;Inherit;False;Multiply;True;3;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;2;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;56;2197.646,1965.487;Inherit;False;Constant;_Float13;Float 13;22;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;45;676.3004,2113.009;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;101;718.3534,1792.714;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.PannerNode;46;924.2498,1858.917;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;39;1162.48,1914.375;Inherit;True;Simplex2D;True;False;2;0;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;104;-1297.831,658.2666;Inherit;False;True;False;True;True;1;0;FLOAT3;0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;103;-1011.829,637.4666;Inherit;True;Simplex2D;True;False;2;0;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;5;-988.4161,963.9192;Inherit;False;True;False;True;True;1;0;FLOAT3;0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.FunctionNode;107;-740.9633,638.4374;Inherit;False;Normal From Height;-1;;1;1942fe2c5f1a1f94881a33d532e4afeb;0;2;20;FLOAT;0;False;110;FLOAT;1;False;2;FLOAT3;40;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;106;-450.3224,872.6459;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;105;-1255.624,756.5384;Inherit;False;Property;_ScaleDistortion;Scale Distortion;3;0;Create;True;0;0;0;False;0;False;2.4;2.4;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;120;-397.6793,641.8879;Inherit;False;Normal;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.WorldPosInputsNode;6;-1543.027,875.5552;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleTimeNode;15;-468.7493,1170.241;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;125;-443.1929,1547.522;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;20;-106.7763,1304.74;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;18;139.8859,1322.435;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.PannerNode;13;-96.02583,938.6428;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;7;170.6622,951.5257;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.PannerNode;30;-114.8272,1579.406;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;28;142.3939,1610.157;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleTimeNode;34;-439.9877,1877.285;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;14;-681.8424,1024.036;Inherit;False;Property;_Layer1Direction;Layer 1 Direction;5;0;Create;True;0;0;0;False;0;False;0.5,0;0.5,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.RangedFloatNode;16;-664.967,1182.227;Inherit;False;Property;_Layer1Speed;Layer 1 Speed;6;0;Create;True;0;0;0;False;0;False;0.6;0.6;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;8;-93.38837,1083.852;Inherit;False;Property;_Layer1Tiling;Layer 1 Tiling;7;0;Create;True;0;0;0;False;0;False;0.2;0.2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;124;-673.1671,1407.346;Inherit;False;Property;_Layer2Direction;Layer 2 Direction;8;0;Create;True;0;0;0;False;0;False;0.5,0;0.5,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.RangedFloatNode;123;-675.3799,1554.89;Inherit;False;Property;_Layer2Speed;Layer 2 Speed;9;0;Create;True;0;0;0;False;0;False;0.4;0.4;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;19;-104.4372,1457.318;Inherit;False;Property;_Layer2Tiling;Layer 2 Tiling;10;0;Create;True;0;0;0;False;0;False;0.15;0.15;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;32;-661.8591,1720.285;Inherit;False;Property;_Layer3Direction;Layer 3 Direction;11;0;Create;True;0;0;0;False;0;False;0,-0.5;0,-0.5;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.RangedFloatNode;33;-667.7184,1876.065;Inherit;False;Property;_Layer3Speed;Layer 3 Speed;12;0;Create;True;0;0;0;False;0;False;0.2;0.2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;29;-93.60603,1719.157;Inherit;False;Property;_Layer3Tiling;Layer 3 Tiling;13;0;Create;True;0;0;0;False;0;False;0.2;0.2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;108;-734.8203,760.3999;Inherit;False;Property;_DistortionStr;Distortion Str;4;0;Create;True;0;0;0;False;0;False;0.24;0.2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;17;536.9893,1289.237;Inherit;True;Property;_Pattern_5;Pattern_4;0;0;Create;True;0;0;0;False;0;False;-1;30a088e57a007264db17be84689c8fe7;30a088e57a007264db17be84689c8fe7;True;0;False;white;Auto;False;Instance;9;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector2Node;43;472.2451,1998.579;Inherit;False;Constant;_Vector1;Vector 1;21;0;Create;True;0;0;0;False;0;False;0.5,0.5;0.5,0.5;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.Vector2Node;102;468.694,1858.398;Inherit;False;Property;_WaveTiling;Wave Tiling;21;0;Create;True;0;0;0;False;0;False;0.2,1;0.2,1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.RangedFloatNode;44;442.408,2153.623;Inherit;False;Property;_WaveSpeed;Wave Speed;23;0;Create;True;0;0;0;False;0;False;4;4;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;40;944.5905,2030.75;Inherit;False;Property;_WaveScale;Wave Scale;22;0;Create;True;0;0;0;False;0;False;0.04;0.06;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;55;2744.735,2086.597;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;118;2975.894,2197.508;Inherit;False;Base Color;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;9;396.4649,1073.908;Inherit;True;Property;_Pattern;Pattern;0;0;Create;True;0;0;0;False;0;False;-1;30a088e57a007264db17be84689c8fe7;30a088e57a007264db17be84689c8fe7;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Compare;134;1171.627,1501.871;Inherit;False;0;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;133;1017.131,1626.858;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;136;846.5271,1376.203;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;138;682.7784,1185.273;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.BlendOpsNode;35;1713.638,1247.66;Inherit;True;Darken;True;3;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;2;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;42;1732.246,1500.591;Inherit;False;Property;_WaveBlend;Wave Blend;24;0;Create;True;0;0;0;False;0;False;0;0.69;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;36;1449.202,1506.282;Inherit;False;Property;_Blend2;Blend 2;15;0;Create;True;0;0;0;False;0;False;0;-0.17;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.BlendOpsNode;24;1374.691,1083.248;Inherit;True;Darken;True;3;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;2;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.Compare;135;1120.316,1275.967;Inherit;False;0;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;25;1105.516,1137.213;Inherit;False;Property;_Blend1;Blend 1;14;0;Create;True;0;0;0;False;0;False;0;0.14;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;10;1103.87,874.0237;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.Compare;137;860.0059,1037.046;Inherit;True;0;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;12;439.299,701.8497;Inherit;False;Property;_Color2;Color 2;2;0;Create;True;0;0;0;False;0;False;0.4453542,0.659279,0.6792453,1;0.5179334,0.7814097,0.9716981,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;11;429.1418,505.0187;Inherit;False;Property;_Color1;Color 1;1;0;Create;True;0;0;0;False;0;False;0.1137255,0.3960785,0.6,1;0.1137253,0.3960783,0.6,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;27;632.9062,1524.605;Inherit;True;Property;_Pattern_6;Pattern_4;0;0;Create;True;0;0;0;False;0;False;-1;30a088e57a007264db17be84689c8fe7;30a088e57a007264db17be84689c8fe7;True;0;False;white;Auto;False;Instance;9;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;78;2973.191,2642.719;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;86;3221.713,2839.199;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;87;3424.59,2956.97;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.WorldNormalVector;84;2918.351,2875.89;Inherit;False;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;83;2547.337,2795.548;Inherit;False;Property;_DisplacementAmount;Displacement Amount;29;0;Create;True;0;0;0;False;0;False;0.4;0.4;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.NormalVertexDataNode;85;3168.804,2949.801;Inherit;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ToggleSwitchNode;139;447.6996,975.2643;Inherit;False;Property;_PatternInvertBW;Pattern Invert  B/W;28;0;Create;True;0;0;0;False;0;False;1;True;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;140;204.7951,849.8411;Inherit;False;Constant;_Float0;Float 0;30;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;4012.767,2317.849;Float;False;True;-1;6;ASEMaterialInspector;0;0;Standard;SimpleWater;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Back;0;False;;0;False;;False;0;False;;0;False;;False;0;Transparent;0.5;True;False;0;False;Transparent;;Transparent;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;True;3;32;10;25;False;0.5;True;2;5;False;;10;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;28;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;17;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;16;FLOAT4;0,0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
Node;AmplifyShaderEditor.RangedFloatNode;112;3358.208,2408.004;Inherit;False;Property;_Smoothness;Smoothness;26;0;Create;True;0;0;0;False;0;False;0;0.857;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;141;3641.365,2628.316;Inherit;False;Property;_Tessellation;Tessellation;27;0;Create;True;0;0;0;False;0;False;4;4;0;0;0;1;FLOAT;0
WireConnection;59;0;70;0
WireConnection;61;0;60;0
WireConnection;62;0;61;0
WireConnection;62;2;67;0
WireConnection;63;0;61;0
WireConnection;63;1;64;0
WireConnection;64;0;65;0
WireConnection;64;1;68;0
WireConnection;65;0;62;0
WireConnection;65;1;66;0
WireConnection;69;0;63;0
WireConnection;69;1;71;0
WireConnection;72;0;73;0
WireConnection;73;0;59;0
WireConnection;73;1;69;0
WireConnection;114;0;116;0
WireConnection;115;0;114;0
WireConnection;41;0;39;0
WireConnection;41;1;35;0
WireConnection;41;2;42;0
WireConnection;45;0;44;0
WireConnection;101;0;5;0
WireConnection;101;1;102;0
WireConnection;46;0;101;0
WireConnection;46;2;43;0
WireConnection;46;1;45;0
WireConnection;39;0;46;0
WireConnection;39;1;40;0
WireConnection;104;0;6;0
WireConnection;103;0;104;0
WireConnection;103;1;105;0
WireConnection;5;0;6;0
WireConnection;107;20;103;0
WireConnection;107;110;108;0
WireConnection;106;0;5;0
WireConnection;106;1;107;40
WireConnection;120;0;107;40
WireConnection;15;0;16;0
WireConnection;125;0;123;0
WireConnection;20;0;106;0
WireConnection;20;2;124;0
WireConnection;20;1;125;0
WireConnection;18;0;20;0
WireConnection;18;1;19;0
WireConnection;13;0;106;0
WireConnection;13;2;14;0
WireConnection;13;1;15;0
WireConnection;7;0;13;0
WireConnection;7;1;8;0
WireConnection;30;0;106;0
WireConnection;30;2;32;0
WireConnection;30;1;34;0
WireConnection;28;0;30;0
WireConnection;28;1;29;0
WireConnection;34;0;33;0
WireConnection;17;1;18;0
WireConnection;55;0;41;0
WireConnection;55;1;56;0
WireConnection;55;2;72;0
WireConnection;118;0;55;0
WireConnection;9;1;7;0
WireConnection;134;0;139;0
WireConnection;134;2;27;1
WireConnection;134;3;133;0
WireConnection;133;0;27;1
WireConnection;136;0;17;1
WireConnection;138;0;9;1
WireConnection;35;0;134;0
WireConnection;35;1;24;0
WireConnection;35;2;36;0
WireConnection;24;0;135;0
WireConnection;24;1;10;0
WireConnection;24;2;25;0
WireConnection;135;0;139;0
WireConnection;135;2;17;1
WireConnection;135;3;136;0
WireConnection;10;0;11;0
WireConnection;10;1;12;0
WireConnection;10;2;137;0
WireConnection;137;0;139;0
WireConnection;137;2;9;1
WireConnection;137;3;138;0
WireConnection;27;1;28;0
WireConnection;78;0;39;0
WireConnection;78;1;83;0
WireConnection;86;0;78;0
WireConnection;86;1;84;2
WireConnection;87;0;86;0
WireConnection;87;1;85;0
WireConnection;139;0;140;0
WireConnection;0;0;119;0
WireConnection;0;1;110;0
WireConnection;0;3;111;0
WireConnection;0;4;112;0
WireConnection;0;9;115;0
WireConnection;0;11;87;0
WireConnection;0;14;141;0
ASEEND*/
//CHKSM=3AA1DEEE42CCE79CD8FB60C2B740A1B26AF8FB6E