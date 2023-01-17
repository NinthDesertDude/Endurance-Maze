﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
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
        public static Texture2D TexBttnMain { get; private set; }
        public static Texture2D TexBttnEdit { get; private set; }
        public static Texture2D TexBttnCmpgn { get; private set; }
        public static Texture2D TexMenuTitle { get; private set; }
        public static Texture2D TexMenuOptions { get; private set; }
        public static Texture2D TexMenuInfo1 { get; private set; }
        public static Texture2D TexMenuInfo2 { get; private set; }
        public static Texture2D TexMenuInfo3 { get; private set; }

        //The title, options section, and how to play.
        Sprite sprCopyright, sprTitle, sprMenuOptions, sprMenuInfo;

        //The menu buttons.
        TitleItemMain bttnCampaign, bttnLevelEditor, bttnHowToPlay, bttnMuteSfx,
            bttnBack;

        //The level editor buttons.
        TitleItemEdit bttnEdit, bttnTest, bttnSave, bttnLoad, bttnClear;

        //The campaign buttons.
        TitleItemCmpgn bttnCmpgnEasy, bttnCmpgnNormal, bttnCmpgnHard,
            bttnCmpgnDoom;

        //The current page of the how to play screen; used when active.
        private int _infoPage;

        /// <summary>
        /// Sets the game instance.
        /// </summary>
        /// <param name="game">The game instance to use.</param>
        public MngrTitle(MainLoop game)
        {
            this.game = game;
            _infoPage = 0; //Sets the current information page.
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
            TexMenuTitle = game.Content.Load<Texture2D>("Content/Sprites/Gui/sprMenuTitle");
            TexMenuOptions = game.Content.Load<Texture2D>("Content/Sprites/Gui/sprMenuOptions");
            TexMenuInfo1 = game.Content.Load<Texture2D>("Content/Sprites/Gui/sprMenuInfo1");
            TexMenuInfo2 = game.Content.Load<Texture2D>("Content/Sprites/Gui/sprMenuInfo2");
            TexMenuInfo3 = game.Content.Load<Texture2D>("Content/Sprites/Gui/sprMenuInfo3");

            //Creates buttons after the textures have loaded.
            bttnCampaign = new TitleItemMain
                (game, TexBttnMain, 334, 96, 0);
            bttnLevelEditor = new TitleItemMain
                (game, TexBttnMain, 334, 142, 1);
            bttnHowToPlay = new TitleItemMain
                (game, TexBttnMain, 334, 188, 2);
            bttnMuteSfx = new TitleItemMain
                (game, TexBttnMain, 334, 280, 4);
            bttnBack = new TitleItemMain
                (game, TexBttnMain, 339, 0, 3);

            bttnEdit = new TitleItemEdit(game, TexBttnEdit, 378, 96, 0);
            bttnTest = new TitleItemEdit(game, TexBttnEdit, 378, 142, 1);
            bttnSave = new TitleItemEdit(game, TexBttnEdit, 378, 188, 2);
            bttnLoad = new TitleItemEdit(game, TexBttnEdit, 378, 234, 3);
            bttnClear = new TitleItemEdit(game, TexBttnEdit, 378, 280, 4);

            bttnCmpgnEasy = new TitleItemCmpgn(game, TexBttnCmpgn, 360, 32, 0);
            bttnCmpgnNormal = new TitleItemCmpgn(game, TexBttnCmpgn, 360, 132, 1);
            bttnCmpgnHard = new TitleItemCmpgn(game, TexBttnCmpgn, 360, 232, 2);
            bttnCmpgnDoom = new TitleItemCmpgn(game, TexBttnCmpgn, 360, 332, 3);

            //Creates the copyright sprite.
            sprCopyright = new Sprite(true, TexCopyright);
            sprCopyright.rectDest.Y = game.GetScreenSize().Y - 16;
            sprCopyright.rectDest.X =
                game.GetScreenSize().X / 2 -
                (sprCopyright.rectDest.Width / 2);            

            //Creates the title sprite.
            sprTitle = new Sprite(true, TexMenuTitle);            
            sprTitle.rectDest.X = 194; //(scr. width - img. width) / 2
            sprTitle.rectDest.Y = 8; //img. width / 2
            
            //Creates the options sprite.
            sprMenuOptions = new Sprite(true, TexMenuOptions);
            sprMenuOptions.rectDest.X = 203; //(scr. width - img. width) / 2
            sprMenuOptions.rectDest.Y = 248;
            
            //Creates the info sprite.
            sprMenuInfo = new Sprite(true, TexMenuInfo1);
            sprMenuInfo.rectDest.Y = 28; //Room for a back button.
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
                    bttnMuteSfx.Update();

                    if (bttnCampaign.isClicked)
                    {
                        bttnCampaign.isClicked = false;

                        game.GmState = GameState.stateCampaignModes;
                        game.SetScreenCaption("Gameplay Modes");
                    }
                    else if (bttnLevelEditor.isClicked)
                    {
                        bttnLevelEditor.isClicked = false;
                        game.GmState = GameState.stateMenuEditor;
                        game.SetScreenCaption("Level editor");
                    }
                    else if (bttnHowToPlay.isClicked)
                    {
                        bttnHowToPlay.isClicked = false;
                        game.GmState = GameState.stateHowtoPlay;
                        game.SetScreenCaption("How to play");
                    }
                    else if (bttnMuteSfx.isClicked)
                    {
                        bttnMuteSfx.isClicked = false;
                        game.isSoundMuted = !game.isSoundMuted;
                        MediaPlayer.IsMuted = game.isSoundMuted;
                        SoundEffect.MasterVolume =
                            Convert.ToInt32(!game.isSoundMuted);
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
                        game.SetScreenCaption("Main menu");
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
                        game.SetScreenCaption("Level Editor");
                    }
                    else if (bttnTest.isClicked)
                    {
                        bttnTest.isClicked = false;

                        //If there is at least one actor block.
                        if (game.mngrEditor.items.Count > 0 &&
                        game.mngrEditor.items.Where(o =>
                        o.BlockType == Type.Actor).Count() > 0)
                        {
                            //Loads the level.
                            game.mngrEditor.LoadTest();

                            //Loads level settings.
                            game.mngrLvl._countdownStart =
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
                            game.SetScreenCaption("Level Editor");
                        }
                    }
                    else if (bttnSave.isClicked)
                    {
                        bttnSave.isClicked = false;

                        if (game.mngrEditor.items.Count > 0 &&
                        game.mngrEditor.items.Where(o =>
                        o.BlockType == Type.Actor).Count() > 0)
                        {
                            game.mngrEditor.LevelSave();
                        }
                    }
                    else if (bttnLoad.isClicked)
                    {
                        bttnLoad.isClicked = false;

                        game.mngrEditor.LoadEdit();
                    }
                    else if (bttnClear.isClicked)
                    {
                        bttnClear.isClicked = false;

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
                        game.SetScreenCaption("Main menu");
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
                        game.SetScreenCaption("Main menu");
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
                    bttnCampaign.Draw();
                    bttnHowToPlay.Draw();
                    bttnLevelEditor.Draw();
                    bttnMuteSfx.Draw();
                    sprMenuOptions.Draw(game.GameSpriteBatch);
                    sprCopyright.Draw(game.GameSpriteBatch);
                    sprTitle.Draw(game.GameSpriteBatch);
                    break;
                //If the how to play screen is active.
                case GameState.stateHowtoPlay:
                    bttnBack.Draw();
                    sprMenuInfo.Draw(game.GameSpriteBatch);
                    sprCopyright.Draw(game.GameSpriteBatch);
                    break;
                //If the edit screen is active.
                case GameState.stateMenuEditor:
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
                    bttnBack.Draw();
                    bttnCmpgnEasy.Draw();
                    bttnCmpgnNormal.Draw();
                    bttnCmpgnHard.Draw();
                    bttnCmpgnDoom.Draw();

                    //Draws the current level numbers.
                    if (game.LvlSeriesEasy.LevelExists())
                    {
                        game.GameSpriteBatch.DrawString(game.fntBold,
                            "On level: " + game.LvlSeriesEasy.LevelNum,
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
                            Color.Green);
                    }

                    if (game.LvlSeriesNormal.LevelExists())
                    {
                        game.GameSpriteBatch.DrawString(game.fntBold,
                            "On level: " + game.LvlSeriesNormal.LevelNum,
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
                            Color.Green);
                    }

                    if (game.LvlSeriesHard.LevelExists())
                    {
                        game.GameSpriteBatch.DrawString(game.fntBold,
                            "On level: " + game.LvlSeriesHard.LevelNum,
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
                            Color.Green);
                    }

                    if (game.LvlSeriesDoom.LevelExists())
                    {
                        game.GameSpriteBatch.DrawString(game.fntBold,
                            "On level: " + game.LvlSeriesDoom.LevelNum,
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
                            Color.Green);
                    }

                    sprCopyright.Draw(game.GameSpriteBatch);
                    break;
            }
        }
    }
}