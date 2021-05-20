using System.Collections.Generic;

namespace ClassicUO.Game.Data
{
    internal static class SailTable
    {
        /// <summary>
        ///     Hide Sails Graphic to be replace
        /// </summary>
        public static Dictionary<ushort, ushort> HideSailsGraphic = new Dictionary<ushort, ushort>();

        /// <summary>
        ///     Hide Sails Invisibility to be hide
        /// </summary>
        public static HashSet<ushort> HideSailsInvisibility = new HashSet<ushort>();

        /// <summary>
        ///     Beter Sails Graphic to be replace
        /// </summary>
        public static Dictionary<ushort, ushort> BeterSailsGraphic = new Dictionary<ushort, ushort>();

        /// <summary>
        ///     Load Sails Graphic
        /// </summary>
        public static void Load()
        {
            HideSailsGraphic = _initialHideSailsGraphic;
            HideSailsInvisibility = _initialHideSailsInvisibility;
            BeterSailsGraphic = _initalBeterSailsGraphic;
        }

        /// <summary>
        ///     Initial Hide Sails Graphic
        /// </summary>
        private static Dictionary<ushort, ushort> _initialHideSailsGraphic = new Dictionary<ushort, ushort>
        {
            { 15959, 50235 },
            { 15962, 50236 },
            { 15980, 50237 },
            { 15986, 50238 },
            { 16072, 50239 },
            { 16075, 50240 },
            { 16093, 50241 },
            { 16098, 50242 },
        };

        /// <summary>
        ///     Initial Hide Sails Invisibility
        /// </summary>
        private static HashSet<ushort> _initialHideSailsInvisibility = new HashSet<ushort>
        {
            15935,
            15936,
            15937,
            15938,
            15960,
            15961,
            15963,
            15964,
            15978,
            15979,
            15981,
            15982,
            15984,
            15985,
            15987,
            15988,
            16073,
            16074,
            16076,
            16078,
            16092,
            16094,
            16095,
            16096,
            16097,
            16099,
            16100,
            16101
        };

        /// <summary>
        ///     Initial Beter Sails Graphic
        /// </summary>
        private static Dictionary<ushort, ushort> _initalBeterSailsGraphic = new Dictionary<ushort, ushort>
        {
            { 15935, 50200 },
            { 15936, 50201 },
            { 15937, 50202 },
            { 15938, 50203 },
            { 15959, 50204 },
            { 15960, 50205 },
            { 15961, 50206 },
            { 15962, 50207 },
            { 15963, 50208 },
            { 15964, 50209 },
            { 15978, 50210 },
            { 15979, 50211 },
            { 15980, 50212 },
            { 15981, 50213 },
            { 15982, 50214 },
            { 15984, 50215 },
            { 15985, 50216 },
            { 15986, 50217 },
            { 15987, 50218 },
            { 15988, 50219 },
            { 16072, 50220 },
            { 16073, 50221 },
            { 16074, 50222 },
            { 16075, 50223 },
            { 16076, 50224 },
            { 16092, 50225 },
            { 16093, 50226 },
            { 16094, 50227 },
            { 16095, 50228 },
            { 16096, 50229 },
            { 16097, 50230 },
            { 16098, 50231 },
            { 16099, 50232 },
            { 16100, 50233 },
            { 16101, 50234 },
        };
    }
}
