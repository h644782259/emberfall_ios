using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Emberfall.Editor
{
    // Actual Unity camera captures. Isolated saves and deterministic effect ages make
    // the previous and sculpted presentations directly comparable in the same scene.
    [InitializeOnLoad]
    public static class ElementalistVfxPreview
    {
        const string Key="Emberfall.ElementalistPreview.";
        const string SaveKey="Emberfall.ValidationSaveDirectory";
        const BindingFlags Private=BindingFlags.NonPublic|BindingFlags.Instance;
        static bool working;
        static ElementalistVfxPreview(){EditorApplication.update+=Tick;}
        public static void Run()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Stop Play Mode before previewing.");
            string output=Path.GetFullPath(Path.Combine(Application.dataPath,"../ArtSource/Review/Elementalist-20261010"));
            Directory.CreateDirectory(Path.Combine(output,"IsolatedSave"));
            SessionState.SetString(Key+"Output",output);
            SessionState.SetString(Key+"PreviousSave",SessionState.GetString(SaveKey,""));
            SessionState.SetString(SaveKey,Path.Combine(output,"IsolatedSave"));
            SessionState.SetBool(Key+"Active",true);
            EditorSceneManager.OpenScene("Assets/Scenes/Main.unity",OpenSceneMode.Single);
            EditorApplication.isPlaying=true;
        }
        static void Tick()
        {
            if(!SessionState.GetBool(Key+"Active",false)||!EditorApplication.isPlaying||working)return;
            var game=GameSession.Instance;if(game==null||game.Progression==null)return;
            working=true;
            try
            {
                if(SystemInfo.graphicsDeviceType==UnityEngine.Rendering.GraphicsDeviceType.Null)throw new InvalidOperationException("Preview needs a real graphics device.");
                UnityEngine.Random.InitState(61453);game.StartNew(HeroClass.Arcanist);
                game.SetPaused(false);
                foreach(var enemy in game.Enemies)if(enemy!=null){enemy.gameObject.SetActive(false);UnityEngine.Object.Destroy(enemy.gameObject);}game.Enemies.Clear();
                game.Player.Teleport(WorldTraversal.NearestWalkable(new Vector3(5,0,5),.45f));
                Camera camera=Camera.main;camera.GetComponent<AdventureCamera>().Snap();
                game.SetPaused(true);
                Vector3 at=game.Player.transform.position+Vector3.forward*3;
                // Preserve normal game-camera orientation and projection; center the comparison.
                camera.transform.position+=at-game.Player.transform.position;
                string output=SessionState.GetString(Key+"Output","");
                foreach(var kind in new[]{FilledVfxKind.Ice,FilledVfxKind.Fire,FilledVfxKind.Lightning})
                foreach(bool sculpted in new[]{false,true})
                {
                    Color tint=kind==FilledVfxKind.Ice?new Color(.51f,.92f,1):kind==FilledVfxKind.Fire?new Color(1,.59f,.28f):new Color(.77f,.48f,1);
                    var effectType=typeof(PlayerController).Assembly.GetType("Emberfall.FilledSkillVfx",true);
                    effectType.GetMethod("Impact",BindingFlags.Public|BindingFlags.Static).Invoke(null,new object[]{game.Player,at,3.7f,kind,tint,CombatVisualPriority.ActionBody,0,sculpted});
                    var effects=UnityEngine.Object.FindObjectsByType(effectType,FindObjectsSortMode.None);
                    if(effects.Length!=1)throw new InvalidOperationException("Expected one isolated effect, got "+effects.Length);
                    var fx=effects[0];
                    for(int frame=0;frame<24;frame++)
                    {
                        float age=frame/24f*(kind==FilledVfxKind.Fire?.8f:1.05f);
                        effectType.GetField("age",Private).SetValue(fx,age);
                        var pieces=(Array)effectType.GetField("pieces",Private).GetValue(fx);
                        int count=(int)effectType.GetField("count",Private).GetValue(fx);
                        for(int i=0;i<count;i++)effectType.GetMethod("Animate",Private).Invoke(fx,new[]{pieces.GetValue(i)});
                        Capture(camera,Path.Combine(output,kind+"-"+(sculpted?"After":"Before")+"-"+frame.ToString("D2")+".png"));
                    }
                    effectType.GetMethod("Retire",Private).Invoke(fx,null);
                }
                File.WriteAllText(Path.Combine(output,"capture.txt"),"Actual Unity "+Application.unityVersion+" Camera.Render; graphics="+SystemInfo.graphicsDeviceType+"; normal gameplay camera projection; direct production Impact replay, 24 sampled ages per element, previous and sculpted path; isolated saves. This is effect rendering evidence, not full skill interaction or device performance acceptance.");
                Finish(0);
            }
            catch(Exception error){Debug.LogException(error);Finish(1);}
        }
        static void Capture(Camera camera,string path)
        {
            var oldTarget=camera.targetTexture;var oldActive=RenderTexture.active;float oldAspect=camera.aspect;
            var render=new RenderTexture(1280,720,24,RenderTextureFormat.ARGB32){antiAliasing=2};
            var texture=new Texture2D(1280,720,TextureFormat.RGB24,false);
            try{camera.aspect=1280f/720;camera.targetTexture=render;camera.Render();RenderTexture.active=render;texture.ReadPixels(new Rect(0,0,1280,720),0,0);texture.Apply();File.WriteAllBytes(path,texture.EncodeToPNG());}
            finally{camera.targetTexture=oldTarget;camera.aspect=oldAspect;RenderTexture.active=oldActive;render.Release();UnityEngine.Object.DestroyImmediate(render);UnityEngine.Object.DestroyImmediate(texture);}
        }
        static void Finish(int code)
        {
            SessionState.SetBool(Key+"Active",false);SessionState.SetString(SaveKey,SessionState.GetString(Key+"PreviousSave",""));
            EditorApplication.isPlaying=false;
            if(Application.isBatchMode)EditorApplication.Exit(code);
        }
    }
}
