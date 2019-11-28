using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace SafeAreas
{
    public class SafeAreaFeature : UnityEngine.Rendering.Universal.ScriptableRendererFeature
    {
        private SafeAreaPass m_SafeAreaPass;
        public SafeAreaSettings settings = new SafeAreaSettings();
        
        [System.Serializable]
        public class SafeAreaSettings
        {
            public Material material;
            public bool enabled;
            public RenderPassEvent Event = RenderPassEvent.AfterRenderingOpaques;
        }
        
        public override void Create()
        {
            m_SafeAreaPass = new SafeAreaPass(settings.Event);
            m_SafeAreaPass.m_material = settings.material;
            m_SafeAreaPass.m_enabled = settings.enabled;
        }

        public override void AddRenderPasses(UnityEngine.Rendering.Universal.ScriptableRenderer renderer, ref UnityEngine.Rendering.Universal.RenderingData renderingData)
        {
            renderer.EnqueuePass(m_SafeAreaPass);
        }
    }

    public class SafeAreaPass : UnityEngine.Rendering.Universal.ScriptableRenderPass
    {
        const string k_RenderTag = "SafeArea Overlay";
        public Material m_material;
        public bool m_enabled;
        public Mesh m_mesh;
        Matrix4x4 viewproj;
        Matrix4x4[] matrices;
        
        public SafeAreaPass(RenderPassEvent renderPassEvent) {
            this.renderPassEvent = renderPassEvent;
            this.m_mesh = UnityEngine.Rendering.Universal.RenderingUtils.fullscreenMesh;
            this.matrices = new Matrix4x4[10];
        }

        public override void Execute(ScriptableRenderContext context, ref UnityEngine.Rendering.Universal.RenderingData renderingData)
        {
            if  (m_enabled) {
                if (m_material == null) {
                    return;
                }
                if (m_mesh == null) {
                    return;
                }

                CommandBuffer cmd = CommandBufferPool.Get(k_RenderTag);
                
                viewproj = Matrix4x4.TRS(Vector3.zero,Quaternion.identity,new Vector3(1.0f,1.0f,1.0f)); //set to 1x1 pixel ratio
                cmd.SetViewProjectionMatrices(Matrix4x4.identity,viewproj);

                matrices[0]= Matrix4x4.TRS(new Vector3( 0f, 0.3333f,0),Quaternion.identity,new Vector3(1,2.0f / Screen.height,1));
                matrices[1]= Matrix4x4.TRS(new Vector3( 0f,-0.3333f,0),Quaternion.identity,new Vector3(1,2.0f / Screen.height,1));
                matrices[2]= Matrix4x4.TRS(new Vector3( 0f, 0.6666f,0),Quaternion.identity,new Vector3(1,2.0f / Screen.height,1));
                matrices[3]= Matrix4x4.TRS(new Vector3( 0f,-0.6666f,0),Quaternion.identity,new Vector3(1,2.0f / Screen.height,1));
                matrices[4]= Matrix4x4.TRS(new Vector3( 0.33333f,0f,0),Quaternion.identity,new Vector3(2.0f / Screen.width,1,1));
                matrices[5]= Matrix4x4.TRS(new Vector3(-0.33333f,0f,0),Quaternion.identity,new Vector3(2.0f / Screen.width,1,1));
                matrices[6]= Matrix4x4.TRS(new Vector3( 0.66666f,0f,0),Quaternion.identity,new Vector3(2.0f / Screen.width,1,1));
                matrices[7]= Matrix4x4.TRS(new Vector3(-0.66666f,0f,0),Quaternion.identity,new Vector3(2.0f / Screen.width,1,1));

                matrices[8]= Matrix4x4.TRS(new Vector3( 0.0f,0.0f,0.0f),Quaternion.identity,new Vector3(2.0f / Screen.width,1,1));
                matrices[9]= Matrix4x4.TRS(new Vector3( 0.0f,0.0f,0.0f),Quaternion.identity,new Vector3(1,2.0f / Screen.height,1));

                cmd.DrawMeshInstanced(m_mesh, 0, m_material,0,matrices,10);
                cmd.SetViewProjectionMatrices(renderingData.cameraData.camera.worldToCameraMatrix, renderingData.cameraData.camera.projectionMatrix);
                
                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
            }
        }
    }
}