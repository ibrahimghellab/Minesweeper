using System;
using System.Data.Common;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using static System.Reflection.Metadata.BlobBuilder;

namespace MinesweeperWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        private int gridSize; // taille de la grille
        private int nbMine; // nombre de bombes
        private int nbCellOpen = 0; // nombre de cellules qui ont été vérifiées, ouvertes
        private int[,] matrix; // matrice conservant les valeurs de la grille (voir ci-dessous)
        private int[] xbombe;
        private int[] ybombe;
        int temps=0;
        DispatcherTimer timer;
        int nbFlags;







        

        public MainWindow()
        {
            InitializeComponent();
            initialisation();
            init();
            CBXDifficulte.SelectedIndex = 0;

        }
        

        public void init()
        {
            CBXDifficulte.Items.Add("Facile");
            CBXDifficulte.Items.Add("Moyen");
            CBXDifficulte.Items.Add("Difficile");
            
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timerTick;
            timer.Start();
        }

         public void initialisation()
        {

            setGridandNbMineSize();
            nbFlags = nbMine;
            LBLFlags.Content = "Flags : " + nbFlags;


            xbombe =new int[nbMine];
            ybombe=new int[nbMine];
            

            

            matrix = new int[gridSize, gridSize];
            nbCellOpen = 0;
            GRDGame.Children.Clear();
            GRDGame.ColumnDefinitions.Clear();
            GRDGame.RowDefinitions.Clear();
            for (int i = 0; i < gridSize; i++)
            {
                GRDGame.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
                GRDGame.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
            }
            placerBombe();
            remplirGridFather();

           
            temps = 0;
            

        }

        private void timerTick(object sender, EventArgs e)
        {
            temps++;
            LBLTemps.Content =temps;
        }


        private void remplirGridFather()
        {
            
            for (int i = 0; i < gridSize; i++)
            {
                for (int j = 0; j < gridSize; j++)
                {
                    

                    Border b = new Border();
                    b.BorderThickness = new Thickness(0);
                    b.BorderBrush = new SolidColorBrush(Colors.LightBlue);
                    b.SetValue(Grid.RowProperty, j);
                    b.SetValue(Grid.ColumnProperty, i);
                    GRDGame.Children.Add(b);
                    Grid grid = new Grid();
                    grid.Background = new SolidColorBrush(Colors.PeachPuff);
                    b.Child = grid;
                    Button bu = new Button();
                    bu.Click += Bu_Click;
                    bu.BorderThickness = new Thickness(0); 
                    bu.MouseRightButtonUp += Button1_MouseRightButtonDown;
                    Label l = new Label(); 
                    grid.Children.Add(l);
                    grid.Children.Add(bu);
                    if ((j+i)% 2 == 0)
                    {
                        bu.Background = new SolidColorBrush(Color.FromArgb(255, 170, 215, 81));
                        grid.Background = new SolidColorBrush(Color.FromArgb(255,215, 184, 153));

                    }
                    else
                    { 
                        bu.Background = new SolidColorBrush(Color.FromArgb(255, 162, 209, 73));
                        grid.Background = new SolidColorBrush(Color.FromArgb(255,229, 194, 159));
                    }
                    if (matrix[i, j] != 0)
                    {
                        l.Content = matrix[i, j];
                        l.FontWeight = FontWeights.Bold;
                        l.FontSize = 12;
                        l.HorizontalAlignment = HorizontalAlignment.Center;
                        l.VerticalAlignment =   VerticalAlignment.Center;
                        
                    }
                    if (matrix[i, j] == 1)
                    {
                        l.Foreground = new SolidColorBrush(Color.FromArgb(255, 25, 118, 210)); 
                    }
                    if (matrix[i, j] == 2)
                    {
                        l.Foreground = new SolidColorBrush(Color.FromArgb(255, 56, 142, 60));
                    }
                    if (matrix[i, j] == 3)
                    {
                        l.Foreground = new SolidColorBrush(Color.FromArgb(255, 211, 47, 47));
                    }
                    if (matrix[i, j] == 4)
                    {
                        l.Foreground = new SolidColorBrush(Color.FromArgb(255, 130, 43, 161));
                    }
                    if (matrix[i, j] == 5)
                    {
                        l.Foreground = new SolidColorBrush(Color.FromArgb(255, 130, 43, 161));
                    }


                   
                }

            }
           
        }



        void setGridandNbMineSize()
        {
            if(CBXDifficulte.SelectedItem == "Facile")
            {
                gridSize = 8;
                nbMine = 10;
                nbFlags = nbMine;
            }
            if (CBXDifficulte.SelectedItem == "Moyen")
            {
                gridSize = 14;
                nbMine = 40;
                nbFlags = nbMine;
            }
            if (CBXDifficulte.SelectedItem == "Difficile")
            {
                gridSize = 20;
                nbMine = 99;
                nbFlags = nbMine;
            }
            

        }

        



        public bool verifieCellule(int column, int row)
        {
            Button button = GetUIElementFromPosition(GRDGame, column, row) is Border border && border.Child is Grid grid && grid.Children[1] is Button btn ? btn : null;


            if (button.IsVisible) 
            {
                button.Visibility = Visibility.Collapsed;
                nbCellOpen++;
                if (matrix[column, row] == -1)
                {
                    MessageBoxResult result = MessageBox.Show("Tu as perdu, veux tu rejouer ?", "", MessageBoxButton.YesNo);
                    if (result == MessageBoxResult.Yes)
                    {

                        initialisation();
                    }
                    if (result == MessageBoxResult.No)
                    {


                        Close();
                    }
                }
                else
                {
                    if (nbCellOpen == (gridSize * gridSize) - nbMine)
                    {

                        MessageBoxResult result1 = MessageBox.Show("BRAVO! Tu as gagné, veux tu rejouer ?", "", MessageBoxButton.YesNo);
                        if (result1 == MessageBoxResult.Yes)
                        {

                            initialisation();
                        }
                        if (result1 == MessageBoxResult.No)
                        {


                            Close();
                        }
                        return true;
                    }
                    else
                    {
                        if (matrix[column, row] == 0)
                        {
                            // la procédure s’appelle ensuite elle-même sur les cellules voisines
                            for (int i = Math.Max(0, column - 1); i <= Math.Min(gridSize - 1, column + 1); i++)
                            {
                                for (int j = Math.Max(0, row - 1); j <= Math.Min(gridSize - 1, row + 1); j++)
                                {
                                    bool resultat = verifieCellule(i, j);
                                    if (resultat)
                                    { return true; }
                                }
                            }
                        }
                    }




                
                        }
                    }
                    return false;
        }
 


        private void Button1_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            
            
                Button bu = (Button)(((Control)sender));
            if(bu.Content is null)
            {
                bu.Content = "F";
                nbFlags--;
                LBLFlags.Content = "Flags : "+nbFlags;
            }
            else
            {
                bu.Content = null;
                nbFlags++;
                LBLFlags.Content = "Flags : " + nbFlags;
            }

        }

        private void Bu_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            //Ici je pars du principe que dans chaque cellule de la grille, j'ai un Border qui contient une grille qui contient mon bouton. 
            Border b = (Border)VisualTreeHelper.GetParent(VisualTreeHelper.GetParent(button));
            int col = Grid.GetColumn(b);
            int row = Grid.GetRow(b);;

            if(button.Content is null) 
            {
                verifieCellule(col, row);
            }
            

        }

        private void placerBombe()
        {
            Random bx = new Random();
            Random by = new Random();
           

            for (int i = 0; i < nbMine; i++)
            {

                
                    int bxx=bx.Next(gridSize);
                    int byy=by.Next(gridSize);
                    
                        matrix[bxx, byy]=-1;
                    
                    

                
            }
            for(int  i = 0; i < gridSize; i++)
            {
                for(int j = 0; j < gridSize; j++)
                {
                    
                    if (matrix[i,j] == -1)
                    {
                       
                       for (int k = Math.Max(0, i - 1); k <= Math.Min(gridSize-1, i+ 1); k++)
                       {
                            for (int l = Math.Max(0, j - 1); l <= Math.Min(gridSize-1, j + 1); l++)
                            {
                                if (!(matrix[k, l] == -1))
                                {
                                    matrix[k, l] += 1;
                                }
                            }
                       }
                    }
                }
            }
            for( int i = 0;i < gridSize; i++)
            {
                for (int j = 0; j < gridSize; j++)
                {
                    Debug.Write(matrix[j,i]);
                }
               Debug.WriteLine("");
            }
            

        }

        private UIElement GetUIElementFromPosition(Grid g, int col, int row)
        {
            return g.Children.Cast<UIElement>().First(e => Grid.GetRow(e) == row && Grid.GetColumn(e) == col);
        }

        private void CBXDifficulte_Loaded(object sender, RoutedEventArgs e)
        {
            CBXDifficulte.SelectedIndex = 0;
        }

        private void CBXDifficulte_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            initialisation();
        }
    }
}



