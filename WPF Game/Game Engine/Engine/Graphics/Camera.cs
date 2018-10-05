﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace GameEngine
{
    public class Camera
    {
        //neccessaty's
        private readonly Player player;
        private readonly List<Tile> Tiles;
        private readonly Render render;
        private readonly Menu DeadMenu;

        //local camerathread
        private Thread Cameramover;

        //camera movement pro loop
        public bool Up, Down, Left, Right;

        //camera position
        public float X, Y;

        //set's up camera given reffered player, Tiles
        public Camera(GameMaker gm)
        {
            player = gm.player;
            Tiles = gm.level.Tiles;
            render = gm.game_render;
            DeadMenu = gm.DeadOverlay;
        }

        private void CameraMovement_Thread()
        {
            while (true)
            {
                if (render.isActive())
                {
                    if (player.Y > 650)
                    {
                        render.Deactivate();
                        DeadMenu.Activate();
                    }
                    //because of gravity being in a diffrent thread it checks if it has to move the camera to keep focus in case of falling etc.
                    if (Math.Abs(player.Y - Y * -1) > 425 && player.Y <= 457)
                        Y -= 0.7f;
                    if (Up)
                    {
                        //check if collision is present otherwise move player to given direction
                        if (Tiles.Count(o => o.Y <= player.Y && player.Collide(o)) == 0)
                        {
                            if (Math.Abs(player.Y - Y * -1) < 125)
                                Y += 0.8f;
                            player.Y -= 0.75f;
                        }
                        else
                        {
                            Gravity.EnableGravityOnObject(player);
                            Up = false;
                        }
                    }

                    if (Left)
                    {
                        //check if collision is present otherwise move player to given direction
                        if (player.X > 0 && Tiles.Count(o => o.X <= player.X && player.Collide(o)) == 0)
                        {
                            if (X + player.X < 175  && X < 0)
                                X += 0.45f;
                            player.X -= 0.45f;
                        }
                        else
                        {
                            player.X += 1;
                            Left = false;
                        }
                    }

                    if (Right)
                    {
                        //check if collision is present otherwise move player to given direction
                        if (Tiles.Count(o => o.X >= player.X && player.Collide(o)) == 0)
                        {
                            if (X + player.X > 625)
                                X -= 0.45f;
                            player.X += 0.45f;
                        }
                        else
                        {
                            player.X -= 1;
                            Right = false;
                        }
                    }
                }

                Thread.Sleep(1);
            }
        }

        //start's camera
        public void Start()
        {
            (Cameramover = new Thread(CameraMovement_Thread)).Start();
        }

        public void Dispose()
        {
            Cameramover.Abort();
        }
    }
}