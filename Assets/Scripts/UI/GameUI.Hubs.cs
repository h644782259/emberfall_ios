using UnityEngine;

namespace Emberfall
{
    public sealed partial class GameUI
    {
        private bool travelReturnPause;
        private string travelError;
        private HubNpcKind inventoryHubNpc;
        private bool merchantShopOpen;

        private static string HubNpcLabel(HubNpcKind kind)
        {
            return kind == HubNpcKind.Merchant ? "商人 · 药剂 / 出售" :
                kind == HubNpcKind.Blacksmith ? "铁匠 · 部位强化" :
                kind == HubNpcKind.Exchange ? "观星员 · 星路 / 兑换" : "营地工坊";
        }

        private static string HubNpcMobileLabel(HubNpcKind kind)
        { return kind == HubNpcKind.Merchant ? "商人交易" : kind == HubNpcKind.Blacksmith ? "铁匠强化" : "星路 / 兑换"; }

        private void OpenNearbyHubNpc()
        {
            if (UITransitionBlocked || session == null || session.InputBlocked) return;
            HubNpcKind kind = session.NearbyHubNpc;
            if (kind == HubNpcKind.None) return;
            CancelHotbarPointer();
            inventoryHubNpc = kind;merchantShopOpen=kind==HubNpcKind.Merchant;smithShopOpen=kind==HubNpcKind.Blacksmith;npcShopScroll=Vector2.zero;
            if (kind == HubNpcKind.Exchange) { OpenChapterSelection();return; }
            else
            {
                panel = Panel.Inventory;
                if(MobileControls.Active)mobileInventoryNpcRequest=kind;
                if (kind == HubNpcKind.Blacksmith) selectedItem = session.Progression.Profile.weaponId;
            }
            session.SetUIBlocking(true);
            BlockUITransition();
        }

        private bool smithShopOpen;
        private Vector2 npcShopScroll;
        private void DrawMerchantShop()
        {
            var layout=MobilePanelGeometry();var p=session.Progression;
            if(DrawMobilePanelChrome(layout,"商人 · 药剂与出售","金币 "+p.Profile.gold+" · 锁定和已穿装备不可出售")){merchantShopOpen=false;return;}
            bool near=session.NearbyHubNpc==HubNpcKind.Merchant&&!session.InDungeon;
            RebuildBagItems();string sell=null;
            float w=layout.Body.Width-18,u=TouchRatio;
            npcShopScroll=BeginTouchScroll("merchant-stock",MobilePanelRect(layout.Body),npcShopScroll,new Rect(0,0,w*u,Mathf.Max(layout.Body.Height,bagItems.Count*64+56)*u));
            Text(TouchRect(8,4,w-16,44),"生命药剂 × "+p.Profile.potions+" · 每瓶 "+ProgressionService.PotionPrice+" 金币",TouchFont(16),pale,true);
            for(int i=0;i<bagItems.Count;i++)
            {
                var item=bagItems[i];float y=56+i*64;bool protectedItem=item.locked||IsEquipped(item);
                DrawIcon(TouchRect(6,y+8,36,36),UIIconAtlas.EquipmentCardIcon(item.slot),GameBalance.RarityColor(item.rarity));
                Text(TouchRect(50,y+4,w-210,52),item.name+(item.locked?" · 已锁定":IsEquipped(item)?" · 已穿戴":""),TouchFont(14),pale,true,true);
                if(DangerButton(TouchRect(w-148,y+6,140,48),"出售 "+p.SellValue(item)+" 金",gold,near&&!protectedItem))sell=item.id;
            }
            EndTouchScroll();
            if(sell!=null)SellInventoryItem(sell);
            if(InventoryAction(MobilePanelRect(layout.FooterButton(0,2)),"购买药剂",near&&p.Profile.gold>=ProgressionService.PotionPrice))Feedback(p.BuyPotion(),"已购买生命药剂");
            if(Button(MobilePanelRect(layout.FooterButton(1,2)),"结束对话",jade))ClosePanel();
        }
        private void DrawBlacksmithShop()
        {
            var layout=MobilePanelGeometry();var p=session.Progression;
            if(DrawMobilePanelChrome(layout,"铁匠 · 部位强化","金币 "+p.Profile.gold+" · 换装继承部位等级")){smithShopOpen=false;return;}
            bool near=session.NearbyHubNpc==HubNpcKind.Blacksmith&&!session.InDungeon;
            float w=layout.Body.Width-18,u=TouchRatio;string upgrade=null;
            npcShopScroll=BeginTouchScroll("smith-upgrades",MobilePanelRect(layout.Body),npcShopScroll,new Rect(0,0,w*u,Mathf.Max(layout.Body.Height,360)*u));
            for(int i=0;i<3;i++)
            {
                var slot=(ItemSlot)i;var item=p.Equipped(slot);int rank=p.SlotUpgradeRank(slot);float y=i*120;
                Text(TouchRect(8,y+4,w-16,26),GameBalance.SlotName(slot)+" +"+rank+(item==null?" · 未穿装备":" · "+item.name),TouchFont(16),pale,true);
                if(item==null)continue;
                bool capped=rank>=ProgressionService.MaximumUpgrade;int cost=p.UpgradeCost(item);
                var next=p.PreviewUpgrade(item,Mathf.Min(rank+1,ProgressionService.MaximumUpgrade));
                Text(TouchRect(8,y+34,w-16,26),"攻击 "+item.attack+" → "+next.attack+" · 防御 "+item.defense+" → "+next.defense+" · 生命 "+item.health+" → "+next.health,TouchFont(13),muted);
                if(InventoryAction(TouchRect(8,y+65,w-16,44),capped?"部位已满级":"强化 · "+cost+" 金",near&&!capped&&p.Profile.gold>=cost))upgrade=item.id;
            }
            EndTouchScroll();
            if(upgrade!=null)Feedback(p.Upgrade(upgrade),"部位强化已更新");
        }

