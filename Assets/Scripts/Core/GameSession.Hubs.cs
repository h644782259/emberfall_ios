using UnityEngine;
namespace Emberfall
{
 public sealed partial class GameSession
 {
  public int CurrentHub {get{return Progression==null?0:Progression.Profile.currentHub;}}
  public bool CanOpenTravelMap {get{return CanTravelNow();}}
  public HubNpcKind NearbyHubNpc
  {
   get
   {
    if(!HasStarted||InDungeon||IsDead||Player==null)return HubNpcKind.None;
    HubNpcKind kind=HubNpcKind.None;float nearest=2.65f;
    for(int index=0;index<2;index++){float distance=Vector3.Distance(Player.transform.position,HubNpcPosition(index));if(distance<nearest){nearest=distance;kind=(HubNpcKind)(index+1);}}
    return kind;
   }
  }
  public static Vector3 HubNpcPosition(int index){return HubSettlementPlan.Npc(index);}
  private bool CanTravelNow()
  {
   return HubTravelRules.CanTravel(HasStarted,InDungeon,IsDead,InCombat,changingZone);
  }
  public bool TravelToHub(int hub)
  {
   if(!CanTravelNow()){Notify("先结束挑战并脱离战斗，再旅行。");return false;}
   if(hub==CurrentHub)return true;
   if(!PreserveWorldLoot()||!Progression.TravelToHub(hub)){Notify(Progression.LastError);return false;}
   loadingSaveSnapshot=true;
   try {if(!ChangeZone(false))return false;}
   finally {loadingSaveSnapshot=false;}
   Notify("已抵达"+HubTravelRules.Name(CurrentHub));return true;
  }
 }
}
