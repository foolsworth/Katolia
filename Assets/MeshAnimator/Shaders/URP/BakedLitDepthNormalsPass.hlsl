#ifndef UNIVERSAL_BAKEDLIT_DEPTH_NORMALS_PASS_INCLUDED
#define UNIVERSAL_BAKEDLIT_DEPTH_NORMALS_PASS_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
// BEGIN GENERATED MESH ANIMATOR CODE
#include "../MeshAnimator.hlsl"
// END GENERATED MESH ANIMATOR CODE

struct Attributes
{
    float4 positionOS   : POSITION;
    float2 uv           : TEXCOORD0;
    half3 normalOS      : NORMAL;
    half4 tangentOS     : TANGENT;

    UNITY_VERTEX_INPUT_INSTANCE_ID
    // BEGIN GENERATED MESH ANIMATOR CODE
    uint vertexId        : SV_VertexID;
    // END GENERATED MESH ANIMATOR CODE
};

struct Varyings
{
    float4 vertex       : SV_POSITION;
    float2 uv           : TEXCOORD0;
    half3 normalWS      : TEXCOORD1;

    #if defined(_NORMALMAP)
        half4 tangentWS : TEXCOORD3;
    #endif

    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

Varyings DepthNormalsVertex(Attributes input)
{
    Varyings output = (Varyings)0;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

    // BEGIN GENERATED MESH ANIMATOR CODE
    float3 animatedPosition;
    float3 animatedNormal;	
    ApplyMeshAnimationValues_float(
        input.positionOS.xyz,
        input.normalOS.xyz,
        UNITY_ACCESS_INSTANCED_PROP(Props, _AnimTimeInfo), 
        _AnimTextures,
        UNITY_ACCESS_INSTANCED_PROP(Props, _AnimInfo),
        UNITY_ACCESS_INSTANCED_PROP(Props, _AnimScalar), 
        UNITY_ACCESS_INSTANCED_PROP(Props, _CrossfadeAnimInfo), 
        UNITY_ACCESS_INSTANCED_PROP(Props, _CrossfadeAnimTimeInfo), 
        UNITY_ACCESS_INSTANCED_PROP(Props, _CrossfadeAnimScalar), 
        UNITY_ACCESS_INSTANCED_PROP(Props, _CrossfadeData),  
        input.vertexId,
        sampler_AnimTextures,
        animatedPosition,
        animatedNormal);
    
    input.positionOS.xyz = animatedPosition;
    
    // END GENERATED MESH ANIMATOR CODE

    VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
    output.vertex = vertexInput.positionCS;
    output.uv = TRANSFORM_TEX(input.uv, _BaseMap).xy;

    // normalWS and tangentWS already normalize.
    // this is required to avoid skewing the direction during interpolation
    // also required for per-vertex SH evaluation
    VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);
    output.normalWS = half3(normalInput.normalWS);
    #if defined(_NORMALMAP)
        real sign = input.tangentOS.w * GetOddNegativeScale();
        output.tangentWS = half4(normalInput.tangentWS.xyz, sign);
    #endif

    return output;
}

float4 DepthNormalsFragment(Varyings input) : SV_TARGET
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

    half4 texColor = (half4) SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv);
    half alpha = texColor.a * _BaseColor.a;
    AlphaDiscard(alpha, _Cutoff);

    #if defined(_GBUFFER_NORMALS_OCT)
        float3 normalWS = normalize(input.normalWS);
        float2 octNormalWS = PackNormalOctQuadEncode(normalWS);           // values between [-1, +1], must use fp32 on some platforms
        float2 remappedOctNormalWS = saturate(octNormalWS * 0.5 + 0.5);   // values between [ 0,  1]
        half3 packedNormalWS = PackFloat2To888(remappedOctNormalWS);      // values between [ 0,  1]
        return half4(packedNormalWS, 0.0);
    #else
        #if defined(_NORMALMAP)
            half3 normalTS = SampleNormal(input.uv, TEXTURE2D_ARGS(_BumpMap, sampler_BumpMap)).xyz;
            half sgn = input.tangentWS.w;      // should be either +1 or -1
            half3 bitangent = sgn * cross(input.normalWS.xyz, input.tangentWS.xyz);
            half3 normalWS = TransformTangentToWorld(normalTS, half3x3(input.tangentWS.xyz, bitangent, input.normalWS));
        #else
            half3 normalWS = input.normalWS;
        #endif

        return half4(NormalizeNormalPerPixel(normalWS), 0.0);
    #endif

}

#endif