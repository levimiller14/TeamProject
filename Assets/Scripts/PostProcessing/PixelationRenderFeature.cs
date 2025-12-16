using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

public class PixelationRenderFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;

        [Range(1f, 32f)]
        public float pixelSize = 4f;

        public Material material;
    }

    public Settings settings = new Settings();

    PixelationPass pixelationPass;

    public override void Create()
    {
        pixelationPass = new PixelationPass(settings);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (settings.material == null)
            return;

        if (renderingData.cameraData.cameraType == CameraType.Preview)
            return;

        if (!Application.isPlaying)
            return;

        renderer.EnqueuePass(pixelationPass);
    }

    protected override void Dispose(bool disposing)
    {
        pixelationPass?.Dispose();
    }

    class PixelationPass : ScriptableRenderPass
    {
        Settings settings;

        static readonly int pixelSizeId = Shader.PropertyToID("_PixelSize");
        static readonly int sourceSizeId = Shader.PropertyToID("_SourceSize");

        class PassData
        {
            public TextureHandle source;
            public TextureHandle destination;
            public Material material;
            public float pixelSize;
            public Vector4 sourceSize;
        }

        public PixelationPass(Settings settings)
        {
            this.settings = settings;
            renderPassEvent = settings.renderPassEvent;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            if (settings.material == null)
                return;

            var resourceData = frameData.Get<UniversalResourceData>();
            var cameraData = frameData.Get<UniversalCameraData>();

            var descriptor = cameraData.cameraTargetDescriptor;
            descriptor.depthBufferBits = 0;

            var source = resourceData.activeColorTexture;

            var destinationDesc = renderGraph.GetTextureDesc(source);
            destinationDesc.name = "_PixelationTempRT";
            destinationDesc.clearBuffer = false;

            var destination = renderGraph.CreateTexture(destinationDesc);

            using (var builder = renderGraph.AddRasterRenderPass<PassData>("Pixelation Pass", out var passData))
            {
                passData.source = source;
                passData.destination = destination;
                passData.material = settings.material;
                passData.pixelSize = settings.pixelSize;
                passData.sourceSize = new Vector4(
                    1f / descriptor.width,
                    1f / descriptor.height,
                    descriptor.width,
                    descriptor.height
                );

                builder.UseTexture(source, AccessFlags.Read);
                builder.SetRenderAttachment(destination, 0, AccessFlags.Write);

                builder.SetRenderFunc((PassData data, RasterGraphContext ctx) =>
                {
                    data.material.SetFloat(pixelSizeId, data.pixelSize);
                    data.material.SetVector(sourceSizeId, data.sourceSize);
                    Blitter.BlitTexture(ctx.cmd, data.source, new Vector4(1, 1, 0, 0), data.material, 0);
                });
            }

            using (var builder = renderGraph.AddRasterRenderPass<PassData>("Pixelation Copy Back", out var passData))
            {
                passData.source = destination;
                passData.destination = source;

                builder.UseTexture(destination, AccessFlags.Read);
                builder.SetRenderAttachment(source, 0, AccessFlags.Write);

                builder.SetRenderFunc((PassData data, RasterGraphContext ctx) =>
                {
                    Blitter.BlitTexture(ctx.cmd, data.source, new Vector4(1, 1, 0, 0), 0, false);
                });
            }
        }

        public void Dispose()
        {
        }
    }
}
