namespace Emberfall
{
    public sealed partial class GameUI
    {
        private bool CloseMobileInventoryDetail()
        {
            // Match the visible compact equipment detail, not a stale flag on a
            // tablet, supply page, or another modal. Preserve both scroll anchors.
            if(!MobileControls.Active || panel!=Panel.Inventory || session.Paused ||
                (MobileCollectionLayout.SideBySideInventory(MobileControls.Layout.Width) && mobileInventoryPicker == 0) ||
                mobileInventoryTab==2 || (!mobileInventoryDetail && mobileInventoryPicker == 0))return false;
            if (mobileInventoryPicker != 0) { mobileInventoryPicker = 0; CancelMobileScroll(); BlockUITransition(); return true; }
            mobileInventoryDetail=false;
            CancelMobileScroll();BlockUITransition();return true;
        }
    }
}
