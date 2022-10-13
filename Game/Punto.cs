using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Game
{
    internal class Punto
    {
        public Image img;
        public Rect hitbox;
        double Y_SPEED;
        BitmapImage SPRITE1 = new BitmapImage(new Uri("pack://application:,,,/Immagini/point.png"));
        //BitmapImage SPRITE2 = new BitmapImage(new Uri("pack://application:,,,/Immagini/point2.png"));

        BitmapImage[] sprites;
        
        public Punto(double ySpeed, Random rnd)
        {
            sprites = new BitmapImage[] { SPRITE1 };//, SPRITE2 };


            img = new Image
            {
                Source = sprites[rnd.Next(0,sprites.Length)],
                Width = 182,
                Height = 205,
                Visibility = Visibility.Visible
            };

            Y_SPEED = ySpeed + rnd.Next(50, 200);
        }

        public double GetX() { return Canvas.GetLeft(img); }
        public double GetY() { return Canvas.GetTop(img); }
        public void SetX(double x) { Canvas.SetLeft(img, x); }
        public void SetY(double y) { Canvas.SetTop(img, y); }

        public bool Falling(Canvas cnvScreen)
        {
            if (GetY() < cnvScreen.Height)
                return true;
            return false;
        }

        public void Caduta(double deltaTime)
        {
            SetY(GetY() + Y_SPEED * deltaTime);
            hitbox = new Rect(new Point(GetX()+10, GetY() + 60), new Point(GetX() + img.Width - 10, GetY() + img.Height - 10));
        }

        public void Respawn(Canvas cnvScreen, Random rnd, double ySpeed)
        {
            SetX(rnd.Next((int) cnvScreen.Width - (int) img.Width));
            SetY(rnd.Next(-1000,0-(int)img.Height));
            Y_SPEED = ySpeed + rnd.Next(0, 50);
            img.Source = sprites[rnd.Next(0, sprites.Length)];
        }

    }
}