        private string HubInventoryTitle
        {
            get
            {
                if (session.InDungeon || session.NearbyHubNpc != inventoryHubNpc) return "行囊";
                return inventoryHubNpc == HubNpcKind.Merchant ? "商人 · 行囊与补给" :
                    inventoryHubNpc == HubNpcKind.Blacksmith ? "铁匠 · 装备与强化" : "行囊";
            }
        }

        private string HubInventoryHint
        {
            get
            {
                if (session.InDungeon || session.NearbyHubNpc != inventoryHubNpc) return "";
                return inventoryHubNpc == HubNpcKind.Merchant ? "下方购买药剂 · 背包右侧出售闲置装备" :
                    inventoryHubNpc == HubNpcKind.Blacksmith ? "右侧强化装备部位 · 换装继承部位等级" : "";
            }
        }

        private void OpenTravelMap()
        {
            if (UITransitionBlocked || session == null || !session.HasStarted || session.IsDead || exitRequest.Open || saveFlow.Open) return;
            travelReturnPause = session.Paused;
            travelError = null;
            CancelHotbarPointer();
            panel = Panel.TravelMap;
            session.SetUIBlocking(true);
            session.SetPaused(false);
            BlockUITransition();
        }

        private bool CloseTravelMap()
        {
            if (panel != Panel.TravelMap) return false;
            panel = Panel.None;
            session.SetUIBlocking(false);
            session.SetPaused(travelReturnPause);
            travelReturnPause = false;
            BlockUITransition();
            return true;
        }

        private void DrawHubActions(float x, float y)
        {
            if (Button(new Rect(x, y, 222, 40), "城镇旅行地图", jade)) OpenTravelMap();
            HubNpcKind nearby = session.NearbyHubNpc;
            if (nearby != HubNpcKind.None && Button(new Rect(x, y - 50, 222, 42), HubNpcLabel(nearby), gold)) OpenNearbyHubNpc();
        }

        private void DrawTravelMap()
        {
            bool mobile = MobileControls.Active;
            float u = mobile ? TouchRatio : 1.35f;
            float w = 520 * u, h = 306 * u;
            Rect r = new Rect((width - w) * .5f, (height - h) * .5f, w, h);
            Fill(new Rect(0, 0, width, height), new Color(.01f, .025f, .045f, .96f));
            Box(r, jade);
            Text(new Rect(r.x + 16*u, r.y + 10*u, 488*u, 28*u), "城镇旅行 · " + HubTravelRules.Name(session.CurrentHub),
                Mathf.RoundToInt(21*u), pale, true);
            string hint = !string.IsNullOrEmpty(travelError) ? travelError : !session.CanOpenTravelMap ?
                "挑战中或附近有敌人时不能旅行，请先安全返回营地。" : "免费旅行 · 商人、铁匠、兑换员提供相同服务 · 装备与货币保留";
            Text(new Rect(r.x + 16*u, r.y + 40*u, 488*u, 30*u), hint, Mathf.RoundToInt(11*u),
                !string.IsNullOrEmpty(travelError) ? gold : muted, false, true);
            GameProfile profile = session.Progression.Profile;
            int mask = HubTravelRules.UnlockedMask(profile.unlockedHubMask, profile.level, profile.clearedRuns);
            // The connecting road is a travel diagram, not a claim about world distances.
            Fill(new Rect(r.x + 60*u, r.y + 95*u, 400*u, 2*u), jade * .5f);
            for (int hub = 0; hub < HubTravelRules.Count; hub++)
            {
                bool unlocked = HubTravelRules.IsUnlocked(mask, hub), current = hub == session.CurrentHub;
                Color tint = hub == 0 ? jade : hub == 1 ? new Color(.95f,.52f,.32f) : new Color(.56f,.66f,1);
                float x = r.x + (16 + hub * 166)*u;
                Rect cardRect = new Rect(x, r.y + 76*u, 156*u, 164*u);
                Fill(cardRect, card); Border(cardRect, unlocked ? tint : muted*.4f);
                Fill(new Rect(x + 68*u, r.y + 86*u, 20*u, 20*u), current ? gold : unlocked ? tint : muted*.4f);
                Text(new Rect(x + 6*u, r.y + 114*u, 144*u, 25*u), HubTravelRules.Name(hub), Mathf.RoundToInt(16*u), unlocked ? pale : muted, true, false, TextAnchor.MiddleCenter);
                Text(new Rect(x + 7*u, r.y + 143*u, 142*u, 31*u), unlocked ? "商人 / 铁匠 / 兑换员" : HubTravelRules.UnlockHint(hub),
                    Mathf.RoundToInt(11*u), muted, false, true, TextAnchor.MiddleCenter);
                if (Button(new Rect(x + 8*u, r.y + 183*u, 140*u, 48*u), current ? "当前城镇" : unlocked ? "前往" : "尚未解锁",
                    tint, unlocked && !current && session.CanOpenTravelMap && !UITransitionBlocked))
                {
                    if (session.TravelToHub(hub))
                    {
                        travelReturnPause = false;
                        CloseTravelMap();
                        return;
                    }
                    travelError = string.IsNullOrEmpty(session.Progression.LastError) ? "旅行未完成，请确认已安全返回营地后重试。" : session.Progression.LastError;
                    BlockUITransition();
                }
            }
            if (Button(new Rect(r.x + 16*u, r.y + 250*u, 488*u, 48*u), travelReturnPause ? "返回暂停菜单" : "返回冒险", jade)) CloseTravelMap();
        }
    }
}
