using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ClassicUO.Configuration;
using ClassicUO.Game.UI.Controls;
using Microsoft.Xna.Framework;

namespace ClassicUO.Game.UI.Gumps
{
    internal sealed class ProfileManagerGump : Gump
    {
        private const int WIDTH = 600;
        private const int HEIGHT = 400;
        private const ushort HUE_FONT = 0xFFFF;
        private readonly int GUMP_POS_X = (ProfileManager.CurrentProfile.GameWindowSize.X >> 2) - 125;
        private readonly int GUMP_POS_Y = 100;

        private const int FROM_COMBOBOX_OFFSET = 30;
        private const int TO_COMBOBOX_OFFSET = 350;


        private readonly string _profilePathName = Path.Combine(CUOEnviroment.ExecutablePath, "Data", "Profiles");

        private string fromPath = "";
        private string toPath = "";

        private List<string> _accList = new List<string>();

        /// From Section
        private Combobox _fromAccCombobox;
        private Combobox _fromServerCombobox;
        private Combobox _fromCharCombobox;

        private int _fromSelectedAccIdx;
        private int _fromSelectedServerIdx;
        private int _fromSelectedCharIdx;

        private List<string> _fromServerList = new List<string>();
        private List<string> _fromCharList = new List<string>();

        // To Section
        private Combobox _toAccCombobox;
        private Combobox _toServerCombobox;
        private Combobox _toCharCombobox;

        private int _toSelectedAccIdx;
        private int _toSelectedServerIdx;
        private int _toSelectedCharIdx;

        private List<string> _toServerList = new List<string>();
        private List<string> _toCharList = new List<string>();

        private List<string> filesToCopy = new List<string>();

        private ScrollArea _scrollArea;

        public ProfileManagerGump() : base(0, 0)
        {
            CanMove = true;

            Add(new AlphaBlendControl(0.05f)
            {
                X = GUMP_POS_X,
                Y = GUMP_POS_Y,
                Width = WIDTH - 2,
                Height = HEIGHT - 2,
                Hue = 999,
                AcceptMouseInput = true,
                CanCloseWithRightClick = true,
                CanMove = true
            });

            #region Boarder
            Add
            (
                new Line
                (
                    GUMP_POS_X,
                    GUMP_POS_Y,
                    WIDTH,
                    1,
                    Color.Gray.PackedValue
                )
            );

            Add
            (
                new Line
                (
                    GUMP_POS_X,
                    GUMP_POS_Y,
                    1,
                    HEIGHT,
                    Color.Gray.PackedValue
                )
            );

            Add
            (
                new Line
                (
                    GUMP_POS_X,
                    GUMP_POS_Y + HEIGHT,
                    WIDTH,
                    1,
                    Color.Gray.PackedValue
                )
            );

            Add
            (
                new Line
                (
                    GUMP_POS_X + WIDTH,
                    GUMP_POS_Y,
                    1,
                    HEIGHT,
                    Color.Gray.PackedValue
                )
            );
            #endregion

            GetAccList();

            Add
            (
                new Label
                (
                    "From Character", true, HUE_FONT, 0,
                    255, Renderer.FontStyle.BlackBorder
                )
                {
                    X = GUMP_POS_X + FROM_COMBOBOX_OFFSET + 50,
                    Y = GUMP_POS_Y + 5
                }
            );
            _fromAccCombobox = new Combobox(GUMP_POS_X + FROM_COMBOBOX_OFFSET, GUMP_POS_Y + 30, 200, _accList.ToArray());
            _fromAccCombobox.OnOptionSelected += OnFromAccOptionSelected;

            Add(_fromAccCombobox);

            Add
            (
                new Label
                (
                    "To Character", true, HUE_FONT, 0,
                    255, Renderer.FontStyle.BlackBorder
                )
                {
                    X = GUMP_POS_X + TO_COMBOBOX_OFFSET + 50,
                    Y = GUMP_POS_Y + 5
                }
            );

            _toAccCombobox = new Combobox(GUMP_POS_X + TO_COMBOBOX_OFFSET, GUMP_POS_Y + 30, 200, _accList.ToArray());
            _toAccCombobox.OnOptionSelected += OnToAccOptionSelected;

            Add(_toAccCombobox);

            Add(new GumpPic(GUMP_POS_X + 270, GUMP_POS_Y + 60, 0x1196, 0));

            Add
            (
                new NiceButton
                (
                    GUMP_POS_X + 270, GUMP_POS_Y + HEIGHT - 50, 60, 40,
                    ButtonAction.Activate, "Copy"
                )
                {
                    ButtonParameter = 1
                }
            );

            SetInScreen();
        }

        #region From Events
        private void OnFromAccOptionSelected(object sender, int args)
        {
            _fromSelectedAccIdx = args;

            Remove(_fromCharCombobox);
            _fromSelectedCharIdx = -1;

            Remove(_scrollArea);

            filesToCopy.Clear();

            fromPath = "";

            GetFromServerList();
        }

        private void OnFromServerOptionSelected(object sender, int args)
        {
            _fromSelectedServerIdx = args;

            Remove(_scrollArea);

            filesToCopy.Clear();

            fromPath = "";

            GetFromCharsList();
        }

