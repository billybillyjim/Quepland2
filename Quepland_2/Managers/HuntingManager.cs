using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


public static class HuntingManager
{
    public static void StartHuntingTrip(HuntingTripInfo TripInfo, int hours)
    {
        TripInfo.IsActive = true;
        TripInfo.StartTime = DateTime.UtcNow;
        TripInfo.ReturnTime = DateTime.UtcNow.AddHours(hours);
        GameState.IsOnHuntingTrip = true;
    }
    public static void EndHunt(HuntingTripInfo TripInfo, double totalTimeHunting, bool useFloor)
    {
        if (totalTimeHunting < 600)
        {
            MessageManager.AddMessage("You decide against going on a hunt before you catch anything.");
            TripInfo.IsActive = false;
            TripInfo.StartTime = DateTime.MinValue;
            TripInfo.ReturnTime = DateTime.MinValue;
            GameState.IsOnHuntingTrip = false;
            return;
        }
        double caughtBase = totalTimeHunting * (1 / 3600d) * (double)((Player.Instance.GetLevel("Hunting") + 4) / 4d);
        double caught = caughtBase.ToGaussianRandom();
        int catchFloor = 0;
        if (useFloor)
        {
            catchFloor = ((Player.Instance.GetLevel("Archery") / 10) + 1) * Math.Max((int)(totalTimeHunting / 3600), 1);
        }
        if (caught < catchFloor)
        {
            caught = catchFloor;
        }

        for (int i = 0; i < caught; i++)
        {
            if(TripInfo.DropTable == null && string.IsNullOrEmpty(TripInfo.DropTableLocation) == false)
            {
                TripInfo.DropTable = ItemManager.Instance.GetMinigameDropTable(TripInfo.DropTableLocation).DropTable;
            }
            if(TripInfo.DropTable != null)
            {
                Drop d = TripInfo.DropTable.GetDrop();
                if (ItemManager.Instance.GetItemByName(d.ItemName).HasRequirements())
                {
                    Player.Instance.Inventory.AddDrop(d);
                    Player.Instance.GainExperience(ItemManager.Instance.GetItemByName(d.ItemName).ExperienceGained);
                    MessageManager.AddMessage("You hunted a " + d + " on your trip.");
                }
                else
                {
                    MessageManager.AddMessage("You hunted a " + d + " on your trip, but it got away.");
                }
            }
            else
            {
                MessageManager.AddMessage("Something went horribly wrong on your hunting trip! You didn't catch a thing.");
                break;
            }
        }
        TripInfo.IsActive = false;
        TripInfo.StartTime = DateTime.MinValue;
        TripInfo.ReturnTime = DateTime.MinValue;
        GameState.IsOnHuntingTrip = false;
    }
}

