using Autodesk.AutoCAD.Colors;

namespace RabCab.Utilities.Settings
{
    public static class Colors
    {
        #region Layer Colors

        public static Color LayerColorRcAnno = Color.FromColorIndex(ColorMethod.ByAci, 7);
        public static Color LayerColorRcVisible = Color.FromColorIndex(ColorMethod.ByAci, 7);
        public static Color LayerColorRcHidden = Color.FromColorIndex(ColorMethod.ByAci, 2);
        public static Color LayerColorConverge = Color.FromColorIndex(ColorMethod.ByAci, 1);
        public static Color LayerColorDefpoints = Color.FromColorIndex(ColorMethod.ByAci, 9);

        #endregion
    }
}