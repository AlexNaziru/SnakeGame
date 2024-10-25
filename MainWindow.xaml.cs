using SnakeGame;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SnakeGame
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        private readonly Dictionary<GridValue, ImageSource> gridValToImage = new()
        {
            { GridValue.Empty, Images.Empty },
            { GridValue.Snake, Images.Body },
            { GridValue.Food, Images.Food }
        };

        // adding the eye picture to our snake
        private readonly Dictionary<Direction, int> dirToRotation = new()
        {
            { Direction.Up, 0 },
            { Direction.Right, 90 },
            { Direction.Down, 180 },
            { Direction.Left, 270}
        };

        /// Two variables for columns and rows 
        private readonly int rows = 20, cols = 20; // here you can change the rows and cols as long as they are equal. Becasue we made the window 400x400
        private readonly Image[,] gridImages;
        private GameState gameState;
        private bool gameRunning;

        public MainWindow()
        {
            InitializeComponent();
            gridImages = SetupGrid();
            gameState = new GameState(rows, cols);
        }

        private async Task RunGame()
        {
            this.Focus(); // Set focus to the window
            Keyboard.Focus(this); // Set keyboard focus
            Draw();
            // Calling the count-down
            await ShowCountDown();
            Overlay.Visibility = Visibility.Hidden;
            await GameLoop();
            // when the game loops ends, it's game over
            await ShowGameOver();
            // now we create a fresh game state for the next game
            gameState = new GameState(rows, cols);
        }

        private async void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Overlay.Visibility == Visibility.Visible)
            {
                // this will prevent window keydown from being called
                e.Handled = true;
            }
            // if the game is not running we set it to true
            if (!gameRunning)
            {
                gameRunning = true;
                await RunGame();
                gameRunning = false;
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            Debug.WriteLine($"KeyDown event fired with key: {e.Key}");
            // Key inputs 
            if (gameState.GameOver)
            {
                return;
            }
           
            switch (e.Key)
            {
                case Key.Left:
                    gameState.ChangeDirection(Direction.Left);
                    break;
                case Key.Right:
                    gameState.ChangeDirection(Direction.Right);
                    break;
                case Key.Up:
                    gameState.ChangeDirection(Direction.Up);
                    break;
                case Key.Down:
                    gameState.ChangeDirection(Direction.Down);
                    break;
            }
        }
        // we need to move it are regular intervals
        private async Task GameLoop()
        {
            Debug.WriteLine("GameLoop started");
            while (!gameState.GameOver)
            {
                // small delay
                await Task.Delay(100); //ms
                gameState.Move();
                Draw();
            }    
        }

        private Image[,] SetupGrid()
        {
            // 2d array
            Image[,] images = new Image[rows, cols];
            GameGrid.Rows = rows;
            GameGrid.Columns = cols;
            GameGrid.Width = GameGrid.Height * (cols/ (double)rows);

            // looping over grid positions
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    Image image = new Image
                    {
                        Source = Images.Empty,
                        // we have a bug where the grid is funky and the snakes body collapses when moving.
                        RenderTransformOrigin = new Point(0.5, 0.5)
                    };
                    images[r, c] = image;
                    GameGrid.Children.Add(image);
                }
            }
            return images;
        }

        private void Draw()
        {
            DrawGrid();
            DrawSnakeHead();
            ScoreText.Text = $"Score {gameState.Score}";
        }

        private void DrawGrid()
        {
            // it loops thjrew every grid position
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0;c < cols; c++)
                {
                    GridValue gridVal = gameState.Grid[r, c];
                    gridImages[r, c].Source = gridValToImage[gridVal];
                    // this ensures that the only rotating image is the snakes head
                    gridImages[r, c].RenderTransform = Transform.Identity;
                }
            }
        }

        private void DrawSnakeHead()
        {
            Position HeadPos = gameState.HeadPosition();
            Image image = gridImages[HeadPos.Row, HeadPos.Column];
            image.Source = Images.Head;

            // eyes looking in the right direction
            int rotation = dirToRotation[gameState.Dir];
            image.RenderTransform = new RotateTransform(rotation);
        }

        private async Task DrawDeadSnakeHead()
        {
            List<Position> positions = new List<Position>(gameState.SnakePositions());
            for (int i = 0; i < positions.Count; i++)
            {
                Position pos = positions[i];
                ImageSource source = (i == 0) ? Images.DeadHead : Images.DeadBody; 
                gridImages[pos.Row, pos.Column].Source = source;
                await Task.Delay(50);
            }
        }

        // Adding a simple count-down so the game doesn't start immediatly after we press any key to start it
        private async Task ShowCountDown()
        {
            for (int i = 3; i >= 1; i--)
            {
                OverlayText.Text = i.ToString();
                await Task.Delay(500);
            }
        }
        // restarting the game
        private async Task ShowGameOver()
        {
            await DrawDeadSnakeHead();
            await Task.Delay(1000);
            Overlay.Visibility = Visibility.Visible;
            OverlayText.Text = "Press any key to start!";
        }
    }   
}