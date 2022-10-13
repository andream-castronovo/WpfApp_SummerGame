using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Ink;

namespace Game
{
    class Player
    {
        // Campi di classe
        public bool shot;
        public Image img;
        public Rect hitbox;
        public double X_SPEED;
        public bool isSick;
        public bool pause;

        public Bullet bullet;

        readonly BitmapImage SPRITEDEFAULT = new BitmapImage(new Uri("pack://application:,,,/Immagini/player.png"));
        readonly BitmapImage SPRITEPAUSA = new BitmapImage(new Uri("pack://application:,,,/Immagini/playerPause.png"));
        readonly BitmapImage SPRITEMALATO = new BitmapImage(new Uri("pack://application:,,,/Immagini/playerMalato.png"));

        readonly BitmapImage BULLETSPRITE = new BitmapImage(new Uri("pack://application:,,,/Immagini/playerBullet.png"));

        public string SpriteVecchio = "default";

        public Player(double xSpeed, double ySpeed, Random rnd) // Costruttore
        {
            img = new Image
            {
                Source = SPRITEDEFAULT,
                Width = 186,
                Height = 264,
            };
            
            bullet = new Bullet(
                ySpeed, 
                -1, 
                rnd,
                new Image()
                {
                    Source = BULLETSPRITE,
                    Width = 54,
                    Height = 77,
                }
                );

            X_SPEED = xSpeed;
            isSick = false;
        }

        public double GetX() { return Canvas.GetLeft(img); }
        public double GetY() { return Canvas.GetTop(img); }
        public void SetX(double x) { Canvas.SetLeft(img, x); }
        public void SetY(double y) { Canvas.SetTop(img, y); }

        public void Spostamento(double deltaTime, Canvas cnvScreen, MainWindow.Direzione direction)
        {
            // Sinistra
            if (direction == MainWindow.Direzione.Sinistra && (GetX() - X_SPEED * deltaTime) > -img.Width / 2)
            {

                SetX(GetX() - X_SPEED * deltaTime);
                
                #region Girare immagine
                img.RenderTransformOrigin = new Point(0.5, 0.5);
                ScaleTransform flipTrans = new ScaleTransform();
                flipTrans.ScaleX = 1;
                img.RenderTransform = flipTrans;
                #endregion
            }
            // Destra
            else if (direction == MainWindow.Direzione.Destra && (GetX() + X_SPEED * deltaTime + img.Width / 2) < cnvScreen.Width)
            {
                SetX(GetX() + X_SPEED * deltaTime);

                #region Girare immagine
                img.RenderTransformOrigin = new Point(0.5, 0.5);
                ScaleTransform flipTrans = new ScaleTransform();
                flipTrans.ScaleX = -1;
                img.RenderTransform = flipTrans;
                #endregion
            }

            hitbox = new Rect(new Point(GetX() + 31, GetY() + 83), new Point(GetX() + img.Width - 31, GetY() + img.Height));

            if (!shot)
            {
                bullet.img.Visibility = Visibility.Collapsed;
                bullet.SetX(GetX() + img.Width / 2);
                bullet.SetY(GetY());
            }

        }

        /// <summary>
        /// Cambia lo sprite del player:
        /// </summary>
        /// <param name="s">Valori accettati: "malato", "default" e "pausa"</param>
        public void CambiaSprite(string s)
        {
            img.Dispatcher.Invoke(() =>
            {
                switch (s)
                {
                    case "default":
                        img.Source = SPRITEDEFAULT;
                        break;
                    case "pausa":
                        img.Source = SPRITEPAUSA;
                        break;
                    case "malato":
                        img.Source = SPRITEMALATO;
                        break;
                    default:
                        throw new Exception("Sprite non valido, valori accettati: \"default\", \"pausa\" e \"malato\"");
                }
            });
        }
        
        const int punizioneVelocità = 200; 
        readonly System.Timers.Timer t = new System.Timers.Timer();

        public void AvviaMalato(int ms)
        {
            SpriteVecchio = "malato";
            t.Stop(); // Interrompo il timer per ricrearlo nuovamente;
                      // Questo perché nel caso in cui il giocatore
                      // colpisca 2 volte di fila un "nemico" verranno 
                      // assegnati 3 secondi di "penalità" dall'ultimo nemico colpito
            
            t.Interval = ms;
            t.AutoReset = false;

            X_SPEED -= !isSick ? punizioneVelocità : 0; // Penalità di 200 in velocità
            isSick = true;
            CambiaSprite("malato"); // Cambia skin allo Sprite
            
            t.Elapsed += new System.Timers.ElapsedEventHandler((s, e) => {
                if (pause) // Se il gioco è in pausa ovviamente non devi togliere il debuff
                {
                    t.Start();
                    return;
                }
                X_SPEED += isSick ? punizioneVelocità : 0;
                CambiaSprite("default");
                SpriteVecchio = "default";
                isSick = false;
            });
            t.Start();
        }


        public void Sparo(double deltaTime, Canvas cnvScreen) 
        {

            if (shot)
            {
                bullet.img.Visibility = Visibility.Visible;

                if (bullet.GetY() > 0 && bullet.GetY() < cnvScreen.Height)
                    bullet.Spostamento(deltaTime, cnvScreen);
                else
                {
                    bullet.SetX(GetX() + img.Width / 2);
                    bullet.SetY(GetY());
                    shot = false;
                }
                
                bullet.hitbox = new Rect(new Point(bullet.GetX() + 31, bullet.GetY() + 83), new Point(bullet.GetX() + bullet.img.Width - 31, bullet.GetY() + bullet.img.Height));

            }
        }



    }
}