        private void OnFromCharOptionSelected(object sender, int args)
        {
            _fromSelectedCharIdx = args;

            Remove(_scrollArea);

            filesToCopy.Clear();

            fromPath = Path.Combine(_profilePathName,
                                    _accList[_fromSelectedAccIdx],
                                    _fromServerList[_fromSelectedServerIdx],
                                    _fromCharList[_fromSelectedCharIdx]);

            BuildFilesList();
        }
        #endregion

        #region To Events
        private void OnToAccOptionSelected(object sender, int args)
        {
            Remove(_toCharCombobox);
            _toSelectedCharIdx = -1;
            _toSelectedAccIdx = args;
            toPath = "";
            GetToServerList();
        }

        private void OnToServerOptionSelected(object sender, int args)
        {
            _toSelectedServerIdx = args;
            toPath = "";
            GetToCharsList();
        }

        private void OnToCharOptionSelected(object sender, int args)
        {
            _toSelectedCharIdx = args;
            toPath = Path.Combine(_profilePathName,
                                  _accList[_toSelectedAccIdx],
                                  _toServerList[_toSelectedServerIdx],
                                  _toCharList[_toSelectedCharIdx]);
        }

        #endregion

        private void GetAccList()
        {
            _accList = GetDirsList(_profilePathName);
        }

        private void GetFromServerList()
        {
            var path = Path.Combine(_profilePathName, _accList[_fromSelectedAccIdx]);

            _fromServerList = GetDirsList(path);

            Remove(_fromServerCombobox);

            _fromServerCombobox = new Combobox(GUMP_POS_X + FROM_COMBOBOX_OFFSET, GUMP_POS_Y + 60, 200, _fromServerList.ToArray());
            _fromServerCombobox.OnOptionSelected += OnFromServerOptionSelected;

            Add(_fromServerCombobox);
        }

        private void GetFromCharsList()
        {
            var path = Path.Combine(_profilePathName, _accList[_fromSelectedAccIdx], _fromServerList[_fromSelectedServerIdx]);

            _fromCharList = GetDirsList(path);

            Remove(_fromCharCombobox);

            _fromCharCombobox = new Combobox(GUMP_POS_X + FROM_COMBOBOX_OFFSET, GUMP_POS_Y + 90, 200, _fromCharList.ToArray());
            _fromCharCombobox.OnOptionSelected += OnFromCharOptionSelected;

            Add(_fromCharCombobox);
        }

        private void GetToServerList()
        {
            var path = Path.Combine(_profilePathName, _accList[_toSelectedAccIdx]);

            _toServerList = GetDirsList(path);

            Remove(_toServerCombobox);

            _toServerCombobox = new Combobox(GUMP_POS_X + TO_COMBOBOX_OFFSET, GUMP_POS_Y + 60, 200, _toServerList.ToArray());
            _toServerCombobox.OnOptionSelected += OnToServerOptionSelected;

            Add(_toServerCombobox);
        }

        private void GetToCharsList()
        {
            var path = Path.Combine(_profilePathName, _accList[_toSelectedAccIdx], _toServerList[_toSelectedServerIdx]);

            _toCharList = GetDirsList(path);

            Remove(_toCharCombobox);

            _toCharCombobox = new Combobox(GUMP_POS_X + TO_COMBOBOX_OFFSET, GUMP_POS_Y + 90, 200, _toCharList.ToArray());
            _toCharCombobox.OnOptionSelected += OnToCharOptionSelected;

            Add(_toCharCombobox);
        }

        private void BuildFilesList()
        {
            Remove(_scrollArea);

            _scrollArea = new ScrollArea
            (
                GUMP_POS_X + 20, GUMP_POS_Y + 140, WIDTH - 40, 200,
                true
            );

            string[] files = Directory.GetFiles(fromPath);
            
            var initY = 0;

            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);

                var checkbox = new Checkbox
                (
                    0xD2, 0xD3, fileInfo.Name, 0,
                    HUE_FONT
                )
                {
                    Y = initY
                };
                checkbox.ValueChanged += OnSelectItem;
                
                _scrollArea.Add(checkbox);
                initY += 25;
            }

            Add(_scrollArea);
        }

        private void OnSelectItem(object sender, EventArgs e)
        {
            if (!(sender is Checkbox c))
            {
                return;
            }

            if (c.IsChecked)
                filesToCopy.Add(c.Text);
            else
                filesToCopy.Remove(c.Text);
        }

        private void DoCopyFiles()
        {
            if (string.IsNullOrEmpty(fromPath) && string.IsNullOrEmpty(toPath))
            {
                return;
            }

            foreach (var fName in filesToCopy)
            {
                var fPath = Path.Combine(fromPath, fName);
                var tPath = Path.Combine(toPath, fName);

                if (File.Exists(fPath))
                {
                    File.Copy(fPath, tPath, true);
                }
            }

            ProfileManager.CurrentProfile.ReloadGumps(ProfileManager.ProfilePath);
        }

        private static List<string> GetDirsList(string path)
        {
            return Directory.GetDirectories(path, "*", SearchOption.TopDirectoryOnly)
                            .Select(folder => new DirectoryInfo(folder))
                            .Select(dirInfo => dirInfo.Name)
                            .ToList();
        }

        public override void OnButtonClick(int buttonId)
        {
            switch (buttonId)
            {
                case 1:
                    DoCopyFiles();
                    break;
            }
        }
    }
}
