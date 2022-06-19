using ItemChanger;
using ItemChanger.Placements;
using ItemChanger.Tags;
using ItemSyncMod.Items;

namespace ItemSyncMod.SyncFeatures
{
    internal class VanillaItems
    {
        private static readonly int[] slyMaskShardsCosts = new int[] { 150, 500, 800, 1500 };
        private static readonly int[] slyVesselFragmentsCosts = new int[] { 550, 900 };
        private static readonly int[] salubraNotchCosts = new int[] { 120, 500, 900, 1400 };
        private static readonly int[] salubraRequiredCharms = new int[] { 5, 10, 18, 25 };

        private static int s_slyShellFrag, s_slyVesselFrag, s_salubraNotch;
        internal static void ResetCounters()
        {
            s_slyShellFrag = s_slyVesselFrag = s_salubraNotch = 0;
        }

        internal static void AddVanillaItemsToICPlacements(List<RandomizerCore.GeneralizedPlacement> vanilla)
        {
            ResetCounters();
            List<AbstractPlacement> vanillaPlacements = new();
            foreach (RandomizerCore.GeneralizedPlacement placement in vanilla)
            {

                AbstractPlacement abstractPlacement = Finder.GetLocation(placement.Location.Name)?.Wrap();
                if (abstractPlacement == null || ItemManager.IsStartLocation(abstractPlacement)) continue;

                AbstractItem item = Finder.GetItem(placement.Item.Name);
                if (item == null) continue;

                abstractPlacement.Add(item);
                vanillaPlacements.Add(abstractPlacement);

                OptionalAddItemCost(item, abstractPlacement);

                item.GetOrAddTag<CompletionWeightTag>().Weight = 0; // Drop from completion percentage
            }

            ItemChangerMod.AddPlacements(vanillaPlacements, PlacementConflictResolution.MergeKeepingOld);
        }

