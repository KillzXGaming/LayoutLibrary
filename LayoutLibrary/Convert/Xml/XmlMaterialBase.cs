using LayoutLibrary.Cafe;
using LayoutLibrary.XmlConverter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace LayoutLibrary.XmlConverter
{
    public class XmlMaterialBase
    {
        [XmlAttribute]
        public string Name;

        [XmlAttribute]
        public int Index;

        public static XmlMaterialBase Create(BflytFile bflyt, ushort index)
        {
            var mat = bflyt.MaterialTable.Materials[index];

            if (mat is MaterialCafe)
                return new XmlMaterialCafe((MaterialCafe)mat, bflyt, index);

            return new XmlMaterialCafe((MaterialCafe)mat, bflyt, index);
        }

        public static MaterialBase ConvertBack(BflytFile bflyt, XmlMaterialBase xmlmat)
        {
            return ((XmlMaterialCafe)xmlmat).Create(bflyt);
        }
    }
}
