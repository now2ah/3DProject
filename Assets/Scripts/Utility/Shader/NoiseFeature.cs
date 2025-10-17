using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

public class NoiseFeature : ScriptableRendererFeature
{
    class NoisePass : ScriptableRenderPass
    {
        const string m_PassName = "NoiseEffectPass";
        Material m_BlitMaterial;

        public void Setup(Material material)
        {
            m_BlitMaterial = material;
            requiresIntermediateTexture = true;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            var resourceData = frameData.Get<UniversalResourceData>();

            if (resourceData.isActiveTargetBackBuffer) { return; }

            var source = resourceData.activeColorTexture;

            var destinationDesc = renderGraph.GetTextureDesc(source);
            destinationDesc.name = $"CameraColor-{m_PassName}";
            destinationDesc.clearBuffer = false;

            TextureHandle destination = renderGraph.CreateTexture(destinationDesc);

            RenderGraphUtils.BlitMaterialParameters param = new(source, destination, m_BlitMaterial, 0);
            renderGraph.AddBlitPass(param, passName: m_PassName);

            resourceData.cameraColor = destination;
        }
    }

    public RenderPassEvent injectionPoint = RenderPassEvent.AfterRenderingPostProcessing;
    public Material material;

    NoisePass m_ScriptablePass;

    public override void Create()
    {
        m_ScriptablePass = new NoisePass();

        m_ScriptablePass.renderPassEvent = injectionPoint;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (material == null) { return; }

        m_ScriptablePass.Setup(material);
        renderer.EnqueuePass(m_ScriptablePass);
    }
}