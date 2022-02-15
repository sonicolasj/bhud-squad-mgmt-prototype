using Blish_HUD;
using Blish_HUD.ArcDps.Common;
using Blish_HUD.Controls;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

using static Blish_HUD.GameService;

namespace Blish_HUD_Module1
{
    [Export(typeof(Blish_HUD.Modules.Module))]
    public class Module : Blish_HUD.Modules.Module
    {
        private static readonly Logger Logger = Logger.GetLogger<Module>();

        #region Service Managers
        //internal SettingsManager SettingsManager => this.ModuleParameters.SettingsManager;
        internal ContentsManager ContentsManager => this.ModuleParameters.ContentsManager;
        //internal DirectoriesManager DirectoriesManager => this.ModuleParameters.DirectoriesManager;
        //internal Gw2ApiManager Gw2ApiManager => this.ModuleParameters.Gw2ApiManager;
        #endregion

        private StandardWindow _window;

        private readonly IList<Label> _labels = new List<Label>();
        private readonly IList<Dropdown> _dropdowns = new List<Dropdown>();
        private readonly IList<CommonFields.Player> _players = new List<CommonFields.Player>();

        [ImportingConstructor]
        public Module([Import("ModuleParameters")] ModuleParameters moduleParameters) : base(moduleParameters) { }

        protected override void OnModuleLoaded(EventArgs e)
        {
            ArcDps.Common.Activate();
            ArcDps.Common.PlayerAdded += OnPlayerAdded;
            ArcDps.Common.PlayerRemoved += OnPlayerRemoved;

            BuildWindow();

            _window.Show();

            // Base handler must be called
            base.OnModuleLoaded(e);
        }

        private void BuildWindow()
        {
            _window = new StandardWindow(
                ContentsManager.GetTexture("155985.png"),
                new Rectangle(40, 26, 913, 691),
                new Rectangle(70, 71, 839, 605)
            )
            {
                Parent = Graphics.SpriteScreen
            };

            var offset = 0;
            const int margin = 5;

            for (int i = 0; i < 10; i++)
            {
                var label = new Label
                {
                    Parent = _window,
                    Text = $"------",
                    AutoSizeWidth = true,
                    Top = offset,
                };

                _labels.Add(label);

                var dropdown = new Dropdown
                {
                    Parent = _window,
                    Top = offset,
                    Width = 100,
                    Left = label.Width + margin,
                };

                _dropdowns.Add(dropdown);

                dropdown.Items.Add("DPS");
                dropdown.Items.Add("BS");
                dropdown.Items.Add("Quickness");
                dropdown.Items.Add("Alac");
                dropdown.Items.Add("HFB");
                dropdown.Items.Add("Druid");

                offset += label.Height + margin;
            }
        }

        private void OnPlayerAdded(CommonFields.Player player)
        {
            if (_players.Any(p => p.AccountName == player.AccountName)) return;

            var newIndex = _players.Count;
            _players.Add(player);

            _labels[newIndex].Text = $"{player.CharacterName} ({player.AccountName})";

            _dropdowns[newIndex].Left = _labels[newIndex].Width + 5;
        }

        private void OnPlayerRemoved(CommonFields.Player player)
        {
            if (_players.All(p => p.AccountName != player.AccountName)) return;

            var knownPlayer = _players.FirstOrDefault(p => p.AccountName == player.AccountName);
            var index = _players.IndexOf(knownPlayer);
            _players.Remove(knownPlayer);

            var previousRole = _dropdowns[index].SelectedItem;

            _labels[index].Text = $"------";
            _dropdowns[index].SelectedItem = "DPS";
            _dropdowns[index].Left = _labels[index].Width + 5;

            ScreenNotification.ShowNotification($"{player.CharacterName} ({player.AccountName}) - {previousRole} - left the instance");
        }

        /// <inheritdoc />
        protected override void Unload()
        {
            ArcDps.Common.PlayerAdded -= OnPlayerAdded;
            ArcDps.Common.PlayerRemoved -= OnPlayerRemoved;

            _players.Clear();

            foreach (var label in _labels) label?.Dispose();
            _labels.Clear();

            foreach (var dropdown in _dropdowns) dropdown?.Dispose();
            _dropdowns.Clear();

            _window.Dispose();
        }
    }
}
