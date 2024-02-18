Shader "Cavrnus/Internal/Particles/Unlit"
{
    Properties
    {
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}
        [MainColor]   _BaseColor("Base Color", Color) = (1,1,1,1)

        _Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5
        _BumpMap("Normal Map", 2D) = "bump" {}

        [HDR] _EmissionColor("Color", Color) = (0,0,0)
        _EmissionMap("Emission", 2D) = "white" {}

        // -------------------------------------
        // Particle specific
        _SoftParticlesNearFadeDistance("Soft Particles Near Fade", Float) = 0.0
        _SoftParticlesFarFadeDistance("Soft Particles Far Fade", Float) = 1.0
        _CameraNearFadeDistance("Camera Near Fade", Float) = 1.0
        _CameraFarFadeDistance("Camera Far Fade", Float) = 2.0
        _DistortionBlend("Distortion Blend", Float) = 0.5
        _DistortionStrength("Distortion Strength", Float) = 1.0

        // -------------------------------------
        // Hidden properties - Generic
        [HideInInspector] _Surface("__surface", Float) = 1.0
        [HideInInspector] _Blend("Blend Mode", Float) = 0.0
        [HideInInspector] _AlphaClip("Alpha Clip", Float) = 0.0
        [HideInInspector] _BlendOp("Blend Op", Float) = 0.0
        [HideInInspector] _SrcBlend("Src blend", Float) = 1.0
        [HideInInspector] _DstBlend("Dst Blend", Float) = 0.0
        [HideInInspector] _ZWrite("zwrite", Float) = 1.0
        [HideInInspector] _Cull("cull", Float) = 0.0
        // Particle specific
        [HideInInspector] _ColorMode("_ColorMode", Float) = 0.0
        [HideInInspector] _BaseColorAddSubDiff("_ColorMode", Vector) = (0,0,0,0)
        [ToggleOff] _FlipbookBlending("__flipbookblending", Float) = 0.0
        [HideInInspector] _SoftParticlesEnabled("Soft Particles", Float) = 0.0
        [HideInInspector] _SoftParticleFadeParams("Soft Particles Fade Params", Vector) = (0,0,0,0)
        [HideInInspector] _CameraFadingEnabled("Camera Fading ", Float) = 0.0
        [HideInInspector] _CameraFadeParams("Camera Fade Params", Vector) = (0,0,0,0)
        [HideInInspector] _DistortionEnabled("Distortion", Float) = 0.0
        [HideInInspector] _DistortionStrengthScaled("Distortion Strength Scaled", Float) = 0.1

        // Editmode props
        _QueueOffset("Queue offset", Float) = 0.0

        // ObsoleteProperties
        [HideInInspector] _FlipbookMode("flipbook", Float) = 0
        [HideInInspector] _Mode("mode", Float) = 0
        [HideInInspector] _Color("color", Color) = (1,1,1,1)
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
            "PreviewType" = "Plane"
            "PerformanceChecks" = "False"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "ForwardLit"

            BlendOp[_BlendOp], Add
            Blend[_SrcBlend][_DstBlend], OneMinusDstAlpha One
            ZWrite[_ZWrite]
            Cull[_Cull]
            ColorMask RGBA

            HLSLPROGRAM
            #pragma target 2.0

            #pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local_fragment _EMISSION

            #pragma shader_feature_local_fragment _ _ALPHAPREMULTIPLY_ON _ALPHAMODULATE_ON
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _ _COLOROVERLAY_ON _COLORCOLOR_ON _COLORADDSUBDIFF_ON
            #pragma shader_feature_local _FLIPBOOKBLENDING_ON
            #pragma shader_feature_local _SOFTPARTICLES_ON
            #pragma shader_feature_local _FADING_ON
            #pragma shader_feature_local _DISTORTION_ON

			#pragma shader_feature __ FOG_EXP2
            #pragma multi_compile_instancing
            #pragma instancing_options procedural:ParticleInstancingSetup

            #pragma vertex vertParticleUnlit
            #pragma fragment fragParticleUnlit

            #include "Packages/com.unity.render-pipelines.universal/Shaders/Particles/ParticlesUnlitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/Particles/ParticlesUnlitForwardPass.hlsl"

            ENDHLSL
        }

        //  Scene view outline pass.
        Pass
        {
            Name "SceneSelectionPass"
            Tags { "LightMode" = "SceneSelectionPass" }

            BlendOp Add
            Blend One Zero
            ZWrite On
            Cull Off

            HLSLPROGRAM
            #define PARTICLES_EDITOR_META_PASS
            #pragma target 2.0

            // -------------------------------------
            // Particle Keywords
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local _FLIPBOOKBLENDING_ON

            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile_instancing
            #pragma instancing_options procedural:ParticleInstancingSetup

            #pragma vertex vertParticleEditor
            #pragma fragment fragParticleSceneHighlight

            #include "Packages/com.unity.render-pipelines.universal/Shaders/Particles/ParticlesUnlitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/Particles/ParticlesEditorPass.hlsl"

            ENDHLSL
        }

        // ------------------------------------------------------------------
        //  Scene picking buffer pass.
        Pass
        {
            Name "ScenePickingPass"
            Tags{ "LightMode" = "Picking" }

            BlendOp Add
            Blend One Zero
            ZWrite On
            Cull Off

            HLSLPROGRAM
            #define PARTICLES_EDITOR_META_PASS
            #pragma target 2.0

            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local _FLIPBOOKBLENDING_ON

            #pragma multi_compile_instancing
            #pragma instancing_options procedural:ParticleInstancingSetup

            #pragma vertex vertParticleEditor
            #pragma fragment fragParticleScenePicking

            #include "Packages/com.unity.render-pipelines.universal/Shaders/Particles/ParticlesUnlitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/Particles/ParticlesEditorPass.hlsl"

            ENDHLSL
        }
    }
    CustomEditor "UnityEditor.Rendering.Universal.ShaderGUI.ParticlesUnlitShader"
}
