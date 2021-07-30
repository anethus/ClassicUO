using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ClassicUO.Configuration;
using ClassicUO.Game.Managers;
using ClassicUO.Game.Scenes;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Resources;
using Microsoft.Xna.Framework;

namespace ClassicUO.Game.UI.Gumps
{
    internal sealed class ProfileManagerGump : Gump
    {
        private const string DATA_FOLDER = "Data";
        private const string PROFILES_FOLDER = "Profiles";
        private const string GUMP_FILE_NAME = "gumps.xml";

        private const int WIDTH = 600;
        private const int HEIGHT = 400;
        private const ushort HUE_FONT = 0xFFFF;
        private readonly int GUMP_POS_X = (ProfileManager.CurrentProfile.GameWindowSize.X >> 2) - 125;
        private readonly int GUMP_POS_Y = 100;

        private const int FROM_COMBOBOX_OFFSET = 30;
        private const int TO_COMBOBOX_OFFSET = 350;

        private readonly string _profilePathName = Path.Combine(CUOEnviroment.ExecutablePath, DATA_FOLDER, PROFILES_FOLDER);

        private string _fromPath = string.Empty;
        private string _toPath = string.Empty;

        private List<string> _accList = new List<string>();

        private Combobox _fromServerCombobox;
        private Combobox _fromCharCombobox;

        private int _fromSelectedAccIdx;
        private int _fromSelectedServerIdx;
        private int _fromSelectedCharIdx;

        private List<string> _fromServerList = new List<string>();
        private List<string> _fromCharList = new List<string>();

        // To Section
        private Combobox _toServerCombobox;
        private Combobox _toCharCombobox;

        private int _toSelectedAccIdx;
        private int _toSelectedServerIdx;
        private int _toSelectedCharIdx;

        private List<string> _toServerList = new List<string>();
        private List<string> _toCharList = new List<string>();

        private readonly List<string> _filesToCopy = new List<string>();

        private ScrollArea _scrollArea;

