﻿using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Color = System.Windows.Media.Color;
using Image = System.Windows.Controls.Image;

namespace GameEngine
{
    public class Screen
    {
        //setup Screen
        public Screen(GameMaker gm)
        {
            //setup Grid
            var grid = new Grid();
            grid.Width = gm.w.Width;
            grid.Height = gm.w.Height;
            //setup Window
            gm.w.Width = gm.Screen_Width;
            gm.w.Height = gm.Screen_Height;
            gm.w.ResizeMode = ResizeMode.NoResize;
            gm.w.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            gm.w.Content = grid;
            CompositionTarget.Rendering += Screen_Rendering;
            gm.w.Closing += delegate
            {
                CompositionTarget.Rendering -= Screen_Rendering;
                framerater.Stop();
                Environment.Exit(0);
            };
            //setup GameData
            GameData = new Label();
            GameData.Foreground = new SolidColorBrush(Colors.White);
            GameData.Background = new SolidColorBrush(new Color {A = 0});
            GameData.Width = 100;
            GameData.Height = 100;
            GameData.FontSize = 24;
            GameData.Visibility = Visibility.Hidden;
            GameData.Margin = new Thickness(0, 0, 700, 508);
            //canvas background
            grid.Children.Add(new Image
            {
                Source = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + "Scene/back.gif")),
                Width = gm.w.Width,
                Height = gm.w.Height
            });
            //setup Canvas & Screen buffer
            canvas = new Image();
            canvas.Width = gm.w.Width;
            canvas.Height = gm.w.Height;
            screen_buffer = new Bitmap(gm.Screen_Width, gm.Screen_Height);
            //set View
            grid.Children.Add(canvas);
            grid.Children.Add(GameData);
        }

        //Creates ImageSource from Bitmap
        private BitmapSource CreateBitmapSource(ref Bitmap bitmap)
        {
            lock (bitmap)
            {
                var hBitmap = bitmap.GetHbitmap();
                try
                {
                    return Imaging.CreateBitmapSourceFromHBitmap(
                        hBitmap,
                        IntPtr.Zero,
                        Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());
                }
                finally
                {
                    DeleteObject(hBitmap);
                }
            }
        }

        //Render event @ framerate
        private void Screen_Rendering(object sender, EventArgs e)
        {
            //every second refresh GameData
            if (GameData.IsVisible)
                if (framerater.Elapsed.TotalSeconds > 1)
                {
                    GameData.Content = refreshrate + "Hz" + Environment.NewLine + FPS + "FPS";
                    FPS = 0;
                    refreshrate = 0;
                    framerater.Restart();
                }
                else
                {
                    refreshrate++;
                }

            //repaint canvas from screen_buffer
            canvas.Source = CreateBitmapSource(ref screen_buffer);
        }

        #region Variables

        //framerater
        public readonly Stopwatch framerater = new Stopwatch();

        //label holds FPS & Refreshrate
        public readonly Label GameData;

        //is actual drawing screen
        private readonly Image canvas;

        //is the drawingbuffer on each screen:refresh
        public Bitmap screen_buffer;

        //
        public int refreshrate;
        public int FPS;

        #region Win32

        //win32-methode removes Handler from memory
        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);

        #endregion

        #endregion
    }
}