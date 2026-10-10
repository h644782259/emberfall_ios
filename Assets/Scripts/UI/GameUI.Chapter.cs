using UnityEngine;
namespace Emberfall
{
    public sealed partial class GameUI
    {
        private Vector2 chapterScroll,chapterResultScroll;
        private object chapterResultScrollOwner;
        private bool chapterStoryExpanded;
        private GameProfile chapterSelectionOwner;
        private string chapterEntryError;
        private bool ChapterSelectionIsCurrent()
        {return (panel==Panel.Chapter||session.DungeonSelectionOpen&&adventureChapterSelected)&&session.OpenChapterSelectionAllowed&&ReferenceEquals(chapterSelectionOwner,session.Progression.Profile);}
        private bool OpenChapterSelection()
        {
            if(UITransitionBlocked||!session.OpenChapterSelectionAllowed)return false;
            chapterSelectionOwner=session.Progression.Profile;chapterEntryError=null;chapterScroll=Vector2.zero;chapterStoryExpanded=false;
            if(!ChapterProgression.IsUnlocked(chapterSelectionOwner,session.SelectedChapterNode))session.SelectedChapterNode=ChapterNode.ForestCourt;
            if(!ChapterProgression.CanEnter(chapterSelectionOwner,session.SelectedChapterNode,session.SelectedChapterDifficulty))session.SelectedChapterDifficulty=ChapterDifficulty.Normal;
            session.SelectedChapterTier=Mathf.Clamp(session.SelectedChapterTier,1,session.Progression.UnlockedChapterTier(session.SelectedChapterNode));
            CancelHotbarPointer();CancelMobileScroll();panel=Panel.Chapter;session.SetUIBlocking(true);BlockUITransition();return true;
        }
        private bool SelectChapterNode(ChapterNode node)
        {
            if(!ChapterSelectionIsCurrent()||!ChapterProgression.IsUnlocked(chapterSelectionOwner,node))return false;
            session.SelectedChapterNode=node;session.SelectedChapterDifficulty=ChapterDifficulty.Normal;session.SelectedChapterTactic=-1;
            chapterScroll=Vector2.zero;chapterStoryExpanded=false;chapterEntryError=null;CancelMobileScroll();return true;
        }
        private bool SelectChapterDifficulty(ChapterDifficulty difficulty)
        {
            if(!ChapterSelectionIsCurrent()||!ChapterProgression.CanEnter(chapterSelectionOwner,session.SelectedChapterNode,difficulty))return false;
            session.SelectedChapterDifficulty=difficulty;chapterEntryError=null;return true;
        }
        private void ChangeChapterTier(int delta)
        {
            if(!ChapterSelectionIsCurrent())return;
            session.SelectedChapterTier=Mathf.Clamp(session.SelectedChapterTier+(delta<0?-1:delta>0?1:0),1,session.Progression.UnlockedChapterTier(session.SelectedChapterNode));
        }
        private void SetChapterLimitedHealing(bool limited)
        {if(!ChapterSelectionIsCurrent())return;session.SelectedChapterLimitedHealing=limited;}
        private bool ConfirmSelectedChapter()
        {
            if(!ChapterSelectionIsCurrent()||!ChapterProgression.CanEnter(chapterSelectionOwner,session.SelectedChapterNode,session.SelectedChapterDifficulty))return false;
            if(!session.ConfirmChapterEnter())
            {chapterEntryError=string.IsNullOrEmpty(session.Progression.LastError)?"暂时无法进入，请确认营地状态后重试。":session.Progression.LastError;BlockUITransition();return false;}
            panel=Panel.None;chapterSelectionOwner=null;chapterEntryError=null;CancelMobileScroll();session.SetUIBlocking(false);BlockUITransition();return true;
        }
        private bool CloseChapterSelection()
        {
            if(panel!=Panel.Chapter)return false;
            panel=Panel.None;chapterSelectionOwner=null;chapterEntryError=null;CancelMobileScroll();session.SetUIBlocking(false);BlockUITransition();return true;
        }
        private bool RetryChapterSettlement()
        {if(!session.ChapterFinished||!session.ChapterRewardPending)return false;bool saved=session.TrySettleChapterReward();BlockUITransition();return saved;}
        private void ReturnFromChapter()
        {if(!session.ChapterFinished)return;if(session.IsDead)session.Respawn();else session.ReturnToCamp();BlockUITransition();}
        private bool ReturnAndSelectNextChapter()
        {
            if(!session.ChapterFinished||session.ChapterRewardPending||session.ChapterRun.Failed)return false;
            int next=(int)session.ActiveChapterNode+1;
            if(next>=3||!ChapterProgression.IsUnlocked(session.Progression.Profile,(ChapterNode)next))return false;
            session.ReturnToCamp();
            if(!session.OpenChapterSelectionAllowed){BlockUITransition();return false;}
            session.SelectedChapterNode=(ChapterNode)next;session.SelectedChapterDifficulty=ChapterDifficulty.Normal;
            return OpenChapterSelection();
        }
        private void DrawChapterSelection()
        {
            if(!ChapterSelectionIsCurrent()){CloseChapterSelection();return;}
            bool mobile=MobileControls.Active;float u=mobile?TouchRatio:1;
            var layout=ChapterPanelGeometry();
            DrawChapterFrame(layout,u,"星路纪事","三段线索 · 可跳读，已解锁节点可独立重玩");
            var profile=session.Progression.Profile;var node=session.SelectedChapterNode;var difficulty=session.SelectedChapterDifficulty;
            // Node identity and completion remain visible while only details scroll.
            float cardWidth=(layout.Body.Width-16)/3;
            for(int i=0;i<3;i++)
            {
                var choice=(ChapterNode)i;bool unlocked=ChapterProgression.IsUnlocked(profile,choice);
                int highest=ChapterProgression.HighestCompletedDifficulty(profile,choice);
                Rect card=ChapterRect(new MobilePanelLayout.Area(layout.Body.X+i*(cardWidth+8),layout.Body.Y,cardWidth,48),u);
                string label=ChapterDefinition.Get(choice).Name+(choice==node?" · 当前":unlocked?"":" · 未解锁");
                if(Button(card,label,choice==node?gold:jade,unlocked)){SelectChapterNode(choice);return;}
                DrawChapterSymbol(new Rect(card.x,card.yMax+4*u,12*u,12*u),choice,choice==node?gold:muted);
                Text(new Rect(card.x+18*u,card.yMax+2*u,card.width-18*u,16*u),highest<0?"尚未通关":"最高通关 · "+ChapterEntryPresentation.DifficultyName((ChapterDifficulty)highest),Mathf.RoundToInt(11*u),highest<0?muted:jade);
                for(int d=0;d<3;d++)
                    Text(new Rect(card.x+d*card.width/3,card.yMax+19*u,card.width/3,16*u),ChapterEntryPresentation.DifficultyName((ChapterDifficulty)d)+(highest>=d?" ✓":" ·"),Mathf.RoundToInt(10*u),highest>=d?jade:muted);
            }
            Rect body=ChapterRect(new MobilePanelLayout.Area(layout.Body.X,layout.Body.Y+88,layout.Body.Width,layout.Body.Height-88),u);
            DrawChapterEntryDetails(body,u);
            if(Button(ChapterRect(layout.FooterButton(0,2),u),"返回副本选择",jade)){CloseChapterSelection();session.EnterDungeon();return;}
            if(Button(ChapterRect(layout.FooterButton(1,2),u),"进入 "+ChapterDefinition.Get(node).Name,gold,ChapterProgression.CanEnter(profile,node,difficulty)))
            {ConfirmSelectedChapter();return;}
        }
        private void DrawChapterEntryDetails(Rect body,float u)
        {
            var profile=session.Progression.Profile;var node=session.SelectedChapterNode;
            int level=AdventureRewardRules.DungeonLevel(session.SelectedChapterTier),mode=(int)node;
            float w=body.width/u-18;
            string mechanic=ChapterDefinition.Get(node).Mechanic+"\n"+ChapterDefinition.DifficultyMechanic(node,session.SelectedChapterDifficulty);
            float descriptionHeight=Style(Mathf.RoundToInt(14*u),false,true).CalcHeight(new GUIContent(mechanic),(w-24)*u)/u+20;
            float rewardsHeight=DrawEntryRewardPreviews(w,u,mode,session.SelectedChapterTier,true,false);
            float imageHeight=Mathf.Clamp(w*.5f,96,MobileControls.Active?160:216),descriptionY=56+imageHeight;
            float total=descriptionY+descriptionHeight+rewardsHeight+48;
            chapterScroll=BeginTouchScroll("chapter-entry",body,chapterScroll,new Rect(0,0,w*u,Mathf.Max(body.height,total*u)));
            DrawChapterSymbol(new Rect(12*u,14*u,28*u,28*u),node,gold);
            Text(new Rect(50*u,8*u,(w-58)*u,30*u),ChapterDefinition.Get(node).Name,Mathf.RoundToInt(20*u),pale,true);
            DrawDungeonEntryArtwork(new Rect(12*u,44*u,(w-24)*u,imageHeight*u),5+(int)node);
            Text(new Rect(12*u,descriptionY*u,(w-24)*u,descriptionHeight*u),mechanic,Mathf.RoundToInt(14*u),muted,false,true);
            float y=descriptionY+descriptionHeight;
            entryRewardViewport=body;entryRewardContentOrigin=new Vector2(body.x-chapterScroll.x,body.y+y*u-chapterScroll.y);
            GUI.BeginGroup(new Rect(0,y*u,w*u,rewardsHeight*u));DrawEntryRewardPreviews(w,u,mode,session.SelectedChapterTier,true,true);GUI.EndGroup();
            if(!string.IsNullOrEmpty(chapterEntryError))Text(new Rect(8*u,(y+rewardsHeight)*u,(w-16)*u,48*u),"暂时无法进入，请稍后重试。",Mathf.RoundToInt(13*u),gold,false,true);
            EndTouchScroll();
        }
        private void DrawInlineChapterEntry(Rect area,float u)
        {
            if(!ReferenceEquals(chapterSelectionOwner,session.Progression.Profile))
            {
                chapterSelectionOwner=session.Progression.Profile;chapterEntryError=null;chapterScroll=Vector2.zero;
                if(!ChapterProgression.IsUnlocked(chapterSelectionOwner,session.SelectedChapterNode))session.SelectedChapterNode=ChapterNode.ForestCourt;
                if(!ChapterProgression.CanEnter(chapterSelectionOwner,session.SelectedChapterNode,session.SelectedChapterDifficulty))session.SelectedChapterDifficulty=ChapterDifficulty.Normal;
                session.SelectedChapterTier=Mathf.Clamp(session.SelectedChapterTier,1,session.Progression.UnlockedChapterTier(session.SelectedChapterNode));
            }
            float cell=(area.width-16*u)/3;
            for(int i=0;i<3;i++)
            {
                var node=(ChapterNode)i;bool unlocked=ChapterProgression.IsUnlocked(chapterSelectionOwner,node);
                if(Button(new Rect(area.x+i*(cell+8*u),area.y,cell,58*u),ChapterDefinition.Get(node).Name+(unlocked?"":"\n"+ChapterProgression.UnlockLevel(node)+"级开启"),node==session.SelectedChapterNode?gold:jade,unlocked))SelectChapterNode(node);
            }
            DrawChapterEntryDetails(new Rect(area.x,area.y+70*u,area.width,Mathf.Max(48*u,area.height-70*u)),u);
        }
        private MobilePanelLayout ChapterPanelGeometry()
        {return MobileControls.Active?MobilePanelGeometry():new MobilePanelLayout(Mathf.Min(960,width),Mathf.Min(660,height));}
        private Rect ChapterRect(MobilePanelLayout.Area area,float u)
        {var layout=ChapterPanelGeometry();float x=MobileControls.Active?0:(width-layout.Width*u)*.5f,y=MobileControls.Active?0:(height-layout.Height*u)*.5f;
            return new Rect(x+area.X*u,y+area.Y*u,area.Width*u,area.Height*u);}
        private void DrawChapterFrame(MobilePanelLayout layout,float u,string title,string subtitle)
        {

            Box(ChapterRect(new MobilePanelLayout.Area(8,4,layout.Width-16,layout.Height-8),u),jade,false);
            DrawChapterSymbol(ChapterRect(new MobilePanelLayout.Area(16,16,14,14),u),session.ChapterFinished?session.ActiveChapterNode:session.SelectedChapterNode,gold);
            Text(ChapterRect(new MobilePanelLayout.Area(36,10,layout.Width-52,28),u),title,Mathf.RoundToInt(22*u),pale,true);
            Text(ChapterRect(new MobilePanelLayout.Area(16,40,layout.Width-32,18),u),subtitle,Mathf.RoundToInt(12*u),muted);
        }
        // Shared procedural marks avoid platform font dependencies: tree, stepped rock, star.
        private void DrawChapterSymbol(Rect r,ChapterNode node,Color color)
        {
            if(node==ChapterNode.ForestCourt)
            {Fill(new Rect(r.x+r.width*.45f,r.y,r.width*.1f,r.height),color);Fill(new Rect(r.x,r.y+r.height*.25f,r.width,r.height*.15f),color);Fill(new Rect(r.x+r.width*.15f,r.y+r.height*.55f,r.width*.7f,r.height*.15f),color);}
            else if(node==ChapterNode.Redrock)
            {Fill(new Rect(r.x,r.y+r.height*.4f,r.width,r.height*.6f),color);Fill(new Rect(r.x+r.width*.25f,r.y,r.width*.5f,r.height*.5f),color);}
            else
            {Fill(new Rect(r.x+r.width*.4f,r.y,r.width*.2f,r.height),color);Fill(new Rect(r.x,r.y+r.height*.4f,r.width,r.height*.2f),color);}
        }
        private void DrawChapterResult()
        {
            if(!ReferenceEquals(chapterResultScrollOwner,session.ChapterRun))
            {chapterResultScrollOwner=session.ChapterRun;chapterResultScroll=Vector2.zero;CancelMobileScroll();}
            float u=MobileControls.Active?TouchRatio:1;var layout=ChapterPanelGeometry();
            if(!session.ChapterResultReady)
            {
                // Preserve the battlefield while the already-dead boss's actual visual retires.
                Rect badge=new Rect((width-300*u)*.5f,14*u,300*u,42*u);
                Fill(badge,new Color(.025f,.06f,.08f,.85f));Text(badge,session.ChapterRewardPending?"节点完成 · 奖励待保存":"节点完成 · 奖励已保存",Mathf.RoundToInt(14*u),jade,true);
                if(Button(new Rect((width-180*u)*.5f,height-62*u,180*u,48*u),"继续 · 查看结果",jade)){session.ContinueChapterResult();BlockUITransition();}
                return;
            }
            bool failed=session.ChapterRun==null||session.ChapterRun.Failed,pending=session.ChapterRewardPending;
            DrawChapterFrame(layout,u,failed?"本次星路止步":pending?"结算待保存":"星路线索已记录",ChapterDefinition.Get(session.ActiveChapterNode).Name);
            string copy=ChapterEntryPresentation.Result(session.ChapterResult);
            if(!string.IsNullOrEmpty(session.Progression.LastError))copy=session.Progression.LastError+"\n\n"+copy;
            float h=Style(Mathf.RoundToInt(16*u),false,true).CalcHeight(new GUIContent(copy),(layout.Body.Width-26)*u)+16*u;
            chapterResultScroll=BeginTouchScroll("chapter-result",ChapterRect(layout.Body,u),chapterResultScroll,new Rect(0,0,(layout.Body.Width-16)*u,Mathf.Max(layout.Body.Height*u,h)));
            Text(new Rect(8*u,8*u,(layout.Body.Width-26)*u,h),copy,Mathf.RoundToInt(16*u),pale,false,true);EndTouchScroll();
            if(pending&&Button(ChapterRect(layout.FooterButton(0,2),u),"重试保存结算",gold)){RetryChapterSettlement();return;}
            if(failed&&Button(ChapterRect(layout.FooterButton(0,2),u),"原条件重试",gold,session.CanRetryChapter)){session.RetryFailedChapter();BlockUITransition();return;}
            if(session.IsDead)
            {if(Button(ChapterRect(layout.FooterButton(1,2),u),"复活",jade)){ReturnFromChapter();return;}}
            else
            {
                if(PopupCloseButton(ChapterRect(layout.Close,u))||Button(ChapterRect(layout.FooterButton(pending||failed?1:0,pending||failed?2:1),u),"继续拾取",jade))
                {session.DismissFinishedResult();BlockUITransition();return;}
            }
        }
    }
}
