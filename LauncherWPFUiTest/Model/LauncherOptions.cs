using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace LauncherWPFUiTest.Model
{
    [XmlRoot("LauncherOptions")]
    public class LauncherOptions
    {
        [XmlElement("UncPath")]
        public string UncPath { get; set; }

        [XmlElement("UserName")]
        public string UserName { get; set; }

        [XmlElement("UserPassword")]
        public string UserPassword { get; set; }

    }
}
