using FxResources.System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PixelationVolume : VolumeComponent, IPostProcessComponent
{
    // Volumeでブレンド可能なColorパラメータ
    public ColorParameter m_Color = new ColorParameter(Color.white, true, false, true);

    // ０から１の範囲でクランプされ、ブレンド可能なFloatパラメータ
    public ClampedFloatParameter intensity = new ClampedFloatParameter(0.0f, 0.0f, 1.0f);
    
    // ピクセルの大きさを制御するパラメータ
    public ClampedFloatParameter pixelSize = new ClampedFloatParameter(100.0f, 1.0f, 512.0f);
    
    // ポスタリゼーション
    public ClampedFloatParameter posterizationLevels = new ClampedFloatParameter(512.0f, 2.0f, 1024.0f);

    // エフェクトがアクティブかどうか
    public bool IsActive() => intensity.value > 0.0f;

    // タイル互換性があるかどうか
    public bool IsTileCompatible() => true;
}