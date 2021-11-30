namespace Playcubegames {
    using System.Collections.Generic;
    using UnityEngine.Rendering.Universal;
    using UnityEngine.Rendering;
    using UnityEngine;
    /**
     * 1. render the two circle to one render texture 
     * 2. blur the render texture
     * 3. use the metaball shader 
     **/
    public class MetaballFeature : ScriptableRendererFeature {
        [System.Serializable]
        public class Settings {
            public string TextureName = "_MetaballTexture";
            public Material blurMat;

        }

        public class BlurPass : ScriptableRenderPass {
            Settings settings;
            RenderTargetHandle targetHandle;
            RenderTargetIdentifier source;

            public BlurPass (Settings settings) {
                this.settings = settings;
                renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
                this.targetHandle.Init (settings.TextureName);
            }

            public void Setup (RenderTargetIdentifier cameraTarget) {
                this.source = cameraTarget;
            }

            public override void Execute (ScriptableRenderContext context, ref RenderingData renderingData) {
                var cmd = CommandBufferPool.Get ();
                cmd.Clear ();
                cmd.name = "MetaballFeature";
                cmd.Blit (source, targetHandle.Identifier (), settings.blurMat);
                context.ExecuteCommandBuffer (cmd);
                CommandBufferPool.Release (cmd);
            }

            public override void Configure (CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor) {
                base.Configure (cmd, cameraTextureDescriptor);
                cmd.GetTemporaryRT (targetHandle.id, cameraTextureDescriptor);
                cmd.SetGlobalTexture (settings.TextureName, targetHandle.Identifier ());
            }

            public override void FrameCleanup (CommandBuffer cmd) {
                cmd.ReleaseTemporaryRT (targetHandle.id);
                base.FrameCleanup (cmd);
            }
        }

        BlurPass blurPass;

        [SerializeField] Settings settings;

        public override void AddRenderPasses (ScriptableRenderer renderer, ref RenderingData renderingData) {
            blurPass.Setup (renderer.cameraColorTarget);
            renderer.EnqueuePass (blurPass);
        }

        public override void Create () {
            blurPass = new BlurPass (settings);
        }

    }
}