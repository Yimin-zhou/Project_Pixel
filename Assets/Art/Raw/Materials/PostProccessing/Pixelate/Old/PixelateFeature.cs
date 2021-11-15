using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PixelateFeature : ScriptableRendererFeature
{
    class PixelateRenderPass : ScriptableRenderPass
    {
        private RenderTargetIdentifier source;
        private RenderTargetHandle tempTexture;
        private PixelateVolume pixelateProperties;
        private FilterMode filterMode;
        private RenderObjects.FilterSettings filteringSettings;
        private RenderTexture rt;

        public PixelateRenderPass(FilterMode filterMode,RenderObjects.FilterSettings filteringSettings)
        {
            this.filterMode = filterMode;
            this.filteringSettings = filteringSettings;
            
            tempTexture.Init("_TempPixelatedTexture");
        }

        public void SetUp(RenderTargetIdentifier source)
        {
            
            this.source = source;
        }
        
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            //找到PixelateVolume
            var stack = VolumeManager.instance.stack;
            pixelateProperties = stack.GetComponent<PixelateVolume>();
            if (pixelateProperties == null || !pixelateProperties.IsActive()) { return; }
            
            CommandBuffer cmd = CommandBufferPool.Get("PixelatedFeature");
            RenderTextureDescriptor cameraTextureDesc = renderingData.cameraData.cameraTargetDescriptor;
            int pixelateWidth = cameraTextureDesc.width / pixelateProperties.pixelatePower.value;
            int pixelateHeight = cameraTextureDesc.height / pixelateProperties.pixelatePower.value;
            cmd.GetTemporaryRT(tempTexture.id,pixelateWidth,pixelateHeight,24,FilterMode.Point,RenderTextureFormat.ARGB32,RenderTextureReadWrite.Default,1);
            cmd.Blit(source,tempTexture.Identifier());
            cmd.Blit(tempTexture.Identifier(),source);
            
            //执行和释放
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);       
        }
        
        public override void FrameCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(tempTexture.id);
        }
    }

    public RenderPassEvent passEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    public FilterMode filterMode = FilterMode.Point;
    public RenderObjects.FilterSettings filterSettings;
    private PixelateRenderPass m_ScriptablePass;

    public override void Create()
    {
        m_ScriptablePass = new PixelateRenderPass(filterMode,filterSettings);
        m_ScriptablePass.renderPassEvent = passEvent;
    }
    
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        m_ScriptablePass.SetUp(renderer.cameraColorTarget);
        renderer.EnqueuePass(m_ScriptablePass);
    }
}


