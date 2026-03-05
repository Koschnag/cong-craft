namespace CongCraft.Engine.Rendering;

/// <summary>
/// Systems that can render geometry into the shadow map implement this interface.
/// </summary>
public interface IShadowCaster
{
    void RenderShadowPass(ShadowMap shadowMap);
}
