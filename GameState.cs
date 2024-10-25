using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace SnakeGame
{
    public class GameState
    {
        // number of rows
        public int Rows {  get; }
        public int Columns { get; }
        // the grid itself
        public GridValue[,] Grid { get; }
        public Direction Dir { get; private set;  }
        public int Score { get; private set; }
        public bool GameOver { get; private set; }

        // Buffer for movement. At the current state, if i press up and immediatly down, the game stops. So we need to store the movements in a buffer
        public readonly LinkedList<Direction> dirChanges = new LinkedList<Direction>();

        // a list of positions currently occupied by the snake. We use linked list because it allows to delete from both ends of the list, head and tail
        private readonly LinkedList<Position> snakePosition = new LinkedList<Position>();
        // it will be used to figure out where the food should spawn
        private readonly Random random = new Random();

        // Constructor
        public GameState(int rows,  int columns)
        {
            Rows = rows;
            Columns = columns;
            // 2d array
            Grid = new GridValue[rows, columns];
            // when the game starts, the snake position will be right
            Dir = Direction.Right;
            AddSnake();
            AddFood();
        }
        // Adding the snake to the grid
        private void AddSnake()
        {
            // middle row
            int r = Rows / 2;
            for (int c = 0; c <= 3; c++)
            {
                Grid[r, c] = GridValue.Snake;
                snakePosition.AddFirst(new Position(r, c));
            }
        }
        // adding food by returning all the empty grid positions
        private IEnumerable<Position> EmptyPositions()
        {
            for (int r = 0; r < Rows; r++)
            {
                for (int c = 0; c < Columns; c++)
                {
                    if (Grid[r, c] == GridValue.Empty)
                    {
                        yield return new Position(r, c);
                    }
                }
            }
        }
        // add foot method
        private void AddFood()
        {
            // list of empty positions
            List<Position> empty = new List<Position>(EmptyPositions());
            // if no empty positions
            if (empty.Count == 0)
            {
                return;
            }
            // taking an empty position at random
            Position pos = empty[random.Next(empty.Count)];
            Grid[pos.Row, pos.Column] = GridValue.Food;
        }
        // position of the snakes head
        public Position HeadPosition()
        {
            return snakePosition.First.Value;
        }
        // position of the snakes tail
        public Position TailPosition()
        {
            return snakePosition.Last.Value;
        }
        // returns all the snakes position
        public IEnumerable<Position> SnakePositions()
        {
            return snakePosition;
        }
        // methods for modifying the snake.
        private void AddHead(Position pos)
        {
            snakePosition.AddFirst(pos);
            // setting the corressponding entry of the grid array
            Grid[pos.Row, pos.Column] = GridValue.Snake;
        }
        // removing the tail
        private void RemoveTail()
        {
            Position tail = snakePosition.Last.Value;
            // making the position behind the tail, empty
            Grid[tail.Row, tail.Column] = GridValue.Empty;
            // removing it from the LinkedList
            snakePosition.RemoveLast();
        }

        private Direction GetLastDirection()
        {
            if (dirChanges.Count == 0)
            {
                return Dir;
            }
            return dirChanges.Last.Value;
        }

        private bool CanChangeDirection(Direction newDir)
        {
            if (dirChanges.Count == 2)
            {
                return false;
            }

            Direction lastDir = GetLastDirection();
            return newDir != lastDir && newDir != lastDir.Opposite();
        }

        // changing the snakes direction
        public void ChangeDirection(Direction dir)
        {
            // if can change direction and added to the buffer
            if (CanChangeDirection(dir))
            {
                dirChanges.AddLast(dir);
            }
        }
        // moving the snake
        // checking if the given position is outside of the grid
        private bool Outside(Position pos)
        {
            return pos.Row < 0 || pos.Row >= Rows || pos.Column < 0 || pos.Column >= Columns;
        }

        private GridValue WillHit(Position newHeadPos)
        {
            // head position outside the grid
            if (Outside(newHeadPos))
            {
                return GridValue.Outside;
            }
            // checking if the head position is the same as the current tail position. PS: if you want to end the game, delete this line
            if (newHeadPos == TailPosition())
            {
                return GridValue.Empty;
            }
            return Grid[newHeadPos.Row, newHeadPos.Column];
        }
        public void Move()
        {

            // we check if there is a direction change in the buffer
            if (dirChanges.Count > 0)
            {
                Dir = dirChanges.First.Value;
                dirChanges.RemoveFirst();
            }

            Position newHeadPos = HeadPosition().Translate(Dir);
            // checking what the head will hit
            GridValue hit = WillHit(newHeadPos);
            if (hit == GridValue.Outside || hit == GridValue.Snake)
            {
                GameOver = true;
            }
            // snake moving to an empty position
            else if (hit == GridValue.Empty)
            {
                // we remove the current tail
                RemoveTail();
                AddHead(newHeadPos);
            }
            // if we hit the food with the head
            else if (hit == GridValue.Food)
            {
                // we don't remove the tail but add a new head
                AddHead(newHeadPos);
                Score++;
                AddFood();
            }
        }
    }
}