        private static void OptionalAddItemCost(AbstractItem item, AbstractPlacement placement)
        {
            int geoCost = 0, essenceCost = 0, grubCost = 0;
            Cost additionalCosts = null;

            switch (item.name)
            {
                // Sly
                case ItemNames.Mask_Shard when placement.Name.StartsWith("Sly"):
                    geoCost = slyMaskShardsCosts[s_slyShellFrag];
                    s_slyShellFrag++;
                    switch (s_slyShellFrag)
                    {
                        case 1:
                            item.AddTag<SetPDBoolOnGiveTag>().fieldName = nameof(PlayerData.slyShellFrag1);
                            break;
                        case 2:
                            item.AddTag<SetPDBoolOnGiveTag>().fieldName = nameof(PlayerData.slyShellFrag2);
                            item.AddTag<PDBoolShopReqTag>().fieldName = nameof(PlayerData.slyShellFrag1);
                            break;
                        case 3:
                            item.AddTag<SetPDBoolOnGiveTag>().fieldName = nameof(PlayerData.slyShellFrag3);
                            item.AddTag<PDBoolShopReqTag>().fieldName = nameof(PlayerData.slyShellFrag2);
                            item.AddTag<PDBoolShopReqTag>().fieldName = nameof(PlayerData.gaveSlykey);
                            break;
                        case 4:
                            item.AddTag<SetPDBoolOnGiveTag>().fieldName = nameof(PlayerData.slyShellFrag4);
                            item.AddTag<PDBoolShopReqTag>().fieldName = nameof(PlayerData.slyShellFrag3);
                            item.AddTag<PDBoolShopReqTag>().fieldName = nameof(PlayerData.gaveSlykey);
                            break;
                    }
                    break;
                case ItemNames.Vessel_Fragment when placement.Name.StartsWith("Sly"):
                    geoCost = slyVesselFragmentsCosts[s_slyVesselFrag];
                    s_slyVesselFrag++;
                    switch (s_slyVesselFrag)
                    {
                        case 1:
                            item.AddTag<SetPDBoolOnGiveTag>().fieldName = nameof(PlayerData.slyVesselFrag1);
                            break;
                        case 2:
                            item.AddTag<SetPDBoolOnGiveTag>().fieldName = nameof(PlayerData.slyVesselFrag2);
                            item.AddTag<PDBoolShopReqTag>().fieldName = nameof(PlayerData.slyVesselFrag1);
                            item.AddTag<PDBoolShopReqTag>().fieldName = nameof(PlayerData.gaveSlykey);
                            break;
                    }
                    break;
                case ItemNames.Elegant_Key:
                    item.AddTag<PDBoolShopReqTag>().fieldName = nameof(PlayerData.gaveSlykey);
                    geoCost = 800; break;
                case ItemNames.Lumafly_Lantern:
                    geoCost = 1800; break;
                case ItemNames.Gathering_Swarm:
                    geoCost = 300; break;
                case ItemNames.Stalwart_Shell:
                    geoCost = 200; break;
                case ItemNames.Heavy_Blow:
                    item.AddTag<PDBoolShopReqTag>().fieldName = nameof(PlayerData.gaveSlykey);
                    geoCost = 350; break;
                case ItemNames.Sprintmaster:
                    item.AddTag<PDBoolShopReqTag>().fieldName = nameof(PlayerData.gaveSlykey);
                    geoCost = 400; break;
                case ItemNames.Rancid_Egg when placement.Name.StartsWith("Sly"):
                    item.AddTag<PDBoolShopReqTag>().fieldName = nameof(PlayerData.gaveSlykey);
                    geoCost = 60; break;
                case ItemNames.Simple_Key:
                    item.AddTag<PDBoolShopReqTag>().fieldName = nameof(PlayerData.gaveSlykey);
                    geoCost = 950; break;

                // Leg Eater
                case ItemNames.Fragile_Heart:
                    geoCost = 350; break;
                case ItemNames.Fragile_Greed:
                    geoCost = 250; break;
                case ItemNames.Fragile_Strength:
                    geoCost = 600; break;
                case ItemNames.Fragile_Heart_Repair:
                    item.AddTag<PDBoolShopRemoveTag>().fieldName = nameof(PlayerData.fragileHealth_unbreakable);
                    item.AddTag<PDBoolShopReqTag>().fieldName = nameof(PlayerData.brokenCharm_23);
                    geoCost = 200; break;
                case ItemNames.Fragile_Greed_Repair:
                    item.AddTag<PDBoolShopRemoveTag>().fieldName = nameof(PlayerData.fragileGreed_unbreakable);
                    item.AddTag<PDBoolShopReqTag>().fieldName = nameof(PlayerData.brokenCharm_24);
                    geoCost = 150; break;
                case ItemNames.Fragile_Strength_Repair:
                    item.AddTag<PDBoolShopRemoveTag>().fieldName = nameof(PlayerData.fragileStrength_unbreakable);
                    item.AddTag<PDBoolShopReqTag>().fieldName = nameof(PlayerData.brokenCharm_25);
                    geoCost = 350; break;

                // Salubra
                case ItemNames.Lifeblood_Heart:
                    geoCost = 250; break;
                case ItemNames.Longnail:
                    geoCost = 300; break;
                case ItemNames.Steady_Body:
                    geoCost = 120; break;
                case ItemNames.Shaman_Stone:
                    geoCost = 220; break;
                case ItemNames.Quick_Focus:
                    geoCost = 800; break;
                case ItemNames.Charm_Notch when placement.Name == "Salubra":
                    geoCost = salubraNotchCosts[s_salubraNotch];
                    int requiredCharms = salubraRequiredCharms[s_salubraNotch];
                    additionalCosts = new PDIntCost(requiredCharms, nameof(PlayerData.charmsOwned), $"Requires {requiredCharms} charms");
                    s_salubraNotch++;
                    switch (s_salubraNotch)
                    {
                        case 1:
                            item.AddTag<SetPDBoolOnGiveTag>().fieldName = nameof(PlayerData.salubraNotch1);
                            break;
                        case 2:
                            item.AddTag<SetPDBoolOnGiveTag>().fieldName = nameof(PlayerData.salubraNotch2);
                            break;
                        case 3:
                            item.AddTag<SetPDBoolOnGiveTag>().fieldName = nameof(PlayerData.salubraNotch3);
                            break;
                        case 4:
                            item.AddTag<SetPDBoolOnGiveTag>().fieldName = nameof(PlayerData.salubraNotch4);
                            break;
                    }
                    break;
                case ItemNames.Salubras_Blessing:
                    item.AddTag<PDBoolShopReqTag>().fieldName = nameof(PlayerData.salubraNotch4);
                    additionalCosts = new PDIntCost(40, nameof(PlayerData.charmsOwned), "Required 40 charms");
                    geoCost = 800; break;

                // Iselda Charms & Maps
                case ItemNames.Wayward_Compass:
                    geoCost = 220; break;
                case ItemNames.Quill:
                    geoCost = 120; break;
                case ItemNames.Crossroads_Map when placement.Name == "Iselda":
                    item.AddTag<PDBoolShopReqTag>().fieldName = nameof(PlayerData.corn_crossroadsLeft);
                    item.AddTag<PDBoolShopRemoveTag>().fieldName = nameof(PlayerData.mapCrossroads);
                    geoCost = 40; break;
                case ItemNames.Greenpath_Map when placement.Name == "Iselda":
                    item.AddTag<PDBoolShopReqTag>().fieldName = nameof(PlayerData.corn_greenpathLeft);
                    item.AddTag<PDBoolShopRemoveTag>().fieldName = nameof(PlayerData.mapGreenpath);
                    geoCost = 80; break;
                case ItemNames.Fog_Canyon_Map when placement.Name == "Iselda":
                    item.AddTag<PDBoolShopReqTag>().fieldName = nameof(PlayerData.corn_fogCanyonLeft);
                    item.AddTag<PDBoolShopRemoveTag>().fieldName = nameof(PlayerData.mapFogCanyon);
                    geoCost = 200; break;
                case ItemNames.Fungal_Wastes_Map when placement.Name == "Iselda":
                    item.AddTag<PDBoolShopReqTag>().fieldName = nameof(PlayerData.corn_fungalWastesLeft);
                    item.AddTag<PDBoolShopRemoveTag>().fieldName = nameof(PlayerData.mapFungalWastes);
                    geoCost = 100; break;
                case ItemNames.City_of_Tears_Map when placement.Name == "Iselda":
                    item.AddTag<PDBoolShopReqTag>().fieldName = nameof(PlayerData.corn_cityLeft);
                    item.AddTag<PDBoolShopRemoveTag>().fieldName = nameof(PlayerData.mapCity);
                    geoCost = 120; break;
                case ItemNames.Royal_Waterways_Map when placement.Name == "Iselda":
                    item.AddTag<PDBoolShopReqTag>().fieldName = nameof(PlayerData.corn_waterwaysLeft);
                    item.AddTag<PDBoolShopRemoveTag>().fieldName = nameof(PlayerData.mapWaterways);
                    geoCost = 100; break;
                case ItemNames.Crystal_Peak_Map when placement.Name == "Iselda":
                    item.AddTag<PDBoolShopReqTag>().fieldName = nameof(PlayerData.corn_minesLeft);
                    item.AddTag<PDBoolShopRemoveTag>().fieldName = nameof(PlayerData.mapMines);
                    geoCost = 150; break;
                case ItemNames.Resting_Grounds_Map when placement.Name == "Iselda":
                    item.AddTag<PDBoolShopReqTag>().fieldName = nameof(PlayerData.visitedRestingGrounds);
                    item.AddTag<PDBoolShopRemoveTag>().fieldName = nameof(PlayerData.mapRestingGrounds);
                    geoCost = 75; break;
                case ItemNames.Howling_Cliffs_Map when placement.Name == "Iselda":
                    item.AddTag<PDBoolShopReqTag>().fieldName = nameof(PlayerData.corn_cliffsLeft);
                    item.AddTag<PDBoolShopRemoveTag>().fieldName = nameof(PlayerData.mapCliffs);
                    geoCost = 100; break;
                case ItemNames.Deepnest_Map when placement.Name == "Iselda":
                    item.AddTag<PDBoolShopReqTag>().fieldName = nameof(PlayerData.corn_deepnestLeft);
                    item.AddTag<PDBoolShopRemoveTag>().fieldName = nameof(PlayerData.mapDeepnest);
                    geoCost = 50; break;
                case ItemNames.Kingdoms_Edge_Map when placement.Name == "Iselda":
                    item.AddTag<PDBoolShopReqTag>().fieldName = nameof(PlayerData.corn_outskirtsLeft);
                    item.AddTag<PDBoolShopRemoveTag>().fieldName = nameof(PlayerData.mapOutskirts);
                    geoCost = 150; break;
                case ItemNames.Queens_Gardens_Map when placement.Name == "Iselda":
                    item.AddTag<PDBoolShopReqTag>().fieldName = nameof(PlayerData.corn_royalGardensLeft);
                    item.AddTag<PDBoolShopRemoveTag>().fieldName = nameof(PlayerData.mapRoyalGardens);
                    geoCost = 200; break;
                case ItemNames.Ancient_Basin_Map when placement.Name == "Iselda":
                    item.AddTag<PDBoolShopReqTag>().fieldName = nameof(PlayerData.corn_abyssLeft);
                    item.AddTag<PDBoolShopRemoveTag>().fieldName = nameof(PlayerData.mapAbyss);
                    geoCost = 150; break;
                // Iselda pins
                case ItemNames.Bench_Pin:
                case ItemNames.Cocoon_Pin:
                case ItemNames.Vendor_Pin:
                case ItemNames.Hot_Spring_Pin:
                case ItemNames.Scarab_Marker:
                case ItemNames.Shell_Marker:
                    geoCost = 100; break;
                case ItemNames.Stag_Station_Pin:
                    item.AddTag<PDBoolShopReqTag>().fieldName = nameof(PlayerData.metStag);
                    geoCost = 100; break;
                case ItemNames.Tram_Pin:
                    item.AddTag<PDBoolShopReqTag>().fieldName = nameof(PlayerData.hasTramPass);
                    geoCost = 100; break;
                case ItemNames.Token_Marker:
                    item.AddTag<PDBoolShopReqTag>().fieldName = nameof(PlayerData.hasDash);
                    geoCost = 100; break;
                case ItemNames.Whispering_Root_Pin:
                    item.AddTag<PDBoolShopReqTag>().fieldName = nameof(PlayerData.hasDreamNail);
                    geoCost = 150; break;
                case ItemNames.Warriors_Grave_Pin:
                    item.AddTag<PDBoolShopReqTag>().fieldName = nameof(PlayerData.hasDreamNail);
                    geoCost = 180; break;
                case ItemNames.Gleaming_Marker:
                    item.AddTag<PDBoolShopReqTag>().fieldName = nameof(PlayerData.hasDash);
                    geoCost = 100; break;

                // Divine
                case ItemNames.Unbreakable_Heart:
                    (placement as ISingleCostPlacement).Cost = Cost.NewGeoCost(12000); break;
                case ItemNames.Unbreakable_Greed:
                    (placement as ISingleCostPlacement).Cost = Cost.NewGeoCost(9000); break;
                case ItemNames.Unbreakable_Strength:
                    (placement as ISingleCostPlacement).Cost = Cost.NewGeoCost(15000); break;

                // Stags
                case ItemNames.Crossroads_Stag:
                    (placement as ISingleCostPlacement).Cost = Cost.NewGeoCost(50); break;
                case ItemNames.Greenpath_Stag:
                    (placement as ISingleCostPlacement).Cost = Cost.NewGeoCost(140); break;
                case ItemNames.Queens_Station_Stag:
                    (placement as ISingleCostPlacement).Cost = Cost.NewGeoCost(120); break;
                case ItemNames.Queens_Gardens_Stag:
                    (placement as ISingleCostPlacement).Cost = Cost.NewGeoCost(200); break;
                case ItemNames.City_Storerooms_Stag:
                    (placement as ISingleCostPlacement).Cost = Cost.NewGeoCost(200); break;
                case ItemNames.Kings_Station_Stag:
                    (placement as ISingleCostPlacement).Cost = Cost.NewGeoCost(300); break;
                case ItemNames.Distant_Village_Stag:
                    (placement as ISingleCostPlacement).Cost = Cost.NewGeoCost(250); break;
                case ItemNames.Hidden_Station_Stag:
                    (placement as ISingleCostPlacement).Cost = Cost.NewGeoCost(300); break;

                
                case ItemNames.Hallownest_Seal when placement.Name.Contains("Seer"):
                    placement.AddTag<DestroySeerRewardTag>().destroyRewards = SeerRewards.HallownestSeal;
                    essenceCost = 100; break;
                case ItemNames.Pale_Ore when placement.Name.Contains("Seer"):
                    placement.AddTag<DestroySeerRewardTag>().destroyRewards = SeerRewards.PaleOre;
                    essenceCost = 300; break;
                case ItemNames.Dream_Wielder:
                    placement.AddTag<DestroySeerRewardTag>().destroyRewards = SeerRewards.DreamWielder;
                    essenceCost = 500; break;
                case ItemNames.Vessel_Fragment when placement.Name.Contains("Seer"):
                    placement.AddTag<DestroySeerRewardTag>().destroyRewards = SeerRewards.VesselFragment;
                    essenceCost = 700; break;
                case ItemNames.Dream_Gate:
                    placement.AddTag<DestroySeerRewardTag>().destroyRewards = SeerRewards.DreamGate;
                    essenceCost = 900; break;
                case ItemNames.Arcane_Egg when placement.Name.Contains("Seer"):
                    placement.AddTag<DestroySeerRewardTag>().destroyRewards = SeerRewards.ArcaneEgg;
                    essenceCost = 1200; break;
                case ItemNames.Mask_Shard when placement.Name.Contains("Seer"):
                    placement.AddTag<DestroySeerRewardTag>().destroyRewards = SeerRewards.MaskShard;
                    essenceCost = 1500; break;
                case ItemNames.Awoken_Dream_Nail:
                    placement.AddTag<DestroySeerRewardTag>().destroyRewards = SeerRewards.AwokenDreamNail;
                    essenceCost = 1800; break;

                case ItemNames.Mask_Shard when placement.Name.Contains("Grub"):
                    placement.AddTag<DestroyGrubRewardTag>().destroyRewards |= GrubfatherRewards.MaskShard;
                    grubCost = 5; break;
                case ItemNames.Grubsong:
                    placement.AddTag<DestroyGrubRewardTag>().destroyRewards |= GrubfatherRewards.Grubsong;
                    grubCost = 10; break;
                case ItemNames.Rancid_Egg when placement.Name.Contains("Grub"):
                    placement.AddTag<DestroyGrubRewardTag>().destroyRewards |= GrubfatherRewards.RancidEgg;
                    grubCost = 16; break;
                case ItemNames.Hallownest_Seal when placement.Name.Contains("Grub"):
                    placement.AddTag<DestroyGrubRewardTag>().destroyRewards |= GrubfatherRewards.HallownestSeal;
                    grubCost = 23; break;
                case ItemNames.Pale_Ore when placement.Name.Contains("Grub"):
                    placement.AddTag<DestroyGrubRewardTag>().destroyRewards |= GrubfatherRewards.PaleOre;
                    grubCost = 31; break;
                case ItemNames.Kings_Idol when placement.Name.Contains("Grub"):
                    placement.AddTag<DestroyGrubRewardTag>().destroyRewards |= GrubfatherRewards.KingsIdol;
                    grubCost = 38; break;
                case ItemNames.Grubberflys_Elegy:
                    placement.AddTag<DestroyGrubRewardTag>().destroyRewards |= GrubfatherRewards.GrubberflysElegy;
                    grubCost = 46; break;
            }

            if (geoCost > 0)
                item.GetOrAddTag<CostTag>().Cost = Cost.NewGeoCost(geoCost) + additionalCosts;
            else if (essenceCost > 0)
                item.GetOrAddTag<CostTag>().Cost = Cost.NewEssenceCost(essenceCost) + additionalCosts;
            else if (grubCost > 0)
                item.GetOrAddTag<CostTag>().Cost = Cost.NewGrubCost(grubCost) + additionalCosts;
        }
    }
}
