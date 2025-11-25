using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PixelationFeature : ScriptableRendererFeature
{
    [Serializable]
    public class Settings
    {
        public Shader m_PixelationShader;
        public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    }
    
    public Settings settings = new Settings();
    
    private PixelationPass m_ScriptablePass;
    private Material m_PixelationMaterial;

    /// <summary>
    /// Render Featureの初期化
    /// Render Feature作成時に一度だけ呼ばれる
    /// </summary>
    public override void Create()
    {
        if (settings.m_PixelationShader != null)
        {
            m_PixelationMaterial = CoreUtils.CreateEngineMaterial(settings.m_PixelationShader);   
        }
        
        m_ScriptablePass = new PixelationPass(m_PixelationMaterial);
    }

    /// <summary>
    /// レンダーパスをキューに追加する
    /// </summary>
    /// <param name="renderer"></param>
    /// <param name="renderingData"></param>
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (m_PixelationMaterial == null || m_ScriptablePass == null)
        {
            Debug.LogWarningFormat("PixelationFeature: Missing material. {0} pass will not execute. Check for missing reference in the assigned renderer.", GetType().Name);
            return;
        }
        
        // VolumeManagerからVolume情報を取得
        var stack = VolumeManager.instance.stack;
        var customVolume = stack.GetComponent<PixelationVolume>();

        // Volumeがアクティブな場合のみパスをキューに追加
        if (customVolume != null && customVolume.IsActive())
        {
            // Volumeの値をPassに渡す
            m_ScriptablePass.Setup(customVolume);
            m_ScriptablePass.renderPassEvent = settings.renderPassEvent;
            renderer.EnqueuePass(m_ScriptablePass);
        }
    }

    protected override void Dispose(bool disposing)
    {
        CoreUtils.Destroy(m_PixelationMaterial);
        m_PixelationMaterial = null;
    }
}