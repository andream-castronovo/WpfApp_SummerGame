using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.SqlServer.Server;

namespace Game
{
    class Bullet
    {

        public Image img;
        public Rect hitbox;
        int direction;

        public double Y_SPEED;
        public bool pause;
        /// <summary>
        /// Classe proiettile
        /// </summary>
        /// <param name="ySpeed">Velocità in cui è sparato</param>
        /// <param name="direction">1: Verso sotto -1: Verso sopra</param>
        public Bullet(double ySpeed, int direction, Random rnd, Image img)
        {

            if (direction != 1 && direction != -1)
                throw new Exception("Direzione non ammessa");

            this.img = img;

            this.direction = direction;

            img.RenderTransformOrigin = new Point(0.5, 0.5);
            ScaleTransform flipTrans = new ScaleTransform();
            flipTrans.ScaleY = direction*-1;
            img.RenderTransform = flipTrans;
            
            img.Visibility = Visibility.Collapsed;

            Y_SPEED = ySpeed+150;

        }
        public double GetX() { return Canvas.GetLeft(img); }
        public double GetY() { return Canvas.GetTop(img); }
        public void SetX(double x) { Canvas.SetLeft(img, x); }
        public void SetY(double y) { Canvas.SetTop(img, y); }

        public void Spostamento(double deltaTime, Canvas cnvScreen)
        {
            SetY(GetY() + Y_SPEED * deltaTime * direction);
        }



    }
}
