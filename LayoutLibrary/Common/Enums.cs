using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LayoutLibrary
{
    public enum AnimCurveType
    {
        Constant,
        Step,
        Hermite,
    }

    public enum UserDataType
    {
        String = 0,
        Int = 1,
        Float = 2,
        SystemData = 3, //v9.0 >=
    }
    public enum TexGenMatrixType : byte
    {
        Matrix2x4 = 0
    }

    public enum LineAlign : byte
    {
        Unspecified = 0,
        Left = 1,
        Center = 2,
        Right = 3,
    };

    public enum GfxBlendFactor : byte
    {
        Factor0 = 0,
        Factor1 = 1,
        DestColor = 2,
        DestInvColor = 3,
        SourceAlpha = 4,
        SourceInvAlpha = 5,
        DestAlpha = 6,
        DestInvAlpha = 7,
        SourceColor = 8,
        SourceInvColor = 9
    }

    public enum GfxBlendOp : byte
    {
        Disable = 0,
        Add = 1,
        Subtract = 2,
        ReverseSubtract = 3,
        SelectMin = 4,
        SelectMax = 5
    }

    public enum GfxLogicOp : byte
    {
        Disable = 0,
        NoOp = 1,
        Clear = 2,
        Set = 3,
        Copy = 4,
        InvCopy = 5,
        Inv = 6,
        And = 7,
        Nand = 8,
        Or = 9,
        Nor = 10,
        Xor = 11,
        Equiv = 12,
        RevAnd = 13,
        InvAd = 14,
        RevOr = 15,
        InvOr = 16
    }

    public enum GfxAlphaFunction : byte
    {
        Never = 0,
        Less = 1,
        LessOrEqual = 2,
        Equal = 3,
        NotEqual = 4,
        GreaterOrEqual = 5,
        Greater = 6,
        Always = 7,
    }

    public enum GfxAlphaOp : byte
    {
        And = 0,
        Or = 1,
        Xor = 2,
        Nor = 3,
    }
    public enum TexGenType : byte
    {
        TextureCoord0 = 0,
        TextureCoord1 = 1,
        TextureCoord2 = 2,
        OrthographicProjection = 3,
        PaneBasedProjection = 4,
        PerspectiveProjection = 5,
        UnknownType6 = 6,
        BrickRepeat = 7,
    }

    public enum PartPaneScaling
    {
        Scaling = 0,
        Ignore = 1,
        FitBoundries = 2,
    }

    public enum FilterMode
    {
        Near = 0,
        Linear = 1
    }

    public enum WrapMode
    {
        Clamp = 0,
        Repeat = 1,
        Mirror = 2
    }

    public enum OriginX : byte
    {
        Center = 0,
        Left = 1,
        Right = 2
    };

    public enum OriginY : byte
    {
        Center = 0,
        Top = 1,
        Bottom = 2
    };

    public enum WindowKind
    {
        Around = 0,
        Horizontal = 1,
        HorizontalNoContent = 2
    }

    public enum WindowFrameTexFlip : byte
    {
        None = 0,
        FlipH = 1,
        FlipV = 2,
        Rotate90 = 3,
        Rotate180 = 4,
        Rotate270 = 5
    }

    public enum AnimationTargetType
    {
        Pane = 0,
        Material = 1,
        User = 2,
    }
}
