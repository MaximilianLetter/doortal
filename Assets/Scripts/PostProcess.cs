using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PostProcess : MonoBehaviour
{
    public Material effectMaterial;
    public RenderTexture stencilTex;
    //public Material stencilMaterial;
    //public Material simpleRender;

    //private RenderTexture cameraRenderTexture;
    //private RenderTexture buffer;

    //private void Start()
    //{
    //    GetComponent<Camera>().depthTextureMode = DepthTextureMode.Depth;
    //}

    //private void OnRenderImage(RenderTexture source, RenderTexture destination)
    //{
    //    RenderTexture buffer = RenderTexture.GetTemporary(source.width, source.height, 24);
    //    Graphics.Blit(source, buffer, effectMaterial);
    //    Graphics.Blit(buffer, destination);
    //    RenderTexture.ReleaseTemporary(buffer);

    //    //Graphics.Blit(source, destination, effectMaterial);
    //}

    public void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        //effectMaterial.
        effectMaterial.SetTexture("_StencilTex", stencilTex);
        Graphics.Blit(source, destination, effectMaterial);
    }

    //private void OnPostRender()
    //{
    //    Graphics.SetRenderTarget(buffer);
    //    GL.Clear(true, true, Color.black);

    //    Graphics.SetRenderTarget(buffer.colorBuffer, cameraRenderTexture.depthBuffer);
    //    Graphics.Blit(cameraRenderTexture, simpleRender);
    //    Graphics.Blit(cameraRenderTexture, effectMaterial);

    //    RenderTexture.active = null;
    //    Graphics.Blit(buffer, simpleRender);
    //}
}


// NOTES: does nothing but could be kind of workaround
//public class PostProcess : MonoBehaviour
//{
//    public Material effectMaterial;
//    private CommandBuffer commandBuffer;

//    private Texture2D m_Texture;
//    private Texture2D texture
//    {
//        get
//        {
//            if (m_Texture == null)
//            {
//                m_Texture = new Texture2D(16, 16);
//            }

//            return m_Texture;
//        }
//    }

//    private Camera m_Camera;
//    public Camera camera
//    {
//        get
//        {
//            if (m_Camera == null)
//            {
//                m_Camera = GetComponent<Camera>();
//            }

//            return m_Camera;
//        }
//    }

//    private void Start()
//    {
//        if (commandBuffer == null)
//        {
//            commandBuffer = new CommandBuffer();
//            commandBuffer.name = "commandBuffer";

//            int stencilTextureID = Shader.PropertyToID("_StencilTexture");
//            commandBuffer.GetTemporaryRT(stencilTextureID, -1, -1, 24);

//            commandBuffer.Blit(BuiltinRenderTextureType.None, stencilTextureID, effectMaterial);

//            commandBuffer.SetGlobalTexture("_StencilTexture", stencilTextureID);
//            camera.AddCommandBuffer(CameraEvent.AfterForwardAlpha, commandBuffer);
//        }
//    }

//    private void OnRenderImage(RenderTexture source, RenderTexture destination)
//    {
//        RenderTexture stencilRT = Shader.GetGlobalTexture("_StencilTexture") as RenderTexture;
//        RenderTexture activeRT = RenderTexture.active;
//        RenderTexture.active = stencilRT;

//        texture.ReadPixels(new Rect(Screen.width / 2, Screen.height / 2, texture.width, texture.height), 0, 0, false);
//        //Debug.Log (texture.GetPixel(0, 0));

//        RenderTexture.active = activeRT;

//        Graphics.Blit(source, destination);
//    }
//}
