using ClassicUO.Game.Managers;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Renderer;
using Microsoft.Xna.Framework;
using static ClassicUO.Game.UI.Gumps.JournalGump;

namespace ClassicUO.Game.UI.Gumps
{
    internal class ModernJournalGump : Gump
    {
        private const int WIDTH = 600;
        private const int HEIGHT = 500;
        private const int CATEGORY_BTN_HEIGHT = 25;
        private const ushort HUE_FONT = 0xFFFF;

        private RenderedTextList _journalEntries;

        public override GumpType GumpType => GumpType.Journal;

        public ModernJournalGump()
            : base(0,0)
        {
            Height = 400;
            CanMove = true;
            AcceptKeyboardInput = false;
            AcceptMouseInput = false;

            Add
            (
                new AlphaBlendControl(0.05f)
                {
                    X = 1,
                    Y = 1,
                    Width = WIDTH - 2,
                    Height = HEIGHT - 2,
                    Hue = 999,
                    AcceptMouseInput = true,
                    CanCloseWithRightClick = true,
                    CanMove = true,
                }
            );

            #region Boarder
            Add
            (
                new Line
                (
                    0,
                    0,
                    WIDTH,
                    1,
                    Color.Gray.PackedValue
                )
            );

            Add
            (
                new Line
                (
                    0,
                    0,
                    1,
                    HEIGHT,
                    Color.Gray.PackedValue
                )
            );

            Add
            (
                new Line
                (
                    0,
                    HEIGHT,
                    WIDTH,
                    1,
                    Color.Gray.PackedValue
                )
            );

            Add
            (
                new Line
                (
                    WIDTH,
                    0,
                    1,
                    HEIGHT,
                    Color.Gray.PackedValue
                )
            );
            #endregion

            Add
            (
                new Line
                (
                    0,
                    CATEGORY_BTN_HEIGHT,
                    WIDTH,
                    1,
                    Color.Gray.PackedValue
                )
            );

            var b = new NiceButton(
                0,
                0,
                40,
                CATEGORY_BTN_HEIGHT,
                ButtonAction.SwitchPage,
                "Knefel",
                0
                )
            {
                ButtonParameter = 1,
                IsSelectable = true,
            };
            Add(b);
            var c = new NiceButton(
                b.Width,
                0,
                40,
                CATEGORY_BTN_HEIGHT,
                ButtonAction.SwitchPage,
                "Knefel",
                0
                )
            {
                ButtonParameter = 2,
                IsSelectable = true,
            };
            Add(c);

            BuildAll();
            BuildGuild();


            InitializeJournalEntries();
            World.Journal.EntryAdded += AddJournalEntry;

            SetInScreen();
        }
        private void InitializeJournalEntries()
        {
            foreach (JournalEntry t in JournalManager.Entries)
            {
                AddJournalEntry(null, t);
            }
        }

        private void AddJournalEntry(object sender, JournalEntry entry)
        {
            var usrSend = entry.Name != string.Empty ? $"{entry.Name}" : string.Empty;

            // Check if ignored person
            if (!string.IsNullOrEmpty(usrSend) && IgnoreManager.IgnoredCharsList.Contains(usrSend))
                return;

            string text = $"{usrSend}: {entry.Text}";

            _journalEntries.AddEntry
            (
                text,
                entry.Font,
                entry.Hue,
                entry.IsUnicode,
                entry.Time,
                entry.TextType
            );
        }

        private void BuildAll()
        {
            const int PAGE = 1;

            ScrollArea rightArea = new ScrollArea
            (
                5,
                20,
                WIDTH - 210,
                420,
                true
            );

            Add
            (
                _journalEntries = new RenderedTextList
                (
                    0,
                    CATEGORY_BTN_HEIGHT + 5,
                    WIDTH,
                    200,
                    rightArea.ScrollBar
                )
            );
            rightArea.Add(_journalEntries);

            Add(rightArea, PAGE);
        }

        private void BuildGuild()
        {
            const int PAGE = 2;

            ScrollArea rightArea = new ScrollArea
            (
                5,
                20,
                WIDTH - 210,
                420,
                true
            );

            rightArea.Add(new Label("TAKI", true, HUE_FONT));

            Add(rightArea, PAGE);
        }

    }
}
