using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DistortFeature : ScriptableRendererFeature {

    [System.Serializable]
    public class Settings {
        public string TextureName = "GrabPassTransparent";
        public LayerMask LayerMask;
    }

    class GrabPass : ScriptableRenderPass {

        RenderTargetHandle tempColorTarget;

        Settings settings;

        RenderTargetIdentifier cameraTarget;

        public GrabPass (Settings settings) {
            this.settings = settings;
            this.renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
            tempColorTarget.Init (this.settings.TextureName);
        }

        public void Setup (RenderTargetIdentifier cameraTarget) {
            this.cameraTarget = cameraTarget;
        }

        public override void Configure (CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor) {
            cmd.GetTemporaryRT (tempColorTarget.id, cameraTextureDescriptor);
            cmd.SetGlobalTexture (settings.TextureName, tempColorTarget.Identifier ());
        }

        public override void Execute (ScriptableRenderContext context, ref RenderingData renderingData) {
            CommandBuffer cmd = CommandBufferPool.Get ();
            cmd.Clear ();
            cmd.name = "GrabPassCmdBuffer";
            using (new ProfilingScope (cmd, new ProfilingSampler ("GrabPass"))) {
                Blit (cmd, cameraTarget, tempColorTarget.Identifier ());
            }
            context.ExecuteCommandBuffer (cmd);
            CommandBufferPool.Release (cmd);
        }

        public override void FrameCleanup (CommandBuffer cmd) {
            cmd.ReleaseTemporaryRT (tempColorTarget.id);
        }
    }

    class RenderPass : ScriptableRenderPass {
        Settings settings;
        List<ShaderTagId> shaderTagIdList = new List<ShaderTagId> ();
        FilteringSettings filteringSettings;
        RenderStateBlock renderStateBlock;

        public RenderPass (Settings settings) {
            this.settings = settings;
            this.renderPassEvent = RenderPassEvent.AfterRenderingTransparents + 1;
            shaderTagIdList.AddRange (new List<ShaderTagId> {
                new ShaderTagId ("SRPDefaultUnlit"),
                new ShaderTagId ("UniversalForward"),
                new ShaderTagId ("LightweightForward")
            });

            filteringSettings = new FilteringSettings (RenderQueueRange.all, settings.LayerMask);
            renderStateBlock = new RenderStateBlock (RenderStateMask.Nothing);
        }

        public override void Execute (ScriptableRenderContext context, ref RenderingData renderingData) {
            CommandBuffer cmd = CommandBufferPool.Get ();
            context.ExecuteCommandBuffer (cmd);
            cmd.Clear ();
            // cmd.name = "RenderPassCmdBuffer";
            // using (new ProfilingScope (cmd, new ProfilingSampler ("Renderer"))) {
            DrawingSettings drawingSettings = CreateDrawingSettings (shaderTagIdList, ref renderingData, SortingCriteria.CommonTransparent);
            context.DrawRenderers (renderingData.cullResults, ref drawingSettings, ref filteringSettings, ref renderStateBlock);
            // }
            context.ExecuteCommandBuffer (cmd);
            CommandBufferPool.Release (cmd);
        }
    }

    GrabPass grabPass;
    RenderPass renderPass;
    [SerializeField] Settings settings;

    public override void AddRenderPasses (ScriptableRenderer renderer, ref RenderingData renderingData) {
        grabPass.Setup (renderer.cameraColorTarget);
        renderer.EnqueuePass (grabPass);
        renderer.EnqueuePass (renderPass);

    }

    public override void Create () {
        grabPass = new GrabPass (settings);
        renderPass = new RenderPass (settings);
    }
}