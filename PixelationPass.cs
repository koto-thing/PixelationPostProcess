using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

public class PixelationPass : ScriptableRenderPass
{
    /// <summary>
    /// レンダリングパスで使用するデータ
    /// </summary>
    public class PassData
    {
        internal TextureHandle source;
        internal Material material;
        internal Color pixelationColor;
        internal float intensity;
        internal float pixelSize;
        internal float posterizationLevel;
    }
    
    private PixelationVolume m_Volume;
    private readonly Material m_Material;
    private readonly ProfilingSampler m_ProfilingSampler;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="settings">レンダリングのフィーチャー</param>
    public PixelationPass(Material material)
    {
        m_Material = material;
        m_ProfilingSampler = new ProfilingSampler(nameof(PixelationPass));
    }

    public void Setup(PixelationVolume volume)
    {
        m_Volume = volume;
    }

    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        if (m_Material == null || m_Volume == null)
            return;
        
        m_Material.SetFloat("_Intensity", m_Volume.intensity.value);
        m_Material.SetColor("_PixelationColor", m_Volume.m_Color.value);
        m_Material.SetFloat("_PixelSize", m_Volume.pixelSize.value);
        m_Material.SetFloat("_PosterizationLevels", m_Volume.posterizationLevels.value);

        var urpResources = frameData.Get<UniversalResourceData>();
        var cameraColor = urpResources.activeColorTexture;

        var tempDesc = renderGraph.GetTextureDesc(cameraColor);
        tempDesc.name = "PixelationTempTexture";
        var tempTexture = renderGraph.CreateTexture(tempDesc);

        using (var builder = renderGraph.AddRasterRenderPass<PassData>("Pixelation Effect", out var passData, m_ProfilingSampler))
        {
            passData.material = m_Material;
            passData.source = cameraColor;
            
            builder.UseTexture(passData.source, AccessFlags.Read);
            builder.SetRenderAttachment(tempTexture, 0, AccessFlags.Write);

            builder.SetRenderFunc((PassData data, RasterGraphContext ctx) =>
            {
                Blitter.BlitTexture(ctx.cmd, data.source, new Vector4(1, 1, 0, 0), data.material, 0);
            });
        }

        using (var builder = renderGraph.AddRasterRenderPass<PassData>("Copy to Camera Target", out var passData, m_ProfilingSampler))
        {
            passData.source = tempTexture;
            builder.UseTexture(passData.source, AccessFlags.Read);
            builder.SetRenderAttachment(cameraColor, 0, AccessFlags.Write);

            builder.SetRenderFunc((PassData data, RasterGraphContext ctx) =>
            {
                Blitter.BlitTexture(ctx.cmd, data.source, new Vector4(1, 1, 0, 0), 0.0f, false);
            });
        }
    }
}