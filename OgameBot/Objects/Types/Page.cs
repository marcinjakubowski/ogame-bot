using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OgameBot.Objects.Types
{
    

    public class Page : BaseEntityType<PageType, Page>
    {
        public static Page Login            { get; } = new Page(PageType.Login           , "login");
        public static Page Overview         { get; } = new Page(PageType.Overview        , "overview");
        public static Page Resources        { get; } = new Page(PageType.Resources       , "resources");
        public static Page Facilities       { get; } = new Page(PageType.Facilities      , "station");  
        public static Page Merchant         { get; } = new Page(PageType.Merchant        , "traderOverview");  
        public static Page Research         { get; } = new Page(PageType.Research        , "research");  
        public static Page Shipyard         { get; } = new Page(PageType.Shipyard        , "shipyard");  
        public static Page Defence          { get; } = new Page(PageType.Defence         , "defense");  
        public static Page Fleet            { get; } = new Page(PageType.Fleet           , "fleet1");
        public static Page FleetDestination { get; } = new Page(PageType.FleetDestination, "fleet2");
        public static Page FleetMission     { get; } = new Page(PageType.FleetMission    , "fleet3");
        public static Page FleetMovement    { get; } = new Page(PageType.FleetMovement   , "movement");
        public static Page Galaxy           { get; } = new Page(PageType.Galaxy          , "galaxy");
        public static Page GalaxyContent    { get; } = new Page(PageType.GalaxyContent   , "galaxyContent");
        public static Page Alliance         { get; } = new Page(PageType.Alliance        , "alliance");
        public static Page EventList        { get; } = new Page(PageType.EventList       , "eventList");
        public static Page Minifleet        { get; } = new Page(PageType.Minifleet       , "minifleet");
        public static Page ResourceSettings { get; } = new Page(PageType.ResourceSettings, "resourceSettings");
        public static Page Options          { get; } = new Page(PageType.Options         , "preferences");
        public static Page Highscores       { get; } = new Page(PageType.Highscores      , "highscore");
        public static Page Notes            { get; } = new Page(PageType.Notes           , "notices");
        public static Page Buddies          { get; } = new Page(PageType.Buddies         , "buddies");
        public static Page Chat             { get; } = new Page(PageType.Chat            , "chat");
        public static Page ResourceUpdate   { get; } = new Page(PageType.ResourceUpdate  , "fetchResources");
        public static Page Messages         { get; } = new Page(PageType.Messages        , "messages");
        public static Page AjaxChat         { get; } = new Page(PageType.AjaxChat        , "ajaxChat");
        public static Page Premium          { get; } = new Page(PageType.Premium         , "premium");
        public static Page Shop             { get; } = new Page(PageType.Shop            , "shop");

        static Page()
        {
        }

        public string Link { get; }

        private Page(PageType type, string link) : base(type)
        {
            Link = link;
        }

        public static implicit operator Page(PageType type)
        {
            return Index[type];
        }

        public static implicit operator PageType(Page type)
        {
            return type.Type;
        }
    }
}
