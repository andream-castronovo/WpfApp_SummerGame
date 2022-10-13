using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Game
{
    class Boss
    {

        readonly BitmapImage DEFAULTSPRITE = new BitmapImage(new Uri("pack://application:,,,/Immagini/boss.png"));
        readonly BitmapImage BULLETSPRITE = new BitmapImage(new Uri("pack://application:,,,/Immagini/bossBullet.png"));

        public bool shot;

        public Image img;
        public Bullet bullet;
        
        public Rect hitbox;

        public double X_SPEED;
        public bool pause;
        public int life; 

        public Boss(double xSpeed, double ySpeed, Random rnd)
        {
            img = new Image()
            {
                Source = DEFAULTSPRITE,
                Width = 332,
                Height = 200.14,
            };

            bullet = new Bullet(
                ySpeed, 
                1, 
                rnd, 
                new Image()
                {
                    Source = BULLETSPRITE,
                    Width = 45,
                    Height = 57,
                });

            X_SPEED = xSpeed;
            shot = false;
            pause = false;
            life = 5;
        }

        public double GetX() { return Canvas.GetLeft(img); }
        public double GetY() { return Canvas.GetTop(img); }
        public void SetX(double x) { Canvas.SetLeft(img, x); }
        public void SetY(double y) { Canvas.SetTop(img, y); }

        enum Direzione { Sinistra, Destra };
        Direzione direction = Direzione.Sinistra;

        public void CambiaDirezione()
        {
            if (direction == Direzione.Sinistra)
                direction = Direzione.Destra;
            else
                direction = Direzione.Sinistra;
        }

        private void Move(double deltaTime)
        {
            if (direction == Direzione.Sinistra)
            {
                SetX(GetX() - deltaTime * X_SPEED);
            }
            else
            {
                SetX(GetX() + deltaTime * X_SPEED);
            }
            
            hitbox = new Rect(new Point(GetX() + 31, GetY() + 83), new Point(GetX() + img.Width - 31, GetY() + img.Height));
        }

        public void Spostamento(double deltaTime, Canvas cnvScreen, Random rnd)
        {
            if (pause)
                return;

            if (GetX() + img.Width/2 > cnvScreen.Width || GetX() + img.Width/2 < 0)
                CambiaDirezione();

            Move(deltaTime);
            
            if (!shot)
            {
                bullet.img.Visibility = Visibility.Collapsed;
                bullet.SetX(GetX() + img.Width / 2);
                bullet.SetY(GetY() + img.Height);

            }
        }

        public void Sparo (double deltaTime, Canvas cnvScreen)
        {
            if (shot)
            {
                bullet.img.Visibility = Visibility.Visible;
                if (bullet.GetY() > 0 && bullet.GetY() < cnvScreen.Height)
                    bullet.Spostamento(deltaTime, cnvScreen);
                else
                {
                    bullet.SetX(GetX() + img.Width / 2);
                    bullet.SetY(GetY() + img.Height);
                    shot = false;
                }

                bullet.hitbox = new Rect(new Point(bullet.GetX() + 31, bullet.GetY() + 83), new Point(bullet.GetX() + bullet.img.Width - 31, bullet.GetY() + bullet.img.Height));

                
            }
        }

    }
}
