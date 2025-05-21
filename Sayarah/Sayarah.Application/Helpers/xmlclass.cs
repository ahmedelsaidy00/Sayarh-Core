using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Sayarah.Application.Helpers
{
	// using System.Xml.Serialization;
	// XmlSerializer serializer = new XmlSerializer(typeof(Urlset));
	// using (StringReader reader = new StringReader(xml))
	// {
	//    var test = (Urlset)serializer.Deserialize(reader);
	// }

	[XmlRoot(ElementName = "url")]
	public class Url
	{

		[XmlElement(ElementName = "loc")]
		public string Loc { get; set; }

		[XmlElement(ElementName = "lastmod")]
		public DateTime Lastmod { get; set; }

		[XmlElement(ElementName = "priority")]
		public double Priority { get; set; }
	}

	[XmlRoot(ElementName = "urlset")]
	public class Urlset
	{

		[XmlElement(ElementName = "url")]
		public List<Url> Url { get; set; }

		[XmlAttribute(AttributeName = "xmlns")]
		public string Xmlns { get; set; }

		[XmlText]
		public string Text { get; set; }
	}

}
