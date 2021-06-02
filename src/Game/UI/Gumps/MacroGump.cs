
using ClassicUO.Configuration;
using ClassicUO.Game.UI.Controls;

namespace ClassicUO.Game.UI.Gumps
{
    internal class MacroGump : Gump
    {

        public MacroGump(string name) : base(0, 0)
        {
            AlphaBlendControl partyGumpBackground = new AlphaBlendControl
            {
                Width = 300,
                Height = 200,
                X = ProfileManager.CurrentProfile.GameWindowSize.X / 2 - 125,
                Y = 150,
                Alpha = 0.2f
            };

            Label text = new Label($"Edit macro: {name}", true, 15)
            {
                X = ProfileManager.CurrentProfile.GameWindowSize.X / 2 - 115,
                Y = partyGumpBackground.Y + 5
            };

            Add(partyGumpBackground);
            Add(text);

            Add
            (
                new MacroControl(name)
                {
                    X = partyGumpBackground.X + 20,
                    Y = partyGumpBackground.Y + 20,
                }
            );

            SetInScreen();
        }
    }
}
