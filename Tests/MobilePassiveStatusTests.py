"""Real passive HUD, active button mapping and ProcessPointer with managed draw/touch boundaries."""
from pathlib import Path
import os,sys,subprocess
root=Path(__file__).resolve().parents[1]
def adapt(f):
 start=f.index('public static class MobilePinnedTargetProductionTests');end=f.index('namespace Emberfall{internal static class ReturningCounterRules',start);f=f[:start]+f[end:]
 start=f.index(' public class GameUI{');end=f.index('\n',start);f=f[:start]+f[end:]
 f=f.replace('public static class Mathf{','public static class Mathf{public static int RoundToInt(float x)=>(int)System.Math.Round(x);')
 f=f.replace('public class GameSession{','public class GameSession{public string ControlFailure(string key)=>Failure;')
 return f
ui=r'''
using System;using System.Collections.Generic;using UnityEngine;using Emberfall;
namespace UnityEngine{public enum TextAnchor{MiddleCenter}}
namespace Emberfall{
 public partial class GameUI{
  GameSession session;enum Panel{None}Panel panel;float scale=1;Vector2 guiOffset;List<Rect> blockedRects=new List<Rect>();Rect[] hotbarSlots=new Rect[8];MobileSkillTap mobileTap=new MobileSkillTap();Color pale,muted;
  public bool LifecycleTouchBlocked,CompanionCommandsVisible;public void RefreshTouchViewport(){}public void ActivateFreeCommand(bool b){}public void ActivateMobileInteraction(int f){}
  float TouchRatio=>MobileControls.Layout.Scale/scale;Rect TouchRect(MobileControlLayout.Area a)=>new Rect(a.X*TouchRatio,a.Y*TouchRatio,a.Width*TouchRatio,a.Height*TouchRatio);int TouchFont(float s)=>Mathf.RoundToInt(s*TouchRatio);
  public bool TryBeginTouchSkill(int f,Vector2 p)=>BeginMobileCast(f,p);public void UpdateTouchSkill(int f,Vector2 p,bool ended,bool cancelled)=>ContinueMobileCast(f,p,ended,cancelled);
  public List<int> Identities=new List<int>();public List<bool> Learned=new List<bool>();public List<Rect> Glyphs=new List<Rect>();public List<string> Captions=new List<string>();
  void DrawSkillIdentity(Rect r,HeroClass hero,int skill,int rank,bool learned,int size){Identities.Add(skill);Learned.Add(learned);Glyphs.Add(r);}
  void Text(Rect r,string s,int font,Color c,bool bold,bool wrap,TextAnchor anchor){Captions.Add(s);if(font>r.height+1)throw new Exception("passive caption clipped");}
  public GameUI(GameSession s){session=s;guiOffset=new Vector2(MobileControls.SafeArea.x,Screen.height-MobileControls.SafeArea.yMax);for(int i=0;i<8;i++)hotbarSlots[i]=TouchRect(MobileControls.Layout.Skills[i]);}
  public void DrawPassives(){blockedRects.Clear();Identities.Clear();Learned.Clear();Glyphs.Clear();Captions.Clear();DrawMobilePassiveIdentities();}
  public int CapturedSkill=>mobileTap.Skill;public bool Captured=>mobileTap.Active;
 }
}
public static class MobilePassiveStatusTests{
 static int count;static void C(bool b,string m){count++;if(!b)throw new Exception(m);}
 static bool Shown(MobileControls c)=>(bool)typeof(MobileControls).GetField("hasJoystickOrigin",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).GetValue(c);
 public static string Run(){
  foreach(var size in new[]{(568f,320f,163f),(1440f,650f,320f),(2048f,1536f,264f)})foreach(int preset in new[]{-1,0,1})foreach(HeroClass kind in Enum.GetValues(typeof(HeroClass)))foreach(int rank in new[]{0,3}){
   Screen.width=size.Item1;Screen.height=size.Item2;MobileControls.SafeArea=new Rect(20,10,Screen.width-40,Screen.height-20);var l=MobileControls.Layout=new MobileControlLayout(Screen.width-40,Screen.height-20,size.Item3,preset);
   var game=new GameSession();var hero=new PlayerController(game);hero.HeroClass=kind;game.Progression.Profile.heroClass=kind;game.Progression.Profile.skillRanks[3]=game.Progression.Profile.skillRanks[8]=rank;
   var target=new EnemyController(8);game.Enemies.Add(target);hero.PinMobileTarget(target);var hud=new GameUI(game);var controls=new MobileControls(game,hud);MobileControls.ResetInput();hud.DrawPassives();float energy=hero.Energy;
   C(hud.Identities.Count==2&&hud.Identities[0]==3&&hud.Identities[1]==8,"two passive identities remain separate from eight active buttons");C(hud.Learned.TrueForAll(v=>v==(rank>0))&&hud.Captions.TrueForAll(v=>v==(rank>0?"被动":"未学")),"passive state reflects learned rank without promising a cast");
   for(int i=0;i<2;i++){
    var a=MobilePassiveStatusLayout.Indicator(i);C(a.X>=12&&a.Y>=12&&a.X+a.Width<=187&&a.Y+a.Height<=70,"passive indicators fit existing status card");
    C(!a.Overlaps(MobilePassiveStatusLayout.HealthBar)&&!a.Overlaps(MobilePassiveStatusLayout.EnergyBar)&&!a.Overlaps(l.CombatView),"passives preserve resources and clear central combat view");
    foreach(var skill in l.Skills)C(!a.Overlaps(skill),"passive state never overlaps active hitbox");
    foreach(var action in new[]{l.Attack,l.Jump,l.Dodge,l.Potion,l.FocusCommand,l.RecallCommand,l.AdventureStatus,l.EncounterText,l.BossHealth})C(!a.Overlaps(action),"passive state does not cover actions or objectives");
    var point=controls.Control(a);C(hud.IsScreenPointOverHUD(point),"real passive renderer registers HUD ownership");controls.ProcessPointer(90+i,TouchPhase.Began,point);controls.ProcessPointer(90+i,TouchPhase.Moved,new Vector2(point.x+80,point.y));
    C(!Shown(controls)&&MobileControls.Move.magnitude==0&&!MobileControls.AttackHeld&&!hud.Captured&&hero.MobilePinnedTarget==target&&hero.Energy==energy&&hero.Casts==0,"passive pointer cannot become joystick cast or world aim");controls.ProcessPointer(90+i,TouchPhase.Ended,point);
   }
   int[] expected={0,1,2,4,5,6,7,9};C(MobileSkillPolicy.ButtonCount==8&&l.Skills.Length==8,"eight active buttons retained");
   for(int i=0;i<8;i++){var point=controls.Control(l.Skills[i]);controls.ProcessPointer(100+i,TouchPhase.Began,point);C(hud.Captured&&hud.CapturedSkill==expected[i],"real pointer captures existing active mapping including ultimate at button seven");controls.ProcessPointer(100+i,TouchPhase.Canceled,point);C(!hud.Captured&&hero.Casts==0,"cancellation retains no cast or captured passive state");}
  }
  return "PASS "+count+" real passive renderer/geometry/active mapping/pointer assertions; managed GUI and input boundary";
 }
}
'''
def add_ui(p):
 mobile=(root/'Assets/Scripts/UI/GameUI.Mobile.cs').read_text();desktop=(root/'Assets/Scripts/UI/GameUI.cs').read_text()
 methods=''.join(extract('UI/GameUI.Mobile.cs',x) for x in ['private void DrawMobilePassiveIdentities(','private bool BeginMobileCast(','private void ContinueMobileCast(','public void CancelMobileCast('])
 methods+=''.join(extract('UI/GameUI.cs',x) for x in ['private Vector2 ScreenToUI(','public bool IsScreenPointOverHUD(','public bool IsScreenPointOverUI('])
 (p/'UI.cs').write_text(ui);(p/'UIMethods.cs').write_text('using UnityEngine;namespace Emberfall{public partial class GameUI{'+methods+'}}');(p/'MobilePassiveStatusLayout.cs').write_text((root/'Assets/Scripts/UI/MobilePassiveStatusLayout.cs').read_text())
 assert 'MobilePassiveStatusLayout.HealthBar' in mobile and 'MobilePassiveStatusLayout.EnergyBar' in mobile
 assert 'DrawMobilePassiveIdentities();' in extract('UI/GameUI.Mobile.cs','private void DrawMobileHotbar(')
 body=extract('UI/GameUI.Mobile.cs','private void DrawMobilePassiveIdentities(');assert 'Button(' not in body and 'mobileTap' not in body and 'IsSkillAvailable' not in body
