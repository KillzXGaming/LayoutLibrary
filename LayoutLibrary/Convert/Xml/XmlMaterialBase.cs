using LayoutLibrary.Cafe;
using LayoutLibrary.Sections.Rev;
using LayoutLibrary.XmlConverter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
            if (mat is MaterialCtr)
                return new XmlMaterialCtr((MaterialCtr)mat, bflyt, index);
            if (mat is MaterialRev)
                return new XmlMaterialRev((MaterialRev)mat, bflyt, index);

            return new XmlMaterialCafe((MaterialCafe)mat, bflyt, index);
        }

        public static MaterialBase ConvertBack(BflytFile bflyt, XmlMaterialBase xmlmat)
        {
            if (xmlmat is XmlMaterialCtr)
                return ((XmlMaterialCtr)xmlmat).Create(bflyt);
            if (xmlmat is XmlMaterialRev)
                return ((XmlMaterialRev)xmlmat).Create(bflyt);

            return ((XmlMaterialCafe)xmlmat).Create(bflyt);
        }
    }
}