        /// <summary>
        /// Default Constructor
        /// </summary>
        public ProfileManagerGump() : base(0, 0)
        {
            CanMove = true;

            Add(new AlphaBlendControl(0.05f)
            {
                X = GUMP_POS_X,
                Y = GUMP_POS_Y,
                Width = WIDTH,
                Height = HEIGHT,
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

            Add(new Label(ResGumps.ProfileCopyAccount, true, HUE_FONT) { X = GUMP_POS_X + 270, Y = GUMP_POS_Y + 30 });
            Add(new Label(ResGumps.ProfileCopyServer, true, HUE_FONT) { X = GUMP_POS_X + 270, Y = GUMP_POS_Y + 60 });
            Add(new Label(ResGumps.ProfileCopyCharacter, true, HUE_FONT) { X = GUMP_POS_X + 270, Y = GUMP_POS_Y + 90 });

            Add
            (
                new Label
                (
                    ResGumps.ProfileCopyFrom, true, HUE_FONT, 0,
                    255, Renderer.FontStyle.BlackBorder
                )
                {
                    X = GUMP_POS_X + FROM_COMBOBOX_OFFSET + 50,
                    Y = GUMP_POS_Y + 5
                }
            );
            Combobox fromAccCombobox = new Combobox(GUMP_POS_X + FROM_COMBOBOX_OFFSET, GUMP_POS_Y + 30, 200, _accList.ToArray());
            fromAccCombobox.OnOptionSelected += OnFromAccOptionSelected;

            Add(fromAccCombobox);

            Add
            (
                new Label
                (
                    ResGumps.ProfileCopyTo, true, HUE_FONT, 0,
                    255, Renderer.FontStyle.BlackBorder
                )
                {
                    X = GUMP_POS_X + TO_COMBOBOX_OFFSET + 50,
                    Y = GUMP_POS_Y + 5
                }
            );

            Combobox toAccCombobox = new Combobox(GUMP_POS_X + TO_COMBOBOX_OFFSET, GUMP_POS_Y + 30, 200, _accList.ToArray());
            toAccCombobox.OnOptionSelected += OnToAccOptionSelected;

            Add(toAccCombobox);

            Add(new GumpPic(GUMP_POS_X + 270, GUMP_POS_Y + -10, 0x1196, 0));
            
            Add
            (
                new NiceButton
                (
                    GUMP_POS_X + 230, GUMP_POS_Y + HEIGHT - 50, 60, 40,
                    ButtonAction.Activate, ResGumps.ProfileCopyCopy
                )
                {
                    ButtonParameter = (int)ButtonsId.DO_COPY
                }
            );

            Add
            (
                new NiceButton
                (
                    GUMP_POS_X + 330, GUMP_POS_Y + HEIGHT - 50, 60, 40,
                    ButtonAction.Activate, ResGumps.ProfileCopyCancel
                )
                {
                    ButtonParameter = (int)ButtonsId.CANCEL
                }
            );

            SetInScreen();
        }

        #region From Events
        /// <summary>
        /// On From Acc Option Selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnFromAccOptionSelected(object sender, int args)
        {
            _fromSelectedAccIdx = args;

            Remove(_fromCharCombobox);
            _fromSelectedCharIdx = -1;

            Remove(_scrollArea);

            _filesToCopy.Clear();

            _fromPath = "";

            GetFromServerList();
        }

        /// <summary>
        /// On From Server Option Selected - clear path and clear files area
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnFromServerOptionSelected(object sender, int args)
        {
            _fromSelectedServerIdx = args;

            Remove(_scrollArea);

            _filesToCopy.Clear();

            _fromPath = string.Empty;

            GetFromCharsList();
        }

        /// <summary>
        /// On From Char Option Selected - build from path
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnFromCharOptionSelected(object sender, int args)
        {
            _fromSelectedCharIdx = args;

            Remove(_scrollArea);

            _filesToCopy.Clear();

            _fromPath = Path.Combine(_profilePathName,
                                    _accList[_fromSelectedAccIdx],
                                    _fromServerList[_fromSelectedServerIdx],
                                    _fromCharList[_fromSelectedCharIdx]);

            BuildFilesList();
        }
        #endregion

        #region To Events
        /// <summary>
        /// On To Acc Option Selected - clear prev char idx and clear to path
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnToAccOptionSelected(object sender, int args)
        {
            Remove(_toCharCombobox);
            _toSelectedCharIdx = -1;
            _toSelectedAccIdx = args;
            _toPath = string.Empty;
            GetToServerList();
        }

        /// <summary>
        /// On To server Option Selected - clear prev path
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnToServerOptionSelected(object sender, int args)
        {
            _toSelectedServerIdx = args;
            _toPath = string.Empty;
            GetToCharsList();
        }

        /// <summary>
        /// On To Chat Option Selected - build to Path
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnToCharOptionSelected(object sender, int args)
        {
            _toSelectedCharIdx = args;
            _toPath = Path.Combine(_profilePathName,
                                  _accList[_toSelectedAccIdx],
                                  _toServerList[_toSelectedServerIdx],
                                  _toCharList[_toSelectedCharIdx]);
        }

        #endregion

        /// <summary>
        /// Get Accounts list
        /// </summary>
        private void GetAccList()
        {
            _accList = GetDirsList(_profilePathName);
        }

        /// <summary>
        /// Get From Server List
        /// </summary>
        private void GetFromServerList()
        {
            var path = Path.Combine(_profilePathName, _accList[_fromSelectedAccIdx]);

            _fromServerList = GetDirsList(path);

            Remove(_fromServerCombobox);

            _fromServerCombobox = new Combobox(GUMP_POS_X + FROM_COMBOBOX_OFFSET, GUMP_POS_Y + 60, 200, _fromServerList.ToArray());
            _fromServerCombobox.OnOptionSelected += OnFromServerOptionSelected;

            Add(_fromServerCombobox);
        }

        /// <summary>
        /// Get From Chars List
        /// </summary>
        private void GetFromCharsList()
        {
            var path = Path.Combine(_profilePathName, _accList[_fromSelectedAccIdx], _fromServerList[_fromSelectedServerIdx]);

            _fromCharList = GetDirsList(path);

            Remove(_fromCharCombobox);
            
            _fromCharCombobox = new Combobox(GUMP_POS_X + FROM_COMBOBOX_OFFSET, GUMP_POS_Y + 90, 200, _fromCharList.ToArray());
            _fromCharCombobox.OnOptionSelected += OnFromCharOptionSelected;

            Add(_fromCharCombobox);
        }

        /// <summary>
        /// Get To Server List
        /// </summary>
        private void GetToServerList()
        {
            var path = Path.Combine(_profilePathName, _accList[_toSelectedAccIdx]);

            _toServerList = GetDirsList(path);

            Remove(_toServerCombobox);

            _toServerCombobox = new Combobox(GUMP_POS_X + TO_COMBOBOX_OFFSET, GUMP_POS_Y + 60, 200, _toServerList.ToArray());
            _toServerCombobox.OnOptionSelected += OnToServerOptionSelected;

            Add(_toServerCombobox);
        }

        /// <summary>
        /// Get To Character List 
        /// </summary>
        private void GetToCharsList()
        {
            var path = Path.Combine(_profilePathName, _accList[_toSelectedAccIdx], _toServerList[_toSelectedServerIdx]);

            _toCharList = GetDirsList(path);

            Remove(_toCharCombobox);
            
            _toCharCombobox = new Combobox(GUMP_POS_X + TO_COMBOBOX_OFFSET, GUMP_POS_Y + 90, 200, _toCharList.ToArray());
            _toCharCombobox.OnOptionSelected += OnToCharOptionSelected;

            Add(_toCharCombobox);
        }

        /// <summary>
        /// Build Files list
        /// </summary>
        private void BuildFilesList()
        {
            // Remove Old Scroll List
            Remove(_scrollArea);

            _scrollArea = new ScrollArea
            (
                GUMP_POS_X + 20, GUMP_POS_Y + 140, WIDTH - 40, 200,
                true
            );

            string[] files = Directory.GetFiles(_fromPath);
            
            var initY = 0;

            // Add Checkbox for file
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

        // On Select Item Event - Add it to list or remove it - depend of selection
        private void OnSelectItem(object sender, EventArgs e)
        {
            if (!(sender is Checkbox c))
            {
                return;
            }

            if (c.IsChecked)
                _filesToCopy.Add(c.Text);
            else
                _filesToCopy.Remove(c.Text);
        }

        /// <summary>
        /// Do Copy File
        /// </summary>
        private void DoCopyFiles()
        {
            // Check for dummy select
            if (string.IsNullOrEmpty(_fromPath) && string.IsNullOrEmpty(_toPath))
            {
                return;
            }

            // Foreach selected file copy it
            foreach (var fName in _filesToCopy)
            {
                var fPath = Path.Combine(_fromPath, fName);
                var tPath = Path.Combine(_toPath, fName);

                if (File.Exists(fPath))
                {
                    File.Copy(fPath, tPath, true);
                }
            }

            // If there was gump file to copy reload gumps
            if(_filesToCopy.Contains(GUMP_FILE_NAME))
                ProfileManager.CurrentProfile.ReloadGumps(ProfileManager.ProfilePath);

            // If there was something to copy display Message Box 
            if (_filesToCopy.Count > 0)
            {
                ProfileManager.PreventSave = true;
                UIManager.Add(new MessageGump(ResGumps.ProfileCopyAskLogout));
                Dispose();
            }
        }

        /// <summary>
        /// Get Dirs List for Path
        /// </summary>
        /// <param name="path">Path</param>
        /// <returns>List of Directories</returns>
        private static List<string> GetDirsList(string path)
        {
            return Directory.GetDirectories(path, "*", SearchOption.TopDirectoryOnly)
                            .Select(folder => new DirectoryInfo(folder))
                            .Select(dirInfo => dirInfo.Name)
                            .ToList();
        }

        /// <summary>
        /// On Button Click
        /// </summary>
        /// <param name="buttonId">Button Id</param>
        public override void OnButtonClick(int buttonId)
        {
            switch (buttonId)
            {
                case (int)ButtonsId.DO_COPY:
                    DoCopyFiles();
                    break;
                case (int)ButtonsId.CANCEL:
                    Dispose();
                    break;
            }
        }

        /// <summary>
        /// Buttons Ids
        /// </summary>
        private enum ButtonsId
        {
            DO_COPY = 1,
            CANCEL
        }

    }

    /// <summary>
    /// Message Gump Displayed to User
    /// </summary>
    internal sealed class MessageGump : Gump
    {
        private const int WIDTH = 350;
        private const int HEIGHT = 100;
        private const ushort HUE_FONT = 0xFFFF;
        private readonly int MESSAGE_GUMP_POS_X = ProfileManager.CurrentProfile.GameWindowSize.X >> 2;
        private readonly int MESSAGE_GUMP_POS_Y = ProfileManager.CurrentProfile.GameWindowSize.Y >> 2;

        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <param name="message"></param>
        public MessageGump(string message) : base(0, 0)
        {
            CanMove = true;

            Add(new AlphaBlendControl(0.05f)
            {
                X = MESSAGE_GUMP_POS_X,
                Y = MESSAGE_GUMP_POS_Y,
                Width = WIDTH,
                Height = HEIGHT,
                Hue = 999,
                AcceptMouseInput = true,
                CanCloseWithRightClick = true,
                CanMove = true
            });

            #region Border Draw
            Add
            (
                new Line(MESSAGE_GUMP_POS_X, MESSAGE_GUMP_POS_Y, WIDTH, 1, Color.Gray.PackedValue)
            );

            Add
            (
                new Line(MESSAGE_GUMP_POS_X, MESSAGE_GUMP_POS_Y, 1, HEIGHT, Color.Gray.PackedValue)
            );

            Add
            (
                new Line(MESSAGE_GUMP_POS_X, MESSAGE_GUMP_POS_Y + HEIGHT, WIDTH, 1, Color.Gray.PackedValue)
            );

            Add
            (
                new Line(WIDTH + MESSAGE_GUMP_POS_X, MESSAGE_GUMP_POS_Y, 1, HEIGHT, Color.Gray.PackedValue)
            );
            #endregion

            Add(new Label(message, true, HUE_FONT)
            {
                X = MESSAGE_GUMP_POS_X + 20,
                Y = MESSAGE_GUMP_POS_Y + 10
            });

            Add
            (
                new NiceButton
                (
                    MESSAGE_GUMP_POS_X + 100, MESSAGE_GUMP_POS_Y + 60, 60, 20,
                    ButtonAction.Activate, ResGumps.ProfileCopyLogout
                )
                { ButtonParameter = (int)ButtonsId.LOGOUT }
            );

            Add
            (
                new NiceButton
                (
                    MESSAGE_GUMP_POS_X + 180, MESSAGE_GUMP_POS_Y + 60, 60, 20,
                    ButtonAction.Activate, ResGumps.ProfileCopyCancel
                )
                { ButtonParameter = (int)ButtonsId.CANCEL }
            );

            SetInScreen();
        }

        /// <summary>
        /// On Button Click 
        /// </summary>
        /// <param name="buttonId">Button Id</param>
        public override void OnButtonClick(int buttonId)
        {
            switch (buttonId)
            {
                case (int)ButtonsId.LOGOUT:
                    LogoutChar();
                    break;
                case (int)ButtonsId.CANCEL:
                    Dispose();
                    break;
            }
        }

        /// <summary>
        /// Display Quit Game Request Gump
        /// </summary>
        private void LogoutChar()
        {
            Client.Game.GetScene<GameScene>()?.RequestQuitGame();
            Dispose();
        }

        /// <summary>
        /// Buttons Ids
        /// </summary>
        private enum ButtonsId
        {
            LOGOUT = 1,
            CANCEL
        }
    }
}
