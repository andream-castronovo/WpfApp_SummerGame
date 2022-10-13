using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Game
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        // Costanti varie
        const double X_SPEED = 400;                 // Pixel al secondo.
        const double Y_SPEED = 300;                 // Pixel al secondo.
        const int MAX_POINTS_ON_SCREEN = 4;
        const int MAX_ENEMIES_ON_SCREEN = 3;
        
        // Varie situazioni di gioco descritte da booleani
        bool GamePaused = false;
        bool bossFight = false;


        // Cose per mostrare fps
        Stopwatch sw = new Stopwatch();
        double frameCounter = 0;
        double previousFrameTime;

        int punteggio = 0;
        
        public enum Direzione { Fermo, Sinistra, Destra };
        Direzione direction = Direzione.Fermo;

        // Oggetti di gioco
        Player player;
        Boss boss; 
        readonly List<Punto> points = new List<Punto>();
        readonly List<Nemico> enemies = new List<Nemico>();

        // Numeri random per velocità e spawn diversi
        Random rnd = new Random();

        private void Window_Loaded(object sender, EventArgs e)
        {
            // Metodo che si avvia ogni volta prima di un frame
            CompositionTarget.Rendering += new EventHandler(Rendering);
            
            player = new Player(X_SPEED, Y_SPEED, rnd);
            
            // Ogni oggetto nuovo che va mostrato va aggiunto come "Child" del Canvas
            cnvScreen.Children.Add(player.img);

            player.SetX(cnvScreen.Width / 2 - player.img.Width/2);
            player.SetY(625);

            for (int i = 0; i < MAX_POINTS_ON_SCREEN; i++)
            {
                points.Add(new Punto(Y_SPEED, rnd)); // Aggiungo alla lista i nemici
                cnvScreen.Children.Add(points[i].img);
            }
            for (int i = 0; i < MAX_ENEMIES_ON_SCREEN; i++)
            {
                enemies.Add(new Nemico(Y_SPEED, rnd));
                cnvScreen.Children.Add(enemies[i].img);
            }
            
            boss = new Boss(X_SPEED, Y_SPEED, rnd);

            cnvScreen.Children.Add(boss.img);
            boss.img.Visibility = Visibility.Collapsed;

            boss.SetX(cnvScreen.Width / 2 - boss.img.Width/2);
            boss.SetY(0);

            cnvScreen.Children.Add(boss.bullet.img);
            boss.bullet.img.Visibility = Visibility.Collapsed;
            
            cnvScreen.Children.Add(player.bullet.img);
            player.bullet.img.Visibility = Visibility.Collapsed;

            lblVitaPolpo.Visibility = Visibility.Collapsed;
        }
        void Rendering(object sender, EventArgs e)
        {
            // Calcoli riguardanti il tempo
            double frameTime = sw.Elapsed.TotalSeconds;
            double deltaTime = frameTime - previousFrameTime;

            // Mostro FPS nella Label
            lblFps.Content = CalcoloFPS(frameTime);

            // Se il gioco è in pausa non proseguire
            if (GamePaused)
                return;
            
            player.Spostamento(
                deltaTime,
                cnvScreen,
                direction
                );

            if (!bossFight)
            {
                GestioneSprite(deltaTime, rnd, points);
                GestioneSprite(deltaTime, rnd, enemies);
            }

            if (punteggio == 50)
                bossFight = true;

            if (bossFight)
            {
                for (int i = 0; i < MAX_ENEMIES_ON_SCREEN; i++)
                    enemies[i].img.Visibility = Visibility.Collapsed;
                for (int i = 0; i < MAX_POINTS_ON_SCREEN; i++)
                    points[i].img.Visibility = Visibility.Collapsed;


                lblVitaPolpo.Visibility = Visibility.Visible;
                boss.img.Visibility = Visibility.Visible;
                boss.Spostamento(
                    deltaTime,
                    cnvScreen,
                    rnd
                    );
                
                boss.bullet.img.Visibility = Visibility.Visible;
                player.bullet.img.Visibility = Visibility.Visible;

                if (boss.GetX() + boss.img.Width / 2 > player.GetX() && boss.GetX() + boss.img.Width / 2 < player.GetX() + player.img.Width)
                    boss.shot = true;


                player.Sparo(deltaTime, cnvScreen);

                boss.Sparo(
                        deltaTime,
                        cnvScreen
                        );

                if (boss.bullet.hitbox.IntersectsWith(player.hitbox))
                {
                    player.AvviaMalato(3000);
                    boss.bullet.SetY(cnvScreen.Height+player.img.Height);
                }

                if (player.bullet.hitbox.IntersectsWith(boss.hitbox))
                {
                    boss.life -= 1;
                    player.bullet.SetY(-player.img.Height);
                }


                if (boss.life == 0)
                {
                    boss.img.Visibility = Visibility.Collapsed;
                    boss.SetX(10000);
                    lblFps.Visibility = Visibility.Collapsed;
                    lblPointsOnScreen.Visibility = Visibility.Collapsed;
                    lblVitaPolpo.Visibility = Visibility.Collapsed;
                    
                    
                    VictoryScreen.Visibility = Visibility.Visible;
                }

                lblVitaPolpo.Content = $"Vita polpo: {boss.life}/5";

            }

            lblPointsOnScreen.Content = $"Punti: {punteggio}/50";
        }

        #region GestioneSprite()
        /// <summary>
        /// Gestione degli sprite nemici
        /// </summary>
        /// <param name="deltaTime">deltaTime</param>
        /// <param name="rnd">rnd</param>
        /// <param name="lista">lista che contiene tutti i nemici spawnati</param>
        void GestioneSprite(double deltaTime, Random rnd, List<Nemico> lista)
        {
            for (int i = 0; i < lista.Count; i++)
            {
                lista[i].Caduta(deltaTime);

                if (!lista[i].Falling(cnvScreen))
                {
                    lista[i].Respawn(cnvScreen, rnd, Y_SPEED);
                }

                if (player.hitbox.IntersectsWith(lista[i].hitbox))
                {
                    
                    lista[i].Respawn(cnvScreen, rnd, Y_SPEED); // Fai respawnare lo sprite
                    punteggio += (punteggio >= 5) ? -5 : -punteggio%5; // Per evitare punteggi negativi
                    player.AvviaMalato(2000); // 2 secondi

                }
            }
        }

        /// <summary>
        /// Gestione sprite che assegnano punti
        /// </summary>
        /// <param name="deltaTime">deltaTime</param>
        /// <param name="rnd">rnd</param>
        /// <param name="lista">Lista che contiene tutti i punti spawnati</param>
        void GestioneSprite(double deltaTime, Random rnd, List<Punto> lista)
        {
            for (int i = 0; i < lista.Count; i++)
            {
                lista[i].Caduta(deltaTime);

                if (!lista[i].Falling(cnvScreen))
                {
                    lista[i].Respawn(cnvScreen, rnd, Y_SPEED);
                }

                if (player.hitbox.IntersectsWith(lista[i].hitbox))
                {
                    lista[i].Respawn(cnvScreen, rnd, Y_SPEED);
                    punteggio += punteggio == 49 ? 1 : 2 ;
                }
            }
        }
        #endregion

        string CalcoloFPS(double frameTime)
        {
            if (frameCounter++ == 0)
            {
                sw.Start();
                previousFrameTime = sw.Elapsed.TotalSeconds;
            }

            previousFrameTime = frameTime;

            if (frameCounter % 30 == 0)
                return ((long)(frameCounter / frameTime)).ToString() + " FPS";
            return lblFps.Content + "";
        }


        private void NoMorePausePls(object sender, RoutedEventArgs e)
        {
            PauseScreen.Visibility = Visibility.Collapsed;
            
            GamePaused = false;
            player.pause = false;
            boss.pause = false;

            Panel.SetZIndex(player.img, -1);
            
            player.CambiaSprite(player.SpriteVecchio);
            
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            // Memorizza il comando attivo di direzione dell'utente.
            if (e.Key == Key.Left)
                direction = Direzione.Sinistra;
            else if (e.Key == Key.Right)
                direction = Direzione.Destra;

            // Attiva lo "sparo" dell'utente.
            if (e.Key == Key.Space && bossFight)
                player.shot = true;

            // Verifica se deve chiudere l'applicazione.
            if (e.Key == Key.Escape)
            {
                GamePaused = true;
                player.pause = true;
                boss.pause = true;

                PauseScreen.Visibility = Visibility.Visible;
                player.CambiaSprite("pausa");
                Panel.SetZIndex(player.img, 1001);
            }
            
        }

        // Konami code per spawnare il boss subito
        List<Key> konamiCodeUser = new List<Key>();
        readonly Key[] konamiCode =
        {
            Key.Up,
            Key.Up,
            Key.Down,
            Key.Down,
            Key.Left,
            Key.Right,
            Key.Left,
            Key.Right,
            Key.B,
            Key.A
        };

        bool isKonamiCode = false;
        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left && direction == Direzione.Sinistra ||
               e.Key == Key.Right && direction == Direzione.Destra)
                direction = 0;

            // Konami code per spawnare il boss subito
            konamiCodeUser.Add(e.Key);
            for (int i = 0; i < konamiCodeUser.Count; i++)
            {
                if (konamiCode[i] == konamiCodeUser[i])
                    isKonamiCode = true;
                else
                {
                    isKonamiCode = false;

                    for (int j = 0; j < konamiCodeUser.Count; j++)
                        Console.WriteLine(konamiCodeUser[j]);

                    konamiCodeUser.Clear();
                    return;
                }
            }

            if (isKonamiCode && konamiCodeUser.Count == 10)
            {
                punteggio = 50;
                Console.WriteLine("Konami Code attivato, boss fight");
                konamiCodeUser.Clear();
            }



        }
    }
}