namespace OgameBot.Objects.Types
{
    public class ShopItem : BaseEntityType<int, ShopItem>
    {
        public string Reference { get; }

        public static ShopItem MetalBoosterBronze = new ShopItem(ShopItemType.MetalBooster, ShopItemRank.Bronze, "de922af379061263a56d7204d1c395cefcfb7d75");
        public static ShopItem MetalBoosterSilver = new ShopItem(ShopItemType.MetalBooster, ShopItemRank.Silver, "ba85cc2b8a5d986bbfba6954e2164ef71af95d4a");
        public static ShopItem MetalBoosterGold   = new ShopItem(ShopItemType.MetalBooster, ShopItemRank.Gold  , "05294270032e5dc968672425ab5611998c409166");

        public static ShopItem CrystalBoosterBronze = new ShopItem(ShopItemType.CrystalBooster, ShopItemRank.Bronze, "3c9f85221807b8d593fa5276cdf7af9913c4a35d");
        public static ShopItem CrystalBoosterSilver = new ShopItem(ShopItemType.CrystalBooster, ShopItemRank.Silver, "422db99aac4ec594d483d8ef7faadc5d40d6f7d3");
        public static ShopItem CrystalBoosterGold   = new ShopItem(ShopItemType.CrystalBooster, ShopItemRank.Gold  , "118d34e685b5d1472267696d1010a393a59aed03");

        public static ShopItem DeuteriumBoosterBronze = new ShopItem(ShopItemType.DeuteriumBooster, ShopItemRank.Bronze, "d9fa5f359e80ff4f4c97545d07c66dbadab1d1be");
        public static ShopItem DeuteriumBoosterSilver = new ShopItem(ShopItemType.DeuteriumBooster, ShopItemRank.Silver, "e4b78acddfa6fd0234bcb814b676271898b0dbb3");
        public static ShopItem DeuteriumBoosterGold   = new ShopItem(ShopItemType.DeuteriumBooster, ShopItemRank.Gold  , "5560a1580a0330e8aadf05cb5bfe6bc3200406e2");

        public static ShopItem ConstructionBoosterBronze = new ShopItem(ShopItemType.ConstructionBooster, ShopItemRank.Bronze, "40f6c78e11be01ad3389b7dccd6ab8efa9347f3c");
        public static ShopItem ConstructionBoosterSilver = new ShopItem(ShopItemType.ConstructionBooster, ShopItemRank.Silver, "4a58d4978bbe24e3efb3b0248e21b3b4b1bfbd8a");
        public static ShopItem ConstructionBoosterGold   = new ShopItem(ShopItemType.ConstructionBooster, ShopItemRank.Gold  , "???"); // #todo

        public static ShopItem ResearchBoosterBronze = new ShopItem(ShopItemType.ResearchBooster, ShopItemRank.Bronze, "da4a2a1bb9afd410be07bc9736d87f1c8059e66d");
        public static ShopItem ResearchBoosterSilver = new ShopItem(ShopItemType.ResearchBooster, ShopItemRank.Silver, "d26f4dab76fdc5296e3ebec11a1e1d2558c713ea");
        public static ShopItem ResearchBoosterGold   = new ShopItem(ShopItemType.ResearchBooster, ShopItemRank.Gold  , "???"); // #todo

        public static ShopItem ShipyardBoosterBronze = new ShopItem(ShopItemType.ShipyardBooster, ShopItemRank.Bronze, "d3d541ecc23e4daa0c698e44c32f04afd2037d84");
        public static ShopItem ShipyardBoosterSilver = new ShopItem(ShopItemType.ShipyardBooster, ShopItemRank.Silver, "27cbcd52f16693023cb966e5026d8a1efbbfc0f9");
        public static ShopItem ShipyardBoosterGold   = new ShopItem(ShopItemType.ShipyardBooster, ShopItemRank.Gold  , "0968999df2fe956aa4a07aea74921f860af7d97f");

        public static ShopItem PlanetFieldsBronze = new ShopItem(ShopItemType.PlanetFields, ShopItemRank.Bronze, "16768164989dffd819a373613b5e1a52e226a5b0");
        public static ShopItem PlanetFieldsSilver = new ShopItem(ShopItemType.PlanetFields, ShopItemRank.Silver, "0e41524dc46225dca21c9119f2fb735fd7ea5cb3");
        public static ShopItem PlanetFieldsGold   = new ShopItem(ShopItemType.PlanetFields, ShopItemRank.Gold  , "04e58444d6d0beb57b3e998edc34c60f8318825a");

        public static ShopItem MoonFieldsBronze = new ShopItem(ShopItemType.MoonFields, ShopItemRank.Bronze, "be67e009a5894f19bbf3b0c9d9b072d49040a2cc");
        public static ShopItem MoonFieldsSilver = new ShopItem(ShopItemType.MoonFields, ShopItemRank.Silver, "c21ff33ba8f0a7eadb6b7d1135763366f0c4b8bf");
        public static ShopItem MoonFieldsGold   = new ShopItem(ShopItemType.MoonFields, ShopItemRank.Gold  , "05ee9654bd11a261f1ff0e5d0e49121b5e7e4401");

        public static ShopItem MOONSBronze = new ShopItem(ShopItemType.MOONS, ShopItemRank.Bronze, "485a6d5624d9de836d3eb52b181b13423f795770");
        public static ShopItem MOONSSilver = new ShopItem(ShopItemType.MOONS, ShopItemRank.Silver, "fd895a5c9fd978b9c5c7b65158099773ba0eccef");
        public static ShopItem MOONSGold   = new ShopItem(ShopItemType.MOONS, ShopItemRank.Gold  , "45d6660308689c65d97f3c27327b0b31f880ae75");

        private ShopItem(ShopItemType type, ShopItemRank rank, string reference) : base((int)type + (int)rank)
        {
            Reference = reference;
        }

        public static implicit operator ShopItem(int shopItem)
        {
            return Index[shopItem];
        }

        public static implicit operator ShopItemType(ShopItem type)
        {
            return (ShopItemType)((type.Type / 10) * 10);
        }

        public static implicit operator ShopItemRank(ShopItem type)
        {
            return (ShopItemRank)(type.Type % 10);
        }

    }
}
