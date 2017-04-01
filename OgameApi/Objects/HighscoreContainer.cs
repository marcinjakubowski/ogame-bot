using System.Xml.Serialization;

namespace OgameApi.Objects
{
    [XmlRoot("highscore")]
    public partial class HighscoreContainer
    {
        [XmlElement("player", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public PlayerHighscore[] Highscores { get; set; }

        [XmlAttribute("timestamp")]
        public int Timestamp { get; set; }

        [XmlAttribute("serverId")]
        public string ServerId { get; set; }
    }
}