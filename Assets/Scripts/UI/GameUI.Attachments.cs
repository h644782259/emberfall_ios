using UnityEngine;
namespace Emberfall
{
    public sealed partial class GameUI
    {
        private void DrawAttachmentWorkshop()
        {merchantMode=1;DrawMerchantService();}
        private void DrawGrowthHudCard()
        {
            if(MobileControls.Active||session.PracticeActive||session.InDungeon||systemHistory)return;
            var goal=session.Progression.SelectedProgressionGoal(session.IsInCamp);
            Rect r=new Rect(18,Mathf.Min(height-156,height-72-AdventureSelectionLayout.LogHeight(session.SystemMessages.Count,systemHistory)-98),282,98);blockedRects.Add(r);Box(r,jade,false);
            DrawIcon(new Rect(r.x+12,r.y+14,24,24),UIIconAtlas.Utility("confirm"),jade);
            Text(new Rect(r.x+46,r.y+10,r.width-58,25),goal.Title,14,pale,true);
            Text(new Rect(r.x+12,r.y+43,r.width-24,38),"达成自动奖励 · 点击查看与定位",12,muted,false,true);
            if(GUI.Button(r,GUIContent.none,invisibleButton))NavigateProgressionGoal(goal);
        }
        private void NavigateProgressionGoal(ProgressionGoalState goal)
        {
            if(!session.IsInCamp)
            {
                ClosePanel();session.ReturnToCamp();
                if(!session.IsInCamp)return;
            }
            if(panel!=Panel.Camp)TogglePanel(Panel.Camp);
            if(goal.Action==ProgressionGoalAction.OpenPresets)
            {progressionGoalsOpen=false;OpenBuildPlans();buildPlanDetails=1;}
            else if(goal.RequiredAdventureTier>0&&goal.MaterialCost==0||goal.MaterialCost>session.Progression.Profile.mechanicMaterials)
            {
                ClosePanel();session.Player.Teleport(WorldTraversal.NearestWalkable(new Vector3(0,0,11),.45f));
                session.EnterDungeon();session.SelectedDungeonTier=Mathf.Min(Mathf.Max(1,goal.RequiredAdventureTier),session.MaximumDungeonTier);
            }
            else if(goal.Identity.Contains("practice")||session.Progression.Profile.progressionGoal==ProgressionGoalKind.ClassTutorial||session.Progression.Profile.progressionGoal==ProgressionGoalKind.CombatTrial)OpenProgressionGoals();
            else NavigateMerchantExchange();
            BlockUITransition();
        }
    }
}
