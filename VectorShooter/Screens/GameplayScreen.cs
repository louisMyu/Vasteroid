#region File Description
//-----------------------------------------------------------------------------
// GameplayScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using FarseerPhysics.Dynamics;
using TimeToLive;
using FarseerPhysics;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Input.Touch;
using System.Collections.Generic;
#endregion

namespace VectorShooter
{
    /// <summary>
    /// This screen implements the actual game logic. It is just a
    /// placeholder to get the idea across: you'll probably want to
    /// put some more interesting gameplay in here!
    /// </summary>
    class GameplayScreen : GameScreen
    {
        #region Fields
        
        ContentManager content;

        private TouchCollection TouchesCollected;
        private bool isLoaded;
        private bool isUpdated;
        private bool isPaused = false;
        private TimeSpan m_CountdownTime;
        Song m_song;
        Random random = new Random();
        private bool songPlaying = false;
        RenderTarget2D backgroundTexture;
        private bool m_BackGroundDrawnOnce = false;
        #endregion

        #region Initialization


        /// <summary>
        /// Constructor.
        /// </summary>
        public GameplayScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(1.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);
            isLoaded = false;
            isUpdated = false;
        }

        /// <summary>
        /// Load graphics content for the game.
        /// </summary>
        public override void LoadContent()
        {
            if (content == null)
                content = ScreenManager.Game.Content;

            // once the load has finished, we use ResetElapsedTime to tell the game's
            // timing mechanism that we have just finished a very long frame, and that
            // it should not try to catch up.
            ScreenManager.Game.ResetElapsedTime();

            isLoaded = true;
        }


        /// <summary>
        /// Unload graphics content used by the game.
        /// </summary>
        public override void UnloadContent()
        {
            //content.Unload();
        }

        #endregion

        #region Update and Draw


        /// <summary>
        /// Updates the state of the game. This method checks the GameScreen.IsActive
        /// property, so the game will stop updating when the pause menu is active,
        /// or if you tab away to a different application.
        /// </summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
            
        }
        private void ResetGame()
        {

        }

        /// <summary>
        /// Lets the game respond to player input. Unlike the Update method,
        /// this will only be called when the gameplay screen is active.
        /// </summary>
        public override void HandleInput(Input input)
        {
            if (input.IsNewKeyPress(Buttons.Back))
            {
                isPaused = true;
                //ScreenManager.Game.Exit();
                //return;
                //this should actually create a menu overlay with the game underneathe
                //this should involve adding a new menu screen for the pause
                ScreenManager.AddScreen(new PauseMenu(), null);
            }
            TouchesCollected = input.TouchState;
        }

        /// <summary>
        /// Draws the gameplay screen.
        /// </summary>
        public override void Draw(GameTime gameTime, Matrix scale)
        {
            SpriteBatch _spriteBatch = ScreenManager.SpriteBatch;

            //make sure the game has loaded and has updated at least one frame
            if (!isLoaded || !isUpdated)
            {
                return;
            }
            
            
            // If the game is transitioning on or off, fade it out to black.
            if (TransitionPosition > 0 && !isPaused)
                ScreenManager.FadeBackBufferToBlack(1f - TransitionAlpha);
        }
        //push the death screen
        private void PushDeathScreen()
        {
            isPaused = true;
            DeathMenu deathMenu = new DeathMenu(UnPauseGame);
            ScreenManager.AddScreen(deathMenu, null);
        }
        private void UnPauseGame()
        {
            isPaused = false;
        }
        private void PauseGame()
        {
            isPaused = true;
        }
        #endregion
    }
}
