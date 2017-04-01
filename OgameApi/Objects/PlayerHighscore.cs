using System.Xml.Serialization;

namespace OgameApi.Objects
{
    public partial class PlayerHighscore
    {
        [XmlAttribute("id")]
        public int Id { get; set; }

        [XmlAttribute("position")]
        public int Position { get; set; }

        [XmlAttribute("score")]
        public int Score { get; set; }
    }
}