harness=(root/'Tests/MobilePinnedTargetProductionTests.py').read_text().split(' env=dict')[0]
harness=harness.replace("(r/'Tests/MobilePinnedTargetProductionTests.cs').read_text()","adapt((r/'Tests/MobilePinnedTargetProductionTests.cs').read_text())").replace('MobilePinnedTargetProductionTests.Run()','MobilePassiveStatusTests.Run()')
harness+='''\n add_ui(p)
 env=dict(os.environ,DOTNET_CLI_HOME=str(p/'cli'),DOTNET_NOLOGO='1');build=[dotnet,'build',str(project),'--configfile',str(p/'NuGet.Config'),'-v:q'];run=[dotnet,str(p/'bin/Debug/net8.0/Test.dll')]
 subprocess.run(build,env=env,check=True);subprocess.run(run,env=env,check=True)
 target=p/'UIMethods.cs';good=target.read_text();assert 'blockedRects.Add(area);' in good;target.write_text(good.replace('blockedRects.Add(area);',''))
 subprocess.run(build,env=env,check=True,stdout=subprocess.DEVNULL);r=subprocess.run(run,env=env,capture_output=True,text=True);assert r.returncode and 'real passive renderer registers HUD ownership' in r.stdout+r.stderr,r.stdout+r.stderr
 print('PASS compiled negative: missing passive HUD blocker rejected')
'''
exec(compile(harness,__file__,'exec'))
