using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace EnduranceTheMaze
{
    /// <summary>
    /// The menu manager. Handles all menu logic.
    /// 
    /// Dependencies: MainLoop.cs, sprMenu textures.
    /// </summary>
    public class MngrTitle
    {
        //Refers to the game instance.
        private MainLoop game;

        //Relevant assets.
        private static Texture2D TexCopyright;
        public static Texture2D TexMenuBackground { get; private set; }
        public static Texture2D TexBgMenuMain { get; private set; }
        public static Texture2D TexBgMenuLevelEditor { get; private set; }
        public static Texture2D TexBgMenuLevels { get; private set; }
        public static Texture2D TexBttnMain { get; private set; }
        public static Texture2D TexBttnEdit { get; private set; }
        public static Texture2D TexBttnCmpgn { get; private set; }
        public static Texture2D TexMenuOptions { get; private set; }
        public static Texture2D TexMenuInfo1 { get; private set; }
        public static Texture2D TexMenuInfo2 { get; private set; }
        public static Texture2D TexMenuInfo3 { get; private set; }

        private Sprite sprCopyright, sprMenuInfo;
        private Sprite sprMenuBackground, sprMenuEditorBgDecor, sprMenuMainBgDecor, sprMenuLevelsBgDecor;

        //The menu buttons.
        private TitleItemMain bttnCampaign, bttnLevelEditor, bttnHowToPlay,
            bttnMusicVolume, bttnSfxVolume, bttnToggleFullscreen,
            bttnQuit, bttnBack;

        //The level editor buttons.
        private TitleItemEdit bttnEdit, bttnTest, bttnSave, bttnLoad, bttnClear;

        //The campaign buttons.
        private TitleItemCmpgn bttnCmpgnEasy, bttnCmpgnNormal, bttnCmpgnHard,
            bttnCmpgnDoom;

        //The current page of the how to play screen; used when active.
        private int _infoPage;

        /// <summary>
        /// Refreshes whether test, save, and clear are enabled or not based on level validity.
        /// </summary>
        public void RefreshButtonState(bool isValid)
        {
            // Disables buttons when returning to the editor, depending on the level state.
            bool isValidWithActors = isValid && !(game.mngrEditor.items.Count == 0 ||
                !game.mngrEditor.items.Any(o => o.BlockType == Type.Actor));

            bttnTest.isDisabled = !isValidWithActors;
            bttnSave.isDisabled = !isValid;
            bttnClear.isDisabled = !isValid;
        }

        /// <summary>
        /// Sets the game instance.
        /// </summary>
        /// <param name="game">The game instance to use.</param>
        public MngrTitle(MainLoop game)
        {
            this.game = game;
            _infoPage = 0; //Sets the current information page.

            game.Window.ClientSizeChanged += Window_ClientSizeChanged;
            game.OnGameStateChange += OnGameStateChanged;
        }

        private void OnGameStateChanged()
        {
            var screenSize = game.GetScreenSize();

            bttnBack.BttnSprite.rectDest.X = game.GmState == GameState.stateCampaignModes
                ? screenSize.X / 2 - bttnBack.BttnSprite.rectDest.Width/2 : game.GmState == GameState.stateMenuEditor
                ? screenSize.X / 2 - bttnBack.BttnSprite.rectDest.Width/2
                : screenSize.X / 2 - bttnBack.BttnSprite.rectDest.Width/2;

            bttnBack.BttnSprite.rectDest.Y = screenSize.Y / 2 - 262;
        }

        /// <summary>
        /// Adjusts all positions to deal with resizing.
        /// </summary>
        private void Window_ClientSizeChanged(object sender, EventArgs e)
        {
            var screenOffset = game.FullscreenHandler.GetCurrentOffset();
            var screenSize = game.GetScreenSize();

            // Main menu
            sprMenuMainBgDecor.rectDest.X = screenSize.X / 2 - sprMenuMainBgDecor.texture.Width / 2;
            sprMenuMainBgDecor.rectDest.Y = screenSize.Y / 2 - sprMenuMainBgDecor.texture.Height / 2;

            bttnCampaign.BttnSprite.rectDest.Position = new(screenSize.X/2 - bttnCampaign.BttnSprite.rectDest.Width/2, (screenSize.Y / 2) - 160); // 44px offsets
            bttnLevelEditor.BttnSprite.rectDest.Position = new(screenSize.X/2 - bttnLevelEditor.BttnSprite.rectDest.Width/2, (screenSize.Y / 2) - 116);
            bttnHowToPlay.BttnSprite.rectDest.Position = new(screenSize.X/2 - bttnHowToPlay.BttnSprite.rectDest.Width/2, (screenSize.Y / 2) - 72);
            bttnMusicVolume.BttnSprite.rectDest.Position = new(screenSize.X/2 - bttnMusicVolume.BttnSprite.rectDest.Width/2, (screenSize.Y / 2) + 32); // 60px gap + 44px
            bttnSfxVolume.BttnSprite.rectDest.Position = new(screenSize.X/2 - bttnSfxVolume.BttnSprite.rectDest.Width/2, (screenSize.Y / 2) + 76);
            bttnToggleFullscreen.BttnSprite.rectDest.Position = new(screenSize.X/2 - bttnToggleFullscreen.BttnSprite.rectDest.Width/2, (screenSize.Y / 2) + 120);
            bttnQuit.BttnSprite.rectDest.Position = new(screenSize.X/2 - bttnQuit.BttnSprite.rectDest.Width/2, (screenSize.Y / 2) + 164);

            // Level Editor
            sprMenuEditorBgDecor.rectDest.X = screenSize.X / 2 - sprMenuEditorBgDecor.texture.Width / 2;
            sprMenuEditorBgDecor.rectDest.Y = screenSize.Y / 2 - sprMenuEditorBgDecor.texture.Height / 2;

            bttnEdit.BttnSprite.rectDest.Position = new(screenSize.X / 2 - bttnEdit.BttnSprite.rectDest.Width/2, (screenSize.Y / 2) - 160); // 44px offsets
            bttnTest.BttnSprite.rectDest.Position = new(screenSize.X / 2 - bttnTest.BttnSprite.rectDest.Width/2, (screenSize.Y / 2) - 116);
            bttnSave.BttnSprite.rectDest.Position = new(screenSize.X / 2 - bttnSave.BttnSprite.rectDest.Width/2, (screenSize.Y / 2) - 72);
            bttnLoad.BttnSprite.rectDest.Position = new(screenSize.X / 2 - bttnLoad.BttnSprite.rectDest.Width/2, (screenSize.Y / 2) - 28);
            bttnClear.BttnSprite.rectDest.Position = new(screenSize.X / 2 - bttnClear.BttnSprite.rectDest.Width/2, (screenSize.Y / 2) + 16);

            // Campaign
            sprMenuLevelsBgDecor.rectDest.X = screenSize.X / 2 - sprMenuLevelsBgDecor.texture.Width / 2 - 250;
            sprMenuLevelsBgDecor.rectDest.Y = screenSize.Y / 2 - sprMenuLevelsBgDecor.texture.Height / 2;

            bttnCmpgnEasy.BttnSprite.rectDest.Position = new(screenSize.X / 2 - bttnCmpgnEasy.BttnSprite.rectDest.Width/2, (screenSize.Y / 2) - 210); // 104px offsets
            bttnCmpgnNormal.BttnSprite.rectDest.Position = new(screenSize.X / 2 - bttnCmpgnNormal.BttnSprite.rectDest.Width/2, (screenSize.Y / 2) - 106);
            bttnCmpgnHard.BttnSprite.rectDest.Position = new(screenSize.X / 2 - bttnCmpgnHard.BttnSprite.rectDest.Width/2, (screenSize.Y / 2) - 2);
            bttnCmpgnDoom.BttnSprite.rectDest.Position = new(screenSize.X / 2 - bttnCmpgnDoom.BttnSprite.rectDest.Width/2, (screenSize.Y / 2) + 102);

            // Shared
            sprMenuBackground.rectDest.Position = new(screenSize.X / 2 - sprMenuBackground.texture.Width/2, screenSize.Y / 2 - sprMenuBackground.texture.Height/2);
            sprCopyright.rectDest.Position = new(screenSize.X / 2 - (sprCopyright.rectDest.Width / 2), screenSize.Y / 2 + 240);
            sprMenuInfo.rectDest.Position = new(screenSize.X / 2 - (sprMenuInfo.rectDest.Width / 2), screenSize.Y / 2 - 200);

            OnGameStateChanged();
        }

        /// <summary>
        /// Loads relevant graphics into memory.
        /// 
        /// Dependencies: MainLoop.cs, sprMenu textures, screen size.
        /// </summary>
        public void LoadContent()
        {
            //Loads the button textures.
            TexBttnMain = TitleItemMain.LoadContent(game.Content);
            TexBttnEdit = TitleItemEdit.LoadContent(game.Content);
            TexBttnCmpgn = TitleItemCmpgn.LoadContent(game.Content);

            //Sets up relevant textures.
            TexCopyright = game.Content.Load<Texture2D>("Content/Sprites/Gui/sprCopyright");
            TexMenuInfo1 = game.Content.Load<Texture2D>("Content/Sprites/Gui/sprMenuInfo1");
            TexMenuInfo2 = game.Content.Load<Texture2D>("Content/Sprites/Gui/sprMenuInfo2");
            TexMenuInfo3 = game.Content.Load<Texture2D>("Content/Sprites/Gui/sprMenuInfo3");
            TexMenuBackground = game.Content.Load<Texture2D>("Content/Sprites/Gui/sprMenuBackground");
            TexBgMenuLevelEditor = game.Content.Load<Texture2D>("Content/Sprites/Gui/sprMenuEditorBackground");
            TexBgMenuMain = game.Content.Load<Texture2D>("Content/Sprites/Gui/sprMenuMainBackground");
            TexBgMenuLevels = game.Content.Load<Texture2D>("Content/Sprites/Gui/sprMenuLevelsBackground");

            //Creates buttons after the textures have loaded.
            bttnCampaign = new TitleItemMain(game, TexBttnMain, 0);
            bttnLevelEditor = new TitleItemMain(game, TexBttnMain, 1);
            bttnHowToPlay = new TitleItemMain(game, TexBttnMain, 2);
            bttnMusicVolume = new TitleItemMain(game, TexBttnMain, 8);
            bttnSfxVolume = new TitleItemMain(game, TexBttnMain, 9);
            bttnToggleFullscreen = new TitleItemMain(game, TexBttnMain, 10);
            bttnQuit = new TitleItemMain(game, TexBttnMain, 11);
            bttnBack = new TitleItemMain(game, TexBttnMain, 3);

            bttnEdit = new TitleItemEdit(game, TexBttnEdit, 0, false);
            bttnTest = new TitleItemEdit(game, TexBttnEdit, 1, true);
            bttnSave = new TitleItemEdit(game, TexBttnEdit, 2, true);
            bttnLoad = new TitleItemEdit(game, TexBttnEdit, 3, false);
            bttnClear = new TitleItemEdit(game, TexBttnEdit, 4, true);

            bttnCmpgnEasy = new TitleItemCmpgn(game, TexBttnCmpgn, 360, 32, 0);
            bttnCmpgnNormal = new TitleItemCmpgn(game, TexBttnCmpgn, 360, 132, 1);
            bttnCmpgnHard = new TitleItemCmpgn(game, TexBttnCmpgn, 360, 232, 2);
            bttnCmpgnDoom = new TitleItemCmpgn(game, TexBttnCmpgn, 360, 332, 3);

            sprCopyright = new Sprite(true, TexCopyright);
            sprMenuBackground = new Sprite(true, TexMenuBackground);
            sprMenuEditorBgDecor = new Sprite(true, TexBgMenuLevelEditor) { alpha = 0.2f };
            sprMenuMainBgDecor = new Sprite(true, TexBgMenuMain);
            sprMenuLevelsBgDecor = new Sprite(true, TexBgMenuLevels) { alpha = 0.5f };
            sprMenuInfo = new Sprite(true, TexMenuInfo1);

            Window_ClientSizeChanged(null, null);
        }

        /// <summary>
        /// Updates button logic and handles click actions.
        /// </summary>
        public void Update()
        {
            //Executes button logic relevant to the active state.
            switch (game.GmState)
            {
                //If the main screen is active.
                case GameState.stateMenu:
                    //Updates the titlescreen buttons.
                    bttnCampaign.Update();
                    bttnLevelEditor.Update();
                    bttnHowToPlay.Update();
                    bttnMusicVolume.Update();
                    bttnSfxVolume.Update();
                    bttnToggleFullscreen.Update();
                    bttnQuit.Update();

                    if (bttnCampaign.isClicked)
                    {
                        bttnCampaign.isClicked = false;

                        game.GmState = GameState.stateCampaignModes;
                        game.SetScreenCaption("Endurance (Menu)");
                    }
                    else if (bttnLevelEditor.isClicked)
                    {
                        bttnLevelEditor.isClicked = false;
                        game.GmState = GameState.stateMenuEditor;
                        game.SetScreenCaption("Endurance (Editor)");
                    }
                    else if (bttnHowToPlay.isClicked)
                    {
                        bttnHowToPlay.isClicked = false;
                        game.GmState = GameState.stateHowtoPlay;
                        game.SetScreenCaption("Endurance (Menu)");
                    }
                    else if (bttnMusicVolume.isClicked)
                    {
                        bttnMusicVolume.isClicked = false;

                        if (game.Prefs.VolumeMusic == 1) { game.Prefs.VolumeMusic = 0; }
                        else if (game.Prefs.VolumeMusic >= 0.5f) { game.Prefs.VolumeMusic = 1; }
                        else if (game.Prefs.VolumeMusic >= 0.3f) { game.Prefs.VolumeMusic = 0.5f; }
                        else if (game.Prefs.VolumeMusic >= 0.1f) { game.Prefs.VolumeMusic = 0.3f; }
                        else { game.Prefs.VolumeMusic = 0.1f; }

                        bool isSoundMuted = game.Prefs.VolumeMusic == 0 && game.Prefs.VolumeSfx == 0;
                        game.playlist.Song.Volume = game.Prefs.VolumeMusic;
                        SfxPlaylist.musicVolume = game.Prefs.VolumeMusic;
                        MediaPlayer.IsMuted = isSoundMuted;
                        FileUtils.SavePreferences(game.Prefs);
                    }
                    else if (bttnSfxVolume.isClicked)
                    {
                        bttnSfxVolume.isClicked = false;

                        if (game.Prefs.VolumeSfx == 1) { game.Prefs.VolumeSfx = 0; }
                        else if (game.Prefs.VolumeSfx >= 0.5f) { game.Prefs.VolumeSfx = 1; }
                        else if (game.Prefs.VolumeSfx >= 0.3f) { game.Prefs.VolumeSfx = 0.5f; }
                        else if (game.Prefs.VolumeSfx >= 0.1f) { game.Prefs.VolumeSfx = 0.3f; }
                        else { game.Prefs.VolumeSfx = 0.1f; }

                        bool isSoundMuted = !(game.Prefs.VolumeMusic == 0 && game.Prefs.VolumeSfx == 0);
                        SfxPlaylist.sfxVolume = game.Prefs.VolumeSfx;
                        MediaPlayer.IsMuted = isSoundMuted;
                        FileUtils.SavePreferences(game.Prefs);
                    }
                    else if (bttnToggleFullscreen.isClicked)
                    {
                        bttnToggleFullscreen.isClicked = false;
                        game.FullscreenHandler.ToggleFullscreen();
                        game.Prefs.Fullscreen = game.FullscreenHandler.IsFullscreen;
                        FileUtils.SavePreferences(game.Prefs);
                    }
                    else if (bttnQuit.isClicked)
                    {
                        bttnQuit.isClicked = false;
                        game.Exit();
                    }
                    break;
                //If the how to play screen is active.
                case GameState.stateHowtoPlay:
                    bttnBack.Update();

                    //Goes back one page.
                    if (game.KbState.IsKeyDown(Keys.Left) &&
                        game.KbStateOld.IsKeyUp(Keys.Left))
                    {
                        if (_infoPage > 0)
                        {
                            _infoPage--;
                        }
                    }

                    //Goes forward one page.
                    if (game.KbState.IsKeyDown(Keys.Right) &&
                        game.KbStateOld.IsKeyUp(Keys.Right))
                    {
                        if (_infoPage < 2) //max page here.
                        {
                            _infoPage++;
                        }
                    }

                    //Determines the texture for the info sprite.
                    switch (_infoPage)
                    {
                        case 0:
                            if (sprMenuInfo.texture != TexMenuInfo1)
                            {
                                sprMenuInfo.SetTexture(true, TexMenuInfo1);
                            }
                            break;
                        case 1:
                            if (sprMenuInfo.texture != TexMenuInfo2)
                            {
                                sprMenuInfo.SetTexture(true, TexMenuInfo2);
                            }
                            break;
                        case 2:
                            if (sprMenuInfo.texture != TexMenuInfo3)
                            {
                                sprMenuInfo.SetTexture(true, TexMenuInfo3);
                            }
                            break;
                    }

                    //If back is pressed.
                    if (bttnBack.isClicked)
                    {
                        bttnBack.isClicked = false;
                        game.GmState = GameState.stateMenu;
                        game.SetScreenCaption("Endurance (Menu)");
                    }
                    break;
                //If the edit screen is active.
                case GameState.stateMenuEditor:
                    //Updates the editing buttons.
                    bttnBack.Update();
                    bttnEdit.Update();
                    bttnTest.Update();
                    bttnSave.Update();
                    bttnLoad.Update();
                    bttnClear.Update();

                    if (bttnEdit.isClicked)
                    {
                        bttnEdit.isClicked = false;
                        game.GmState = GameState.stateEditor;
                        game.SetScreenCaption("Endurance (Editor)");
                    }
                    else if (bttnTest.isClicked)
                    {
                        bttnTest.isClicked = false;

                        //If there is at least one actor block.
                        if (game.mngrEditor.items.Count > 0 &&
                        game.mngrEditor.items.Any(o =>
                        o.BlockType == Type.Actor))
                        {
                            //Loads the level.
                            game.mngrEditor.LoadTest();

                            //Loads level settings.
                            game.mngrLvl.countdownStart =
                                game.mngrEditor.opGameDelay;
                            game.mngrLvl.opLvlLink =
                                game.mngrEditor.opLvlLink;
                            game.mngrLvl.OpMaxSteps =
                                game.mngrEditor.opMaxSteps;
                            game.mngrLvl.OpReqGoals =
                                game.mngrEditor.opMinGoals;
                            game.mngrLvl.opSyncActors =
                                game.mngrEditor.opSyncActors;
                            game.mngrLvl.opSyncDeath =
                                game.mngrEditor.opSyncDeath;

                            game.GmState = GameState.stateGameplayEditor;
                            game.SetScreenCaption("Endurance (Editor)");
                        }
                    }
                    else if (bttnSave.isClicked)
                    {
                        bttnSave.isClicked = false;
                        game.mngrEditor.LevelSave();
                    }
                    else if (bttnLoad.isClicked)
                    {
                        bttnLoad.isClicked = false;
                        bool didLevelLoad = game.mngrEditor.LoadEdit();
                        if (didLevelLoad) { game.mngrEditor.activeItem = null; }
                        RefreshButtonState(didLevelLoad || game.mngrEditor.items.Count > 0);
                    }
                    else if (bttnClear.isClicked)
                    {
                        bttnClear.isClicked = false;
                        RefreshButtonState(false);
                        game.mngrEditor.activeItem = null;

                        if (game.mngrEditor.items.Count > 0)
                        {
                            //Clears the block list.
                            game.mngrEditor.items = new List<ImgBlock>();

                            //Resets camera position.
                            game.mngrEditor.camX = 0;
                            game.mngrEditor.camY = 0;
                            game.mngrEditor.camLayer = 0;
                        }
                    }
                    else if (bttnBack.isClicked)
                    {
                        bttnBack.isClicked = false;
                        game.GmState = GameState.stateMenu;
                        game.SetScreenCaption("Endurance (Menu)");
                    }
                    break;
                //If the campaign mode screen is active.
                case GameState.stateCampaignModes:
                    //Updates the editing buttons.
                    bttnBack.Update();

                    bttnCmpgnEasy.Update();
                    bttnCmpgnNormal.Update();
                    bttnCmpgnHard.Update();
                    bttnCmpgnDoom.Update();

                    if (bttnCmpgnEasy.isClicked)
                    {
                        bttnCmpgnEasy.isClicked = false;

                        //After completing the series, click to restart it.
                        if (!game.LvlSeriesEasy.LevelExists())
                        {
                            game.LvlSeriesEasy.LevelNum = 1;
                        }

                        game.mngrCampaign.SetSeries(game.LvlSeriesEasy);
                    }
                    else if (bttnCmpgnNormal.isClicked)
                    {
                        bttnCmpgnNormal.isClicked = false;

                        if (!game.LvlSeriesNormal.LevelExists())
                        {
                            game.LvlSeriesNormal.LevelNum = 1;
                        }

                        game.mngrCampaign.SetSeries(game.LvlSeriesNormal);
                    }
                    else if (bttnCmpgnHard.isClicked)
                    {
                        bttnCmpgnHard.isClicked = false;

                        if (!game.LvlSeriesHard.LevelExists())
                        {
                            game.LvlSeriesHard.LevelNum = 1;
                        }

                        game.mngrCampaign.SetSeries(game.LvlSeriesHard);
                    }
                    else if (bttnCmpgnDoom.isClicked)
                    {
                        bttnCmpgnDoom.isClicked = false;

                        if (!game.LvlSeriesDoom.LevelExists())
                        {
                            game.LvlSeriesDoom.LevelNum = 1;
                        }

                        game.mngrCampaign.SetSeries(game.LvlSeriesDoom);
                    }
                    else if (bttnBack.isClicked)
                    {
                        bttnBack.isClicked = false;
                        game.GmState = GameState.stateMenu;
                        game.SetScreenCaption("Endurance (Menu)");
                    }
                    break;
            }
        }

        public void Draw()
        {
            switch (game.GmState)
            {
                //If the main screen is active.
                case GameState.stateMenu:
                    sprMenuMainBgDecor.Draw(game.GameSpriteBatch);
                    bttnCampaign.Draw();
                    bttnHowToPlay.Draw();
                    bttnLevelEditor.Draw();
                    bttnMusicVolume.Draw();
                    bttnSfxVolume.Draw();
                    bttnToggleFullscreen.Draw();
                    bttnQuit.Draw();
                    sprCopyright.Draw(game.GameSpriteBatch);

                    game.GameSpriteBatch.DrawString(game.fntBold,
                        $"{(int)(game.Prefs.VolumeMusic * 100)}%",
                        new Vector2(
                            bttnMusicVolume.BttnSprite.rectDest.X +
                            bttnMusicVolume.BttnSprite.rectDest.Width + 10,
                            bttnMusicVolume.BttnSprite.rectDest.Y + 4),
                        Color.Black);

                    game.GameSpriteBatch.DrawString(game.fntBold,
                        $"{(int)(game.Prefs.VolumeSfx * 100)}%",
                        new Vector2(
                            bttnSfxVolume.BttnSprite.rectDest.X +
                            bttnSfxVolume.BttnSprite.rectDest.Width + 10,
                            bttnSfxVolume.BttnSprite.rectDest.Y + 4),
                        Color.Black);

                    game.GameSpriteBatch.DrawString(game.fntBold,
                        game.Prefs.Fullscreen ? "Fullscreen" : "Windowed",
                        new Vector2(
                            bttnToggleFullscreen.BttnSprite.rectDest.X +
                            bttnToggleFullscreen.BttnSprite.rectDest.Width + 10,
                            bttnToggleFullscreen.BttnSprite.rectDest.Y + 4),
                        Color.Black);

                    break;
                //If the how to play screen is active.
                case GameState.stateHowtoPlay:
                    sprMenuBackground.alpha = 0.3f;
                    sprMenuBackground.Draw(game.GameSpriteBatch);
                    sprMenuBackground.alpha = 1;
                    bttnBack.Draw();
                    sprMenuInfo.Draw(game.GameSpriteBatch);
                    sprCopyright.Draw(game.GameSpriteBatch);
                    break;
                //If the edit screen is active.
                case GameState.stateMenuEditor:
                    sprMenuBackground.Draw(game.GameSpriteBatch);
                    sprMenuEditorBgDecor.Draw(game.GameSpriteBatch);
                    bttnBack.Draw();
                    bttnEdit.Draw();
                    bttnTest.Draw();
                    bttnSave.Draw();
                    bttnLoad.Draw();
                    bttnClear.Draw();
                    sprCopyright.Draw(game.GameSpriteBatch);
                    break;
                //If the campaign mode screen is active.
                case GameState.stateCampaignModes:
                    sprMenuBackground.Draw(game.GameSpriteBatch);
                    sprMenuLevelsBgDecor.Draw(game.GameSpriteBatch);
                    bttnBack.Draw();
                    bttnCmpgnEasy.Draw();
                    bttnCmpgnNormal.Draw();
                    bttnCmpgnHard.Draw();
                    bttnCmpgnDoom.Draw();

                    //Draws the current level numbers.
                    if (game.LvlSeriesEasy.LevelExists())
                    {
                        game.GameSpriteBatch.DrawString(game.fntBold,
                            "On level " + game.LvlSeriesEasy.LevelNum,
                            new Vector2(
                                bttnCmpgnEasy.BttnSprite.rectDest.X +
                                bttnCmpgnEasy.BttnSprite.rectDest.Width + 4,
                                bttnCmpgnEasy.BttnSprite.rectDest.Y +
                                (bttnCmpgnEasy.BttnSprite.rectDest.Height / 2)),
                            Color.Black);
                    }
                    else
                    {
                        game.GameSpriteBatch.DrawString(game.fntBold,
                            "Completed!",
                            new Vector2(
                                bttnCmpgnEasy.BttnSprite.rectDest.X +
                                bttnCmpgnEasy.BttnSprite.rectDest.Width + 4,
                                bttnCmpgnEasy.BttnSprite.rectDest.Y +
                                (bttnCmpgnEasy.BttnSprite.rectDest.Height / 2)),
                            Color.Blue);
                    }

                    if (game.LvlSeriesNormal.LevelExists())
                    {
                        game.GameSpriteBatch.DrawString(game.fntBold,
                            "On level " + game.LvlSeriesNormal.LevelNum,
                            new Vector2(
                                bttnCmpgnNormal.BttnSprite.rectDest.X +
                                bttnCmpgnNormal.BttnSprite.rectDest.Width + 4,
                                bttnCmpgnNormal.BttnSprite.rectDest.Y +
                                (bttnCmpgnNormal.BttnSprite.rectDest.Height / 2)),
                            Color.Black);
                    }
                    else
                    {
                        game.GameSpriteBatch.DrawString(game.fntBold,
                            "Completed!",
                            new Vector2(
                                bttnCmpgnNormal.BttnSprite.rectDest.X +
                                bttnCmpgnNormal.BttnSprite.rectDest.Width + 4,
                                bttnCmpgnNormal.BttnSprite.rectDest.Y +
                                (bttnCmpgnNormal.BttnSprite.rectDest.Height / 2)),
                            Color.Blue);
                    }

                    if (game.LvlSeriesHard.LevelExists())
                    {
                        game.GameSpriteBatch.DrawString(game.fntBold,
                            "On level " + game.LvlSeriesHard.LevelNum,
                            new Vector2(
                                bttnCmpgnHard.BttnSprite.rectDest.X +
                                bttnCmpgnHard.BttnSprite.rectDest.Width + 4,
                                bttnCmpgnHard.BttnSprite.rectDest.Y +
                                (bttnCmpgnHard.BttnSprite.rectDest.Height / 2)),
                            Color.Black);
                    }
                    else
                    {
                        game.GameSpriteBatch.DrawString(game.fntBold,
                            "Completed!",
                            new Vector2(
                                bttnCmpgnHard.BttnSprite.rectDest.X +
                                bttnCmpgnHard.BttnSprite.rectDest.Width + 4,
                                bttnCmpgnHard.BttnSprite.rectDest.Y +
                                (bttnCmpgnHard.BttnSprite.rectDest.Height / 2)),
                            Color.Blue);
                    }

                    if (game.LvlSeriesDoom.LevelExists())
                    {
                        game.GameSpriteBatch.DrawString(game.fntBold,
                            "On level " + game.LvlSeriesDoom.LevelNum,
                            new Vector2(
                                bttnCmpgnDoom.BttnSprite.rectDest.X +
                                bttnCmpgnDoom.BttnSprite.rectDest.Width + 4,
                                bttnCmpgnDoom.BttnSprite.rectDest.Y +
                                (bttnCmpgnDoom.BttnSprite.rectDest.Height / 2)),
                            Color.Black);
                    }
                    else
                    {
                        game.GameSpriteBatch.DrawString(game.fntBold,
                            "Completed!",
                            new Vector2(
                                bttnCmpgnDoom.BttnSprite.rectDest.X +
                                bttnCmpgnDoom.BttnSprite.rectDest.Width + 4,
                                bttnCmpgnDoom.BttnSprite.rectDest.Y +
                                (bttnCmpgnDoom.BttnSprite.rectDest.Height / 2)),
                            Color.Blue);
                    }

                    sprCopyright.Draw(game.GameSpriteBatch);
                    break;
            }
        }
    }
}