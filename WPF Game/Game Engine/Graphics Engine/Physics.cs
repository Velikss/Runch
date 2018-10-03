﻿using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;

namespace GameEngine
{
    public class PhysicalObject
    {
        //rectangle inside object which which contains the collision points
        public Rectangle collision;
        //oriental info
        public float X, Y;
        public int Height, Width;
        //bool states if object is in contact with the ground
        public bool Landed;
        //contains the sprite for the object
        public Image Sprite;
        //bool if the object collides with the parametered object
        public bool Collide(Rectangle _co2)
        {
            return collision.IntersectsWith(_co2);
        }
    }
    
    public static class Gravity
    {
        //holds the enities to engage gravity on
        private static readonly List<PhysicalObject> entities = new List<PhysicalObject>();
        //holds possible collision objects reffered by Tiles from GameMaker.cs
        private static List<Tile> objects;
        //void enables gravity on parametered object
        public static void EnableGravityOnObject(PhysicalObject po)
        {
            lock (entities)
            {
                entities.Add(po);
            }
        }
        //disables gravity for parametered object
        public static void DisableGravityOnObject(PhysicalObject po)
        {
            lock (entities)
            {
                entities.Remove(po);
            }
        }
        //starts the gravity given reffered objects to collide with
        public static void EnableGravity(ref List<Tile> objs)
        {
            objects = objs;
            //bool is equaled to the status of being on the ground
            bool land;
            new Thread((ThreadStart) delegate
            {
                for (;;)
                    try
                    {
                        //checks for each entity if it has landed, because of man-made input standard collision doesn't suffice therfore a stands boolean is used
                        foreach (var po in entities)
                        {
                            land = false;
                            if (po is Player)
                            {
                                foreach (var obj in objects.Where(o =>
                                    o.X - o.Width <= po.X && o.X + o.Width >= po.X && o.Ground))
                                    if (((Player) po).Stands(obj))
                                        land = true;
                            }
                            else
                            {
                                foreach (var obj in objects.Where(o => o.X <= po.X && o.X + o.Width >= po.X))
                                    if (po.Collide(obj.collision))
                                        land = true;
                            }
                            //if entity hasn't landed drop player, if landed reset the used jumps variable
                            if (!(po.Landed = land))
                                po.Y += 0.7f;
                            else
                                GameMaker.jumps = 0;
                        }

                        Thread.Sleep(1);
                    }
                    catch
                    {
                        //nothing
                    }
            }).Start();
        }
        //checks if gravity is engaged on the given player
        public static bool HasGravity(Player player) => entities.Contains(player);
    }
}